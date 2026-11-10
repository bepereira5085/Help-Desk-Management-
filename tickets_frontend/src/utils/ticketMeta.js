import { colors, date } from 'quasar'

// Cores de status exigidas: Verde = Aberto, Amarelo = Em andamento, Vermelho = Finalizado
export const STATUS = {
  1: { label: 'Pendente', color: 'amber-8', icon: 'hourglass_bottom' },
  2: { label: 'Em andamento', color: 'indigo-10', icon: 'construction' },
  3: { label: 'Finalizado', color: 'positive', icon: 'task_alt' }
}

export const PRIORITY = {
  1: { label: 'Baixa', color: '#78909C', icon: 'flag' },
  2: { label: 'Normal', color: '#1976D2', icon: 'flag' },
  3: { label: 'Alta', color: '#FB8C00', icon: 'flag' },
  4: { label: 'Urgente', color: '#D32F2F', icon: 'flag' }
}

export const STATUS_OPEN = 1
export const STATUS_IN_PROGRESS = 2
export const STATUS_DONE = 3

/** Converte um dos mapas acima em opções para q-select / q-btn-toggle */
export function toOptions(map) {
  return Object.entries(map).map(([value, meta]) => ({ ...meta, value: Number(value) }))
}

export function formatDateTime(value) {
  return value ? date.formatDate(value, 'DD/MM/YYYY HH:mm') : ''
}

/** Extrai a mensagem de erro retornada pela API */
export function apiErrorMessage(error, fallback = 'Ocorreu um erro inesperado.') {
  return error?.response?.data?.message || fallback
}

/**
 * Aceita tanto cores CSS (#hex, rgb(...)) quanto nomes da paleta do Quasar
 * ('positive', 'amber-8', 'indigo-10'...) e devolve um valor CSS válido.
 */
export function resolveColor(color) {
  if (!color || color.startsWith('#') || color.startsWith('rgb')) return color
  return colors.getPaletteColor(color)
}

/** Estilo "suave": texto na cor, fundo e borda na mesma cor com opacidade reduzida */
export function softColorStyle(value) {
  const color = resolveColor(value)
  return {
    color,
    backgroundColor: `color-mix(in srgb, ${color} 15%, transparent)`,
    borderColor: `color-mix(in srgb, ${color} 35%, transparent)`
  }
}
