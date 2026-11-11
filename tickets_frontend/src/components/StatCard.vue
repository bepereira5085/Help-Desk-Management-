<template>
  <q-card flat bordered class="stat-card" :style="{ borderBottomColor: cssColor }">
    <q-card-section class="q-py-sm q-px-md">
      <div class="text-caption text-weight-bold text-uppercase" :style="{ color: textColor }">
        {{ label }}
      </div>
      <div class="text-h5 text-weight-bold">{{ value }}</div>
    </q-card-section>
  </q-card>
</template>

<script setup>
import { computed } from 'vue'
import { useReadableColor } from '@/composables/useReadableColor'
import { resolveColor } from '@/utils/ticketMeta'

const props = defineProps({
  label: { type: String, required: true },
  value: { type: Number, default: 0 },
  color: { type: String, default: '#616161' }
})

const readable = useReadableColor()
const cssColor = computed(() => resolveColor(props.color)) // borda
const textColor = computed(() => readable(props.color)) // rótulo, legível no modo escuro
</script>

<style scoped>
.stat-card {
  min-width: 120px;
  border-bottom-width: 3px;
  border-bottom-style: solid;
}
</style>
