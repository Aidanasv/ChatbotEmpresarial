<template>
  <v-layout class="dashboard-layout">
    <v-navigation-drawer v-model="uiStore.dashboardDrawerOpen" :temporary="isMobile" width="260" elevation="0">
      <v-list density="compact" nav class="px-3 pt-4">
        <v-list-subheader class="text-caption font-weight-bold text-uppercase">General</v-list-subheader>

        <template v-if="isSuperAdmin">
          <v-list-item to="/dashboard/admin" prepend-icon="mdi-view-grid-outline" title="Resumen global" rounded="lg"
            color="primary"></v-list-item>
          <v-list-item to="/dashboard/admin/subscriptions" prepend-icon="mdi-credit-card-outline" title="Suscripciones"
            rounded="lg" color="primary"></v-list-item>
        </template>

        <template v-else>
          <v-list-item to="/dashboard/panel" prepend-icon="mdi-view-dashboard" title="Home" rounded="lg"
            color="primary"></v-list-item>
          <v-list-item to="/dashboard/subscriptions" prepend-icon="mdi-credit-card-outline" title="Suscripciones"
            rounded="lg" color="primary"></v-list-item>
          <v-list-item to="/dashboard/try-chatbot" prepend-icon="mdi-play-circle-outline" title="Probar chatbot"
            rounded="lg" color="primary"></v-list-item>
          <v-list-item to="/dashboard/conversations" prepend-icon="mdi-message-text-outline" title="Conversaciones"
            rounded="lg" color="primary"></v-list-item>
          <v-list-item to="/dashboard/users" prepend-icon="mdi-account-multiple-outline" title="Usuarios" rounded="lg"
            color="primary"></v-list-item>

          <v-divider class="my-4"></v-divider>

          <v-list-subheader class="text-caption font-weight-bold text-uppercase">Configuración</v-list-subheader>

          <v-list-item to="/dashboard/knowledge" prepend-icon="mdi-book-open-variant" title="Conocimiento" rounded="lg"
            color="primary"></v-list-item>
          <v-list-item to="/dashboard/personality" prepend-icon="mdi-account-voice" title="Personalidad" rounded="lg"
            color="primary"></v-list-item>
          <v-list-item to="/dashboard/appearance" prepend-icon="mdi-palette-outline" title="Apariencia" rounded="lg"
            color="primary"></v-list-item>
          <v-list-item to="/dashboard/company" prepend-icon="mdi-cog-outline" title="Perfil" rounded="lg"
            color="primary"></v-list-item>
        </template>
      </v-list>

    </v-navigation-drawer>

    <v-main>
      <router-view></router-view>
    </v-main>
  </v-layout>
</template>

<script setup lang="ts">
import { computed, watch } from 'vue'
import { useAuthStore } from '@/stores/useAuthStore'
import { useDisplay } from 'vuetify'
import { useUiStore } from '@/stores/useUiStore'

const authStore = useAuthStore()
const uiStore = useUiStore()
const { mdAndDown } = useDisplay()

const isMobile = computed(() => mdAndDown.value)

watch(isMobile, (mobile) => {
  uiStore.setDashboardDrawer(!mobile)
}, { immediate: true })

const normalizedRole = computed(() => (authStore.role || '').toLowerCase())
const isSuperAdmin = computed(() => normalizedRole.value === 'superadmin')

</script>