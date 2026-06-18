import type { CompaniesPanelQuery, CompaniesPanelResponse, SuperAdminAnalyticsState } from "@/types/SuperAdminAnalytics";
import type { CompanyLifecycleStatus } from "@/types/companyStatus";
import type { SubscriptionPlan, UpsertSubscriptionPayload } from "@/types/Subscription";
import axios from "axios";
import { defineStore } from "pinia";
import { useUiStore } from "./useUiStore";

export const useSuperAdminStore = defineStore('superAdmin', {
    state: (): SuperAdminAnalyticsState => ({
        totalCompanies: 0,
        totalUsers: 0,
        totalConversations: 0,
        mrr: 0,
        companyPanelData: [],
        companyPanelTotal: 0,
        companyPanelPage: 1,
        companyPanelPageSize: 10,
        activeCompaniesCount: 0,
        inReviewCompaniesCount: 0,
        inactiveCompaniesCount: 0,
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
                useUiStore().showError(
                    'Ocurrió un error al cargar las analíticas generales. Por favor, inténtalo de nuevo.',
                    "Error al cargar analíticas",
                    3000
                );
            }
        },
        async getCompanyPanelData(query: CompaniesPanelQuery = {}) {
            try {
                const page = query.page ?? this.companyPanelPage;
                const pageSize = query.pageSize ?? this.companyPanelPageSize;

                const response = await axios.get<CompaniesPanelResponse>('http://localhost:5267/api/analytics/companies', {
                    params: {
                        search: query.search ?? '',
                        status: query.status ?? 'Todas',
                        page,
                        pageSize
                    },
                    headers: { Authorization: `Bearer ${localStorage.getItem('token')}` }
                });
                const data = response.data;
                this.companyPanelData = data.items;
                this.companyPanelTotal = data.total;
                this.companyPanelPage = data.page;
                this.companyPanelPageSize = data.pageSize;
                this.activeCompaniesCount = data.activeCount;
                this.inReviewCompaniesCount = data.inReviewCount;
                this.inactiveCompaniesCount = data.inactiveCount;
            } catch (error) {
                console.error("Error fetching company panel data:", error);
                useUiStore().showError(
                    'Ocurrió un error al obtener la lista de empresas. Por favor, inténtalo de nuevo.',
                    "Error al cargar empresas",
                    3000
                );
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
                useUiStore().showSuccess("Estado de la empresa actualizado correctamente.", "Éxito", 3000);
            } catch (error) {
                console.error("Error updating company status:", error);
                useUiStore().showError(
                    'Ocurrió un error al actualizar el estado de la empresa. Por favor, inténtalo de nuevo.',
                    "Error al actualizar",
                    3000
                );
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
                useUiStore().showError(
                    'Ocurrió un error al obtener los planes de suscripción. Por favor, inténtalo de nuevo.',
                    "Error al cargar suscripciones",
                    3000
                );
                throw error;
            }
        },
        async createSubscription(payload: UpsertSubscriptionPayload) {
            try {
                const response = await axios.post<SubscriptionPlan>("http://localhost:5267/api/subscriptions", payload, {
                    headers: { Authorization: `Bearer ${localStorage.getItem('token')}` }
                });

                this.subscriptions = [...this.subscriptions, response.data].sort((left, right) => left.price - right.price);
                useUiStore().showSuccess("Plan de suscripción creado correctamente.", "Éxito", 3000);
                return response.data;
            } catch (error) {
                console.error("Error creating subscription:", error);
                useUiStore().showError(
                    'Ocurrió un error al crear la suscripción. Por favor, inténtalo de nuevo.',
                    "Error al crear",
                    3000
                );
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
                useUiStore().showSuccess("Plan de suscripción actualizado correctamente.", "Éxito", 3000);
                return response.data;

            } catch (error) {
                console.error("Error updating subscription:", error);
                useUiStore().showError(
                    'Ocurrió un error al actualizar la suscripción. Por favor, inténtalo de nuevo.',
                    "Error al actualizar",
                    3000
                );
                throw error;
            }
        }
    }
})