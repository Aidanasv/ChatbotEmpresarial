<template>
    <v-card class="admin-table-card" rounded="xl" elevation="0">
        <v-card-text>
            <div class="d-flex flex-column flex-md-row justify-space-between align-start align-md-center ga-4 mb-4">
                <div>
                    <h3 class="text-h6 font-weight-bold mb-1">Empresas registradas</h3>
                    <p class="text-body-2 text-medium-emphasis mb-0">{{ total }} empresas en la
                        plataforma</p>
                </div>

                <div class="d-flex align-center ga-2 w-100 w-md-auto">
                    <v-text-field v-model="query" density="comfortable" prepend-inner-icon="mdi-magnify"
                        variant="outlined" hide-details rounded="lg" placeholder="Buscar empresa..."
                        class="admin-search" />
                </div>
            </div>

            <v-tabs v-model="statusFilter" bg-color="transparent" color="primary" class="mb-3" density="compact">
                <v-tab value="Todas">Todas ({{ total }})</v-tab>
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
                    <tr v-for="company in companies" :key="company.companyId" class="v-card--link">
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

            <div class="d-flex justify-end align-center ga-2 mt-4">
                <v-btn variant="outlined" size="small" class="text-none" :disabled="page <= 1" @click="goToPreviousPage">
                    Anterior
                </v-btn>
                <v-chip size="small" variant="tonal">Página {{ page }} / {{ totalPages }}</v-chip>
                <v-btn variant="outlined" size="small" class="text-none" :disabled="page >= totalPages" @click="goToNextPage">
                    Siguiente
                </v-btn>
            </div>
        </v-card-text>
    </v-card>
</template>

<script setup lang="ts">
import { computed, ref, watch } from 'vue'
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
    total: number
    page: number
    pageSize: number
    activeCount: number
    inReviewCount: number
    inactiveCount: number
}>()

const emit = defineEmits<{
    (event: 'status-change', payload: { companyId: string; status: CompanyLifecycleStatus }): void
    (event: 'filters-change', payload: { query: string; status: 'Todas' | CompanyLifecycleStatus; page: number; pageSize: number }): void
}>()

const query = ref('')
const statusFilter = ref<'Todas' | CompanyLifecycleStatus>('Todas')
const router = useRouter()
const page = ref(props.page || 1)
const totalPages = computed(() => Math.max(1, Math.ceil(props.total / props.pageSize)))
let searchDebounce: ReturnType<typeof setTimeout> | null = null

const emitFilters = () => {
    emit('filters-change', {
        query: query.value,
        status: statusFilter.value,
        page: page.value,
        pageSize: props.pageSize
    })
}

const goToPreviousPage = () => {
    if (page.value <= 1) return
    page.value -= 1
    emitFilters()
}

const goToNextPage = () => {
    if (page.value >= totalPages.value) return
    page.value += 1
    emitFilters()
}

watch(() => props.page, (nextPage) => {
    page.value = nextPage
})

watch(statusFilter, () => {
    page.value = 1
    emitFilters()
})

watch(query, () => {
    if (searchDebounce) {
        clearTimeout(searchDebounce)
    }

    searchDebounce = setTimeout(() => {
        page.value = 1
        emitFilters()
    }, 350)
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
