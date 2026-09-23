import axios from 'axios'

const apiClient = axios.create({
  baseURL: '/api',
  headers: {
    'Content-Type': 'application/json'
  }
})

export const api = {
  // Services
  getServices: (onlyActive = false) => apiClient.get('/services', { params: { onlyActive } }),
  createService: (data) => apiClient.post('/services', data),
  updateService: (id, data) => apiClient.put(`/services/${id}`, data),
  deleteService: (id) => apiClient.delete(`/services/${id}`),

  // Products
  getProducts: (onlyActive = false) => apiClient.get('/products', { params: { onlyActive } }),
  createProduct: (data) => apiClient.post('/products', data),
  updateProduct: (id, data) => apiClient.put(`/products/${id}`, data),
  deleteProduct: (id) => apiClient.delete(`/products/${id}`),
  updateStock: (id, stock) => apiClient.patch(`/products/${id}/stock`, stock, {
    headers: { 'Content-Type': 'application/json' }
  }),

  // Orders & POS
  getOrders: (params) => apiClient.get('/orders', { params }),
  getOrder: (id) => apiClient.get(`/orders/${id}`),
  createOrder: (orderData) => apiClient.post('/orders', orderData),
  cancelOrder: (id) => apiClient.post(`/orders/${id}/cancel`),

  // Dashboard
  getDashboardSummary: (params) => apiClient.get('/dashboard/summary', { params }),
  getDayDetail: (date) => apiClient.get('/dashboard/day-detail', { params: { date } }),

  // Settings
  getSettings: () => apiClient.get('/settings'),
  updateSettings: (data) => apiClient.put('/settings', data),

  // VietQR
  generateVietQr: (amount, orderCode, description) => 
    apiClient.get('/vietqr/generate', { params: { amount, orderCode, description } }),
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
  })
}

export default api
