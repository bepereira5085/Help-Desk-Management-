<template>
  <div class="login-page flex flex-center q-pa-md">
    <q-card class="login-card" flat bordered>
      <q-card-section class="text-center q-pt-xl">
        <q-avatar size="72px" color="primary" text-color="white" icon="support_agent" />
        <div class="text-h5 text-weight-bold q-mt-md">Helpdesk Corporativo</div>
        <div class="text-body2 text-grey-7">Entre para abrir e acompanhar seus chamados</div>
      </q-card-section>

      <q-form class="q-px-lg q-pb-lg q-gutter-y-md" @submit="submit">
        <q-input
          v-model.trim="form.login"
          outlined
          label="Login"
          autofocus
          autocomplete="username"
          :rules="[(v) => !!v || 'Informe o login']"
        >
          <template #prepend><q-icon name="person" /></template>
        </q-input>

        <q-input
          v-model="form.password"
          outlined
          label="Senha"
          :type="showPassword ? 'text' : 'password'"
          autocomplete="current-password"
          :rules="[(v) => !!v || 'Informe a senha']"
        >
          <template #prepend><q-icon name="lock" /></template>
          <template #append>
            <q-icon
              :name="showPassword ? 'visibility_off' : 'visibility'"
              class="cursor-pointer"
              @click="showPassword = !showPassword"
            />
          </template>
        </q-input>

        <q-btn
          type="submit"
          color="primary"
          unelevated
          no-caps
          size="lg"
          class="full-width"
          label="Entrar"
          :loading="loading"
        />
      </q-form>
    </q-card>
  </div>
</template>

<script setup>
import { reactive, ref } from 'vue'
import { useRouter } from 'vue-router'
import { useQuasar } from 'quasar'
import { useAuthStore } from '@/stores/auth'
import { apiErrorMessage } from '@/utils/ticketMeta'

const $q = useQuasar()
const router = useRouter()
const auth = useAuthStore()

const form = reactive({ login: '', password: '' })
const showPassword = ref(false)
const loading = ref(false)

async function submit() {
  loading.value = true
  try {
    await auth.login(form.login, form.password)
    $q.notify({ type: 'positive', message: `Bem-vindo(a), ${auth.user.name}!` })
    router.push({ name: 'tickets' })
  } catch (error) {
    $q.notify({
      type: 'negative',
      message: apiErrorMessage(error, 'Não foi possível conectar ao servidor.')
    })
  } finally {
    loading.value = false
  }
}
</script>

<style scoped>
.login-page {
  min-height: 100vh;
  background: linear-gradient(135deg, #1976d2 0%, #0d47a1 100%);
}

.login-card {
  width: 400px;
  max-width: 100%;
  border-radius: 16px;
}
</style>
