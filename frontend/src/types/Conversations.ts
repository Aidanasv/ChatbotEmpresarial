import type { ConversationPanelData } from "./Analytics";

export interface ConversationState {
    conversations: ConversationPanelData[];
    limit: number;
    offset: number;
    conversationMessage: ConversationMessage | null;
}

export interface ConversationMessage {
    id: number;
    chatbotSettingsId: number;
    customerEmail: string;
    customerName: string;
    customerPhone: string;
    topic: string;
    status: string;
    createdAt: string;
    messages: Message[];
}

export interface Message {
    id: number;
    conversationId: number;
    role: string;
    content: string;
    createdAt: string;
}