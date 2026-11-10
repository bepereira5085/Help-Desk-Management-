<template>
  <div>
    <q-table
      flat
      bordered
      :title="title"
      :rows="rows"
      :columns="columns"
      row-key="id"
      :loading="loading"
      :pagination="{ rowsPerPage: 10 }"
      no-data-label="Nenhum registro cadastrado"
    >
      <template #top-right>
        <q-btn unelevated no-caps color="primary" icon="add" label="Adicionar" @click="openForm()" />
      </template>

      <template #body-cell-preview="props">
        <q-td :props="props">
          <StatusChip :label="props.row.name" :color="props.row.color" />
        </q-td>
      </template>

      <template #body-cell-actions="props">
        <q-td :props="props">
          <q-btn flat round dense icon="edit" color="primary" @click="openForm(props.row)">
            <q-tooltip>Editar</q-tooltip>
          </q-btn>
          <q-btn flat round dense icon="delete" color="negative" @click="confirmDelete(props.row)">
            <q-tooltip>Excluir</q-tooltip>
          </q-btn>
        </q-td>
      </template>
    </q-table>

    <!-- Criação / edição -->
    <q-dialog v-model="dialogOpen">
      <q-card style="width: 420px; max-width: 95vw">
        <q-card-section class="text-h6">
          {{ form.id ? `Editar ${singular}` : `Novo(a) ${singular}` }}
        </q-card-section>

        <q-form @submit="save">
          <q-card-section class="q-gutter-y-md q-pt-none">
            <q-input
              v-model.trim="form.name"
              outlined
              label="Nome *"
              autofocus
              :rules="[(v) => !!v || 'Informe o nome']"
            />

            <q-input
              v-model="form.color"
              outlined
              label="Cor *"
              :rules="[(v) => /^#[0-9a-fA-F]{6}$/.test(v) || 'Use o formato #RRGGBB']"
            >
              <template #prepend>
                <q-icon name="circle" :style="{ color: form.color }" />
              </template>
              <template #append>
                <q-icon name="colorize" class="cursor-pointer">
                  <q-popup-proxy cover transition-show="scale" transition-hide="scale">
                    <q-color v-model="form.color" format-model="hex" no-header-tabs />
                  </q-popup-proxy>
                </q-icon>
              </template>
            </q-input>

            <div class="row items-center q-gutter-sm">
              <span class="text-caption text-grey-7">Prévia:</span>
              <StatusChip :label="form.name || 'Exemplo'" :color="form.color" />
            </div>
          </q-card-section>

          <q-card-actions align="right" class="q-pa-md">
            <q-btn flat no-caps label="Cancelar" v-close-popup />
            <q-btn type="submit" unelevated no-caps color="primary" label="Salvar" :loading="saving" />
          </q-card-actions>
        </q-form>
      </q-card>
    </q-dialog>
  </div>
</template>

<script setup>
import { onMounted, reactive, ref } from 'vue'
import { useQuasar } from 'quasar'
import { api } from '@/boot/axios'
import StatusChip from '@/components/StatusChip.vue'
import { apiErrorMessage } from '@/utils/ticketMeta'

// CRUD genérico de registros { id, name, color } — usado para Setores e Tags
const props = defineProps({
  endpoint: { type: String, required: true },
  title: { type: String, required: true },
  singular: { type: String, required: true }
})

const emit = defineEmits(['changed'])

const $q = useQuasar()

const columns = [
  { name: 'id', label: '#', field: 'id', align: 'left', sortable: true, style: 'width: 60px' },
  { name: 'name', label: 'Nome', field: 'name', align: 'left', sortable: true },
  { name: 'preview', label: 'Prévia', field: 'color', align: 'left' },
  { name: 'actions', label: 'Ações', field: 'id', align: 'right' }
]

const rows = ref([])
const loading = ref(false)
const saving = ref(false)
const dialogOpen = ref(false)
const form = reactive({ id: null, name: '', color: '#1976D2' })

async function load() {
  loading.value = true
  try {
    const { data } = await api.get(props.endpoint)
    rows.value = data
  } catch (error) {
    $q.notify({ type: 'negative', message: apiErrorMessage(error) })
  } finally {
    loading.value = false
  }
}

function openForm(row = null) {
  Object.assign(form, row ? { ...row } : { id: null, name: '', color: '#1976D2' })
  dialogOpen.value = true
}

async function save() {
  saving.value = true
  try {
    const payload = { name: form.name, color: form.color }
    if (form.id) await api.put(`${props.endpoint}/${form.id}`, payload)
    else await api.post(props.endpoint, payload)

    $q.notify({ type: 'positive', message: `${props.singular} salvo(a).` })
    dialogOpen.value = false
    await load()
    emit('changed')
  } catch (error) {
    $q.notify({ type: 'negative', message: apiErrorMessage(error, 'Não foi possível salvar.') })
  } finally {
    saving.value = false
  }
}

function confirmDelete(row) {
  $q.dialog({
    title: `Excluir ${props.singular.toLowerCase()}`,
    message: `Deseja excluir "${row.name}"?`,
    cancel: { label: 'Cancelar', flat: true, noCaps: true },
    ok: { label: 'Excluir', color: 'negative', unelevated: true, noCaps: true }
  }).onOk(async () => {
    try {
      await api.delete(`${props.endpoint}/${row.id}`)
      $q.notify({ type: 'positive', message: `${props.singular} excluído(a).` })
      await load()
      emit('changed')
    } catch (error) {
      $q.notify({ type: 'negative', message: apiErrorMessage(error, 'Não foi possível excluir.') })
    }
  })
}

onMounted(load)
</script>
