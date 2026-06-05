export interface ChatbotState {
    conversations: Message[];
    isLoading: boolean;
    error: Error | null;
    chatbotId: number | null;
    conversationId: number | null;
}

export interface Message {
    id: number;
    role: 'user' | 'bot';
    content: string;
    time: string;
}

export interface ChatbotRequest {
    message: string;
}

export interface ChatbotResponse {
    response: string;
}

export interface Conversation {
    chatbotId: number;
    customerName: string;
    customerEmail: string;
    customerPhone: string;
    topic: string;
}

export interface ConversationResponse {
    conversationId: number;
}
