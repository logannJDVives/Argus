# Argus — Secret Scanner & SBOM Generator

Argus is a .NET application that scans your project's source code for hardcoded secrets and generates a Software Bill of Materials (SBOM) from NuGet dependencies. Developed as a Graduation Thesis (Graduaatsproef) addressing two critical DevSecOps challenges: **credential leaks** and **software supply chain security**.

---

## Table of Contents

- [Features](#features)
- [Architecture](#architecture)
- [Prerequisites](#prerequisites)
- [Getting Started](#getting-started)
  - [1. Clone the repository](#1-clone-the-repository)
  - [2. Configure the database](#2-configure-the-database)
  - [3. Configure JWT](#3-configure-jwt)
  - [4. Run the API](#4-run-the-api)
  - [5. Run the frontend](#5-run-the-frontend)
- [Using the Application](#using-the-application)
  - [Register & Login](#register--login)
  - [Upload a Project](#upload-a-project)
  - [Start a Scan](#start-a-scan)
  - [View Scan Results](#view-scan-results)
  - [Review Secrets](#review-secrets)
  - [Export SBOM](#export-sbom)
- [Secret Detection](#secret-detection)
- [SDK Usage](#sdk-usage)
- [Project Structure](#project-structure)

---

## Features

- 🔍 **Secret detection** — regex-based and entropy-based detection of hardcoded secrets
- 📦 **SBOM generation** — parses `.csproj` files and enriches components with live NuGet metadata
- 🔐 **JWT authentication** — secure user accounts with register/login
- 📤 **ZIP upload** — upload any .NET project as a `.zip` archive (max 200 MB)
- 📋 **CycloneDX export** — export your SBOM in the industry-standard CycloneDX format
- 🖥️ **.NET MAUI frontend** — cross-platform UI built with Blazor Hybrid

---

## Architecture

```
Argus.Api          → ASP.NET Core REST API
Argus.FrontEnd     → .NET MAUI Blazor Hybrid frontend
Argus.Services     → Business logic (scanning, detection, enrichment)
Argus.Data         → Entity Framework Core + SQL Server
Argus.Entities     → Domain models
Argus.Dto          → Data transfer objects
Argus.Interfaces   → Abstractions / interfaces
Argus.Sdk          → Reusable HTTP client SDK
Argus.TestConsole  → Console test runner
```

---

## Prerequisites

| Requirement | Version |
|---|---|
| .NET SDK | 10.0 or higher |
| SQL Server / LocalDB | Any recent version |
| Visual Studio | 2022 or higher (MAUI workload required) |

---

## Getting Started

### 1. Clone the repository

```bash
git clone https://github.com/logannJDVives/Argus.git
cd Argus
```

### 2. Configure the database

Open `Argus.Api/appsettings.json` and update the connection string if needed:

```json
"ConnectionStrings": {
  "DefaultConnection": "Server=(localdb)\\MSSQLLocalDB;Database=ArgusDb;Trusted_Connection=True;"
}
```

Then apply the Entity Framework migrations to create the database:

```bash
cd Argus.Api
dotnet ef database update
```

### 3. Configure JWT

In `Argus.Api/appsettings.json`, replace the JWT key with a strong secret of at least 32 characters:

```json
"Jwt": {
  "Key": "your-strong-secret-key-at-least-32-characters!",
  "Issuer": "ArgusApi",
  "Audience": "ArgusFrontEnd"
}
```

> ⚠️ **Never commit a real JWT key to source control.**

### 4. Run the API

```bash
cd Argus.Api
dotnet run
```

The API will be available at `https://localhost:7001`.
Swagger UI is available at `https://localhost:7001` (root) in development mode.

### 5. Run the frontend

Open the solution in Visual Studio, set `Argus.FrontEnd` as the startup project, and press **F5**.

---

## Using the Application

### Register & Login

1. Open the Argus frontend application.
2. Navigate to **Register** and create an account with your email and a password (min. 8 characters, at least 1 digit).
3. Navigate to **Login** and sign in with your credentials.
4. Your session is persisted — you will remain logged in between app restarts.

---

### Upload a Project

1. Navigate to **Upload project** in the sidebar.
2. Enter a **project name**.
3. Click the upload area and select a `.zip` archive of your project (max 200 MB).
4. Click **Upload**.

> **Tip:** Zip the root folder of your .NET solution. Argus will scan all `.cs`, `.json`, `.config`, `.env`, `.yaml`, `.yml` and `.xml` files inside.

---

### Start a Scan

1. Navigate to **Projects** in the sidebar.
2. Click on your uploaded project.
3. Click **Start scan**.
4. The scan runs in the background. The status will update: **In Progress** → **Completed** (or **Failed**).

---

### View Scan Results

After a scan completes, click the scan entry in **Scan History** to open the results page.

#### Secrets tab
A paginated list of all detected secrets with:
- **File path** and **line number**
- **Severity** — `Critical`, `High`, `Medium`, or `Low`
- **Confidence** — `High`, `Medium`, or `Low`
- **Rule ID** — which detection rule triggered (e.g. `OPENAI_API_KEY`, `HIGH_ENTROPY`)
- **Masked value** — the secret value is always masked for safety

#### SBOM tab
A list of all NuGet packages found across `.csproj` files, enriched with:
- License
- Description
- Homepage
- Publisher & published date

---

### Review Secrets

On the **Secrets** tab you can mark each finding as:
- ✅ **Reviewed** — you have acknowledged this finding
- 🚫 **False positive** — this is not an actual secret

---

### Export SBOM

On the **SBOM** tab, click **Export CycloneDX** to download the SBOM as a CycloneDX-compliant JSON file for use with third-party security tools.

---

## Secret Detection

Argus uses two detection engines that run in parallel on every scan:

### 1. Regex Detector
Pattern-based detection for well-known secret formats:

| Rule ID | Description | Severity |
|---|---|---|
| `AWS_ACCESS_KEY` | AWS access key (`AKIA...`) | Critical |
| `AWS_SECRET_KEY` | AWS secret access key | Critical |
| `GITHUB_TOKEN` | GitHub personal access token (`ghp_`, `ghs_`) | Critical |
| `AZURE_KEY` | Azure storage account / shared access key | Critical |
| `OPENAI_API_KEY` | OpenAI API key (`sk-...`) | Critical |
| `STRIPE_KEY` | Stripe live secret key (`sk_live_...`) | Critical |
| `PRIVATE_KEY` | RSA / EC / OpenSSH private key header | Critical |
| `GENERIC_API_KEY` | Generic API key assignment | High |
| `CONNECTION_STRING` | Password in a connection string | High |
| `GENERIC_SECRET` | Generic secret / token / password assignment | Medium |

### 2. Entropy Detector
Detects high-entropy strings that are likely randomly generated secrets, even if they don't match a known pattern. This also covers secrets found inside **code comments** (`//`, `#`).

### Heuristic Filter
After detection, findings are filtered to reduce false positives:
- Known placeholder values (`changeme`, `your-key-here`, etc.) are excluded
- URLs and file paths are excluded
- Regex findings on comment lines are excluded (entropy findings in comments are **kept**)
- Findings where the value matches the variable name are excluded

---

## SDK Usage

Argus ships a reusable SDK (`Argus.Sdk`) for programmatic access from any .NET application:

```csharp
var options = new ArgusApiClientOptions("https://localhost:7001");
using var httpClient = new HttpClient();
var client = new ArgusApiClient(httpClient, options);

// Authenticate
var auth = await client.LoginAsync(new LoginDto { Email = "user@example.com", Password = "password" });
client.SetToken(auth.Token);

// Upload a project
var zipBytes = await File.ReadAllBytesAsync("myproject.zip");
var projectId = await client.UploadProjectAsync("MyProject", zipBytes, "myproject.zip");

// Start a scan
var scan = await client.StartScanAsync(projectId);

// Get results
var secrets = await client.GetSecretsAsync(scan.Id);
```

---

## Project Structure

```
Argus/
├── Argus.Api/                  # REST API + controllers
│   ├── Controllers/
│   ├── appsettings.json
│   └── Program.cs
├── Argus.FrontEnd/             # .NET MAUI Blazor Hybrid frontend
│   ├── Components/Pages/
│   ├── Services/
│   └── MauiProgram.cs
├── Argus.Services/             # Business logic
│   ├── Detection/              # RegexDetector, EntropyDetector, HeuristicFilter
│   ├── Parsing/                # CsprojPackageParser
│   ├── ScanService.cs
│   ├── ProjectUploadService.cs
│   └── NuGetEnricher.cs
├── Argus.Data/                 # EF Core DbContext + migrations
├── Argus.Entities/             # Domain entities
├── Argus.Dto/                  # DTOs for API communication
├── Argus.Interfaces/           # Service interfaces
├── Argus.Sdk/                  # Reusable API client SDK
└── Argus.TestConsole/          # Console test runner + FakeSecretsProject
```
