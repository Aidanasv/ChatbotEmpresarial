<template>
    <v-card class="admin-table-card" rounded="xl" elevation="0">
        <v-card-text>
            <div class="d-flex flex-column flex-md-row justify-space-between align-start align-md-center ga-4 mb-4">
                <div>
                    <h3 class="text-h6 font-weight-bold mb-1">Empresas registradas</h3>
                    <p class="text-body-2 text-medium-emphasis mb-0">{{ filteredCompanies.length }} empresas en la
                        plataforma</p>
                </div>

                <div class="d-flex align-center ga-2 w-100 w-md-auto">
                    <v-text-field v-model="query" density="comfortable" prepend-inner-icon="mdi-magnify"
                        variant="outlined" hide-details rounded="lg" placeholder="Buscar empresa..."
                        class="admin-search" />
                    <v-btn variant="outlined" rounded="lg">Exportar</v-btn>
                </div>
            </div>

            <v-tabs v-model="statusFilter" bg-color="transparent" color="primary" class="mb-3" density="compact">
                <v-tab value="Todas">Todas ({{ companies.length }})</v-tab>
                <v-tab value="Active">Activas ({{ activeCount }})</v-tab>
                <v-tab value="InReview">En revision ({{ inReviewCount }})</v-tab>
                <v-tab value="Inactive">Inactivas ({{ inactiveCount }})</v-tab>
            </v-tabs>

            <v-table class="admin-companies-table">
                <thead>
                    <tr>
                        <th>Empresa</th>
                        <th>Plan</th>
                        <th>Usuarios</th>
                        <th>Conversaciones</th>
                        <th>MRR</th>
                        <th>Estado</th>
                        <th>Registro</th>
                        <th class="text-right"></th>
                    </tr>
                </thead>
                <tbody>
                    <tr v-for="company in filteredCompanies" :key="company.companyId" class="v-card--link">
                        <td>
                            <div class="d-flex align-center ga-3">
                                <v-avatar color="blue-lighten-5" size="34">{{ company.initials }}</v-avatar>
                                <div>
                                    <p class="font-weight-bold mb-0">{{ company.companyName }}</p>
                                    <p class="text-body-2 text-medium-emphasis mb-0">{{ company.companyEmail }}</p>
                                </div>
                            </div>
                        </td>
                        <td><v-chip size="small" variant="flat">{{ company.companySubscription }}</v-chip></td>
                        <td>{{ company.users }}</td>
                        <td>{{ company.conversations }}</td>
                        <td>{{ company.mrr.toFixed(2) }} €</td>
                        <td>
                            <CompanyStatusSelector :model-value="company.status"
                                @update:model-value="(nextStatus) => handleStatusChange(company, nextStatus)" />
                        </td>
                        <td>{{ company.createdAt }}</td>

                        <td class="text-right">
                            <v-btn variant="text" icon="mdi-eye-outline" size="small" color="grey-darken-1"
                                @click="verDetalles(company)"></v-btn>
                        </td>
                    </tr>
                </tbody>
            </v-table>
        </v-card-text>
    </v-card>
</template>

<script setup lang="ts">
import { computed, ref } from 'vue'
import { useRouter } from 'vue-router'
import CompanyStatusSelector from '@/components/admin/CompanyStatusSelector.vue'
import type { CompanyLifecycleStatus } from '@/types/companyStatus'

export type AdminCompany = {
    companyId: string
    initials: string
    companyName: string
    companyEmail: string
    companySubscription: string
    users: number
    conversations: number
    mrr: number
    status: CompanyLifecycleStatus
    createdAt: string
}

const props = defineProps<{
    companies: AdminCompany[]
}>()

const emit = defineEmits<{
    (event: 'status-change', payload: { companyId: string; status: CompanyLifecycleStatus }): void
}>()

const query = ref('')
const statusFilter = ref<'Todas' | CompanyLifecycleStatus>('Todas')
const router = useRouter()

const activeCount = computed(() => props.companies.filter((c) => c.status === 'Active').length)
const inReviewCount = computed(() => props.companies.filter((c) => c.status === 'InReview').length)
const inactiveCount = computed(() => props.companies.filter((c) => c.status === 'Inactive').length)

const filteredCompanies = computed(() => {
    const q = query.value.trim().toLowerCase()

    return props.companies.filter((company) => {
        const matchesStatus = statusFilter.value === 'Todas' || company.status === statusFilter.value
        const matchesQuery =
            q.length === 0 ||
            company.companyName.toLowerCase().includes(q) ||
            company.companyEmail.toLowerCase().includes(q)

        return matchesStatus && matchesQuery
    })
})

const verDetalles = (company: AdminCompany) => {
    router.push({
        name: 'dashboard-admin-company-details',
        params: { companyId: company.companyId }
    })
}

const handleStatusChange = (company: AdminCompany, nextStatus: CompanyLifecycleStatus) => {
    emit('status-change', { companyId: company.companyId, status: nextStatus })
}
</script>
