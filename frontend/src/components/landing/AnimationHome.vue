<template>
  <div class="illustration-container">
    <div class="glow-bg"></div>

    <v-card class="chat-card elevation-12 rounded-xl pa-5 position-relative">
      <div class="d-flex align-center mb-4">
        <v-avatar color="primary" size="40" class="mr-3 elevation-1">
          <v-icon color="white" size="22">mdi-robot</v-icon>
        </v-avatar>
        <div class="text-left">
          <p class="text-subtitle-2 font-weight-bold mb-0">Asistente BotForge</p>
          <div class="d-flex align-center">
            <span class="status-indicator mr-1"></span>
            <span class="text-caption text-medium-emphasis">En línea</span>
          </div>
        </div>
      </div>

      <div class="d-flex flex-column gap-3">
        <div class="msg-bubble msg-received pa-3 text-body-2">
          ¡Hola! ¿En qué puedo ayudarte hoy?
        </div>
        <div class="msg-bubble msg-sent pa-3 text-body-2 align-self-end">
          Quiero crear un chatbot para mi tienda
        </div>
        <div class="msg-bubble msg-received pa-3 text-body-2">
          Claro, puedes empezar gratis en minutos. ¿Qué tipo de productos vendes?
        </div>
      </div>
    </v-card>

    <v-card class="floating-card stat-card-1 elevation-4 px-2 py-1 rounded-xl">
      <div class="stat-card-body">
        <div class="stat-card-top">
          <v-avatar color="success-lighten-5" size="30" rounded="lg">
            <v-icon color="success" size="16">mdi-flash</v-icon>
          </v-avatar>
          <span class="stat-value">1.2s</span>
        </div>
        <p class="stat-label text-caption text-medium-emphasis mb-0">Respuesta</p>
      </div>
    </v-card>

    <v-card class="floating-card stat-card-2 elevation-4 px-2 py-1 rounded-xl">
      <div class="stat-card-body">
        <div class="stat-card-top">
          <v-avatar color="primary-lighten-5" size="30" rounded="lg">
            <v-icon color="primary" size="16">mdi-chat-processing-outline</v-icon>
          </v-avatar>
          <span class="stat-value">1,284</span>
        </div>
        <p class="stat-label text-caption text-medium-emphasis mb-0">Mensajes</p>
      </div>
    </v-card>

    <v-card class="floating-card stat-card-3 elevation-4 px-2 py-1 rounded-xl">
      <div class="stat-card-body">
        <div class="stat-card-top">
          <v-avatar color="amber-lighten-5" size="30" rounded="lg">
            <v-icon color="amber-darken-2" size="16">mdi-shield-check-outline</v-icon>
          </v-avatar>
          <span class="stat-value">99.9%</span>
        </div>
        <p class="stat-label text-caption text-medium-emphasis mb-0">Uptime</p>
      </div>
    </v-card>
  </div>
</template>

<script setup lang="ts">
</script>
