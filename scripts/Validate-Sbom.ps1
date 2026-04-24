# ============================================================
# Validate-Sbom.ps1
# Exporteert een CycloneDX SBOM via de Argus API en valideert
# deze met de officiële CycloneDX CLI.
#
# Gebruik:
#   .\scripts\Validate-Sbom.ps1 -ScanId "<guid>"
#
# Vereisten:
#   dotnet tool install --global CycloneDX
# ============================================================

param(
    [Parameter(Mandatory = $true)]
    [string] $ScanId,

    [string] $ApiBaseUrl  = "http://localhost:5245",
    [string] $Email       = "admin@argus.local",
    [string] $Password    = "Admin1234!",
    [string] $OutputDir   = "$PSScriptRoot\sbom-output"
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

# ── 1. Output map aanmaken ────────────────────────────────────────
if (-not (Test-Path $OutputDir)) {
    New-Item -ItemType Directory -Path $OutputDir | Out-Null
}

# ── 2. Inloggen en JWT ophalen ────────────────────────────────────
Write-Host "`n[1/4] Inloggen bij Argus API..." -ForegroundColor Cyan

$loginBody = @{ email = $Email; password = $Password } | ConvertTo-Json
$loginResponse = Invoke-RestMethod `
    -Uri        "$ApiBaseUrl/api/auth/login" `
    -Method     POST `
    -Body       $loginBody `
    -ContentType "application/json"

$token = $loginResponse.token
if ([string]::IsNullOrWhiteSpace($token)) {
    Write-Error "Login mislukt: geen token ontvangen."
}

Write-Host "    Token ontvangen (${($token.Substring(0,20))}...)" -ForegroundColor Green

# ── 3. SBOM exporteren ────────────────────────────────────────────
Write-Host "`n[2/4] SBOM exporteren voor scan $ScanId..." -ForegroundColor Cyan

$sbomPath = Join-Path $OutputDir "sbom-$ScanId.cdx.json"

Invoke-RestMethod `
    -Uri     "$ApiBaseUrl/api/scans/$ScanId/components/export/cyclonedx" `
    -Headers @{ Authorization = "Bearer $token" } `
    -OutFile $sbomPath

Write-Host "    Opgeslagen: $sbomPath" -ForegroundColor Green

# ── 4. Schema validatie met CycloneDX CLI ─────────────────────────
Write-Host "`n[3/4] CycloneDX schema-validatie uitvoeren..." -ForegroundColor Cyan

$cliOutput = dotnet CycloneDX validate --input-file $sbomPath --fail-on-errors 2>&1

if ($LASTEXITCODE -eq 0) {
    Write-Host "`n    SBOM is geldig (schema-conformiteit: OK)" -ForegroundColor Green
} else {
    Write-Host "`n    Validatie MISLUKT:" -ForegroundColor Red
    Write-Host $cliOutput -ForegroundColor Yellow
    exit 1
}

# ── 5. Inhoud samenvatting ────────────────────────────────────────
Write-Host "`n[4/4] SBOM inhoud samenvatting:" -ForegroundColor Cyan

$sbom = Get-Content $sbomPath | ConvertFrom-Json

Write-Host "    bomFormat   : $($sbom.bomFormat)"
Write-Host "    specVersion : $($sbom.specVersion)"
Write-Host "    serialNumber: $($sbom.serialNumber)"
Write-Host "    version     : $($sbom.version)"
Write-Host "    timestamp   : $($sbom.metadata.timestamp)"
Write-Host "    componenten : $($sbom.components.Count)"

if ($sbom.components.Count -gt 0) {
    Write-Host "`n    Eerste 5 componenten:"
    $sbom.components | Select-Object -First 5 | ForEach-Object {
        Write-Host "      - $($_.name) $($_.version)  [$($_.purl)]"
    }
}

Write-Host "`nKlaar! SBOM opgeslagen in: $sbomPath`n" -ForegroundColor Green
