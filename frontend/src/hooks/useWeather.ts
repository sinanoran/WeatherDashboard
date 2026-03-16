import { useCallback, useEffect, useState } from 'react'
import type { WeatherData } from '../types'
import {
  fetchWeather,
  getDefaultLocation,
  setDefaultLocation as apiSetDefaultLocation,
} from '../services/weatherApi'

export function useWeather() {
  const [weather, setWeather] = useState<WeatherData | null>(null)
  const [defaultCity, setDefaultCity] = useState('')
  const [loading, setLoading] = useState(false)
  const [error, setError] = useState<string | null>(null)

  const search = useCallback(async (city: string) => {
    if (!city.trim()) return
    setLoading(true)
    setError(null)
    try {
      const data = await fetchWeather(city.trim())
      setWeather(data)
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Unknown error')
      setWeather(null)
    } finally {
      setLoading(false)
    }
  }, [])

  const updateDefaultLocation = useCallback(async (city: string) => {
    setLoading(true)
    setError(null)
    try {
      const saved = await apiSetDefaultLocation(city.trim())
      setDefaultCity(saved)
      const data = await fetchWeather(saved)
      setWeather(data)
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Unknown error')
    } finally {
      setLoading(false)
    }
  }, [])

  // Load default location & weather on mount.
  useEffect(() => {
    let cancelled = false
    async function loadWeather() {
    setLoading(true);
    try {
      const city = await getDefaultLocation();
      if (cancelled) return;
      setDefaultCity(city);
      const data = await fetchWeather(city);
      if (cancelled) return;

      setWeather(data);
    } catch (err) {
      if (!cancelled) {
        setError(err instanceof Error ? err.message : "Unknown error");
      }
    } finally {
      if (!cancelled) {
        setLoading(false);
      }
    }
  }

  loadWeather();
    return () => { cancelled = true }
  }, [])

  const clearError = useCallback(() => setError(null), [])

  return { weather, defaultCity, loading, error, search, updateDefaultLocation, clearError }
}
