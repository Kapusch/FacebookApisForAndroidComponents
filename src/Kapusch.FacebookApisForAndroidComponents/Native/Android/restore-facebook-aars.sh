#!/usr/bin/env bash
set -euo pipefail

ROOT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
REPO_ROOT="$(cd "$ROOT_DIR/../../../.." && pwd)"
LOCK_FILE="$REPO_ROOT/DependencyLocks/Android/lockstate.txt"
DEPS_OUT="$ROOT_DIR/build/deps"

mkdir -p "$DEPS_OUT"

if [ ! -f "$LOCK_FILE" ]; then
  echo "Missing lockstate.txt (expected at $LOCK_FILE)." >&2
  exit 1
fi

sha256_file() {
  local file="$1"
  if command -v shasum >/dev/null 2>&1; then
    shasum -a 256 "$file" | awk '{print $1}'
    return 0
  fi
  if command -v sha256sum >/dev/null 2>&1; then
    sha256sum "$file" | awk '{print $1}'
    return 0
  fi
  echo "No SHA-256 tool found (need shasum or sha256sum)." >&2
  return 1
}

echo "[KapuschFacebookInteropAndroid] Restoring pinned dependencies from lock file..."

tmp_dir="$(mktemp -d)"
cleanup() { rm -rf "$tmp_dir"; }
trap cleanup EXIT

downloaded_any=false

while IFS= read -r line || [ -n "$line" ]; do
  case "$line" in
    ''|'#'*) continue ;;
  esac

  url="$(echo "$line" | awk '{print $1}')"
  expected_sha="$(echo "$line" | awk '{print $2}')"
  filename="$(echo "$line" | awk '{print $3}')"

  if [ -z "$url" ] || [ -z "$expected_sha" ] || [ -z "$filename" ]; then
    echo "Invalid lock line (expected: <url> <sha256> <filename>): $line" >&2
    exit 1
  fi

  out_path="$DEPS_OUT/$filename"
  tmp_path="$tmp_dir/$filename"

  echo "[KapuschFacebookInteropAndroid] Downloading $filename"
  curl -fsSL --retry 3 --retry-delay 1 -o "$tmp_path" "$url"

  actual_sha="$(sha256_file "$tmp_path")"
  if [ "$actual_sha" != "$expected_sha" ]; then
    echo "SHA256 mismatch for $filename" >&2
    echo "Expected: $expected_sha" >&2
    echo "Actual:   $actual_sha" >&2
    exit 1
  fi

  mv -f "$tmp_path" "$out_path"
  downloaded_any=true

done < "$LOCK_FILE"

if [ "$downloaded_any" != true ]; then
  echo "No artifacts restored (lock file empty?)." >&2
  exit 1
fi

if ls "$DEPS_OUT"/facebook-*.aar >/dev/null 2>&1; then
  echo "[KapuschFacebookInteropAndroid] Done: $DEPS_OUT"
  exit 0
fi

echo "Restored artifacts did not include any facebook-*.aar. Check lock file contents." >&2
exit 1
