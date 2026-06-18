<template>
    <div class="d-flex flex-column pa-0" :style="{ maxWidth: props.minWidth ?? '350px', width: '100%' }">

        <v-responsive :aspect-ratio="9 / 16" class="w-100 rounded-xl elevation-4 overflow-hidden" maxHeight="600px"
            :class="modelValue.widgetPosition ? 'ml-auto' : 'mr-auto'">
            <v-card class="d-flex flex-column h-100" variant="flat" tile>

                <div class="d-flex align-center ga-3 pa-3"
                    :style="{ backgroundColor: primaryColor, color: contrastColor }">
                    <div v-if="modelValue.showChatbotAvatar" class="d-flex align-center justify-center rounded-lg"
                        style="background-color: rgba(255,255,255,0.25); width: 32px; height: 32px;">
                        <v-icon :color="contrastColor" size="18">mdi-message-text</v-icon>
                    </div>

                    <div>
                        <div class="text-body-2 font-weight-bold">{{ modelValue.title ?? 'Asistente' }}</div>
                    </div>

                    <v-btn v-if="props.onClose" icon size="small" variant="flat" :color="contrastColor" class="ml-auto"
                        @click="props.onClose?.()">
                        <v-icon size="18">mdi-close</v-icon>
                    </v-btn>
                </div>

                <v-card-text ref="chatHistory"
                    class="pa-3 d-flex flex-column flex-grow-1 overflow-y-auto ga-2 bg-white">
                    <div v-for="msg in displayMessages" :key="msg.id" :class="[
                        'rounded-lg pa-2 text-caption d-flex flex-column',
                        msg.role === 'user' ? 'align-self-end text-white' : 'align-self-start bg-grey-lighten-4 text-grey-darken-4'
                    ]" :style="[
                msg.role === 'user' ? { backgroundColor: primaryColor, color: contrastColor } : {},
                { maxWidth: '80%', wordBreak: 'break-word', minWidth: '60%' }
            ]">
                        <div v-if="msg.role === 'bot'" v-html="md.render(msg.content)"></div>

                        <span v-else>{{ msg.content }}</span>

                        <span class="mt-1 align-self-end text-right opacity-70" style="font-size: 0.65rem;">
                            {{ msg.time }}
                        </span>
                    </div>
                </v-card-text>

                <v-divider></v-divider>

                <div class="pa-4 bg-white">
                    <v-text-field v-model="inputMessage" variant="outlined" rounded="xl"
                        placeholder="Escribe un mensaje..." hide-details bg-color="white"
                        @keyup.enter="handleSendMessage">
                        <template #append-inner>
                            <v-btn icon="mdi-send" :color="primaryColor" size="small" variant="flat"
                                :disabled="!inputMessage.trim() || isLoading" :loading="isLoading"
                                @click="handleSendMessage"></v-btn>
                        </template>
                    </v-text-field>
                </div>
            </v-card>
        </v-responsive>

        <br />

        <v-btn v-if="!props.onSendMessage" icon size="large" :color="primaryColor" elevation="4"
            :class="modelValue.widgetPosition ? 'ml-auto' : 'mr-auto'">
            <v-icon :color="contrastColor">mdi-message-text</v-icon>
        </v-btn>

    </div>
</template>

<script setup lang="ts">
import MarkdownIt from 'markdown-it'
import { computed, ref, watch, nextTick, onMounted } from 'vue'
import { useTheme } from 'vuetify'

export interface Message {
    id: number;
    role: 'user' | 'bot';
    content: string;
    time: string;
}

const md = new MarkdownIt({
    html: false,
    linkify: true,
    typographer: true
})

export interface ChatbotSettings {
    primaryColor: string;
    showChatbotAvatar: boolean;
    widgetPosition: boolean;
    title?: string;
}

const props = defineProps<{
    modelValue: ChatbotSettings;
    messages?: Message[];
    onSendMessage?: (message: string) => Promise<void> | void;
    onClose?: () => void;
    minWidth?: string;
}>()

const isLoading = ref(false)
const inputMessage = ref('')
const theme = useTheme()
const chatHistory = ref<any>(null)

const getCurrentTime = () => {
    const now = new Date()
    return `${now.getHours().toString().padStart(2, '0')}:${now.getMinutes().toString().padStart(2, '0')}`
}

const defaultMessages: Message[] = [
    { id: 1, role: 'bot', content: '¡Hola! 👋 ¿En qué puedo ayudarte?', time: getCurrentTime() },
    { id: 2, role: 'user', content: 'Quiero saber los precios', time: getCurrentTime() },
    { id: 3, role: 'bot', content: '¡Claro! Tenemos planes desde **$29/mes**. ¿Te cuento más?', time: getCurrentTime() }
]

const displayMessages = computed(() => {
    return props.messages !== undefined ? props.messages : defaultMessages
})

// FUNCIÓN DE SCROLL AUTOMÁTICO AL FINAL
const scrollToBottom = () => {
    nextTick(() => {
        if (chatHistory.value) {
            const element = chatHistory.value.$el || chatHistory.value
            element.scrollTop = element.scrollHeight
        }
    })
}

watch(() => displayMessages.value, () => {
    scrollToBottom()
}, { deep: true })

onMounted(() => {
    scrollToBottom()
})

const handleSendMessage = async () => {
    if (!inputMessage.value.trim() || isLoading.value) return

    const text = inputMessage.value

    if (props.onSendMessage) {
        isLoading.value = true
        try {
            inputMessage.value = ''
            await props.onSendMessage(text)
        } finally {
            isLoading.value = false
        }
    } else {
        isLoading.value = true
        setTimeout(() => {
            inputMessage.value = ''
            isLoading.value = false
        }, 1000)
    }
}

const themePrimaryColor = computed<string>(() => String(theme.current.value.colors.primary ?? '#536DFE'))

const primaryColor = computed<string>(() => {
    const color = props.modelValue.primaryColor?.trim()
    return color ? color : themePrimaryColor.value
})

const contrastColor = computed(() => {
    if (isTransparentColor(primaryColor.value)) {
        return String(theme.current.value.colors['on-surface'] ?? '#111827')
    }
    return getContrastColor(primaryColor.value)
})

const isTransparentColor = (color: string) => {
    const normalized = color.toLowerCase()
    return normalized === 'transparent'
        || normalized === 'rgba(0, 0, 0, 0)'
        || normalized === '#00000000'
}

const getContrastColor = (hexColor: string) => {
    const hex = hexColor.replace('#', '')
    const normalized = hex.length === 3
        ? hex.split('').map((value) => value + value).join('')
        : hex

    const red = parseInt(normalized.slice(0, 2), 16)
    const green = parseInt(normalized.slice(2, 4), 16)
    const blue = parseInt(normalized.slice(4, 6), 16)

    const luminance = (0.299 * red) + (0.587 * green) + (0.114 * blue)
    return luminance > 186 ? '#111827' : '#FFFFFF'
}
</script>