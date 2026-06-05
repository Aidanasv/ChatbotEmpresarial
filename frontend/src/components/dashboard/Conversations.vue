<template>
  <v-container fluid class="pa-0 v-window-item--active" style="height: calc(100vh - 64px);">
    <v-row no-gutters class="h-100">

      <v-col cols="12" md="4" lg="3" class="d-flex flex-column border-e h-100 bg-surface">
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
                @click="activeChatId = chat.conversationId; conversationsStore.getConversationMessages(chat.conversationId)"
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

      <v-col cols="12" md="8" lg="9" class="d-flex flex-column bg-grey-lighten-5 h-100">

        <template v-if="conversationMessage">
          <div class="pa-4 bg-white border-b d-flex align-center justify-space-between flex-shrink-0">
            <div class="d-flex align-center">
              <v-avatar color="primary" variant="tonal" class="mr-3 font-weight-bold">
                {{ conversationMessage.customerName ? conversationMessage.customerName.charAt(0).toUpperCase() : 'C' }}
              </v-avatar>
              <div>
                <div class="font-weight-bold text-body-1">{{ conversationMessage.customerName || 'Cliente Anónimo' }}
                </div>
                <div class="text-caption text-medium-emphasis d-flex align-center">
                  <v-icon size="x-small"
                    :color="conversationMessage.status === 'Open' || conversationMessage.status === 'Abierta' ? 'success' : 'grey'"
                    class="mr-1">mdi-circle</v-icon>
                  {{ conversationMessage.status === 'Open' || conversationMessage.status === 'Abierta' ? 'Activo ahora'
                  : 'Cerrada' }}
                </div>
              </div>
            </div>
            <v-btn variant="tonal" color="primary" size="small" class="text-none font-weight-bold">
              Tomar control manual
            </v-btn>
          </div>

          <div class="flex-grow-1 overflow-y-auto pa-6 d-flex flex-column" style="gap: 16px;">
            <div v-for="message in conversationMessage.messages" :key="message.id" class="mb-4">
              <div v-if="message.role === 'Bot'" class="d-flex align-start">
                <v-avatar size="34" color="primary" variant="tonal" class="mr-3 mt-1">
                  <v-icon size="18">mdi-robot-outline</v-icon>
                </v-avatar>
                <div>
                  <v-sheet class="pa-3 px-4 rounded-xl rounded-be-lg text-body-2" border color="white"
                    style="max-width: min(680px, 80vw); line-height: 1.45;">
                    {{ message.content }}
                  </v-sheet>
                  <div class="text-caption text-medium-emphasis mt-1">{{ message.createdAt }}</div>
                </div>
              </div>

              <div v-else class="d-flex justify-end">
                <div class="text-right">
                  <v-sheet class="pa-3 px-4 rounded-xl rounded-bs-lg text-body-2 text-white" color="primary"
                    style="max-width: min(680px, 80vw); line-height: 1.45;">
                    {{ message.content }}
                  </v-sheet>
                  <div class="text-caption text-medium-emphasis mt-1">{{ message.createdAt }}</div>
                </div>
              </div>
            </div>
          </div>

          <div class="pa-4 bg-white border-t flex-shrink-0">
            <v-text-field v-model="newMessage" variant="outlined"
              :placeholder="`Escribe un mensaje para ${conversationMessage.customerName || 'el cliente'}...`"
              hide-details bg-color="white" append-inner-icon="mdi-send" @click:append-inner="sendMessage"
              @keyup.enter="sendMessage">
              <template v-slot:prepend-inner>
                <v-btn icon="mdi-paperclip" variant="text" size="small" color="grey-darken-1" class="mr-1"></v-btn>
              </template>
            </v-text-field>
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

const conversationsStore = useConversationsStore()
const analyticsStore = useAnalyticsStore()
const { conversations, limit, offset, conversationMessage } = storeToRefs(conversationsStore)
const { totalConversations } = storeToRefs(analyticsStore)

const activeChatId = ref<number | null>(null)
const newMessage = ref('')
const isLoading = ref(true)

const currentPage = ref(1)

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
  if (newConversations.length > 0 && activeChatId.value === null) {
    activeChatId.value = newConversations[0].conversationId
    conversationsStore.getConversationMessages(activeChatId.value)
  }
})

watch(currentPage, (newPage) => {
  const newOffset = (newPage - 1) * limit.value

  offset.value = newOffset
  loadConversations()
})

const sendMessage = () => {
  if (!newMessage.value.trim() || activeChatId.value === null) return
  console.log(`Enviando mensaje a la conversación ${activeChatId.value}:`, newMessage.value)
  newMessage.value = ''
}

onMounted(() => {
  limit.value = 7
  offset.value = 0
  loadConversations()
})
</script>