import { createRouter, createWebHistory } from 'vue-router'
import POSView from '@/views/POSView.vue'
import DashboardView from '@/views/DashboardView.vue'
import ServicesView from '@/views/ServicesView.vue'
import ProductsView from '@/views/ProductsView.vue'
import OrdersView from '@/views/OrdersView.vue'
import ExpensesView from '@/views/ExpensesView.vue'
import SettingsView from '@/views/SettingsView.vue'

const routes = [
  {
    path: '/',
    name: 'POS',
    component: POSView,
    meta: { title: 'Bán Hàng POS' }
  },
  {
    path: '/dashboard',
    name: 'Dashboard',
    component: DashboardView,
    meta: { title: 'Báo Cáo Doanh Thu' }
  },
  {
    path: '/expenses',
    name: 'Expenses',
    component: ExpensesView,
    meta: { title: 'Quản Lý Chi Tiêu' }
  },
  {
    path: '/services',
    name: 'Services',
    component: ServicesView,
    meta: { title: 'Quản Lý Dịch Vụ' }
  },
  {
    path: '/products',
    name: 'Products',
    component: ProductsView,
    meta: { title: 'Quản Lý Sản Phẩm & Tồn Kho' }
  },
  {
    path: '/orders',
    name: 'Orders',
    component: OrdersView,
    meta: { title: 'Lịch Sử Đơn Hàng' }
  },
  {
    path: '/settings',
    name: 'Settings',
    component: SettingsView,
    meta: { title: 'Cài Đặt VietQR & Tiệm' }
  }
]

const router = createRouter({
  history: createWebHistory(),
  routes
})

router.beforeEach((to, from, next) => {
  document.title = `${to.meta.title || 'POS'} | Công Barber`
  next()
})

export default router
