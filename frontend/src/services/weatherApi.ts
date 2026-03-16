import type { ProblemDetails, WeatherData } from '../types'

const API_BASE = '/api'

async function handleResponse<T>(response: Response): Promise<T> {
  if (!response.ok) {
    const body = await response.json().catch(() => null)

    // Support both legacy { error } and RFC 9457 ProblemDetails responses
    const problem = body as (ProblemDetails & { error?: string }) | null
    const message =
      problem?.error ??
      problem?.detail ??
      problem?.title ??
      `Request failed (${response.status})`
    throw new Error(message)
  }
  return response.json() as Promise<T>
}

export async function fetchWeather(city?: string): Promise<WeatherData> {
  const url = city
    ? `${API_BASE}/weather?city=${encodeURIComponent(city)}`
    : `${API_BASE}/weather`
  const response = await fetch(url)
  return handleResponse<WeatherData>(response)
}

export async function getDefaultLocation(): Promise<string> {
  const response = await fetch(`${API_BASE}/location/default`)
  const data = await handleResponse<{ city: string }>(response)
  return data.city
}

export async function setDefaultLocation(city: string): Promise<string> {
  const response = await fetch(`${API_BASE}/location/default`, {
    method: 'PUT',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ city }),
  })
  const data = await handleResponse<{ city: string }>(response)
  return data.city
}
