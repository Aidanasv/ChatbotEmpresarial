<template>
  <v-dialog v-model="dialog" max-width="500" persistent>
    <v-card class="rounded-xl pa-2" elevation="10">
      
      <v-card-title class="d-flex justify-space-between align-center pt-4 px-4 pb-2">
        <span class="text-h6 font-weight-bold">
          {{ isEditMode ? 'Editar usuario' : 'Añadir nuevo usuario' }}
        </span>
        <v-btn icon="mdi-close" variant="text" size="small" @click="closeModal" :disabled="isSubmitting"></v-btn>
      </v-card-title>

      <v-card-text class="px-4 py-2">
        <v-form ref="form" v-model="isFormValid" @submit.prevent="submitUser">
          
          <div class="mb-1 text-body-2 text-medium-emphasis">Nombre del usuario</div>
          <v-text-field
            v-model="formData.userName"
            variant="outlined"
            density="compact"
            placeholder="Ej. Ana García"
            :rules="[v => !!v || 'El nombre es requerido']"
            class="mb-4"
          ></v-text-field>

          <div class="mb-1 text-body-2 text-medium-emphasis">Correo electrónico</div>
          <v-text-field
            v-model="formData.email"
            variant="outlined"
            density="compact"
            placeholder="usuario@empresa.com"
            type="email"
            :rules="emailRules"
            class="mb-4"
          ></v-text-field>

          <div class="mb-1 text-body-2 text-medium-emphasis">Rol del usuario</div>
          <v-select
            v-model="formData.role"
            :items="['Admin', 'User']" 
            variant="outlined"
            density="compact"
            :rules="[v => !!v || 'Debes seleccionar un rol']"
            class="mb-4"
          ></v-select>

          <template v-if="!isEditMode">
            <div class="mb-1 text-body-2 text-medium-emphasis">Contraseña temporal</div>
            <v-text-field
              v-model="formData.password"
              variant="outlined"
              density="compact"
              type="password"
              placeholder="Asigna una contraseña inicial"
              :rules="[v => !!v || 'La contraseña es requerida']"
              class="mb-4"
            ></v-text-field>
          </template>

          <v-switch
            v-model="isActive"
            color="primary"
            :label="isEditMode ? 'Usuario activo' : 'Marcar como Activo inmediatamente'"
            hide-details
            class="mb-2"
          ></v-switch>

        </v-form>
      </v-card-text>

      <v-card-actions class="px-4 pb-4 pt-0">
        <v-btn
          color="primary"
          variant="flat"
          block
          size="large"
          class="text-none font-weight-bold rounded-lg"
          :loading="isSubmitting"
          @click="submitUser"
        >
          {{ isEditMode ? 'Guardar cambios' : 'Crear usuario' }}
        </v-btn>
      </v-card-actions>
      
    </v-card>
  </v-dialog>
</template>

<script setup lang="ts">
import { ref, watch, computed } from 'vue'

const props = defineProps({
  modelValue: {
    type: Boolean,
    required: true
  },
  userToEdit: {
    type: Object,
    default: null
  }
})

const emit = defineEmits(['update:modelValue', 'save'])

const dialog = ref(props.modelValue)
const form = ref<any>(null)
const isFormValid = ref(false)
const isSubmitting = ref(false)
const isActive = ref(true)

const isEditMode = computed(() => !!props.userToEdit)

// Estado unificado del formulario
const formData = ref({
  id: null as number | null,
  userName: '',
  email: '',
  role: 'User',
  password: ''
})

const emailRules = [
  (v: string) => !!v || 'El correo es requerido',
  (v: string) => /.+@.+\..+/.test(v) || 'El formato del correo debe ser válido'
]

watch(() => props.modelValue, (isOpen) => {
  dialog.value = isOpen
  
  if (isOpen) {
    if (props.userToEdit) {
      formData.value = {
        id: props.userToEdit.id,
        userName: props.userToEdit.userName,
        email: props.userToEdit.email,
        role: props.userToEdit.role,
        password: '' // No se envía en UpdateUserDTO
      }
      isActive.value = props.userToEdit.status?.toLowerCase() === 'activo'
    } else {
      resetForm()
    }
  }
})

watch(dialog, (val) => {
  emit('update:modelValue', val)
  if (!val && form.value) form.value.resetValidation()
})

const closeModal = () => {
  dialog.value = false
}

const resetForm = () => {
  formData.value = {
    id: null,
    userName: '',
    email: '',
    role: 'User',
    password: ''
  }
  isActive.value = true
  if (form.value) form.value.resetValidation()
}

const submitUser = async () => {
  const { valid } = await form.value.validate()
  if (!valid) return

  try {
    isSubmitting.value = true

    let userDataToSubmit: any = {}

    if (isEditMode.value) {
      // Estructura exacta para UpdateUserDTO (C#)
      userDataToSubmit = {
        id: formData.value.id, // Necesario para saber a quién actualizar en el Store/API
        userName: formData.value.userName,
        email: formData.value.email,
        role: formData.value.role,
        status: isActive.value ? 'Activo' : 'Inactivo'
      }
    } else {
      // Estructura exacta para CreateUserDTO (C#)
      userDataToSubmit = {
        userName: formData.value.userName,
        email: formData.value.email,
        password: formData.value.password,
        role: formData.value.role,
        status: isActive.value ? 'Activo' : 'Inactivo'
      }
    }

    emit('save', { isEdit: isEditMode.value, data: userDataToSubmit })
    
    closeModal()
  } catch (error) {
    console.error('Error al preparar el usuario', error)
  } finally {
    isSubmitting.value = false
  }
}
</script>