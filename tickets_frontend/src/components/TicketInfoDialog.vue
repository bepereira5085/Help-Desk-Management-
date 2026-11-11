<template>
  <q-dialog v-model="open" persistent @hide="onHide">
    <q-card class="ticket-form-dialog">
      <q-card-section class="row items-center no-wrap">
        <q-icon
          :name="isEdit ? 'edit_note' : 'add_circle'"
          size="28px"
          :color="$q.dark.isActive ? 'white' : 'primary'"
        />
        <div class="text-h6 q-ml-sm">
          {{ isEdit ? `Editar chamado #${route.params.id}` : 'Novo chamado' }}
        </div>
        <q-space />
        <q-btn flat round dense icon="close" v-close-popup />
      </q-card-section>

      <q-separator />

      <q-form ref="formRef" greedy @submit="save">
        <q-card-section>
          <q-inner-loading :showing="loading" />

          <div class="row q-col-gutter-md">
            <div class="col-12 col-sm-6">
              <q-select
                v-model="form.departmentId"
                :options="catalog.departments"
                option-value="id"
                option-label="name"
                emit-value
                map-options
                outlined
                label="Setor *"
                :rules="[(v) => !!v || 'Informe o setor']"
              >
                <template #prepend><q-icon name="apartment" /></template>
              </q-select>
            </div>

            <div class="col-12 col-sm-6">
              <q-select
                v-model="form.priority"
                :options="priorityOptions"
                emit-value
                map-options
                outlined
                label="Prioridade *"
              >
                <template #prepend>
                  <q-icon name="flag" :style="{ color: readable(PRIORITY[form.priority]?.color) }" />
                </template>
                <template #option="scope">
                  <q-item v-bind="scope.itemProps">
                    <q-item-section avatar>
                      <q-icon name="flag" :style="{ color: readable(scope.opt.color) }" />
                    </q-item-section>
                    <q-item-section>{{ scope.opt.label }}</q-item-section>
                  </q-item>
                </template>
              </q-select>
            </div>

            <div class="col-12 col-sm-7">
              <q-input
                v-model.trim="form.itemName"
                outlined
                label="Nome do item *"
                hint="Ex.: Computador, Impressora, Sistema"
                maxlength="150"
                :rules="[(v) => !!v || 'Informe o item']"
              >
                <template #prepend><q-icon name="devices" /></template>
              </q-input>
            </div>

            <div class="col-12 col-sm-5">
              <q-input
                v-model.trim="form.equipmentTag"
                outlined
                label="Etiqueta do equipamento"
                hint="Número patrimonial (opcional)"
                maxlength="50"
              >
                <template #prepend><q-icon name="qr_code_2" /></template>
              </q-input>
            </div>

            <div class="col-12">
              <q-input
                v-model.trim="form.description"
                type="textarea"
                outlined
                autogrow
                input-style="min-height: 96px"
                label="Descrição do problema *"
                maxlength="2000"
                counter
                :rules="[(v) => !!v || 'Descreva o problema']"
              />
            </div>

            <div class="col-12">
              <q-select
                v-model="form.tagIds"
                :options="catalog.tags"
                option-value="id"
                option-label="name"
                emit-value
                map-options
                multiple
                outlined
                label="Tags"
              >
                <template #prepend><q-icon name="sell" /></template>
                <template #selected-item="scope">
                  <q-chip
                    removable
                    dense
                    :tabindex="scope.tabindex"
                    :style="softColorStyle(scope.opt.color, readable(scope.opt.color))"
                    class="tag-chip q-ma-xs"
                    @remove="scope.removeAtIndex(scope.index)"
                  >
                    {{ scope.opt.name }}
                  </q-chip>
                </template>
                <template #option="scope">
                  <q-item v-bind="scope.itemProps">
                    <q-item-section avatar>
                      <q-icon name="circle" size="14px" :style="{ color: readable(scope.opt.color) }" />
                    </q-item-section>
                    <q-item-section>{{ scope.opt.name }}</q-item-section>
                    <q-item-section side>
                      <q-icon v-if="scope.selected" name="check" :color="$q.dark.isActive ? 'white' : 'primary'" />
                    </q-item-section>
                  </q-item>
                </template>
              </q-select>
            </div>

            <!-- Somente o administrador altera o status pela edição -->
            <div v-if="isEdit && auth.isAdmin" class="col-12 col-sm-6">
              <q-select
                v-model="form.status"
                :options="statusOptions"
                emit-value
                map-options
                outlined
                label="Status"
              >
                <template #prepend>
                  <q-icon name="circle" :style="{ color: readable(STATUS[form.status]?.color) }" />
                </template>
                <template #option="scope">
                  <q-item v-bind="scope.itemProps">
                    <q-item-section avatar>
                      <q-icon name="circle" size="14px" :style="{ color: readable(scope.opt.color) }" />
                    </q-item-section>
                    <q-item-section>{{ scope.opt.label }}</q-item-section>
                  </q-item>
                </template>
              </q-select>
            </div>
          </div>
        </q-card-section>

        <q-separator />

        <q-card-actions align="right" class="q-pa-md">
          <q-btn flat no-caps label="Cancelar" v-close-popup />
          <q-btn
            type="submit"
            unelevated
            no-caps
            color="primary"
            :icon="isEdit ? 'save' : 'send'"
            :label="isEdit ? 'Salvar alterações' : 'Abrir chamado'"
            :loading="saving"
            :disable="loading"
          />
        </q-card-actions>
      </q-form>
    </q-card>
  </q-dialog>
</template>

<script setup>
import { computed, onMounted, reactive, ref } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { useQuasar } from 'quasar'
import { api } from '@/boot/axios'
import { useAuthStore } from '@/stores/auth'
import { useCatalogStore } from '@/stores/catalog'
import { useReadableColor } from '@/composables/useReadableColor'
import {
  PRIORITY,
  STATUS,
  STATUS_OPEN,
  apiErrorMessage,
  softColorStyle,
  toOptions
} from '@/utils/ticketMeta'

// Formulário único para criação (/tickets/new) e edição (/tickets/:id/edit)
const emit = defineEmits(['saved'])

const $q = useQuasar()
const route = useRoute()
const router = useRouter()
const auth = useAuthStore()
const catalog = useCatalogStore()
const readable = useReadableColor()

const isEdit = computed(() => !!route.params.id)
const priorityOptions = toOptions(PRIORITY)
const statusOptions = toOptions(STATUS)

const open = ref(true)
const loading = ref(false)
const saving = ref(false)
const formRef = ref(null)

const form = reactive({
  departmentId: auth.user?.departmentId ?? null,
  itemName: '',
  description: '',
  priority: 2,
  equipmentTag: '',
  tagIds: [],
  status: STATUS_OPEN
})

async function loadTicket() {
  const { data } = await api.get(`/tickets/${route.params.id}`)

  const canEdit = auth.isAdmin || (data.userId === auth.user?.id && data.status === STATUS_OPEN)
  if (!canEdit) {
    $q.notify({ type: 'warning', message: 'Este chamado não pode mais ser editado.' })
    router.replace({ name: 'ticket-details', params: { id: data.id } })
    return
  }

  Object.assign(form, {
    departmentId: data.departmentId,
    itemName: data.itemName,
    description: data.description,
    priority: data.priority,
    equipmentTag: data.equipmentTag ?? '',
    tagIds: data.tags.map((t) => t.id),
    status: data.status
  })
}

async function save() {
  saving.value = true
  const payload = {
    departmentId: form.departmentId,
    itemName: form.itemName,
    description: form.description,
    priority: form.priority,
    equipmentTag: form.equipmentTag || null,
    tagIds: form.tagIds
  }

  try {
    if (isEdit.value) {
      if (auth.isAdmin) payload.status = form.status
      const { data } = await api.put(`/tickets/${route.params.id}`, payload)
      $q.notify({ type: 'positive', message: 'Chamado atualizado.' })
      emit('saved')
      router.replace({ name: 'ticket-details', params: { id: data.id } })
    } else {
      const { data } = await api.post('/tickets', payload)
      $q.notify({ type: 'positive', message: `Chamado #${data.id} aberto com sucesso.` })
      emit('saved')
      open.value = false
    }
  } catch (error) {
    $q.notify({ type: 'negative', message: apiErrorMessage(error, 'Não foi possível salvar.') })
  } finally {
    saving.value = false
  }
}

// Fechou sem navegar para outra rota -> volta para a listagem
function onHide() {
  if (route.name === 'ticket-new' || route.name === 'ticket-edit') {
    router.push({ name: 'tickets' })
  }
}

onMounted(async () => {
  loading.value = true
  try {
    await catalog.load()
    if (isEdit.value) await loadTicket()
  } catch (error) {
    $q.notify({ type: 'negative', message: apiErrorMessage(error, 'Erro ao carregar dados.') })
    open.value = false
  } finally {
    loading.value = false
  }
})
</script>

<style scoped>
.tag-chip {
  border: 1px solid;
  font-weight: 600;
}

.ticket-form-dialog {
  width: 720px;
  max-width: 95vw;
}
</style>
