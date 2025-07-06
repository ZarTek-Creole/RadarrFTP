# RadarrFTP v1.0.0 - First Release with FTPS Support

## 🎉 What's New

This is the **first stable release** of RadarrFTP, a fork of Radarr with integrated FTPS (FTP over TLS/SSL) support for secure movie downloading and indexing.

## ✨ Key Features

### 🔒 FTPS Download Client
- **Secure TLS/SSL connections** to FTPS servers
- **Directory-based downloading** with automatic file discovery
- **RAR extraction** using SharpCompress library
- **Intelligent movie detection** and organization
- **Connection pooling** and retry mechanisms

### 🔍 FTPS Indexer
- **Automatic content discovery** on FTPS servers
- **Advanced release name parsing** for quality detection
- **Multi-server support** for redundancy
- **Real-time indexing** of new releases
- **Quality filtering** and preferences

### 🎬 Full Radarr Integration
- **Native integration** with existing Radarr workflow
- **Web UI configuration** for FTPS settings
- **Download queue management** with FTPS sources
- **Quality profiles** and custom formats support
- **Notifications** and webhooks compatibility

### 🖥️ Modern Frontend
- **React 18.3.1** with latest dependencies
- **Responsive design** for desktop and mobile
- **Real-time updates** via SignalR
- **Dark/light theme** support

## 📦 Downloads

### Windows x64 (Recommended)
- **File:** `RadarrFTP-Windows-x64-v1.0.0.zip` (65MB)
- **Self-contained:** No additional installation required
- **Includes:** .NET 6.0 runtime and all dependencies
- **Usage:** Extract and run `Radarr.Console.exe`

### Source Code
- **Build from source** using the provided build scripts
- **Requirements:** .NET 6.0 SDK, Node.js 20+, Yarn
- **Platforms:** Windows, Linux, macOS

## 🚀 Installation

### Windows (Easy)
1. Download `RadarrFTP-Windows-x64-v1.0.0.zip`
2. Extract to your preferred location
3. Run `Radarr.Console.exe`
4. Open http://localhost:7878 in your browser
5. Configure FTPS settings in Settings → Download Clients

### Build from Source
```bash
git clone https://github.com/ZarTek-Creole/RadarrFTP.git
cd RadarrFTP
yarn install
dotnet build src/Radarr.sln --configuration Release
```

## ⚙️ Configuration

### FTPS Download Client Setup
1. Go to **Settings → Download Clients**
2. Click **Add (+)** and select **FTPS**
3. Configure your FTPS server settings:
   - **Host:** Your FTPS server address
   - **Port:** Usually 21 or 990
   - **Username/Password:** Your credentials
   - **Use SSL:** Enable for secure connections
   - **Remote Path:** Base directory for downloads

### FTPS Indexer Setup
1. Go to **Settings → Indexers**
2. Click **Add (+)** and select **FTPS**
3. Configure indexer settings:
   - **Host:** Your FTPS server address
   - **Search Paths:** Directories to scan for releases
   - **Update Interval:** How often to check for new content

## 🔧 Technical Details

### System Requirements
- **Windows:** 10/11 (x64)
- **Memory:** 512MB RAM minimum, 1GB recommended
- **Storage:** 100MB for application, additional for downloads
- **Network:** Internet connection for metadata and FTPS access

### Dependencies Included
- **.NET 6.0 Runtime**
- **FFProbe** for media analysis
- **SQLite** for database
- **SharpCompress** for RAR extraction
- **All required libraries** and frameworks

### Security Features
- **TLS/SSL encryption** for all FTPS communications
- **Certificate validation** with custom CA support
- **Secure credential storage** in encrypted database
- **Connection timeout** and retry protection

## 🐛 Known Issues

- Some antivirus software may flag the executable (false positive)
- First startup may take longer due to database initialization
- FTPS passive mode required for most configurations

## 🤝 Contributing

This project is a fork of the original Radarr with FTPS extensions. Contributions are welcome!

- **Repository:** https://github.com/ZarTek-Creole/RadarrFTP
- **Issues:** Report bugs and feature requests
- **Pull Requests:** Submit improvements and fixes

## 📄 License

RadarrFTP is licensed under GPL-3.0, same as the original Radarr project.

## 🙏 Acknowledgments

- **Radarr Team** for the excellent base application
- **Community contributors** for testing and feedback
- **FTPS server providers** for development access

---

**Enjoy your secure movie downloading with RadarrFTP!** 🎬🔒

