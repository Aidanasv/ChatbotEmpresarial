import { defineStore } from "pinia";
import axios from "axios";
import type { ChatbotResponse, ChatbotState, Conversation, ConversationResponse } from "@/types/chatbot";
import { useUiStore } from "./useUiStore";

export const useChatBotStore = defineStore("chatbot", {
    state: (): ChatbotState => ({
        conversations: [
            { id: 1, role: "bot", content: "¡Hola! Soy tu asistente virtual. ¿En qué puedo ayudarte hoy?", time: new Date().toLocaleTimeString('es-ES', { hour: '2-digit', minute: '2-digit' }) }
        ],
        isLoading: false,
        error: null,
        chatbotId: null,
        conversationId: null
    }),

    actions: {
        getTimeLabel() {
            const now = new Date();
            return now.toLocaleTimeString('es-ES', { hour: '2-digit', minute: '2-digit' });
        },

        async initializeChatbot(conversation: Conversation) {
            try {
                this.isLoading = true;
                const response = await axios.post<ConversationResponse>("http://localhost:5267/api/chatbot/start-conversation", conversation);
                this.conversationId = response.data.conversationId;
            } catch (error) {
                this.error = error as Error;
                useUiStore().showError(
                    'Ocurrió un error al inicializar el chatbot. Por favor, inténtalo de nuevo.',
                    "Error de inicialización",
                    3000
                );
            } finally {
                this.isLoading = false;
            }
        },

        async sendMenssage(message: string) {
            try {
                this.isLoading = true;
                const response = await axios.post<ChatbotResponse>("http://localhost:5267/api/chatbot/chat", { message, conversationId: this.conversationId });
                this.conversations.push({ id: Date.now(), role: "bot", content: response.data.response, time: this.getTimeLabel() });
            } catch (error) {
                this.error = error as Error;
                useUiStore().showError(
                    'Ocurrió un error al enviar el mensaje. Por favor, inténtalo de nuevo.',
                    "Error al enviar",
                    3000
                );
            } finally {
                this.isLoading = false;
            }
        }
    }
});