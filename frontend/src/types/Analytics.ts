import type { st } from "vue-router/dist/router-CWoNjPRp.mjs";

export interface AnalyticsState {
    totalConversations: number;
    totalActiveUsers: number;
    averageMessagesByConversation: number;
    averageResponseTime: number;
    conversationPanelData: ConversationPanelData[];
}

export interface ConversationPanelData {
    conversationId: number;
    role: string;
    customerName: string;
    lastMessage: string;
    lastMessageTime: string;
    status: string;
}