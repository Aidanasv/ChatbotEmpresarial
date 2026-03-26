<template>
  <v-app>
    <AppNavbar />

    <v-main>
      <router-view />
    </v-main>

    <AppFooter />
  </v-app>
</template>

<script setup lang="ts">
import { onMounted } from 'vue'
import { useTheme } from 'vuetify'
import AppNavbar from '@/components/layout/AppNavbar.vue'
import AppFooter from '@/components/layout/AppFooter.vue'

const theme = useTheme()

onMounted(() => {
  const preferDark = window.matchMedia('(prefers-color-scheme: dark)')
  theme.global.name.value = preferDark.matches ? 'dark' : 'light'

  preferDark.addEventListener('change', (e) => {
    theme.global.name.value = e.matches ? 'dark' : 'light'
  })
})
</script>