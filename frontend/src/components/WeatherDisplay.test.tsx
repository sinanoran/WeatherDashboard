import { render, screen } from '@testing-library/react'
import { describe, it, expect } from 'vitest'
import WeatherDisplay from './WeatherDisplay'
import type { WeatherData } from '../types'

const mockWeather: WeatherData = {
  city: 'Tokyo',
  temperatureC: 22,
  temperatureF: 71.6,
  humidity: 55,
  windSpeedMps: 4.2,
  description: 'partly cloudy',
  icon: 'https://openweathermap.org/img/wn/02d@2x.png',
}

describe('WeatherDisplay', () => {
  it('renders the city name', () => {
    render(<WeatherDisplay weather={mockWeather} />)
    expect(screen.getByText('Tokyo')).toBeInTheDocument()
  })

  it('renders temperature in both units', () => {
    render(<WeatherDisplay weather={mockWeather} />)
    expect(screen.getByText(/22°C/)).toBeInTheDocument()
    expect(screen.getByText(/71.6°F/)).toBeInTheDocument()
  })

  it('renders humidity and wind speed', () => {
    render(<WeatherDisplay weather={mockWeather} />)
    expect(screen.getByText('55%')).toBeInTheDocument()
    expect(screen.getByText('4.2 m/s')).toBeInTheDocument()
  })

  it('renders the weather icon with correct alt text', () => {
    render(<WeatherDisplay weather={mockWeather} />)
    const img = screen.getByAltText('partly cloudy')
    expect(img).toHaveAttribute('src', mockWeather.icon)
  })
})
