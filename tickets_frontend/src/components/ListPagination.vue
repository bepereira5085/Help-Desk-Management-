<template>
  <div class="row items-center justify-end q-gutter-md">
    <q-pagination
      v-model="page"
      :max="maxPages"
      :max-pages="7"
      direction-links
      boundary-links
      :color="dark ? 'white' : 'primary'"
      :active-color="dark ? 'grey-8' : 'primary'"
      active-text-color="white"
      active-design="unelevated"
    />

    <q-select
      v-if="allowChangePageItemsAmount"
      v-model="perPage"
      :options="perPageOptions"
      dense
      outlined
      options-dense
      label="Por página"
      style="width: 130px"
    />
  </div>
</template>

<script setup>
import { computed, watch } from 'vue'
import { useQuasar } from 'quasar'

// Paginador reutilizável: v-model:page e v-model:per-page
const props = defineProps({
  // Total de itens da listagem (usado para calcular o número de páginas)
  total: { type: Number, default: 0 },
  // Quando false, esconde o seletor de itens por página
  allowChangePageItemsAmount: { type: Boolean, default: false },
  perPageOptions: { type: Array, default: () => [10, 20, 50, 100] }
})

const page = defineModel('page', { type: Number, default: 1 })
const perPage = defineModel('perPage', { type: Number, default: 20 })

const $q = useQuasar()
const dark = computed(() => $q.dark.isActive)

const maxPages = computed(() => Math.max(1, Math.ceil(props.total / perPage.value)))

// Evita ficar numa página que deixou de existir (ex.: após cancelar chamados)
watch(maxPages, (max) => {
  if (page.value > max) page.value = max
})

// Trocar a quantidade por página volta para a primeira página
watch(perPage, () => {
  page.value = 1
})
</script>
