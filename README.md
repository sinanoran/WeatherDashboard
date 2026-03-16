# WeatherDashboard

A full-stack weather dashboard built with **.NET 10**, **React 19**, and **.NET Aspire**. It fetches live weather data from the OpenWeatherMap API, caches results in Redis (with an automatic fallback to in-memory cache when Redis is unavailable), and serves a Vite-powered React frontend.

## Key Features

- **Live weather data** via the OpenWeatherMap API with geocoding support
- **Default location** — set a favourite city that loads automatically on startup; a **Use Default** button in the search bar lets you return to it at any time
- **Distributed caching** — Redis when available, in-memory otherwise; configurable TTL via `appsettings.json`
- **Resilient cache layer** — cache failures are logged and fall through transparently to the live provider
- **Fallback weather data** — deterministic dummy data is returned when no API key is configured

## Architecture

```
WeatherDashboard/
├── WeatherDashboard.AppHost/        # .NET Aspire orchestrator
├── WeatherDashboard.Server/         # ASP.NET Core backend
│   ├── Api/Endpoints/               # Minimal API endpoint definitions
│   ├── Application/
│   │   ├── Contracts/               # Request/response DTOs (API surface)
│   │   ├── Interfaces/              # Application service abstractions
│   │   └── Services/                # Application service implementations
│   ├── Domain/
│   │   ├── Interfaces/              # Domain ports (IWeatherProvider, ILocationRepository)
│   │   └── Models/                  # Core domain value objects
│   └── Infrastructure/
│       ├── ErrorHandling/           # Global exception handler (RFC 9457 ProblemDetails)
│       ├── OpenWeatherMap/          # Weather provider + geocoding integration
│       └── Redis/                   # Caching (weather) and persistence (default location)
├── WeatherDashboard.Server.Tests/   # xUnit + Moq unit tests
└── frontend/                        # React 19 + TypeScript + Vite SPA
```

The backend follows a **layered architecture** with clear separation of concerns:

- **Domain** — Pure value objects and port interfaces with no external dependencies.
- **Application** — Orchestrates domain logic, defines contracts, and exposes service interfaces consumed by the API layer.
- **Infrastructure** — Implements domain ports: OpenWeatherMap HTTP integration, distributed caching/persistence (Redis when available, in-memory otherwise), and error handling.
- **Api** — Thin minimal API endpoints that delegate to application services.

This separation keeps the domain and application layers testable in isolation while allowing infrastructure details (cache provider, weather API) to be swapped without touching business logic.

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- [Node.js](https://nodejs.org/) (v18+)
- A container runtime such as [Docker](https://www.docker.com/) or [Podman](https://podman.io/) (optional — used by Aspire to run the Redis container; the server falls back to an in-memory cache automatically when no Redis connection string is present)

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

### 3. Configure the cache TTL (optional)

Weather responses are cached for **10 minutes** by default. Adjust the value in `WeatherDashboard.Server/appsettings.json` if needed:

```json
{
  "WeatherCache": {
    "ExpirationMinutes": 10
  }
}
```

### 4. Install frontend dependencies

```bash
cd frontend
npm install
cd ..
```

### 5. Run with .NET Aspire

```bash
dotnet run --project WeatherDashboard.AppHost
```

This starts the full stack:
- **Redis** container (managed by Aspire)
- **Backend** ASP.NET Core server
- **Frontend** Vite dev server (proxied through Aspire)

The Aspire dashboard will open in your browser and show all running resources with their endpoints.

> **No container runtime?** You can still run the backend on its own. Redis will be unavailable and the server will switch to an in-memory cache automatically:
>
> ```bash
> dotnet run --project WeatherDashboard.Server
> ```

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
