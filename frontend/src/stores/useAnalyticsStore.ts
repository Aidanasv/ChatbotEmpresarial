import type { AnalyticsState, ConversationPanelData } from "@/types/Analytics";
import axios from "axios";
import { defineStore } from "pinia";
import { useUiStore } from "./useUiStore";

export const useAnalyticsStore = defineStore('analytics', {
    state: (): AnalyticsState => ({
        totalConversations: 0,
        totalActiveUsers: 0,
        averageMessagesByConversation: 0,
        averageResponseTime: 0,
        conversationPanelData: []
    }),
    actions: {
        async getAnalyticsData() {
            try {
                const response = await axios.get('http://localhost:5267/api/analytics/analytics', {
                    headers: { Authorization: `Bearer ${localStorage.getItem('token')}` }
                });
                const data: AnalyticsState = response.data;
                this.totalConversations = data.totalConversations;
                this.totalActiveUsers = data.totalActiveUsers;
                this.averageMessagesByConversation = data.averageMessagesByConversation;
                this.averageResponseTime = data.averageResponseTime;
            } catch (error) {
                useUiStore().showError(
                    'Ocurrió un error al cargar las métricas. Por favor, inténtalo de nuevo.',
                    "Error al cargar analíticas",
                    3000
                );
            }
        },

        async getConversationData(limit: number = 5, offset: number = 0) {
            try {
                const response = await axios.get<ConversationPanelData[]>(`http://localhost:5267/api/analytics/conversations?limit=${limit}&offset=${offset}`, {
                    headers: { Authorization: `Bearer ${localStorage.getItem('token')}` }
                });
                const data: ConversationPanelData[] = response.data;
                this.conversationPanelData = data;
            } catch (error) {
                useUiStore().showError(
                    'Ocurrió un error al obtener los datos de las conversaciones. Por favor, inténtalo más tarde.',
                    "Error de conexión",
                    3000
                );
            }
        }
    }
})