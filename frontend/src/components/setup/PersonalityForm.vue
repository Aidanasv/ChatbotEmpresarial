<template>
  <div class="step-content text-left">
    <div class="mb-8">
      <h2 class="text-h5 font-weight-bold mb-1">Personalidad del chatbot</h2>
      <p class="text-medium-emphasis">Define cómo se comportará y comunicará tu asistente</p>
    </div>

    <v-form>
      <label class="form-label">Nombre del chatbot <span class="text-error">*</span></label>
      <v-text-field
        :model-value="modelValue.botName"
        @update:model-value="update('botName', $event)"
        placeholder="Ej: Asistente Acme, Luna, Max..."
        variant="solo" flat class="setup-input mb-6" hide-details
      ></v-text-field>

      <label class="form-label mb-3">Tono de comunicación</label>
      <v-row class="mb-6">
        <v-col cols="12" sm="6" v-for="option in toneOptions" :key="option.value">
          <v-card
            flat
            :class="['tone-card cursor-pointer pa-4 rounded-xl d-flex align-start', { 'is-selected': modelValue.tone === option.value }]"
            @click="update('tone', option.value)"
          >
            <v-radio
              :model-value="modelValue.tone === option.value"
              color="primary"
              class="mr-2 tone-radio"
              hide-details
              readonly
            ></v-radio>
            
            <div class="pt-1">
              <div class="font-weight-bold text-body-1 mb-1 tone-title">{{ option.title }}</div>
              <div class="text-caption text-medium-emphasis">{{ option.desc }}</div>
            </div>
          </v-card>
        </v-col>
      </v-row>

      <label class="form-label">Mensaje de bienvenida</label>
      <v-textarea
        :model-value="modelValue.welcomeMsg"
        @update:model-value="update('welcomeMsg', $event)"
        placeholder="¡Hola! 👋 Soy el asistente de Acme. ¿En qué puedo ayudarte hoy?"
        variant="solo" flat class="setup-input mb-6" rows="2" hide-details
      ></v-textarea>

      <label class="form-label">Respuesta cuando no sabe algo</label>
      <v-textarea
        :model-value="modelValue.fallbackMsg"
        @update:model-value="update('fallbackMsg', $event)"
        placeholder="Lo siento, no tengo esa información. ¿Te gustaría hablar con un agente?"
        variant="solo" flat class="setup-input" rows="2" hide-details
      ></v-textarea>
    </v-form>
  </div>
</template>

<script setup lang="ts">
const props = defineProps<{
  modelValue: {
    botName: string;
    tone: string;
    welcomeMsg: string;
    fallbackMsg: string;
  }
}>()

const emit = defineEmits(['update:modelValue'])

const update = (field: string, value: any) => {
  emit('update:modelValue', { ...props.modelValue, [field]: value })
}

const toneOptions = [
  { value: 'formal', title: 'Formal', desc: 'Profesional y corporativo' },
  { value: 'amigable', title: 'Amigable', desc: 'Cercano y conversacional' },
  { value: 'tecnico', title: 'Técnico', desc: 'Preciso y detallado' },
  { value: 'casual', title: 'Casual', desc: 'Relajado e informal' }
]
</script>