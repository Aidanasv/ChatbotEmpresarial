<template>
  <v-container fluid class="pa-6 pa-md-8">
    <div class="mb-8">
      <h1 class="text-h4 font-weight-bold mb-2">Bienvenido de vuelta</h1>
      <p class="text-medium-emphasis">Aquí tienes un resumen de la actividad de tu chatbot</p>
    </div>

    <v-row class="mb-6">
      <v-col cols="12" sm="6" md="3" v-for="(kpi, i) in kpis" :key="i">
        <v-card class="dash-card pa-5 h-100" flat>
          <div class="d-flex justify-space-between align-start mb-4">
            <div class="kpi-icon-box">
              <v-icon :icon="kpi.icon" size="large"></v-icon>
            </div>
            <span :class="['text-caption font-weight-bold d-flex align-center', kpi.trendUp ? 'text-success' : 'text-error']">
              {{ kpi.trend }} <v-icon size="small" class="ml-1">{{ kpi.trendUp ? 'mdi-trending-up' : 'mdi-trending-down' }}</v-icon>
            </span>
          </div>
          <div class="text-h4 font-weight-bold mb-1">{{ kpi.value }}</div>
          <div class="text-caption text-medium-emphasis">{{ kpi.title }}</div>
        </v-card>
      </v-col>
    </v-row>

    <v-row>
      <v-col cols="12" md="8">
        <v-card class="dash-card fill-height" flat>
          <div class="pa-5 border-bottom d-flex justify-space-between align-center">
            <div>
              <h2 class="text-h6 font-weight-bold">Conversaciones recientes</h2>
              <p class="text-caption text-medium-emphasis mb-0">Últimas interacciones con tu chatbot</p>
            </div>
            <v-btn to="/dashboard/conversations" variant="text" size="small" color="primary" class="text-none font-weight-bold" append-icon="mdi-arrow-top-right">
              Ver todas
            </v-btn>
          </div>

          <v-list lines="two" class="pa-0">
            <template v-for="(chat, i) in recentChats" :key="i">
              <v-list-item class="px-5 py-3">
                <template v-slot:prepend>
                  <v-avatar color="primary" variant="tonal" size="40" class="mr-4 font-weight-bold">
                    {{ chat.initial }}
                  </v-avatar>
                </template>
                <v-list-item-title class="font-weight-bold text-body-2">{{ chat.name }}</v-list-item-title>
                <v-list-item-subtitle class="text-caption mt-1">{{ chat.message }}</v-list-item-subtitle>
                
                <template v-slot:append>
                  <div class="d-flex flex-column align-end">
                    <span :class="['text-caption font-weight-bold mb-1 d-flex align-center', chat.status === 'Resuelto' ? 'text-success' : 'text-warning']">
                      <v-icon size="x-small" class="mr-1">{{ chat.status === 'Resuelto' ? 'mdi-check-circle-outline' : 'mdi-alert-circle-outline' }}</v-icon>
                      {{ chat.status }}
                    </span>
                    <span class="text-caption text-medium-emphasis">{{ chat.time }}</span>
                  </div>
                </template>
              </v-list-item>
              <v-divider v-if="i !== recentChats.length - 1" class="border-bottom"></v-divider>
            </template>
          </v-list>
        </v-card>
      </v-col>

      <v-col cols="12" md="4">
        <v-card class="dash-card fill-height pa-5" flat>
          <h2 class="text-h6 font-weight-bold mb-1">Acciones rápidas</h2>
          <p class="text-caption text-medium-emphasis mb-5">Gestiona tu chatbot</p>

          <div class="d-flex flex-column" style="gap: 12px;">
            <v-card variant="outlined" class="pa-4 d-flex align-center cursor-pointer rounded-lg dash-card" v-for="(action, i) in actions" :key="i">
              <v-avatar color="grey-lighten-4" size="40" class="mr-4">
                <v-icon :icon="action.icon" size="small" color="grey-darken-2"></v-icon>
              </v-avatar>
              <div>
                <div class="text-body-2 font-weight-bold">{{ action.title }}</div>
                <div class="text-caption text-medium-emphasis">{{ action.subtitle }}</div>
              </div>
            </v-card>
          </div>
        </v-card>
      </v-col>
    </v-row>
  </v-container>
</template>

<script setup lang="ts">
// Mocks
const kpis = [
  { title: 'Conversaciones', value: '1,284', trend: '+12.5%', trendUp: true, icon: 'mdi-message-outline' },
  { title: 'Usuarios activos', value: '342', trend: '+8.2%', trendUp: true, icon: 'mdi-account-group-outline' },
  { title: 'Tasa de resolución', value: '94.2%', trend: '+3.1%', trendUp: true, icon: 'mdi-check-circle-outline' },
  { title: 'Tiempo medio resp.', value: '1.2s', trend: '-0.3s', trendUp: false, icon: 'mdi-clock-outline' }
]

const recentChats = [
  { initial: 'C', name: 'Carlos M.', message: '¿Cuál es el horario de atención?', status: 'Resuelto', time: 'Hace 5 min' },
  { initial: 'A', name: 'Ana L.', message: 'Necesito información sobre precios', status: 'Resuelto', time: 'Hace 12 min' },
  { initial: 'P', name: 'Pedro R.', message: 'Tengo un problema con mi pedido #4521', status: 'Escalado', time: 'Hace 18 min' },
  { initial: 'M', name: 'María G.', message: '¿Cómo puedo cambiar mi contraseña?', status: 'Resuelto', time: 'Hace 25 min' },
  { initial: 'L', name: 'Luis S.', message: 'Quiero hablar con un agente', status: 'Escalado', time: 'Hace 32 min' }
]

const actions = [
  { icon: 'mdi-auto-fix', title: 'Entrenar con nuevos datos', subtitle: 'Sube documentos o añade FAQs' },
  { icon: 'mdi-chart-box-outline', title: 'Ver analíticas completas', subtitle: 'Métricas detalladas de uso' },
  { icon: 'mdi-message-processing-outline', title: 'Probar chatbot', subtitle: 'Abre una conversación de prueba' },
  { icon: 'mdi-account-cog-outline', title: 'Gestionar usuarios', subtitle: 'Ve quién interactúa con tu bot' }
]
</script>