<template>
  <v-container fluid class="pa-4 pa-md-8 home-dashboard">
    <div class="mb-6 mb-md-8">
      <h1 class="text-h4 font-weight-bold mb-2 dashboard-section-title">Bienvenido de vuelta</h1>
      <p class="text-medium-emphasis dashboard-section-subtitle">Aquí tienes un resumen de la actividad de tu chatbot</p>
    </div>

    <KpiCardsGrid :items="kpis" :is-loading="isLoading" row-class="mb-6" />

    <v-row>
      <v-col cols="12" md="12">
        <v-card class="rounded-xl bg-surface dashboard-panel-card" border elevation="0">
          <div class="pa-4 pa-md-5 d-flex justify-space-between align-center border-b ">
            <div>
              <h2 class="text-h6 font-weight-bold dashboard-section-title">Conversaciones recientes</h2>
              <p class="text-caption text-medium-emphasis mb-0 dashboard-section-subtitle">Últimas interacciones con tu chatbot</p>
            </div>
            <v-btn to="/dashboard/conversations" variant="text" size="small" color="primary" class="text-none font-weight-bold dashboard-ghost-btn" append-icon="mdi-arrow-top-right">
              Ver todas
            </v-btn>
          </div>

          <div v-if="isLoading" class="d-flex justify-center align-center pa-10">
            <v-progress-circular indeterminate color="primary"></v-progress-circular>
          </div>

          <div v-else-if="!conversationPanelData || conversationPanelData.length === 0" class="d-flex flex-column justify-center align-center pa-10 text-center dashboard-empty-state">
            <v-icon icon="mdi-message-text-off-outline" size="x-large" color="disabled" class="mb-2"></v-icon>
            <p class="text-body-2 text-medium-emphasis">No hay conversaciones recientes disponibles</p>
          </div>

          <v-list v-else lines="two" class="pa-0 bg-transparent">
            <template v-for="(chat, i) in conversationPanelData" :key="i">
              <v-list-item class="px-4 px-md-5 py-3 v-card--link dashboard-list-item" @click="openConversation(chat.conversationId)">
                <template v-slot:prepend>
                  <v-avatar color="primary" variant="tonal" size="40" class="mr-4 font-weight-bold">
                    {{ chat.customerName ? chat.customerName.charAt(0).toUpperCase() : 'C' }}
                  </v-avatar>
                </template>
                <v-list-item-title class="font-weight-bold text-body-2">
                  {{ chat.customerName || 'Cliente Anónimo' }}
                </v-list-item-title>
                <v-list-item-subtitle class="text-caption mt-1">
                  {{ chat.role ? chat.role + ': ' : '' }}{{ chat.lastMessage || 'Sin mensajes aún' }}
                </v-list-item-subtitle>
                
                <template v-slot:append>
                  <div class="d-flex flex-column align-end">
                    <span :class="['text-caption font-weight-bold mb-1 d-flex align-center dashboard-status-pill', chat.status === 'Open' ? 'status-open' : 'status-closed']">
                      <v-icon size="x-small" class="mr-1">
                        {{ chat.status === 'Open' ? 'mdi-alert-circle-outline' : 'mdi-check-circle-outline' }}
                      </v-icon>
                      {{ chat.status === 'Open' ? 'Abierta' : 'Cerrada' }}
                    </span>
                    <span class="text-caption text-medium-emphasis">{{ chat.lastMessageTime }}</span>
                  </div>
                </template>
              </v-list-item>
              <v-divider v-if="i !== conversationPanelData.length - 1" class="border-b"></v-divider>
            </template>
          </v-list>
        </v-card>
      </v-col>
      
    </v-row>
  </v-container>
</template>

<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import { storeToRefs } from 'pinia'
import { useAnalyticsStore } from '@/stores/useAnalyticsStore'
import { useRouter } from 'vue-router'
import KpiCardsGrid, { type KpiCardItem } from '@/components/dashboard/KpiCardsGrid.vue'

const analyticsStore = useAnalyticsStore()
const router = useRouter()
const { 
  totalConversations, 
  totalActiveUsers, 
  averageMessagesByConversation, 
  averageResponseTime, 
  conversationPanelData 
} = storeToRefs(analyticsStore)

const isLoading = ref(true)

const kpis = computed<KpiCardItem[]>(() => [
  { 
    title: 'Conversaciones Totales', 
    value: totalConversations.value ?? 0, 
    icon: 'mdi-message-text-outline', 
    trend: 'Global', 
    trendUp: true 
  },
  { 
    title: 'Usuarios Activos (15m)', 
    value: totalActiveUsers.value ?? 0, 
    icon: 'mdi-account-multiple-outline', 
    trend: 'En tiempo real', 
    trendUp: true 
  },
  { 
    title: 'Mensajes / Chat (Promedio)', 
    value: averageMessagesByConversation.value ? averageMessagesByConversation.value.toFixed(1) : '0', 
    icon: 'mdi-chart-bell-curve-cumulative', 
    trend: 'Interacción', 
    trendUp: true 
  },
  { 
    title: 'Tiempo de respuesta IA', 
    value: averageResponseTime.value ? `${averageResponseTime.value.toFixed(2)}s` : '0s', 
    icon: 'mdi-clock-outline', 
    trend: 'Velocidad', 
    trendUp: true 
  }
])

const openConversation = (conversationId: number) => {
  router.push({
    name: 'dashboard-conversations',
    query: { conversationId: String(conversationId) }
  })
}

onMounted(async () => {
  try {
    isLoading.value = true
    await analyticsStore.getAnalyticsData()
  } catch (error) {
    console.error('Error loading dashboard analytics:', error)
  } finally {
    isLoading.value = false
  }
  await analyticsStore.getConversationData()
})
</script>