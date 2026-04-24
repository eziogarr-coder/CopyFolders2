# CopyFolders2 📂

---

## English 🇬🇧

CopyFolders2 is a lightweight tool built with **Avalonia UI** and **.NET 8** to quickly and easily backup your files on Windows 10/11. Read the [Help](https://psmate.com/Help/CF2/intro.html)

### ✨ Features
- Modern and intuitive graphical interface.
- High performance optimized for .NET 8.
- **Portable**: A single executable file, no installation required. .NET 8 included.

## 🚀 Download
1. Go to the Releases section of this repository [Releases](https://github.com/eziogarr-coder/CopyFolders2/releases/tag/v2.0.1).
2. Download the latest `CopyFolders2.exe` wherever you prefer.
3. Program is portable and does not need installation: just double-click to run!

### 🛡️ Security Note (Antivirus False Positives)
Some antivirus software might flag the executable as `malicious`.
**This is a false positive.**
This happens because the app is published as a "Single File" with compression enabled to keep the size small (~20MB). The full source code is available here for inspection to guarantee transparency and safety. Always test the executable with your antivirus before running it.

### 🛠️ How to build 
 
To generate the same standalone executable, run this command from the project terminal:
```bash
dotnet publish CopyFolders2.csproj -c Release -r win-x64 -p:PublishSingleFile=true -p:SelfContained=true -p:EnableCompressionInSingleFile=true -p:PublishTrimmed=true -p:IncludeNativeLibrariesForSelfExtract=true
```

### 🌐 [Author website: psmate.com](https://psmate.com)

---

## Italiano 🇮🇹

CopyFolders2 è un tool leggero realizzato con **Avalonia UI** e **.NET 8** per effettuare velocemente e facilmente il backup dei tuoi file su Windows 10/11. Leggi l' [Help](https://psmate.com/Help/CF2/intro.html)

### ✨ Caratteristiche
- Interfaccia grafica moderna e intuitiva.
- Alte prestazioni ottimizzate per .NET 8.
- **Portable**: Un unico file eseguibile senza necessità di installazione, le .NET 8 sono incluse.

## 🚀 Download
1. Vai nella sezione [Releases](https://github.com/eziogarr-coder/CopyFolders2/releases/tag/v2.0.1).
2. Scarica il file `CopyFolders2.exe`dove preferisci.
3. Il programma è portable e non richede installazione: avvialo con un doppio clic.

### 🛡️ Nota sulla Sicurezza (Falsi Positivi Antivirus)
Alcuni antivirus potrebbero segnalare l'eseguibile come `malicious`.
**Si tratta di un falso positivo.**
Ciò accade perché l'app è pubblicata in modalità "Single File" con compressione attiva per ridurne il peso (~20MB). Il codice sorgente è interamente disponibile qui per essere verificato a garanzia di trasparenza e sicurezza. Testa sempre l' eseguibile col tuo antivirus prima di lanciarlo.

### 🛠️ Come compilare
  
Per generare lo stesso eseguibile autonomo, lancia questo comando dal terminale del progetto:
```bash
dotnet publish CopyFolders2.csproj -c Release -r win-x64 -p:PublishSingleFile=true -p:SelfContained=true -p:EnableCompressionInSingleFile=true -p:PublishTrimmed=true -p:IncludeNativeLibrariesForSelfExtract=true
```

### 🌐 [Sito web dell' autore: psmate.com](https://psmate.com)

---

## 🛠️ Tech Stack
- **IDE**: JetBrains Rider 2025.3.2
- **Framework**: Avalonia UI
- **Runtime**: .NET 8
