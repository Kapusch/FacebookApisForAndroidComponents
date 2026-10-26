# Injected by Kapusch.Facebook.Android only when KapuschFacebookFeatures contains AppEvents.
#
# facebook-core logs Google Play Billing purchases and subscriptions automatically (when automatic
# App Events logging and the Events Manager setting allow it) by looking billing classes and
# methods up by name through reflection. facebook-core's own consumer rules do not keep them, so
# R8 can remove members only reached that way (and rename them when an app turns obfuscation on;
# .NET for Android passes -dontobfuscate by default). The SDK then silently skips purchase
# auto-logging. These rules are inert when Play Billing is not in the app.
-keep public class com.android.billingclient.api.** { public *; }
