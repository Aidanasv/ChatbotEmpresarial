<template>
    <v-container fluid class="pa-6 pa-md-8">
        <v-card class="dash-card rounded-xl" flat>
            <div class="pa-6 d-flex align-center justify-space-between flex-wrap ga-3">
                <div class="d-flex align-center">
                    <v-avatar color="primary" variant="tonal" size="42" class="mr-3">
                        <v-icon>mdi-robot-outline</v-icon>
                    </v-avatar>
                    <div>
                        <h2 class="text-h6 font-weight-bold mb-0">Modo prueba</h2>
                        <p class="text-body-2 text-medium-emphasis mb-0">Simula conversaciones con tu chatbot</p>
                    </div>
                </div>

                <div class="d-flex align-center ga-2">
                    <v-chip size="small" variant="outlined" color="default" class="font-weight-bold">
                        <v-icon start size="small">mdi-star-four-points-circle-outline</v-icon>
                        gpt-4o-mini
                    </v-chip>
                    <v-btn variant="outlined" rounded="lg" class="text-none" prepend-icon="mdi-refresh" @click="resetChat">
                        Reiniciar
                    </v-btn>
                </div>
            </div>
            <v-divider></v-divider>

            <div v-if="!hasConversation" class="pa-6">
                <v-card variant="outlined" class="pa-5 rounded-lg" max-width="640">
                    <h3 class="text-subtitle-1 font-weight-bold mb-1">Datos del cliente</h3>
                    <p class="text-body-2 text-medium-emphasis mb-4">Completa estos datos para iniciar la conversacion de prueba.</p>

                    <v-form @submit.prevent="startConversation">
                        <v-text-field
                            v-model="customerForm.name"
                            label="Nombre"
                            variant="outlined"
                            class="mb-3"
                            :rules="nameRules"
                            required
                        ></v-text-field>

                        <v-text-field
                            v-model="customerForm.email"
                            label="Correo"
                            type="email"
                            variant="outlined"
                            class="mb-3"
                            :rules="emailRules"
                            required
                        ></v-text-field>

                        <v-text-field
                            v-model="customerForm.phone"
                            label="Telefono"
                            variant="outlined"
                            class="mb-4"
                            :rules="phoneRules"
                            required
                        ></v-text-field>

                        <div class="d-flex justify-end">
                            <v-btn type="submit" color="primary" class="text-none" :loading="chatbotStore.isLoading">
                                Iniciar conversacion
                            </v-btn>
                        </div>
                    </v-form>
                </v-card>
            </div>

            <template v-else>
                <v-sheet class="pa-6 bg-grey-lighten-5 overflow-y-auto" min-height="420" max-height="calc(100vh - 280px)">
                    <div v-for="message in messages" :key="message.id" class="mb-4">
                        <div v-if="message.role === 'bot'" class="d-flex align-start">
                            <v-avatar size="34" color="primary" variant="tonal" class="mr-3 mt-1">
                                <v-icon size="18">mdi-robot-outline</v-icon>
                            </v-avatar>
                            <div>
                                <v-sheet class="pa-3 px-4 rounded-xl rounded-be-lg text-body-2" border color="white" style="max-width: min(680px, 80vw); line-height: 1.45;">
                                    {{ message.content }}
                                </v-sheet>
                                <div class="text-caption text-medium-emphasis mt-1">{{ message.time }}</div>
                            </div>
                        </div>

                        <div v-else class="d-flex justify-end">
                            <div class="text-right">
                                <v-sheet class="pa-3 px-4 rounded-xl rounded-bs-lg text-body-2 text-white" color="primary" style="max-width: min(680px, 80vw); line-height: 1.45;">
                                    {{ message.content }}
                                </v-sheet>
                                <div class="text-caption text-medium-emphasis mt-1">{{ message.time }}</div>
                            </div>
                        </div>
                    </div>
                </v-sheet>

                <v-divider></v-divider>
                <div class="pa-4">
                    <v-text-field
                        v-model="inputMessage"
                        variant="outlined"
                        placeholder="Escribe un mensaje para probar tu chatbot..."
                        hide-details
                        bg-color="white"
                        class="try-input"
                        @keyup.enter="sendMessage"
                    >
                        <template #append-inner>
                            <v-btn
                                icon="mdi-send"
                                color="primary"
                                size="small"
                                variant="flat"
                                :disabled="!inputMessage.trim() || chatbotStore.isLoading"
                                :loading="chatbotStore.isLoading"
                                @click="sendMessage"
                            ></v-btn>
                        </template>
                    </v-text-field>
                </div>
            </template>
        </v-card>
    </v-container>
</template>

<script setup lang="ts">
import { computed, ref } from 'vue'
import { useChatBotStore } from '@/stores/useChatBotStore'
import type { Conversation } from '@/types/chatbot'
import { useAuthStore } from '@/stores/useAuthStore'

const chatbotStore = useChatBotStore()
const authStore = useAuthStore()

const messages = computed(() => chatbotStore.conversations)
const hasConversation = computed(() => chatbotStore.conversationId !== null)
const inputMessage = ref('')
const customerForm = ref({
    name: '',
    email: '',
    phone: ''
})

const nameRules = [
    (value: string) => !!value.trim() || 'El nombre es obligatorio'
]

const emailRules = [
    (value: string) => !!value.trim() || 'El correo es obligatorio',
    (value: string) => /.+@.+\..+/.test(value) || 'Ingresa un correo valido'
]

const phoneRules = [
    (value: string) => !!value.trim() || 'El telefono es obligatorio',
    (value: string) => value.replace(/\D/g, '').length >= 7 || 'Ingresa un telefono valido'
]

const getTimeLabel = () => {
    const now = new Date()
    return now.toLocaleTimeString('es-ES', { hour: '2-digit', minute: '2-digit' })
}

const isCustomerFormValid = () => {
    const isNameValid = !!customerForm.value.name.trim()
    const isEmailValid = /.+@.+\..+/.test(customerForm.value.email)
    const isPhoneValid = customerForm.value.phone.replace(/\D/g, '').length >= 7
    return isNameValid && isEmailValid && isPhoneValid
}

const startConversation = async () => {
    if (!isCustomerFormValid()) return

    const payload: Conversation = {
        chatbotId: parseInt(authStore.chatbotId ?? '0'),
        customerName: customerForm.value.name.trim(),
        customerEmail: customerForm.value.email.trim(),
        customerPhone: customerForm.value.phone.trim(),
        topic: 'Modo prueba'
    }

    await chatbotStore.initializeChatbot(payload)
}

const sendMessage = async () => {
    const trimmed = inputMessage.value.trim()
    if (!trimmed || !hasConversation.value) return

    chatbotStore.conversations.push({ id: Date.now(), role: 'user', content: trimmed, time: getTimeLabel() })

    await chatbotStore.sendMenssage(trimmed)

    inputMessage.value = ''
}

const resetChat = () => {
    chatbotStore.conversations = []
    chatbotStore.conversationId = null
    inputMessage.value = ''
}
</script>
