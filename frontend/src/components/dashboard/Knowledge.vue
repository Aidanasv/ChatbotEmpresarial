<template>
  <v-container fluid class="pa-6 pa-md-8 analytics-container">

    <v-alert v-if="documentUploadLimit === 0" type="warning" variant="tonal" class="mb-4">
      Tu plan no permite subir documentos. Solo puedes revisar las FAQs y los documentos ya existentes.
    </v-alert>

    <v-alert v-else-if="documentUploadLimit !== null" type="info" variant="tonal" class="mb-4">
      <span v-if="hasFiniteDocumentLimit && remainingDocumentSlots > 0">
        Tu plan permite {{ documentUploadLimit }} documentos y te quedan {{ remainingDocumentSlots }} cupos.
      </span>
      <span v-else>
        Has alcanzado el maximo de documentos permitido por tu plan.
      </span>
    </v-alert>

    <v-alert
      v-if="rebuildStatus.status !== 'idle'"
      :type="rebuildAlertType"
      variant="tonal"
      class="mb-4"
    >
      <div class="d-flex align-center justify-space-between">
        <div>
          <div class="font-weight-bold">Estado de reconstruccion del corpus: {{ rebuildStatus.status }}</div>
          <div class="text-body-2">{{ rebuildStatus.message || 'Sin mensaje.' }}</div>
        </div>
        <v-chip size="small" :color="rebuildChipColor" variant="flat">{{ rebuildStatus.status }}</v-chip>
      </div>
    </v-alert>

    <KnowledgeForm
      v-model="knowledge"
      :can-upload-documents="documentUploadLimit !== 0"
      :document-upload-limit="documentUploadLimit"
      @update:pending-files="onPendingFilesUpdate"
      @update:deleted-document-ids="onDeletedDocumentsUpdate"
    />

    <div class="d-flex justify-end mt-8">
      <v-btn color="primary" size="large" rounded="lg" @click="saveChanges" :loading="setupStore.isLoading"
        class="text-none font-weight-bold px-8">
        Guardar cambios
      </v-btn>
    </div>

  </v-container>
</template>

<script setup lang="ts">
import { computed, onMounted, onUnmounted, ref } from 'vue'
import { useAuthStore } from '@/stores/useAuthStore'
import { useSetupStore } from '@/stores/useSetupStore'
import KnowledgeForm from '@/components/setup/KnowledgeForm.vue'
import { getDocumentUploadLimitFromFeatures } from '@/utils/subscriptionPermissions'

const setupStore = useSetupStore()
const authStore = useAuthStore()

const knowledge = computed({
  get: () => setupStore.knowledgeSetup,
  set: (val) => { setupStore.knowledgeSetup = val }
})

const currentSubscriptionFeatures = ref<string[]>([])
const documentUploadLimit = computed(() => getDocumentUploadLimitFromFeatures(currentSubscriptionFeatures.value))
const hasFiniteDocumentLimit = computed(() => documentUploadLimit.value !== null)
const remainingDocumentSlots = computed(() => {
  if (documentUploadLimit.value === null) {
    return 0
  }

  return Math.max(documentUploadLimit.value - knowledge.value.documents.length, 0)
})

const pendingFiles = ref<File[]>([])
const deletedDocumentIds = ref<string[]>([])
const rebuildStatus = ref<{
  status: 'idle' | 'running' | 'succeeded' | 'failed',
  message?: string
}>({ status: 'idle' })

let rebuildInterval: ReturnType<typeof setInterval> | null = null

const rebuildAlertType = computed(() => {
  if (rebuildStatus.value.status === 'failed') return 'error'
  if (rebuildStatus.value.status === 'succeeded') return 'success'
  return 'info'
})

const rebuildChipColor = computed(() => {
  if (rebuildStatus.value.status === 'failed') return 'error'
  if (rebuildStatus.value.status === 'succeeded') return 'success'
  return 'primary'
})

const onPendingFilesUpdate = (files: File[]) => {
  pendingFiles.value = files
}

const onDeletedDocumentsUpdate = (ids: string[]) => {
  deletedDocumentIds.value = ids
}

const stopRebuildPolling = () => {
  if (rebuildInterval) {
    clearInterval(rebuildInterval)
    rebuildInterval = null
  }
}

const fetchRebuildStatus = async () => {
  const status = await setupStore.getCorpusRebuildStatus()
  rebuildStatus.value = {
    status: status.status,
    message: status.message
  }

  if (status.status === 'succeeded' || status.status === 'failed' || status.status === 'idle') {
    stopRebuildPolling()
  }
}

const startRebuildPolling = () => {
  stopRebuildPolling()
  rebuildInterval = setInterval(async () => {
    try {
      await fetchRebuildStatus()
    } catch (error) {
      console.error('Error consultando estado de reconstruccion:', error)
    }
  }, 3000)
}

const saveChanges = async () => {
  try {
    const hasFileChanges = pendingFiles.value.length > 0 || deletedDocumentIds.value.length > 0

    await setupStore.saveKnowledgeSetup(knowledge.value, pendingFiles.value, deletedDocumentIds.value)
    pendingFiles.value = []
    deletedDocumentIds.value = []

    if (hasFileChanges) {
      rebuildStatus.value = {
        status: 'running',
        message: 'Reconstruccion en batch iniciada...'
      }
      startRebuildPolling()
    }

    console.log("¡Conocimiento del chatbot guardado correctamente!")
  } catch (error) {
    console.error("Error al guardar el conocimiento:", error)
  }
}

onMounted(async () => {
  try {
    if (authStore.companyId) {
      currentSubscriptionFeatures.value = await setupStore.getCurrentSubscriptionFeatures(Number(authStore.companyId))
    } else {
      currentSubscriptionFeatures.value = await setupStore.getCurrentSubscriptionFeatures()
    }

    await fetchRebuildStatus()
    if (rebuildStatus.value.status === 'running') {
      startRebuildPolling()
    }
  } catch (error) {
    console.error('Error cargando estado inicial de reconstruccion:', error)
  }
})

onUnmounted(() => {
  stopRebuildPolling()
})
</script>