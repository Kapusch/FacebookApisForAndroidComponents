# Kapusch.Facebook.Android

This package embeds a small Android interop Activity and the required Facebook Android SDK AARs, and injects them into consuming apps via `buildTransitive` MSBuild.

Set `KapuschFacebookFeatures` to `Login` (default), `Share`, or `Login;Share`.

## Notes
- No secrets are included.
- You must configure your AndroidManifest / resources as required by the Facebook Android SDK.
