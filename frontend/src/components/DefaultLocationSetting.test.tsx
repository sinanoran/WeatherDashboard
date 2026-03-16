import { render, screen, fireEvent } from '@testing-library/react'
import { describe, it, expect, vi } from 'vitest'
import DefaultLocationSetting from './DefaultLocationSetting'

describe('DefaultLocationSetting', () => {
  it('displays the current default location', () => {
    render(<DefaultLocationSetting currentDefault="London" onSave={vi.fn()} />)
    expect(screen.getByText(/London/)).toBeInTheDocument()
  })

  it('calls onSave with the new city on submit', () => {
    const onSave = vi.fn()
    render(<DefaultLocationSetting currentDefault="London" onSave={onSave} />)

    fireEvent.change(screen.getByPlaceholderText(/set new default/i), {
      target: { value: 'Paris' },
    })
    fireEvent.click(screen.getByRole('button', { name: /save default/i }))

    expect(onSave).toHaveBeenCalledWith('Paris')
  })

  it('clears the input after submit', () => {
    render(<DefaultLocationSetting currentDefault="London" onSave={vi.fn()} />)

    const input = screen.getByPlaceholderText(/set new default/i) as HTMLInputElement
    fireEvent.change(input, { target: { value: 'Rome' } })
    fireEvent.click(screen.getByRole('button', { name: /save default/i }))

    expect(input.value).toBe('')
  })
})
