<template>
  <v-container class="pt-8 pb-16 cta-container">
    <v-sheet rounded="xl" class="cta-banner pa-10 pa-md-16 text-center elevation-10">
      <v-row justify="center">
        <v-col cols="12" md="8">
          <h2 class="text-h4 text-md-h3 font-weight-bold mb-4 cta-title">
            ¿Listo para transformar tu atención al cliente?
          </h2>

          <p class="text-h6 font-weight-regular mb-8 cta-subtitle">
            Únete a cientos de empresas que ya usan BotForge para automatizar sus conversaciones.
          </p>

          <v-btn size="x-large" rounded="lg" elevation="2" class="text-none font-weight-bold px-8 cta-btn"
            to="/register">
            Crear mi chatbot ahora
            <v-icon end>mdi-arrow-right</v-icon>
          </v-btn>
        </v-col>
      </v-row>
    </v-sheet>
  </v-container>
</template>

<script setup lang="ts">
</script>