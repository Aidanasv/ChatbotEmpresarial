import type { SuperAdminAnalyticsState } from "@/types/SuperAdminAnalytics";
import type { CompanyLifecycleStatus } from "@/types/companyStatus";
import type { SubscriptionPlan, UpsertSubscriptionPayload } from "@/types/Subscription";
import axios from "axios";
import { defineStore } from "pinia";

export const useSuperAdminStore = defineStore('superAdmin', {
    state: (): SuperAdminAnalyticsState => ({
        totalCompanies: 0,
        totalUsers: 0,
        totalConversations: 0,
        mrr: 0,
        companyPanelData: [],
        subscriptions: []
    }),
    actions: {
        async getAnalyticsData() {
            try {
                const response = await axios.get('http://localhost:5267/api/analytics/superadminanalytics', {
                    headers: { Authorization: `Bearer ${localStorage.getItem('token')}` }
                })
                const data = response.data;
                this.totalCompanies = data.totalCompanies;
                this.totalUsers = data.totalUsers;
                this.totalConversations = data.totalConversations;
                this.mrr = data.mrr;
            } catch (error) {
                console.error("Error fetching analytics data:", error);
            }
        },
        async getCompanyPanelData(limit: number = 5, offset: number = 0) {
            try {
                const response = await axios.get(`http://localhost:5267/api/analytics/companies?limit=${limit}&offset=${offset}`, {
                    headers: { Authorization: `Bearer ${localStorage.getItem('token')}` }
                });
                const data = response.data;
                this.companyPanelData = data;
            } catch (error) {
                console.error("Error fetching company panel data:", error);
            }
        },
        async updateCompanyStatus(companyId: number, status: CompanyLifecycleStatus) {
            try {
                await axios.patch(`http://localhost:5267/api/setup/company/${companyId}/status`,
                    { status },
                    { headers: { Authorization: `Bearer ${localStorage.getItem('token')}` } }
                );

                this.companyPanelData = this.companyPanelData.map((company) =>
                    Number(company.companyId) === companyId ? { ...company, status } : company
                );
            } catch (error) {
                console.error("Error updating company status:", error);
                throw error;
            }
        },
        async getSubscriptions() {
            try {
                const response = await axios.get<SubscriptionPlan[]>("http://localhost:5267/api/subscriptions", {
                    headers: { Authorization: `Bearer ${localStorage.getItem('token')}` }
                });

                this.subscriptions = response.data;
            } catch (error) {
                console.error("Error fetching subscriptions:", error);
                throw error;
            }
        },
        async createSubscription(payload: UpsertSubscriptionPayload) {
            try {
                const response = await axios.post<SubscriptionPlan>("http://localhost:5267/api/subscriptions", payload, {
                    headers: { Authorization: `Bearer ${localStorage.getItem('token')}` }
                });

                this.subscriptions = [...this.subscriptions, response.data].sort((left, right) => left.price - right.price);
                return response.data;
            } catch (error) {
                console.error("Error creating subscription:", error);
                throw error;
            }
        },
        async updateSubscription(subscriptionId: number, payload: UpsertSubscriptionPayload) {
            try {
                const response = await axios.put<SubscriptionPlan>(`http://localhost:5267/api/subscriptions/${subscriptionId}`, payload, {
                    headers: { Authorization: `Bearer ${localStorage.getItem('token')}` }
                });

                this.subscriptions = this.subscriptions
                    .map((subscription) => subscription.id === subscriptionId ? response.data : subscription)
                    .sort((left, right) => left.price - right.price);

                return response.data;
            } catch (error) {
                console.error("Error updating subscription:", error);
                throw error;
            }
        }
    }
})