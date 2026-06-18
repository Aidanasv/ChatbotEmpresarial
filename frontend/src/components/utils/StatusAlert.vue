<template>
  <v-alert
    v-if="visible && hasMessage"
    :type="resolvedType"
    :text="message"
    :closable="closable"
    class="status-alert"
    border="start"
    @click:close="handleClose"
  >
    <slot></slot>
  </v-alert>
</template>

<script setup lang="ts">
import { computed } from 'vue'

type AlertType = 'success' | 'warning' | 'error' | 'info'

const props = withDefaults(defineProps<{
  modelValue?: boolean
  message?: string
  title?: string
  type?: AlertType
  closable?: boolean
  variant?: 'flat' | 'tonal' | 'outlined' | 'text' | 'plain'
}>(), {
  modelValue: true,
  message: '',
  title: '',
  type: 'info',
  closable: true,
  variant: 'tonal'
})

const emit = defineEmits<{
  (event: 'update:modelValue', value: boolean): void
}>()

const resolvedType = computed<AlertType>(() => props.type)
const visible = computed(() => props.modelValue)
const hasMessage = computed(() => props.message.trim().length > 0)

const handleClose = () => {
  emit('update:modelValue', false)
}
</script>

<style scoped>
.status-alert {
  width: 100%;
}
</style>
