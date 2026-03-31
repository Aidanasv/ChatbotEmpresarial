<template>
  <div class="step-content text-left">
    <div class="mb-8">
      <h2 class="text-h5 font-weight-bold mb-1">Apariencia del widget</h2>
      <p class="text-medium-emphasis">Personaliza cómo se verá el chatbot en tu sitio web</p>
    </div>

    <v-form>
      <label class="form-label mb-3">Color principal</label>
      <div class="d-flex flex-wrap gap-3 mb-8">
        <div 
          v-for="color in colorOptions" 
          :key="color"
          class="color-circle cursor-pointer"
          :style="{ backgroundColor: color }"
          :class="{ 'is-selected': modelValue.primaryColor === color }"
          @click="update('primaryColor', color)"
        >
          <v-icon v-if="modelValue.primaryColor === color" color="white" size="small">mdi-check</v-icon>
        </div>
      </div>

      <v-card variant="outlined" class="pa-4 rounded-xl mb-6 appearance-card" flat>
        <div class="d-flex align-center justify-space-between">
          <div>
            <div class="font-weight-bold">Mostrar avatar</div>
            <div class="text-caption text-medium-emphasis">Imagen del bot en el chat</div>
          </div>
          <v-switch
            :model-value="modelValue.showAvatar"
            @update:model-value="update('showAvatar', $event)"
            color="primary"
            hide-details
            inset
          ></v-switch>
        </div>
      </v-card>

      <label class="form-label mb-3">Posición del widget</label>
      <v-btn-toggle
        :model-value="modelValue.position"
        @update:model-value="update('position', $event)"
        mandatory
        variant="outlined"
        class="w-100 position-selector"
      >
        <v-btn value="right" class="flex-grow-1 text-none">Inferior derecha</v-btn>
        <v-btn value="left" class="flex-grow-1 text-none">Inferior izquierda</v-btn>
      </v-btn-toggle>
      
      <div 
        class="mt-8 pa-6 rounded-xl preview-box d-flex flex-column transition-swing" 
        :class="modelValue.position === 'left' ? 'align-start' : 'align-end'"
      >
        <span class="text-caption text-medium-emphasis mb-4 w-100 text-center">Vista previa</span>
        <v-btn icon size="large" :color="modelValue.primaryColor" elevation="4">
          <v-icon>mdi-message-text</v-icon>
        </v-btn>
      </div>
    </v-form>
  </div>
</template>

<script setup lang="ts">
const props = defineProps<{
  modelValue: {
    primaryColor: string;
    showAvatar: boolean;
    position: string;
  }
}>()

const emit = defineEmits(['update:modelValue'])

const update = (field: string, value: any) => {
  emit('update:modelValue', { ...props.modelValue, [field]: value })
}

const colorOptions = ['#4F46E5', '#7C3AED', '#10B981', '#EF4444', '#F59E0B', '#06B6D4']
</script>