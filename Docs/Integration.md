# Integration (Android)

This package injects a small Android Activity (provided by an embedded AAR) that runs the Facebook Login flow and returns its result via `Intent` extras.

Set `KapuschFacebookFeatures` to `Login` (default), `Share`, or `Login;Share`.

## Required AndroidManifest entries

You must include the required Meta/Facebook Android SDK entries in your app manifest (example only; use resources, do not hardcode secrets):

- `com.facebook.sdk.ApplicationId`
- `com.facebook.sdk.ClientToken`

You also need to declare `com.facebook.FacebookActivity` and `com.facebook.CustomTabActivity` as per Facebook SDK documentation.

## Launching the flow

From your app, start the interop Activity:
- `AndroidFacebookInterop.LoginActivityClassName`

Then parse the `Intent` extras:
- `AndroidFacebookInterop.ExtraStatus`
- `AndroidFacebookInterop.ExtraAccessToken`
- `AndroidFacebookInterop.ExtraUserId`
- `AndroidFacebookInterop.ExtraErrorCode`
- `AndroidFacebookInterop.ExtraErrorMessage`

## Sign-out

Call `AndroidFacebookInterop.SendSignOutBroadcast(context)`.

## Photo sharing

With the `Share` feature selected, start
`AndroidFacebookInterop.ShareActivityClassName` and pass a JPEG file path using
`AndroidFacebookInterop.ExtraShareImagePath`. Read `ExtraStatus` and
`ExtraErrorCode` from the activity result. The wrapper uses `SharePhotoContent`
and never injects a message.

The wrapper records the ShareDialog mode availability in Android logs. When no
supported dialog mode is available, `ExtraStatus` is `failed` and
`ExtraErrorCode` reports
`share_dialog_unavailable_native_<0|1>_web_<0|1>`. Consumers may use that
diagnostic to offer an explicit Android image-intent fallback. The wrapper does
not silently change transport.
