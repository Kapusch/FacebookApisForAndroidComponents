#!/usr/bin/env bash

set -euo pipefail

project="tests/FeatureSelection.proj"

check() {
  dotnet msbuild "$project" -nologo -v:q -t:ValidateFeatureSelection "$@"
}

# Default selection must stay Login only for existing consumers.
check -p:ExpectedLoginEnabled=True -p:ExpectedShareEnabled=False -p:ExpectedAppEventsEnabled=False

check -p:RequestedFacebookFeatures=Share -p:ExpectedLoginEnabled=False -p:ExpectedShareEnabled=True -p:ExpectedAppEventsEnabled=False

check -p:RequestedFacebookFeatures=Login%3BShare -p:ExpectedLoginEnabled=True -p:ExpectedShareEnabled=True -p:ExpectedAppEventsEnabled=False

check -p:RequestedFacebookFeatures=AppEvents -p:ExpectedLoginEnabled=False -p:ExpectedShareEnabled=False -p:ExpectedAppEventsEnabled=True

check -p:RequestedFacebookFeatures=Login%3BAppEvents -p:ExpectedLoginEnabled=True -p:ExpectedShareEnabled=False -p:ExpectedAppEventsEnabled=True

check "-p:RequestedFacebookFeatures=Login%3B Share%3B AppEvents" -p:ExpectedLoginEnabled=True -p:ExpectedShareEnabled=True -p:ExpectedAppEventsEnabled=True

echo "Feature selection validated"

# KFBA001: AppEvents without a Play Install Referrer library warns; every other case stays silent.
referrer_check() {
  dotnet msbuild tests/InstallReferrerCheck.proj -nologo -v:q -t:CheckInstallReferrer -warnaserror "$@"
}

if referrer_check -p:RequestedFacebookFeatures=AppEvents >/dev/null 2>&1; then
  echo "Expected KFBA001 for AppEvents without an install referrer library" >&2
  exit 1
fi
referrer_check -p:RequestedFacebookFeatures=AppEvents -p:ReferenceInstallReferrer=managed
referrer_check -p:RequestedFacebookFeatures=AppEvents -p:ReferenceInstallReferrer=aar
referrer_check -p:RequestedFacebookFeatures=AppEvents -p:KapuschFacebookAndroidSuppressInstallReferrerWarning=true
referrer_check

echo "Install referrer check validated"
