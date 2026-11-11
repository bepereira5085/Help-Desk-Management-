<template>
  <q-page padding>
    <div class="page-container">
      <!-- Cabeçalho -->
      <div class="row items-end q-col-gutter-md q-mb-lg">
        <div class="col-12 col-md">
          <div class="text-h4 text-weight-bold">Painel de Chamados</div>
          <div class="text-body1 text-grey-7">
            {{
              auth.isAdmin
                ? 'Visão geral e acompanhamento de todas as solicitações.'
                : 'Acompanhe as solicitações que você abriu.'
            }}
          </div>
        </div>
        <div class="col-12 col-md-auto row q-gutter-sm">
          <StatCard label="Total" :value="stats.total" />
          <StatCard
            v-for="s in statusOptions"
            :key="s.value"
            :label="s.label"
            :value="stats[s.value]"
            :color="s.color"
          />
        </div>
      </div>

      <!-- Filtros -->
      <q-card flat bordered class="q-mb-lg">
        <q-card-section>
          <div class="row q-col-gutter-md">
            <div class="col-12 col-md-4">
              <div class="filter-label">Buscar por item</div>
              <q-input
                v-model="filters.itemName"
                outlined
                dense
                clearable
                debounce="400"
                placeholder="Ex.: Computador, Impressora..."
              >
                <template #prepend><q-icon name="search" /></template>
              </q-input>
            </div>

            <div class="col-12 col-sm-6 col-md-4">
              <div class="filter-label">Status do chamado</div>
              <q-btn-toggle
                v-model="filters.status"
                spread
                no-caps
                unelevated
                class="filter-toggle"
                toggle-color="primary"
                :options="statusToggle"
              />
            </div>

            <div class="col-12 col-sm-6 col-md-4">
              <div class="filter-label">Prioridade</div>
              <q-btn-toggle
                v-model="filters.priority"
                spread
                no-caps
                unelevated
                class="filter-toggle"
                toggle-color="primary"
                :options="priorityToggle"
              />
            </div>

            <!-- Filtros exclusivos do administrador -->
            <template v-if="auth.isAdmin">
              <div class="col-12 col-sm-6 col-md-4">
                <div class="filter-label">Data de abertura</div>
                <q-input
                  v-model="filters.date"
                  outlined
                  dense
                  clearable
                  readonly
                  placeholder="Qualquer data"
                >
                  <template #prepend>
                    <q-icon name="event" class="cursor-pointer" />
                  </template>
                  <q-popup-proxy cover transition-show="scale" transition-hide="scale">
                    <q-date v-model="filters.date" mask="DD/MM/YYYY" today-btn>
                      <div class="row justify-end">
                        <q-btn v-close-popup label="OK" color="primary" flat />
                      </div>
                    </q-date>
                  </q-popup-proxy>
                </q-input>
              </div>

              <div class="col-12 col-sm-6 col-md-4">
                <div class="filter-label">Setor</div>
                <q-select
                  v-model="filters.departmentId"
                  :options="catalog.departments"
                  option-value="id"
                  option-label="name"
                  emit-value
                  map-options
                  outlined
                  dense
                  clearable
                  placeholder="Todos os setores"
                >
                  <template #prepend><q-icon name="apartment" /></template>
                </q-select>
              </div>
            </template>
          </div>
        </q-card-section>
      </q-card>

      <!-- Barra de ações -->
      <div class="row items-center q-mb-md">
        <div class="text-subtitle1 text-grey-8">
          {{ tickets.length }} chamado(s) — ordenados por prioridade e data
        </div>
        <q-space />
        <div class="row items-center q-gutter-sm">
          <q-btn
            flat
            no-caps
            :color="actionColor"
            icon="filter_alt_off"
            label="Limpar filtros"
            :disable="!hasFilters"
            @click="clearFilters"
          />
          <q-btn flat round :color="actionColor" icon="refresh" :loading="loading" @click="refreshAll">
            <q-tooltip>Recarregar</q-tooltip>
          </q-btn>
          <q-btn
            unelevated
            no-caps
            color="primary"
            icon="add"
            label="Novo chamado"
            :to="{ name: 'ticket-new' }"
          />
        </div>
      </div>

      <!-- Listagem: 3 cards por linha no desktop -->
      <div class="relative-position" style="min-height: 200px">
        <div v-if="tickets.length" class="row q-col-gutter-md">
          <div v-for="ticket in pageTickets" :key="ticket.id" class="col-12 col-sm-6 col-md-4">
            <TicketCard :ticket="ticket" @click="openDetails" />
          </div>
        </div>

        <div v-else-if="!loading" class="column items-center q-py-xl text-grey-6">
          <q-icon name="inbox" size="64px" />
          <div class="text-h6 q-mt-sm">Nenhum chamado encontrado</div>
          <div>Ajuste os filtros ou abra um novo chamado.</div>
        </div>

        <q-inner-loading :showing="loading" />
      </div>

      <!-- Paginação -->
      <ListPagination
        v-if="tickets.length"
        v-model:page="page"
        v-model:per-page="perPage"
        :total="tickets.length"
        class="q-mt-lg"
      />
    </div>

    <!-- Diálogos por rota (novo / detalhes / edição) -->
    <router-view @saved="refreshAll" />
  </q-page>
</template>

<script setup>
import { computed, onMounted, reactive, ref, watch } from 'vue'
import { useRouter } from 'vue-router'
import { useQuasar } from 'quasar'
import { api } from '@/boot/axios'
import { useAuthStore } from '@/stores/auth'
import { useCatalogStore } from '@/stores/catalog'
import ListPagination from '@/components/ListPagination.vue'
import StatCard from '@/components/StatCard.vue'
import TicketCard from '@/components/TicketCard.vue'
import { PRIORITY, STATUS, apiErrorMessage, toOptions } from '@/utils/ticketMeta'

const $q = useQuasar()
const router = useRouter()
const auth = useAuthStore()
const catalog = useCatalogStore()

const statusOptions = toOptions(STATUS)
// Opções dos q-btn-toggle (apenas label/value; "0" = sem filtro). Prioridade: Urgente primeiro
const statusToggle = [
  { label: 'Todos', value: 0 },
  ...statusOptions.map(({ label, value }) => ({ label: label.replace('Em andamento', 'Andamento'), value }))
]
const priorityToggle = [
  { label: 'Todas', value: 0 },
  ...toOptions(PRIORITY)
    .reverse()
    .map(({ label, value }) => ({ label, value }))
]

const tickets = ref([])
const statsSource = ref([]) // todos os chamados visíveis (sem filtros), para os contadores
const loading = ref(false)

// Paginação (feita no front): 20 cards por página por padrão
const page = ref(1)
const perPage = ref(20)

const pageTickets = computed(() => {
  const start = (page.value - 1) * perPage.value
  return tickets.value.slice(start, start + perPage.value)
})

const DEFAULT_FILTERS = {
  itemName: '',
  status: 0,
  priority: 0,
  date: null,
  departmentId: null
}

const filters = reactive({ ...DEFAULT_FILTERS })

// O watch em "filters" dispara uma única vez e recarrega a listagem
function clearFilters() {
  Object.assign(filters, DEFAULT_FILTERS)
}

// A cor primária é escura demais para botões "flat" sobre o fundo do modo escuro
const actionColor = computed(() => ($q.dark.isActive ? 'white' : 'primary'))

const hasFilters = computed(
  () =>
    !!filters.itemName || !!filters.status || !!filters.priority || !!filters.date || !!filters.departmentId
)

const stats = computed(() => {
  const result = { total: statsSource.value.length }
  statusOptions.forEach((s) => {
    result[s.value] = statsSource.value.filter((t) => t.status === s.value).length
  })
  return result
})

function buildParams() {
  const params = {}
  if (filters.itemName) params.itemName = filters.itemName
  if (filters.status) params.status = filters.status
  if (filters.priority) params.priority = filters.priority
  if (auth.isAdmin) {
    if (filters.departmentId) params.departmentId = filters.departmentId
    if (filters.date) {
      const [day, month, year] = filters.date.split('/')
      params.date = `${year}-${month}-${day}`
    }
  }
  return params
}

// A ordenação (prioridade Urgente → Baixa e, em seguida, data mais recente) vem do backend
async function fetchTickets(params = {}) {
  const { data } = await api.get('/tickets', { params })
  return data
}

async function withLoading(task) {
  loading.value = true
  try {
    await task()
  } catch (error) {
    $q.notify({ type: 'negative', message: apiErrorMessage(error, 'Erro ao carregar chamados.') })
  } finally {
    loading.value = false
  }
}

// Mudança de filtro: busca apenas a listagem filtrada (os contadores não mudam)
function loadTickets() {
  return withLoading(async () => {
    tickets.value = await fetchTickets(buildParams())
    page.value = 1
  })
}

// Abertura da página ou chamado criado/editado/cancelado: atualiza listagem e contadores.
// Sem filtros ativos, uma única requisição serve para os dois.
function refreshAll() {
  return withLoading(async () => {
    if (!hasFilters.value) {
      tickets.value = statsSource.value = await fetchTickets()
      return
    }
    const [filtered, all] = await Promise.all([fetchTickets(buildParams()), fetchTickets()])
    tickets.value = filtered
    statsSource.value = all
  })
}

function openDetails(ticket) {
  router.push({ name: 'ticket-details', params: { id: ticket.id } })
}

watch(filters, loadTickets)

onMounted(() => {
  refreshAll()
  catalog.load().catch(() => {})
})
</script>

<style scoped>
.filter-label {
  font-size: 12px;
  font-weight: 600;
  margin-bottom: 6px;
  opacity: 0.75;
}

.filter-toggle {
  border: 1px solid rgba(0, 0, 0, 0.12);
}

.body--dark .filter-toggle {
  border-color: rgba(255, 255, 255, 0.2);
}
</style>
