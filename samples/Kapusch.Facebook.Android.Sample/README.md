# Kapusch.Facebook.Android.Sample

Small, buildable Android sample project for validating:
- Managed API compilation
- Native asset injection via `AndroidAarLibrary` (wrapper + Facebook AARs)

No secrets are committed.

## Build (local)

Prereqs:
- JDK 21
- Android SDK (`ANDROID_SDK_ROOT` or `ANDROID_HOME`)
- .NET SDK 10 (`global.json` pins 10.0.100)

Build native assets (repo-only):
```bash
bash src/Kapusch.FacebookApisForAndroidComponents/Native/Android/build.sh
bash src/Kapusch.FacebookApisForAndroidComponents/Native/Android/restore-facebook-aars.sh
```

Build the sample:
```bash
dotnet build samples/Kapusch.Facebook.Android.Sample/Kapusch.Facebook.Android.Sample.csproj -c Debug
```
