import { defineStore } from 'pinia'
import type { CompanySetup, PersonalitySetup, AppearanceSetup, SetUpState, CompanySetupResponse, KnowledgeSetup } from '@/types/setup'
import axios from 'axios'
import { useUiStore } from './useUiStore'
import { hasSubscriptionFeature } from '@/utils/subscriptionPermissions'

const API_BASE_URL = import.meta.env.VITE_API_URL || 'https://chatbotempresarial-1067165831463.europe-west1.run.app'

export const useSetupStore = defineStore('setup', {
    state: (): SetUpState => ({
        isLoading: false,
        step: 1,
        companySetup: {
            companyName: '',
            legalName: '',
            cif: '',
            email: '',
            sector: '',
            website: '',
            description: ''
        },
        personalitySetup: {
            chatbotName: '',
            chatbotTone: 0,
            greetingMessage: '',
            fallbackMessage: ''
        },
        appearanceSetup: {
            primaryColor: '',
            showChatbotAvatar: false,
            widgetPosition: true,
            title: ''
        },
        knowledgeSetup: {
            documents: [],
            faqs: []
        }
    }),
    actions: {
        async setCompanySetup(companySetup: CompanySetup) {
            this.companySetup = companySetup;
        },

        async saveCompanySetup(companySetup: CompanySetup) {
            try {
                const response = await axios.post<CompanySetupResponse>(`${API_BASE_URL}/api/setup/company`, companySetup,
                    { headers: { Authorization: `Bearer ${localStorage.getItem('token')}` } }
                );
                this.companySetup = response.data;
                useUiStore().showSuccess("Configuración de la empresa guardada correctamente.", "Éxito", 3000);
            } catch (error) {
                console.error('Error saving company setup:', error);
                useUiStore().showError(
                    'Ocurrió un error al guardar la configuración de la empresa. Por favor, inténtalo de nuevo.',
                    "Error al guardar",
                    3000
                );
                throw error;
            }
        },

        async setPersonalitySetup(personalitySetup: PersonalitySetup) {
            this.personalitySetup = personalitySetup;
        },

        async savePersonalitySetup(personalitySetup: PersonalitySetup) {
            try {
                this.isLoading = true;
                const response = await axios.post(`${API_BASE_URL}/api/setup/personalization`, personalitySetup,
                    { headers: { Authorization: `Bearer ${localStorage.getItem('token')}` } }
                );
                this.isLoading = false;
                useUiStore().showSuccess("Configuración de personalidad guardada correctamente.", "Éxito", 3000);
                return response.data;
            } catch (error) {
                this.isLoading = false;
                console.error('Error saving personality setup:', error);
                useUiStore().showError(
                    'Ocurrió un error al guardar la personalidad del chatbot. Por favor, inténtalo de nuevo.',
                    "Error al guardar",
                    3000
                );
                throw error;
            }
        },

        async setAppearanceSetup(appearanceSetup: AppearanceSetup) {
            this.appearanceSetup = appearanceSetup;
        },

        async saveAppearanceSetup(appearanceSetup: AppearanceSetup) {
            try {
                this.isLoading = true;
                const response = await axios.post(`${API_BASE_URL}/api/setup/appearance`, appearanceSetup,
                    { headers: { Authorization: `Bearer ${localStorage.getItem('token')}` } }
                );
                this.isLoading = false;
                useUiStore().showSuccess("Configuración de apariencia guardada correctamente.", "Éxito", 3000);
                return response.data;
            } catch (error) {
                this.isLoading = false;
                console.error('Error saving appearance setup:', error);
                useUiStore().showError(
                    'Ocurrió un error al guardar la apariencia. Por favor, inténtalo de nuevo.',
                    "Error al guardar",
                    3000
                );
                throw error;
            }
        },

        async saveKnowledgeSetup(knowledgeSetup: KnowledgeSetup, files: File[] = [], documentsToDelete: string[] = []) {
            try {
                this.isLoading = true;

                // Guardar FAQs
                const response = await axios.post(`${API_BASE_URL}/api/setup/faqs`, knowledgeSetup.faqs,
                    { headers: { Authorization: `Bearer ${localStorage.getItem('token')}` } }
                );

                // Subir nuevos documentos si existen
                if (files.length > 0) {
                    const formData = new FormData();
                    files.forEach(file => formData.append('files', file));

                    await axios.post(`${API_BASE_URL}/api/documentSources`, formData,
                        { headers: { Authorization: `Bearer ${localStorage.getItem('token')}` } }
                    );
                }

                // Borrar documentos marcados para eliminar
                if (documentsToDelete.length > 0) {
                    await this.deleteDocuments(documentsToDelete);
                }

                this.isLoading = false;
                useUiStore().showSuccess("Configuración de conocimiento guardada correctamente.", "Éxito", 3000);
                return response.data;
            } catch (error) {
                this.isLoading = false;
                console.error('Error saving knowledge setup:', error);
                useUiStore().showError(
                    'Ocurrió un error al guardar la base de conocimiento. Por favor, inténtalo de nuevo.',
                    "Error al guardar",
                    3000
                );
                throw error;
            }
        },

        async saveInitialSetup() {
            try {
                this.isLoading = true;
                const response = await axios.post(`${API_BASE_URL}/api/setup/setupInitial`, {
                    companySetup: this.companySetup,
                    personalitySetup: this.personalitySetup,
                    appearanceSetup: this.appearanceSetup
                }, { headers: { Authorization: `Bearer ${localStorage.getItem('token')}` } });
                this.isLoading = false;
                return response.data;
            } catch (error) {
                this.isLoading = false;
                console.error('Error saving initial setup:', error);
                useUiStore().showError(
                    'Ocurrió un error al guardar la configuración inicial. Por favor, inténtalo de nuevo.',
                    "Error de configuración",
                    3000
                );
                throw error;
            }
        },

        async getSetupData(companyId: number) {
            try {
                this.isLoading = true;
                const response = await axios.get(`${API_BASE_URL}/api/setup/setup/${companyId}`, {
                    headers: { Authorization: `Bearer ${localStorage.getItem('token')}` }
                });
                const { companySetup, personalitySetup, appearanceSetup, knowledgeSetup } = response.data;
                this.companySetup = companySetup;
                this.personalitySetup = personalitySetup;
                this.appearanceSetup = appearanceSetup;
                this.knowledgeSetup = knowledgeSetup;
                this.isLoading = false;
            } catch (error) {
                this.isLoading = false;
                console.error('Error fetching setup data:', error);
                useUiStore().showError(
                    'Ocurrió un error al obtener los datos de configuración. Por favor, inténtalo de nuevo.',
                    "Error de carga",
                    3000
                );
                throw error;
            }
        },

        async getCurrentSubscription() {
            try {
                const response = await axios.get<{ subscriptionId: number }>(`${API_BASE_URL}/api/setup/company/subscription`, {
                    headers: { Authorization: `Bearer ${localStorage.getItem('token')}` }
                });

                return response.data.subscriptionId;
            } catch (error) {
                console.error('Error fetching current subscription:', error);
                useUiStore().showError(
                    'Ocurrió un error al cargar tu suscripción actual. Por favor, inténtalo de nuevo.',
                    "Error al cargar",
                    3000
                );  
                throw error;
            }
        },

        async getCurrentSubscriptionLimits() {
            try {
                const response = await axios.get<{ subscriptionId: number; maxUsers: number }>(`${API_BASE_URL}/api/setup/company/subscription`, {
                    headers: { Authorization: `Bearer ${localStorage.getItem('token')}` }
                });

                return response.data;
            } catch (error) {
                console.error('Error fetching current subscription limits:', error);
                useUiStore().showError(
                    'Ocurrio un error al cargar los limites de tu suscripcion. Por favor, intentalo de nuevo.',
                    'Error al cargar',
                    3000
                );
                throw error;
            }
        },

        async getCurrentSubscriptionFeatures(companyId?: number) {
            try {
                const response = await axios.get<{ companyId: number, features: string[] }>(`${API_BASE_URL}/api/setup/company/subscription/features`, {
                    params: companyId ? { companyId } : undefined,
                    headers: { Authorization: `Bearer ${localStorage.getItem('token')}` }
                });

                return response.data.features ?? [];
            } catch (error) {
                console.error('Error fetching current subscription features:', error);
                useUiStore().showError(
                    'Ocurrio un error al cargar las caracteristicas de tu suscripcion. Por favor, intentalo de nuevo.',
                    'Error al cargar',
                    3000
                );
                throw error;
            }
        },

        async canUseCurrentSubscriptionFeature(requiredFeature: string, companyId?: number) {
            const features = await this.getCurrentSubscriptionFeatures(companyId);
            return hasSubscriptionFeature(features, requiredFeature);
        },

        async updateCompanySubscription(subscriptionId: number) {
            try {
                const response = await axios.patch<{ subscriptionId: number }>(`${API_BASE_URL}/api/setup/company/subscription/${subscriptionId}`, null, {
                    headers: { Authorization: `Bearer ${localStorage.getItem('token')}` }
                });

                return response.data.subscriptionId;
            } catch (error) {
                console.error('Error updating company subscription:', error);
                useUiStore().showError(
                    'Ocurrió un error al actualizar la suscripción de la empresa. Por favor, inténtalo de nuevo.',
                    "Error al actualizar",
                    3000
                );
                throw error;
            }
        },

        async deleteDocuments(documentSourceIds: string[]) {
            try {
                const response = await axios.delete(`${API_BASE_URL}/api/documentSources/files`,
                    {
                        data: { documentSourceIds },
                        headers: { Authorization: `Bearer ${localStorage.getItem('token')}` }
                    }
                );
                return response.data;
            } catch (error) {
                console.error('Error deleting documents:', error);
                useUiStore().showError(
                    'Ocurrió un error al eliminar los documentos seleccionados. Por favor, inténtalo de nuevo.',
                    "Error al eliminar",
                    3000
                );
                throw error;
            }
        },

        async getCorpusRebuildStatus() {
            try {
                const response = await axios.get(`${API_BASE_URL}/api/documentSources/rebuild-status`,
                    { headers: { Authorization: `Bearer ${localStorage.getItem('token')}` } }
                );

                return response.data as {
                    status: 'idle' | 'running' | 'succeeded' | 'failed',
                    startedAtUtc?: string,
                    finishedAtUtc?: string,
                    message?: string,
                    corpusName?: string
                };
            } catch (error) {
                console.error('Error fetching corpus rebuild status:', error);
                useUiStore().showError(
                    'Ocurrió un error al consultar el estado de indexación de los documentos. Por favor, inténtalo de nuevo.',
                    "Error de estado",
                    3000
                );
                throw error;
            }
        }
    }
})