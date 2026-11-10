import { defineBoot } from '#q-app'
import axios from 'axios'
import { useAuthStore } from '@/stores/auth'

// Em dev o Quasar faz proxy de /api para o backend (ver quasar.config.js -> devServer.proxy)
const api = axios.create({
  baseURL: import.meta.env.DEV ? '/api' : 'http://localhost:5193/api'
})

export default defineBoot(({ app, router, store }) => {
  const auth = useAuthStore(store)

  // Envia o token JWT em todas as requisições
  api.interceptors.request.use((config) => {
    if (auth.token) config.headers.Authorization = `Bearer ${auth.token}`
    return config
  })

  // Token expirado/inválido -> volta para o login
  api.interceptors.response.use(
    (response) => response,
    (error) => {
      const isLoginRequest = error.config?.url?.includes('/auth/login')
      if (error.response?.status === 401 && !isLoginRequest) {
        auth.logout()
        router.push({ name: 'login' })
      }
      return Promise.reject(error)
    }
  )

  app.config.globalProperties.$api = api
})

export { api }
