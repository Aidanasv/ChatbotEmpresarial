<template>
  <v-container fluid class="pa-0 chat-view-container">
    <v-row no-gutters class="h-100">
      
      <v-col cols="12" md="4" lg="3" class="chat-sidebar d-flex flex-column border-right">
        <div class="pa-4 border-bottom">
          <h2 class="text-h6 font-weight-bold mb-4">Bandeja de entrada</h2>
          <v-text-field
            density="compact"
            variant="outlined"
            prepend-inner-icon="mdi-magnify"
            placeholder="Buscar conversación..."
            hide-details
            bg-color="grey-lighten-4"
          ></v-text-field>
        </div>

        <v-list lines="two" class="flex-grow-1 overflow-y-auto pa-0 bg-transparent">
          <template v-for="(chat, index) in chatList" :key="chat.id">
            <v-list-item 
              :value="chat.id" 
              :active="activeChat === chat.id"
              @click="activeChat = chat.id"
              class="px-4 py-3 cursor-pointer chat-list-item"
              active-color="primary"
            >
              <template v-slot:prepend>
                <v-avatar :color="chat.status === 'Resuelto' ? 'grey-lighten-2' : 'primary'" variant="tonal" class="mr-3 font-weight-bold">
                  {{ chat.name.charAt(0) }}
                </v-avatar>
              </template>
              
              <v-list-item-title class="font-weight-bold text-body-2 d-flex justify-space-between">
                {{ chat.name }}
                <span class="text-caption text-medium-emphasis font-weight-regular">{{ chat.time }}</span>
              </v-list-item-title>
              
              <v-list-item-subtitle class="text-caption mt-1 text-truncate">
                {{ chat.lastMessage }}
              </v-list-item-subtitle>
            </v-list-item>
            <v-divider v-if="index < chatList.length - 1"></v-divider>
          </template>
        </v-list>
      </v-col>

      <v-col cols="12" md="8" lg="9" class="chat-main-area d-flex flex-column bg-grey-lighten-5">
        
        <div class="pa-4 bg-white border-bottom d-flex align-center justify-space-between">
          <div class="d-flex align-center">
            <v-avatar color="primary" variant="tonal" class="mr-3 font-weight-bold">C</v-avatar>
            <div>
              <div class="font-weight-bold text-body-1">Carlos M.</div>
              <div class="text-caption text-medium-emphasis d-flex align-center">
                <v-icon size="x-small" color="success" class="mr-1">mdi-circle</v-icon> Activo ahora
              </div>
            </div>
          </div>
          <v-btn variant="tonal" color="primary" size="small" class="text-none font-weight-bold">
            Tomar control manual
          </v-btn>
        </div>

        <div class="chat-history flex-grow-1 overflow-y-auto pa-6 d-flex flex-column" style="gap: 16px;">
          <div class="d-flex justify-center mb-2">
            <v-chip size="small" variant="flat" color="grey-lighten-3" class="text-caption text-medium-emphasis font-weight-medium">Hoy, 10:42 AM</v-chip>
          </div>

          <div class="d-flex justify-end">
            <div class="message-bubble user-bubble bg-primary text-white">
              Hola, ¿cuál es el horario de atención para soporte técnico?
            </div>
          </div>

          <div class="d-flex align-start">
            <v-avatar size="32" color="primary" class="mr-3 mt-1">
              <v-icon size="small" color="white">mdi-robot-outline</v-icon>
            </v-avatar>
            <div class="message-bubble bot-bubble bg-white">
              ¡Hola Carlos! Nuestro equipo de soporte técnico está disponible de Lunes a Viernes de 9:00 AM a 6:00 PM (Hora local). ¿Te puedo ayudar en algo más?
            </div>
          </div>
        </div>

        <div class="chat-input-area pa-4 bg-white border-top">
          <v-text-field
            v-model="newMessage"
            variant="outlined"
            placeholder="Escribe un mensaje para Carlos..."
            hide-details
            bg-color="white"
            append-inner-icon="mdi-send"
            @click:append-inner="sendMessage"
            @keyup.enter="sendMessage"
          >
            <template v-slot:prepend-inner>
              <v-btn icon="mdi-paperclip" variant="text" size="small" color="grey-darken-1" class="mr-1"></v-btn>
            </template>
          </v-text-field>
        </div>
      </v-col>

    </v-row>
  </v-container>
</template>

<script setup lang="ts">
import { ref } from 'vue'

const activeChat = ref(1)
const newMessage = ref('')

const chatList = [
  { id: 1, name: 'Carlos M.', lastMessage: 'Hola, ¿cuál es el horario de atención para soporte técnico?', time: '10:42 AM', status: 'Activo' },
  { id: 2, name: 'Ana L.', lastMessage: '¡Perfecto, muchas gracias!', time: 'Ayer', status: 'Resuelto' },
  { id: 3, name: 'Pedro R.', lastMessage: 'Necesito hablar con un humano por favor.', time: 'Ayer', status: 'Escalado' },
  { id: 4, name: 'María G.', lastMessage: '¿Cómo puedo cambiar mi contraseña?', time: 'Ayer', status: 'Resuelto' },
  { id: 5, name: 'Luis S.', lastMessage: 'El pedido no ha llegado a mi domicilio.', time: 'Lun', status: 'Escalado' }
]

const sendMessage = () => {
  if (!newMessage.value.trim()) return
  console.log('Enviando mensaje:', newMessage.value)
  newMessage.value = ''
}
</script>