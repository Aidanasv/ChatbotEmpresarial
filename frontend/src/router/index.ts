import { createRouter, createWebHistory } from 'vue-router'

import HomeView from '@/pages/Index.vue'
import LoginView from '@/pages/Login.vue'
import SetupView from '@/pages/Setup.vue'
import DashboardView from '@/pages/Dashboard.vue'

import Panel from '@/components/dashboard/Panel.vue'
import Conversations from '@/components/dashboard/Conversations.vue'
import Analytics from '@/components/dashboard/Analytics.vue'
import Users from '@/components/dashboard/Users.vue'
import Personality from '@/components/dashboard/Personality.vue'
import { useAuthStore } from '@/stores/useAuthStore'
import { useSetupStore } from '@/stores/useSetupStore'
import App from '@/App.vue'
import Apperearance from '@/components/dashboard/Apperearance.vue'
import Company from '@/components/dashboard/Company.vue'
import Knowledge from '@/components/dashboard/Knowledge.vue'

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
        await setupStore.getSetupData(authStore.companyId)
      },
      children: [
        {
          path: 'panel',
          name: 'dashboard-panel',
          component: Panel
        },
        {
          path: 'conversations',
          name: 'dashboard-conversations',
          component: Conversations
        },
        {
          path: 'analytics',
          name: 'dashboard-analytics',
          component: Analytics
        },
        {
          path: 'users',
          name: 'dashboard-users',
          component: Users
        },
        {
          path: 'personality',
          name: 'dashboard-personality',
          component: Personality
        },
        {
          path: 'appearance',
          name: 'dashboard-appearance',
          component: Apperearance
        },
        {
          path: 'company',
          name: 'dashboard-company',
          component: Company
        },
        {
          path: 'knowledge',
          name: 'dashboard-knowledge',
          component: Knowledge,
        }
      ]
    }
  ],
  
  scrollBehavior() {
    return { top: 0 }
  }
})

export default router