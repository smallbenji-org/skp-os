import { createRouter, createWebHistory } from 'vue-router'
import { useAuthStore } from '@/Stores/AuthStore'

const router = createRouter({
  history: createWebHistory(),
  routes: [
    {
      path: '/login',
      name: 'login',
      component: () => import('@/views/AuthView.vue'),
      meta: { public: true, authMode: 'login' },
    },
    {
      path: '/register',
      name: 'register',
      component: () => import('@/views/AuthView.vue'),
      meta: { public: true, authMode: 'register' },
    },
    {
      path: '/',
      component: () => import('@/layouts/MainLayout.vue'),
      meta: { requiresAuth: true },
      children: [
        {
          path: '',
          name: 'forside',
          component: () => import('@/views/HomeView.vue'),
        },
        {
          path: 'projekter',
          name: 'projekter',
          component: () => import('@/views/ProjectsView.vue'),
        },
        {
          path: 'skp-projekter',
          name: 'skp-projekter',
          component: () => import('@/views/SKPProjectsView.vue'),
        },
        {
          path: 'logbog',
          name: 'logbog',
          component: () => import('@/views/LogView.vue'),
        },
        {
          path: 'ff',
          name: 'ff',
          component: () => import('@/views/FFView.vue'),
        },
        {
          path: 'location',
          name: 'location',
          component: () => import('@/views/LocationView.vue'),
        },
        {
          path: 'meddelelser',
          name: 'meddelelser',
          component: () => import('@/views/MessagesView.vue'),
        },
        {
          path: 'info',
          name: 'info',
          component: () => import('@/views/InfoView.vue'),
        },
        {
          path: 'hjaelp',
          name: 'hjaelp',
          component: () => import('@/views/HelpView.vue'),
        },
        {
          path: 'indstillinger',
          name: 'indstillinger',
          component: () => import('@/views/SettingsView.vue'),
        },
      ],
    },
    {
      path: '/:pathMatch(.*)*',
      redirect: '/',
    },
  ],
})

router.beforeEach(async (to) => {
  const authStore = useAuthStore()

  if (to.meta.public) {
    if (authStore.IS_AUTHENTICATED) {
      return { name: 'forside' }
    }
    return true
  }

  if (!authStore.IS_AUTHENTICATED) {
    try {
      await authStore.GET_ME()
    } catch {
      return { name: 'login' }
    }
  }

  if (!authStore.IS_AUTHENTICATED) {
    return { name: 'login' }
  }

  if (to.meta.roles && authStore.IS_AUTHENTICATED) {
    const roles = to.meta.roles as string[]
    if (!authStore.HAS_ANY_ROLE(roles)) {
      return { name: 'forside' }
    }
  }

  return true
})

export default router
