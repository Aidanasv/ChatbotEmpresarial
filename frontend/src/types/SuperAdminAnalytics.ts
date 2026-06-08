import type { AdminCompany } from "@/components/admin/AdminCompaniesTable.vue";
import type { SubscriptionPlan } from "@/types/Subscription";

export interface SuperAdminAnalyticsState {
    totalCompanies: number;
    totalUsers: number;
    totalConversations: number;
    mrr: number;
    companyPanelData: AdminCompany[];
    subscriptions: SubscriptionPlan[];
}

