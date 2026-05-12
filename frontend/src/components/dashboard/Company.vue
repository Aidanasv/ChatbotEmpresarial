<template>
    <v-container fluid class="pa-6 pa-md-8 analytics-container">

        <CompanyForm v-model="company" />

        <div class="d-flex justify-end mt-8">
            <v-btn color="primary" size="large" rounded="lg" @click="saveChanges" :loading="setupStore.isLoading"
                class="text-none font-weight-bold px-8">
                Guardar cambios
            </v-btn>
        </div>

    </v-container>
</template>

<script setup lang="ts">
import { computed } from 'vue'
import { useSetupStore } from '@/stores/useSetupStore'
import CompanyForm from '@/components/setup/CompanyForm.vue'

const setupStore = useSetupStore()

const company = computed({
    get: () => setupStore.companySetup,
    set: (val) => { setupStore.companySetup = val }
})

const saveChanges = async () => {
    try {
        await setupStore.saveCompanySetup(company.value)

        console.log("¡Datos de la empresa guardados correctamente!")
    } catch (error) {
        console.error("Error al guardar la empresa:", error)
    }
}
</script>