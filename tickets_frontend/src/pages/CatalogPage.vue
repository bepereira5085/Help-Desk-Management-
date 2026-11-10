<template>
  <q-page padding>
    <div class="page-container">
      <div class="q-mb-lg">
        <div class="text-h4 text-weight-bold">Cadastros</div>
        <div class="text-body1 text-grey-7">
          Gerencie os setores e as tags disponíveis na abertura de chamados.
        </div>
      </div>

      <q-card flat bordered>
        <q-tabs v-model="tab" align="left" active-color="primary" indicator-color="primary" no-caps>
          <q-tab name="departments" icon="apartment" label="Setores" />
          <q-tab name="tags" icon="sell" label="Tags" />
        </q-tabs>
        <q-separator />

        <q-tab-panels v-model="tab" animated>
          <q-tab-panel name="departments">
            <CatalogManager
              endpoint="/departments"
              title="Setores"
              singular="Setor"
              @changed="refreshCatalog"
            />
          </q-tab-panel>
          <q-tab-panel name="tags">
            <CatalogManager endpoint="/tags" title="Tags" singular="Tag" @changed="refreshCatalog" />
          </q-tab-panel>
        </q-tab-panels>
      </q-card>
    </div>
  </q-page>
</template>

<script setup>
import { ref } from 'vue'
import { useCatalogStore } from '@/stores/catalog'
import CatalogManager from '@/components/CatalogManager.vue'

const tab = ref('departments')
const catalog = useCatalogStore()

// Mantém os selects do restante do sistema atualizados
function refreshCatalog() {
  catalog.load(true).catch(() => {})
}
</script>
