import { useQuasar } from 'quasar'
import { resolveColor } from '@/utils/ticketMeta'

/**
 * Devolve uma função que converte uma cor (hex ou nome da paleta do Quasar) em CSS
 * legível como texto/ícone sobre o fundo atual: no modo escuro, cores escuras
 * (ex.: indigo-10) são clareadas misturando com branco.
 */
export function useReadableColor() {
  const $q = useQuasar()

  return (color) => {
    const css = resolveColor(color)
    return $q.dark.isActive ? `color-mix(in srgb, ${css} 45%, white)` : css
  }
}
