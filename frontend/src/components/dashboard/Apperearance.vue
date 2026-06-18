<template>
    <v-container fluid class="pa-6 pa-md-8 analytics-container">
        <v-alert v-if="!canEditAppearance" type="warning" variant="tonal" class="mb-4">
            Tu plan actual solo permite usar el estilo base del widget. Cambia de plan para editar la apariencia.
        </v-alert>

        <AppearanceForm v-model="appearance" :disabled="!canEditAppearance" />
        
        <div class="d-flex justify-end mt-8">
            <v-btn 
                color="primary" 
                size="large" 
                rounded="lg" 
                @click="saveChanges"
                :disabled="!canEditAppearance"
                :loading="setupStore.isLoading" 
                class="text-none font-weight-bold px-8"
            >
                Guardar cambios
            </v-btn>
        </div>
    </v-container>
</template>

<script setup lang="ts">
import { computed, onMounted, ref } from 'vue'
import { useSetupStore } from '@/stores/useSetupStore'
import { useAuthStore } from '@/stores/useAuthStore'
import AppearanceForm from '../setup/AppearanceForm.vue'
import { SUBSCRIPTION_FEATURES, hasSubscriptionFeature } from '@/utils/subscriptionPermissions'

const setupStore = useSetupStore()
const authStore = useAuthStore()

const appearance = computed({
    get: () => setupStore.appearanceSetup,
    set: (val) => { setupStore.appearanceSetup = val }
})

const currentSubscriptionFeatures = ref<string[]>([])
const canEditAppearance = computed(() => hasSubscriptionFeature(currentSubscriptionFeatures.value, SUBSCRIPTION_FEATURES.COLOR_AND_AVATAR_CUSTOMIZATION))

onMounted(async () => {
    currentSubscriptionFeatures.value = await setupStore.getCurrentSubscriptionFeatures(authStore.companyId ? Number(authStore.companyId) : undefined)
})

const saveChanges = async () => {
    try {
        await setupStore.saveAppearanceSetup(appearance.value)
        
        console.log("¡Apariencia del chatbot guardada correctamente!")
    } catch (error) {
        console.error("Error al guardar la apariencia:", error)
    }
}
</script>