<template>
  <v-card
    class="splan-card d-flex flex-column h-100 rounded-xl"
    :class="{ 'splan-card--popular': isMostPopular }"
    elevation="0"
  >
    <!-- Header -->
    <div class="splan-header pa-5 pb-4">
      <div class="d-flex align-start justify-space-between mb-3">
        <div
          class="splan-icon-wrap d-flex align-center justify-center rounded-lg"
          :class="isMostPopular ? 'splan-icon-wrap--primary' : 'splan-icon-wrap--muted'"
        >
          <v-icon size="20">{{ planIcon }}</v-icon>
        </div>

        <v-chip
          v-if="isMostPopular"
          color="primary"
          size="small"
          rounded="pill"
          class="font-weight-semibold"
        >
          Más popular
        </v-chip>
      </div>

      <div class="text-h6 font-weight-bold mb-1">{{ plan.name }}</div>
      <div class="text-body-2 text-medium-emphasis">{{ planDescription }}</div>
    </div>

    <v-divider />

    <!-- Price -->
    <div class="px-5 py-4">
      <div class="d-flex align-end ga-1">
        <span class="splan-price font-weight-bold">{{ formattedPrice }}</span>
        <span class="text-body-2 text-medium-emphasis mb-1">/mes</span>
      </div>
    </div>

    <v-divider />

    <!-- Features -->
    <div class="px-5 py-4 flex-grow-1">
      <div class="text-caption text-medium-emphasis font-weight-semibold text-uppercase mb-3 ls-wide">
        Incluye
      </div>
      <div
        v-for="feature in plan.features"
        :key="feature"
        class="d-flex align-start ga-2 mb-2"
      >
        <v-icon size="16" color="success" class="mt-1 flex-shrink-0">mdi-check-circle</v-icon>
        <span class="text-body-2">{{ feature }}</span>
      </div>
    </div>

    <v-divider />

    <!-- Footer -->
    <div class="px-5 py-4 d-flex align-center justify-space-between">
      <div class="d-flex align-center ga-2 text-medium-emphasis">
        <v-icon size="16">{{ mode === 'edit' ? 'mdi-office-building-outline' : 'mdi-account-multiple-outline' }}</v-icon>
        <span class="text-body-2">
          {{ mode === 'edit' ? `${plan.companiesCount} empresas` : `Hasta ${plan.maxUsers} usuarios` }}
        </span>
      </div>

      <v-btn
        v-if="mode === 'edit'"
        variant="tonal"
        color="primary"
        size="small"
        prepend-icon="mdi-pencil-outline"
        class="text-none"
        rounded="lg"
        @click="emit('edit', plan)"
      >
        Editar
      </v-btn>

      <v-btn
        v-else
        :color="isSelected ? 'success' : 'primary'"
        :variant="isSelected ? 'tonal' : 'flat'"
        size="small"
        :prepend-icon="isSelected ? 'mdi-check-circle' : 'mdi-check-circle-outline'"
        class="text-none"
        rounded="lg"
        @click="emit('select', plan)"
      >
        {{ isSelected ? 'Seleccionado' : 'Seleccionar' }}
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
    currency: 'EUR',
    maximumFractionDigits: props.plan.price % 1 === 0 ? 0 : 2
  }).format(props.plan.price)
})

const planDescription = computed(() => {
  if (props.plan.maxUsers <= 2) return 'Para probar la plataforma'
  if (props.plan.maxUsers <= 5) return 'Para pequeñas empresas'
  return 'Para organizaciones con mayor volumen'
})

const planIcon = computed(() => {
  const normalized = props.plan.name.toLowerCase()
  if (normalized.includes('free') || normalized.includes('basic')) return 'mdi-flash-outline'
  if (normalized.includes('pro') || normalized.includes('standard')) return 'mdi-crown-outline'
  return 'mdi-office-building-outline'
})
</script>

<style scoped>
.splan-card {
  border: 1px solid rgba(var(--v-theme-on-surface), 0.1);
  background: rgb(var(--v-theme-surface));
  transition: box-shadow 0.2s ease, border-color 0.2s ease;
}

.splan-card:hover {
  box-shadow: 0 8px 24px rgba(var(--v-theme-on-surface), 0.08) !important;
  border-color: rgba(var(--v-theme-primary), 0.3);
}

.splan-card--popular {
  border-color: rgba(var(--v-theme-primary), 0.4);
  box-shadow: 0 4px 16px rgba(var(--v-theme-primary), 0.12) !important;
}

.splan-icon-wrap {
  width: 40px;
  height: 40px;
}

.splan-icon-wrap--primary {
  background: rgba(var(--v-theme-primary), 0.12);
  color: rgb(var(--v-theme-primary));
}

.splan-icon-wrap--muted {
  background: rgba(var(--v-theme-on-surface), 0.06);
  color: rgba(var(--v-theme-on-surface), 0.5);
}

.splan-price {
  font-size: 2rem;
  line-height: 1;
}

.ls-wide {
  letter-spacing: 0.06em;
}
</style>
