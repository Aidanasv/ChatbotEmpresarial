export interface SubscriptionPlan {
    id: number
    name: string
    price: number
    maxUsers: number
    features: string[]
    companiesCount: number
    projectedMonthlyRevenue: number
}

export interface UpsertSubscriptionPayload {
    name: string
    price: number
    maxUsers: number
    features: string[]
}
