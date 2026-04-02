# Argus - Automated Secret Scanning & SBOM Generation

## 🛡️ About the Project
This project was developed as part of my Graduation Thesis (Graduaatsproef). It addresses two critical challenges in modern DevSecOps: **Credential Leaks** and **Software Supply Chain Security**.

### The Problem
* **Hardcoded Secrets:** Developers accidentally committing API keys, tokens, or passwords to version control.
* **Shadow Dependencies:** Lack of visibility into third-party libraries and their vulnerabilities.

### The Solution
This application provides an automated pipeline to:
1.  **Scan Source Code:** Detect sensitive information using pattern matching and entropy analysis.
2.  **Generate SBOM:** Create a comprehensive Software Bill of Materials (SBOM) to document every component and dependency within the project.

## ✨ Key Features
* **Automated Secret Detection:** Scans for AWS keys, GitHub tokens, database credentials, etc.
* **SBOM Generation:** Supports industry standards like [e.g., CycloneDX or SPDX].
* **Reporting:** Provides a clear overview of security risks and component health.

## 🚀 Getting Started
(Add instructions here on how to run your app)
