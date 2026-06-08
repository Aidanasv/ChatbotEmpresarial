import { defineStore } from 'pinia'

export const useUiStore = defineStore('ui', {
  state: () => ({
    dashboardDrawerOpen: true
  }),
  actions: {
    toggleDashboardDrawer() {
      this.dashboardDrawerOpen = !this.dashboardDrawerOpen
    },
    setDashboardDrawer(open: boolean) {
      this.dashboardDrawerOpen = open
    }
  }
})
