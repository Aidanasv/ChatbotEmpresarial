<template>
  <div class="relative-wrapper">
    <div class="bg-blur-glow-1"></div>
    <div class="bg-blur-glow-2"></div>

    <v-container class="pt-12 pb-6 hero-container">
      <v-row align="center">
        <v-col cols="12" md="6" class="pr-md-10 text-left">
          <v-chip variant="tonal" color="primary" class="mb-6 px-4 py-2 font-weight-medium hero-chip">
            <v-icon start size="small">mdi-creation</v-icon>
            Potenciado con IA
          </v-chip>

          <h1 class="text-h3 text-sm-h2 font-weight-black mb-4 hero-title">
            Tu chatbot, <span class="brand-highlight">tu marca,</span><br> tu estilo
          </h1>

          <p class="text-h6 text-medium-emphasis mb-6 font-weight-regular hero-description">
            Crea un asistente virtual inteligente personalizado para tu empresa. Configura su conocimiento,
            apariencia y personalidad en minutos.
          </p>

          <div class="d-flex flex-column mb-8 benefits-list">
            <div v-for="benefit in benefits" :key="benefit" class="d-flex align-center text-body-1 text-medium-emphasis py-1">
              <v-icon color="success" class="mr-2" size="20">mdi-check-circle</v-icon>
              <span>{{ benefit }}</span>
            </div>
          </div>

          <div class="d-flex flex-wrap ga-4 mt-6">
            <v-btn color="primary" size="large" rounded="lg" class="px-6 text-none font-weight-bold elevation-2" to="/login?mode=signup">
              Comenzar gratis
              <v-icon end>mdi-arrow-right</v-icon>
            </v-btn>
            <v-btn variant="outlined" color="primary" size="large" rounded="lg" class="px-6 text-none font-weight-bold" to="/login">
              Iniciar sesión
            </v-btn>
          </div>
        </v-col>

        <v-col cols="12" md="6" class="mt-12 mt-md-0 d-flex justify-center align-center">
          <AnimationHome />
        </v-col>
      </v-row>
    </v-container>
  </div>
</template>

<script setup lang="ts">
import AnimationHome from './AnimationHome.vue'

const benefits = [
  'Sin código necesario',
  'Listo en minutos',
  'Soporte 24/7 automático',
]
</script>