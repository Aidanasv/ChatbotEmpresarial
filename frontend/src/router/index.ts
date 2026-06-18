import { createRouter, createWebHistory } from 'vue-router'

import HomeView from '@/pages/Index.vue'
import LoginView from '@/pages/Login.vue'
import SetupView from '@/pages/Setup.vue'
import DashboardView from '@/pages/Dashboard.vue'
import AdminDashboardView from '@/pages/AdminDashboard.vue'
import MyChatbotView from '@/pages/MyChatbot.vue'
import ResetPasswordView from '@/pages/ResetPassword.vue'

import Conversations from '@/components/dashboard/Conversations.vue'
import Users from '@/components/dashboard/Users.vue'
import Personality from '@/components/dashboard/Personality.vue'
import TryChatbot from '@/components/dashboard/TryChatbot.vue'
import { useAuthStore } from '@/stores/useAuthStore'
import { useSetupStore } from '@/stores/useSetupStore'
import Apperearance from '@/components/dashboard/Apperearance.vue'
import Company from '@/components/dashboard/Company.vue'
import Knowledge from '@/components/dashboard/Knowledge.vue'
import HomeDashboard from '@/components/dashboard/HomeDashboard.vue'
import ViewDetailsCompany from '@/pages/ViewDetailsCompany.vue'
import AdminSubscriptionsView from '@/pages/AdminSubscriptions.vue'

const router = createRouter({
  history: createWebHistory(import.meta.env.BASE_URL),
  routes: [
    {
      path: '/',
      name: 'home',
      component: HomeView
    },
    {
      path: '/login',
      name: 'login',
      component: LoginView
    },
    {
      path: '/reset-password',
      name: 'reset-password',
      component: ResetPasswordView
    },
    {
      path: '/setup',
      name: 'setup',
      component: SetupView
    },
    {
      path: '/dashboard',
      component: DashboardView,
      redirect: () => {
        const authStore = useAuthStore()
        const role = (authStore.role || '').toLowerCase()
        return role === 'superadmin' ? '/dashboard/admin' : '/dashboard/panel'
      },
      beforeEnter: async () => {
        const authStore = useAuthStore()
        authStore.ensureTokenValidity()
        const role = (authStore.role || '').toLowerCase()
        if (!authStore.isAuthenticated) {
          return '/login'
        }

        if (role === 'superadmin') {
          return true
        }

        const setupStore = useSetupStore()
        if (authStore.companyId) {
          await setupStore.getSetupData(parseInt(authStore.companyId))
        } else {
          return '/setup'
        }
      },
      children: [
        {
          path: 'panel',
          name: 'dashboard-panel',
          component: HomeDashboard,
          meta: { title: 'Home' }
        },
        {
          path: 'admin',
          name: 'dashboard-admin',
          component: AdminDashboardView,
          meta: { title: 'Resumen global', subtitle: 'BIA Admin' }
        },
        {
          path: 'admin/subscriptions',
          name: 'dashboard-admin-subscriptions',
          component: AdminSubscriptionsView,
          meta: { title: 'Suscripciones', subtitle: 'BIA Admin' }
        },
        {
          path: 'subscriptions',
          name: 'dashboard-subscriptions',
          component: AdminSubscriptionsView,
          meta: { title: 'Suscripciones' }
        },
        {
          path: 'admin/company/:companyId',
          name: 'dashboard-admin-company-details',
          component: ViewDetailsCompany,
          meta: { title: 'Detalle de empresa', subtitle: 'BIA Admin' }
        },
        {
          path: 'try-chatbot',
          name: 'dashboard-try-chatbot',
          component: TryChatbot,
          meta: { title: 'Probar chatbot' }
        },
        {
          path: 'conversations',
          name: 'dashboard-conversations',
          component: Conversations,
          meta: { title: 'Conversaciones' }
        },
        {
          path: 'users',
          name: 'dashboard-users',
          component: Users,
          meta: { title: 'Usuarios' }
        },
        {
          path: 'personality',
          name: 'dashboard-personality',
          component: Personality,
          meta: { title: 'Personalidad' }
        },
        {
          path: 'appearance',
          name: 'dashboard-appearance',
          component: Apperearance,
          meta: { title: 'Apariencia' }
        },
        {
          path: 'company',
          name: 'dashboard-company',
          component: Company,
          meta: { title: 'Perfil' }
        },
        {
          path: 'knowledge',
          name: 'dashboard-knowledge',
          component: Knowledge,
          meta: { title: 'Conocimiento' }
        }
      ]
    },
    {
      path: '/my-chatbot/:chatbotId',
      name: 'my-chatbot',
      component: MyChatbotView,
      meta: { embed: true }
    }
  ],

  scrollBehavior() {
    return { top: 0 }
  }
})

router.beforeEach((to) => {
  const isMyChatbotRoute = to.path.startsWith('/my-chatbot/')
  const authStore = useAuthStore()

  if (!isMyChatbotRoute) {
    authStore.ensureTokenValidity()
  }

  if (!to.path.startsWith('/dashboard')) {
    return true
  }

  if (!authStore.isAuthenticated) {
    return '/login'
  }

  const role = (authStore.role || '').toLowerCase()
  const isDashboardAdminRoute = to.path === '/dashboard/admin' || to.path.startsWith('/dashboard/admin/')

  if (role === 'superadmin') {
    if (!isDashboardAdminRoute) {
      return '/dashboard/admin'
    }
    return true
  }

  if (isDashboardAdminRoute) {
    return '/dashboard/panel'
  }

  return true
})

export default router