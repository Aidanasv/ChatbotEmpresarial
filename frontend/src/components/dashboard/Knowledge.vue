<template>
  <v-container fluid class="pa-6 pa-md-8 analytics-container">
    
    <KnowledgeForm v-model="knowledge" />
    
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
import KnowledgeForm from '@/components/setup/KnowledgeForm.vue'

const setupStore = useSetupStore()

const knowledge = computed({
    get: () => setupStore.knowledgeSetup,
    set: (val) => { setupStore.knowledgeSetup = val }
})

const saveChanges = async () => {
    try {
        await setupStore.saveKnowledgeSetup(knowledge.value)
        
        console.log("¡Conocimiento del chatbot guardado correctamente!")
    } catch (error) {
        console.error("Error al guardar el conocimiento:", error)
    }
}
</script>