#!/usr/bin/env python3
"""Verify the JNI contract between the managed API and the Java App Events facade.

Subcommands:
  jni   Every (method, signature) the C# bridge calls must exist as a public static method of
        FacebookAppEventsInterop in the built wrapper AAR (checked with `javap -s`).
  apk   An APK built with R8 must still define the facade and every method the C# bridge calls,
        plus the Facebook SDK entry points it relies on (parsed from the dex files).

Usage:
  python3 scripts/verify-interop.py jni [--aar <wrapper.aar>]
  python3 scripts/verify-interop.py apk <app.apk> [--feature Login] [--feature Share]
"""

from __future__ import annotations

import argparse
import re
import struct
import subprocess
import sys
import tempfile
import zipfile
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
MANAGED = ROOT / "src/Kapusch.FacebookApisForAndroidComponents/Facebook/FacebookAppEvents.cs"
DEFAULT_AAR = ROOT / "src/Kapusch.FacebookApisForAndroidComponents/Native/Android/build/aar/kfb-facebookinterop-release.aar"
JAVA_CLASS = "com.kapusch.facebook.androidinterop.FacebookAppEventsInterop"
INTEROP_DESCRIPTOR = "Lcom/kapusch/facebook/androidinterop/FacebookAppEventsInterop;"

# Upstream classes the facade calls directly or that bootstrap the SDK.
REQUIRED_CLASSES = {
    "Lcom/facebook/FacebookSdk;",
    "Lcom/facebook/appevents/AppEventsLogger;",
    "Lcom/facebook/internal/FacebookInitProvider;",
}
FEATURE_CLASSES = {
    "Login": {"Lcom/kapusch/facebook/androidinterop/FacebookLoginActivity;", "Lcom/facebook/login/LoginManager;"},
    "Share": {"Lcom/kapusch/facebook/androidinterop/FacebookShareActivity;", "Lcom/facebook/share/widget/ShareDialog;"},
}


def managed_calls() -> set[tuple[str, str]]:
    source = MANAGED.read_text(encoding="utf-8")
    calls = set(re.findall(r'(?:CallVoid|CallString|Method)\(\s*"(\w+)",\s*"([^"]+)"', source))
    calls |= {(name, "()Z") for name in re.findall(r'CallBool\("(\w+)"\)', source)}
    if not calls:
        raise SystemExit(f"No JNI calls found in {MANAGED.relative_to(ROOT)}")
    return calls


def verify_jni(aar: Path) -> int:
    if not aar.exists():
        raise SystemExit(f"Missing wrapper AAR {aar}. Run Native/Android/build.sh first.")
    with tempfile.TemporaryDirectory() as tmp:
        jar = Path(tmp) / "classes.jar"
        with zipfile.ZipFile(aar) as archive:
            jar.write_bytes(archive.read("classes.jar"))
        output = subprocess.check_output(["javap", "-s", "-public", "-cp", str(jar), JAVA_CLASS], text=True)
    exported: set[tuple[str, str]] = set()
    pending: str | None = None
    for line in output.splitlines():
        match = re.match(r"\s*public static \S+ (\w+)\(", line)
        if match:
            pending = match.group(1)
            continue
        descriptor = re.match(r"\s*descriptor: (\S+)", line)
        if descriptor and pending:
            exported.add((pending, descriptor.group(1)))
            pending = None
    missing = sorted(managed_calls() - exported)
    if missing:
        for name, signature in missing:
            print(f"Missing Java method {JAVA_CLASS}.{name}{signature}", file=sys.stderr)
        return 1
    print(f"JNI contract verified ({len(managed_calls())} managed calls match {JAVA_CLASS})")
    return 0


def uleb128(data: bytes, offset: int) -> tuple[int, int]:
    result = shift = 0
    while True:
        byte = data[offset]
        offset += 1
        result |= (byte & 0x7F) << shift
        if byte < 0x80:
            return result, offset
        shift += 7


def dex_symbols(data: bytes) -> tuple[set[str], set[tuple[str, str]]]:
    def u32(offset: int) -> int:
        return struct.unpack_from("<I", data, offset)[0]

    string_count, string_offset = u32(0x38), u32(0x3C)
    strings = []
    for index in range(string_count):
        _, start = uleb128(data, u32(string_offset + index * 4))
        end = data.index(b"\0", start)
        strings.append(data[start:end].decode("utf-8", errors="replace"))
    type_count, type_offset = u32(0x40), u32(0x44)
    types = [strings[u32(type_offset + index * 4)] for index in range(type_count)]
    method_count, method_offset = u32(0x58), u32(0x5C)
    methods = set()
    for index in range(method_count):
        class_index, _, name_index = struct.unpack_from("<HHI", data, method_offset + index * 8)
        methods.add((types[class_index], strings[name_index]))
    class_count, class_offset = u32(0x60), u32(0x64)
    defined = {types[u32(class_offset + index * 32)] for index in range(class_count)}
    return defined, methods


def verify_apk(apk: Path, features: list[str]) -> int:
    defined: set[str] = set()
    methods: set[tuple[str, str]] = set()
    with zipfile.ZipFile(apk) as archive:
        dex_files = [name for name in archive.namelist() if re.fullmatch(r"classes\d*\.dex", name)]
        if not dex_files:
            raise SystemExit(f"No dex files in {apk}")
        for name in dex_files:
            classes, refs = dex_symbols(archive.read(name))
            defined |= classes
            methods |= refs
    required = set(REQUIRED_CLASSES) | {INTEROP_DESCRIPTOR}
    for feature in features:
        required |= FEATURE_CLASSES[feature]
    errors = [f"Class not defined in dex: {descriptor}" for descriptor in sorted(required - defined)]
    for name, _ in sorted(managed_calls()):
        if (INTEROP_DESCRIPTOR, name) not in methods:
            errors.append(f"Facade method removed by R8: {name}")
    if errors:
        print("\n".join(errors), file=sys.stderr)
        return 1
    print(f"{apk.name}: facade and {len(required)} required classes survived R8 ({len(dex_files)} dex files)")
    return 0


def main() -> int:
    parser = argparse.ArgumentParser(description=__doc__, formatter_class=argparse.RawDescriptionHelpFormatter)
    sub = parser.add_subparsers(dest="command", required=True)
    jni = sub.add_parser("jni")
    jni.add_argument("--aar", type=Path, default=DEFAULT_AAR)
    apk = sub.add_parser("apk")
    apk.add_argument("apk", type=Path)
    apk.add_argument("--feature", action="append", default=[], choices=sorted(FEATURE_CLASSES))
    args = parser.parse_args()
    if args.command == "jni":
        return verify_jni(args.aar)
    return verify_apk(args.apk, args.feature)


if __name__ == "__main__":
    sys.exit(main())
