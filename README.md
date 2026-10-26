# FacebookApisForAndroidComponents

Public OSS repository that packages selected Facebook Android features into a consumable .NET NuGet.

## Package

- NuGet ID: `Kapusch.Facebook.Android`

## What this repo ships

A NuGet package that:
- provides managed entry points for Facebook Login, photo sharing and App Events,
- redistributes the required **Facebook Android SDK AARs** (19.0.0) inside the `.nupkg` (classic/native packaging),
- injects the AARs into consuming apps via `buildTransitive` `AndroidAarLibrary` items.

Select any combination of `Login`, `Share` and `AppEvents` with
`KapuschFacebookFeatures` (for example `Login;AppEvents`). The default is
`Login`; only selected native AARs are injected into the consuming artifact.
`AppEvents` needs no extra AAR (facebook-core is always injected) and adds an R8
rule for purchase auto-logging.

## Dependency ingestion paths

- Facebook AARs: downloaded at repo build time from Maven Central by
  `restore-facebook-aars.sh`, verified against `DependencyLocks/Android/lockstate.txt`.
- The interop AAR compiles against the same Facebook version through Gradle.
- The sample (repo-only) references `Xamarin.Google.Android.InstallReferrer`, which
  downloads `installreferrer-2.2.aar` from Google Maven on its first build.
- Not bundled: AndroidX, Kotlin, Play Install Referrer and Google Play services.
  Consumers get them from their own graph (managed `Xamarin.*` NuGets); see
  `Docs/Integration.md` for the App Events recommendations.

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

Validate (all run in CI):
- `python3 scripts/verify-public.py` (no real ids or emails; private terms come from the `KAPUSCH_PRIVATE_TERMS` variable or the untracked `.git/info/private-terms`)
- `bash tests/validate-feature-selection.sh`
- `python3 scripts/app-event-constants.py` (C# constants match the bundled SDK)
- `python3 scripts/verify-interop.py jni` (C# JNI calls match the Java facade)
- `python3 scripts/validate-package.py artifacts/nuget`
- `python3 scripts/verify-interop.py apk <sample Release apk> --feature Login --feature Share` (facade survives R8)

## Consumption

- Install the package from NuGet.org for public release tags.
- Install the package from GitHub Packages for internal preview builds.
- Follow `Docs/Integration.md` for required AndroidManifest entries / meta-data.

## CI

- PR CI builds, packs, runs the validators and builds the sample with R8.
- Publishing is handled by `.github/workflows/publish.yml` with channel routing:
	- tag `vX.Y.Z` on `master` -> NuGet.org (stable)
	- tag `vX.Y.Z-rc.N` on `master` -> NuGet.org (pre-release)
	- non-tag runs (`workflow_dispatch`) -> GitHub Packages (`X.Y.Z-preview.<run>.<sha>`)
	- `workflow_dispatch` on `master` with `manual_version` -> NuGet.org (forced version)
- NuGet.org publishing uses NuGet Trusted Publishing (OIDC via `NuGet/login@v1`), no long-lived NuGet API key.

### Required GitHub secret

- `NUGET_USER`: your nuget.org profile username (not email), used by `NuGet/login@v1`.

See `Docs/Release.md` for the validated release sequence. Historical tags remain immutable even when obsolete branches are deleted.
