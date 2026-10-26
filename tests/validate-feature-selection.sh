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
