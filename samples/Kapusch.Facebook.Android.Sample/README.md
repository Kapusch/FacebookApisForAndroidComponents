# Kapusch.Facebook.Android.Sample

Small, buildable Android sample project for validating:
- Managed API compilation
- Native asset injection via `AndroidAarLibrary` (wrapper + Facebook AARs) for
  `KapuschFacebookFeatures=Login;Share;AppEvents`
- The App Events facade surviving R8 shrinking (Release builds use `AndroidLinkTool=r8`)

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

Build with R8 and check the shrunk APK still contains the facade:
```bash
dotnet build samples/Kapusch.Facebook.Android.Sample/Kapusch.Facebook.Android.Sample.csproj -c Release
python3 scripts/verify-interop.py apk \
  samples/Kapusch.Facebook.Android.Sample/bin/Release/net10.0-android/com.kapusch.facebookandroid.sample-Signed.apk \
  --feature Login --feature Share
```

## Running end to end

The first build copies `Properties/AndroidManifest.template.xml` and
`Resources/values/strings.template` to untracked `AndroidManifest.xml` and
`strings.xml`. Put your own Meta app ID, client token and `fb<app id>` scheme in
the untracked `strings.xml` only; never commit them.

The manifest template sets every automatic collection switch to `false`
(`AutoInitEnabled`, `AutoLogAppEventsEnabled`, `AdvertiserIDCollectionEnabled`,
`AutoLogMetaDataEnabled`, `AutoLogProductImprovementEnabled`). The sample's
"Grant ads consent" button enables auto-init and advertiser ID collection, then
calls `ActivateApp()` with automatic logging still off; "Withdraw ads consent"
reverts them. Debug builds enable `SetDebugLoggingEnabled(true)`, so logged and
flushed events appear in `adb logcat`. Use Meta Events Manager > Test events to
confirm delivery.

### Access token attribution check

The facade creates a new `AppEventsLogger` for every call, because a logger tags
every event with the Facebook access token that was current when the logger was
created. To check that events follow the current login state, use a real Meta
app ID and:

1. log a custom event and flush;
2. sign in with Facebook Login, log another event and flush;
3. sign out, log a third event and flush.

Inspect the `access_token` field of each `POST /<app id>/activities` request
(an HTTPS proxy or the Android Studio Network Inspector; the SDK redacts tokens
in logcat). Only the request flushed in step 2 may carry the signed-in user's
token; steps 1 and 3 use the app's client token.
