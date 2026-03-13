import { render, screen, fireEvent } from '@testing-library/react'
import { describe, it, expect, vi } from 'vitest'
import SearchBar from './SearchBar'

describe('SearchBar', () => {
  it('renders an input and a button', () => {
    render(<SearchBar onSearch={vi.fn()} />)
    expect(screen.getByPlaceholderText(/enter city name/i)).toBeInTheDocument()
    expect(screen.getByRole('button', { name: /search/i })).toBeInTheDocument()
  })

  it('calls onSearch with trimmed input on submit', () => {
    const onSearch = vi.fn()
    render(<SearchBar onSearch={onSearch} />)

    fireEvent.change(screen.getByPlaceholderText(/enter city name/i), {
      target: { value: '  Berlin  ' },
    })
    fireEvent.click(screen.getByRole('button', { name: /search/i }))

    expect(onSearch).toHaveBeenCalledWith('Berlin')
  })

  it('disables the button when input is empty', () => {
    render(<SearchBar onSearch={vi.fn()} />)
    expect(screen.getByRole('button', { name: /search/i })).toBeDisabled()
  })

  it('disables controls when disabled prop is true', () => {
    render(<SearchBar onSearch={vi.fn()} disabled />)
    expect(screen.getByPlaceholderText(/enter city name/i)).toBeDisabled()
  })
})
