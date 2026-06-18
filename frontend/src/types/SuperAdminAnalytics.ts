import type { AdminCompany } from "@/components/admin/AdminCompaniesTable.vue";
import type { SubscriptionPlan } from "@/types/Subscription";

export interface SuperAdminAnalyticsState {
    totalCompanies: number;
    totalUsers: number;
    totalConversations: number;
    mrr: number;
    companyPanelData: AdminCompany[];
    companyPanelTotal: number;
    companyPanelPage: number;
    companyPanelPageSize: number;
    activeCompaniesCount: number;
    inReviewCompaniesCount: number;
    inactiveCompaniesCount: number;
    subscriptions: SubscriptionPlan[];
}

export interface CompaniesPanelQuery {
    search?: string;
    status?: 'Todas' | 'Active' | 'InReview' | 'Inactive';
    page?: number;
    pageSize?: number;
}

export interface CompaniesPanelResponse {
    items: AdminCompany[];
    total: number;
    page: number;
    pageSize: number;
    activeCount: number;
    inReviewCount: number;
    inactiveCount: number;
}

