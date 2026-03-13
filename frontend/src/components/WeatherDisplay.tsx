import type { WeatherData } from '../types'

interface WeatherDisplayProps {
  weather: WeatherData
}

export default function WeatherDisplay({ weather }: WeatherDisplayProps) {
  return (
    <div className="weather-display">
      <h2>{weather.city}</h2>
      <img
        src={weather.icon}
        alt={weather.description}
        className="weather-icon"
      />
      <p className="description">{weather.description}</p>
      <div className="weather-details">
        <div className="detail-card">
          <span className="label">Temperature</span>
          <span className="value">
            {weather.temperatureC}°C / {weather.temperatureF}°F
          </span>
        </div>
        <div className="detail-card">
          <span className="label">Humidity</span>
          <span className="value">{weather.humidity}%</span>
        </div>
        <div className="detail-card">
          <span className="label">Wind Speed</span>
          <span className="value">{weather.windSpeedMps} m/s</span>
        </div>
      </div>
    </div>
  )
}
