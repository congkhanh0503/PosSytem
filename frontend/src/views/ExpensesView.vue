<template>
  <div class="p-6 space-y-6 overflow-y-auto h-screen max-w-6xl mx-auto bg-slate-50 text-slate-800">
    <!-- Header & Action Button -->
    <div class="flex flex-col sm:flex-row justify-between sm:items-center gap-4 border-b border-slate-200 pb-4">
      <div>
        <h2 class="text-2xl font-black text-slate-900 tracking-tight">Quản Lý Chi Phí Doanh Nghiệp</h2>
        <p class="text-xs text-slate-500 mt-0.5 font-medium">Theo dõi tiền mặt bằng, điện nước, nhập vật tư và chi phí vận hành hàng ngày</p>
      </div>
      <button
        @click="openModal()"
        class="py-2.5 px-4 rounded-xl bg-indigo-600 hover:bg-indigo-700 text-white font-bold text-xs shadow-md shadow-indigo-500/25 transition flex items-center gap-2 active:scale-95 self-start sm:self-auto"
      >
        <Plus class="w-4 h-4" />
        Thêm Khoản Chi Mới
      </button>
    </div>

    <!-- Quick Stat Overview Cards (3 Cards) -->
    <div class="grid grid-cols-1 sm:grid-cols-3 gap-4">
      <div class="p-4 rounded-2xl bg-white border border-slate-200 shadow-xs relative overflow-hidden">
        <div class="flex items-center justify-between mb-2">
          <span class="text-xs text-slate-500 uppercase font-semibold">Chi Tiêu Hôm Nay</span>
          <div class="w-8 h-8 rounded-lg bg-rose-50 text-rose-600 flex items-center justify-center">
            <ArrowDownRight class="w-4 h-4" />
          </div>
        </div>
        <h3 class="text-xl font-black text-rose-600">{{ formatCurrency(overview.todayTotal) }}</h3>
        <p class="text-[11px] text-slate-500 mt-1 flex items-center gap-2">
          <span>💵 Tiền mặt: <b class="text-slate-800">{{ formatCurrency(overview.todayCashTotal) }}</b></span>
          <span>•</span>
          <span>💳 CK: <b class="text-indigo-600">{{ formatCurrency(overview.todayTransferTotal) }}</b></span>
        </p>
      </div>

      <div class="p-4 rounded-2xl bg-white border border-slate-200 shadow-xs relative overflow-hidden">
        <div class="flex items-center justify-between mb-2">
          <span class="text-xs text-slate-500 uppercase font-semibold">Tổng Chi Tháng Này</span>
          <div class="w-8 h-8 rounded-lg bg-purple-50 text-purple-600 flex items-center justify-center">
            <Calendar class="w-4 h-4" />
          </div>
        </div>
        <h3 class="text-xl font-black text-purple-700">{{ formatCurrency(overview.monthTotal) }}</h3>
        <p class="text-[11px] text-slate-500 mt-1 flex items-center gap-2">
          <span>💵 Tiền mặt: <b class="text-slate-800">{{ formatCurrency(overview.monthCashTotal) }}</b></span>
          <span>•</span>
          <span>💳 CK: <b class="text-indigo-600">{{ formatCurrency(overview.monthTransferTotal) }}</b></span>
        </p>
      </div>

      <div class="p-4 rounded-2xl bg-white border border-slate-200 shadow-xs relative overflow-hidden">
        <div class="flex items-center justify-between mb-2">
          <span class="text-xs text-slate-500 uppercase font-semibold">Khoản Chi Gần Nhất</span>
          <div class="w-8 h-8 rounded-lg bg-indigo-50 text-indigo-600 flex items-center justify-center">
            <Receipt class="w-4 h-4" />
          </div>
        </div>
        <h3 class="text-sm font-bold text-slate-900 truncate">{{ overview.latestExpense?.title || 'Chưa có' }}</h3>
        <p class="text-[11px] text-indigo-600 mt-1 font-bold flex items-center gap-1.5">
          <span>{{ overview.latestExpense ? formatCurrency(overview.latestExpense.amount) : '0 ₫' }}</span>
          <span v-if="overview.latestExpense" class="text-[10px] px-1.5 py-0.5 rounded font-medium bg-slate-100 text-slate-600">
            {{ overview.latestExpense.paymentMethod === 'Transfer' ? '💳 CK' : '💵 Tiền mặt' }}
          </span>
        </p>
      </div>
    </div>

    <!-- BỘ LỌC LỊCH THÔNG MINH (SMART DATE FILTER) -->
    <div class="bg-white border border-slate-200 rounded-2xl p-4 space-y-3.5 shadow-xs">
      <!-- Preset Buttons & Category Select -->
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

        <div class="flex items-center gap-2 text-xs">
          <select
            v-model="filters.paymentMethod"
            @change="loadExpenses"
            class="bg-white border border-slate-200 rounded-xl px-3 py-1.5 text-slate-800 focus:outline-none focus:border-indigo-500 shadow-2xs"
          >
            <option value="">Tất cả hình thức chi</option>
            <option value="Cash">💵 Tiền mặt</option>
            <option value="Transfer">💳 Chuyển khoản</option>
          </select>

          <select
            v-model="filters.category"
            @change="loadExpenses"
            class="bg-white border border-slate-200 rounded-xl px-3 py-1.5 text-slate-800 focus:outline-none focus:border-indigo-500 shadow-2xs"
          >
            <option value="">Tất cả danh mục</option>
            <option v-for="cat in categories" :key="cat" :value="cat">{{ cat }}</option>
          </select>

          <button
            @click="resetFilters"
            class="p-2 rounded-xl bg-white border border-slate-200 hover:bg-slate-50 text-slate-600 transition shadow-2xs"
            title="Xóa bộ lọc"
          >
            <RotateCcw class="w-3.5 h-3.5" />
          </button>
        </div>
      </div>

      <!-- Custom Date Pickers (Khi chọn Tùy chọn) -->
      <div v-if="selectedPreset === 'custom'" class="flex flex-wrap items-center gap-3 pt-3 border-t border-slate-100 text-xs animate-fade-in">
        <span class="text-slate-500 font-semibold">Khoảng ngày chi:</span>
        <div class="flex items-center gap-2">
          <label class="text-slate-400">Từ:</label>
          <input
            v-model="filters.fromDate"
            type="date"
            @change="loadExpenses"
            class="bg-white border border-slate-200 rounded-xl px-3 py-1.5 text-slate-800 focus:outline-none focus:border-indigo-500 shadow-2xs"
          />
        </div>
        <div class="flex items-center gap-2">
          <label class="text-slate-400">Đến:</label>
          <input
            v-model="filters.toDate"
            type="date"
            @change="loadExpenses"
            class="bg-white border border-slate-200 rounded-xl px-3 py-1.5 text-slate-800 focus:outline-none focus:border-indigo-500 shadow-2xs"
          />
        </div>
      </div>
    </div>

    <!-- Expenses Table -->
    <div class="bg-white border border-slate-200 rounded-2xl overflow-hidden shadow-xs">
      <div class="overflow-x-auto">
        <table class="w-full text-left text-xs">
          <thead class="bg-slate-50 text-slate-500 uppercase tracking-wider font-bold text-[11px] border-b border-slate-200">
            <tr>
              <th class="py-3.5 px-4">Tên Khoản Chi</th>
              <th class="py-3.5 px-4">Phân Loại</th>
              <th class="py-3.5 px-4">Hình Thức</th>
              <th class="py-3.5 px-4">Số Tiền (VNĐ)</th>
              <th class="py-3.5 px-4">Ngày Chi</th>
              <th class="py-3.5 px-4">Ghi Chú</th>
              <th class="py-3.5 px-4 text-right">Thao Tác</th>
            </tr>
          </thead>
          <tbody class="divide-y divide-slate-100 font-medium">
            <tr v-if="expenses.length === 0">
              <td colspan="7" class="py-12 text-center text-slate-400">
                Không tìm thấy khoản chi tiêu nào
              </td>
            </tr>

            <tr v-for="exp in expenses" :key="exp.id" class="hover:bg-slate-50/80 transition">
              <td class="py-3.5 px-4">
                <p class="font-bold text-slate-900 text-sm">{{ exp.title }}</p>
              </td>
              <td class="py-3.5 px-4">
                <span 
                  class="px-2 py-0.5 rounded font-bold text-[11px]"
                  :class="getCategoryBadgeClass(exp.category)"
                >
                  {{ exp.category }}
                </span>
              </td>
              <td class="py-3.5 px-4">
                <span 
                  class="px-2 py-0.5 rounded-full font-bold text-[10px] inline-flex items-center gap-1"
                  :class="exp.paymentMethod === 'Transfer' ? 'bg-indigo-50 text-indigo-700 border border-indigo-200' : 'bg-emerald-50 text-emerald-700 border border-emerald-200'"
                >
                  <span>{{ exp.paymentMethod === 'Transfer' ? '💳 Chuyển khoản' : '💵 Tiền mặt' }}</span>
                </span>
              </td>
              <td class="py-3.5 px-4 font-black text-rose-600 text-sm">
                -{{ formatCurrency(exp.amount) }}
              </td>
              <td class="py-3.5 px-4 text-slate-600">
                {{ formatDate(exp.date) }}
              </td>
              <td class="py-3.5 px-4 max-w-[220px]">
                <span v-if="exp.note" class="text-slate-500 truncate block" :title="exp.note">
                  {{ exp.note }}
                </span>
                <span v-else class="text-slate-300">—</span>
              </td>
              <td class="py-3.5 px-4 text-right space-x-2">
                <button 
                  @click="openModal(exp)"
                  class="p-1.5 rounded-lg bg-slate-100 hover:bg-indigo-50 text-slate-600 hover:text-indigo-600 transition"
                  title="Sửa"
                >
                  <Edit3 class="w-4 h-4" />
                </button>
                <button 
                  @click="deleteExpense(exp.id)"
                  class="p-1.5 rounded-lg bg-rose-50 hover:bg-rose-100 text-rose-600 transition"
                  title="Xóa"
                >
                  <Trash2 class="w-4 h-4" />
                </button>
              </td>
            </tr>
          </tbody>
        </table>
      </div>
    </div>

    <!-- Modal Form (Thêm/Sửa Khoản Chi) -->
    <div v-if="isModalOpen" class="fixed inset-0 z-50 flex items-center justify-center p-4 bg-slate-900/40 backdrop-blur-sm">
      <div class="relative w-full max-w-md bg-white border border-slate-200 rounded-2xl shadow-2xl p-6">
        <h3 class="text-lg font-bold text-slate-900 mb-4">
          {{ editingId ? 'Chỉnh Sửa Khoản Chi' : 'Thêm Khoản Chi Mới' }}
        </h3>

        <form @submit.prevent="saveExpense" class="space-y-4 text-xs">
          <div>
            <label class="block text-slate-600 mb-1 font-medium">Tên khoản chi *</label>
            <input
              v-model="form.title"
              type="text"
              required
              placeholder="VD: Mua vật tư văn phòng"
              class="w-full bg-white border border-slate-200 rounded-xl px-3 py-2 text-slate-800 focus:outline-none focus:border-indigo-500 shadow-2xs"
            />
          </div>

          <div>
            <div class="flex justify-between items-center mb-1">
              <label class="block text-slate-600 font-medium">Số tiền chi (VNĐ) *</label>
              <span v-if="form.amount > 0" class="text-xs font-bold text-indigo-600">
                👉 {{ formatCurrency(form.amount) }}
              </span>
            </div>
            <input
              v-model.number="form.amount"
              type="number"
              min="0"
              step="any"
              required
              placeholder="VD: 50000"
              class="w-full bg-white border border-slate-200 rounded-xl px-3 py-2 text-slate-900 font-bold focus:outline-none focus:border-indigo-500 shadow-2xs"
            />
            
            <!-- Nút chọn nhanh số tiền -->
            <div class="flex items-center gap-1.5 mt-2 overflow-x-auto no-scrollbar pb-0.5">
              <button
                v-for="presetAmt in [20000, 50000, 100000, 200000, 500000]"
                :key="presetAmt"
                type="button"
                @click="form.amount = presetAmt"
                class="px-2 py-1 rounded-lg text-[10px] font-bold transition border"
                :class="form.amount === presetAmt ? 'bg-indigo-600 text-white border-indigo-600' : 'bg-slate-50 text-slate-600 border-slate-200 hover:bg-slate-100'"
              >
                {{ formatCurrency(presetAmt) }}
              </button>
            </div>
          </div>

          <div>
            <label class="block text-slate-600 mb-1 font-medium">Ngày chi *</label>
            <input
              v-model="form.date"
              type="date"
              required
              class="w-full bg-white border border-slate-200 rounded-xl px-3 py-2 text-slate-800 focus:outline-none focus:border-indigo-500 shadow-2xs"
            />
          </div>

          <div>
            <label class="block text-slate-600 mb-1 font-medium">Hình thức chi tiền *</label>
            <div class="grid grid-cols-2 gap-2">
              <button
                type="button"
                @click="form.paymentMethod = 'Cash'"
                class="py-2 px-3 rounded-xl border text-xs font-bold transition flex items-center justify-center gap-1.5 cursor-pointer"
                :class="form.paymentMethod !== 'Transfer' ? 'bg-emerald-50 border-emerald-500 text-emerald-700 shadow-2xs ring-1 ring-emerald-400' : 'bg-slate-50 border-slate-200 text-slate-600 hover:bg-slate-100'"
              >
                <span>💵 Tiền mặt</span>
              </button>
              <button
                type="button"
                @click="form.paymentMethod = 'Transfer'"
                class="py-2 px-3 rounded-xl border text-xs font-bold transition flex items-center justify-center gap-1.5 cursor-pointer"
                :class="form.paymentMethod === 'Transfer' ? 'bg-indigo-50 border-indigo-500 text-indigo-700 shadow-2xs ring-1 ring-indigo-400' : 'bg-slate-50 border-slate-200 text-slate-600 hover:bg-slate-100'"
              >
                <span>💳 Chuyển khoản</span>
              </button>
            </div>
          </div>

          <div>
            <label class="block text-slate-600 mb-1 font-medium">Phân loại danh mục</label>
            <select
              v-model="form.category"
              class="w-full bg-white border border-slate-200 rounded-xl px-3 py-2 text-slate-800 focus:outline-none focus:border-indigo-500 shadow-2xs"
            >
              <option v-for="cat in categories" :key="cat" :value="cat">{{ cat }}</option>
            </select>
          </div>

          <div>
            <label class="block text-slate-600 mb-1 font-medium">Ghi chú</label>
            <textarea
              v-model="form.note"
              rows="2"
              placeholder="Chi tiết nơi mua, lý do chi tiêu..."
              class="w-full bg-white border border-slate-200 rounded-xl px-3 py-2 text-slate-800 focus:outline-none focus:border-indigo-500 shadow-2xs"
            ></textarea>
          </div>

          <div class="flex gap-3 pt-3">
            <button
              type="button"
              @click="isModalOpen = false"
              class="flex-1 py-2.5 rounded-xl bg-slate-100 hover:bg-slate-200 text-slate-700 font-semibold transition"
            >
              Hủy
            </button>
            <button
              type="submit"
              class="flex-1 py-2.5 rounded-xl bg-indigo-600 hover:bg-indigo-700 text-white font-bold transition shadow-sm"
            >
              Lưu Khoản Chi
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
import { Plus, Edit3, Trash2, ArrowDownRight, Calendar, Receipt, RotateCcw } from 'lucide-vue-next'

const { toast, confirm } = useNotify()

const expenses = ref([])
const categories = ref([
  'Mặt bằng & Tiện ích',
  'Vật tư & Hàng hóa',
  'Thiết bị & Công cụ',
  'Sinh hoạt & Tiếp khách',
  'Marketing & Quảng bá',
  'Khác'
])

const isModalOpen = ref(false)
const editingId = ref(null)
const selectedPreset = ref('today')

const form = ref({
  title: '',
  amount: 50000,
  paymentMethod: 'Cash',
  category: 'Vật tư & Hàng hóa',
  date: new Date().toISOString().split('T')[0],
  note: ''
})

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
  category: '',
  paymentMethod: ''
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

  loadExpenses()
}

async function loadExpenses() {
  try {
    const params = {}
    if (filters.value.date) params.date = filters.value.date
    if (filters.value.fromDate) params.fromDate = filters.value.fromDate
    if (filters.value.toDate) params.toDate = filters.value.toDate
    if (filters.value.category) params.category = filters.value.category
    if (filters.value.paymentMethod) params.paymentMethod = filters.value.paymentMethod

    const res = await api.getExpenses(params)
    expenses.value = res.data
  } catch (err) {
    console.error('Lỗi khi tải chi tiêu:', err)
  }
}

async function loadCategories() {
  try {
    const res = await api.getExpenseCategories()
    if (res.data?.length) categories.value = res.data
  } catch (err) {
    console.error('Lỗi tải danh mục:', err)
  }
}

const overview = ref({
  todayTotal: 0,
  todayCashTotal: 0,
  todayTransferTotal: 0,
  monthTotal: 0,
  monthCashTotal: 0,
  monthTransferTotal: 0,
  latestExpense: null
})

async function loadSummary() {
  try {
    const res = await api.getExpenseSummary()
    if (res?.data) {
      overview.value = {
        todayTotal: res.data.todayTotal || 0,
        todayCashTotal: res.data.todayCashTotal || 0,
        todayTransferTotal: res.data.todayTransferTotal || 0,
        monthTotal: res.data.monthTotal || 0,
        monthCashTotal: res.data.monthCashTotal || 0,
        monthTransferTotal: res.data.monthTransferTotal || 0,
        latestExpense: res.data.latestExpense || null
      }
    }
  } catch (err) {
    console.error('Lỗi khi tải tổng quan chi tiêu:', err)
  }
}

onMounted(() => {
  applyPreset('today')
  loadCategories()
  loadSummary()
})

function formatCurrency(val) {
  return new Intl.NumberFormat('vi-VN', { style: 'currency', currency: 'VND' }).format(val || 0)
}

function formatDate(dateStr) {
  if (!dateStr) return ''
  const d = new Date(dateStr)
  return d.toLocaleDateString('vi-VN', { day: '2-digit', month: '2-digit', year: 'numeric' })
}

function getCategoryBadgeClass(cat) {
  switch (cat) {
    case 'Mặt bằng & Tiện ích': return 'bg-amber-50 text-amber-700 border border-amber-200'
    case 'Vật tư & Hàng hóa': return 'bg-indigo-50 text-indigo-700 border border-indigo-200'
    case 'Thiết bị & Công cụ': return 'bg-blue-50 text-blue-700 border border-blue-200'
    case 'Sinh hoạt & Tiếp khách': return 'bg-emerald-50 text-emerald-700 border border-emerald-200'
    case 'Marketing & Quảng bá': return 'bg-purple-50 text-purple-700 border border-purple-200'
    default: return 'bg-slate-100 text-slate-700 border border-slate-200'
  }
}

function resetFilters() {
  filters.value.category = ''
  filters.value.paymentMethod = ''
  applyPreset('today')
}

function openModal(exp = null) {
  if (exp) {
    editingId.value = exp.id
    form.value = {
      title: exp.title,
      amount: exp.amount,
      paymentMethod: exp.paymentMethod || 'Cash',
      category: exp.category,
      date: exp.date.split('T')[0],
      note: exp.note || ''
    }
  } else {
    editingId.value = null
    form.value = {
      title: '',
      amount: 50000,
      paymentMethod: 'Cash',
      category: categories.value[0] || 'Vật tư & Hàng hóa',
      date: getISODate(new Date()),
      note: ''
    }
  }
  isModalOpen.value = true
}

async function saveExpense() {
  try {
    if (editingId.value) {
      await api.updateExpense(editingId.value, { ...form.value, id: editingId.value })
      toast.success('Cập nhật khoản chi thành công!')
    } else {
      await api.createExpense(form.value)
      toast.success('Thêm khoản chi mới thành công!')
    }
    isModalOpen.value = false
    loadExpenses()
    loadSummary()
  } catch (err) {
    toast.error('Lỗi lưu khoản chi: ' + err.message)
  }
}

async function deleteExpense(id) {
  const ok = await confirm({
    title: 'Xóa khoản chi?',
    message: 'Bạn có chắc chắn muốn xóa khoản chi này không?',
    type: 'danger',
    confirmText: 'Xóa ngay',
    cancelText: 'Hủy'
  })
  if (!ok) return

  try {
    await api.deleteExpense(id)
    toast.success('Đã xóa khoản chi thành công!')
    loadExpenses()
    loadSummary()
  } catch (err) {
    toast.error('Không thể xóa: ' + err.message)
  }
}
</script>
