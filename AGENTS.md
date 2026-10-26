# Kapusch.FacebookApisForAndroidComponents — AI Working Agreement

## Goals
- Produce a reproducible Android NuGet package for selectable Facebook Login, Share and App Events interop.
- Do not commit secrets.

## Packaging constraints
- Public OSS repo: keep docs/sample generic and not app-specific.
- The NuGet ships the required `.aar` artifacts and injects them into consuming apps via `buildTransitive` `AndroidAarLibrary`.
- Consuming apps must not download native deps at build time.
- Do not bundle shared dependencies (AndroidX, Kotlin, Play Install Referrer, Google Play services); consumers get them from managed `Xamarin.*` NuGets.
- The default feature selection stays `Login`; new features must not change what existing consumers inject.
- The App Events facade is reached only through JNI: keep `consumer-rules.pro`, `scripts/verify-interop.py` and `FacebookAppEvents.cs` in sync, and regenerate the constants with `scripts/app-event-constants.py --write` after an SDK bump.
- This repo may download dependencies during CI/build, but consuming apps must not.

## Repo layout
- `src/Kapusch.FacebookApisForAndroidComponents/` — NuGet project (managed API + buildTransitive MSBuild)
- `src/Kapusch.FacebookApisForAndroidComponents/Native/Android/` — Gradle wrapper library + scripts (repo-only)
- `scripts/` — deterministic validators (package layout, JNI contract, R8 output, constants, public-tree scan)
- `tests/` — MSBuild feature-selection and warning tests
- `DependencyLocks/` — pinned third-party artifacts (URLs + SHA256)
- `Docs/` — integration docs
- `samples/` — optional sample template (no secrets committed)

## Safety
- Do not add new dependency ingestion paths without documenting them in `README.md`.
- Do not commit real app ids/secrets.
- Keep the tree generic: no application or company names, real Meta app ids, client tokens or emails (`scripts/verify-public.py`).
- When package contents change, update `scripts/validate-package.py` in the same commit.

## Branches and releases
- `master` is the only long-lived branch and the source of every NuGet.org release.
- Implement changes on short-lived branches and target `master` through a PR. Never implement directly on `release/*`.
- Both stable (`vX.Y.Z`) and prerelease (`vX.Y.Z-rc.N`) tags must reference commits reachable from `origin/master`.
- Manual runs without a version publish previews to GitHub Packages. See `Docs/Release.md` for the complete contract.
