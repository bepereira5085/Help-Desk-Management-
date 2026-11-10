<template>
  <q-layout view="hHh lpR fFf">
    <q-header elevated class="bg-primary text-white">
      <q-toolbar class="q-px-md">
        <q-btn flat dense no-caps :to="{ name: 'tickets' }" class="q-px-sm">
          <q-icon name="support_agent" size="28px" class="q-mr-sm" />
          <span class="text-h6 text-weight-regular">Helpdesk Corporativo</span>
        </q-btn>

        <q-space />

        <div class="row items-center q-gutter-sm no-wrap">
          <template v-if="auth.isAdmin">
            <q-btn
              flat
              no-caps
              icon="confirmation_number"
              :label="$q.screen.gt.sm ? 'Chamados' : undefined"
              :to="{ name: 'tickets' }"
            />
            <q-btn
              flat
              no-caps
              icon="tune"
              :label="$q.screen.gt.sm ? 'Cadastros' : undefined"
              :to="{ name: 'catalog' }"
            />
          </template>

          <q-chip
            color="white"
            text-color="primary"
            icon="person"
            class="text-weight-medium"
            :label="$q.screen.gt.xs ? `${auth.user?.name} (${roleLabel})` : roleLabel"
          />

          <q-btn
            flat
            round
            :icon="$q.dark.isActive ? 'light_mode' : 'dark_mode'"
            @click="toggleDark"
          >
            <q-tooltip>Alternar tema</q-tooltip>
          </q-btn>

          <q-btn flat no-caps icon="logout" :label="$q.screen.gt.xs ? 'Sair' : undefined" @click="logout" />
        </div>
      </q-toolbar>
    </q-header>

    <q-page-container>
      <router-view />
    </q-page-container>
  </q-layout>
</template>

<script setup>
import { computed } from 'vue'
import { useRouter } from 'vue-router'
import { useQuasar } from 'quasar'
import { useAuthStore } from '@/stores/auth'
import { useCatalogStore } from '@/stores/catalog'

const $q = useQuasar()
const router = useRouter()
const auth = useAuthStore()
const catalog = useCatalogStore()

const roleLabel = computed(() => (auth.isAdmin ? 'Admin' : 'Comum'))

function toggleDark() {
  $q.dark.toggle()
  $q.localStorage.set('helpdesk-dark', $q.dark.isActive)
}

function logout() {
  $q.dialog({
    title: 'Sair',
    message: 'Deseja encerrar a sessão?',
    cancel: { label: 'Cancelar', flat: true, noCaps: true },
    ok: { label: 'Sair', color: 'primary', unelevated: true, noCaps: true }
  }).onOk(() => {
    auth.logout()
    catalog.reset()
    router.push({ name: 'login' })
  })
}
</script>
