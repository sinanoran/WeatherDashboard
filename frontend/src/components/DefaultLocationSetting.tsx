import { useState } from 'react'
import type { FormEvent } from 'react'

interface DefaultLocationSettingProps {
  currentDefault: string
  onSave: (city: string) => void
  disabled?: boolean
}

export default function DefaultLocationSetting({
  currentDefault,
  onSave,
  disabled,
}: DefaultLocationSettingProps) {
  const [city, setCity] = useState('')

  const handleSubmit = (e: FormEvent) => {
    e.preventDefault()
    if (city.trim()) {
      onSave(city.trim())
      setCity('')
    }
  }

  return (
    <div className="default-location">
      <p>
        Default location: <strong>{currentDefault || '-'}</strong>
      </p>
      <form onSubmit={handleSubmit}>
        <input
          type="text"
          placeholder="Set new default..."
          aria-label="New default city"
          value={city}
          onChange={(e) => setCity(e.target.value)}
          disabled={disabled}
        />
        <button type="submit" disabled={disabled || !city.trim()}>
          Save Default
        </button>
      </form>
    </div>
  )
}
