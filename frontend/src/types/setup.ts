export interface CompanySetup {
    companyName: string;
    legalName: string;
    cif: string;
    email: string;
    sector: string;
    website: string;
    description: string;
}

export interface CompanySetupResponse extends CompanySetup {
    id: number;
    renewalDate: Date;
    createdAt: Date;
    status: string;
}

export interface PersonalitySetup {
    chatbotName: string;
    chatbotTone: number;
    greetingMessage: string;
    fallbackMessage: string;
}

export interface AppearanceSetup {
    primaryColor: string;
    showChatbotAvatar: boolean;
    widgetPosition: boolean;
}

export interface KnowledgeSetup {
    documents: { name: string; id?: string; createdAt?: string }[];
    faqs: { id: number | null; question: string; answer: string; createdAt: string | null; updatedAt: string | null }[];
}

export interface SetUpState {
    isLoading: boolean;
    step: number;
    companySetup: CompanySetup;
    personalitySetup: PersonalitySetup;
    appearanceSetup: AppearanceSetup;
    knowledgeSetup: KnowledgeSetup;
}
