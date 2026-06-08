<template>
  <v-menu :close-on-content-click="true">
    <template #activator="{ props, isActive }">
      <v-chip v-bind="props" size="small" variant="tonal" class="status-selector-chip"
        :color="companyStatusColors[modelValue]" :disabled="disabled || loading">
        <span>{{ companyStatusLabels[modelValue] }}</span>
        <v-icon end size="16" class="status-selector-arrow" :class="{ 'status-selector-arrow--open': isActive }">
          mdi-chevron-down
        </v-icon>
      </v-chip>
    </template>

    <v-list density="compact" min-width="170">
      <v-list-item v-for="option in companyStatusOptions" :key="option.value" :active="option.value === modelValue"
        :disabled="loading" @click="handleSelect(option.value)">
        <template #prepend>
          <v-icon size="18" :color="companyStatusColors[option.value]">mdi-circle</v-icon>
        </template>
        <v-list-item-title>{{ option.label }}</v-list-item-title>
      </v-list-item>
    </v-list>
  </v-menu>
</template>

<script setup lang="ts">
import { companyStatusColors, companyStatusLabels, companyStatusOptions, type CompanyLifecycleStatus } from '@/types/companyStatus'

const props = withDefaults(defineProps<{
  modelValue: CompanyLifecycleStatus
  loading?: boolean
  disabled?: boolean
}>(), {
  loading: false,
  disabled: false
})

const emit = defineEmits<{
  (event: 'update:modelValue', value: CompanyLifecycleStatus): void
}>()

const handleSelect = (nextStatus: CompanyLifecycleStatus) => {
  if (nextStatus === props.modelValue) {
    return
  }

  emit('update:modelValue', nextStatus)
}
</script>
