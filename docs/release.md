# Release

- **Merge to `main`** — packages publish to GitHub Packages (`merge.yml`).
- **GitHub Release published** — same version as the tag is packed, pushed to **nuget.org**, and `.nupkg` files are attached to the release (`release.yml` → `dotnet-release-publish.yml`).

Configure org/repo secret **`NUGET_API_KEY`** before the first release.

Register versions in `novolis-registry` after the first successful nuget.org publish.
