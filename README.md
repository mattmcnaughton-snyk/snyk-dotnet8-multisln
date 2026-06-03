# snyk-dotnet8-multisln

A deliberately vulnerable **.NET 8** sample repo for exercising Snyk, built around
**two independent solution files** so you can debug how Snyk behaves with multiple
`.sln` files in one repository.

> ⚠️ **Intentionally insecure.** Every project here ships known-vulnerable NuGet
> packages and insecure source code on purpose. Do not copy any of it into real software.

## Why this repo exists

1. **Multiple `.sln` files.** `Alpha.sln` and `Beta.sln` sit at the repo root and share
   **no** projects. This is the setup for debugging how `snyk test --all-projects` versus
   `snyk test --file=<solution>.sln` resolve and scope projects.
2. **Build-before-scan is enforced by Snyk itself.** Snyk Open Source for .NET reads each
   project's `obj/project.assets.json`, which only exists after a restore/build. Scan before
   you build and Snyk finds nothing to test (see below) — so a `dotnet build` is required to
   validate Open Source findings.
3. **Findings to validate.** Each project carries distinct vulnerable dependencies (Snyk
   Open Source / SCA) and `Alpha.App` / `Beta.App` contain insecure code (Snyk Code / SAST).

## Layout

```
snyk-dotnet8-multisln/
├── Alpha.sln                     # references ONLY the alpha/* projects
├── Beta.sln                      # references ONLY the beta/* projects
├── alpha/
│   ├── Alpha.App/                # console; SQLi, weak hash, hardcoded secrets
│   │   └── Microsoft.Data.SqlClient 1.0.19239.1
│   └── Alpha.Lib/                # classlib referenced by Alpha.App
│       └── Newtonsoft.Json 9.0.1
└── beta/
    ├── Beta.App/                 # console; command injection, path traversal, XXE
    │   └── log4net 2.0.3
    └── Beta.Lib/                 # classlib referenced by Beta.App
        └── RestSharp 106.6.7
```

The two solutions are **fully independent** — no project belongs to both.

## Requirements

- **Snyk CLI** (tested with `1.1303.1`), authenticated (`snyk auth`).
- A **.NET SDK that can target `net8.0`.** This repo was scaffolded and built with the
  **.NET 10 SDK** — building `net8.0` works because the SDK restores the net8 reference
  packs from NuGet. There is intentionally **no `global.json`** (pinning to a missing 8.0
  SDK would break the build).
- ⚠️ **Build-only on a net10-only machine.** Without the .NET 8 *runtime* installed you can
  `dotnet build` and scan, but `dotnet run` will fail. That's fine — Snyk only needs the
  build output. To actually run the apps, install the .NET 8 runtime or add
  `<RollForward>Major</RollForward>` to the app csproj files.

## The build-before-scan requirement (Open Source / SCA)

Run a scan **before** building and Snyk has no restored assets to read:

```console
$ snyk test --all-projects        # no dotnet build yet

 FATAL   No supported files found (SNYK-CLI-0008)
         Could not detect supported target files in <repo>.
```

Build first, then the same command works:

```bash
dotnet build Alpha.sln
dotnet build Beta.sln
# or restore-only: dotnet restore Alpha.sln && dotnet restore Beta.sln
```

`dotnet build` writes `obj/project.assets.json` under each project — that's the file Snyk
reads. It is git-ignored (build artifacts), so a fresh clone always requires a build first.

> Snyk **Code** (SAST, `snyk code test`) scans source directly and does **not** require a build.

## Commands to try

| Command | Scope | Result on this repo |
|---|---|---|
| `snyk test --all-projects` | every restored `.csproj` in the tree | **4 projects** (Alpha.App, Alpha.Lib, Beta.App, Beta.Lib) |
| `snyk test --file=Alpha.sln` | the Alpha solution only | **2 projects** (Alpha.App, Alpha.Lib) |
| `snyk test --file=Beta.sln` | the Beta solution only | **2 projects** (Beta.App, Beta.Lib) |
| `snyk code test` | all source (no build needed) | **5 SAST issues** |

### Multiple `.sln` behavior (the thing to debug)

- `snyk test --all-projects` **ignores `.sln` grouping** and auto-detects *every* restored
  `.csproj` as its own project. It does **not** pick a single solution. With two root-level
  `.sln` files it reports all four projects.
- `snyk test --file=<solution>.sln` uses that solution as the entry point and scopes the
  scan to the projects it references.
- Because Alpha and Beta share no projects, there is **no dedup overlap** between them — a
  clean baseline. (To reproduce *duplicate* findings across solutions, add a shared project
  to both `.sln` files.)

## Expected findings (validated)

### Snyk Open Source — `snyk test` (7 issues, build required)

| Project | Package | Issue (severity) | Snyk ID |
|---|---|---|---|
| Alpha.App | `Microsoft.Data.SqlClient@1.0.19239.1` | Unprotected Storage of Credentials (High) | SNYK-DOTNET-MICROSOFTDATASQLCLIENT-6149434 |
| Alpha.App | `Microsoft.Data.SqlClient@1.0.19239.1` | Information Exposure (Medium) | SNYK-DOTNET-MICROSOFTDATASQLCLIENT-3110423 |
| Alpha.App | `System.Text.RegularExpressions@4.3.0` (transitive) | ReDoS (High) | SNYK-DOTNET-SYSTEMTEXTREGULAREXPRESSIONS-174708 |
| Alpha.Lib | `Newtonsoft.Json@9.0.1` | Insecure Defaults (High) | SNYK-DOTNET-NEWTONSOFTJSON-2774678 |
| Beta.App | `log4net@2.0.3` | XXE Injection (High) | SNYK-DOTNET-LOG4NET-568897 |
| Beta.App | `log4net@2.0.3` | Improper Encoding/Escaping of Output (Medium) | SNYK-DOTNET-LOG4NET-16032174 |
| Beta.Lib | `RestSharp@106.6.7` | ReDoS (Medium) | SNYK-DOTNET-RESTSHARP-1316436 |

`dotnet build` also surfaces these via NuGet's own audit (NU1902/NU1903/NU1904).

### Snyk Code — `snyk code test` (5 issues, no build)

| Issue (severity) | Location |
|---|---|
| SQL Injection (Medium) | `alpha/Alpha.App/Program.cs` — `LookupUser` |
| Use of Hardcoded Credentials (Low) ×2 | `alpha/Alpha.App/Program.cs` — connection string + AWS token |
| Use of Password Hash With Insufficient Computational Effort (Low) | `alpha/Alpha.App/Program.cs` — `HashPassword` (MD5) |
| XML External Entity (XXE) Injection (High) | `beta/Beta.App/Program.cs` — `ParseDocument` |

### Sinks NOT flagged by the default Snyk Code ruleset

`beta/Beta.App/Program.cs` also contains two insecure sinks that the **default** C# ruleset
does **not** report (the taint source is identical to the XXE finding that *is* reported):

- **OS Command Injection (CWE-78)** — `RunReport` (`Process.StartInfo.Arguments` from input)
- **Path Traversal (CWE-23)** — `ReadConfig` (untrusted input concatenated into a file path)

These are intentionally left in place as candidates for **custom rule extensions / Impact
Testing** — i.e. cases where a custom rule should catch what the stock ruleset misses.

## Notes

- This is its own standalone git repository (it happens to live under `~/projects`, alongside
  other independent sample repos).
- No CI, no remote — add `git remote add origin <url>` if you want to push it somewhere.
