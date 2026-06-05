<template>
  <v-container fluid class="pa-6 pa-md-8 analytics-container">
    
    <PersonalityForm v-model="personality" />
    
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
import PersonalityForm from '@/components/setup/PersonalityForm.vue'

const setupStore = useSetupStore()

const personality = computed({
    get: () => setupStore.personalitySetup,
    set: (val) => { setupStore.personalitySetup = val }
})

const saveChanges = async () => {
    try {
        await setupStore.savePersonalitySetup(personality.value)
        
        console.log("¡Personalidad del chatbot guardada correctamente!")
    } catch (error) {
        console.error("Error al guardar la personalidad:", error)
    }
}
</script>