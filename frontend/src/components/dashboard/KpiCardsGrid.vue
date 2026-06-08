<template>
  <v-row :class="rowClass">
    <v-col cols="12" sm="6" md="3" v-for="(kpi, i) in items" :key="`${kpi.title}-${i}`">
      <v-card class="pa-5 h-100 rounded-xl bg-surface" border elevation="0">
        <div class="d-flex justify-space-between align-center mb-3 ga-2">
          <div class="text-h4 font-weight-bold">
            <v-progress-circular v-if="isLoading" indeterminate color="primary" size="24"
              width="3"></v-progress-circular>
            <span v-else>{{ kpi.value }}</span>
          </div>
          <div class="text-primary">
            <v-icon :icon="kpi.icon" size="large"></v-icon>
          </div>
        </div>

        <div class="text-caption text-medium-emphasis">{{ kpi.title }}</div>
      </v-card>
    </v-col>
  </v-row>
</template>

<script setup lang="ts">
export type KpiCardItem = {
  title: string
  value: string | number
  icon: string
}

withDefaults(
  defineProps<{
    items: KpiCardItem[]
    isLoading?: boolean
    rowClass?: string
  }>(),
  {
    isLoading: false,
    rowClass: 'mb-6'
  }
)
</script>
