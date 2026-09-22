<template>
  <div class="p-6 space-y-6 overflow-y-auto h-screen max-w-6xl mx-auto">
    <!-- Header & Action Button -->
    <div class="flex flex-col sm:flex-row justify-between sm:items-center gap-4 border-b border-barber-border pb-4">
      <div>
        <h2 class="text-2xl font-extrabold text-white">Quản Lý Chi Tiêu Tiệm</h2>
        <p class="text-xs text-zinc-400 mt-0.5">Theo dõi tiền mặt bằng, điện nước, nhập phụ liệu, dao cạo và chi phí vận hành</p>
      </div>
      <button
        @click="openModal()"
        class="py-2.5 px-4 rounded-xl bg-gradient-to-r from-amber-500 to-amber-600 hover:from-amber-400 hover:to-amber-500 text-black font-extrabold text-xs shadow-lg shadow-amber-500/20 transition flex items-center gap-2 active:scale-95 self-start sm:self-auto"
      >
        <Plus class="w-4 h-4" />
        Thêm Khoản Chi Mới
      </button>
    </div>

    <!-- Quick Stat Overview Cards (3 Cards) -->
    <div class="grid grid-cols-1 sm:grid-cols-3 gap-4">
      <div class="p-4 rounded-2xl bg-barber-card border border-barber-border relative overflow-hidden">
        <div class="flex items-center justify-between mb-2">
          <span class="text-xs text-zinc-400 uppercase font-medium">Chi Tiêu Hôm Nay</span>
          <div class="w-8 h-8 rounded-lg bg-rose-500/10 text-rose-400 flex items-center justify-center">
            <ArrowDownRight class="w-4 h-4" />
          </div>
        </div>
        <h3 class="text-xl font-extrabold text-rose-400">{{ formatCurrency(todayTotal) }}</h3>
        <p class="text-[11px] text-zinc-500 mt-1">Các khoản chi phát sinh trong ngày</p>
      </div>

      <div class="p-4 rounded-2xl bg-barber-card border border-barber-border relative overflow-hidden">
        <div class="flex items-center justify-between mb-2">
          <span class="text-xs text-zinc-400 uppercase font-medium">Tổng Chi Tháng Này</span>
          <div class="w-8 h-8 rounded-lg bg-purple-500/10 text-purple-400 flex items-center justify-center">
            <Calendar class="w-4 h-4" />
          </div>
        </div>
        <h3 class="text-xl font-extrabold text-purple-300">{{ formatCurrency(monthTotal) }}</h3>
        <p class="text-[11px] text-zinc-500 mt-1">Bao gồm điện nước, mặt bằng, phụ liệu</p>
      </div>

      <div class="p-4 rounded-2xl bg-barber-card border border-barber-border relative overflow-hidden">
        <div class="flex items-center justify-between mb-2">
          <span class="text-xs text-zinc-400 uppercase font-medium">Khoản Chi Gần Nhất</span>
          <div class="w-8 h-8 rounded-lg bg-amber-500/10 text-barber-gold flex items-center justify-center">
            <Receipt class="w-4 h-4" />
          </div>
        </div>
        <h3 class="text-sm font-bold text-white truncate">{{ latestExpense?.title || 'Chưa có' }}</h3>
        <p class="text-[11px] text-amber-200 mt-1 font-semibold">
          {{ latestExpense ? formatCurrency(latestExpense.amount) : '0 ₫' }}
        </p>
      </div>
    </div>

    <!-- BỘ LỌC LỊCH THÔNG MINH (SMART DATE FILTER) -->
    <div class="bg-barber-card border border-barber-border rounded-2xl p-4 space-y-3.5 shadow-xl">
      <!-- Preset Buttons & Category Select -->
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

        <div class="flex items-center gap-2 text-xs">
          <select
            v-model="filters.category"
            @change="loadExpenses"
            class="bg-barber-dark border border-zinc-700 rounded-xl px-3 py-1.5 text-white focus:outline-none focus:border-barber-gold"
          >
            <option value="">Tất cả danh mục</option>
            <option v-for="cat in categories" :key="cat" :value="cat">{{ cat }}</option>
          </select>

          <button
            @click="resetFilters"
            class="p-2 rounded-xl bg-zinc-800 hover:bg-zinc-700 text-zinc-300 transition"
            title="Xóa bộ lọc"
          >
            <RotateCcw class="w-3.5 h-3.5" />
          </button>
        </div>
      </div>

      <!-- Custom Date Pickers (Khi chọn Tùy chọn) -->
      <div v-if="selectedPreset === 'custom'" class="flex flex-wrap items-center gap-3 pt-3 border-t border-zinc-800 text-xs animate-fade-in">
        <span class="text-zinc-400 font-medium">Khoảng ngày chi:</span>
        <div class="flex items-center gap-2">
          <label class="text-zinc-500">Từ:</label>
          <input
            v-model="filters.fromDate"
            type="date"
            @change="loadExpenses"
            class="bg-barber-dark border border-zinc-700 rounded-xl px-3 py-1.5 text-white focus:outline-none focus:border-barber-gold"
          />
        </div>
        <div class="flex items-center gap-2">
          <label class="text-zinc-500">Đến:</label>
          <input
            v-model="filters.toDate"
            type="date"
            @change="loadExpenses"
            class="bg-barber-dark border border-zinc-700 rounded-xl px-3 py-1.5 text-white focus:outline-none focus:border-barber-gold"
          />
        </div>
      </div>
    </div>

    <!-- Expenses Table -->
    <div class="bg-barber-card border border-barber-border rounded-2xl overflow-hidden shadow-xl">
      <div class="overflow-x-auto">
        <table class="w-full text-left text-xs">
          <thead class="bg-zinc-900/80 text-zinc-400 uppercase tracking-wider font-semibold border-b border-barber-border">
            <tr>
              <th class="py-3.5 px-4">Tên Khoản Chi</th>
              <th class="py-3.5 px-4">Phân Loại</th>
              <th class="py-3.5 px-4">Số Tiền (VNĐ)</th>
              <th class="py-3.5 px-4">Ngày Chi</th>
              <th class="py-3.5 px-4">Ghi Chú</th>
              <th class="py-3.5 px-4 text-right">Thao Tác</th>
            </tr>
          </thead>
          <tbody class="divide-y divide-zinc-800/60">
            <tr v-if="expenses.length === 0">
              <td colspan="6" class="py-12 text-center text-zinc-500">
                Không tìm thấy khoản chi tiêu nào
              </td>
            </tr>

            <tr v-for="exp in expenses" :key="exp.id" class="hover:bg-zinc-800/40 transition">
              <td class="py-3.5 px-4">
                <p class="font-bold text-white text-sm">{{ exp.title }}</p>
              </td>
              <td class="py-3.5 px-4">
                <span 
                  class="px-2 py-0.5 rounded font-medium text-[11px]"
                  :class="getCategoryBadgeClass(exp.category)"
                >
                  {{ exp.category }}
                </span>
              </td>
              <td class="py-3.5 px-4 font-extrabold text-rose-400 text-sm">
                -{{ formatCurrency(exp.amount) }}
              </td>
              <td class="py-3.5 px-4 text-zinc-300">
                {{ formatDate(exp.date) }}
              </td>
              <td class="py-3.5 px-4 max-w-[220px]">
                <span v-if="exp.note" class="text-zinc-400 truncate block" :title="exp.note">
                  {{ exp.note }}
                </span>
                <span v-else class="text-zinc-600">—</span>
              </td>
              <td class="py-3.5 px-4 text-right space-x-2">
                <button 
                  @click="openModal(exp)"
                  class="p-1.5 rounded-lg bg-zinc-800 hover:bg-zinc-700 text-zinc-300 hover:text-white transition"
                  title="Sửa"
                >
                  <Edit3 class="w-4 h-4" />
                </button>
                <button 
                  @click="deleteExpense(exp.id)"
                  class="p-1.5 rounded-lg bg-rose-500/10 hover:bg-rose-500/20 text-rose-400 transition"
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
    <div v-if="isModalOpen" class="fixed inset-0 z-50 flex items-center justify-center p-4 bg-black/80 backdrop-blur-sm">
      <div class="relative w-full max-w-md bg-barber-card border border-barber-border rounded-2xl shadow-2xl p-6">
        <h3 class="text-lg font-bold text-white mb-4">
          {{ editingId ? 'Chỉnh Sửa Khoản Chi' : 'Thêm Khoản Chi Mới' }}
        </h3>

        <form @submit.prevent="saveExpense" class="space-y-4 text-xs">
          <div>
            <label class="block text-zinc-400 mb-1 font-medium">Tên khoản chi *</label>
            <input
              v-model="form.title"
              type="text"
              required
              placeholder="VD: Mua lưỡi lam & bọt cạo râu"
              class="w-full bg-barber-dark border border-zinc-700 rounded-xl px-3 py-2 text-white focus:outline-none focus:border-barber-gold"
            />
          </div>

          <div class="grid grid-cols-2 gap-3">
            <div>
              <label class="block text-zinc-400 mb-1 font-medium">Số tiền chi (VNĐ) *</label>
              <input
                v-model.number="form.amount"
                type="number"
                min="1000"
                step="5000"
                required
                class="w-full bg-barber-dark border border-zinc-700 rounded-xl px-3 py-2 text-white font-bold focus:outline-none focus:border-barber-gold"
              />
            </div>
            <div>
              <label class="block text-zinc-400 mb-1 font-medium">Ngày chi *</label>
              <input
                v-model="form.date"
                type="date"
                required
                class="w-full bg-barber-dark border border-zinc-700 rounded-xl px-3 py-2 text-white focus:outline-none focus:border-barber-gold"
              />
            </div>
          </div>

          <div>
            <label class="block text-zinc-400 mb-1 font-medium">Phân loại danh mục</label>
            <select
              v-model="form.category"
              class="w-full bg-barber-dark border border-zinc-700 rounded-xl px-3 py-2 text-white focus:outline-none focus:border-barber-gold"
            >
              <option v-for="cat in categories" :key="cat" :value="cat">{{ cat }}</option>
            </select>
          </div>

          <div>
            <label class="block text-zinc-400 mb-1 font-medium">Ghi chú</label>
            <textarea
              v-model="form.note"
              rows="2"
              placeholder="Chi tiết nơi mua, lý do chi tiêu..."
              class="w-full bg-barber-dark border border-zinc-700 rounded-xl px-3 py-2 text-white focus:outline-none focus:border-barber-gold"
            ></textarea>
          </div>

          <div class="flex gap-3 pt-3">
            <button
              type="button"
              @click="isModalOpen = false"
              class="flex-1 py-2.5 rounded-xl bg-zinc-800 hover:bg-zinc-700 text-zinc-300 font-semibold transition"
            >
              Hủy
            </button>
            <button
              type="submit"
              class="flex-1 py-2.5 rounded-xl bg-barber-gold hover:bg-amber-400 text-black font-bold transition"
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
import { Plus, Edit3, Trash2, ArrowDownRight, Calendar, Receipt, RotateCcw } from 'lucide-vue-next'

const expenses = ref([])
const categories = ref([
  'Mặt bằng & Tiện ích',
  'Phụ liệu & Hóa chất',
  'Dụng cụ & Máy móc',
  'Sinh hoạt & Ăn uống',
  'Marketing & Quảng cáo',
  'Khác'
])

const isModalOpen = ref(false)
const editingId = ref(null)
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
  category: ''
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

  loadExpenses()
}

async function loadExpenses() {
  try {
    const params = {}
    if (filters.value.date) params.date = filters.value.date
    if (filters.value.fromDate) params.fromDate = filters.value.fromDate
    if (filters.value.toDate) params.toDate = filters.value.toDate
    if (filters.value.category) params.category = filters.value.category

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

onMounted(() => {
  applyPreset('today')
  loadCategories()
})

const todayTotal = computed(() => {
  const todayStr = new Date().toISOString().split('T')[0]
  return expenses.value
    .filter(e => e.date?.startsWith(todayStr))
    .reduce((sum, e) => sum + e.amount, 0)
})

const monthTotal = computed(() => {
  const currentMonth = new Date().getMonth()
  const currentYear = new Date().getFullYear()
  return expenses.value
    .filter(e => {
      const d = new Date(e.date)
      return d.getMonth() === currentMonth && d.getFullYear() === currentYear
    })
    .reduce((sum, e) => sum + e.amount, 0)
})

const latestExpense = computed(() => {
  return expenses.value[0] || null
})

function resetFilters() {
  filters.value.date = ''
  filters.value.category = ''
  loadExpenses()
}

function openModal(exp = null) {
  if (exp) {
    editingId.value = exp.id
    form.value = {
      title: exp.title,
      amount: exp.amount,
      category: exp.category,
      date: exp.date ? exp.date.split('T')[0] : new Date().toISOString().split('T')[0],
      note: exp.note || ''
    }
  } else {
    editingId.value = null
    form.value = {
      title: '',
      amount: 50000,
      category: 'Phụ liệu & Hóa chất',
      date: new Date().toISOString().split('T')[0],
      note: ''
    }
  }
  isModalOpen.value = true
}

async function saveExpense() {
  try {
    const payload = {
      ...form.value,
      date: new Date(form.value.date).toISOString()
    }

    if (editingId.value) {
      await api.updateExpense(editingId.value, { ...payload, id: editingId.value })
    } else {
      await api.createExpense(payload)
    }

    isModalOpen.value = false
    loadExpenses()
  } catch (err) {
    alert('Lỗi khi lưu khoản chi: ' + (err.response?.data || err.message))
  }
}

async function deleteExpense(id) {
  if (!confirm('Bạn có chắc muốn xóa khoản chi này?')) return
  try {
    await api.deleteExpense(id)
    loadExpenses()
  } catch (err) {
    alert('Không thể xóa: ' + err.message)
  }
}

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
    case 'Mặt bằng & Tiện ích':
      return 'bg-purple-500/10 text-purple-300 border border-purple-500/30'
    case 'Phụ liệu & Hóa chất':
      return 'bg-amber-500/10 text-barber-gold border border-amber-500/30'
    case 'Dụng cụ & Máy móc':
      return 'bg-cyan-500/10 text-cyan-300 border border-cyan-500/30'
    case 'Sinh hoạt & Ăn uống':
      return 'bg-emerald-500/10 text-emerald-300 border border-emerald-500/30'
    case 'Marketing & Quảng cáo':
      return 'bg-blue-500/10 text-blue-300 border border-blue-500/30'
    default:
      return 'bg-zinc-800 text-zinc-400'
  }
}
</script>
