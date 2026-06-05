<template>
  <v-dialog v-model="dialog" max-width="400" persistent>
    <v-card class="rounded-xl pa-4 text-center">
      <v-card-text class="pt-4">
        <v-icon :icon="icon" size="64" :color="color" class="mb-4"></v-icon>
        <h3 class="text-h5 font-weight-bold mb-2">{{ title }}</h3>
        <p class="text-medium-emphasis mb-0">
          {{ message }}
        </p>
      </v-card-text>
      
      <v-card-actions class="justify-center pb-4 gap-2">
        <v-btn 
          variant="text" 
          class="text-none font-weight-bold" 
          color="grey-darken-1"
          :disabled="loading"
          @click="closeModal"
        >
          {{ cancelText }}
        </v-btn>
        
        <v-btn 
          :color="color" 
          variant="flat" 
          class="text-none font-weight-bold px-6" 
          :loading="loading"
          @click="confirmAction"
        >
          {{ confirmText }}
        </v-btn>
      </v-card-actions>
    </v-card>
  </v-dialog>
</template>

<script setup lang="ts">
import { ref, watch } from 'vue'

const props = defineProps({
  modelValue: { type: Boolean, required: true },
  title: { type: String, default: '¿Estás seguro?' },
  message: { type: String, default: 'Esta acción no se puede deshacer.' },
  confirmText: { type: String, default: 'Confirmar' },
  cancelText: { type: String, default: 'Cancelar' },
  color: { type: String, default: 'error' },
  icon: { type: String, default: 'mdi-alert-circle-outline' },
  loading: { type: Boolean, default: false }
})

const emit = defineEmits(['update:modelValue', 'confirm'])

const dialog = ref(props.modelValue)

watch(() => props.modelValue, (val) => {
  dialog.value = val
})

watch(dialog, (val) => {
  emit('update:modelValue', val)
})

const closeModal = () => {
  dialog.value = false
}

const confirmAction = () => {
  emit('confirm')
}
</script>