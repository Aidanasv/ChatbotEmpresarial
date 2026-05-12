<template>
  <v-app-bar flat class="app-navbar px-4 px-md-10">
    <div class="d-flex align-center cursor-pointer" @click="$router.push('/')">
      <v-icon color="primary" size="x-large" class="mr-2">mdi-message-processing</v-icon>
      <span class="text-h6 font-weight-bold navbar-logo-text">BotForge</span>
    </div>

    <v-spacer></v-spacer>

    <div v-if="!authStore.isAuthenticated" class="d-none d-md-flex align-center">
      <v-btn variant="text" class="navbar-link">Características</v-btn>
      <v-btn variant="text" class="navbar-link">Planes</v-btn>
      <v-btn to="/login?mode=signin" variant="text" class="navbar-link font-weight-bold ml-2">Iniciar sesión</v-btn>
      <v-btn color="primary" elevation="0" rounded="lg" class="text-none px-6 ml-4 font-weight-bold" to="/login?mode=signup">
        Comenzar gratis
      </v-btn>
    </div>

    <div v-else class="d-none d-md-flex align-center">
      <v-btn to="/dashboard" variant="text" class="navbar-link font-weight-bold">Dashboard</v-btn>
      <v-btn color="primary" elevation="0" rounded="lg" class="text-none px-6 ml-4 font-weight-bold" @click="handleLogout">
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
  </v-app-bar>
</template>

<script setup lang="ts">
import { useRouter } from 'vue-router'
import { useAuthStore } from '@/stores/useAuthStore'

const router = useRouter()
const authStore = useAuthStore()

const handleLogout = () => {
  authStore.logout()
  router.push('/')
}
</script>