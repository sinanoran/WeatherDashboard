# WeatherDashboard

A full-stack weather dashboard built with **.NET 10**, **React 19**, and **.NET Aspire**. It fetches live weather data from the OpenWeatherMap API, caches results in Redis, and serves a Vite-powered React frontend.

## Architecture

```
WeatherDashboard/
├── WeatherDashboard.AppHost/        # .NET Aspire orchestrator
├── WeatherDashboard.Server/         # ASP.NET Core backend
│   ├── Api/Endpoints/               # Minimal API endpoint definitions
│   ├── Application/
│   │   ├── Contracts/               # Request/response DTOs (API surface)
│   │   ├── Interfaces/              # Application service abstractions
│   │   ├── Mapping/                 # Domain-to-contract mappers
│   │   └── Services/                # Application service implementations
│   ├── Domain/
│   │   ├── Interfaces/              # Domain ports (IWeatherProvider, ILocationRepository)
│   │   └── Models/                  # Core domain value objects
│   └── Infrastructure/
│       ├── ErrorHandling/           # Global exception handler (RFC 9457 ProblemDetails)
│       ├── OpenWeatherMap/          # Weather provider + geocoding integration
│       └── Redis/                   # Caching (weather) and persistence (default location)
├── WeatherDashboard.Server.Tests/   # xUnit + NSubstitute integration & unit tests
└── frontend/                        # React 19 + TypeScript + Vite SPA
```

The backend follows a **layered architecture** with clear separation of concerns:

- **Domain** — Pure value objects and port interfaces with no external dependencies.
- **Application** — Orchestrates domain logic, defines contracts, and exposes service interfaces consumed by the API layer.
- **Infrastructure** — Implements domain ports: OpenWeatherMap HTTP integration, Redis caching/persistence, and error handling.
- **Api** — Thin minimal API endpoints that delegate to application services.

This separation keeps the domain and application layers testable in isolation while allowing infrastructure details (cache provider, weather API) to be swapped without touching business logic.

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- [Node.js](https://nodejs.org/) (v18+)
- A container runtime such as [Docker](https://www.docker.com/) or [Podman](https://podman.io/) (required by Aspire for Redis)

## Setup and Installation

### 1. Clone the repository

```bash
git clone https://github.com/sinanoran/WeatherDashboard.git
cd WeatherDashboard
```

### 2. Configure the OpenWeatherMap API key (optional)

The app requires an [OpenWeatherMap API key](https://openweathermap.org/api) for live weather data. If no key is configured, the server returns deterministic fallback data — useful for development and testing.

Store the key in user secrets for the AppHost project:

```bash
cd WeatherDashboard.AppHost
dotnet user-secrets set "OpenWeatherMap:ApiKey" "<your-api-key>"
cd ..
```

### 3. Install frontend dependencies

```bash
cd frontend
npm install
cd ..
```

### 4. Run with .NET Aspire

```bash
dotnet run --project WeatherDashboard.AppHost
```

This starts the full stack:
- **Redis** container (managed by Aspire)
- **Backend** ASP.NET Core server
- **Frontend** Vite dev server (proxied through Aspire)

The Aspire dashboard will open in your browser and show all running resources with their endpoints.

## API Endpoints

All endpoints are served under the `/api` prefix. An OpenAPI document is available at `/openapi/v1.json` in development.

### Weather

#### `GET /api/weather?city={city}`

Returns current weather data for the given city. If `city` is omitted, the default location is used.

**Query Parameters**

| Parameter | Type     | Required | Description                          |
|-----------|----------|----------|--------------------------------------|
| `city`    | `string` | No       | City name (max 100 characters)       |

**Response** `200 OK`

```json
{
  "city": "London",
  "temperatureC": 15.2,
  "temperatureF": 59.4,
  "humidity": 72,
  "windSpeedMps": 3.5,
  "description": "overcast clouds",
  "icon": "04d"
}
```

**Error Responses**

| Status | Condition                  |
|--------|----------------------------|
| `400`  | City name exceeds 100 chars |
| `404`  | City not found              |

---

### Location

#### `GET /api/location/default`

Returns the current default location.

**Response** `200 OK`

```json
{
  "city": "London"
}
```

#### `PUT /api/location/default`

Sets the default location. The city must be valid and resolvable by the weather service.

**Request Body**

```json
{
  "city": "Tokyo"
}
```

**Response** `200 OK`

```json
{
  "city": "Tokyo"
}
```

**Error Responses**

| Status | Condition                  |
|--------|----------------------------|
| `400`  | City name missing or too long |
| `404`  | City not found              |

## Running Tests

### Run all backend tests (server)

From the repository root:

```bash
dotnet test WeatherDashboard.Server.Tests/WeatherDashboard.Server.Tests.csproj
```

This runs the server test project using **xUnit**, **Moq**, and **WebApplicationFactory**.

### Run frontend tests

From the repository root:

```bash
cd frontend
npm test
```

This runs the frontend test suite using **Vitest** and **React Testing Library**.
