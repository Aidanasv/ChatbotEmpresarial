<template>
  <div class="setup-page-wrapper">
    <v-container class="py-12 setup-container">

      <div class="text-center mb-10">
        <v-chip variant="tonal" color="primary" class="mb-4 font-weight-bold">
          <v-icon start size="small">mdi-auto-fix</v-icon>
          Configuración inicial
        </v-chip>
        <h1 class="text-h3 font-weight-black mb-2">Crea tu chatbot</h1>
        <p class="text-medium-emphasis">Completa los pasos para personalizar tu asistente virtual</p>
      </div>

      <v-card flat class="mx-auto setup-card rounded-xl pa-8">

        <div class="stepper-nav d-flex justify-center align-center mb-10 flex-wrap">
          <template v-for="(item, index) in stepsList" :key="index">
            <v-chip :class="['step-pill', { 'is-active': step >= index + 1 }]"
              variant="flat" class="px-5 py-4 font-weight-bold">
              <v-icon start size="small" v-if="step > index + 1">mdi-check</v-icon>
              <v-icon start size="small" v-else>{{ item.icon }}</v-icon>
              {{ item.text }}
            </v-chip>
            <div v-if="index < 2" :class="['step-line', { 'is-filled': step > index + 1 }]"></div>
          </template>
        </div>

        <v-window v-model="step" touchless>
          <v-window-item :value="1">
            <CompanyForm v-model="chatbotData.companySetup" />
          </v-window-item>
          <v-window-item :value="2">
            <PersonalityForm v-model="chatbotData.personalitySetup" />
          </v-window-item>
          <v-window-item :value="3">
            <AppearanceForm v-model="chatbotData.appearanceSetup" />
          </v-window-item>
        </v-window>

        <div class="d-flex justify-space-between align-center mt-10 pt-6 border-top">
          <v-btn variant="text" @click="step--" :disabled="step === 1" class="text-none text-medium-emphasis">
            <v-icon start>mdi-chevron-left</v-icon> Anterior
          </v-btn>

          <span class="text-caption font-weight-bold text-medium-emphasis">Paso {{ step }} de 3</span>

          <v-btn color="primary" size="large" rounded="lg" @click="handleNext" class="text-none px-8 setup-next-btn">
            {{ step === 3 ? 'Crear chatbot' : 'Siguiente' }}
            <v-icon end v-if="step < 3">mdi-chevron-right</v-icon>
          </v-btn>
        </div>
      </v-card>
    </v-container>
  </div>
</template>

<script setup lang="ts">
import { ref } from 'vue'
import CompanyForm from '@/components/setup/CompanyForm.vue'
import PersonalityForm from '@/components/setup/PersonalityForm.vue'
import AppearanceForm from '@/components/setup/AppearanceForm.vue'
import type { SetUpState } from '@/types/setup'
import { useSetupStore } from '@/stores/useSetupStore'

const setupStore = useSetupStore()
const step = ref(setupStore.step || 1)


const stepsList = [
  { text: 'Empresa', icon: 'mdi-domain' },
  { text: 'Personalidad', icon: 'mdi-account-voice' },
  { text: 'Apariencia', icon: 'mdi-palette-outline' }
]
const chatbotData = ref<SetUpState>({
  step: setupStore.step,
  companySetup: setupStore.companySetup,
  personalitySetup: setupStore.personalitySetup,
  appearanceSetup: setupStore.appearanceSetup,
})

const handleNext = async () => {

  if (step.value === 1) {
    await setupStore.setCompanySetup(chatbotData.value.companySetup)
  } else if (step.value === 2) {
    await setupStore.setPersonalitySetup(chatbotData.value.personalitySetup)
  } else {
    await setupStore.setAppearanceSetup(chatbotData.value.appearanceSetup)
    await setupStore.saveInitialSetup()
  }

  if (step.value < 3) {
    step.value++
  }
}
</script>