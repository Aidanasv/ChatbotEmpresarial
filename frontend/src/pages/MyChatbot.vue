<template>
    <v-container fluid class="mychatbot-page pa-0 fill-height">
        <div v-if="isEmbedAvailable" :class="`d-flex align-end ${horizontalAlignClass} w-100 h-100 pa-3 pa-sm-4`">
            <v-btn
                v-if="!isChatOpen"
                icon
                size="x-large"
                :color="modelValue.primaryColor"
                elevation="0"
                class="bubble-launcher"
                @click="isChatOpen = true"
            >
                <v-icon color="white">mdi-message-text</v-icon>
            </v-btn>

            <div v-else :class="`chat-stage d-flex align-end ${horizontalAlignClass} w-100`">
                <v-card
                    v-if="!hasConversation"
                    rounded="xl"
                    elevation="10"
                    class="start-card"
                    style="width: min(100vw - 24px, 430px);"
                >
                    <div class="d-flex align-center justify-space-between px-5 py-4">
                        <div>
                            <div class="text-subtitle-1 font-weight-bold">Iniciar conversacion</div>
                            <div class="text-caption text-medium-emphasis">Completa tus datos para comenzar el chat</div>
                        </div>

                        <v-btn icon size="small" variant="tonal" color="primary" @click="isChatOpen = false">
                            <v-icon size="18">mdi-close</v-icon>
                        </v-btn>
                    </div>

                    <v-divider />

                    <v-form @submit.prevent="startConversation" class="pa-5 d-flex flex-column ga-3">
                        <div class="start-helper text-body-2">
                            Llena tus datos para iniciar la conversacion con el asistente.
                        </div>

                        <v-text-field
                            v-model="customerForm.name"
                            label="Nombre"
                            variant="outlined"
                            :rules="nameRules"
                            hide-details="auto"
                            density="comfortable"
                            required
                        />

                        <v-text-field
                            v-model="customerForm.email"
                            label="Correo electronico"
                            type="email"
                            variant="outlined"
                            :rules="emailRules"
                            hide-details="auto"
                            density="comfortable"
                            required
                        />

                        <v-text-field
                            v-model="customerForm.phone"
                            label="Telefono"
                            variant="outlined"
                            :rules="phoneRules"
                            hide-details="auto"
                            density="comfortable"
                            required
                        />

                        <v-btn
                            type="submit"
                            color="primary"
                            size="large"
                            rounded="lg"
                            class="text-none font-weight-bold"
                            :loading="chatbotStore.isLoading"
                        >
                            Iniciar chat
                        </v-btn>
                    </v-form>
                </v-card>

                <WidgetChatbot
                    v-else
                    :model-value="modelValue"
                    :messages="messages"
                    :onSendMessage="sendMessage"
                    :onClose="() => { isChatOpen = false }"
                    min-height="min(100dvh - 24px, 640px)"
                    min-width="min(100vw - 24px, 390px)"
                />
            </div>
        </div>

        <div v-else class="d-flex align-center justify-center w-100 h-100 pa-4">
            <v-card v-if="isLoadingConfig" elevation="0" rounded="lg" class="pa-6 text-center">
                <v-progress-circular indeterminate color="primary" size="30" class="mb-3" />
                <div class="text-body-2 text-medium-emphasis">Cargando chatbot...</div>
            </v-card>

            <v-alert v-else type="warning" variant="tonal" border="start" class="w-100" style="max-width: 420px;">
                {{ errorMessage || 'Este chatbot no esta disponible para este dominio.' }}
            </v-alert>
        </div>

        <v-alert
            v-if="errorMessage && isEmbedAvailable"
            type="error"
            variant="tonal"
            class="position-absolute"
            style="left: 12px; right: 12px; top: 12px; z-index: 20;"
        >
            {{ errorMessage }}
        </v-alert>
    </v-container>
</template>

<script setup lang="ts">
import { computed, onMounted, onUnmounted, ref, watch } from 'vue'
import { useRoute } from 'vue-router'
import axios from 'axios'
import { useTheme } from 'vuetify'
import WidgetChatbot from '@/components/dashboard/widgetChatbot.vue'
import { useChatBotStore } from '@/stores/useChatBotStore'
import type { Conversation, Message } from '@/types/chatbot'

const route = useRoute()
const chatbotStore = useChatBotStore()
const theme = useTheme()

const errorMessage = ref('')
const isChatOpen = ref(false)
const isEmbedAvailable = ref(false)
const isLoadingConfig = ref(true)
const embedConfig = ref({
    primaryColor: '',
    showChatbotAvatar: true,
    widgetPosition: true,
    title: 'Asistente'
})

const customerForm = ref({
    name: '',
    email: '',
    phone: ''
})

const getEmbedOrigin = () => {
    if (!document.referrer) return window.location.origin

    try {
        return new URL(document.referrer).origin
    } catch {
        return window.location.origin
    }
}

const chatbotId = computed<number>(() => {
    const parsed = Number(route.params.chatbotId)
    return Number.isFinite(parsed) ? parsed : 0
})

const modelValue = computed(() => ({
    primaryColor: embedConfig.value.primaryColor,
    showChatbotAvatar: embedConfig.value.showChatbotAvatar,
    widgetPosition: embedConfig.value.widgetPosition,
    title: embedConfig.value.title
}))

const horizontalAlignClass = computed(() => modelValue.value.widgetPosition ? 'justify-end' : 'justify-start')

const messages = computed<Message[]>(() => chatbotStore.conversations)
const hasConversation = computed(() => chatbotStore.conversationId !== null)

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

const resetConversation = () => {
    chatbotStore.conversations = []
    chatbotStore.conversationId = null
    errorMessage.value = ''
}

const isCustomerFormValid = () => {
    const isNameValid = !!customerForm.value.name.trim()
    const isEmailValid = /.+@.+\..+/.test(customerForm.value.email)
    const isPhoneValid = customerForm.value.phone.replace(/\D/g, '').length >= 7
    return isNameValid && isEmailValid && isPhoneValid
}

const startConversation = async () => {
    if (!isEmbedAvailable.value || !isCustomerFormValid() || !chatbotId.value) return

    const payload: Conversation = {
        chatbotId: chatbotId.value,
        customerName: customerForm.value.name.trim(),
        customerEmail: customerForm.value.email.trim(),
        customerPhone: customerForm.value.phone.trim(),
        topic: 'Widget iframe'
    }

    await chatbotStore.initializeChatbot(payload)

    if (!chatbotStore.conversationId) {
        errorMessage.value = 'No se pudo iniciar la conversacion. Intenta nuevamente.'
    }
}

const loadEmbedConfig = async () => {
    isLoadingConfig.value = true

    if (!chatbotId.value) {
        isEmbedAvailable.value = false
        isChatOpen.value = false
        errorMessage.value = 'El chatbot no es valido para esta URL.'
        isLoadingConfig.value = false
        return
    }

    try {
        errorMessage.value = ''
        const response = await axios.get(`http://localhost:5267/api/chatbot/embed/${chatbotId.value}`)
        const apiPrimaryColor = String(response.data.primaryColor ?? '#536DFE').trim() || '#536DFE'

        if (theme.themes.value.embedTheme) {
            theme.themes.value.embedTheme.colors.primary = apiPrimaryColor
        }

        isEmbedAvailable.value = true
        embedConfig.value = {
            primaryColor: apiPrimaryColor,
            showChatbotAvatar: Boolean(response.data.showChatbotAvatar ?? true),
            widgetPosition: Boolean(response.data.widgetPosition ?? true),
            title: String(response.data.title ?? 'Asistente')
        }
    } catch {
        isEmbedAvailable.value = false
        isChatOpen.value = false
        errorMessage.value = 'Este chatbot no esta disponible para este dominio.'
    } finally {
        isLoadingConfig.value = false
    }
}

const sendMessage = async (text: string) => {
    const trimmed = text.trim()
    if (!trimmed || !hasConversation.value) return

    errorMessage.value = ''
    chatbotStore.conversations.push({
        id: Date.now(),
        role: 'user',
        content: trimmed,
        time: getTimeLabel()
    })

    await chatbotStore.sendMenssage(trimmed)

    if (chatbotStore.error) {
        errorMessage.value = 'Ocurrio un error al enviar el mensaje.'
    }
}

watch(chatbotId, () => {
    resetConversation()
    customerForm.value = { name: '', email: '', phone: '' }
    loadEmbedConfig()
})

onMounted(() => {
    axios.defaults.headers.common['X-Embed-Origin'] = getEmbedOrigin()

    resetConversation()
    loadEmbedConfig()
})

onUnmounted(() => {
    delete axios.defaults.headers.common['X-Embed-Origin']
})
</script>

<style scoped>
.mychatbot-page {
    background: transparent;
}

.bubble-launcher {
    box-shadow: 0 14px 30px rgba(0, 0, 0, 0.18), 0 8px 18px rgba(var(--v-theme-primary), 0.28) !important;
}

.start-card {
    border: 1px solid rgba(var(--v-theme-on-surface), 0.08);
    backdrop-filter: blur(4px);
}

.start-helper {
    color: rgba(var(--v-theme-on-surface), 0.82);
    background: rgba(var(--v-theme-primary), 0.08);
    border: 1px solid rgba(var(--v-theme-primary), 0.2);
    border-radius: 10px;
    padding: 10px 12px;
}
</style>