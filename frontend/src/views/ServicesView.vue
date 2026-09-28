<template>
  <div class="p-6 space-y-6 overflow-y-auto h-screen max-w-6xl mx-auto bg-slate-50 text-slate-800">
    <!-- Header -->
    <div class="flex flex-col sm:flex-row justify-between items-start sm:items-center gap-3 border-b border-slate-200 pb-4">
      <div>
        <h2 class="text-2xl font-black text-slate-900 tracking-tight">Quản Lý Dịch Vụ & Bảng Giá</h2>
        <p class="text-xs text-slate-500 mt-0.5 font-medium">Quản lý menu gói dịch vụ, đơn giá và màu sắc nhận diện trên POS</p>
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
          Thêm Dịch Vụ Mới
        </button>
      </div>
    </div>

    <!-- Table of Services -->
    <div class="bg-white border border-slate-200 rounded-2xl overflow-hidden shadow-xs">
      <div class="overflow-x-auto">
        <table class="w-full text-left text-xs">
          <thead class="bg-slate-50 text-slate-500 uppercase tracking-wider font-bold text-[11px] border-b border-slate-200">
            <tr>
              <th class="py-3.5 px-4">Tên Dịch Vụ</th>
              <th class="py-3.5 px-4">Phân Loại</th>
              <th class="py-3.5 px-4">Đơn Giá</th>
              <th class="py-3.5 px-4">Trạng Thái</th>
              <th class="py-3.5 px-4 text-right">Thao Tác</th>
            </tr>
          </thead>
          <tbody class="divide-y divide-slate-100 font-medium">
            <tr v-for="svc in services" :key="svc.id" class="hover:bg-slate-50/80 transition">
              <td class="py-3 px-4">
                <p class="font-bold text-slate-900 text-sm">{{ svc.name }}</p>
                <p v-if="svc.description" class="text-slate-500 text-[11px] mt-0.5 line-clamp-1">{{ svc.description }}</p>
              </td>
              <td class="py-3 px-4">
                <span 
                  class="px-2.5 py-1 rounded-lg border font-bold text-[11px] inline-flex items-center gap-1.5 shadow-2xs"
                  :style="{
                    backgroundColor: getCategoryColor(svc.category) + '15',
                    borderColor: getCategoryColor(svc.category) + '40',
                    color: getCategoryColor(svc.category)
                  }"
                >
                  <span 
                    class="w-2 h-2 rounded-full inline-block" 
                    :style="{ backgroundColor: getCategoryColor(svc.category) }"
                  ></span>
                  {{ svc.category }}
                </span>
              </td>
              <td class="py-3 px-4 font-black text-indigo-600 text-sm">
                {{ formatCurrency(svc.price) }}
              </td>
              <td class="py-3 px-4">
                <span 
                  class="px-2 py-0.5 rounded-full font-bold text-[10px]"
                  :class="svc.isActive ? 'bg-emerald-50 text-emerald-600 border border-emerald-200' : 'bg-slate-100 text-slate-500 border border-slate-200'"
                >
                  {{ svc.isActive ? 'Đang phục vụ' : 'Tạm dừng' }}
                </span>
              </td>
              <td class="py-3 px-4 text-right space-x-2">
                <button 
                  @click="openModal(svc)"
                  class="p-1.5 rounded-lg bg-slate-100 hover:bg-indigo-50 text-slate-600 hover:text-indigo-600 transition"
                  title="Sửa"
                >
                  <Edit3 class="w-4 h-4" />
                </button>
                <button 
                  @click="deleteService(svc.id)"
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

    <!-- Modal Form (Thêm/Sửa Dịch Vụ) -->
    <div v-if="isModalOpen" class="fixed inset-0 z-50 flex items-center justify-center p-4 bg-slate-900/40 backdrop-blur-sm">
      <div class="relative w-full max-w-md bg-white border border-slate-200 rounded-2xl shadow-2xl p-6">
        <h3 class="text-lg font-bold text-slate-900 mb-4">
          {{ editingId ? 'Chỉnh Sửa Dịch Vụ' : 'Thêm Dịch Vụ Mới' }}
        </h3>

        <form @submit.prevent="saveService" class="space-y-4 text-xs">
          <div>
            <label class="block text-slate-600 mb-1 font-medium">Tên dịch vụ *</label>
            <input
              v-model="form.name"
              type="text"
              required
              placeholder="VD: Dịch vụ tiêu chuẩn"
              class="w-full bg-white border border-slate-200 rounded-xl px-3 py-2 text-slate-800 focus:outline-none focus:border-indigo-500 shadow-2xs"
            />
          </div>

          <div>
            <label class="block text-slate-600 mb-1 font-medium">Đơn giá (VNĐ) *</label>
            <input
              v-model.number="form.price"
              type="number"
              min="0"
              step="5000"
              required
              class="w-full bg-white border border-slate-200 rounded-xl px-3 py-2 text-slate-900 font-bold focus:outline-none focus:border-indigo-500 shadow-2xs"
            />
          </div>

          <div>
            <div class="flex justify-between items-center mb-1">
              <label class="block text-slate-600 font-medium">Phân loại dịch vụ</label>
              <button 
                type="button" 
                @click="isCategoryModalOpen = true" 
                class="text-[11px] text-indigo-600 hover:underline flex items-center gap-1 font-semibold"
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

          <div>
            <label class="block text-slate-600 mb-1 font-medium">Mô tả chi tiết</label>
            <textarea
              v-model="form.description"
              rows="2"
              placeholder="Quy trình thực hiện hoặc điểm nhấn của dịch vụ..."
              class="w-full bg-white border border-slate-200 rounded-xl px-3 py-2 text-slate-800 focus:outline-none focus:border-indigo-500 shadow-2xs"
            ></textarea>
          </div>

          <div class="flex items-center gap-2 pt-1">
            <input
              type="checkbox"
              id="isActive"
              v-model="form.isActive"
              class="w-4 h-4 rounded text-indigo-600 focus:ring-indigo-500 bg-white border-slate-300"
            />
            <label for="isActive" class="text-slate-700 font-medium select-none cursor-pointer">Đang hoạt động (hiển thị trên POS)</label>
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
              Lưu Dịch Vụ
            </button>
          </div>
        </form>
      </div>
    </div>

    <!-- Modal Quản Lý Phân Loại Dịch Vụ -->
    <div v-if="isCategoryModalOpen" class="fixed inset-0 z-50 flex items-center justify-center p-4 bg-slate-900/40 backdrop-blur-sm">
      <div class="relative w-full max-w-lg bg-white border border-slate-200 rounded-2xl shadow-2xl p-6">
        <div class="flex justify-between items-center border-b border-slate-100 pb-3 mb-4">
          <div>
            <h3 class="text-lg font-bold text-slate-900 flex items-center gap-2">
              <Layers class="w-5 h-5 text-indigo-600" />
              Quản Lý Phân Loại Dịch Vụ
            </h3>
            <p class="text-xs text-slate-500 mt-0.5">Thêm, xóa phân loại và chọn màu sắc nhận diện</p>
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
          <p class="text-xs font-bold text-slate-700">Thêm Phân Loại Mới</p>
          <div class="flex gap-2">
            <input
              v-model="newCatName"
              type="text"
              placeholder="VD: Chăm sóc cơ bản, Gói dịch vụ VIP..."
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
import { ref, onMounted } from 'vue'
import api from '@/services/api'
import { Plus, Edit3, Trash2, Layers } from 'lucide-vue-next'

const services = ref([])
const isModalOpen = ref(false)
const isCategoryModalOpen = ref(false)
const editingId = ref(null)

const categories = ref([
  { id: 1, name: 'Dịch vụ chính', color: '#4f46e5' },
  { id: 2, name: 'Combo', color: '#8b5cf6' },
  { id: 3, name: 'Chăm sóc', color: '#10b981' },
  { id: 4, name: 'Thư giãn', color: '#06b6d4' }
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
  price: 100000,
  durationMinutes: 30,
  category: 'Dịch vụ chính',
  description: '',
  isActive: true
})

function formatCurrency(val) {
  return new Intl.NumberFormat('vi-VN', { style: 'currency', currency: 'VND' }).format(val)
}

async function loadCategories() {
  try {
    const res = await api.getServiceCategories()
    if (res.data && res.data.length > 0) {
      categories.value = res.data
    }
  } catch (err) {
    console.error('Lỗi tải danh mục dịch vụ:', err)
  }
}

async function loadServices() {
  try {
    const res = await api.getServices()
    services.value = res.data
  } catch (err) {
    console.error('Lỗi tải dịch vụ:', err)
  }
}

function openModal(svc = null) {
  if (svc) {
    editingId.value = svc.id
    form.value = { ...svc }
  } else {
    editingId.value = null
    form.value = {
      name: '',
      price: 100000,
      durationMinutes: 30,
      category: categories.value[0]?.name || 'Dịch vụ chính',
      description: '',
      isActive: true
    }
  }
  isModalOpen.value = true
}

async function saveService() {
  try {
    if (editingId.value) {
      await api.updateService(editingId.value, { ...form.value, id: editingId.value })
    } else {
      await api.createService(form.value)
    }
    isModalOpen.value = false
    loadServices()
  } catch (err) {
    alert('Lỗi lưu dịch vụ: ' + err.message)
  }
}

async function deleteService(id) {
  if (!confirm('Bạn có chắc chắn muốn xóa dịch vụ này?')) return
  try {
    await api.deleteService(id)
    loadServices()
  } catch (err) {
    alert('Không thể xóa: ' + err.message)
  }
}

async function addCategory() {
  const name = newCatName.value.trim()
  if (!name) {
    alert('Vui lòng nhập tên phân loại!')
    return
  }

  try {
    await api.createServiceCategory({
      name,
      color: selectedColor.value
    })
    newCatName.value = ''
    loadCategories()
  } catch (err) {
    alert('Lỗi thêm phân loại: ' + (err.response?.data || err.message))
  }
}

async function deleteCategory(cat) {
  if (!confirm(`Bạn có chắc muốn xóa phân loại "${cat.name}"?`)) return
  try {
    if (cat.id) {
      await api.deleteServiceCategory(cat.id)
    }
    categories.value = categories.value.filter(c => c.name !== cat.name)
    loadCategories()
  } catch (err) {
    alert('Lỗi xóa phân loại: ' + (err.response?.data || err.message))
  }
}

onMounted(() => {
  loadCategories()
  loadServices()
})
</script>
