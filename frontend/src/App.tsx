import SearchBar from './components/SearchBar'
import WeatherDisplay from './components/WeatherDisplay'
import DefaultLocationSetting from './components/DefaultLocationSetting'
import { useWeather } from './hooks/useWeather'
import './App.css'

function App() {

  const { weather, defaultCity, loading, error, search, updateDefaultLocation, clearError } = useWeather()

  return (
    <div className="app-container">
      <header className="app-header">
        <h1 className="app-title">Weather Dashboard</h1>
      </header>

      <main className="main-content">
        <section className="weather-section">
          <div className="card">
            <SearchBar onSearch={search} disabled={loading} />

            {loading && <p className="status" role="status">Loading...</p>}
            {error && (
              <p className="status error" role="alert">
                {error}
                <button className="dismiss-btn" onClick={clearError} aria-label="Dismiss error">X</button>
              </p>
            )}
            {weather && !loading && <WeatherDisplay weather={weather} />}

            <DefaultLocationSetting
              currentDefault={defaultCity}
              onSave={updateDefaultLocation}
              disabled={loading}
            />
          </div>
        </section>
      </main>

      <footer className="app-footer">
        <nav aria-label="Footer navigation">
          <a href="https://aspire.dev" target="_blank" rel="noopener noreferrer">
            Powered by .NET Aspire
          </a>
        </nav>
      </footer>
    </div>
  )
}

export default App
