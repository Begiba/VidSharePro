# VidSharePro 🎥

**VidSharePro** is an enterprise-grade video sharing and processing platform built with .NET 8, following Clean Architecture and Domain-Driven Design (DDD) principles. It handles secure video uploads, background FFmpeg processing, and high-performance streaming.

## 🚀 Key Features

* **Secure Streaming**: Implements JWT-authorized video streaming with support for large files (89MB+) using HTTP Range Requests (Byte-streaming).
* **Background Processing**: A dedicated worker service manages video validation and thumbnail generation using FFmpeg.
* **Encapsulated Domain**: Strictly follows DDD patterns with private setters and state-controlled transitions (e.g., `StartProcessing()`, `TransitionToReady()`).
* **Responsive UI**: A modern, CSP-compliant frontend built with jQuery and CSS3, featuring real-time upload progress.

## 🏗️ Architecture

The solution is divided into four main layers:

1. **Domain**: Contains Entities (Video, User), Enums (VideoStatus), and Domain Logic.
2. **Application**: Contains Interfaces, DTOs, and Business Services (VideoService).
3. **Infrastructure**: Handles Data Persistence (EF Core), Background Jobs, and File Storage.
4. **Web API**: RESTful endpoints and Content Security Policy (CSP) management.

## 🛠️ Getting Started

### Prerequisites

* [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
* [FFmpeg](https://ffmpeg.org/download.html) (Must be added to your system PATH)
* SQL Server (LocalDB or Express)

### Installation

1. **Clone the repo**:
```bash
git clone https://github.com/your-repo/VidSharePro.git
cd VidSharePro

```


2. **Update Configuration**:
In `src/VidSharePro.API/appsettings.json`, set your storage path:
```json
"StorageOptions": {
  "Path": "D:\\VidSharePro\\Storage"
}

```


3. **Apply Migrations**:
```bash
dotnet ef database update --project src/VidSharePro.Infrastructure --startup-project src/VidSharePro.API

```


4. **Run the Project**:
```bash
dotnet run --project src/VidSharePro.API

```



## 🧪 Testing

The project includes a comprehensive unit test suite focusing on domain invariants and service orchestration.

```bash
dotnet test tests/VidSharePro.Tests

```

* **Domain Tests**: Verify video state transitions (e.g., preventing a `Failed` video from being marked `Ready`).
* **Application Tests**: Mock repositories to verify service logic without database dependencies.

## 🔒 Security & Performance

* **CSP Compliance**: The frontend is strictly configured to block inline scripts, mitigating XSS risks.
* **Memory Efficiency**: Large videos are streamed using `PhysicalFile` to ensure low memory overhead on the server.
* **Atomic State**: Database and file system sync is managed via background jobs to prevent partial failures during upload.

