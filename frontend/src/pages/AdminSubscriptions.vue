<template>
  <v-container fluid class="pa-6 pa-md-8 subscriptions-page">
    <div class="d-flex flex-column flex-md-row justify-space-between align-start align-md-center ga-4 mb-8">
      <div>
        <h1 class="text-h4 font-weight-bold mb-2">{{ isSuperAdmin ? 'Control de suscripciones' : 'Elige tu plan' }}</h1>
        <p class="text-medium-emphasis mb-0">{{ isSuperAdmin ? 'Gestiona planes, precios y características' :
          'Selecciona el plan que mejor se adapta a tu empresa' }}</p>
      </div>

      <v-btn v-if="isSuperAdmin" color="primary" rounded="xl" size="large" prepend-icon="mdi-plus"
        @click="openCreateDialog">
        Nuevo plan
      </v-btn>
    </div>

    <v-alert v-if="errorMessage" type="error" variant="tonal" class="mb-6">
      {{ errorMessage }}
    </v-alert>

    <v-alert v-if="successMessage" type="success" variant="tonal" class="mb-6">
      {{ successMessage }}
    </v-alert>

    <v-row class="mb-2">
      <v-col v-for="plan in subscriptions" :key="plan.id" cols="12" md="4">
        <SubscriptionPlanCard :plan="plan" :is-most-popular="plan.id === mostPopularPlanId"
          :mode="isSuperAdmin ? 'edit' : 'select'" :is-selected="selectedSubscriptionId === plan.id"
          @edit="openEditDialog" @select="selectPlan" />
      </v-col>
    </v-row>

    <v-card v-if="isSuperAdmin" class="subscription-revenue-card pa-6 pa-md-8 rounded-xl" elevation="0">
      <h2 class="text-h5 font-weight-bold mb-2">Resumen de ingresos por plan</h2>
      <p class="text-medium-emphasis mb-6">Proyección mensual basada en empresas activas</p>

      <div v-for="plan in subscriptions" :key="`revenue-${plan.id}`"
        class="d-flex justify-space-between align-center py-5 subscription-revenue-row">
        <div class="d-flex align-center ga-4 flex-wrap">
          <v-chip size="small" :color="plan.id === mostPopularPlanId ? 'primary' : 'info'" variant="tonal">
            {{ plan.name }}
          </v-chip>
          <span class="text-body-1 text-medium-emphasis">{{ plan.companiesCount }} empresas × {{
            formatCurrency(plan.price) }}</span>
        </div>
        <strong class="text-h6">{{ formatCurrency(plan.projectedMonthlyRevenue) }}/mes</strong>
      </div>

      <div class="d-flex justify-space-between align-center pt-6 mt-2 subscription-revenue-total">
        <span class="text-h5 font-weight-bold">Total proyectado</span>
        <span class="text-h4 font-weight-bold">{{ formatCurrency(totalProjectedRevenue) }}/mes</span>
      </div>
    </v-card>

    <v-dialog v-if="isSuperAdmin" v-model="dialogOpen" max-width="640">
      <v-card rounded="xl">
        <v-card-title class="text-h5 font-weight-bold pt-6 px-6">
          {{ editingPlanId === null ? 'Nuevo plan' : 'Editar plan' }}
        </v-card-title>

        <v-card-text class="px-6 pb-2">
          <v-row>
            <v-col cols="12" md="6">
              <v-text-field v-model="form.name" label="Nombre" variant="outlined" rounded="lg" />
            </v-col>
            <v-col cols="12" md="6">
              <v-text-field v-model.number="form.price" label="Precio mensual" type="number" min="0" variant="outlined"
                rounded="lg" prefix="$" />
            </v-col>
            <v-col cols="12">
              <v-text-field v-model.number="form.maxUsers" label="Usuarios máximos" type="number" min="1"
                variant="outlined" rounded="lg" />
            </v-col>
            <v-col cols="12">
              <v-textarea v-model="featuresText" label="Características (una por línea)" variant="outlined" rounded="lg"
                rows="6" auto-grow />
            </v-col>
          </v-row>
        </v-card-text>

        <v-card-actions class="px-6 pb-6">
          <v-spacer />
          <v-btn variant="text" @click="closeDialog">Cancelar</v-btn>
          <v-btn color="primary" :loading="isSaving" @click="savePlan">Guardar</v-btn>
        </v-card-actions>
      </v-card>
    </v-dialog>
  </v-container>
</template>

<script setup lang="ts">
import { computed, onMounted, reactive, ref } from 'vue'
import { storeToRefs } from 'pinia'
import SubscriptionPlanCard from '@/components/admin/SubscriptionPlanCard.vue'
import { useSuperAdminStore } from '@/stores/useSuperAdminStore'
import { useAuthStore } from '@/stores/useAuthStore'
import { useSetupStore } from '@/stores/useSetupStore'
import type { SubscriptionPlan, UpsertSubscriptionPayload } from '@/types/Subscription'

const superAdminStore = useSuperAdminStore()
const authStore = useAuthStore()
const setupStore = useSetupStore()
const { subscriptions } = storeToRefs(superAdminStore)

const isLoading = ref(false)
const isSaving = ref(false)
const isSelectingPlan = ref(false)
const errorMessage = ref('')
const successMessage = ref('')
const dialogOpen = ref(false)
const editingPlanId = ref<number | null>(null)
const selectedSubscriptionId = ref<number | null>(null)
const featuresText = ref('')
const form = reactive<UpsertSubscriptionPayload>({
  name: '',
  price: 0,
  maxUsers: 1,
  features: []
})

const isSuperAdmin = computed(() => (authStore.role || '').toLowerCase() === 'superadmin')

const totalProjectedRevenue = computed(() => {
  return subscriptions.value.reduce((total, plan) => total + plan.projectedMonthlyRevenue, 0)
})

const mostPopularPlanId = computed(() => {
  if (subscriptions.value.length === 0) {
    return null
  }

  return [...subscriptions.value].sort((left, right) => right.companiesCount - left.companiesCount)[0]?.id ?? null
})

const formatCurrency = (value: number) => {
  return new Intl.NumberFormat('es-ES', {
    style: 'currency',
    currency: 'USD',
    maximumFractionDigits: value % 1 === 0 ? 0 : 2
  }).format(value)
}

const resetForm = () => {
  form.name = ''
  form.price = 0
  form.maxUsers = 1
  form.features = []
  featuresText.value = ''
  editingPlanId.value = null
}

const openCreateDialog = () => {
  resetForm()
  dialogOpen.value = true
}

const openEditDialog = (plan: SubscriptionPlan) => {
  editingPlanId.value = plan.id
  form.name = plan.name
  form.price = plan.price
  form.maxUsers = plan.maxUsers
  form.features = [...plan.features]
  featuresText.value = plan.features.join('\n')
  dialogOpen.value = true
}

const closeDialog = () => {
  dialogOpen.value = false
  resetForm()
}

const fetchSubscriptions = async () => {
  try {
    errorMessage.value = ''
    successMessage.value = ''
    isLoading.value = true
    await superAdminStore.getSubscriptions()
  } catch (error) {
    console.error('Error loading subscriptions:', error)
    errorMessage.value = 'No se pudieron cargar las suscripciones.'
  } finally {
    isLoading.value = false
  }
}

const fetchCurrentSubscription = async () => {
  try {
    selectedSubscriptionId.value = await setupStore.getCurrentSubscription()
  } catch (error) {
    console.error('Error loading current subscription:', error)
    errorMessage.value = 'No se pudo cargar tu suscripción actual.'
  }
}

const selectPlan = async (plan: SubscriptionPlan) => {
  if (isSuperAdmin.value || isSelectingPlan.value) {
    return
  }

  if (selectedSubscriptionId.value === plan.id) {
    return
  }

  try {
    errorMessage.value = ''
    successMessage.value = ''
    isSelectingPlan.value = true
    selectedSubscriptionId.value = await setupStore.updateCompanySubscription(plan.id)
    successMessage.value = `Plan ${plan.name} seleccionado correctamente.`
  } catch (error) {
    console.error('Error selecting subscription:', error)
    errorMessage.value = 'No se pudo actualizar el plan.'
  } finally {
    isSelectingPlan.value = false
  }
}

const savePlan = async () => {
  const parsedFeatures = featuresText.value
    .split('\n')
    .map((feature) => feature.trim())
    .filter((feature) => feature.length > 0)

  if (!form.name.trim() || parsedFeatures.length === 0) {
    errorMessage.value = 'Debes indicar nombre y al menos una característica.'
    return
  }

  try {
    errorMessage.value = ''
    isSaving.value = true

    const payload: UpsertSubscriptionPayload = {
      name: form.name.trim(),
      price: Number(form.price),
      maxUsers: Number(form.maxUsers),
      features: parsedFeatures
    }

    if (editingPlanId.value === null) {
      await superAdminStore.createSubscription(payload)
    } else {
      await superAdminStore.updateSubscription(editingPlanId.value, payload)
    }

    closeDialog()
  } catch (error) {
    console.error('Error saving subscription:', error)
    errorMessage.value = 'No se pudo guardar el plan.'
  } finally {
    isSaving.value = false
  }
}

onMounted(async () => {
  await fetchSubscriptions()
  if (!isSuperAdmin.value) {
    await fetchCurrentSubscription()
  }
})
</script>
