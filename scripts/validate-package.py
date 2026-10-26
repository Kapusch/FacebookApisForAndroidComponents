#!/usr/bin/env python3
"""Validate the packed Kapusch.Facebook.Android nupkg against the repository contract.

Checks the required entries, that android/deps holds exactly the locked Facebook AARs with their
locked SHA-256 digests (one SDK version), that no shared AndroidX/Kotlin/Google artifacts are
bundled, that the wrapper AAR carries the App Events facade and its consumer keep rule, and that
the default feature selection is still Login.

Usage:
  python3 scripts/validate-package.py [artifacts/nuget]
"""

from __future__ import annotations

import hashlib
import re
import sys
import zipfile
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
LOCK_FILE = ROOT / "DependencyLocks/Android/lockstate.txt"
PACKAGE_ID = "Kapusch.Facebook.Android"
WRAPPER = "android/aar/kfb-facebookinterop-release.aar"
REQUIRED = {
    "THIRD_PARTY_NOTICES.md",
    "nuget-readme.md",
    WRAPPER,
    "buildTransitive/Kapusch.Facebook.Android.props",
    "buildTransitive/Kapusch.Facebook.Android.targets",
    "buildTransitive/proguard/kapusch-facebook-appevents.pro",
}
WRAPPER_CLASSES = {
    "com/kapusch/facebook/androidinterop/FacebookAppEventsInterop.class",
    "com/kapusch/facebook/androidinterop/FacebookLoginActivity.class",
    "com/kapusch/facebook/androidinterop/FacebookShareActivity.class",
    "com/kapusch/facebook/androidinterop/FacebookLogoutReceiver.class",
}
# Shared dependencies must come from the consumer's graph (managed Xamarin.* NuGets).
FORBIDDEN = re.compile(r"(?i)(androidx|kotlin|installreferrer|billing|play-services|gson)")


def read_lock() -> dict[str, str]:
    entries = {}
    for line in LOCK_FILE.read_text(encoding="utf-8").splitlines():
        line = line.strip()
        if not line or line.startswith("#"):
            continue
        _, sha, filename = line.split()
        entries[filename] = sha
    return entries


def main() -> int:
    folder = Path(sys.argv[1]) if len(sys.argv) > 1 else ROOT / "artifacts/nuget"
    packages = sorted(folder.glob(f"{PACKAGE_ID}.*.nupkg"))
    if len(packages) != 1:
        raise SystemExit(f"Expected exactly one {PACKAGE_ID} nupkg in {folder}, found {len(packages)}")
    package = packages[0]
    errors: list[str] = []
    lock = read_lock()

    with zipfile.ZipFile(package) as archive:
        names = set(archive.namelist())
        errors += [f"missing entry {entry}" for entry in sorted(REQUIRED - names)]
        if not any(re.fullmatch(r"lib/net10\.0-android[0-9.]*/Kapusch\.Facebook\.Android\.dll", n) for n in names):
            errors.append("missing lib/net10.0-android*/Kapusch.Facebook.Android.dll")

        deps = {n.removeprefix("android/deps/"): n for n in names if n.startswith("android/deps/") and not n.endswith("/")}
        if set(deps) != set(lock):
            errors.append(f"android/deps {sorted(deps)} does not match lockstate {sorted(lock)}")
        for filename, entry in deps.items():
            digest = hashlib.sha256(archive.read(entry)).hexdigest()
            if lock.get(filename) != digest:
                errors.append(f"{entry} SHA-256 {digest} does not match lockstate")
        versions = {re.sub(r"^facebook-[a-z]+-(.+)\.aar$", r"\1", f) for f in lock}
        if len(versions) != 1:
            errors.append(f"locked Facebook AARs mix versions: {sorted(versions)}")

        for name in sorted(names):
            if name.startswith("android/") and FORBIDDEN.search(name):
                errors.append(f"shared dependency must not be bundled: {name}")

        if WRAPPER in names:
            with zipfile.ZipFile(archive.open(WRAPPER)) as wrapper:
                wrapper_names = set(wrapper.namelist())
                with zipfile.ZipFile(wrapper.open("classes.jar")) as classes:
                    missing = WRAPPER_CLASSES - set(classes.namelist())
                    errors += [f"wrapper AAR misses {c}" for c in sorted(missing)]
                rules = wrapper.read("proguard.txt").decode() if "proguard.txt" in wrapper_names else ""
                if "-keep class com.kapusch.facebook.androidinterop.** { *; }" not in rules:
                    errors.append("wrapper AAR proguard.txt must keep com.kapusch.facebook.androidinterop.**")

        props = archive.read("buildTransitive/Kapusch.Facebook.Android.props").decode()
        if ">Login</KapuschFacebookFeatures>" not in props:
            errors.append("default KapuschFacebookFeatures must stay Login")
        targets = archive.read("buildTransitive/Kapusch.Facebook.Android.targets").decode()
        if "';AppEvents;'" not in targets or "kapusch-facebook-appevents.pro" not in targets:
            errors.append("targets must wire the AppEvents feature and its R8 rules")

    if errors:
        print(f"{package.name} failed validation:\n  " + "\n  ".join(errors), file=sys.stderr)
        return 1
    print(f"{package.name} validated ({len(names)} entries, Facebook SDK {versions.pop()})")
    return 0


if __name__ == "__main__":
    sys.exit(main())
