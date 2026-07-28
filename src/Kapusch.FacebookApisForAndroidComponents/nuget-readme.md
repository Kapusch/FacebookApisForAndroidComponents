# Kapusch.Facebook.Android

This package embeds a small Android interop Activity and the required Facebook Android SDK AARs, and injects them into consuming apps via `buildTransitive` MSBuild.

Set `KapuschFacebookFeatures` to `Login` (default), `Share`, or `Login;Share`.

## Notes
- No secrets are included.
- You must configure your AndroidManifest / resources as required by the Facebook Android SDK.
- Photo sharing reports native and web ShareDialog availability through a
  stable `share_dialog_unavailable_native_<0|1>_web_<0|1>` error code when the
  SDK cannot open a supported composer.
