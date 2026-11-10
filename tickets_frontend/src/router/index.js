import { defineRouter } from '#q-app'
import { createRouter, createMemoryHistory, createWebHistory, createWebHashHistory } from 'vue-router'
import routes from './routes'
import { useAuthStore } from '@/stores/auth'

/*
 * If not building with SSR mode, you can
 * directly export the Router instantiation;
 *
 * The function below can be async too; either use
 * async/await or return a Promise which resolves
 * with the Router instance.
 */

export default defineRouter(function ({ store }) {
  const createHistory = import.meta.env.QUASAR_SERVER
    ? createMemoryHistory
    : (import.meta.env.QUASAR_VUE_ROUTER_MODE === 'history' ? createWebHistory : createWebHashHistory)

  const Router = createRouter({
    scrollBehavior: () => ({ left: 0, top: 0 }),
    routes,

    // Leave this as is and make changes in quasar.conf.js instead!
    // quasar.conf.js -> build -> vueRouterMode
    // quasar.conf.js -> build -> publicPath
    history: createHistory(import.meta.env.QUASAR_VUE_ROUTER_BASE)
  })

  // Proteção de rotas: exige login e, em algumas, perfil de administrador
  Router.beforeEach((to) => {
    const auth = useAuthStore(store)

    if (to.matched.some((r) => r.meta.requiresAuth) && !auth.isAuthenticated) {
      return { name: 'login' }
    }
    if (to.matched.some((r) => r.meta.guestOnly) && auth.isAuthenticated) {
      return { name: 'tickets' }
    }
    if (to.matched.some((r) => r.meta.requiresAdmin) && !auth.isAdmin) {
      return { name: 'tickets' }
    }
  })

  return Router
})
