# Kapusch.FacebookApisForAndroidComponents — AI Working Agreement

## Goals
- Produce a reproducible Android NuGet package for Facebook Login interop.
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
