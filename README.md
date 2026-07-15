# FacebookApisForAndroidComponents

Public OSS repository that packages selected Facebook Android features into a consumable .NET NuGet.

## Package

- NuGet ID: `Kapusch.Facebook.Android`

## What this repo ships

A NuGet package that:
- provides managed entry points for Facebook Login and photo sharing, and
- redistributes the required **Facebook Android SDK AARs** inside the `.nupkg` (classic/native packaging),
- injects the AARs into consuming apps via `buildTransitive` `AndroidAarLibrary` items.

Select `KapuschFacebookFeatures=Login`, `Share`, or `Login;Share`. The default is
`Login`; only selected native AARs are injected into the consuming artifact.

## Third-party licenses

See `THIRD_PARTY_NOTICES.md`.

## Developer docs

- Integration: `Docs/Integration.md`
- Source mode: `Docs/SourceMode.md`
- Release workflow: `Docs/Release.md`
- Samples: `samples/README.md`

## Build (local)

Prereqs:
- JDK 21
- Android SDK installed (set `ANDROID_SDK_ROOT` or `ANDROID_HOME`)
- .NET SDK 10 (`global.json` pins 10.0.100)

Build the native wrapper (repo-only):
- `bash src/Kapusch.FacebookApisForAndroidComponents/Native/Android/build.sh`

Restore pinned Facebook AARs (repo-only):
- `bash src/Kapusch.FacebookApisForAndroidComponents/Native/Android/restore-facebook-aars.sh`

Pack the NuGet:
- `dotnet pack src/Kapusch.FacebookApisForAndroidComponents/Kapusch.FacebookApisForAndroidComponents.csproj -c Release -o artifacts/nuget`

## Consumption

- Install the package from NuGet.org for public release tags.
- Install the package from GitHub Packages for internal preview builds.
- Follow `Docs/Integration.md` for required AndroidManifest entries / meta-data.

## CI

- PR CI is build-only.
- Publishing is handled by `.github/workflows/publish.yml` with channel routing:
	- tag `vX.Y.Z` on `master` -> NuGet.org (stable)
	- tag `vX.Y.Z-rc.N` on `master` -> NuGet.org (pre-release)
	- non-tag runs (`workflow_dispatch`) -> GitHub Packages (`X.Y.Z-preview.<run>.<sha>`)
	- `workflow_dispatch` on `master` with `manual_version` -> NuGet.org (forced version)
- NuGet.org publishing uses NuGet Trusted Publishing (OIDC via `NuGet/login@v1`), no long-lived NuGet API key.

### Required GitHub secret

- `NUGET_USER`: your nuget.org profile username (not email), used by `NuGet/login@v1`.

See `Docs/Release.md` for the validated release sequence. Historical tags remain immutable even when obsolete branches are deleted.
