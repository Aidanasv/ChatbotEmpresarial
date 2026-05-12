<template>
  <div class="step-content text-left">
    <div class="mb-8">
      <h2 class="text-h5 font-weight-bold mb-1">Base de conocimiento</h2>
      <p class="text-medium-emphasis">Proporciona la información que usará tu chatbot para responder</p>
    </div>

    <v-form>
      <div class="upload-zone mb-8 d-flex flex-column align-center justify-center pa-10 rounded-xl cursor-pointer"
        @click="triggerFile">
        <v-icon size="40" class="mb-3 upload-icon text-medium-emphasis">mdi-upload-outline</v-icon>
        <div class="font-weight-bold text-body-1">Sube documentos de tu empresa</div>
        <div class="text-caption text-medium-emphasis">PDF, DOCX, TXT — máx. 10MB por archivo</div>

        <input type="file" class="d-none" ref="fileInput" @change="handleFileSelect" multiple accept=".pdf,.docx,.txt">

        <v-btn variant="text" color="primary" class="mt-4 text-none font-weight-bold">
          Seleccionar archivos
        </v-btn>
      </div>

      <label class="form-label">Preguntas frecuentes</label>
      <v-textarea :model-value="modelValue.faqs" @update:model-value="update('faqs', $event)"
        placeholder="P: ¿Cuál es el horario de atención? \nR: Lunes a viernes de 9:00 a 18:00." variant="solo" flat
        class="setup-input mb-6" rows="3" hide-details></v-textarea>
    </v-form>
  </div>
</template>

<script setup lang="ts">
import { ref } from 'vue'

const props = defineProps<{
  modelValue: {
    documents: { title: string; url: string; status: string; size: number; createdAt: Date }[];
    faqs: { question: string; answer: string; createdAt: Date; updatedAt: Date }[];
  }
}>()

const emit = defineEmits(['update:modelValue'])

const update = (field: string, value: any) => {
  emit('update:modelValue', { ...props.modelValue, [field]: value })
}

const fileInput = ref<HTMLInputElement | null>(null)

const triggerFile = () => {
  fileInput.value?.click()
}

const handleFileSelect = (event: Event) => {
  const target = event.target as HTMLInputElement
  if (target.files && target.files.length > 0) {
    update('documents', Array.from(target.files).map(file => ({
      title: file.name,
      url: URL.createObjectURL(file),
      status: 'pending',
      size: file.size,
      createdAt: new Date()
    })))
  }
}
</script>