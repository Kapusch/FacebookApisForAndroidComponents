#!/usr/bin/env bash
set -euo pipefail

ROOT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
BUILD_DIR="$ROOT_DIR/build"

GRADLE_VERSION="8.7"
GRADLE_HOME="$ROOT_DIR/.gradle/gradle-$GRADLE_VERSION"
GRADLE_BIN="$GRADLE_HOME/bin/gradle"
GRADLE_USER_HOME="${GRADLE_USER_HOME:-$ROOT_DIR/.gradle/user-home}"
export GRADLE_USER_HOME

ANDROID_SDK="${ANDROID_SDK_ROOT:-${ANDROID_HOME:-}}"
if [ -z "$ANDROID_SDK" ]; then
  DEFAULT_MACOS_SDK="$HOME/Library/Android/sdk"
  DEFAULT_LINUX_SDK="$HOME/Android/Sdk"

  if [ -d "$DEFAULT_MACOS_SDK" ]; then
    ANDROID_SDK="$DEFAULT_MACOS_SDK"
  elif [ -d "$DEFAULT_LINUX_SDK" ]; then
    ANDROID_SDK="$DEFAULT_LINUX_SDK"
  else
    echo "ANDROID_SDK_ROOT (or ANDROID_HOME) is required to build Android native interop." >&2
    exit 1
  fi
fi

mkdir -p "$ROOT_DIR/.gradle" "$GRADLE_USER_HOME"
cat > "$ROOT_DIR/local.properties" <<EOF2
sdk.dir=$ANDROID_SDK
EOF2

if [ ! -x "$GRADLE_BIN" ]; then
  echo "[KapuschFacebookInteropAndroid] Bootstrapping Gradle $GRADLE_VERSION..."
  ARCHIVE="$ROOT_DIR/.gradle/gradle-$GRADLE_VERSION-bin.zip"
  curl -fsSL -o "$ARCHIVE" "https://services.gradle.org/distributions/gradle-$GRADLE_VERSION-bin.zip"
  unzip -q -o "$ARCHIVE" -d "$ROOT_DIR/.gradle"
fi

rm -rf "$BUILD_DIR"
mkdir -p "$BUILD_DIR/aar" "$BUILD_DIR/deps"

echo "[KapuschFacebookInteropAndroid] Building AAR..."
"$GRADLE_BIN" -p "$ROOT_DIR" :facebookinterop:assembleRelease

cp "$ROOT_DIR/facebookinterop/build/outputs/aar/facebookinterop-release.aar" "$BUILD_DIR/aar/kfb-facebookinterop-release.aar"

echo "[KapuschFacebookInteropAndroid] Done: $BUILD_DIR"
