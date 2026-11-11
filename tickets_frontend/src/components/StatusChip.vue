<template>
  <span class="status-chip" :class="{ 'status-chip--dense': dense }" :style="chipStyle">
    <q-icon v-if="icon" :name="icon" class="status-chip__icon" />
    {{ label }}
  </span>
</template>

<script setup>
import { computed } from 'vue'
import { useReadableColor } from '@/composables/useReadableColor'
import { softColorStyle } from '@/utils/ticketMeta'

// Chip genérico: texto na cor informada sobre um fundo da mesma cor com opacidade reduzida.
// Usado para status, prioridade, tags e setores.
const props = defineProps({
  label: { type: String, required: true },
  color: { type: String, default: '#757575' },
  icon: { type: String, default: null },
  dense: { type: Boolean, default: false }
})

const readable = useReadableColor()

// Texto clareado no modo escuro; fundo e borda mantêm a cor original
const chipStyle = computed(() => softColorStyle(props.color, readable(props.color)))
</script>

<style scoped>
.status-chip {
  display: inline-flex;
  align-items: center;
  justify-content: center;
  gap: 4px;
  padding: 3px 10px;
  border: 1px solid;
  border-radius: 999px;
  font-size: 12px;
  font-weight: 600;
  line-height: 1.4;
  white-space: nowrap;
  text-align: center;
}

.status-chip--dense {
  padding: 1px 8px;
  font-size: 11px;
}

.status-chip__icon {
  font-size: 1.15em;
}
</style>
