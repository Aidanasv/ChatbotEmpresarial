<template>
  <v-container >
    <div class="mb-8">
      <h2 class="text-h5 font-weight-bold mb-1">Apariencia del widget</h2>
      <p class="text-medium-emphasis">Personaliza cómo se verá el chatbot en tu sitio web</p>
    </div>
    <v-alert v-if="disabled" type="warning" variant="tonal" class="mb-6">
      Tu plan actual solo permite usar el estilo base. Cambia de plan para editar colores, avatar y posición.
    </v-alert>
    <v-row class="mb-8" align="center" justify="space-around" >
      <v-col class="d-flex flex-column align-center" cols="12" md="6">
        <v-form>
          <label class="form-label mb-3">Color principal</label>
          <v-color-picker hide-inputs v-model="color" rounded="xl" dot-size="32" class="w-100" :disabled="disabled"></v-color-picker>

          <br />
          <v-card variant="outlined" class="pa-4 rounded-xl mb-6 appearance-card" flat>
            <div class="d-flex align-center justify-space-between">
              <div>
                <div class="font-weight-bold">Mostrar avatar</div>
                <div class="text-caption text-medium-emphasis">Imagen del bot en el chat</div>
              </div>
              <v-switch :model-value="modelValue.showChatbotAvatar"
                @update:model-value="update('showChatbotAvatar', $event)" color="primary" hide-details inset :disabled="disabled"></v-switch>
            </div>
          </v-card>

          <label class="form-label mb-3">Posición del widget</label>
          <div class="d-flex flex-column flex-sm-row ga-4 w-100">
            <v-btn :variant="!modelValue.widgetPosition ? 'flat' : 'outlined'"
              :color="!modelValue.widgetPosition ? 'primary' : 'grey-darken-1'" class="flex-grow-1 text-none"
              rounded="xl" @click="update('widgetPosition', false)" :disabled="disabled">
              Inferior izquierda
            </v-btn>

            <v-btn :variant="modelValue.widgetPosition ? 'flat' : 'outlined'"
              :color="modelValue.widgetPosition ? 'primary' : 'grey-darken-1'" class="flex-grow-1 text-none"
              rounded="xl" @click="update('widgetPosition', true)" :disabled="disabled">
              Inferior derecha
            </v-btn>
          </div>

        </v-form>
      </v-col>
      <v-col class="d-flex flex-column align-center" cols="12" md="6">
        <v-card rounded="xl" class="pa-4 w-100" elevation="1">
          <div class="text-subtitle-2 font-weight-bold mb-1">Vista previa</div>
          <div class="text-caption text-medium-emphasis mb-4">Así se verá tu widget</div>

          <WidgetChatbot :model-value="modelValue" />
        </v-card>

      </v-col>
    </v-row>


  </v-container>
</template>

<script setup lang="ts">
import { ref, watch, computed } from 'vue';
import WidgetChatbot from '../dashboard/widgetChatbot.vue';

const props = defineProps<{
  modelValue: {
    primaryColor: string;
    showChatbotAvatar: boolean;
    widgetPosition: boolean;
    title: string;
  },
  disabled?: boolean
}>()

const color = ref(props.modelValue.primaryColor)

watch(color, (newColor) => {
  update('primaryColor', newColor)
})

const emit = defineEmits(['update:modelValue'])

const disabled = computed(() => props.disabled ?? false)

const update = (field: string, value: any) => {
  if (disabled.value) {
    return
  }

  emit('update:modelValue', { ...props.modelValue, [field]: value })
}
</script>
