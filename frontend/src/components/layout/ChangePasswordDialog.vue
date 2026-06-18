<template>
  <v-dialog :model-value="modelValue" max-width="460" @update:model-value="emit('update:modelValue', $event)">
    <v-card rounded="xl">
      <div class="px-6 pt-6 pb-3 d-flex align-center justify-space-between">
        <div>
          <div class="text-h6 font-weight-bold">Cambiar contraseña</div>
          <div class="text-body-2 text-medium-emphasis">Actualiza tu acceso sin cerrar sesión.</div>
        </div>

        <v-btn icon variant="text" @click="closeDialog">
          <v-icon>mdi-close</v-icon>
        </v-btn>
      </div>

      <v-card-text class="pt-2">
        <v-form @submit.prevent="handleSubmit" class="d-flex flex-column ga-4">
          <v-text-field
            v-model="currentPassword"
            label="Contraseña actual"
            type="password"
            variant="outlined"
            hide-details="auto"
            required
          />

          <v-text-field
            v-model="newPassword"
            label="Nueva contraseña"
            type="password"
            variant="outlined"
            hide-details="auto"
            required
          />

          <v-text-field
            v-model="confirmPassword"
            label="Confirmar nueva contraseña"
            type="password"
            variant="outlined"
            hide-details="auto"
            required
          />

          <div v-if="errorMessage" class="text-body-2 text-error">{{ errorMessage }}</div>

          <div class="d-flex justify-end ga-3 pt-2">
            <v-btn variant="text" class="text-none" @click="closeDialog">Cancelar</v-btn>
            <v-btn type="submit" color="primary" class="text-none" :loading="isSubmitting">Guardar</v-btn>
          </div>
        </v-form>
      </v-card-text>
    </v-card>
  </v-dialog>
</template>

<script setup lang="ts">
import { ref, watch } from 'vue'
import { useAuthStore } from '@/stores/useAuthStore'

const props = defineProps<{
  modelValue: boolean;
}>()

const emit = defineEmits<{
  (event: 'update:modelValue', value: boolean): void;
}>()

const authStore = useAuthStore()
const currentPassword = ref('')
const newPassword = ref('')
const confirmPassword = ref('')
const errorMessage = ref('')
const isSubmitting = ref(false)

const resetForm = () => {
  currentPassword.value = ''
  newPassword.value = ''
  confirmPassword.value = ''
  errorMessage.value = ''
  isSubmitting.value = false
}

const closeDialog = () => {
  resetForm()
  emit('update:modelValue', false)
}

const handleSubmit = async () => {
  errorMessage.value = ''

  if (!currentPassword.value || !newPassword.value) {
    errorMessage.value = 'Completa todos los campos.'
    return
  }

  if (newPassword.value.length < 6) {
    errorMessage.value = 'La nueva contraseña debe tener al menos 6 caracteres.'
    return
  }

  if (newPassword.value !== confirmPassword.value) {
    errorMessage.value = 'La confirmación no coincide con la nueva contraseña.'
    return
  }

  isSubmitting.value = true
  const updated = await authStore.changePassword({
    currentPassword: currentPassword.value,
    newPassword: newPassword.value
  })
  isSubmitting.value = false

  if (updated) {
    closeDialog()
  }
}

watch(() => props.modelValue, (isOpen) => {
  if (!isOpen) {
    resetForm()
  }
})
</script>