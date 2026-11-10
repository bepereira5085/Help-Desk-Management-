import { defineStore } from 'pinia'
import { ref } from 'vue'
import { api } from '@/boot/axios'

// Setores e tags usados nos selects e filtros
export const useCatalogStore = defineStore('catalog', () => {
  const departments = ref([])
  const tags = ref([])
  const loaded = ref(false)

  async function load(force = false) {
    if (loaded.value && !force) return
    const [deps, tgs] = await Promise.all([api.get('/departments'), api.get('/tags')])
    departments.value = deps.data
    tags.value = tgs.data
    loaded.value = true
  }

  function reset() {
    departments.value = []
    tags.value = []
    loaded.value = false
  }

  return { departments, tags, loaded, load, reset }
})
