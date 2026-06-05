import { createRouter, createWebHistory } from 'vue-router'

import HomeView from '@/pages/Index.vue'
import LoginView from '@/pages/Login.vue'
import SetupView from '@/pages/Setup.vue'
import DashboardView from '@/pages/Dashboard.vue'

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
      path: '/setup',
      name: 'setup',
      component: SetupView
    },
    {
      path: '/dashboard',
      component: DashboardView,
      redirect: '/dashboard/panel',
      beforeEnter: async () => {
        const authStore = useAuthStore()
        if (!authStore.isAuthenticated) {
          return '/login'
        }
        const setupStore = useSetupStore()
        if (authStore.companyId) {
          await setupStore.getSetupData(parseInt(authStore.companyId))
        }else {
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
    }
  ],

  scrollBehavior() {
    return { top: 0 }
  }
})

export default router