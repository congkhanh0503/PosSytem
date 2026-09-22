<template>
  <div class="p-6 space-y-6 overflow-y-auto h-screen max-w-7xl mx-auto">
    <!-- Header -->
    <div class="border-b border-barber-border pb-4">
      <h2 class="text-2xl font-extrabold text-white">Lịch Sử Đơn Hàng Điện Tử</h2>
      <p class="text-xs text-zinc-400 mt-0.5">Quản lý giao dịch, lọc theo lịch và đối soát doanh thu theo khoảng thời gian</p>
    </div>

    <!-- BỘ LỌC LỊCH THÔNG MINH (SMART DATE FILTER) -->
    <div class="bg-barber-card border border-barber-border rounded-2xl p-4 space-y-4 shadow-xl">
      
      <!-- Hàng 1: Các nút chọn nhanh khoảng thời gian -->
      <div class="flex flex-wrap items-center justify-between gap-3">
        <div class="flex items-center gap-1.5 overflow-x-auto no-scrollbar py-0.5 text-xs">
          <button
            v-for="preset in datePresets"
            :key="preset.id"
            @click="applyPreset(preset.id)"
            class="px-3.5 py-1.5 rounded-xl font-bold transition-all duration-200 whitespace-nowrap flex items-center gap-1.5"
            :class="selectedPreset === preset.id ? 'bg-barber-gold text-black shadow-md shadow-amber-500/20' : 'bg-zinc-800/80 text-zinc-400 hover:bg-zinc-800 hover:text-white'"
          >
            <Calendar class="w-3.5 h-3.5" />
            <span>{{ preset.label }}</span>
          </button>
        </div>

        <!-- Bộ lọc phụ: Phương thức & Trạng thái -->
        <div class="flex items-center gap-2 text-xs">
          <select
            v-model="filters.paymentMethod"
            @change="loadOrders"
            class="bg-barber-dark border border-zinc-700 rounded-xl px-3 py-1.5 text-white focus:outline-none focus:border-barber-gold"
          >
            <option value="">Tất cả hình thức</option>
            <option value="VietQR">VietQR</option>
            <option value="Cash">Tiền mặt</option>
          </select>

          <select
            v-model="filters.status"
            @change="loadOrders"
            class="bg-barber-dark border border-zinc-700 rounded-xl px-3 py-1.5 text-white focus:outline-none focus:border-barber-gold"
          >
            <option value="">Tất cả trạng thái</option>
            <option value="Completed">Đã thanh toán</option>
            <option value="Cancelled">Đã hủy</option>
          </select>

          <button
            @click="resetFilters"
            class="p-2 rounded-xl bg-zinc-800 hover:bg-zinc-700 text-zinc-300 transition"
            title="Làm mới bộ lọc"
          >
            <RotateCcw class="w-3.5 h-3.5" />
          </button>
        </div>
      </div>

      <!-- Hàng 2: Input Từ Ngày - Đến Ngày (Khi chọn Tùy chọn) -->
      <div v-if="selectedPreset === 'custom'" class="flex flex-wrap items-center gap-3 pt-3 border-t border-zinc-800 text-xs animate-fade-in">
        <span class="text-zinc-400 font-medium">Khoảng ngày:</span>
        <div class="flex items-center gap-2">
          <label class="text-zinc-500">Từ:</label>
          <input
            v-model="filters.fromDate"
            type="date"
            @change="loadOrders"
            class="bg-barber-dark border border-zinc-700 rounded-xl px-3 py-1.5 text-white focus:outline-none focus:border-barber-gold"
          />
        </div>
        <div class="flex items-center gap-2">
          <label class="text-zinc-500">Đến:</label>
          <input
            v-model="filters.toDate"
            type="date"
            @change="loadOrders"
            class="bg-barber-dark border border-zinc-700 rounded-xl px-3 py-1.5 text-white focus:outline-none focus:border-barber-gold"
          />
        </div>
      </div>

      <!-- Hàng 3: Thống kê nhanh của kết quả lọc (Metrics Summary) -->
      <div class="pt-3 border-t border-zinc-800 grid grid-cols-1 sm:grid-cols-3 gap-3 text-xs">
        <div class="p-2.5 rounded-xl bg-barber-dark/70 border border-zinc-800 flex justify-between items-center">
          <span class="text-zinc-400">Số lượng đơn:</span>
          <span class="font-extrabold text-white text-sm">{{ orders.length }} đơn</span>
        </div>
        <div class="p-2.5 rounded-xl bg-barber-dark/70 border border-zinc-800 flex justify-between items-center">
          <span class="text-zinc-400">Tổng tiền đã giảm giá:</span>
          <span class="font-bold text-emerald-400">{{ formatCurrency(totalFilteredDiscount) }}</span>
        </div>
        <div class="p-2.5 rounded-xl bg-barber-dark/70 border border-zinc-800 flex justify-between items-center">
          <span class="text-zinc-400">Tổng doanh thu thực nhận:</span>
          <span class="font-extrabold text-barber-gold text-base">{{ formatCurrency(totalFilteredRevenue) }}</span>
        </div>
      </div>

    </div>

    <!-- Orders Table -->
    <div class="bg-barber-card border border-barber-border rounded-2xl overflow-hidden shadow-xl">
      <div class="overflow-x-auto">
        <table class="w-full text-left text-xs">
          <thead class="bg-zinc-900/80 text-zinc-400 uppercase tracking-wider font-semibold border-b border-barber-border">
            <tr>
              <th class="py-3.5 px-4">Mã Đơn</th>
              <th class="py-3.5 px-4">Thời Gian</th>
              <th class="py-3.5 px-4">Khách Hàng</th>
              <th class="py-3.5 px-4">Tạm Tính</th>
              <th class="py-3.5 px-4">Giảm Giá (%)</th>
              <th class="py-3.5 px-4">Thực Thu</th>
              <th class="py-3.5 px-4">Thanh Toán</th>
              <th class="py-3.5 px-4">Ghi Chú</th>
              <th class="py-3.5 px-4">Trạng Thái</th>
              <th class="py-3.5 px-4 text-right">Chi Tiết</th>
            </tr>
          </thead>
          <tbody class="divide-y divide-zinc-800/60">
            <tr v-if="orders.length === 0">
              <td colspan="10" class="py-12 text-center text-zinc-500">
                Không tìm thấy đơn hàng nào trong khoảng thời gian này
              </td>
            </tr>

            <tr 
              v-for="order in orders" 
              :key="order.id" 
              @click="openDetail(order)"
              class="hover:bg-zinc-800/40 cursor-pointer transition"
            >
              <td class="py-3.5 px-4">
                <span class="font-mono font-bold text-amber-200">
                  {{ order.orderCode }}
                </span>
              </td>
              <td class="py-3.5 px-4 text-zinc-400">
                {{ formatDateTime(order.createdAt) }}
              </td>
              <td class="py-3.5 px-4 font-medium text-white">
                {{ order.customerName || 'Khách vãng lai' }}
              </td>
              <td class="py-3.5 px-4 text-zinc-400">
                {{ formatCurrency(order.subTotal) }}
              </td>
              <td class="py-3.5 px-4">
                <span v-if="order.discountPercent > 0" class="text-emerald-400 font-bold">
                  -{{ formatCurrency(order.discountAmount) }} ({{ order.discountPercent }}%)
                </span>
                <span v-else class="text-zinc-600">0%</span>
              </td>
              <td class="py-3.5 px-4 font-extrabold text-barber-gold text-sm">
                {{ formatCurrency(order.finalAmount) }}
              </td>
              <td class="py-3.5 px-4">
                <span 
                  class="px-2 py-0.5 rounded font-medium text-[11px]"
                  :class="order.paymentMethod === 'VietQR' ? 'bg-amber-500/10 text-barber-gold border border-amber-500/30' : 'bg-zinc-800 text-zinc-300'"
                >
                  {{ order.paymentMethod }}
                </span>
              </td>
              <td class="py-3.5 px-4 max-w-[180px]">
                <p v-if="order.note" class="truncate text-amber-100/80 italic" :title="order.note">
                  {{ order.note }}
                </p>
                <span v-else class="text-zinc-600">—</span>
              </td>
              <td class="py-3.5 px-4">
                <span 
                  class="px-2 py-0.5 rounded-full font-bold text-[10px]"
                  :class="order.paymentStatus === 'Completed' ? 'bg-emerald-500/10 text-emerald-400 border border-emerald-500/30' : 'bg-rose-500/10 text-rose-400 border border-rose-500/30'"
                >
                  {{ order.paymentStatus === 'Completed' ? 'Hoàn tất' : 'Đã hủy' }}
                </span>
              </td>
              <td class="py-3.5 px-4 text-right">
                <button class="text-barber-gold hover:text-amber-300 font-semibold text-xs">
                  Xem ➜
                </button>
              </td>
            </tr>
          </tbody>
        </table>
      </div>
    </div>

    <!-- Order Detail Modal -->
    <OrderDetailModal
      :isOpen="isModalOpen"
      :order="selectedOrder"
      @close="isModalOpen = false"
      @cancel-order="handleCancelOrder"
    />

  </div>
</template>

<script setup>
import { ref, computed, onMounted } from 'vue'
import api from '@/services/api'
import OrderDetailModal from '@/components/OrderDetailModal.vue'
import { Calendar, RotateCcw } from 'lucide-vue-next'

const orders = ref([])
const isModalOpen = ref(false)
const selectedOrder = ref(null)
const selectedPreset = ref('today')

const datePresets = [
  { id: 'today', label: 'Hôm nay' },
  { id: 'yesterday', label: 'Hôm qua' },
  { id: 'last7days', label: '7 ngày qua' },
  { id: 'thisMonth', label: 'Tháng này' },
  { id: 'lastMonth', label: 'Tháng trước' },
  { id: 'all', label: 'Tất cả' },
  { id: 'custom', label: 'Tùy chọn ngày' }
]

const filters = ref({
  date: '',
  fromDate: '',
  toDate: '',
  paymentMethod: '',
  status: ''
})

function getISODate(d) {
  return d.toISOString().split('T')[0]
}

function applyPreset(presetId) {
  selectedPreset.value = presetId
  const now = new Date()

  filters.value.date = ''
  filters.value.fromDate = ''
  filters.value.toDate = ''

  if (presetId === 'today') {
    filters.value.date = getISODate(now)
  } else if (presetId === 'yesterday') {
    const yesterday = new Date(now)
    yesterday.setDate(yesterday.getDate() - 1)
    filters.value.date = getISODate(yesterday)
  } else if (presetId === 'last7days') {
    const start = new Date(now)
    start.setDate(start.getDate() - 6)
    filters.value.fromDate = getISODate(start)
    filters.value.toDate = getISODate(now)
  } else if (presetId === 'thisMonth') {
    const start = new Date(now.getFullYear(), now.getMonth(), 1)
    filters.value.fromDate = getISODate(start)
    filters.value.toDate = getISODate(now)
  } else if (presetId === 'lastMonth') {
    const start = new Date(now.getFullYear(), now.getMonth() - 1, 1)
    const end = new Date(now.getFullYear(), now.getMonth(), 0) // Ngày cuối cùng của tháng trước
    filters.value.fromDate = getISODate(start)
    filters.value.toDate = getISODate(end)
  } else if (presetId === 'custom') {
    if (!filters.value.fromDate) {
      filters.value.fromDate = getISODate(now)
      filters.value.toDate = getISODate(now)
    }
  }

  loadOrders()
}

async function loadOrders() {
  try {
    const params = {}
    if (filters.value.date) params.date = filters.value.date
    if (filters.value.fromDate) params.fromDate = filters.value.fromDate
    if (filters.value.toDate) params.toDate = filters.value.toDate
    if (filters.value.paymentMethod) params.paymentMethod = filters.value.paymentMethod
    if (filters.value.status) params.status = filters.value.status

    const res = await api.getOrders(params)
    orders.value = res.data
  } catch (err) {
    console.error('Lỗi khi tải đơn hàng:', err)
  }
}

onMounted(() => {
  applyPreset('today')
})

const totalFilteredRevenue = computed(() => {
  return orders.value
    .filter(o => o.paymentStatus === 'Completed')
    .reduce((sum, o) => sum + o.finalAmount, 0)
})

const totalFilteredDiscount = computed(() => {
  return orders.value
    .filter(o => o.paymentStatus === 'Completed')
    .reduce((sum, o) => sum + o.discountAmount, 0)
})

function resetFilters() {
  filters.value.paymentMethod = ''
  filters.value.status = ''
  applyPreset('today')
}

function openDetail(order) {
  selectedOrder.value = order
  isModalOpen.value = true
}

async function handleCancelOrder(id) {
  if (!confirm('Bạn có chắc chắn muốn hủy đơn hàng này? Tồn kho sản phẩm (nếu có) sẽ được hoàn lại.')) return
  try {
    await api.cancelOrder(id)
    isModalOpen.value = false
    loadOrders()
  } catch (err) {
    alert('Không thể hủy đơn: ' + (err.response?.data || err.message))
  }
}

function formatCurrency(val) {
  return new Intl.NumberFormat('vi-VN', { style: 'currency', currency: 'VND' }).format(val || 0)
}

function formatDateTime(dateStr) {
  if (!dateStr) return ''
  const d = new Date(dateStr)
  return d.toLocaleString('vi-VN', { 
    hour: '2-digit', 
    minute: '2-digit', 
    day: '2-digit', 
    month: '2-digit', 
    year: 'numeric' 
  })
}
</script>
