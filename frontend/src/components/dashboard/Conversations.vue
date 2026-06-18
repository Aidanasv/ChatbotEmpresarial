<template>
  <v-container fluid class="pa-0 v-window-item--active chat-view-container dashboard-full-height">
    <v-row no-gutters class="h-100">

      <v-col v-if="!smAndDown || activeChatId === null" cols="12" md="4" lg="5" class="d-flex flex-column border-e h-100 bg-surface">
        <div class="pa-4 border-b">
          <h2 class="text-h6 font-weight-bold mb-4">Bandeja de entrada</h2>
          <v-text-field density="compact" variant="outlined" prepend-inner-icon="mdi-magnify"
            placeholder="Buscar conversación..." hide-details bg-color="grey-lighten-4"></v-text-field>
        </div>

        <div v-if="isLoading" class="d-flex justify-center align-center flex-grow-1">
          <v-progress-circular indeterminate color="primary"></v-progress-circular>
        </div>

        <div v-else-if="conversations.length === 0"
          class="d-flex flex-column justify-center align-center flex-grow-1 pa-4 text-center">
          <v-icon icon="mdi-message-text-off-outline" size="x-large" color="disabled" class="mb-2"></v-icon>
          <p class="text-body-2 text-medium-emphasis">No hay conversaciones</p>
        </div>

        <template v-else>
          <v-list lines="two" class="flex-grow-1 overflow-y-auto pa-0 bg-transparent">
            <template v-for="(chat, index) in conversations" :key="chat.conversationId">
              <v-list-item :value="chat.conversationId" :active="activeChatId === chat.conversationId"
                @click="selectConversation(chat.conversationId)"
                class="px-4 py-3 v-card--link" active-color="primary">
                <template v-slot:prepend>
                  <v-avatar
                    :color="chat.status === 'Resolved' || chat.status === 'Resuelto' ? 'grey-lighten-2' : 'primary'"
                    variant="tonal" class="mr-3 font-weight-bold">
                    {{ chat.customerName ? chat.customerName.charAt(0).toUpperCase() : 'C' }}
                  </v-avatar>
                </template>

                <v-list-item-title class="font-weight-bold text-body-2 d-flex justify-space-between align-center">
                  <span class="text-truncate mr-2">{{ chat.customerName || 'Cliente Anónimo' }}</span>
                  <span class="text-caption text-medium-emphasis font-weight-regular flex-shrink-0">
                    {{ chat.lastMessageTime }}
                  </span>
                </v-list-item-title>

                <v-list-item-subtitle class="text-caption mt-1 text-truncate">
                  {{ chat.lastMessage || 'Sin mensajes aún' }}
                </v-list-item-subtitle>
              </v-list-item>
              <v-divider v-if="index < conversations.length - 1" class="border-b"></v-divider>
            </template>
          </v-list>

          <v-pagination v-model="currentPage"
            :length="totalConversations > 0 ? Math.ceil(totalConversations / limit) : 1" total-visible="3"
            density="comfortable" class="my-2 border-t pt-2 flex-shrink-0"></v-pagination>
        </template>
      </v-col>

      <v-col v-if="!smAndDown || activeChatId !== null" cols="12" md="8" lg="7" class="d-flex flex-column bg-grey-lighten-5 h-100">

        <template v-if="conversationMessage">
          <div class="pa-4 bg-white border-b d-flex align-center justify-space-between flex-shrink-0">
            <div class="d-flex align-center">
              <v-btn v-if="smAndDown" icon="mdi-arrow-left" variant="text" size="small" class="mr-2" @click="goBackToList"></v-btn>
              <v-avatar color="primary" variant="tonal" class="mr-3 font-weight-bold">
                {{ conversationMessage.customerName ? conversationMessage.customerName.charAt(0).toUpperCase() : 'C' }}
              </v-avatar>
              <div>
                <div class="font-weight-bold text-body-1">{{ conversationMessage.customerName || 'Cliente Anónimo' }}
                </div>
                <v-chip
                  size="small"
                  variant="tonal"
                  class="mt-1"
                  :color="conversationMessage.status === 'Open' || conversationMessage.status === 'Abierta' ? 'success' : 'grey'"
                >
                  {{ conversationMessage.status === 'Open' || conversationMessage.status === 'Abierta' ? 'Abierta' : 'Cerrada' }}
                </v-chip>
    
              
                <div v-if="conversationMessage.customerEmail" class="text-caption text-medium-emphasis d-flex align-center mt-1">
                  <v-icon size="x-small" class="mr-1">mdi-email-outline</v-icon>
                  {{ conversationMessage.customerEmail }}
                </div>
                <div v-if="conversationMessage.customerPhone" class="text-caption text-medium-emphasis d-flex align-center">
                  <v-icon size="x-small" class="mr-1">mdi-phone-outline</v-icon>
                  {{ conversationMessage.customerPhone }}
                </div>
              </div>
            </div>
          </div>

          <div class="flex-grow-1 overflow-y-auto pa-4 pa-md-6 d-flex flex-column dashboard-chat-thread">
            <div v-for="message in conversationMessage.messages" :key="message.id" class="mb-4">
              <div v-if="isBotMessage(message.role)" class="d-flex align-start">
            
                <div>
                  <v-sheet class="pa-3 px-4 rounded-xl rounded-be-lg text-body-2 dashboard-chat-bubble dashboard-chat-bubble--bot" border color="white">
                    <div v-html="renderBotMessage(message.content)"></div>
                  </v-sheet>
                  <div class="text-caption text-medium-emphasis mt-1">{{ formatTime(message.createdAt) }}</div>
                </div>
              </div>

              <div v-else class="d-flex justify-end">
                <div class="text-right">
                  <v-sheet class="pa-3 px-4 rounded-xl rounded-bs-lg text-body-2 text-white dashboard-chat-bubble dashboard-chat-bubble--user" color="primary">
                    {{ message.content }}
                  </v-sheet>
                  <div class="text-caption text-medium-emphasis mt-1">{{ formatTime(message.createdAt) }}</div>
                </div>
              </div>
            </div>
          </div>

        </template>

        <template v-else>
          <div class="d-flex flex-column justify-center align-center flex-grow-1 text-center text-medium-emphasis">
            <v-icon icon="mdi-forum-outline" size="100" color="grey-lighten-1" class="mb-4"></v-icon>
            <h3 class="text-h6 font-weight-bold">No hay ninguna conversación seleccionada</h3>
            <p class="text-body-2">Elige un chat de la lista izquierda para comenzar el seguimiento</p>
          </div>
        </template>

      </v-col>
    </v-row>
  </v-container>
</template>

<script setup lang="ts">
import { ref, computed, onMounted, watch } from 'vue'
import { storeToRefs } from 'pinia'
import { useConversationsStore } from '@/stores/useConversations'
import { useAnalyticsStore } from '@/stores/useAnalyticsStore'
import { useRoute } from 'vue-router'
import { useDisplay } from 'vuetify'
import MarkdownIt from 'markdown-it'

const conversationsStore = useConversationsStore()
const analyticsStore = useAnalyticsStore()
const route = useRoute()
const { smAndDown } = useDisplay()
const { conversations, limit, offset, conversationMessage } = storeToRefs(conversationsStore)
const { totalConversations } = storeToRefs(analyticsStore)

const activeChatId = ref<number | null>(null)
const isLoading = ref(true)

const currentPage = ref(1)
const md = new MarkdownIt({
  html: true,
  linkify: true,
  typographer: true
})

const formatTime = (value: string): string => {
  const parsedDate = new Date(value)

  if (Number.isNaN(parsedDate.getTime())) {
    return value
  }

  return parsedDate.toLocaleTimeString('es-ES', {
    hour: '2-digit',
    minute: '2-digit'
  })
}

const isBotMessage = (role: string): boolean => role.toLowerCase() === 'bot'

const renderBotMessage = (content: string): string => md.render(content)

const selectConversation = async (conversationId: number) => {
  activeChatId.value = conversationId
  await conversationsStore.getConversationMessages(conversationId)
}

const goBackToList = () => {
  activeChatId.value = null
}

const selectConversationFromQuery = async (): Promise<boolean> => {
  const conversationIdQuery = route.query.conversationId
  const targetConversationId = Number(conversationIdQuery)

  if (!Number.isFinite(targetConversationId) || targetConversationId <= 0) {
    return false
  }

  activeChatId.value = targetConversationId
  await conversationsStore.getConversationMessages(targetConversationId)
  return true
}

const loadConversations = async () => {
  try {
    isLoading.value = true
    await conversationsStore.getConversationData(limit.value, offset.value)
  } catch (error) {
    console.error('Error cargando las conversaciones:', error)
  } finally {
    isLoading.value = false
  }
}

watch(conversations, (newConversations) => {
  if (route.query.conversationId) {
    return
  }

  if (smAndDown.value) {
    return
  }

  const [firstConversation] = newConversations

  if (firstConversation && activeChatId.value === null) {
    selectConversation(firstConversation.conversationId)
  }
})

watch(() => route.query.conversationId, async (newConversationId) => {
  if (!newConversationId) {
    return
  }

  await selectConversationFromQuery()
})

watch(currentPage, (newPage) => {
  const newOffset = (newPage - 1) * limit.value

  offset.value = newOffset
  loadConversations()
})



onMounted(async () => {
  limit.value = 7
  offset.value = 0
  await loadConversations()

  const selectedFromQuery = await selectConversationFromQuery()
  const [firstConversation] = conversations.value

  if (!selectedFromQuery && !smAndDown.value && firstConversation && activeChatId.value === null) {
    await selectConversation(firstConversation.conversationId)
  }
})
</script>