<template>
    <v-container fluid class="pa-6 pa-md-8 analytics-container">
        <AppearanceForm v-model="appearance" />
        
        <div class="d-flex justify-end mt-8">
            <v-btn 
                color="primary" 
                size="large" 
                rounded="lg" 
                @click="saveChanges"
                :loading="setupStore.isLoading" 
                class="text-none font-weight-bold px-8"
            >
                Guardar cambios
            </v-btn>
        </div>
    </v-container>
</template>

<script setup lang="ts">
import { computed } from 'vue'
import { useSetupStore } from '@/stores/useSetupStore'
import AppearanceForm from '../setup/AppearanceForm.vue'

const setupStore = useSetupStore()

const appearance = computed({
    get: () => setupStore.appearanceSetup,
    set: (val) => { setupStore.appearanceSetup = val }
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