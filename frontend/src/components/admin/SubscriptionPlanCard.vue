<template>
  <v-card class="subscription-card h-100 pa-4 pa-md-6 rounded-xl" elevation="0">
    <div class="d-flex justify-space-between align-start mb-4">
      <div>
        <v-chip v-if="isMostPopular" color="primary" size="small" class="mb-3" rounded="pill">
          Más popular
        </v-chip>
        <div class="d-flex align-center ga-2 mb-2">
          <v-icon :color="isMostPopular ? 'primary' : 'grey-darken-1'">{{ planIcon }}</v-icon>
          <h2 class="text-h5 font-weight-bold">{{ plan.name }}</h2>
        </div>
        <p class="text-body-1 text-medium-emphasis mb-0">{{ planDescription }}</p>
      </div>

      <v-chip size="small" variant="tonal" color="primary">{{ plan.maxUsers }} usuarios</v-chip>
    </div>

    <div class="d-flex align-end ga-1 mb-6">
      <span class="text-h2 font-weight-bold">{{ formattedPrice }}</span>
      <span class="text-h6 text-medium-emphasis mb-1">/mes</span>
    </div>

    <v-list class="bg-transparent pa-0 mb-4">
      <v-list-item v-for="feature in plan.features" :key="feature" class="px-0 min-height-0" density="comfortable">
        <template #prepend>
          <v-icon size="18" color="success">mdi-check</v-icon>
        </template>
        <v-list-item-title class="text-body-1">{{ feature }}</v-list-item-title>
      </v-list-item>
    </v-list>

    <v-divider class="mb-4" />

    <div class="d-flex justify-space-between align-center flex-wrap ga-3">
      <div v-if="mode === 'edit'" class="d-flex align-center ga-2 text-medium-emphasis">
        <v-icon size="18">mdi-office-building-outline</v-icon>
        <span class="text-body-1">{{ plan.companiesCount }} empresas</span>
      </div>
      <div v-else class="d-flex align-center ga-2 text-medium-emphasis">
        <v-icon size="18">mdi-account-multiple-outline</v-icon>
        <span class="text-body-1">Hasta {{ plan.maxUsers }} usuarios</span>
      </div>
      <v-btn v-if="mode === 'edit'" variant="text" prepend-icon="mdi-pencil-outline" class="px-0"
        @click="emit('edit', plan)">
        Editar
      </v-btn>
      <v-btn v-else :color="isSelected ? 'success' : 'primary'" :variant="isSelected ? 'tonal' : 'flat'"
        prepend-icon="mdi-check-circle-outline" @click="emit('select', plan)">
        {{ isSelected ? 'Seleccionado' : 'Seleccionar plan' }}
      </v-btn>
    </div>
  </v-card>
</template>

<script setup lang="ts">
import { computed } from 'vue'
import type { SubscriptionPlan } from '@/types/Subscription'

const props = withDefaults(defineProps<{
  plan: SubscriptionPlan
  isMostPopular: boolean
  mode?: 'edit' | 'select'
  isSelected?: boolean
}>(), {
  mode: 'edit',
  isSelected: false
})

const emit = defineEmits<{
  (event: 'edit', plan: SubscriptionPlan): void
  (event: 'select', plan: SubscriptionPlan): void
}>()

const formattedPrice = computed(() => {
  return new Intl.NumberFormat('es-ES', {
    style: 'currency',
    currency: 'USD',
    maximumFractionDigits: props.plan.price % 1 === 0 ? 0 : 2
  }).format(props.plan.price)
})

const planDescription = computed(() => {
  if (props.plan.maxUsers <= 2) {
    return 'Para probar la plataforma'
  }

  if (props.plan.maxUsers <= 5) {
    return 'Para pequeñas empresas'
  }

  return 'Para organizaciones con mayor volumen'
})

const planIcon = computed(() => {
  const normalized = props.plan.name.toLowerCase()
  if (normalized.includes('free') || normalized.includes('basic')) {
    return 'mdi-flash-outline'
  }

  if (normalized.includes('pro') || normalized.includes('standard')) {
    return 'mdi-crown-outline'
  }

  return 'mdi-office-building-outline'
})
</script>
