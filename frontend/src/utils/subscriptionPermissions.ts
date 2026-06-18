export const SUBSCRIPTION_FEATURES = {
    URL_READING: 'Lectura de URL',
    WEBSITE_READ_ONLY: 'Solo lectura de pagina web (URL)',
    LIMITED_DOCUMENT_UPLOAD: 'Subida limitada de documentacion',
    UNLIMITED_DOCUMENT_UPLOAD: 'Subida ilimitada de documentacion',
    COLOR_AND_AVATAR_CUSTOMIZATION: 'Personalizacion de colores y avatar',
    STANDARD_SUPPORT: 'Soporte Tecnico Estandar',
    SUPPORT_24_7: 'Soporte Tecnico 24/7',
    DEFAULT_DESIGN_AND_COLORS: 'Diseno y colores predeterminados'
} as const;

const normalize = (value: string): string => {
    return value
        .trim()
        .toLowerCase()
        .normalize('NFD')
        .replace(/[\u0300-\u036f]/g, '');
}

export const hasSubscriptionFeature = (features: string[] = [], requiredFeature: string): boolean => {
    if (!requiredFeature.trim()) {
        return false;
    }

    const normalizedRequired = normalize(requiredFeature);
    return features.some((feature) => normalize(feature) === normalizedRequired);
}

export const hasAnySubscriptionFeature = (features: string[] = [], requiredFeatures: string[]): boolean => {
    return requiredFeatures.some((feature) => hasSubscriptionFeature(features, feature));
}

export const getDocumentUploadLimitFromFeatures = (features: string[] = []): number | null => {
    if (hasSubscriptionFeature(features, SUBSCRIPTION_FEATURES.UNLIMITED_DOCUMENT_UPLOAD)) {
        return null;
    }

    if (hasSubscriptionFeature(features, SUBSCRIPTION_FEATURES.LIMITED_DOCUMENT_UPLOAD)) {
        return 5;
    }

    return 0;
}
