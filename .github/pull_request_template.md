## Summary

- What does this change do?

## Checklist

- [ ] No secrets committed
- [ ] Built wrapper: `bash src/Kapusch.FacebookApisForAndroidComponents/Native/Android/build.sh`
- [ ] Restored pinned AARs: `bash src/Kapusch.FacebookApisForAndroidComponents/Native/Android/restore-facebook-aars.sh`
- [ ] Packed NuGet: `dotnet pack src/Kapusch.FacebookApisForAndroidComponents/Kapusch.FacebookApisForAndroidComponents.csproj -c Release -o artifacts/nuget`
- [ ] Updated docs if behavior/integration changed
