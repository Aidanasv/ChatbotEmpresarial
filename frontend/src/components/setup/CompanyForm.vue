<template>
  <div class="step-content text-left">
    <div class="mb-8">
      <h2 class="text-h5 font-weight-bold mb-1">Información de tu empresa</h2>
      <p class="text-medium-emphasis">Cuéntanos sobre tu negocio para personalizar el chatbot</p>
    </div>

    <v-form>
      <label class="form-label">Nombre de la empresa <span class="text-error">*</span></label>
      <v-text-field
        :model-value="modelValue.companyName"
        @update:model-value="update('companyName', $event)"
        placeholder="Acme Corp"
        variant="solo" flat class="setup-input mb-4" hide-details
      ></v-text-field>

      <label class="form-label">Industria / Sector</label>
      <v-text-field
        :model-value="modelValue.industry"
        @update:model-value="update('industry', $event)"
        placeholder="Ej: Tecnología, Salud, E-commerce..."
        variant="solo" flat class="setup-input mb-4" hide-details
      ></v-text-field>

      <label class="form-label">Sitio web</label>
      <v-text-field
        :model-value="modelValue.website"
        @update:model-value="update('website', $event)"
        placeholder="https://tuempresa.com"
        variant="solo" flat class="setup-input mb-4" hide-details
      ></v-text-field>

      <label class="form-label">Descripción breve de la empresa</label>
      <v-textarea
        :model-value="modelValue.description"
        @update:model-value="update('description', $event)"
        placeholder="¿A qué se dedica tu empresa? ¿Cuáles son sus principales productos o servicios?"
        variant="solo" flat class="setup-input" rows="4" hide-details
      ></v-textarea>
    </v-form>
  </div>
</template>

<script setup lang="ts">
const props = defineProps<{
  modelValue: {
    companyName: string;
    industry: string;
    website: string;
    description: string;
  }
}>()

const emit = defineEmits(['update:modelValue'])

const update = (field: string, value: any) => {
  emit('update:modelValue', { ...props.modelValue, [field]: value })
}
</script>