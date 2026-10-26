# Kapusch.Facebook.Android

This package embeds a small Android interop AAR and the required Facebook Android SDK AARs (19.0.0), and injects them into consuming apps via `buildTransitive` MSBuild.

Set `KapuschFacebookFeatures` to any combination of `Login` (default), `Share` and `AppEvents`, for example `Login;AppEvents`.

`AppEvents` exposes `FacebookAppEvents` (SDK initialization, consent switches, `ActivateApp`, `LogEvent`, `LogPurchase`, `LogStartTrial`, `LogSubscribe`, `LogCompleteRegistration`, anonymous ID, user ID and hashed user data) plus `FacebookAppEventNames` and `FacebookAppEventParameterNames` constants.

## Notes
- No secrets are included.
- You must configure your AndroidManifest / resources as required by the Facebook Android SDK.
- The SDK collects automatically by default. To gate App Events on consent, set `com.facebook.sdk.AutoInitEnabled`, `AutoLogAppEventsEnabled`, `AdvertiserIDCollectionEnabled`, `AutoLogMetaDataEnabled` and `AutoLogProductImprovementEnabled` to `false` in the manifest and enable them at runtime. See the repository's `Docs/Integration.md`.
- With `AppEvents`, add `Xamarin.Google.Android.InstallReferrer` so Meta can read the Play install referrer (warning `KFBA001` otherwise).
- Do not log trial, subscription or purchase events from the app when a server already sends them to Meta, and turn automatic purchase logging off in that case.
- Photo sharing reports native and web ShareDialog availability through a
  stable `share_dialog_unavailable_native_<0|1>_web_<0|1>` error code when the
  SDK cannot open a supported composer.
