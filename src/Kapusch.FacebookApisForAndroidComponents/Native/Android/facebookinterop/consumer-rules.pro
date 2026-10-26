# Keep public surface (interop activities, receiver and App Events facade).
# FacebookAppEventsInterop is reached only from managed code through JNI by
# name, so R8 cannot see its callers and must not rename or remove it.
-keep class com.kapusch.facebook.androidinterop.** { *; }
