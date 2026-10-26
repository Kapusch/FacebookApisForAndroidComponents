# Integration (Android)

`Kapusch.Facebook.Android` injects a small interop AAR plus the pinned Facebook
Android SDK AARs into the consuming app. Select the features with
`KapuschFacebookFeatures`:

| Value | Injected Facebook AARs | Managed entry points |
| --- | --- | --- |
| `Login` (default) | core, common, bolts, login | `AndroidFacebookInterop` (Login activity, sign-out) |
| `Share` | core, common, bolts, share | `AndroidFacebookInterop` (Share activity) |
| `AppEvents` | core, common, bolts | `FacebookAppEvents`, `FacebookAppEventNames`, `FacebookAppEventParameterNames` |

Combine values with `;`, for example `Login;AppEvents`. facebook-core is always
injected, so the Facebook SDK initialization and its manifest entries are the
same for every selection.

```xml
<PropertyGroup>
  <KapuschFacebookFeatures>Login;AppEvents</KapuschFacebookFeatures>
</PropertyGroup>
```

## Runtime dependencies the app provides

The package redistributes only the Facebook AARs. Their POMs also declare
AndroidX and Kotlin runtime dependencies (annotation, core-ktx,
legacy-support-core-utils including `LocalBroadcastManager`, appcompat,
cardview, browser, activity, fragment, kotlin-stdlib), which the app must
provide through managed `Xamarin.AndroidX.*` / `Xamarin.Kotlin.StdLib`
packages. .NET MAUI apps usually have most of them already; a bare
`net-android` app needs at least `Xamarin.AndroidX.AppCompat`,
`Xamarin.AndroidX.CardView` and `Xamarin.AndroidX.LocalBroadcastManager`
(facebook-core crashes at startup with `NoClassDefFoundError` without the
latter). See `samples/Kapusch.Facebook.Android.Sample`.

## Required AndroidManifest entries

You must include the required Meta/Facebook Android SDK entries in your app manifest (example only; use resources, do not hardcode secrets):

- `com.facebook.sdk.ApplicationId`
- `com.facebook.sdk.ClientToken`

You also need to declare `com.facebook.FacebookActivity` and `com.facebook.CustomTabActivity` as per Facebook SDK documentation.

## Login

From your app, start the interop Activity:
- `AndroidFacebookInterop.LoginActivityClassName`

Then parse the `Intent` extras:
- `AndroidFacebookInterop.ExtraStatus`
- `AndroidFacebookInterop.ExtraAccessToken`
- `AndroidFacebookInterop.ExtraUserId`
- `AndroidFacebookInterop.ExtraErrorCode`
- `AndroidFacebookInterop.ExtraErrorMessage`

### Sign-out

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

## App Events

`FacebookAppEvents` is a thin JNI bridge to `FacebookSdk` and
`AppEventsLogger`. The SDK keeps its own disk queue, batching and retries; the
wrapper adds no queue and changes no SDK default.

### Dependencies the app provides

- **Play Install Referrer (recommended).** facebook-core sends the Play install
  referrer with the install ping and events only when the
  `com.android.installreferrer` classes are in the app. Add the managed binding:

  ```xml
  <PackageReference Include="Xamarin.Google.Android.InstallReferrer" Version="2.2.0.9" />
  ```

  The package does not bundle `installreferrer-2.2.aar`: other SDKs need the
  same library, and two copies of it fail the build with duplicate classes.
  That binding downloads the AAR from Google Maven on the first build
  (Xamarin.Build.Download). With `AppEvents` selected and no install referrer
  library referenced, the build emits warning `KFBA001`; set
  `KapuschFacebookAndroidSuppressInstallReferrerWarning=true` to silence it.
- **Google advertising ID (optional).** facebook-core reads the advertising ID
  through `com.google.android.gms.ads.identifier.AdvertisingIdClient` when that
  class is present (for example via `Xamarin.GooglePlayServices.Ads.Identifier`)
  and advertiser ID collection is enabled.

facebook-core also merges `com.google.android.gms.permission.AD_ID` and four
`android.permission.ACCESS_ADSERVICES_*` permissions into the app manifest.
Remove the ones you do not want with `tools:node="remove"`.

### Manifest switches

All of these default to `true` in the SDK. The runtime setters persist their
value, and a persisted value wins over the manifest on later launches.

| Manifest `meta-data` (`com.facebook.sdk.`) | Runtime API | Effect |
| --- | --- | --- |
| `AutoInitEnabled` | `SetAutoInitEnabled`, `IsAutoInitEnabled` | `false` keeps the SDK from fully initializing; the SDK does not process Graph API responses until then. |
| `AutoLogAppEventsEnabled` | `SetAutoLogAppEventsEnabled`, `IsAutoLogAppEventsEnabled` | Automatic install ping, app activation and, when Events Manager allows it, Google Play purchase and subscription events. Since 19.0.0 a local `false` wins over Events Manager. |
| `AdvertiserIDCollectionEnabled` | `SetAdvertiserIdCollectionEnabled`, `IsAdvertiserIdCollectionEnabled` | Advertising ID collection. |
| `AutoLogMetaDataEnabled` | `SetAutoLogMetaDataEnabled`, `IsAutoLogMetaDataEnabled` | SDK 19.0.0+: attaches the screen title and AppLink URL to events. `false` also discards what was collected. |
| `AutoLogProductImprovementEnabled` | `SetAutoLogProductImprovementEnabled`, `IsAutoLogProductImprovementEnabled` | SDK 19.0.0+: control for product-improvement data. The 19.0.0 changelog says the SDK does not collect such data yet. |

`SetDataProcessingOptions(new[] { FacebookDataProcessingOptions.LimitedDataUse })`
enables Limited Data Use (country and state `0` let Meta geolocate the user);
an empty array opts out explicitly.

### Consent-gating recipe

1. Ship every switch off in the app manifest:

   ```xml
   <meta-data android:name="com.facebook.sdk.AutoInitEnabled" android:value="false" />
   <meta-data android:name="com.facebook.sdk.AutoLogAppEventsEnabled" android:value="false" />
   <meta-data android:name="com.facebook.sdk.AdvertiserIDCollectionEnabled" android:value="false" />
   <meta-data android:name="com.facebook.sdk.AutoLogMetaDataEnabled" android:value="false" />
   <meta-data android:name="com.facebook.sdk.AutoLogProductImprovementEnabled" android:value="false" />
   ```

2. On every launch, apply the stored consent explicitly, because the runtime
   values persist across launches:

   ```csharp
   if (consentGranted)
   {
       FacebookAppEvents.SetAutoInitEnabled(true);
       FacebookAppEvents.SetAdvertiserIdCollectionEnabled(true);
       // Either let the SDK log automatically ...
       // FacebookAppEvents.SetAutoLogAppEventsEnabled(true);
       // ... or keep automatic logging off and log activation yourself. ActivateApp()
       // still sends the one-time install ping and starts activate/deactivate tracking.
       FacebookAppEvents.SetAutoLogAppEventsEnabled(false);
       FacebookAppEvents.ActivateApp();
   }
   ```

3. On withdrawal, turn everything off and clear the identifiers you set:

   ```csharp
   FacebookAppEvents.SetAutoLogAppEventsEnabled(false);
   FacebookAppEvents.SetAdvertiserIdCollectionEnabled(false);
   FacebookAppEvents.SetAutoInitEnabled(false);
   FacebookAppEvents.ClearUserId();
   FacebookAppEvents.ClearUserData();
   ```

   Stop logging events manually as well. The SDK cannot delete events it
   already sent; that goes through Meta's data deletion process.

The `FacebookInitProvider` merged from facebook-core still calls
`FacebookSdk.sdkInitialize` at process start, with `AutoInitEnabled=false`. The
SDK then still prepares its startup Graph requests and rejects them before
processing a response (logcat: `GraphRequest can't be used when Facebook SDK
isn't fully initialized`). For the strictest setup, remove the provider and
initialize after consent:

```xml
<!-- Requires xmlns:tools="http://schemas.android.com/tools" on <manifest>. -->
<provider android:name="com.facebook.internal.FacebookInitProvider" tools:node="remove" />
```

```csharp
FacebookAppEvents.InitializeSdk(); // before any App Events, Login or Share call
```

Without the provider, every SDK feature (including Login and Share) needs
`InitializeSdk()` first. Calls that need an initialized SDK throw
`Java.Lang.IllegalStateException` otherwise.

### Logging events

```csharp
FacebookAppEvents.LogStartTrial(value: 0m, currencyIsoCode: "EUR", orderId: subscriptionId);
FacebookAppEvents.LogSubscribe(value: 4.99m, currencyIsoCode: "EUR", orderId: subscriptionId);
FacebookAppEvents.LogPurchase(4.99m, "EUR");
FacebookAppEvents.LogCompleteRegistration(registrationMethod: "email");
FacebookAppEvents.LogEvent(
    FacebookAppEventNames.ViewedContent,
    new Dictionary<string, object> { [FacebookAppEventParameterNames.ContentId] = "item-1" }
);
```

- `FacebookAppEventNames`, `FacebookAppEventParameterNames` and
  `FacebookAppEventParameterValues` mirror `AppEventsConstants` of the bundled
  SDK exactly (generated by `scripts/app-event-constants.py`). Note that the
  subscription event is `Subscribe`, not `fb_mobile_subscribe`.
- Parameter values may be `string`, `bool` (sent as `"1"`/`"0"`), integral
  types, finite `float`/`double`, or `decimal`.
- `fb_order_id` is the ID shared by every event of one subscription.
- `LogPurchase` flushes immediately unless the flush behavior is
  `FacebookFlushBehavior.ExplicitOnly`. `Flush()` is not a delivery receipt.

### Avoid double counting with server-side events

Do not rely on Meta to deduplicate SDK events against server-side events
(Conversions API, or a subscription platform's Meta integration): that needs an
event ID shared by both sides, and such integrations do not coordinate one with
the SDK. When a server already sends trial, subscription or purchase events:

- do not log those events from the app as well;
- disable automatic purchase logging: keep `AutoLogAppEventsEnabled=false` and
  call `ActivateApp()` (install and activation are still measured), and/or set
  "Log In-App Events Automatically" to "No" in the Meta App Dashboard.

### Identity

- `GetAnonymousId()` returns the SDK's persisted anonymous app/device ID, for
  example to forward to a server-side integration.
- `SetUserId` / `ClearUserId` attach or remove a user ID on subsequent events.
- `SetUserData(new FacebookUserData { Email = ..., ExternalId = ... })` enables
  advanced matching; the SDK normalizes and SHA-256 hashes every value before
  persisting or sending it. Collect such data only with a legal basis.

### Diagnostics

`FacebookAppEvents.SetDebugLoggingEnabled(true)` turns on SDK debug mode and the
`APP_EVENTS` logging behavior, so logged and flushed events appear in
`adb logcat`. Confirm delivery in Meta Events Manager > Test events. Turn it off
in production builds.

### R8 and shrinking

Nothing to configure. The interop AAR keeps its own classes (they are reached
only through JNI), facebook-core ships its own consumer rules, and the
`AppEvents` feature adds `buildTransitive/proguard/kapusch-facebook-appevents.pro`,
which keeps the Play Billing API that facebook-core calls by reflection for
purchase auto-logging. CI builds the sample with R8 and checks the shrunk APK.

### Not exposed on Android

Push-notification open logging, product catalog items, WebView augmentation,
codeless setup and manual install referrer injection are not wrapped. The
iOS-only concepts (App Tracking Transparency, SKAdNetwork, Aggregated Event
Measurement and URL forwarding) have no Android equivalent.

## Upgrading to 1.2.0

1.2.0 moves every Facebook AAR from 18.1.3 to 19.0.0. The default feature
selection, the injected AARs and the merged manifest are unchanged, but the SDK
itself behaves differently:

- With automatic logging on (the SDK default), 19.0.0 can auto-log Google Play
  Billing Library 8 purchases and subscriptions when Meta enables it server
  side; 18.x had no Billing 8 support. Apps that also send those events from a
  server should turn automatic purchase logging off as described above.
- From 2026-10-12 (per the 19.0.0 changelog), events carry the screen title and
  AppLink URL unless `AutoLogMetaDataEnabled` is `false`.
- A local `AutoLogAppEventsEnabled=false` now wins over the Events Manager
  setting.
