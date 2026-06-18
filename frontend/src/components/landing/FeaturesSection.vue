<template>
  <section class="features-section pt-6 pb-8">
    <v-container>
      <div class="text-center mb-16">
        <h2 class="text-h3 font-weight-bold mb-4 section-title">
          Todo lo que necesitas
        </h2>
        <p class="text-h6 text-medium-emphasis font-weight-regular section-subtitle">
          Herramientas poderosas para crear el chatbot perfecto para tu negocio.
        </p>
      </div>

      <v-row>
        <v-col cols="12" sm="6" md="3" v-for="(feature, index) in features" :key="index">
          <v-card class="h-100 pa-6 rounded-xl text-left hover-card" elevation="1">
            <v-avatar size="56" class="mb-5 rounded-lg icon-box">
              <v-icon 
                :icon="feature.icon" 
                size="x-large" 
                color="primary" 
                class="feature-icon"
              ></v-icon>
            </v-avatar>

            <h3 class="text-h6 font-weight-bold mb-3">{{ feature.title }}</h3>
            <p class="text-body-1 text-medium-emphasis line-height-medium">
              {{ feature.description }}
            </p>
          </v-card>
        </v-col>
      </v-row>
    </v-container>
  </section>
</template>

<script setup lang="ts">
const features = [
  {
    icon: 'mdi-brain',
    title: 'IA Personalizada',
    description: 'Entrena tu chatbot con la información de tu empresa para respuestas precisas.'
  },
  {
    icon: 'mdi-palette-outline',
    title: 'Diseño a tu medida',
    description: 'Personaliza colores, estilo y tono de voz según tu marca.'
  },
  {
    icon: 'mdi-message-text-outline',
    title: 'Conversaciones naturales',
    description: 'Interacciones fluidas que tus clientes amarán.'
  },
  {
    icon: 'mdi-shield-check-outline',
    title: 'Seguro y confiable',
    description: 'Tus datos protegidos con los más altos estándares de seguridad.'
  }
]
</script>