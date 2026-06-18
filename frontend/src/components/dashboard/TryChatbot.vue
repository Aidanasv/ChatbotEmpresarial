<template>
    <v-container fluid class="pa-6 pa-md-8 min-vh-100">
        <!-- Header -->
        <div class="d-flex align-center justify-space-between flex-wrap ga-4 mb-8 pa-6 rounded-xl"
            style="background: linear-gradient(135deg, rgba(83, 109, 254, 0.08) 0%, rgba(25, 118, 210, 0.08) 100%); border: 1px solid rgba(var(--v-theme-primary), 0.1);">
            <div class="d-flex align-center ga-4">
                <v-avatar color="primary" variant="tonal" size="56">
                    <v-icon size="28">mdi-robot-outline</v-icon>
                </v-avatar>
                <div>
                    <h1 class="text-h4 font-weight-bold mb-1">Modo prueba</h1>
                    <p class="text-body-2 text-medium-emphasis mb-0">Simula conversaciones con tu chatbot</p>
                </div>
            </div>
            <v-btn variant="tonal" color="error" @click="resetChat" v-if="hasConversation" class="text-none">
                <v-icon start>mdi-refresh</v-icon>
                Reiniciar
            </v-btn>
        </div>

        <v-row class="ga-0" align="start">
            <v-col cols="12" md="4" class="pr-md-4">
                <v-card rounded="xl" elevation="1">
                    <div class="pa-4 border-b d-flex align-center justify-space-between flex-wrap ga-2">
                        <div>
                            <h2 class="text-subtitle-1 font-weight-bold mb-1">Como embeber</h2>
                            <p class="text-caption text-medium-emphasis mb-0">Fragmento listo para pegar.</p>
                        </div>
                        <v-btn variant="tonal" color="primary" size="small" class="text-none" @click="copyEmbedSnippet">
                            {{ copyStatusText }}
                        </v-btn>
                    </div>

                    <div class="pa-4" style="background: #0f172a; color: #e2e8f0; overflow-x: auto;">
                        <pre
                            style="margin: 0; white-space: pre; font-size: 11px; line-height: 1.45;"><code>{{ embedSnippet }}</code></pre>
                    </div>
                </v-card>
            </v-col>

            <v-col cols="12" md="8" class="pl-md-4">
                <div class="d-flex justify-center">
                    <div v-if="!hasConversation" style="width: 100%; max-width: 600px;">
                        <v-card rounded="xl" class="pa-8" elevation="2">
                            <div class="mb-8">
                                <h2 class="text-h5 font-weight-bold mb-2">Datos del cliente</h2>
                                <p class="text-body-2 text-medium-emphasis mb-0">Completa estos datos para iniciar la
                                    conversacion de prueba</p>
                            </div>

                            <v-form @submit.prevent="startConversation" class="d-flex flex-column ga-4">
                                <v-text-field v-model="customerForm.name" label="Nombre" variant="outlined"
                                    :rules="nameRules" required>
                                </v-text-field>

                                <v-text-field v-model="customerForm.email" label="Correo electronico" type="email"
                                    variant="outlined" :rules="emailRules" required>
                                </v-text-field>

                                <v-text-field v-model="customerForm.phone" label="Telefono" variant="outlined"
                                    :rules="phoneRules" required>
                                </v-text-field>

                                <div class="d-flex justify-end pt-4">
                                    <v-btn type="submit" color="primary" size="large" rounded="lg"
                                        :loading="chatbotStore.isLoading" class="text-none font-weight-bold px-8">
                                        Iniciar conversacion
                                    </v-btn>
                                </div>
                            </v-form>
                        </v-card>
                    </div>

                    <template v-else>
                        <div class="d-flex justify-center align-start" style="width: 100%;">
                            <WidgetChatbot :model-value="modelValue" :messages="messages"
                                :onSendMessage="sendMessage" />
                        </div>
                    </template>
                </div>
            </v-col>
        </v-row>
    </v-container>
</template>

<script setup lang="ts">
import { computed, ref } from 'vue'
import WidgetChatbot from './widgetChatbot.vue'
import { useChatBotStore } from '@/stores/useChatBotStore'
import type { Conversation } from '@/types/chatbot'
import { useAuthStore } from '@/stores/useAuthStore'
import { useSetupStore } from '@/stores/useSetupStore'

const chatbotStore = useChatBotStore()
const setupStore = useSetupStore()
const authStore = useAuthStore()

const messages = computed(() => chatbotStore.conversations)
const hasConversation = computed(() => chatbotStore.conversationId !== null)
const inputMessage = ref('')
const copyStatusText = ref('Copiar codigo')
const customerForm = ref({
    name: '',
    email: '',
    phone: ''
})

const embedChatbotId = computed(() => authStore.chatbotId ?? '18')
const embedUrl = computed(() => `http://localhost:3000/my-chatbot/${embedChatbotId.value}`)
const embedSnippet = computed(() => `.widget-frame {
    inset: 0;
    width: 100vw;
    height: 100vh;
    border-radius: 0;
    background-color: transparent;
}

<iframe
    class="widget-frame"
    title="Chatbot embebido"
    src="${embedUrl.value}"
    loading="lazy"
    referrerpolicy="strict-origin-when-cross-origin"
    allow="clipboard-write"
></iframe>`)

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

const sendMessage = async (text: string) => {
    const trimmed = text.trim()
    if (!trimmed || !hasConversation.value) return


    chatbotStore.conversations.push({ id: Date.now(), role: 'user', content: trimmed, time: getTimeLabel() })
    await chatbotStore.sendMenssage(trimmed)
}

const resetChat = () => {
    chatbotStore.conversations = []
    chatbotStore.conversationId = null
    inputMessage.value = ''
}

const copyEmbedSnippet = async () => {
    try {
        await navigator.clipboard.writeText(embedSnippet.value)
        copyStatusText.value = 'Copiado'
    } catch {
        copyStatusText.value = 'No se pudo copiar'
    } finally {
        setTimeout(() => {
            copyStatusText.value = 'Copiar codigo'
        }, 1500)
    }
}

const modelValue = computed(() => ({
    primaryColor: setupStore.appearanceSetup.primaryColor ?? '#536DFE',
    showChatbotAvatar: setupStore.appearanceSetup.showChatbotAvatar ?? true,
    widgetPosition: setupStore.appearanceSetup.widgetPosition ?? false,
    title: setupStore.appearanceSetup.title ?? 'Asistente'
}))
</script>
