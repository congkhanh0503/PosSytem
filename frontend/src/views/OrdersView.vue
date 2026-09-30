<template>
  <div class="p-6 space-y-6 overflow-y-auto h-screen max-w-7xl mx-auto bg-slate-50 text-slate-800">
    <!-- Header -->
    <div class="border-b border-slate-200 pb-4">
      <h2 class="text-2xl font-black text-slate-900 tracking-tight">Lịch Sử Đơn Hàng Điện Tử</h2>
      <p class="text-xs text-slate-500 mt-0.5 font-medium">Quản lý giao dịch, sửa thông tin, hủy đơn và đối soát doanh thu theo khoảng thời gian (Hệ thống lưu vết minh bạch chống gian lận)</p>
    </div>

    <!-- BỘ LỌC LỊCH THÔNG MINH (SMART DATE FILTER) -->
    <div class="bg-white border border-slate-200 rounded-2xl p-4 space-y-4 shadow-xs">
      
      <!-- Hàng 1: Các nút chọn nhanh khoảng thời gian -->
      <div class="flex flex-wrap items-center justify-between gap-3">
        <div class="flex items-center gap-1.5 overflow-x-auto no-scrollbar py-0.5 text-xs">
          <button
            v-for="preset in datePresets"
            :key="preset.id"
            @click="applyPreset(preset.id)"
            class="px-3.5 py-1.5 rounded-xl font-bold transition-all duration-200 whitespace-nowrap flex items-center gap-1.5"
            :class="selectedPreset === preset.id ? 'bg-indigo-600 text-white shadow-2xs' : 'bg-slate-100 text-slate-600 hover:text-slate-900 hover:bg-slate-200/80'"
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
            class="bg-white border border-slate-200 rounded-xl px-3 py-1.5 text-slate-800 focus:outline-none focus:border-indigo-500 shadow-2xs"
          >
            <option value="">Tất cả hình thức</option>
            <option value="VietQR">VietQR</option>
            <option value="Cash">Tiền mặt</option>
          </select>

          <select
            v-model="filters.status"
            @change="loadOrders"
            class="bg-white border border-slate-200 rounded-xl px-3 py-1.5 text-slate-800 focus:outline-none focus:border-indigo-500 shadow-2xs"
          >
            <option value="">Tất cả trạng thái</option>
            <option value="Completed">Đã thanh toán</option>
            <option value="Cancelled">Đã hủy</option>
          </select>

          <button
            @click="resetFilters"
            class="p-2 rounded-xl bg-white border border-slate-200 hover:bg-slate-50 text-slate-600 transition shadow-2xs"
            title="Làm mới bộ lọc"
          >
            <RotateCcw class="w-3.5 h-3.5" />
          </button>
        </div>
      </div>

      <!-- Hàng 2: Input Từ Ngày - Đến Ngày (Khi chọn Tùy chọn) -->
      <div v-if="selectedPreset === 'custom'" class="flex flex-wrap items-center gap-3 pt-3 border-t border-slate-100 text-xs animate-fade-in">
        <span class="text-slate-500 font-semibold">Khoảng ngày:</span>
        <div class="flex items-center gap-2">
          <label class="text-slate-400">Từ:</label>
          <input
            v-model="filters.fromDate"
            type="date"
            @change="loadOrders"
            class="bg-white border border-slate-200 rounded-xl px-3 py-1.5 text-slate-800 focus:outline-none focus:border-indigo-500 shadow-2xs"
          />
        </div>
        <div class="flex items-center gap-2">
          <label class="text-slate-400">Đến:</label>
          <input
            v-model="filters.toDate"
            type="date"
            @change="loadOrders"
            class="bg-white border border-slate-200 rounded-xl px-3 py-1.5 text-slate-800 focus:outline-none focus:border-indigo-500 shadow-2xs"
          />
        </div>
      </div>

      <!-- Hàng 3: Thống kê nhanh của kết quả lọc (Metrics Summary) -->
      <div class="pt-3 border-t border-slate-100 grid grid-cols-1 sm:grid-cols-3 gap-3 text-xs">
        <div class="p-2.5 rounded-xl bg-slate-50 border border-slate-200/80 flex justify-between items-center">
          <span class="text-slate-500 font-medium">Số lượng đơn:</span>
          <span class="font-extrabold text-slate-900 text-sm">
            {{ orders.length }} đơn 
            <span class="text-[11px] font-normal text-slate-500">
              ({{ completedOrdersCount }} hoàn tất • {{ cancelledOrdersCount }} đã hủy)
            </span>
          </span>
        </div>
        <div class="p-2.5 rounded-xl bg-slate-50 border border-slate-200/80 flex justify-between items-center">
          <span class="text-slate-500 font-medium">Tổng tiền đã giảm giá:</span>
          <span class="font-bold text-emerald-600">{{ formatCurrency(totalFilteredDiscount) }}</span>
        </div>
        <div class="p-2.5 rounded-xl bg-slate-50 border border-slate-200/80 flex justify-between items-center">
          <span class="text-slate-500 font-medium">Tổng doanh thu thực nhận:</span>
          <span class="font-black text-indigo-600 text-base">{{ formatCurrency(totalFilteredRevenue) }}</span>
        </div>
      </div>

    </div>

    <!-- Orders Table -->
    <div class="bg-white border border-slate-200 rounded-2xl overflow-hidden shadow-xs">
      <div class="overflow-x-auto">
        <table class="w-full text-left text-xs">
          <thead class="bg-slate-50 text-slate-500 uppercase tracking-wider font-bold text-[11px] border-b border-slate-200">
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
              <th class="py-3.5 px-4 text-right">Thao Tác</th>
            </tr>
          </thead>
          <tbody class="divide-y divide-slate-100 font-medium">
            <tr v-if="orders.length === 0">
              <td colspan="10" class="py-12 text-center text-slate-400">
                Không tìm thấy đơn hàng nào trong khoảng thời gian này
              </td>
            </tr>

            <tr 
              v-for="order in orders" 
              :key="order.id" 
              class="transition"
              :class="order.paymentStatus === 'Cancelled' ? 'bg-rose-50/30 hover:bg-rose-50/50' : 'hover:bg-slate-50/80'"
            >
              <td class="py-3.5 px-4 cursor-pointer" @click="openDetail(order)">
                <span 
                  class="font-mono font-bold hover:underline"
                  :class="order.paymentStatus === 'Cancelled' ? 'text-slate-400 line-through' : 'text-indigo-600'"
                >
                  {{ order.orderCode }}
                </span>
              </td>
              <td class="py-3.5 px-4 text-slate-500 cursor-pointer" @click="openDetail(order)">
                {{ formatDateTime(order.createdAt) }}
              </td>
              <td class="py-3.5 px-4 font-bold text-slate-900 cursor-pointer" @click="openDetail(order)">
                {{ order.customerName || 'Khách vãng lai' }}
              </td>
              <td class="py-3.5 px-4 text-slate-600 cursor-pointer" @click="openDetail(order)">
                <span :class="order.paymentStatus === 'Cancelled' ? 'line-through text-slate-400' : ''">
                  {{ formatCurrency(order.subTotal) }}
                </span>
              </td>
              <td class="py-3.5 px-4 cursor-pointer" @click="openDetail(order)">
                <span v-if="order.discountPercent > 0" class="text-emerald-600 font-bold">
                  -{{ formatCurrency(order.discountAmount) }} ({{ order.discountPercent }}%)
                </span>
                <span v-else class="text-slate-400">0%</span>
              </td>
              <td class="py-3.5 px-4 font-black text-sm cursor-pointer" @click="openDetail(order)">
                <span :class="order.paymentStatus === 'Cancelled' ? 'text-slate-400 line-through' : 'text-indigo-600'">
                  {{ formatCurrency(order.finalAmount) }}
                </span>
              </td>
              <td class="py-3.5 px-4 cursor-pointer" @click="openDetail(order)">
                <span 
                  class="px-2 py-0.5 rounded font-semibold text-[11px]"
                  :class="order.paymentMethod === 'VietQR' ? 'bg-blue-50 text-blue-600 border border-blue-200' : 'bg-slate-100 text-slate-700'"
                >
                  {{ order.paymentMethod }}
                </span>
              </td>
              <td class="py-3.5 px-4 max-w-[180px] cursor-pointer" @click="openDetail(order)">
                <p v-if="order.note" class="truncate text-slate-600 italic" :title="order.note">
                  {{ order.note }}
                </p>
                <span v-else class="text-slate-300">—</span>
              </td>
              <td class="py-3.5 px-4 cursor-pointer" @click="openDetail(order)">
                <span 
                  class="px-2 py-0.5 rounded-full font-bold text-[10px]"
                  :class="order.paymentStatus === 'Completed' ? 'bg-emerald-50 text-emerald-600 border border-emerald-200' : 'bg-rose-50 text-rose-600 border border-rose-200'"
                >
                  {{ order.paymentStatus === 'Completed' ? 'Hoàn tất' : 'Đã hủy' }}
                </span>
              </td>
              <td class="py-3.5 px-4 text-right space-x-1.5 whitespace-nowrap">
                <button 
                  @click.stop="openDetail(order)"
                  class="p-1.5 rounded-lg bg-slate-100 hover:bg-slate-200 text-slate-600 hover:text-slate-900 transition"
                  title="Xem chi tiết"
                >
                  <Eye class="w-3.5 h-3.5" />
                </button>
                <button 
                  v-if="order.paymentStatus === 'Completed'"
                  @click.stop="openEditModal(order)"
                  class="p-1.5 rounded-lg bg-indigo-50 hover:bg-indigo-100 text-indigo-600 transition"
                  title="Chỉnh sửa đơn"
                >
                  <Edit3 class="w-3.5 h-3.5" />
                </button>
                <button 
                  v-if="order.paymentStatus === 'Completed'"
                  @click.stop="handleCancelOrder(order)"
                  class="p-1.5 rounded-lg bg-rose-50 hover:bg-rose-100 text-rose-600 transition"
                  title="Hủy đơn hàng (Hoàn tồn kho, không xóa vĩnh viễn)"
                >
                  <Ban class="w-3.5 h-3.5" />
                </button>
                <span 
                  v-else
                  class="inline-block p-1.5 text-rose-300 cursor-not-allowed"
                  title="Đơn hàng đã ở trạng thái hủy"
                >
                  <XCircle class="w-3.5 h-3.5" />
                </span>
              </td>
            </tr>
          </tbody>
        </table>
      </div>
    </div>

    <!-- Modal Chi Tiết Đơn Hàng -->
    <OrderDetailModal
      :is-open="isModalOpen"
      :order="selectedOrder"
      @close="isModalOpen = false"
      @cancel-order="handleCancelOrder"
      @edit-order="openEditModalFromDetail"
    />

    <!-- Modal Chỉnh Sửa Đơn Hàng -->
    <div v-if="isEditModalOpen" class="fixed inset-0 z-50 flex items-center justify-center p-4 bg-slate-900/40 backdrop-blur-sm">
      <div class="relative w-full max-w-md bg-white border border-slate-200 rounded-2xl shadow-2xl p-6">
        <div class="flex justify-between items-center border-b border-slate-100 pb-3 mb-4">
          <div>
            <h3 class="text-lg font-bold text-slate-900 flex items-center gap-2">
              <Edit3 class="w-5 h-5 text-indigo-600" />
              Sửa Đơn: {{ editingOrder?.orderCode }}
            </h3>
            <p class="text-xs text-slate-500 mt-0.5">Tiền gốc tạm tính: {{ formatCurrency(editingOrder?.subTotal) }}</p>
          </div>
          <button @click="isEditModalOpen = false" class="text-slate-400 hover:text-slate-600 p-1">
            ✕
          </button>
        </div>

        <form @submit.prevent="saveEditOrder" class="space-y-4 text-xs">
          <div>
            <label class="block text-slate-600 mb-1 font-medium">Tên khách hàng</label>
            <input
              v-model="editForm.customerName"
              type="text"
              placeholder="VD: Anh Nam"
              class="w-full bg-white border border-slate-200 rounded-xl px-3 py-2 text-slate-800 focus:outline-none focus:border-indigo-500 shadow-2xs"
            />
          </div>

          <div>
            <label class="block text-slate-600 mb-1 font-medium">Số điện thoại</label>
            <input
              v-model="editForm.customerPhone"
              type="text"
              placeholder="VD: 0912345678"
              class="w-full bg-white border border-slate-200 rounded-xl px-3 py-2 text-slate-800 focus:outline-none focus:border-indigo-500 shadow-2xs"
            />
          </div>

          <div class="grid grid-cols-2 gap-3">
            <div>
              <label class="block text-slate-600 mb-1 font-medium">Hình thức thanh toán</label>
              <select
                v-model="editForm.paymentMethod"
                class="w-full bg-white border border-slate-200 rounded-xl px-3 py-2 text-slate-800 focus:outline-none focus:border-indigo-500 shadow-2xs"
              >
                <option value="VietQR">VietQR</option>
                <option value="Cash">Tiền mặt</option>
              </select>
            </div>

            <div>
              <label class="block text-slate-600 mb-1 font-medium">Trạng thái</label>
              <select
                v-model="editForm.paymentStatus"
                class="w-full bg-white border border-slate-200 rounded-xl px-3 py-2 text-slate-800 focus:outline-none focus:border-indigo-500 shadow-2xs"
              >
                <option value="Completed">Đã thanh toán</option>
                <option value="Cancelled">Đã hủy</option>
              </select>
            </div>
          </div>

          <div>
            <div class="flex justify-between items-center mb-1">
              <label class="block text-slate-600 font-medium">Giảm giá (%)</label>
              <span class="text-emerald-600 font-bold">
                Thực thu mới: {{ formatCurrency(calculatedFinalAmount) }}
              </span>
            </div>
            <input
              v-model.number="editForm.discountPercent"
              type="number"
              min="0"
              max="100"
              class="w-full bg-white border border-slate-200 rounded-xl px-3 py-2 text-slate-900 font-bold focus:outline-none focus:border-indigo-500 shadow-2xs"
            />
          </div>

          <div>
            <label class="block text-slate-600 mb-1 font-medium">Ghi chú</label>
            <textarea
              v-model="editForm.note"
              rows="2"
              placeholder="Ghi chú đơn hàng, lý do sửa..."
              class="w-full bg-white border border-slate-200 rounded-xl px-3 py-2 text-slate-800 focus:outline-none focus:border-indigo-500 shadow-2xs"
            ></textarea>
          </div>

          <div class="flex gap-3 pt-3">
            <button
              type="button"
              @click="isEditModalOpen = false"
              class="flex-1 py-2.5 rounded-xl bg-slate-100 hover:bg-slate-200 text-slate-700 font-semibold transition"
            >
              Hủy
            </button>
            <button
              type="submit"
              class="flex-1 py-2.5 rounded-xl bg-indigo-600 hover:bg-indigo-700 text-white font-bold transition shadow-sm"
            >
              Lưu Cập Nhật
            </button>
          </div>
        </form>
      </div>
    </div>

  </div>
</template>

<script setup>
import { ref, computed, onMounted } from 'vue'
import api from '@/services/api'
import { useNotify } from '@/composables/useNotify'
import OrderDetailModal from '@/components/OrderDetailModal.vue'
import { Calendar, RotateCcw, Edit3, Ban, XCircle, Eye } from 'lucide-vue-next'

const { toast, confirm } = useNotify()

const orders = ref([])
const selectedOrder = ref(null)
const isModalOpen = ref(false)

const isEditModalOpen = ref(null)
const editingOrder = ref(null)
const editForm = ref({
  customerName: '',
  customerPhone: '',
  paymentMethod: 'VietQR',
  paymentStatus: 'Completed',
  discountPercent: 0,
  note: ''
})

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
    const end = new Date(now.getFullYear(), now.getMonth(), 0)
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

const completedOrdersCount = computed(() => {
  return orders.value.filter(o => o.paymentStatus === 'Completed').length
})

const cancelledOrdersCount = computed(() => {
  return orders.value.filter(o => o.paymentStatus === 'Cancelled').length
})

const calculatedFinalAmount = computed(() => {
  if (!editingOrder.value) return 0
  const subTotal = editingOrder.value.subTotal || 0
  const pct = Math.min(100, Math.max(0, Number(editForm.value.discountPercent) || 0))
  const discountAmount = Math.round(subTotal * (pct / 100))
  return Math.max(0, subTotal - discountAmount)
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

function openEditModal(order) {
  editingOrder.value = order
  editForm.value = {
    customerName: order.customerName || '',
    customerPhone: order.customerPhone || '',
    paymentMethod: order.paymentMethod || 'VietQR',
    paymentStatus: order.paymentStatus || 'Completed',
    discountPercent: order.discountPercent || 0,
    note: order.note || ''
  }
  isEditModalOpen.value = true
}

function openEditModalFromDetail(order) {
  isModalOpen.value = false
  openEditModal(order)
}

async function saveEditOrder() {
  if (!editingOrder.value) return
  try {
    await api.updateOrder(editingOrder.value.id, editForm.value)
    isEditModalOpen.value = false
    toast.success('Cập nhật đơn hàng thành công!')
    loadOrders()
  } catch (err) {
    toast.error('Không thể lưu cập nhật đơn hàng: ' + (err.response?.data || err.message))
  }
}

async function handleCancelOrder(orderOrId) {
  const targetId = typeof orderOrId === 'object' ? orderOrId.id : orderOrId
  const currentOrder = typeof orderOrId === 'object' ? orderOrId : orders.value.find(o => o.id === targetId)
  const code = currentOrder ? currentOrder.orderCode : ''

  const confirmed = await confirm({
    title: `Xác nhận hủy đơn hàng [${code}]?`,
    message: `• Đơn hàng sẽ chuyển sang trạng thái "ĐÃ HỦY" để kiểm toán chống gian lận/thất thoát.\n• Doanh thu của đơn sẽ tự động bị loại khỏi báo cáo sổ sách.\n• Tồn kho của các sản phẩm trong đơn sẽ được hoàn trả lại ngay.`,
    type: 'danger',
    confirmText: 'Xác nhận hủy đơn',
    cancelText: 'Giữ lại đơn'
  })
  if (!confirmed) return

  const reason = prompt('Vui lòng nhập lý do hủy đơn hàng (VD: Khách đổi ý, Nhập sai số tiền, ...):', 'Khách đổi ý')
  if (reason === null) return // Bấm hủy bỏ prompt

  try {
    await api.cancelOrder(targetId, { reason: reason.trim() || 'Hủy đơn hàng' })
    isModalOpen.value = false
    await loadOrders()
    toast.success(`Đã hủy thành công đơn hàng [${code}]. Đơn hàng đã chuyển sang trạng thái ĐÃ HỦY.`)
  } catch (err) {
    toast.error('Không thể hủy đơn: ' + (err.response?.data || err.message))
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
    second: '2-digit',
    day: '2-digit', 
    month: '2-digit', 
    year: 'numeric' 
  })
}
</script>
