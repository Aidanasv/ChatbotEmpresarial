<template>
  <v-app>
    <AppNavbar v-if="!isEmbedRoute" />

    <div v-if="!isEmbedRoute" class="app-global-alert-wrap">
      <StatusAlert
        :model-value="uiStore.globalAlert.visible"
        :type="uiStore.globalAlert.type"
        :message="uiStore.globalAlert.message"
        :title="uiStore.globalAlert.title"
        :closable="false"
      />
    </div>

    <v-main :style="isEmbedRoute ? 'background: transparent !important' : ''">
      <router-view />
    </v-main>

    <AppFooter v-if="!isEmbedRoute" />
  </v-app>
</template>

<script setup lang="ts">
import { computed } from 'vue'
import { useRoute } from 'vue-router'

import AppNavbar from '@/components/layout/AppNavbar.vue'
import AppFooter from '@/components/layout/AppFooter.vue'
import StatusAlert from '@/components/utils/StatusAlert.vue'
import { useUiStore } from '@/stores/useUiStore'

const route = useRoute()
const isEmbedRoute = computed(() => Boolean(route.meta.embed))
const uiStore = useUiStore()


</script>

<style scoped>
.app-global-alert-wrap {
  position: fixed;
  right: 16px;
  bottom: 16px;
  width: min(280px, calc(100vw - 24px));
  z-index: 1200;
}
</style>