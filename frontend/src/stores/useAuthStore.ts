import type {
    ApiMessageResponse,
    AuthState,
    ChangePasswordPayload,
    ForgotPasswordPayload,
    LoginAuth,
    RegisterAuth,
    ResetPasswordPayload,
    UserAuthResponse
} from "@/types/userAuth";
import { defineStore, getActivePinia } from "pinia";
import axios from "axios";
import { jwtDecode } from "jwt-decode";
import { useUiStore } from "./useUiStore";

type DecodedJwt = Record<string, string | number | undefined>
const AUTH_API_BASE_URL = `${import.meta.env.VITE_API_URL || "https://chatbotempresarial-1067165831463.europe-west1.run.app"}/api/auth`;

const AUTH_STORAGE_KEYS = ["token", "userId", "email", "role", "companyId", "chatbotId"] as const;

const clearAuthStorage = () => {
    AUTH_STORAGE_KEYS.forEach((key) => localStorage.removeItem(key));
}

const getTokenExpiration = (token: string): number | null => {
    try {
        const decodedToken = jwtDecode<DecodedJwt>(token);
        const exp = decodedToken.exp;
        if (typeof exp === "number") {
            return exp;
        }

        if (typeof exp === "string") {
            const parsed = Number(exp);
            return Number.isFinite(parsed) ? parsed : null;
        }

        return null;
    } catch {
        return null;
    }
}

const isTokenExpired = (token: string): boolean => {
    const exp = getTokenExpiration(token);

    if (exp === null) {
        return false;
    }

    const nowInSeconds = Math.floor(Date.now() / 1000);
    return nowInSeconds > exp;
}

const getStringClaim = (value: string | number | undefined): string | null => {
    if (typeof value === "string") {
        return value;
    }

    if (typeof value === "number") {
        return String(value);
    }

    return null;
}

const getInitialAuthState = (): AuthState => {
    const token = localStorage.getItem("token");

    if (token && isTokenExpired(token)) {
        clearAuthStorage();
    }

    return {
        userId: localStorage.getItem("userId") || null,
        email: localStorage.getItem("email") || null,
        role: localStorage.getItem("role") || null,
        companyId: localStorage.getItem("companyId") || null,
        chatbotId: localStorage.getItem("chatbotId") || null
    };
}

export const useAuthStore = defineStore("auth", {
    state: (): AuthState => getInitialAuthState(),

    getters: {
        isAuthenticated: (state) => !!state.userId,
        isSuperAdmin: (state) => (state.role || "").toLowerCase() === "superadmin",
    },
    actions: {
        resetAllStores() {
            const pinia = getActivePinia();

            if (!pinia) {
                return;
            }

            const stores = (pinia as any)._s as Map<string, any>;
            stores.forEach((store, id) => {
                if (id === this.$id) {
                    return;
                }

                if (typeof store.$reset === "function") {
                    store.$reset();
                }
            });
        },

        async register(userAuth: RegisterAuth) {
            try {
                const response = await axios.post<UserAuthResponse>(`${AUTH_API_BASE_URL}/register`, userAuth);
                await this.processingToken(response.data.token || "");
                useUiStore().showSuccess("¡Registro exitoso! Has iniciado sesión correctamente.", "Éxito", 2000);
            } catch (error) {
                const uiStore = useUiStore();
                uiStore.showError(
                    'Ocurrió un error durante el registro. Por favor, inténtalo de nuevo.',
                    "Error de registro",
                    3000
                );
                console.error("Error during registration:", error);
            }
        },

        async login(userAuth: LoginAuth) {
            try {
                const response = await axios.post<UserAuthResponse>(`${AUTH_API_BASE_URL}/login`, userAuth);
                await this.processingToken(response.data.token || "");
                useUiStore().showSuccess("¡Inicio de sesión exitoso!", "Éxito", 2000);
            } catch (error) {
                useUiStore().showError(
                    'Ocurrió un error al iniciar sesión. Por favor, verifica tus datos e inténtalo de nuevo.',
                    "Error al iniciar sesión",
                    3000
                );
                console.error("Error during login:", error);
            }
        },

        async forgotPassword(payload: ForgotPasswordPayload) {
            try {
                const response = await axios.post<ApiMessageResponse>(`${AUTH_API_BASE_URL}/forgot-password`, payload);
                useUiStore().showSuccess(response.data.message, "Correo enviado", 3000);
                return true;
            } catch (error) {
                useUiStore().showError(
                    'No se pudo iniciar la recuperación de contraseña. Inténtalo de nuevo.',
                    "Error de recuperación",
                    3000
                );
                console.error("Error during forgot password:", error);
                return false;
            }
        },

        async resetPassword(payload: ResetPasswordPayload) {
            try {
                const response = await axios.post<ApiMessageResponse>(`${AUTH_API_BASE_URL}/reset-password`, payload);
                useUiStore().showSuccess(response.data.message, "Contraseña actualizada", 3000);
                return true;
            } catch (error) {
                useUiStore().showError(
                    'No se pudo restablecer la contraseña. Verifica el enlace o solicita uno nuevo.',
                    "Error de recuperación",
                    3000
                );
                console.error("Error during reset password:", error);
                return false;
            }
        },

        async changePassword(payload: ChangePasswordPayload) {
            try {
                const token = localStorage.getItem("token");
                const response = await axios.post<ApiMessageResponse>(`${AUTH_API_BASE_URL}/change-password`, payload, {
                    headers: {
                        Authorization: `Bearer ${token}`
                    }
                });
                useUiStore().showSuccess(response.data.message, "Contraseña actualizada", 3000);
                return true;
            } catch (error) {
                useUiStore().showError(
                    'No se pudo cambiar la contraseña. Revisa tu contraseña actual e inténtalo de nuevo.',
                    "Error al cambiar contraseña",
                    3000
                );
                console.error("Error during change password:", error);
                return false;
            }
        },

        async processingToken(token: string) {
            const uiStore = useUiStore();
            if (!token) {
                this.logout();
                uiStore.showError(
                    "No se proporcionó un token de acceso. Por favor, inicia sesión de nuevo.",
                    "Error de autenticación",
                    3000
                );
                return;
            }

            if (isTokenExpired(token)) {
                this.logout();
                uiStore.showInfo(
                    "Tu sesión ha expirado. Por favor, inicia sesión de nuevo.",
                    "Sesión expirada",
                    3000
                );
                return;
            }

            const decodedToken = jwtDecode<DecodedJwt>(token);
            console.log("Decoded Token:", decodedToken);
            this.userId = getStringClaim(decodedToken.userId);
            this.email = getStringClaim(decodedToken.email) || getStringClaim(decodedToken["http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress"]);
            this.role = getStringClaim(decodedToken.role) || getStringClaim(decodedToken["http://schemas.microsoft.com/ws/2008/06/identity/claims/role"]);
            this.companyId = getStringClaim(decodedToken.companyId);
            this.chatbotId = getStringClaim(decodedToken.chatbotId);
            localStorage.setItem("token", token);
            localStorage.setItem("userId", this.userId || "");
            localStorage.setItem("email", this.email || "");
            localStorage.setItem("role", this.role || "");
            localStorage.setItem("companyId", this.companyId || "");
            localStorage.setItem("chatbotId", this.chatbotId || "");
        },

        ensureTokenValidity() {
            const token = localStorage.getItem("token");

            if (!token) {
                this.resetAllStores();
                return;
            }

            if (isTokenExpired(token)) {
                this.logout();
            }
        },

        logout() {
            this.resetAllStores();
            this.userId = null;
            this.email = null;
            this.role = null;
            this.companyId = null;
            this.chatbotId = null;
            clearAuthStorage();
            const uiStore = useUiStore();
            uiStore.showInfo(
                "Has cerrado sesión correctamente o tu sesión ha expirado.",
                "Sesión cerrada",
                3000
            );

        }
    }

});