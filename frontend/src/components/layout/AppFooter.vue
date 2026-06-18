<template>
  <v-footer class="app-footer py-4 px-4 px-md-8 d-flex justify-space-between align-center">
    <div class="d-flex align-center ga-2">
      <v-avatar color="primary" size="24">
        <span class="text-caption font-weight-bold text-white">{{ logoLetter }}</span>
      </v-avatar>
      <div class="footer-text">{{ companyBrandName }}</div>
    </div>
    <div class="footer-text text-medium-emphasis">
      © 2026 BIA. Todos los derechos reservados.
    </div>
  </v-footer>
</template>

<script setup lang="ts">
import { computed } from 'vue'
import { useAuthStore } from '@/stores/useAuthStore'
import { useSetupStore } from '@/stores/useSetupStore'

const authStore = useAuthStore()
const setupStore = useSetupStore()

const companyBrandName = computed(() => {
  if (!authStore.isAuthenticated) {
    return 'BIA'
  }

  const configuredCompanyName = setupStore.companySetup.companyName?.trim()
  if (configuredCompanyName) {
    return configuredCompanyName
  }

  if ((authStore.role || '').toLowerCase() === 'superadmin') {
    return 'BIA Admin'
  }

  return 'Mi Empresa'
})

const logoLetter = computed(() => companyBrandName.value.charAt(0).toUpperCase())
</script>