<template>
  <v-app-bar flat class="app-navbar px-3 px-md-8">
    <template v-if="showDrawerToggle">
      <v-btn icon variant="text" class="mr-2" @click="uiStore.toggleDashboardDrawer()">
        <v-icon>mdi-menu</v-icon>
      </v-btn>

      <div class="d-flex align-center cursor-pointer navbar-header-logo" @click="handleGoHome">
        <v-avatar color="primary" size="30" class="mr-2">
          <v-icon size="16" color="white">mdi-message-processing</v-icon>
        </v-avatar>
        <span class="text-subtitle-2 font-weight-bold navbar-logo-text d-none d-md-inline">BotForge</span>
      </div>

      <v-spacer></v-spacer>
      <v-menu location="bottom end">
        <template #activator="{ props }">
          <v-btn variant="text" class="ml-2" v-bind="props" aria-label="Menu de usuario">
            <v-avatar color="primary" size="34" class="mr-2">
              <span class="text-caption font-weight-bold text-white">{{ logoLetter }}</span>
            </v-avatar>
            <div class="d-flex flex-column">
              <span class="text-subtitle-1 font-weight-bold navbar-logo-text">{{ companyBrandName }}</span>
              <span class="text-caption text-medium-emphasis navbar-brand-subtitle">Plataforma BotForge</span>
            </div>
          </v-btn>
        </template>

        <v-list density="compact" min-width="200">
          <v-list-item prepend-icon="mdi-account" :title="companyBrandName"></v-list-item>
          <v-divider class="my-1"></v-divider>
          <v-list-item prepend-icon="mdi-view-dashboard" title="Dashboard" @click="router.push('/dashboard')"></v-list-item>
          <v-list-item prepend-icon="mdi-logout" title="Cerrar sesión" @click="handleLogout"></v-list-item>
          <v-list-item prepend-icon="mdi-help-circle-outline" title="Ayuda" @click="router.push('/help')"></v-list-item>
        </v-list>
      </v-menu>


    </template>

    <template v-else>
      <div class="d-flex align-center cursor-pointer navbar-brand" @click="handleGoHome">
        <v-avatar color="primary" size="34" class="mr-2">
          <span class="text-caption font-weight-bold text-white">{{ logoLetter }}</span>
        </v-avatar>
        <div class="d-flex flex-column">
          <span class="text-subtitle-1 font-weight-bold navbar-logo-text">{{ companyBrandName }}</span>
          <span class="text-caption text-medium-emphasis navbar-brand-subtitle">Plataforma BotForge</span>
        </div>
      </div>

      <v-spacer></v-spacer>

      <div v-if="!authStore.isAuthenticated" class="d-none d-md-flex align-center">
        <v-btn variant="text" class="navbar-link">Características</v-btn>
        <v-btn variant="text" class="navbar-link">Planes</v-btn>
        <v-btn to="/login?mode=signin" variant="text" class="navbar-link font-weight-bold ml-2">Iniciar sesión</v-btn>
        <v-btn color="primary" elevation="0" rounded="lg" class="text-none px-6 ml-4 font-weight-bold"
          to="/login?mode=signup">
          Comenzar gratis
        </v-btn>
      </div>

      <div v-else class="d-none d-md-flex align-center">
        <v-btn to="/dashboard" variant="text" class="navbar-link font-weight-bold">Dashboard</v-btn>
        <v-btn color="primary" elevation="0" rounded="lg" class="text-none px-6 ml-4 font-weight-bold"
          @click="handleLogout">
          Cerrar sesión
        </v-btn>
      </div>

      <div v-if="!authStore.isAuthenticated" class="d-flex d-md-none align-center">
        <v-btn icon to="/login?mode=signin">
          <v-icon>mdi-account</v-icon>
        </v-btn>
      </div>

      <div v-else class="d-flex d-md-none align-center">
        <v-btn icon to="/dashboard">
          <v-icon>mdi-view-dashboard</v-icon>
        </v-btn>
        <v-btn icon @click="handleLogout">
          <v-icon>mdi-logout</v-icon>
        </v-btn>
      </div>
    </template>
  </v-app-bar>
</template>

<script setup lang="ts">
import { computed, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { useRoute } from 'vue-router'
import { useAuthStore } from '@/stores/useAuthStore'
import { useSetupStore } from '@/stores/useSetupStore'
import { useUiStore } from '@/stores/useUiStore'

const router = useRouter()
const route = useRoute()
const authStore = useAuthStore()
const setupStore = useSetupStore()
const uiStore = useUiStore()

const isDashboardRoute = computed(() => route.path.startsWith('/dashboard'))
const showDrawerToggle = computed(() => authStore.isAuthenticated && isDashboardRoute.value)

const companyBrandName = computed(() => {
  if (!authStore.isAuthenticated) {
    return 'BotForge'
  }

  const configuredCompanyName = setupStore.companySetup.companyName?.trim()
  if (configuredCompanyName) {
    return configuredCompanyName
  }

  if ((authStore.role || '').toLowerCase() === 'superadmin') {
    return 'BotForge Admin'
  }

  return 'Mi Empresa'
})

const logoLetter = computed(() => companyBrandName.value.charAt(0).toUpperCase())

const handleGoHome = () => {
  if (authStore.isAuthenticated) {
    router.push('/dashboard')
    return
  }

  router.push('/')
}

onMounted(async () => {
  const hasCompanyId = !!authStore.companyId
  const hasCompanyName = !!setupStore.companySetup.companyName
  const isSuperAdmin = (authStore.role || '').toLowerCase() === 'superadmin'

  if (!authStore.isAuthenticated || !hasCompanyId || hasCompanyName || isSuperAdmin) {
    return
  }

  try {
    await setupStore.getSetupData(parseInt(authStore.companyId || '0', 10))
  } catch (error) {
    console.error('Error loading company brand in navbar:', error)
  }
})

const handleLogout = () => {
  authStore.logout()
  uiStore.setDashboardDrawer(true)
  router.push('/')
}
</script>