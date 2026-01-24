# Contributing

Thanks for contributing!

## Prerequisites

- JDK 21
- Android SDK installed (set `ANDROID_SDK_ROOT` or `ANDROID_HOME`)
- .NET SDK 10 (this repo pins `10.0.100` via `global.json`)

## Local build

If you are working without the NuGet (ProjectReference), see `Docs/SourceMode.md`.

Build the Android wrapper AAR:

- `bash src/Kapusch.FacebookApisForAndroidComponents/Native/Android/build.sh`

Restore pinned Facebook AARs:

- `bash src/Kapusch.FacebookApisForAndroidComponents/Native/Android/restore-facebook-aars.sh`

Pack the NuGet:

- `dotnet pack src/Kapusch.FacebookApisForAndroidComponents/Kapusch.FacebookApisForAndroidComponents.csproj -c Release -o artifacts/nuget`

## Formatting

- C#: follow `.editorconfig` (tabs, LF).
- Java/Kotlin/Gradle: keep changes minimal and consistent with existing style.

## Pull requests

- Keep PRs focused and well-scoped.
- Do not commit secrets.
- If you update the Facebook SDK version, update both:
  - `src/Kapusch.FacebookApisForAndroidComponents/Native/Android/facebookinterop/build.gradle.kts`
  - `DependencyLocks/Android/lockstate.txt` (URLs + SHA256)

## License

By contributing, you agree that your contributions will be licensed under the repository license (MIT).
