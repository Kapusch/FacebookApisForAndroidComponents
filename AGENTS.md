# Kapusch.FacebookApisForAndroidComponents — AI Working Agreement

## Goals
- Produce a reproducible Android NuGet package for selectable Facebook Login and Share interop.
- Do not commit secrets.

## Packaging constraints
- Public OSS repo: keep docs/sample generic and not app-specific.
- The NuGet ships the required `.aar` artifacts and injects them into consuming apps via `buildTransitive` `AndroidAarLibrary`.
- Consuming apps must not download native deps at build time.
- This repo may download dependencies during CI/build, but consuming apps must not.

## Repo layout
- `src/Kapusch.FacebookApisForAndroidComponents/` — NuGet project (managed API + buildTransitive MSBuild)
- `src/Kapusch.FacebookApisForAndroidComponents/Native/Android/` — Gradle wrapper library + scripts (repo-only)
- `DependencyLocks/` — pinned third-party artifacts (URLs + SHA256)
- `Docs/` — integration docs
- `samples/` — optional sample template (no secrets committed)

## Safety
- Do not add new dependency ingestion paths without documenting them in `README.md`.
- Do not commit real app ids/secrets.

## Branches and releases
- `master` is the only long-lived branch and the source of every NuGet.org release.
- Implement changes on short-lived branches and target `master` through a PR. Never implement directly on `release/*`.
- Both stable (`vX.Y.Z`) and prerelease (`vX.Y.Z-rc.N`) tags must reference commits reachable from `origin/master`.
- Manual runs without a version publish previews to GitHub Packages. See `Docs/Release.md` for the complete contract.
