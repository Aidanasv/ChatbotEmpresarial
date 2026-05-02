<template>
  <v-app-bar flat class="app-navbar px-4 px-md-10">
    <div class="d-flex align-center cursor-pointer" @click="$router.push('/')">
      <v-icon color="primary" size="x-large" class="mr-2">mdi-message-processing</v-icon>
      <span class="text-h6 font-weight-bold navbar-logo-text">BotForge</span>
    </div>

    <v-spacer></v-spacer>

    <div class="d-none d-md-flex align-center">
      <v-btn variant="text" class="navbar-link">Características</v-btn>
      <v-btn variant="text" class="navbar-link">Planes</v-btn>
      <v-btn to="/login?mode=signin" variant="text" class="navbar-link font-weight-bold ml-2">Iniciar sesión</v-btn>
      <v-btn color="primary" elevation="0" rounded="lg" class="text-none px-6 ml-4 font-weight-bold" to="/login?mode=signup">
        Comenzar gratis
      </v-btn>
    </div>

    <v-btn icon class="d-flex d-md-none">
      <v-icon>mdi-menu</v-icon>
    </v-btn>
  </v-app-bar>
</template>

<script setup lang="ts">

</script>