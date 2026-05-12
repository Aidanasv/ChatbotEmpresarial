<template>
  <v-layout class="dashboard-layout">
    <v-navigation-drawer permanent width="260" elevation="0">
      <div class="pa-5 d-flex align-center mb-2">
        <v-icon color="primary" size="x-large" class="mr-2">mdi-message-processing</v-icon>
        <span class="text-h6 font-weight-bold">BotForge</span>
      </div>

      <v-list density="compact" nav class="px-3">
        <v-list-subheader class="text-caption font-weight-bold text-uppercase">General</v-list-subheader>
        
        <v-list-item to="/dashboard/panel" prepend-icon="mdi-view-dashboard" title="Panel" rounded="lg" color="primary"></v-list-item>
        <v-list-item to="/dashboard/conversations" prepend-icon="mdi-message-text-outline" title="Conversaciones" rounded="lg" color="primary"></v-list-item>
        <v-list-item to="/dashboard/analytics" prepend-icon="mdi-chart-bar" title="Analíticas" rounded="lg" color="primary"></v-list-item>
        <v-list-item to="/dashboard/users" prepend-icon="mdi-account-multiple-outline" title="Usuarios" rounded="lg" color="primary"></v-list-item>

        <v-divider class="my-4"></v-divider>

        <v-list-subheader class="text-caption font-weight-bold text-uppercase">Configuración</v-list-subheader>
        
        <v-list-item to="/dashboard/knowledge" prepend-icon="mdi-book-open-variant" title="Conocimiento" rounded="lg" color="primary"></v-list-item>
        <v-list-item to="/dashboard/personality" prepend-icon="mdi-account-voice" title="Personalidad" rounded="lg" color="primary"></v-list-item>
        <v-list-item to="/dashboard/appearance" prepend-icon="mdi-palette-outline" title="Apariencia" rounded="lg" color="primary"></v-list-item>
        <v-list-item to="/dashboard/company" prepend-icon="mdi-cog-outline" title="Perfil" rounded="lg" color="primary"></v-list-item>
      </v-list>

      <template v-slot:append>
        <div class="pa-3">
          <v-list density="compact" nav>
            <v-list-item prepend-icon="mdi-help-circle-outline" title="Ayuda" value="ayuda" rounded="lg"></v-list-item>
            <v-list-item prepend-icon="mdi-logout" title="Cerrar sesión" value="logout" rounded="lg"></v-list-item>
          </v-list>
        </div>
      </template>
    </v-navigation-drawer>

    <v-app-bar flat elevation="0" height="70">
      <div class="ml-6">
        <div class="text-subtitle-1 font-weight-bold line-height-1">Panel de control</div>
        <div class="text-caption text-medium-emphasis">Asistente Acme</div>
      </div>

      <v-spacer></v-spacer>

      <v-chip color="success" variant="flat" size="small" class="mr-6 font-weight-bold bg-green-lighten-5 text-green-darken-2">
        <v-icon start size="x-small">mdi-circle</v-icon>
        En línea
      </v-chip>
    </v-app-bar>

    <v-main>
      <router-view></router-view>
    </v-main>
  </v-layout>
</template>

<script setup lang="ts">

</script>