<template>
  <q-dialog v-model="open" @hide="onHide">
    <q-card class="ticket-dialog">
      <q-inner-loading :showing="loading" />

      <template v-if="ticket">
        <q-card-section class="row items-start no-wrap">
          <div class="col">
            <div class="text-caption text-grey-7">Chamado #{{ ticket.id }}</div>
            <div class="text-h6">{{ ticket.itemName }}</div>
          </div>
          <q-btn flat round dense icon="close" v-close-popup />
        </q-card-section>

        <q-card-section class="row q-gutter-sm q-pt-none">
          <StatusChip :label="status.label" :color="status.color" :icon="status.icon" />
          <StatusChip
            :label="`Prioridade ${priority.label}`"
            :color="priority.color"
            :icon="priority.icon"
          />
        </q-card-section>

        <q-separator />

        <q-card-section>
          <div class="row q-col-gutter-md">
            <div v-for="info in infos" :key="info.label" class="col-12 col-sm-6">
              <div class="text-caption text-grey-7 row items-center">
                <q-icon :name="info.icon" class="q-mr-xs" />{{ info.label }}
              </div>
              <div class="text-body1">{{ info.value || '—' }}</div>
            </div>
          </div>

          <div class="text-caption text-grey-7 q-mt-md">Descrição do problema</div>
          <div class="text-body1 ticket-dialog__description">{{ ticket.description }}</div>

          <template v-if="ticket.tags.length">
            <div class="text-caption text-grey-7 q-mt-md q-mb-xs">Tags</div>
            <div class="row q-gutter-xs">
              <StatusChip
                v-for="tag in ticket.tags"
                :key="tag.id"
                :label="tag.name"
                :color="tag.color"
              />
            </div>
          </template>
        </q-card-section>

        <q-separator />

        <q-card-actions class="q-pa-md">
          <q-btn
            v-if="canCancel"
            flat
            no-caps
            color="negative"
            icon="delete"
            label="Cancelar chamado"
            :loading="busy"
            @click="confirmCancel"
          />
          <q-space />
          <template v-if="auth.isAdmin">
            <q-btn
              v-if="ticket.status === STATUS_OPEN"
              unelevated
              no-caps
              :style="{ background: resolveColor(STATUS[STATUS_IN_PROGRESS].color), color: 'white' }"
              icon="play_arrow"
              label="Iniciar atendimento"
              :loading="busy"
              @click="changeStatus(STATUS_IN_PROGRESS)"
            />
            <q-btn
              v-if="ticket.status !== STATUS_DONE"
              unelevated
              no-caps
              :style="{ background: resolveColor(STATUS[STATUS_DONE].color), color: 'white' }"
              icon="task_alt"
              label="Finalizar"
              :loading="busy"
              @click="changeStatus(STATUS_DONE)"
            />
          </template>
          <q-btn
            v-if="canEdit"
            unelevated
            no-caps
            color="primary"
            icon="edit"
            label="Editar"
            :to="{ name: 'ticket-edit', params: { id: ticket.id } }"
          />
        </q-card-actions>
      </template>
    </q-card>
  </q-dialog>
</template>

<script setup>
import { computed, onMounted, ref } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { useQuasar } from 'quasar'
import { api } from '@/boot/axios'
import { useAuthStore } from '@/stores/auth'
import StatusChip from '@/components/StatusChip.vue'
import {
  PRIORITY,
  STATUS,
  STATUS_DONE,
  STATUS_IN_PROGRESS,
  STATUS_OPEN,
  apiErrorMessage,
  formatDateTime,
  resolveColor
} from '@/utils/ticketMeta'

const emit = defineEmits(['saved'])

const $q = useQuasar()
const route = useRoute()
const router = useRouter()
const auth = useAuthStore()

const open = ref(true)
const loading = ref(false)
const busy = ref(false)
const ticket = ref(null)

const status = computed(() => STATUS[ticket.value.status])
const priority = computed(() => PRIORITY[ticket.value.priority])

const isOwner = computed(() => ticket.value?.userId === auth.user?.id)
const isOpen = computed(() => ticket.value?.status === STATUS_OPEN)
const canEdit = computed(() => auth.isAdmin || (isOwner.value && isOpen.value))
const canCancel = computed(() => isOpen.value && (auth.isAdmin || isOwner.value))

const infos = computed(() => [
  { label: 'Aberto em', icon: 'schedule', value: formatDateTime(ticket.value.createdAt) },
  { label: 'Solicitante', icon: 'person', value: ticket.value.userName },
  { label: 'Setor', icon: 'apartment', value: ticket.value.departmentName },
  { label: 'Etiqueta do equipamento', icon: 'qr_code_2', value: ticket.value.equipmentTag }
])

async function load() {
  loading.value = true
  try {
    const { data } = await api.get(`/tickets/${route.params.id}`)
    ticket.value = data
  } catch (error) {
    $q.notify({ type: 'negative', message: apiErrorMessage(error, 'Chamado não encontrado.') })
    open.value = false
  } finally {
    loading.value = false
  }
}

async function changeStatus(newStatus) {
  busy.value = true
  try {
    const { data } = await api.patch(`/tickets/${ticket.value.id}/status`, { status: newStatus })
    ticket.value = data
    $q.notify({ type: 'positive', message: `Status alterado para "${STATUS[newStatus].label}".` })
    emit('saved')
  } catch (error) {
    $q.notify({ type: 'negative', message: apiErrorMessage(error) })
  } finally {
    busy.value = false
  }
}

function confirmCancel() {
  $q.dialog({
    title: 'Cancelar chamado',
    message: `O chamado #${ticket.value.id} será excluído. Deseja continuar?`,
    cancel: { label: 'Voltar', flat: true, noCaps: true },
    ok: { label: 'Cancelar chamado', color: 'negative', unelevated: true, noCaps: true },
    persistent: true
  }).onOk(async () => {
    busy.value = true
    try {
      await api.delete(`/tickets/${ticket.value.id}`)
      $q.notify({ type: 'positive', message: 'Chamado cancelado.' })
      emit('saved')
      open.value = false
    } catch (error) {
      $q.notify({ type: 'negative', message: apiErrorMessage(error) })
    } finally {
      busy.value = false
    }
  })
}

// Ao fechar o diálogo volta para a listagem (exceto se já navegou para outra rota, ex.: Editar)
function onHide() {
  if (route.name === 'ticket-details') router.push({ name: 'tickets' })
}

onMounted(load)
</script>

<style scoped>
.ticket-dialog {
  width: 640px;
  max-width: 95vw;
  min-height: 200px;
}

.ticket-dialog__description {
  white-space: pre-wrap;
}
</style>
