import { defineStore } from 'pinia'
import type { CompanySetup, PersonalitySetup, AppearanceSetup, SetUpState, CompanySetupResponse, KnowledgeSetup } from '@/types/setup'
import axios from 'axios'

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
            widgetPosition: true
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
                const response = await axios.post<CompanySetupResponse>('http://localhost:5267/api/setup/company', companySetup,
                    { headers: { Authorization: `Bearer ${localStorage.getItem('token')}` } }
                );
                this.companySetup = response.data;
            } catch (error) {
                console.error('Error saving company setup:', error);
                throw error;
            }
        },

        async setPersonalitySetup(personalitySetup: PersonalitySetup) {
            this.personalitySetup = personalitySetup;
        },

        async savePersonalitySetup(personalitySetup: PersonalitySetup) {
            try {
                this.isLoading = true;
                const response = await axios.post('http://localhost:5267/api/setup/personalization', personalitySetup,
                    { headers: { Authorization: `Bearer ${localStorage.getItem('token')}` } }
                );
                this.isLoading = false;
                return response.data;
            } catch (error) {
                this.isLoading = false;
                console.error('Error saving personality setup:', error);
                throw error;
            }
        },

        async setAppearanceSetup(appearanceSetup: AppearanceSetup) {
            this.appearanceSetup = appearanceSetup;
        },

        async saveAppearanceSetup(appearanceSetup: AppearanceSetup) {
            try {
                this.isLoading = true;
                const response = await axios.post('http://localhost:5267/api/setup/appearance', appearanceSetup,
                    { headers: { Authorization: `Bearer ${localStorage.getItem('token')}` } }
                );
                this.isLoading = false;
                return response.data;
            } catch (error) {
                this.isLoading = false;
                console.error('Error saving appearance setup:', error);
                throw error;
            }
        },

        async saveKnowledgeSetup(knowledgeSetup: KnowledgeSetup, files: File[] = []) {
            try {
                this.isLoading = true;
                const response = await axios.post('http://localhost:5267/api/setup/faqs', knowledgeSetup.faqs,
                    { headers: { Authorization: `Bearer ${localStorage.getItem('token')}` } }
                );

                if (files.length > 0) {
                    const formData = new FormData();
                    files.forEach(file => formData.append('files', file));

                    await axios.post('http://localhost:5267/api/documentSources', formData,
                        { headers: { Authorization: `Bearer ${localStorage.getItem('token')}` } }
                    );
                }

                this.isLoading = false;
                return response.data;
            } catch (error) {
                this.isLoading = false;
                console.error('Error saving knowledge setup:', error);
                throw error;
            }
        },

        async saveInitialSetup() {
            try {
                this.isLoading = true;
                const response = await axios.post('http://localhost:5267/api/setup/setupInitial', {
                    companySetup: this.companySetup,
                    personalitySetup: this.personalitySetup,
                    appearanceSetup: this.appearanceSetup
                }, { headers: { Authorization: `Bearer ${localStorage.getItem('token')}` } });
                this.isLoading = false;
                return response.data;
            } catch (error) {
                this.isLoading = false;
                console.error('Error saving initial setup:', error);
                throw error;
            }
        },

        async getSetupData(companyId: number) {
            try {
                this.isLoading = true;
                const response = await axios.get(`http://localhost:5267/api/setup/setup/${companyId}`, {
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
                throw error;
            }
        }
    }
})