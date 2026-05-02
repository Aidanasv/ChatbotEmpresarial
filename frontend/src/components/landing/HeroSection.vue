<template>
  <v-container class="py-16 hero-container">
    <v-row align="center">
      <v-col cols="12" md="6" class="pr-md-10">
        <v-chip variant="tonal" class="mb-6 px-4 py-2 font-weight-medium hero-chip">
          <v-icon start size="small">mdi-creation</v-icon>
          Potenciado con IA
        </v-chip>

        <h1 class="text-h3 text-sm-h2 font-weight-black mb-4 hero-title">
          Tu chatbot, <span class="brand-highlight">tu marca,</span><br> tu estilo
        </h1>

        <p class="text-h6 text-medium-emphasis mb-8 font-weight-regular hero-description">
          Crea un asistente virtual inteligente personalizado para tu empresa. Configura su conocimiento,
          apariencia y personalidad en minutos.
        </p>

        </v-col>

      <v-col cols="12" md="6">
        <v-sheet 
          height="450" 
          rounded="xl"
          class="d-flex flex-column align-center justify-center elevation-10 hero-illustration-box"
        >
          <v-icon size="80" class="mb-4 robot-icon">mdi-robot-outline</v-icon>
          <span class="illustration-text">Aquí irá tu ilustración 3D</span>
        </v-sheet>
      </v-col>
    </v-row>
  </v-container>
</template>

<script setup lang="ts">
</script>