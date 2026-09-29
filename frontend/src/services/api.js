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
  cancelOrder: (id) => apiClient.post(`/orders/${id}/cancel`),

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

  // License & Bản quyền
  getLicenseStatus: () => apiClient.get('/license/status'),
  activateLicense: (licenseKey) => apiClient.post('/license/activate', { licenseKey }),
  syncLicense: () => apiClient.post('/license/sync'),
  initShop: (data) => apiClient.post('/license/init-shop', data),
  updateShopProfile: (data) => apiClient.post('/license/update-profile', data)
}

export default api
