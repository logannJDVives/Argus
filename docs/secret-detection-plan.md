# Secret Detection — Plan van Aanpak

## Architectuur overzicht

```
ScanService.StartScanAsync()
  └── ProjectFileScannerService.ScanProjectAsync()        ← al klaar ✅
        └── List<ScannedFile>
              ├── RegexDetector.DetectAsync(file)          ← Stap 3
              ├── EntropyDetector.DetectAsync(file)        ← Stap 4
              └── alle findings
                    └── HeuristicFilter.FilterAsync()      ← Stap 5
                          └── DetectedSecret entities → DB
```

---

## Stap 1 — Intern model: `SecretFinding`

> Tussenmodel dat detectors opleveren, vóórdat het een `DetectedSecret` entity wordt.

**Locatie:** `Argus.Interfaces\Models\SecretFinding.cs`

| Property        | Type           | Doel                                                        |
| --------------- | -------------- | ----------------------------------------------------------- |
| `FilePath`      | `string`       | Relatief pad van het bestand                                |
| `LineNumber`    | `int`          | Regelnummer waar de match zit                               |
| `MatchedValue`  | `string`       | De gevonden waarde (voor masking later)                     |
| `RuleId`        | `string`       | Welke regel triggerde (bv. `"AWS_ACCESS_KEY"`, `"HIGH_ENTROPY"`) |
| `DetectorType`  | `DetectorType` | `Regex` of `Entropy`                                       |
| `Severity`      | `Severity`     | Afgeleid van de regel                                       |
| `Confidence`    | `Confidence`   | Hoe zeker we zijn                                           |

Hierbij ook een `DetectorType` enum aanmaken in dezelfde map:

```csharp
public enum DetectorType
{
    Regex,
    Entropy
}
```

---

## Stap 2 — Interface: `ISecretDetector`

**Locatie:** `Argus.Interfaces\ISecretDetector.cs`

```csharp
Task<IReadOnlyList<SecretFinding>> DetectAsync(ScannedFile file)
```

Eén interface, twee implementaties (`RegexDetector` en `EntropyDetector`).  
`ScanService` injecteert `IEnumerable<ISecretDetector>` en loopt over beide.

---

## Stap 3 — `RegexDetector`

**Locatie:** `Argus.Services\Detection\RegexDetector.cs`

### Werking

1. Lees het bestand regel voor regel (`File.ReadLinesAsync`)
2. Per regel: test tegen een dictionary van `RuleId → Regex`
3. Bij match → maak `SecretFinding` aan

### Regex patronen (starterset)

| RuleId              | Patroon                                                         | Severity |
| ------------------- | --------------------------------------------------------------- | -------- |
| `AWS_ACCESS_KEY`    | `AKIA[0-9A-Z]{16}`                                             | Critical |
| `AWS_SECRET_KEY`    | `(?i)aws_secret_access_key\s*=\s*\S+`                          | Critical |
| `GENERIC_API_KEY`   | `(?i)(api[_-]?key\|apikey)\s*[:=]\s*["']?\S{16,}`              | High     |
| `CONNECTION_STRING` | `(?i)(password\|pwd)\s*=\s*[^;]{4,}`                           | High     |
| `PRIVATE_KEY`       | `-----BEGIN (RSA\|EC\|OPENSSH) PRIVATE KEY-----`               | Critical |
| `GENERIC_SECRET`    | `(?i)(secret\|token\|password)\s*[:=]\s*["']?\S{8,}`           | Medium   |
| `GITHUB_TOKEN`      | `gh[ps]_[A-Za-z0-9_]{36,}`                                     | Critical |
| `AZURE_KEY`         | `(?i)(AccountKey\|SharedAccessKey)\s*=\s*[A-Za-z0-9+/=]{20,}`  | Critical |

---

## Stap 4 — `EntropyDetector` (Shannon)

**Locatie:** `Argus.Services\Detection\EntropyDetector.cs`

### Werking

1. Lees het bestand regel voor regel
2. Extraheer **string-literals** en **waarden na `=` of `:`** met een simpele regex
3. Bereken Shannon entropy per gevonden token:

```
H(X) = -Σ p(x) × log₂(p(x))
```

4. **Drempelwaarden:**
   - Hex-strings (0-9, a-f): entropy > **3.0** → verdacht
   - Base64/mixed (alle printable chars): entropy > **4.5** → verdacht
   - Minimale tokenlengte: **16 karakters** (korter = te veel ruis)
5. Bij trigger → `SecretFinding` met `RuleId = "HIGH_ENTROPY"`, `Confidence = Medium`

### Shannon Entropy formule uitleg

Per token (string) tel je hoe vaak elk karakter voorkomt:

```
Voorbeeld: "aabbcc"
  a → 2/6, b → 2/6, c → 2/6
  H = -(3 × (2/6 × log₂(2/6))) = 1.585 bits → laag (niet random)

Voorbeeld: "k8Qz!mX3pL"
  elk karakter uniek → H ≈ 3.32 bits → hoog (mogelijk secret)
```

Hoe **hoger** de entropy, hoe **willekeuriger** de string, hoe groter de kans dat het een secret is.

---

## Stap 5 — `HeuristicFilter` (false positive reductie)

**Locatie:** `Argus.Services\Detection\HeuristicFilter.cs`

### Filtert findings eruit als:

- Waarde is een bekende placeholder: `TODO`, `xxx`, `changeme`, `your-key-here`, `<placeholder>`
- Waarde is een pad of URL zonder geheime data
- Bestand is in een test-map (`**/test/**`, `**/tests/**`, `**/*.Test.cs`)
- Waarde is identiek aan de variabelenaam (bv. `password = "password"`)
- De match zit in een commentaarregel (`//`, `/* */`, `#`)

### Verhoogt Confidence als:

- Bestand is `.env` of `.config` → `Confidence = High`
- Variabelenaam bevat `secret`, `key`, `token`, `password` → `Confidence += 1`

---

## Stap 6 — Integratie in `ScanService`

De huidige flow:

```
files = scanner.ScanProjectAsync()  →  sla FilesScanned op  →  klaar
```

Wordt:

```
files = scanner.ScanProjectAsync()
    ↓
per file → RegexDetector.DetectAsync(file)
per file → EntropyDetector.DetectAsync(file)
    ↓
alle findings → HeuristicFilter.FilterAsync(findings)
    ↓
findings → map naar DetectedSecret entities → SaveChangesAsync
    ↓
scanRun.SecretCount = secrets.Count
```

### Mapping `SecretFinding` → `DetectedSecret`

| SecretFinding    | DetectedSecret     | Transformatie                              |
| ---------------- | ------------------ | ------------------------------------------ |
| `FilePath`       | `FilePath`         | Direct                                     |
| `LineNumber`     | `LineNumber`       | Direct                                     |
| `MatchedValue`   | `MaskedValue`      | Mask: toon eerste 4 + laatste 2 karakters  |
| `MatchedValue`   | `Hash`             | SHA-256 hash van de originele waarde       |
| `RuleId`         | `RuleId`           | Direct                                     |
| `DetectorType`   | `Type`             | `.ToString()`                              |
| `Severity`       | `Severity`         | Direct (zelfde enum)                       |
| `Confidence`     | `Confidence`       | Direct (zelfde enum, na heuristic adjust)  |

---

## Stap 7 — DI registratie

In `Program.cs`:

```csharp
builder.Services.AddScoped<ISecretDetector, RegexDetector>();
builder.Services.AddScoped<ISecretDetector, EntropyDetector>();
builder.Services.AddScoped<HeuristicFilter>();
```

`ScanService` krijgt `IEnumerable<ISecretDetector>` geïnjecteerd — zo loop je automatisch over beide detectors.

---

## Volgorde van implementatie

| Stap  | Wat                                       | Waar                           |
| ----- | ----------------------------------------- | ------------------------------ |
| **1** | `SecretFinding` model + `DetectorType` enum | `Argus.Interfaces\Models\`     |
| **2** | `ISecretDetector` interface               | `Argus.Interfaces\`            |
| **3** | `RegexDetector`                           | `Argus.Services\Detection\`    |
| **4** | `EntropyDetector` (Shannon)               | `Argus.Services\Detection\`    |
| **5** | `HeuristicFilter`                         | `Argus.Services\Detection\`    |
| **6** | `ScanService` aanpassen                   | `Argus.Services\ScanService.cs`|
| **7** | DI registratie                            | `Argus.Api\Program.cs`         |
| **8** | Testen via Swagger                        | `POST /api/projects/{id}/scans`|

---

## Bestaande entities die we gebruiken

### `DetectedSecret` (Argus\DetectedSecret.cs)

```csharp
public class DetectedSecret
{
    public Guid Id { get; set; }
    public string Type { get; set; }          // "Regex" of "Entropy"
    public string FilePath { get; set; }
    public int LineNumber { get; set; }
    public string MaskedValue { get; set; }   // "AKIA****AB"
    public string Hash { get; set; }          // SHA-256
    public Severity Severity { get; set; }
    public string RuleId { get; set; }        // "AWS_ACCESS_KEY", "HIGH_ENTROPY"
    public Confidence Confidence { get; set; }
    public bool IsFalsePositive { get; set; }
    public bool IsReviewed { get; set; }
    public Guid ScanRunId { get; set; }
    public ScanRun ScanRun { get; set; }
}
```

### `Severity` enum (Argus\Severity.cs)

```
Low = 0, Medium = 1, High = 2, Critical = 3
```

### `Confidence` enum (Argus\Confidence.cs)

```
Low = 0, Medium = 1, High = 2
```
