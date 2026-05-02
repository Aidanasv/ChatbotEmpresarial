<template>
  <v-container fluid class="pa-6 pa-md-8 analytics-container">
    
    <div class="d-flex flex-column flex-md-row justify-space-between align-md-center mb-8">
      <div>
        <h1 class="text-h4 font-weight-bold mb-2">Analíticas Detalladas</h1>
        <p class="text-medium-emphasis">Rendimiento y métricas de tu asistente en tiempo real</p>
      </div>
      
      <div class="mt-4 mt-md-0 d-flex gap-3">
        <v-btn variant="outlined" color="grey-darken-1" prepend-icon="mdi-calendar" class="text-none">
          Últimos 30 días
        </v-btn>
        <v-btn color="primary" prepend-icon="mdi-download" class="text-none font-weight-bold">
          Exportar PDF
        </v-btn>
      </div>
    </div>

    <v-row class="mb-6">
      <v-col cols="12" sm="6" md="4" v-for="(stat, i) in summaryStats" :key="i">
        <v-card class="dash-card pa-5 h-100" flat>
          <div class="d-flex align-center mb-4">
            <v-avatar :color="stat.color" variant="tonal" rounded size="48" class="mr-4">
              <v-icon :icon="stat.icon" size="x-large"></v-icon>
            </v-avatar>
            <div>
              <div class="text-caption text-medium-emphasis text-uppercase font-weight-bold">{{ stat.title }}</div>
              <div class="text-h4 font-weight-black mt-1">{{ stat.value }}</div>
            </div>
          </div>
          <div class="d-flex align-center text-caption">
            <span :class="['font-weight-bold mr-1', stat.trendUp ? 'text-success' : 'text-error']">
              {{ stat.trend }}
            </span>
            <span class="text-medium-emphasis">vs el periodo anterior</span>
          </div>
        </v-card>
      </v-col>
    </v-row>

    <v-row>
      <v-col cols="12">
        <v-card class="dash-card pa-6" flat>
          <div class="d-flex justify-space-between align-center mb-6">
            <h2 class="text-h6 font-weight-bold">Volumen de conversaciones</h2>
            <v-btn variant="text" size="small" color="primary" class="text-none">Ver detalles</v-btn>
          </div>
          
          <div class="chart-placeholder d-flex flex-column justify-center align-center bg-grey-lighten-4 rounded-lg">
            <v-icon size="64" color="grey-lighten-1" class="mb-3">mdi-chart-bell-curve-cumulative</v-icon>
            <span class="text-medium-emphasis font-weight-medium">Área para gráfico de líneas (Ej: ApexCharts o Chart.js)</span>
          </div>
        </v-card>
      </v-col>

      <v-col cols="12" md="6">
        <v-card class="dash-card pa-6 h-100" flat>
          <h2 class="text-h6 font-weight-bold mb-6">Satisfacción del usuario</h2>
          
          <div class="chart-placeholder-small d-flex flex-column justify-center align-center bg-grey-lighten-4 rounded-lg">
            <v-icon size="48" color="grey-lighten-1" class="mb-3">mdi-chart-donut</v-icon>
            <span class="text-medium-emphasis font-weight-medium">Área para gráfico circular</span>
          </div>
        </v-card>
      </v-col>

      <v-col cols="12" md="6">
        <v-card class="dash-card pa-6 h-100" flat>
          <h2 class="text-h6 font-weight-bold mb-6">Temas más consultados</h2>
          
          <div class="chart-placeholder-small d-flex flex-column justify-center align-center bg-grey-lighten-4 rounded-lg">
            <v-icon size="48" color="grey-lighten-1" class="mb-3">mdi-chart-bar</v-icon>
            <span class="text-medium-emphasis font-weight-medium">Área para gráfico de barras horizontales</span>
          </div>
        </v-card>
      </v-col>
    </v-row>
  </v-container>
</template>

<script setup lang="ts">
const summaryStats = [
  { 
    title: 'Total Mensajes', 
    value: '24,592', 
    trend: '+18.2%', 
    trendUp: true, 
    icon: 'mdi-message-text-multiple-outline',
    color: 'primary'
  },
  { 
    title: 'Tasa de Retención', 
    value: '68.4%', 
    trend: '+5.1%', 
    trendUp: true, 
    icon: 'mdi-account-convert-outline',
    color: 'success'
  },
  { 
    title: 'Tasa de Abandono', 
    value: '12.1%', 
    trend: '-2.4%', 
    trendUp: false,
    icon: 'mdi-exit-run',
    color: 'error'
  }
]
</script>