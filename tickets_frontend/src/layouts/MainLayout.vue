<template>
  <q-layout view="hHh lpR fFf">
    <q-header elevated class="bg-primary text-white">
      <q-toolbar class="q-px-md">
        <q-btn flat dense no-caps :to="{ name: 'tickets' }" class="q-px-sm">
          <q-icon name="support_agent" size="28px" class="q-mr-sm" />
          <span class="text-h6 text-weight-regular">
            {{ isMobile ? 'Helpdesk' : 'Helpdesk Corporativo' }}
          </span>
        </q-btn>

        <q-space />

        <!-- Desktop: ações visíveis no cabeçalho -->
        <div v-if="!isMobile" class="row items-center q-gutter-sm no-wrap">
          <template v-if="auth.isAdmin">
            <q-btn
              v-for="link in navLinks"
              :key="link.label"
              flat
              no-caps
              :icon="link.icon"
              :label="link.label"
              :to="link.to"
            />
          </template>

          <q-chip
            color="white"
            text-color="primary"
            icon="person"
            class="text-weight-medium"
            :label="`${auth.user?.name} (${roleLabel})`"
          />

          <q-btn flat round :icon="themeIcon" @click="toggleDark">
            <q-tooltip>Alternar tema</q-tooltip>
          </q-btn>

          <q-btn flat no-caps icon="logout" label="Sair" @click="logout" />
        </div>

        <!-- Mobile: tudo dentro de um menu -->
        <q-btn v-else flat round icon="menu" aria-label="Menu">
          <q-menu anchor="bottom right" self="top right" :offset="[0, 8]">
            <q-list style="min-width: 240px">
              <q-item>
                <q-item-section avatar>
                  <q-avatar color="primary" text-color="white" icon="person" />
                </q-item-section>
                <q-item-section>
                  <q-item-label class="text-weight-medium">{{ auth.user?.name }}</q-item-label>
                  <q-item-label caption>{{ roleLabel }}</q-item-label>
                </q-item-section>
              </q-item>

              <q-separator />

              <template v-if="auth.isAdmin">
                <q-item
                  v-for="link in navLinks"
                  :key="link.label"
                  v-close-popup
                  clickable
                  :to="link.to"
                  exact
                >
                  <q-item-section avatar><q-icon :name="link.icon" /></q-item-section>
                  <q-item-section>{{ link.label }}</q-item-section>
                </q-item>
                <q-separator />
              </template>

              <q-item clickable @click="toggleDark">
                <q-item-section avatar><q-icon :name="themeIcon" /></q-item-section>
                <q-item-section>Modo escuro</q-item-section>
                <q-item-section side>
                  <q-toggle :model-value="$q.dark.isActive" dense @update:model-value="toggleDark" />
                </q-item-section>
              </q-item>

              <q-separator />

              <q-item v-close-popup clickable class="text-negative" @click="logout">
                <q-item-section avatar><q-icon name="logout" /></q-item-section>
                <q-item-section>Sair</q-item-section>
              </q-item>
            </q-list>
          </q-menu>
        </q-btn>
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
const isMobile = computed(() => $q.screen.lt.md)
const themeIcon = computed(() => ($q.dark.isActive ? 'light_mode' : 'dark_mode'))

const navLinks = [
  { label: 'Chamados', icon: 'confirmation_number', to: { name: 'tickets' } },
  { label: 'Cadastros', icon: 'tune', to: { name: 'catalog' } }
]

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
