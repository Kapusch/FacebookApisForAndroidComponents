# Integration (Android)

This package injects a small Android Activity (provided by an embedded AAR) that runs the Facebook Login flow and returns its result via `Intent` extras.

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
