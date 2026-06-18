import { defineStore } from 'pinia'

export type GlobalAlertType = 'success' | 'warning' | 'error' | 'info'

type GlobalAlertState = {
  visible: boolean
  type: GlobalAlertType
  title: string
  message: string
}

let alertTimeoutId: ReturnType<typeof setTimeout> | null = null;

export const useUiStore = defineStore('ui', {
  state: () => ({
    dashboardDrawerOpen: true,
    globalAlert: {
      visible: false,
      type: 'info',
      title: '',
      message: ''
    } as GlobalAlertState
  }),
  actions: {
    toggleDashboardDrawer() {
      this.dashboardDrawerOpen = !this.dashboardDrawerOpen
    },
    setDashboardDrawer(open: boolean) {
      this.dashboardDrawerOpen = open
    },
    showAlert(payload: {
      message: string
      type?: GlobalAlertType
      title?: string
      autoCloseMs?: number
    }) {
      this.globalAlert.visible = true
      this.globalAlert.type = payload.type ?? 'info'
      this.globalAlert.title = payload.title ?? ''
      this.globalAlert.message = payload.message

      const duration = payload.autoCloseMs ?? 3000

      if (alertTimeoutId) {
        clearTimeout(alertTimeoutId)
      }

      if (duration > 0) {
        alertTimeoutId = setTimeout(() => {
          this.hideAlert()
        }, duration)
      }
    },
    showSuccess(message: string, title = '', autoCloseMs = 3000) {
      this.showAlert({ message, title, type: 'success', autoCloseMs })
    },
    showWarning(message: string, title = '', autoCloseMs = 3000) {
      this.showAlert({ message, title, type: 'warning', autoCloseMs })
    },
    showError(message: string, title = '', autoCloseMs = 3000) {
      this.showAlert({ message, title, type: 'error', autoCloseMs })
    },
    showInfo(message: string, title = '', autoCloseMs = 3000) {
      this.showAlert({ message, title, type: 'info', autoCloseMs })
    },
    hideAlert() {
      this.globalAlert.visible = false
      this.globalAlert.title = ''
      this.globalAlert.message = ''
      
      if (alertTimeoutId) {
        clearTimeout(alertTimeoutId)
        alertTimeoutId = null
      }
    }
  }
})