import { defineStore } from 'pinia'
import type { CompanySetup, PersonalitySetup, AppearanceSetup, SetUpState, CompanySetupResponse } from '@/types/setup'
import axios from 'axios'

export const useSetupStore = defineStore('setup', {
    state: (): SetUpState => ({
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
                const response = await axios.post('http://localhost:5267/api/setup/personalization', personalitySetup,
                    { headers: { Authorization: `Bearer ${localStorage.getItem('token')}` } }
                );
                return response.data;
            } catch (error) {
                console.error('Error saving personality setup:', error);
                throw error;
            }
        },

        async setAppearanceSetup(appearanceSetup: AppearanceSetup) {
            this.appearanceSetup = appearanceSetup;
        },

        async saveInitialSetup() {
            try {
                const response = await axios.post('http://localhost:5267/api/setup/setupInitial', {
                    companySetup: this.companySetup,
                    personalitySetup: this.personalitySetup,
                    appearanceSetup: this.appearanceSetup
                }, { headers: { Authorization: `Bearer ${localStorage.getItem('token')}` } });
                return response.data;
            } catch (error) {
                console.error('Error saving initial setup:', error);
                throw error;
            }
        }
    }
})