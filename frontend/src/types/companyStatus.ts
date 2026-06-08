export type CompanyLifecycleStatus = 'Active' | 'Inactive' | 'InReview'

export const companyStatusLabels: Record<CompanyLifecycleStatus, string> = {
    Active: 'Activa',
    Inactive: 'Inactiva',
    InReview: 'En revision'
}

export const companyStatusColors: Record<CompanyLifecycleStatus, string> = {
    Active: 'success',
    Inactive: 'error',
    InReview: 'warning'
}

export const companyStatusOptions: Array<{ value: CompanyLifecycleStatus; label: string }> = [
    { value: 'Active', label: companyStatusLabels.Active },
    { value: 'InReview', label: companyStatusLabels.InReview },
    { value: 'Inactive', label: companyStatusLabels.Inactive }
]
