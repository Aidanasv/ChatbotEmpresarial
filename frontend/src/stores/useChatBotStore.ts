import { defineStore } from "pinia";
import axios from "axios";
import type { ChatbotResponse, ChatbotState, Conversation, ConversationResponse } from "@/types/chatbot";
import type { c } from "vue-router/dist/router-CWoNjPRp.mjs";

export const useChatBotStore = defineStore("chatbot", {
    state: (): ChatbotState => ({
        conversations: [],
        isLoading: false,
        error: null,
        chatbotId: null,
        conversationId: null
    }), 

    actions: {
        async initializeChatbot(conversation: Conversation) {
            try {
                this.isLoading = true;
               const response = await axios.post<ConversationResponse>("http://localhost:5267/api/chatbot/start-conversation", conversation, {
                });
                this.conversationId = response.data.conversationId;
            } catch (error) {
                this.error = error as Error;
            } finally {
                this.isLoading = false;
            }
        },

        async sendMenssage(message:string) {
            try {
                this.isLoading = true;
                const response = await axios.post<ChatbotResponse>("http://localhost:5267/api/chatbot/chat", { message, conversationId: this.conversationId }, {
                });
                this.conversations.push({ id: Date.now(), role: "bot", content: response.data.response, time: new Date().toISOString() });
            } catch (error) {
                this.error = error as Error;
            } finally {
                this.isLoading = false;
            }
        }
    }
});