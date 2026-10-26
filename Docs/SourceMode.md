# Source mode

For local development, you can reference the project directly and inject the native AARs from the repo build output.

## Steps

1. Build native assets:
   - `bash src/Kapusch.FacebookApisForAndroidComponents/Native/Android/build.sh`
   - `bash src/Kapusch.FacebookApisForAndroidComponents/Native/Android/restore-facebook-aars.sh`
2. Reference `src/Kapusch.FacebookApisForAndroidComponents/Kapusch.FacebookApisForAndroidComponents.csproj` from your app.
3. Import the build files in NuGet's order:
   - `src/Kapusch.FacebookApisForAndroidComponents/buildTransitive/Kapusch.Facebook.Android.props` at the top of your project (it sets the default `KapuschFacebookFeatures=Login`),
   - `src/Kapusch.FacebookApisForAndroidComponents/buildTransitive/Kapusch.Facebook.Android.targets` at the bottom.
4. Set `UseKapuschFacebookAndroidInteropFromSource=true` in your app project.

Without the `.props` import, `KapuschFacebookFeatures` stays empty unless you
set it, and only the always-injected core AARs are added. See
`samples/Kapusch.Facebook.Android.Sample` for a working setup.
