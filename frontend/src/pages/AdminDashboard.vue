<template>
    <v-container fluid class="pa-6 pa-md-8">
        <div class="mb-8">
            <h1 class="text-h4 font-weight-bold mb-2">Resumen global</h1>
            <p class="text-medium-emphasis">Vision general de la plataforma BotForge</p>
        </div>

        <KpiCardsGrid :items="cards" :is-loading="isLoading" row-class="mb-2" />

        <AdminCompaniesTable
            :companies="companies"
            :total="companyPanelTotal"
            :page="companyPanelPage"
            :page-size="companyPanelPageSize"
            :active-count="activeCompaniesCount"
            :in-review-count="inReviewCompaniesCount"
            :inactive-count="inactiveCompaniesCount"
            @status-change="handleCompanyStatusChange"
            @filters-change="handleCompanyFiltersChange"
        />
    </v-container>
</template>

<script setup lang="ts">
import { computed, onMounted, ref } from 'vue'
import { storeToRefs } from 'pinia'
import AdminCompaniesTable, { type AdminCompany } from '@/components/admin/AdminCompaniesTable.vue'
import KpiCardsGrid, { type KpiCardItem } from '@/components/dashboard/KpiCardsGrid.vue'
import { useSuperAdminStore } from '@/stores/useSuperAdminStore'
import type { CompanyLifecycleStatus } from '@/types/companyStatus'

const superAdminStore = useSuperAdminStore()
const {
    mrr,
    companyPanelData,
    totalCompanies,
    totalUsers,
    totalConversations,
    companyPanelTotal,
    companyPanelPage,
    companyPanelPageSize,
    activeCompaniesCount,
    inReviewCompaniesCount,
    inactiveCompaniesCount
} = storeToRefs(superAdminStore)

const formatEur = (value: number) => {
    return new Intl.NumberFormat('es-ES', {
        style: 'currency',
        currency: 'EUR',
        minimumFractionDigits: 2,
        maximumFractionDigits: 2
    }).format(value)
}

const cards = computed<KpiCardItem[]>(() => [
    { title: 'Empresas activas', value: `${totalCompanies.value}`, icon: 'mdi-office-building-outline' },
    { title: 'Usuarios finales', value: `${totalUsers.value}`, icon: 'mdi-account-group-outline' },
    { title: 'Conversaciones (mes)', value: `${totalConversations.value}`, icon: 'mdi-message-outline' },
    { title: 'MRR total', value: formatEur(mrr.value), icon: 'mdi-currency-eur' }
])

const companies = computed<AdminCompany[]>(() => companyPanelData.value)
const companyFilters = ref<{ query: string; status: 'Todas' | CompanyLifecycleStatus; page: number; pageSize: number }>({
    query: '',
    status: 'Todas',
    page: 1,
    pageSize: 10
})

const isLoading = ref(true)

const fetchCompanies = async () => {
    await superAdminStore.getCompanyPanelData({
        search: companyFilters.value.query,
        status: companyFilters.value.status,
        page: companyFilters.value.page,
        pageSize: companyFilters.value.pageSize
    })
}

const handleCompanyStatusChange = async (payload: { companyId: string; status: 'Active' | 'Inactive' | 'InReview' }) => {
    try {
        await superAdminStore.updateCompanyStatus(Number(payload.companyId), payload.status)
        await fetchCompanies()
    } catch (error) {
        console.error('Error updating company status:', error)
    }
}

const handleCompanyFiltersChange = async (payload: { query: string; status: 'Todas' | CompanyLifecycleStatus; page: number; pageSize: number }) => {
    companyFilters.value = payload
    await fetchCompanies()
}

onMounted(async () => {
    try {
        isLoading.value = true
        await superAdminStore.getAnalyticsData()
    } catch (error) {
        console.error('Error loading dashboard analytics:', error)
    }

    try {
        await fetchCompanies()
    } catch (error) {
        console.error('Error loading company panel data:', error)
    } finally {
        isLoading.value = false
    }
})
</script>

