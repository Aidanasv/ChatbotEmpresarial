<template>
  <v-sheet max-width="1000" class="mx-auto bg-transparent">
    
    <div class="mb-8">
      <h1 class="text-h4 font-weight-bold mb-2">Base de conocimiento</h1>
      <p class="text-medium-emphasis">Gestiona la información que usará tu chatbot para responder</p>
    </div>

    <v-card class="mb-6 pa-6 pa-md-8 rounded-xl" border elevation="0">
      <div class="d-flex align-center mb-1">
        <v-icon color="primary" class="mr-3" size="28">mdi-file-document-multiple-outline</v-icon>
        <h2 class="text-h6 font-weight-bold mb-0">Documentos</h2>
      </div>
      <p class="text-medium-emphasis text-body-2 mb-6 ml-10">Sube archivos con información sobre tu empresa</p>

      <v-alert v-if="!canUploadDocuments" type="warning" variant="tonal" class="ml-md-10 mb-6">
        Tu plan no permite subir documentos. Solo podrás revisar los archivos existentes.
      </v-alert>

      <v-alert v-else-if="documentUploadLimit !== null" type="info" variant="tonal" class="ml-md-10 mb-6">
        <span v-if="hasFiniteDocumentLimit && remainingDocumentSlots > 0">
          Puedes subir hasta {{ documentUploadLimit }} documentos. Te quedan {{ remainingDocumentSlots }} cupos.
        </span>
        <span v-else>
          Has alcanzado el maximo de {{ documentUploadLimit }} documentos permitidos por tu plan.
        </span>
      </v-alert>

      <div class="ml-md-10">
        <v-file-input
          v-model="chosenFiles"
          label="Selecciona o arrastra tus documentos (PDF, DOCX, TXT)"
          multiple
          accept=".pdf,.docx,.txt"
          variant="outlined"
          prepend-icon="mdi-upload-outline"
          bg-color="grey-lighten-5"
          class="rounded-lg"
          show-size
          :disabled="!canUploadDocuments || remainingDocumentSlots === 0"
          @update:model-value="handleFileSelect"
        ></v-file-input>

        <v-list class="mt-4 bg-transparent pa-0">
          <v-list-item v-for="(doc, index) in documents" :key="index" class="border rounded-lg mb-2 pa-3">
            <template v-slot:prepend>
              <v-icon color="primary">mdi-file-outline</v-icon>
            </template>
            <v-list-item-title class="font-weight-medium">{{ doc.name }}</v-list-item-title>
            <template v-slot:append>
              <v-btn
                icon="mdi-delete-outline"
                variant="text"
                color="error"
                density="comfortable"
                :disabled="!canUploadDocuments"
                @click="removeDocument(index)"
              ></v-btn>
            </template>
          </v-list-item>
        </v-list>
      </div>
    </v-card>

    <v-card class="pa-6 pa-md-8 rounded-xl mb-6" border elevation="0">
      <div class="d-flex align-center justify-space-between mb-1">
        <div class="d-flex align-center">
          <v-icon color="primary" class="mr-3" size="28">mdi-help-circle-outline</v-icon>
          <h2 class="text-h6 font-weight-bold mb-0">Preguntas frecuentes</h2>
        </div>
        <v-chip variant="outlined" color="primary" size="small" class="font-weight-bold">
          {{ faqs.length }} FAQs
        </v-chip>
      </div>
      <p class="text-medium-emphasis text-body-2 mb-6 ml-10">Respuestas predefinidas para consultas comunes</p>

      <div class="ml-md-10">
        <div class="mb-6">
          <v-hover v-slot="{ isHovering, props }" v-for="(faq, index) in faqs" :key="index">
            <v-card 
              v-bind="props"
              variant="outlined" 
              :class="['rounded-lg pa-4 mb-3 transition-swing', 
                       isHovering && editingIndex !== index ? 'bg-grey-lighten-4' : 'bg-transparent']"
            >
              <div v-if="editingIndex !== index" class="d-flex justify-space-between align-start">
                <div>
                  <div class="font-weight-bold text-body-1 mb-1">{{ faq.question }}</div>
                  <div class="text-medium-emphasis text-body-2">{{ faq.answer }}</div>
                </div>
                <div class="d-flex align-center">
                  <v-btn icon="mdi-pencil-outline" variant="text" color="medium-emphasis" density="comfortable" class="mr-1" @click="startEdit(index)"></v-btn>
                  <v-btn icon="mdi-delete-outline" variant="text" color="medium-emphasis" density="comfortable" @click="removeFaq(index)"></v-btn>
                </div>
              </div>

              <div v-else class="w-100">
                <v-text-field
                  v-model="editTempQuestion"
                  label="Pregunta"
                  variant="outlined"
                  density="compact"
                  bg-color="white"
                  class="mb-3"
                  hide-details
                ></v-text-field>

                <v-textarea
                  v-model="editTempAnswer"
                  label="Respuesta"
                  variant="outlined"
                  density="compact"
                  bg-color="white"
                  class="mb-3"
                  rows="2"
                  hide-details
                ></v-textarea>

                <div class="d-flex justify-end">
                  <v-btn variant="text" color="medium-emphasis" class="text-none mr-2" @click="cancelEdit">Cancelar</v-btn>
                  <v-btn color="primary" class="text-none" elevation="0" @click="saveEdit(index)">Guardar</v-btn>
                </div>
              </div>
            </v-card>
          </v-hover>
        </div>

        <v-sheet border="dashed" class="rounded-lg pa-5 bg-transparent border-opacity-50 border-medium-emphasis">
          <div class="font-weight-medium mb-4 d-flex align-center">
            <v-icon size="20" class="mr-2">mdi-plus</v-icon> Añadir nueva FAQ
          </div>
          
          <v-text-field
            v-model="newQuestion"
            placeholder="Pregunta..."
            variant="outlined"
            density="comfortable"
            bg-color="grey-lighten-5"
            class="mb-3"
            hide-details
          ></v-text-field>

          <v-textarea
            v-model="newAnswer"
            placeholder="Respuesta..."
            variant="outlined"
            density="comfortable"
            bg-color="grey-lighten-5"
            class="mb-4"
            rows="3"
            hide-details
          ></v-textarea>

          <v-btn 
            color="primary" 
            class="text-none font-weight-bold px-6" 
            rounded="lg"
            elevation="0"
            :disabled="!newQuestion || !newAnswer"
            @click="addFaq"
          >
            Añadir FAQ
          </v-btn>
        </v-sheet>
      </div>
    </v-card>

  </v-sheet>
</template>

<script setup lang="ts">
import { ref, computed } from 'vue'

const props = defineProps<{
  modelValue: {
    faqs: { id: number | null, question: string, answer: string, createdAt: string | null, updatedAt: string | null }[],
    documents: { name: string, id?: string, createdAt?: string }[]
  },
  canUploadDocuments?: boolean,
  documentUploadLimit?: number | null
}>()

const emit = defineEmits(['update:modelValue', 'update:pending-files', 'update:deleted-document-ids'])

const rawFilesList = ref<{ tempId: string, file: File }[]>([]) 
const chosenFiles = ref<File[]>([])
const deletedDocumentIds = ref<string[]>([]) // Track IDs of documents to delete from corpus

const documents = computed(() => props.modelValue.documents)
const canUploadDocuments = computed(() => props.canUploadDocuments ?? true)
const hasFiniteDocumentLimit = computed(() => props.documentUploadLimit !== null && props.documentUploadLimit !== undefined)
const remainingDocumentSlots = computed(() => {
  if (props.documentUploadLimit === null || props.documentUploadLimit === undefined) {
    return 0
  }

  return Math.max(props.documentUploadLimit - documents.value.length, 0)
})

const handleFileSelect = (files: File | File[] | null) => {
  if (!canUploadDocuments.value) return
  if (remainingDocumentSlots.value === 0) return

  if (!files) return
  const fileArray = Array.isArray(files) ? files : [files]
  if (fileArray.length === 0) return

  if (remainingDocumentSlots.value !== null && fileArray.length > remainingDocumentSlots.value) {
    return
  }

  const pendingEntries = fileArray.map(file => ({
    tempId: `tmp-${Date.now()}-${Math.random().toString(36).slice(2, 8)}`,
    file
  }))

  rawFilesList.value = [...rawFilesList.value, ...pendingEntries]
  emit('update:pending-files', rawFilesList.value.map(item => item.file))

  const newDocs = pendingEntries.map(({ file, tempId }) => ({
    name: file.name,
    id: tempId,
    createdAt: undefined
  }))
  
  emit('update:modelValue', {
    ...props.modelValue,
    documents: [...documents.value, ...newDocs]
  })

  chosenFiles.value = []
}

const removeDocument = (index: number) => {
  const docToRemove = documents.value[index]
  const updatedDocs = documents.value.filter((_, i) => i !== index)

  if (docToRemove?.id?.startsWith('tmp-')) {
    rawFilesList.value = rawFilesList.value.filter(entry => entry.tempId !== docToRemove.id)
    emit('update:pending-files', rawFilesList.value.map(item => item.file))
  } else if (docToRemove?.id && !docToRemove.id.startsWith('tmp-')) {
    if (!deletedDocumentIds.value.includes(docToRemove.id)) {
      deletedDocumentIds.value.push(docToRemove.id)
    }
    emit('update:deleted-document-ids', deletedDocumentIds.value)
  }
  
  emit('update:modelValue', { ...props.modelValue, documents: updatedDocs })
}

const faqs = computed(() => props.modelValue.faqs)
const newQuestion = ref('')
const newAnswer = ref('')

const addFaq = () => {
  const updatedFaqs = [...faqs.value, { id: null, question: newQuestion.value, answer: newAnswer.value, createdAt: null, updatedAt: null }]
  emit('update:modelValue', { ...props.modelValue, faqs: updatedFaqs })
  newQuestion.value = ''
  newAnswer.value = ''
}

const removeFaq = (index: number) => {
  const updatedFaqs = faqs.value.filter((_, i) => i !== index)
  emit('update:modelValue', { ...props.modelValue, faqs: updatedFaqs })
  
  if (editingIndex.value === index) cancelEdit()
}

const editingIndex = ref<number | null>(null)
const editTempQuestion = ref('')
const editTempAnswer = ref('')

const startEdit = (index: number) => {
  editingIndex.value = index
  if (faqs.value[index]) {
    editTempQuestion.value = faqs.value[index].question
    editTempAnswer.value = faqs.value[index].answer
  }
}

const cancelEdit = () => {
  editingIndex.value = null
  editTempQuestion.value = ''
  editTempAnswer.value = ''
}

const saveEdit = (index: number) => {
  const updatedFaqs = [...faqs.value]
  if (updatedFaqs[index]) {
    updatedFaqs[index] = { 
      ...updatedFaqs[index], 
      question: editTempQuestion.value, 
      answer: editTempAnswer.value 
    }
  }
  
  emit('update:modelValue', { ...props.modelValue, faqs: updatedFaqs })
  cancelEdit()
}

</script>