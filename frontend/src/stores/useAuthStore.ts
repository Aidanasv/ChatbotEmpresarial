import type { LoginAuth, UserAuthResponse, RegisterAuth, AuthState } from "@/types/userAuth";
import { defineStore } from "pinia";
import axios from "axios";
import { jwtDecode } from "jwt-decode";

type DecodedJwt = Record<string, string | undefined>

export const useAuthStore = defineStore("auth", {
    state: (): AuthState => ({
        userId: localStorage.getItem("userId") || null,
        email: localStorage.getItem("email") || null,
        role: localStorage.getItem("role") || null,
        companyId: localStorage.getItem("companyId") || null,
        chatbotId: localStorage.getItem("chatbotId") || null
    }),

    getters: {
        isAuthenticated: (state) => !!state.userId,
        isSuperAdmin: (state) => (state.role || "").toLowerCase() === "superadmin",
    },
    actions: {
        async register(userAuth: RegisterAuth) {
            try {
                const response = await axios.post<UserAuthResponse>("http://localhost:5267/api/auth/register", userAuth);
                await this.processingToken(response.data.token || "");
            } catch (error) {
                console.error("Error during registration:", error);
            }
        },

        async login(userAuth: LoginAuth) {
            try {
                const response = await axios.post<UserAuthResponse>("http://localhost:5267/api/auth/login", userAuth);
                await this.processingToken(response.data.token || "");
            } catch (error) {
                console.error("Error during login:", error);
            }
        },

        async processingToken(token: string) {
            const decodedToken = jwtDecode<DecodedJwt>(token);
            console.log("Decoded Token:", decodedToken);
            this.userId = decodedToken.userId || null;
            this.email = decodedToken.email || decodedToken["http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress"] || null;
            this.role = decodedToken.role || decodedToken["http://schemas.microsoft.com/ws/2008/06/identity/claims/role"] || null;
            this.companyId = decodedToken.companyId || null;
            this.chatbotId = decodedToken.chatbotId || null;
            localStorage.setItem("token", token);
            localStorage.setItem("userId", this.userId || "");
            localStorage.setItem("email", this.email || "");
            localStorage.setItem("role", this.role || "");
            localStorage.setItem("companyId", this.companyId || "");
            localStorage.setItem("chatbotId", this.chatbotId || "");
        },

        logout() {
            this.userId = null;
            this.email = null;
            this.role = null;
            this.companyId = null;
            this.chatbotId = null;
            localStorage.clear();
        }
    }

});