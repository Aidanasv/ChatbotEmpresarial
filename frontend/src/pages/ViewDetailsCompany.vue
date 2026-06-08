<template>
	<v-container fluid class="pa-6 pa-md-8 setup-container">
		<div class="d-flex flex-wrap align-center justify-space-between ga-3 mb-8">
			<div>
				<v-btn variant="text" prepend-icon="mdi-arrow-left" class="px-0 mb-2" @click="goBack">
					Volver al resumen global
				</v-btn>
				<h1 class="text-h4 font-weight-bold mb-1">Detalle de empresa</h1>
				<p class="text-medium-emphasis mb-0">Vista de solo lectura de configuración empresarial</p>
			</div>

			<div class="d-flex align-center ga-2">
				<CompanyStatusSelector :model-value="companyStatus" :loading="statusLoading"
					@update:model-value="handleCompanyStatusChange" />
			</div>
		</div>

		<v-alert v-if="errorMessage" type="error" variant="tonal" class="mb-6">
			{{ errorMessage }}
		</v-alert>

		<v-card class="mb-6 pa-6 pa-md-8 rounded-xl" border elevation="0" :loading="isLoading">
			<div class="d-flex align-center mb-1">
				<v-icon color="primary" class="mr-3" size="28">mdi-office-building-outline</v-icon>
				<h2 class="text-h6 font-weight-bold mb-0">Información de tu empresa</h2>
			</div>
			<p class="text-medium-emphasis text-body-2 mb-6 ml-10">Datos visibles desde configuración de empresa</p>

			<v-form class="ml-md-10">
				<label class="form-label">Nombre de la empresa</label>
				<v-text-field :model-value="companySetup.companyName" variant="solo" flat class="setup-input mb-4"
					hide-details readonly />

				<label class="form-label">Razón social</label>
				<v-text-field :model-value="companySetup.legalName" variant="solo" flat class="setup-input mb-4"
					hide-details readonly />

				<label class="form-label">CIF / NIF</label>
				<v-text-field :model-value="companySetup.cif" variant="solo" flat class="setup-input mb-4" hide-details
					readonly />

				<label class="form-label">Correo electrónico de contacto</label>
				<v-text-field :model-value="companySetup.email" variant="solo" flat class="setup-input mb-4"
					hide-details readonly />

				<label class="form-label">Industria / Sector</label>
				<v-text-field :model-value="companySetup.sector" variant="solo" flat class="setup-input mb-4"
					hide-details readonly />

				<label class="form-label">Sitio web</label>
				<v-text-field :model-value="companySetup.website" variant="solo" flat class="setup-input mb-4"
					hide-details readonly />

				<label class="form-label">Descripción breve de la empresa</label>
				<v-textarea :model-value="companySetup.description" variant="solo" flat class="setup-input" rows="4"
					hide-details readonly />
			</v-form>
		</v-card>

		<v-card class="mb-6 pa-6 pa-md-8 rounded-xl" border elevation="0" :loading="isLoading">
			<div class="d-flex align-center mb-1">
				<v-icon color="primary" class="mr-3" size="28">mdi-file-document-multiple-outline</v-icon>
				<h2 class="text-h6 font-weight-bold mb-0">Documentos</h2>
			</div>
			<p class="text-medium-emphasis text-body-2 mb-6 ml-10">Listado de documentos cargados en conocimiento</p>

			<div class="ml-md-10">
				<v-alert v-if="knowledgeSetup.documents.length === 0" type="info" variant="tonal" density="comfortable"
					class="mb-0">
					Esta empresa no tiene documentos registrados.
				</v-alert>

				<v-list v-else class="bg-transparent pa-0">
					<v-list-item v-for="(doc, index) in knowledgeSetup.documents" :key="`${doc.name}-${index}`"
						class="border rounded-lg mb-2 pa-3">
						<template #prepend>
							<v-icon color="primary">mdi-file-outline</v-icon>
						</template>
						<v-list-item-title class="font-weight-medium">{{ doc.name }}</v-list-item-title>
						<v-list-item-subtitle v-if="doc.createdAt" class="text-caption">
							Subido: {{ doc.createdAt }}
						</v-list-item-subtitle>
					</v-list-item>
				</v-list>
			</div>
		</v-card>

		<v-card class="pa-6 pa-md-8 rounded-xl" border elevation="0" :loading="isLoading">
			<div class="d-flex align-center justify-space-between mb-1">
				<div class="d-flex align-center">
					<v-icon color="primary" class="mr-3" size="28">mdi-help-circle-outline</v-icon>
					<h2 class="text-h6 font-weight-bold mb-0">Preguntas frecuentes</h2>
				</div>
				<v-chip variant="outlined" color="primary" size="small" class="font-weight-bold">
					{{ knowledgeSetup.faqs.length }} FAQs
				</v-chip>
			</div>
			<p class="text-medium-emphasis text-body-2 mb-6 ml-10">Respuestas configuradas para el chatbot</p>

			<div class="ml-md-10">
				<v-alert v-if="knowledgeSetup.faqs.length === 0" type="info" variant="tonal" density="comfortable"
					class="mb-0">
					Esta empresa no tiene FAQs configuradas.
				</v-alert>

				<div v-else>
					<v-card v-for="(faq, index) in knowledgeSetup.faqs" :key="`${faq.question}-${index}`"
						variant="outlined" class="rounded-lg pa-4 mb-3 bg-transparent">
						<div class="font-weight-bold text-body-1 mb-1">{{ faq.question }}</div>
						<div class="text-medium-emphasis text-body-2">{{ faq.answer }}</div>
					</v-card>
				</div>
			</div>
		</v-card>
	</v-container>
</template>

<script setup lang="ts">
import { computed, onMounted, ref, watch } from 'vue'
import { storeToRefs } from 'pinia'
import { useRoute, useRouter } from 'vue-router'
import { useSetupStore } from '@/stores/useSetupStore'
import { useSuperAdminStore } from '@/stores/useSuperAdminStore'
import CompanyStatusSelector from '@/components/admin/CompanyStatusSelector.vue'
import type { CompanyLifecycleStatus } from '@/types/companyStatus'

const route = useRoute()
const router = useRouter()
const setupStore = useSetupStore()
const superAdminStore = useSuperAdminStore()
const { companySetup, knowledgeSetup, isLoading } = storeToRefs(setupStore)
const errorMessage = ref('')
const companyStatus = ref<CompanyLifecycleStatus>('InReview')
const statusLoading = ref(false)

const companyId = computed(() => String(route.params.companyId || '0'))

const fetchCompanyDetails = async () => {
	errorMessage.value = ''
	const parsedCompanyId = Number.parseInt(companyId.value, 10)

	if (Number.isNaN(parsedCompanyId) || parsedCompanyId <= 0) {
		errorMessage.value = 'El identificador de empresa no es valido.'
		return
	}

	try {
		await superAdminStore.getCompanyPanelData(200, 0)
		const matchedCompany = superAdminStore.companyPanelData.find((company) => Number(company.companyId) === parsedCompanyId)
		if (matchedCompany) {
			companyStatus.value = matchedCompany.status
		}

		await setupStore.getSetupData(parsedCompanyId)
	} catch (error) {
		console.error('Error loading company details:', error)
		errorMessage.value = 'No se pudieron cargar los datos de la empresa.'
	}
}

const handleCompanyStatusChange = async (nextStatus: CompanyLifecycleStatus) => {
	const parsedCompanyId = Number.parseInt(companyId.value, 10)
	if (Number.isNaN(parsedCompanyId) || parsedCompanyId <= 0) {
		return
	}

	try {
		statusLoading.value = true
		await superAdminStore.updateCompanyStatus(parsedCompanyId, nextStatus)
		companyStatus.value = nextStatus
	} catch (error) {
		console.error('Error updating company status from detail:', error)
		errorMessage.value = 'No se pudo actualizar el estado de la empresa.'
	} finally {
		statusLoading.value = false
	}
}

onMounted(async () => {
	await fetchCompanyDetails()
})

watch(
	() => route.params.companyId,
	async (newCompanyId, oldCompanyId) => {
		if (newCompanyId !== oldCompanyId) {
			await fetchCompanyDetails()
		}
	}
)

const goBack = () => {
	router.push('/dashboard/admin')
}
</script>
