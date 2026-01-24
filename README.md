# FacebookApisForAndroidComponents

Public OSS repository that packages **Facebook Login for Android** into a consumable .NET NuGet.

## Package

- NuGet ID: `Kapusch.Facebook.Android`

## What this repo ships

A NuGet package that:
- provides a small managed API for starting a Facebook Login flow on Android, and
- redistributes the required **Facebook Android SDK AARs** inside the `.nupkg` (classic/native packaging),
- injects the AARs into consuming apps via `buildTransitive` `AndroidAarLibrary` items.

## Third-party licenses

See `THIRD_PARTY_NOTICES.md`.

## Developer docs

- Integration: `Docs/Integration.md`
- Source mode: `Docs/SourceMode.md`
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

- Install the package from GitHub Packages (pre-release).
- Follow `Docs/Integration.md` for required AndroidManifest entries / meta-data.

## CI

- PR CI is build-only.
- Publishing is handled by a workflow that pushes a pre-release to GitHub Packages.
