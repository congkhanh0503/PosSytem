import axios from 'axios'
import { useNotify } from '@/composables/useNotify'

const apiClient = axios.create({
  baseURL: '/api',
  headers: {
    'Content-Type': 'application/json'
  },
  timeout: 30000
})

apiClient.interceptors.response.use(
  (response) => response,
  (error) => {
    const { toast } = useNotify()

    if (!error.response) {
      if (error.code === 'ECONNABORTED' || error.message?.includes('timeout')) {
        toast.error('Kết nối quá thời gian chờ (Timeout). Vui lòng thử lại!', 'Lỗi Kết Nối')
      } else {
        toast.error('Không thể kết nối đến máy chủ POS nội bộ!', 'Mất Kết Nối')
      }
    } else if (error.response.status >= 500) {
      const serverMsg = error.response.data?.message || 'Máy chủ POS gặp sự cố nội bộ. Vui lòng thử lại!'
      toast.error(serverMsg, `Lỗi Hệ Thống (${error.response.status})`)
    }

    return Promise.reject(error)
  }
)

export const api = {
  // Services
  getServices: (onlyActive = false) => apiClient.get('/services', { params: { onlyActive } }),
  createService: (data) => apiClient.post('/services', data),
  updateService: (id, data) => apiClient.put(`/services/${id}`, data),
  deleteService: (id) => apiClient.delete(`/services/${id}`),

  // Products
  getProducts: (onlyActive = false, onlyPos = false) => apiClient.get('/products', { params: { onlyActive, onlyPos } }),
  createProduct: (data) => apiClient.post('/products', data),
  updateProduct: (id, data) => apiClient.put(`/products/${id}`, data),
  deleteProduct: (id) => apiClient.delete(`/products/${id}`),
  updateStock: (id, stock) => apiClient.patch(`/products/${id}/stock`, stock, {
    headers: { 'Content-Type': 'application/json' }
  }),
  toggleShowOnPos: (id) => apiClient.patch(`/products/${id}/toggle-pos`),

  // Orders & POS
  getOrders: (params) => apiClient.get('/orders', { params }),
  getOrder: (id) => apiClient.get(`/orders/${id}`),
  createOrder: (orderData) => apiClient.post('/orders', orderData),
  updateOrder: (id, orderData) => apiClient.put(`/orders/${id}`, orderData),
  deleteOrder: (id) => apiClient.delete(`/orders/${id}`),
  cancelOrder: (id, data) => apiClient.post(`/orders/${id}/cancel`, data),
  closeShift: () => apiClient.post('/orders/close-shift'),

  // Service Categories
  getServiceCategories: () => apiClient.get('/servicecategories'),
  createServiceCategory: (data) => apiClient.post('/servicecategories', data),
  deleteServiceCategory: (id) => apiClient.delete(`/servicecategories/${id}`),

  // Product Categories
  getProductCategories: () => apiClient.get('/productcategories'),
  createProductCategory: (data) => apiClient.post('/productcategories', data),
  deleteProductCategory: (id) => apiClient.delete(`/productcategories/${id}`),

  // Dashboard
  getDashboardSummary: (params) => apiClient.get('/dashboard/summary', { params }),
  getDayDetail: (date) => apiClient.get('/dashboard/day-detail', { params: { date } }),

  // Settings
  getSettings: () => apiClient.get('/settings'),
  updateSettings: (data) => apiClient.put('/settings', data),

  // VietQR
  generateVietQr: (amount, orderCode, description) => 
    apiClient.get('/vietqr/generate', { params: { amount, orderCode, description } }),
  getVietQrByOrderId: (orderId) => apiClient.get(`/vietqr/order/${orderId}`),
  getPopularBanks: () => apiClient.get('/vietqr/popular-banks'),

  // Expenses (Quản lý chi tiêu)
  getExpenses: (params) => apiClient.get('/expenses', { params }),
  getExpense: (id) => apiClient.get(`/expenses/${id}`),
  createExpense: (data) => apiClient.post('/expenses', data),
  updateExpense: (id, data) => apiClient.put(`/expenses/${id}`, data),
  deleteExpense: (id) => apiClient.delete(`/expenses/${id}`),
  getExpenseCategories: () => apiClient.get('/expenses/categories'),

  // Backup & Restore
  getBackupInfo: () => apiClient.get('/backup/info'),
  downloadBackup: () => apiClient.get('/backup/download', { responseType: 'blob' }),
  restoreBackup: (formData) => apiClient.post('/backup/restore', formData, {
    headers: { 'Content-Type': 'multipart/form-data' }
  }),
  uploadCloudBackup: () => apiClient.post('/backup/cloud-upload'),
  getCloudBackups: () => apiClient.get('/backup/cloud-list'),
  restoreCloudBackup: (data) => apiClient.post('/backup/cloud-restore', data),

  // License & Bản quyền
  getLicenseStatus: () => apiClient.get('/license/status'),
  activateLicense: (licenseKey) => apiClient.post('/license/activate', { licenseKey }),
  syncLicense: () => apiClient.post('/license/sync'),
  initShop: (data) => apiClient.post('/license/init-shop', data),
  updateShopProfile: (data) => apiClient.post('/license/update-profile', data),

  // Hệ Thống & Phiên Bản (Version Management)
  getSystemVersion: () => apiClient.get('/system/version'),
  checkUpdate: () => apiClient.get('/system/check-update'),
  shutdownSystem: () => apiClient.post('/system/shutdown')
}

export default api
