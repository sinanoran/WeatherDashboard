import { useState } from 'react'
import type { FormEvent } from 'react'

interface SearchBarProps {
  onSearch: (city: string) => void
  defaultCity?: string
  disabled?: boolean
}

export default function SearchBar({ onSearch, defaultCity, disabled }: SearchBarProps) {
  const [query, setQuery] = useState('')

  const handleSubmit = (e: FormEvent) => {
    e.preventDefault()
    if (query.trim()) {
      onSearch(query.trim())
    }
  }

  const handleUseDefault = () => {
    if (defaultCity) {
      onSearch(defaultCity)
    }
  }

  return (
    <form className="search-bar" onSubmit={handleSubmit}>
      <input
        type="text"
        placeholder="Enter city name..."
        aria-label="City name"
        value={query}
        onChange={(e) => setQuery(e.target.value)}
        disabled={disabled}
      />
      <button type="submit" disabled={disabled || !query.trim()}>
        Search
      </button>
      {defaultCity && (
        <button type="button" onClick={handleUseDefault} disabled={disabled}>
          Use Default
        </button>
      )}
    </form>
  )
}
