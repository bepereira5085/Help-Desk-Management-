import { defineStore } from 'pinia'
import { computed, ref } from 'vue'
import { api } from '@/boot/axios'

const STORAGE_KEY = 'helpdesk-auth'

function readStorage() {
  try {
    return JSON.parse(localStorage.getItem(STORAGE_KEY)) || {}
  } catch {
    return {}
  }
}

export const useAuthStore = defineStore('auth', () => {
  const saved = readStorage()
  const token = ref(saved.token || null)
  const user = ref(saved.user || null)

  const isAuthenticated = computed(() => !!token.value)
  const isAdmin = computed(() => !!user.value?.isAdmin)

  function persist() {
    if (token.value) {
      localStorage.setItem(STORAGE_KEY, JSON.stringify({ token: token.value, user: user.value }))
    } else {
      localStorage.removeItem(STORAGE_KEY)
    }
  }

  async function login(loginName, password) {
    const { data } = await api.post('/auth/login', { login: loginName, password })
    token.value = data.token
    user.value = data.user
    persist()
  }

  function logout() {
    token.value = null
    user.value = null
    persist()
  }

  return { token, user, isAuthenticated, isAdmin, login, logout }
})
