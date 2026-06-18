import type { ConversationPanelData } from "@/types/Analytics";
import type { ConversationMessage, ConversationState } from "@/types/Conversations"
import axios from "axios";
import { defineStore } from "pinia";
import { useUiStore } from "./useUiStore";

export const useConversationsStore = defineStore('conversations', {
    state: (): ConversationState => ({
        conversations: [],
        limit: 5,
        offset: 0,
        conversationMessage: null
    }),
    actions: {
        async getConversationData(limit: number = 5, offset: number = 0) {
            try {
                const response = await axios.get<ConversationPanelData[]>(`http://localhost:5267/api/analytics/conversations?limit=${limit}&offset=${offset}`, {
                    headers: { Authorization: `Bearer ${localStorage.getItem('token')}` }
                });
                const data: ConversationPanelData[] = response.data;
                this.conversations = data;
                this.limit = limit;
                this.offset = offset;
            } catch (error) {
                console.error('Error fetching conversation data:', error);
                useUiStore().showError(
                    'Ocurrió un error al obtener los datos de las conversaciones. Por favor, inténtalo de nuevo.',
                    "Error al cargar conversaciones",
                    3000
                );
            }
        },
        async getConversationMessages(conversationId: number) {
            try {
                const response = await axios.get<ConversationMessage>(`http://localhost:5267/api/analytics/conversationsById/${conversationId}`, {
                    headers: { Authorization: `Bearer ${localStorage.getItem('token')}` }
                });
                const data: ConversationMessage = response.data;
                this.conversationMessage = data;
            } catch (error) {
                console.error('Error fetching conversation messages:', error);
                useUiStore().showError(
                    'Ocurrió un error al cargar los mensajes de la conversación. Por favor, inténtalo de nuevo.',
                    "Error al cargar mensajes",
                    3000
                );
            }
        }
    }
})