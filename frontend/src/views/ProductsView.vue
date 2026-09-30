<template>
  <div class="p-6 space-y-6 overflow-y-auto h-screen max-w-6xl mx-auto bg-slate-50 text-slate-800">
    <!-- Header -->
    <div class="flex flex-col sm:flex-row justify-between items-start sm:items-center gap-3 border-b border-slate-200 pb-4">
      <div>
        <h2 class="text-2xl font-black text-slate-900 tracking-tight">Quản Lý Sản Phẩm & Tồn Kho</h2>
        <p class="text-xs text-slate-500 mt-0.5 font-medium">Theo dõi danh mục sản phẩm, giá bán, giá vốn và số lượng tồn kho</p>
      </div>
      <div class="flex items-center gap-2.5">
        <button
          @click="isCategoryModalOpen = true"
          class="py-2.5 px-4 rounded-xl bg-white hover:bg-slate-50 text-slate-700 border border-slate-200 font-bold text-xs transition flex items-center gap-2 active:scale-95 shadow-2xs"
        >
          <Layers class="w-4 h-4 text-indigo-600" />
          Quản Lý Phân Loại ({{ categories.length }})
        </button>
        <button
          @click="openModal()"
          class="py-2.5 px-4 rounded-xl bg-indigo-600 hover:bg-indigo-700 text-white font-bold text-xs shadow-md shadow-indigo-500/25 transition flex items-center gap-2 active:scale-95"
        >
          <Plus class="w-4 h-4" />
          Thêm Sản Phẩm Mới
        </button>
      </div>
    </div>

    <!-- Thanh Lọc: Phân biệt Bán tại POS vs Kho nội bộ & Tìm kiếm -->
    <div class="flex flex-col sm:flex-row items-stretch sm:items-center justify-between gap-3">
      <!-- Tabs Lọc -->
      <div class="flex bg-slate-100 p-1 rounded-xl border border-slate-200 text-xs font-bold">
        <button
          @click="statusFilter = 'all'"
          class="px-3.5 py-1.5 rounded-lg transition"
          :class="statusFilter === 'all' ? 'bg-white text-indigo-600 shadow-2xs' : 'text-slate-600 hover:text-slate-900'"
        >
          Tất cả ({{ products.length }})
        </button>
        <button
          @click="statusFilter = 'pos'"
          class="px-3.5 py-1.5 rounded-lg transition flex items-center gap-1.5"
          :class="statusFilter === 'pos' ? 'bg-white text-indigo-600 shadow-2xs' : 'text-slate-600 hover:text-slate-900'"
        >
          <ShoppingCart class="w-3.5 h-3.5" />
          Bán tại POS ({{ posProductsCount }})
        </button>
        <button
          @click="statusFilter = 'internal'"
          class="px-3.5 py-1.5 rounded-lg transition flex items-center gap-1.5"
          :class="statusFilter === 'internal' ? 'bg-white text-indigo-600 shadow-2xs' : 'text-slate-600 hover:text-slate-900'"
        >
          <Box class="w-3.5 h-3.5" />
          Kho dùng nội bộ ({{ internalProductsCount }})
        </button>
      </div>

      <!-- Tìm kiếm nhanh -->
      <div class="relative w-full sm:w-64">
        <input
          v-model="productSearch"
          type="text"
          placeholder="Tìm theo tên sản phẩm..."
          class="w-full bg-white border border-slate-200 rounded-xl px-3.5 py-1.5 pl-9 text-xs text-slate-800 placeholder-slate-400 focus:outline-none focus:border-indigo-500 shadow-2xs transition"
        />
        <Search class="w-4 h-4 text-slate-400 absolute left-3 top-2" />
        <button 
          v-if="productSearch" 
          @click="productSearch = ''" 
          class="absolute right-3 top-1.5 text-slate-400 hover:text-slate-600 text-xs"
        >
          ✕
        </button>
      </div>
    </div>

    <!-- Products Table -->
    <div class="bg-white border border-slate-200 rounded-2xl overflow-hidden shadow-xs">
      <div class="overflow-x-auto">
        <table class="w-full text-left text-xs">
          <thead class="bg-slate-50 text-slate-500 uppercase tracking-wider font-bold text-[11px] border-b border-slate-200">
            <tr>
              <th class="py-3.5 px-4">Sản Phẩm</th>
              <th class="py-3.5 px-4">Phân Loại</th>
              <th class="py-3.5 px-4">Giá Nhập</th>
              <th class="py-3.5 px-4">Giá Bán Lẻ</th>
              <th class="py-3.5 px-4">Tồn Kho</th>
              <th class="py-3.5 px-4 text-center">Kênh Phân Phối</th>
              <th class="py-3.5 px-4 text-right">Thao Tác</th>
            </tr>
          </thead>
          <tbody class="divide-y divide-slate-100 font-medium">
            <tr v-for="prod in filteredProducts" :key="prod.id" class="hover:bg-slate-50/80 transition">
              <td class="py-3 px-4">
                <p class="font-bold text-slate-900 text-sm">{{ prod.name }}</p>
                <p v-if="prod.description" class="text-slate-500 text-[11px] mt-0.5 line-clamp-1">{{ prod.description }}</p>
              </td>
              <td class="py-3 px-4">
                <span 
                  class="px-2.5 py-1 rounded-lg border font-bold text-[11px] inline-flex items-center gap-1.5 shadow-2xs"
                  :style="{
                    backgroundColor: getCategoryColor(prod.category) + '15',
                    borderColor: getCategoryColor(prod.category) + '40',
                    color: getCategoryColor(prod.category)
                  }"
                >
                  <span 
                    class="w-2 h-2 rounded-full inline-block" 
                    :style="{ backgroundColor: getCategoryColor(prod.category) }"
                  ></span>
                  {{ prod.category }}
                </span>
              </td>
              <td class="py-3 px-4 text-slate-600">
                {{ formatCurrency(prod.costPrice) }}
              </td>
              <td class="py-3 px-4 font-black text-indigo-600 text-sm">
                {{ formatCurrency(prod.salePrice) }}
              </td>
              <td class="py-3 px-4">
                <div class="flex items-center gap-2">
                  <span 
                    class="font-mono font-bold text-sm"
                    :class="prod.stockQuantity <= prod.lowStockAlert ? 'text-rose-600 font-black' : 'text-slate-900'"
                  >
                    {{ prod.stockQuantity }}
                  </span>
                  <span 
                    v-if="prod.stockQuantity <= prod.lowStockAlert" 
                    class="text-[10px] font-bold text-rose-600 bg-rose-50 px-1.5 py-0.5 rounded border border-rose-200"
                  >
                    Sắp hết!
                  </span>
                </div>
              </td>
              <td class="py-3 px-4 text-center">
                <button
                  type="button"
                  @click="togglePos(prod)"
                  class="px-2.5 py-1 rounded-lg border text-[11px] font-bold transition inline-flex items-center gap-1.5 active:scale-95 shadow-2xs"
                  :class="prod.showOnPos !== false 
                    ? 'bg-emerald-50 text-emerald-600 border-emerald-200 hover:bg-emerald-100' 
                    : 'bg-slate-100 text-slate-600 border-slate-200 hover:bg-slate-200 hover:text-slate-900'"
                  :title="prod.showOnPos !== false ? 'Đang hiện ở POS (Click để chuyển sang Kho nội bộ)' : 'Đang là Kho nội bộ (Click để hiện ở POS)'"
                >
                  <span 
                    class="w-2 h-2 rounded-full inline-block"
                    :class="prod.showOnPos !== false ? 'bg-emerald-500 shadow-2xs shadow-emerald-500/50' : 'bg-slate-400'"
                  ></span>
                  {{ prod.showOnPos !== false ? 'Bán tại POS' : 'Kho nội bộ' }}
                </button>
              </td>
              <td class="py-3 px-4 text-right space-x-2">
                <button 
                  @click="openModal(prod)"
                  class="p-1.5 rounded-lg bg-slate-100 hover:bg-indigo-50 text-slate-600 hover:text-indigo-600 transition"
                  title="Sửa"
                >
                  <Edit3 class="w-4 h-4" />
                </button>
                <button 
                  @click="deleteProduct(prod.id)"
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

    <!-- Modal Form (Thêm/Sửa Sản Phẩm) -->
    <div v-if="isModalOpen" class="fixed inset-0 z-50 flex items-center justify-center p-4 bg-slate-900/40 backdrop-blur-sm">
      <div class="relative w-full max-w-md bg-white border border-slate-200 rounded-2xl shadow-2xl p-6">
        <h3 class="text-lg font-bold text-slate-900 mb-4">
          {{ editingId ? 'Chỉnh Sửa Sản Phẩm' : 'Thêm Sản Phẩm Mới' }}
        </h3>

        <form @submit.prevent="saveProduct" class="space-y-4 text-xs">
          <div>
            <label class="block text-slate-600 mb-1 font-medium">Tên sản phẩm *</label>
            <input
              v-model="form.name"
              type="text"
              required
              placeholder="VD: Sản phẩm mẫu A"
              class="w-full bg-white border border-slate-200 rounded-xl px-3 py-2 text-slate-800 focus:outline-none focus:border-indigo-500 shadow-2xs"
            />
          </div>

          <div>
            <div class="flex justify-between items-center mb-1">
              <label class="block text-slate-600 font-medium">Phân loại *</label>
              <button 
                type="button" 
                @click="isCategoryModalOpen = true" 
                class="text-[11px] text-indigo-600 hover:underline font-semibold"
              >
                + Quản lý phân loại
              </button>
            </div>
            <select
              v-model="form.category"
              class="w-full bg-white border border-slate-200 rounded-xl px-3 py-2 text-slate-800 focus:outline-none focus:border-indigo-500 shadow-2xs"
            >
              <option v-for="cat in categories" :key="cat.name" :value="cat.name">
                {{ cat.name }}
              </option>
            </select>
          </div>

          <div class="grid grid-cols-2 gap-3">
            <div>
              <label class="block text-slate-600 mb-1 font-medium">Giá nhập vốn (VNĐ)</label>
              <input
                v-model.number="form.costPrice"
                type="number"
                min="0"
                step="5000"
                class="w-full bg-white border border-slate-200 rounded-xl px-3 py-2 text-slate-800 focus:outline-none focus:border-indigo-500 shadow-2xs"
              />
            </div>
            <div>
              <label class="block text-slate-600 mb-1 font-medium">Giá bán lẻ (VNĐ) *</label>
              <input
                v-model.number="form.salePrice"
                type="number"
                min="0"
                step="5000"
                required
                class="w-full bg-white border border-slate-200 rounded-xl px-3 py-2 text-slate-900 font-bold focus:outline-none focus:border-indigo-500 shadow-2xs"
              />
            </div>
          </div>

          <div class="grid grid-cols-2 gap-3">
            <div>
              <label class="block text-slate-600 mb-1 font-medium">Số lượng tồn kho</label>
              <input
                v-model.number="form.stockQuantity"
                type="number"
                min="0"
                class="w-full bg-white border border-slate-200 rounded-xl px-3 py-2 text-slate-800 focus:outline-none focus:border-indigo-500 shadow-2xs"
              />
            </div>
            <div>
              <label class="block text-slate-600 mb-1 font-medium">Cảnh báo khi dưới</label>
              <input
                v-model.number="form.lowStockAlert"
                type="number"
                min="1"
                class="w-full bg-white border border-slate-200 rounded-xl px-3 py-2 text-slate-800 focus:outline-none focus:border-indigo-500 shadow-2xs"
              />
            </div>
          </div>

          <!-- Switch chọn: Hiện ở POS bán hàng hay Kho dùng nội bộ -->
          <div class="p-3 bg-slate-50 border border-slate-200 rounded-xl flex items-center justify-between gap-3">
            <div class="space-y-0.5">
              <div class="flex items-center gap-2">
                <span class="font-bold text-slate-900 text-xs">Hiển thị ở màn hình POS</span>
                <span 
                  class="text-[10px] px-1.5 py-0.5 rounded font-bold"
                  :class="form.showOnPos ? 'bg-emerald-50 text-emerald-600 border border-emerald-200' : 'bg-slate-100 text-slate-600 border border-slate-200'"
                >
                  {{ form.showOnPos ? 'Bán tại POS' : 'Kho nội bộ' }}
                </span>
              </div>
              <p class="text-[11px] text-slate-500">
                {{ form.showOnPos ? 'Khách mua tại quầy - Hiện ở POS để tính tiền' : 'Vật tư/phụ liệu lưu kho dùng nội bộ - Không hiện ở POS' }}
              </p>
            </div>
            <label class="relative inline-flex items-center cursor-pointer flex-shrink-0">
              <input type="checkbox" v-model="form.showOnPos" class="sr-only peer">
              <div class="w-11 h-6 bg-slate-300 peer-focus:outline-none rounded-full peer peer-checked:after:translate-x-full peer-checked:after:border-white after:content-[''] after:absolute after:top-[2px] after:left-[2px] after:bg-white after:border-slate-300 after:border after:rounded-full after:h-5 after:w-5 after:transition-all peer-checked:bg-indigo-600"></div>
            </label>
          </div>

          <div>
            <label class="block text-slate-600 mb-1 font-medium">Mô tả đặc tính</label>
            <textarea
              v-model="form.description"
              rows="2"
              placeholder="Thông số, quy cách, ghi chú..."
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
              Lưu Sản Phẩm
            </button>
          </div>
        </form>
      </div>
    </div>

    <!-- Modal Quản Lý Phân Loại Sản Phẩm -->
    <div v-if="isCategoryModalOpen" class="fixed inset-0 z-50 flex items-center justify-center p-4 bg-slate-900/40 backdrop-blur-sm">
      <div class="relative w-full max-w-lg bg-white border border-slate-200 rounded-2xl shadow-2xl p-6">
        <div class="flex justify-between items-center border-b border-slate-100 pb-3 mb-4">
          <div>
            <h3 class="text-lg font-bold text-slate-900 flex items-center gap-2">
              <Layers class="w-5 h-5 text-indigo-600" />
              Quản Lý Phân Loại Sản Phẩm
            </h3>
            <p class="text-xs text-slate-500 mt-0.5">Thêm, xóa phân loại sản phẩm trong kho & chọn màu nhận diện</p>
          </div>
          <button 
            @click="isCategoryModalOpen = false"
            class="text-slate-400 hover:text-slate-600 p-1 rounded-lg"
          >
            ✕
          </button>
        </div>

        <!-- Form Thêm Phân Loại Mới -->
        <div class="bg-slate-50 p-3.5 rounded-xl border border-slate-200 mb-4 space-y-3">
          <p class="text-xs font-bold text-slate-700">Thêm Phân Loại Sản Phẩm Mới</p>
          <div class="flex gap-2">
            <input
              v-model="newCatName"
              type="text"
              placeholder="VD: Dụng cụ, Mỹ phẩm, Phụ kiện..."
              class="flex-1 bg-white border border-slate-200 rounded-xl px-3 py-2 text-xs text-slate-800 placeholder-slate-400 focus:outline-none focus:border-indigo-500"
            />
            <button
              @click="addCategory"
              class="px-4 py-2 bg-indigo-600 hover:bg-indigo-700 text-white font-bold text-xs rounded-xl transition flex items-center gap-1 active:scale-95 shadow-2xs"
            >
              <Plus class="w-3.5 h-3.5" /> Thêm
            </button>
          </div>

          <!-- Color palette picker -->
          <div>
            <label class="block text-[11px] text-slate-500 mb-1.5">Chọn màu nhận diện:</label>
            <div class="flex items-center gap-2 flex-wrap">
              <button
                v-for="color in colorPresets"
                :key="color"
                type="button"
                @click="selectedColor = color"
                class="w-6 h-6 rounded-full border-2 transition-transform"
                :class="selectedColor === color ? 'scale-125 border-slate-900 shadow-md' : 'border-transparent hover:scale-110'"
                :style="{ backgroundColor: color }"
              ></button>
              <input
                v-model="selectedColor"
                type="color"
                class="w-7 h-7 rounded border-0 bg-transparent cursor-pointer ml-1"
                title="Chọn màu tùy ý"
              />
            </div>
          </div>
        </div>

        <!-- Danh Sách Phân Loại Hiện Có -->
        <div class="space-y-2 max-h-60 overflow-y-auto pr-1 text-xs">
          <p class="text-slate-500 font-medium mb-1">Danh sách hiện tại ({{ categories.length }}):</p>
          <div 
            v-for="cat in categories" 
            :key="cat.id || cat.name"
            class="flex justify-between items-center p-2.5 bg-slate-50 rounded-xl border border-slate-200/80"
          >
            <div class="flex items-center gap-2.5">
              <span 
                class="w-3.5 h-3.5 rounded-full inline-block shadow-2xs"
                :style="{ backgroundColor: cat.color || '#4f46e5' }"
              ></span>
              <span class="font-bold text-slate-800">{{ cat.name }}</span>
            </div>
            
            <button
              @click="deleteCategory(cat)"
              class="p-1 text-slate-400 hover:text-rose-600 transition"
              title="Xóa phân loại"
            >
              <Trash2 class="w-4 h-4" />
            </button>
          </div>
        </div>

        <div class="mt-5 pt-3 border-t border-slate-100 flex justify-end">
          <button
            @click="isCategoryModalOpen = false"
            class="px-5 py-2 rounded-xl bg-slate-900 hover:bg-slate-800 text-white font-semibold text-xs transition shadow-sm"
          >
            Hoàn Tất
          </button>
        </div>
      </div>
    </div>

  </div>
</template>

<script setup>
import { ref, computed, onMounted } from 'vue'
import api from '@/services/api'
import { useNotify } from '@/composables/useNotify'
import { Plus, Edit3, Trash2, Layers, ShoppingCart, Box, Search } from 'lucide-vue-next'

const { toast, confirm } = useNotify()

const products = ref([])
const isModalOpen = ref(false)
const isCategoryModalOpen = ref(false)
const editingId = ref(null)

const statusFilter = ref('all') // 'all', 'pos', 'internal'
const productSearch = ref('')

const posProductsCount = computed(() => products.value.filter(p => p.showOnPos !== false).length)
const internalProductsCount = computed(() => products.value.filter(p => p.showOnPos === false).length)

const filteredProducts = computed(() => {
  return products.value.filter(p => {
    // 1. Lọc theo kênh Bán tại POS vs Kho nội bộ
    if (statusFilter.value === 'pos' && p.showOnPos === false) return false
    if (statusFilter.value === 'internal' && p.showOnPos !== false) return false

    // 2. Tìm kiếm theo tên hoặc phân loại
    if (productSearch.value.trim()) {
      const q = productSearch.value.toLowerCase().trim()
      const matchName = p.name && p.name.toLowerCase().includes(q)
      const matchCat = p.category && p.category.toLowerCase().includes(q)
      if (!matchName && !matchCat) return false
    }

    return true
  })
})

const categories = ref([
  { id: 1, name: 'Mỹ phẩm', color: '#4f46e5' },
  { id: 2, name: 'Dụng cụ', color: '#f59e0b' },
  { id: 3, name: 'Phụ kiện', color: '#8b5cf6' },
  { id: 4, name: 'Chăm sóc', color: '#10b981' },
  { id: 5, name: 'Tiêu hao', color: '#06b6d4' }
])

const colorPresets = [
  '#4f46e5', // Diro Indigo
  '#2563eb', // Blue
  '#06b6d4', // Cyan
  '#10b981', // Emerald
  '#f59e0b', // Amber
  '#ec4899', // Pink
  '#8b5cf6', // Violet
  '#64748b'  // Slate
]
const newCatName = ref('')
const selectedColor = ref('#4f46e5')

function getCategoryColor(catName) {
  const found = categories.value.find(c => c.name === catName)
  return found?.color || '#4f46e5'
}

const form = ref({
  name: '',
  costPrice: 0,
  salePrice: 200000,
  stockQuantity: 10,
  lowStockAlert: 3,
  category: 'Mỹ phẩm',
  description: '',
  showOnPos: true,
  isActive: true
})

function formatCurrency(val) {
  return new Intl.NumberFormat('vi-VN', { style: 'currency', currency: 'VND' }).format(val)
}

async function loadCategories() {
  try {
    const res = await api.getProductCategories()
    if (res.data && res.data.length > 0) {
      categories.value = res.data
    }
  } catch (err) {
    console.error('Lỗi tải danh mục sản phẩm:', err)
  }
}

async function loadProducts() {
  try {
    const res = await api.getProducts()
    products.value = res.data
  } catch (err) {
    console.error('Lỗi tải sản phẩm:', err)
  }
}

function openModal(prod = null) {
  if (prod) {
    editingId.value = prod.id
    form.value = { 
      ...prod,
      showOnPos: prod.showOnPos !== false
    }
  } else {
    editingId.value = null
    form.value = {
      name: '',
      costPrice: 0,
      salePrice: 150000,
      stockQuantity: 10,
      lowStockAlert: 3,
      category: categories.value[0]?.name || 'Mỹ phẩm',
      description: '',
      showOnPos: true,
      isActive: true
    }
  }
  isModalOpen.value = true
}

async function togglePos(prod) {
  const currentVal = prod.showOnPos !== false
  prod.showOnPos = !currentVal
  try {
    await api.toggleShowOnPos(prod.id)
    toast.info(prod.showOnPos ? 'Đã bật hiển thị bán tại POS' : 'Đã chuyển thành sản phẩm kho nội bộ')
  } catch (err) {
    prod.showOnPos = currentVal
    toast.error('Không thể cập nhật trạng thái hiển thị POS: ' + (err.response?.data || err.message))
  }
}

async function saveProduct() {
  try {
    if (editingId.value) {
      await api.updateProduct(editingId.value, { ...form.value, id: editingId.value })
      toast.success('Cập nhật sản phẩm thành công!')
    } else {
      await api.createProduct(form.value)
      toast.success('Thêm sản phẩm mới thành công!')
    }
    isModalOpen.value = false
    loadProducts()
  } catch (err) {
    toast.error('Lỗi lưu sản phẩm: ' + err.message)
  }
}

async function deleteProduct(id) {
  const ok = await confirm({
    title: 'Xóa sản phẩm?',
    message: 'Bạn có chắc chắn muốn xóa sản phẩm này không?',
    type: 'danger',
    confirmText: 'Xóa ngay',
    cancelText: 'Hủy'
  })
  if (!ok) return

  try {
    await api.deleteProduct(id)
    toast.success('Đã xóa sản phẩm thành công!')
    loadProducts()
  } catch (err) {
    toast.error('Không thể xóa: ' + err.message)
  }
}

async function addCategory() {
  const name = newCatName.value.trim()
  if (!name) {
    toast.warning('Vui lòng nhập tên phân loại!')
    return
  }

  try {
    await api.createProductCategory({
      name,
      color: selectedColor.value
    })
    newCatName.value = ''
    toast.success('Thêm phân loại thành công!')
    loadCategories()
  } catch (err) {
    toast.error('Lỗi thêm phân loại: ' + (err.response?.data || err.message))
  }
}

async function deleteCategory(cat) {
  const ok = await confirm({
    title: 'Xóa phân loại?',
    message: `Bạn có chắc muốn xóa phân loại "${cat.name}"?`,
    type: 'danger',
    confirmText: 'Xóa',
    cancelText: 'Hủy'
  })
  if (!ok) return

  try {
    if (cat.id) {
      await api.deleteProductCategory(cat.id)
    }
    categories.value = categories.value.filter(c => c.name !== cat.name)
    toast.success('Đã xóa phân loại thành công!')
    loadCategories()
  } catch (err) {
    toast.error('Lỗi xóa phân loại: ' + (err.response?.data || err.message))
  }
}

onMounted(() => {
  loadCategories()
  loadProducts()
})
</script>
