# Snp.CFDIDownload

This repository contains a library that illustrates how to interact with the SAT CFDI mass download service. The code targets **.NET Framework 4.8** and includes a small MSTest project.

## Structure

- `src/Snp.CFDIDownload` – class library implementing `CFDIDownloadService`.
- `test/Snp.CFDIDownload.Tests` – unit tests using `FakeHttpMessageHandler`.
- `Snp.CFDIDownload.sln` – Visual Studio solution file.

## Building

You will need the .NET SDK or Visual Studio with .NET Framework 4.8 installed.
From a developer command prompt run:

```bash
msbuild Snp.CFDIDownload.sln
```

To execute the tests use MSTest or the `dotnet` CLI:

```bash
dotnet test Snp.CFDIDownload.sln
```

These commands require the appropriate tooling installed on your machine.
