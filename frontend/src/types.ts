export interface WeatherData {
  city: string
  temperatureC: number
  temperatureF: number
  humidity: number
  windSpeedMps: number
  description: string
  icon: string
}

export interface ProblemDetails {
  title?: string
  detail?: string
  status?: number
}
