const routes = [
  {
    path: '/login',
    name: 'login',
    component: () => import('@/pages/LoginPage.vue'),
    meta: { guestOnly: true }
  },
  {
    path: '/',
    component: () => import('@/layouts/MainLayout.vue'),
    meta: { requiresAuth: true },
    children: [
      {
        path: '',
        component: () => import('@/pages/TicketsListPage.vue'),
        // Diálogos abertos por rota, sobre a listagem
        children: [
          {
            path: '',
            name: 'tickets',
            component: { render: () => null },
            meta: { keepScroll: true }
          },
          {
            path: 'tickets/new',
            name: 'ticket-new',
            component: () => import('@/components/TicketInfoDialog.vue'),
            meta: { keepScroll: true }
          },
          {
            path: 'tickets/:id',
            name: 'ticket-details',
            component: () => import('@/components/TicketDetailsDialog.vue'),
            meta: { keepScroll: true }
          },
          {
            path: 'tickets/:id/edit',
            name: 'ticket-edit',
            component: () => import('@/components/TicketInfoDialog.vue'),
            meta: { keepScroll: true }
          }
        ]
      },
      {
        path: 'admin/catalog',
        name: 'catalog',
        component: () => import('@/pages/CatalogPage.vue'),
        meta: { requiresAdmin: true }
      }
    ]
  },

  // Always leave this as last one,
  // but you can also remove it
  {
    path: '/:catchAll(.*)*',
    component: () => import('@/pages/ErrorNotFound.vue')
  }
]

export default routes
