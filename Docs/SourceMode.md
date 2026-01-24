# Source mode

For local development, you can reference the project directly and inject the native AARs from the repo build output.

## Steps

1. Build native assets:
   - `bash src/Kapusch.FacebookApisForAndroidComponents/Native/Android/build.sh`
   - `bash src/Kapusch.FacebookApisForAndroidComponents/Native/Android/restore-facebook-aars.sh`
2. Reference `src/Kapusch.FacebookApisForAndroidComponents/Kapusch.FacebookApisForAndroidComponents.csproj` from your app.
3. Import the build targets:
   - `src/Kapusch.FacebookApisForAndroidComponents/buildTransitive/Kapusch.Facebook.Android.targets`
4. Set `UseKapuschFacebookAndroidInteropFromSource=true` in your app project.
