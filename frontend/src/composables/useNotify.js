import { reactive } from 'vue'

const state = reactive({
  toasts: [],
  confirmModal: {
    isOpen: false,
    title: '',
    message: '',
    type: 'danger', // 'danger' | 'warning' | 'info' | 'success'
    confirmText: 'Xác nhận',
    cancelText: 'Hủy',
    resolve: null
  }
})

let nextToastId = 1

export function useNotify() {
  const showToast = (message, type = 'info', title = '', duration = 4000) => {
    const id = nextToastId++
    const toastItem = {
      id,
      message,
      type, // 'success' | 'error' | 'warning' | 'info'
      title,
      duration
    }
    state.toasts.push(toastItem)

    if (duration > 0) {
      setTimeout(() => {
        removeToast(id)
      }, duration)
    }
    return id
  }

  const removeToast = (id) => {
    const idx = state.toasts.findIndex(t => t.id === id)
    if (idx !== -1) {
      state.toasts.splice(idx, 1)
    }
  }

  const toast = (message, options = {}) => {
    return showToast(
      message,
      options.type || 'info',
      options.title || '',
      options.duration ?? 4000
    )
  }

  toast.success = (message, title = 'Thành công', duration = 4000) =>
    showToast(message, 'success', title, duration)

  toast.error = (message, title = 'Lỗi', duration = 5000) =>
    showToast(message, 'error', title, duration)

  toast.warning = (message, title = 'Cảnh báo', duration = 4500) =>
    showToast(message, 'warning', title, duration)

  toast.info = (message, title = 'Thông báo', duration = 4000) =>
    showToast(message, 'info', title, duration)

  const confirm = ({
    title = 'Xác nhận thao tác',
    message = 'Bạn có chắc chắn muốn thực hiện hành động này?',
    type = 'danger',
    confirmText = 'Xác nhận',
    cancelText = 'Hủy'
  } = {}) => {
    return new Promise((resolve) => {
      state.confirmModal = {
        isOpen: true,
        title,
        message,
        type,
        confirmText,
        cancelText,
        resolve
      }
    })
  }

  const handleConfirmAction = (accepted) => {
    if (state.confirmModal.resolve) {
      state.confirmModal.resolve(accepted)
    }
    state.confirmModal.isOpen = false
    state.confirmModal.resolve = null
  }

  return {
    state,
    toast,
    confirm,
    removeToast,
    handleConfirmAction
  }
}
