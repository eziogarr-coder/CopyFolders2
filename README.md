# CopyFolders2 📂

---

## English 🇬🇧

CopyFolders2 is a lightweight tool built with **Avalonia UI** and **.NET 8** to simplify folder management and copying on Windows.

### ✨ Features
- Modern and intuitive graphical interface.
- High performance optimized for .NET 8.
- **Portable**: A single executable file, no installation required. .NET 8 included.

## 🚀 Installation
1. Go to the Releases section of this repository [Releases](https://github.com/eziogarr-coder/CopyFolders2/releases/tag/v2.0.1).
2. Download the latest `CopyFolders2.exe`.
3. No installation needed: just double-click to run!

### 🛡️ Security Note (Antivirus False Positives)
Some antivirus software (like Symantec) might flag the executable as `ML.Attribute.HighConfidence`.
**This is a false positive.**
This happens because the app is published as a "Single File" with compression enabled to keep the size small (~20MB). The full source code is available here for inspection to guarantee transparency and safety.

### 🛠️ How to build 
 
To generate the same standalone executable, run this command from the project terminal:
```bash
dotnet publish CopyFolders2.csproj -c Release -r win-x64 -p:PublishSingleFile=true -p:SelfContained=true -p:EnableCompressionInSingleFile=true -p:PublishTrimmed=true -p:IncludeNativeLibrariesForSelfExtract=true
```

---

## Italiano 🇮🇹

CopyFolders2 è un tool leggero realizzato con **Avalonia UI** e **.NET 8** per semplificare la gestione e la copia di cartelle su Windows.

### ✨ Caratteristiche
- Interfaccia grafica moderna e intuitiva.
- Alte prestazioni ottimizzate per .NET 8.
- **Portable**: Un unico file eseguibile senza necessità di installazione, le .NET 8 sono incluse.

## 🚀 Installazione
1. Vai nella sezione [Releases](https://github.com/eziogarr-coder/CopyFolders2/releases/tag/v2.0.1).
2. Scarica il file `CopyFolders2.exe`.
3. Avvialo con un doppio clic.

### 🛡️ Nota sulla Sicurezza (Falsi Positivi Antivirus)
Alcuni antivirus (come Symantec) potrebbero segnalare l'eseguibile come `ML.Attribute.HighConfidence`.
**Si tratta di un falso positivo.**
Ciò accade perché l'app è pubblicata in modalità "Single File" con compressione attiva per ridurne il peso (~20MB). Il codice sorgente è interamente disponibile qui per essere verificato a garanzia di trasparenza e sicurezza.

### 🛠️ Come compilare
  
Per generare lo stesso eseguibile autonomo, lancia questo comando dal terminale del progetto:
```bash
dotnet publish CopyFolders2.csproj -c Release -r win-x64 -p:PublishSingleFile=true -p:SelfContained=true -p:EnableCompressionInSingleFile=true -p:PublishTrimmed=true -p:IncludeNativeLibrariesForSelfExtract=true
```

---

## 🛠️ Tech Stack
- **IDE**: JetBrains Rider
- **Framework**: Avalonia UI
- **Runtime**: .NET 8
