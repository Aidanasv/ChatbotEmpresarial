    <template>
        <v-container class="d-flex flex-column w-100 pa-0">
            <v-card rounded="xl" class="d-flex flex-column w-100" elevation="4"
                :class="modelValue.widgetPosition ? 'ml-auto' : 'mr-auto'"
                :style="{ minHeight: props.minHeight ?? '400px', minWidth: props.minWidth ?? '150px' }">

                <div class="rounded-t-xl d-flex align-center ga-3 pa-3"
                    :style="{ backgroundColor: primaryColor, color: contrastColor }">
                    <div v-if="modelValue.showChatbotAvatar"
                        class="widget-avatar-icon d-flex align-center justify-center rounded-lg"
                        style="background-color: rgba(255,255,255,0.25); width: 32px; height: 32px;">
                        <v-icon :color="contrastColor" size="18">mdi-message-text</v-icon>
                    </div>
                    <div>
                        <div class="text-body-2 font-weight-bold">{{ modelValue.title ?? 'Asistente' }}</div>
                        <div class="text-caption" style="opacity: 0.7;">En línea</div>
                    </div>

                    <v-btn v-if="props.onClose" icon size="small" variant="flat" :color="contrastColor" class="ml-auto"
                        @click="props.onClose?.()">
                        <v-icon size="18">mdi-close</v-icon>
                    </v-btn>

                </div>

                <div class="pa-3 d-flex flex-column" style="gap: 8px;" justify-content="flex-start"
                    :style="{ minHeight: 'calc(' + (props.minHeight ?? '400px') + ' - 120px)' }">
                    <div v-for="msg in displayMessages" :key="msg.id" :class="[
                        'rounded-lg pa-2 text-caption d-flex flex-column',
                        msg.role === 'user' ? 'align-self-end text-white' : 'align-self-start bg-grey-lighten-4'
                    ]" :style="msg.role === 'user' ? { backgroundColor: primaryColor, color: contrastColor } : {}"
                        style="max-width: 80%;">
                        <div v-if="msg.role === 'bot'" v-html="md.render(msg.content)" class="chat-markdown"></div>

                        <span v-else>{{ msg.content }}</span>

                        <span class="chat-timestamp mt-1">
                            {{ msg.time }}
                        </span>
                    </div>
                </div>

                <v-divider></v-divider>

                <div class="pa-4">
                    <v-text-field v-model="inputMessage" variant="outlined" placeholder="Escribe un mensaje..."
                        hide-details bg-color="white" class="try-input" @keyup.enter="handleSendMessage">
                        <template #append-inner>
                            <v-btn icon="mdi-send" :color="primaryColor" size="small" variant="flat"
                                :disabled="!inputMessage.trim() || isLoading" :loading="isLoading"
                                @click="handleSendMessage"></v-btn>
                        </template>
                    </v-text-field>
                </div>
            </v-card>

            <br />

            <v-btn v-if="!props.onSendMessage" icon size="large" :color="primaryColor" elevation="4"
                :class="modelValue.widgetPosition ? 'ml-auto' : 'mr-auto'">
                <v-icon :color="contrastColor">mdi-message-text</v-icon>
            </v-btn>

        </v-container>
    </template>

<script setup lang="ts">
import MarkdownIt from 'markdown-it'
import { computed, ref } from 'vue'
import { useTheme } from 'vuetify'

export interface Message {
    id: number;
    role: 'user' | 'bot';
    content: string;
    time: string;
}

const md = new MarkdownIt({
    html: true,
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
    minHeight?: string;
    minWidth?: string;
}>()

const isLoading = ref(false)
const inputMessage = ref('')
const theme = useTheme()


const getCurrentTime = () => {
    const now = new Date()
    return `${now.getHours().toString().padStart(2, '0')}:${now.getMinutes().toString().padStart(2, '0')}`
}

const defaultMessages: Message[] = [
    { id: 1, role: 'bot', content: '¡Hola! 👋 ¿En qué puedo ayudarte?', time: getCurrentTime() },
    { id: 2, role: 'user', content: 'Quiero saber los precios', time: getCurrentTime() },
    { id: 3, role: 'bot', content: '¡Claro! Tenemos planes desde $29/mes. ¿Te cuento más?', time: getCurrentTime() }
]

const displayMessages = computed(() => {
    if (props.messages !== undefined) {
        return props.messages
    }
    return defaultMessages
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
        // Simulador visual
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
        || normalized === 'rgba(0,0,0,0)'
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