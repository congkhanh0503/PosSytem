<template>
  <div class="p-6 space-y-6 overflow-y-auto h-screen max-w-6xl mx-auto">
    <!-- Header -->
    <div class="flex justify-between items-center border-b border-barber-border pb-4">
      <div>
        <h2 class="text-2xl font-extrabold text-white">Bảng Giá & Dịch Vụ Cắt Tóc</h2>
        <p class="text-xs text-zinc-400 mt-0.5">Quản lý menu dịch vụ hiển thị trên màn hình bán hàng POS</p>
      </div>
      <button
        @click="openModal()"
        class="py-2.5 px-4 rounded-xl bg-barber-gold hover:bg-amber-400 text-black font-bold text-xs shadow-lg shadow-amber-500/20 transition flex items-center gap-2 active:scale-95"
      >
        <Plus class="w-4 h-4" />
        Thêm Dịch Vụ Mới
      </button>
    </div>

    <!-- Table of Services -->
    <div class="bg-barber-card border border-barber-border rounded-2xl overflow-hidden shadow-xl">
      <div class="overflow-x-auto">
        <table class="w-full text-left text-xs">
          <thead class="bg-zinc-900/80 text-zinc-400 uppercase tracking-wider font-semibold border-b border-barber-border">
            <tr>
              <th class="py-3.5 px-4">Tên Dịch Vụ</th>
              <th class="py-3.5 px-4">Phân Loại</th>
              <th class="py-3.5 px-4">Thời Gian</th>
              <th class="py-3.5 px-4">Đơn Giá</th>
              <th class="py-3.5 px-4">Trạng Thái</th>
              <th class="py-3.5 px-4 text-right">Thao Tác</th>
            </tr>
          </thead>
          <tbody class="divide-y divide-zinc-800/60">
            <tr v-for="svc in services" :key="svc.id" class="hover:bg-zinc-800/40 transition">
              <td class="py-3 px-4">
                <p class="font-bold text-white text-sm">{{ svc.name }}</p>
                <p v-if="svc.description" class="text-zinc-400 text-[11px] mt-0.5 line-clamp-1">{{ svc.description }}</p>
              </td>
              <td class="py-3 px-4">
                <span class="px-2 py-0.5 rounded bg-zinc-800 text-zinc-300 border border-zinc-700 font-medium">
                  {{ svc.category }}
                </span>
              </td>
              <td class="py-3 px-4 text-zinc-300 font-medium">
                {{ svc.durationMinutes }} phút
              </td>
              <td class="py-3 px-4 font-extrabold text-amber-300 text-sm">
                {{ formatCurrency(svc.price) }}
              </td>
              <td class="py-3 px-4">
                <span 
                  class="px-2 py-0.5 rounded-full font-bold text-[10px]"
                  :class="svc.isActive ? 'bg-emerald-500/10 text-emerald-400 border border-emerald-500/30' : 'bg-zinc-800 text-zinc-500'"
                >
                  {{ svc.isActive ? 'Đang phục vụ' : 'Tạm dừng' }}
                </span>
              </td>
              <td class="py-3 px-4 text-right space-x-2">
                <button 
                  @click="openModal(svc)"
                  class="p-1.5 rounded-lg bg-zinc-800 hover:bg-zinc-700 text-zinc-300 hover:text-white transition"
                  title="Sửa"
                >
                  <Edit3 class="w-4 h-4" />
                </button>
                <button 
                  @click="deleteService(svc.id)"
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

    <!-- Modal Form (Thêm/Sửa) -->
    <div v-if="isModalOpen" class="fixed inset-0 z-50 flex items-center justify-center p-4 bg-black/80 backdrop-blur-sm">
      <div class="relative w-full max-w-md bg-barber-card border border-barber-border rounded-2xl shadow-2xl p-6">
        <h3 class="text-lg font-bold text-white mb-4">
          {{ editingId ? 'Chỉnh Sửa Dịch Vụ' : 'Thêm Dịch Vụ Mới' }}
        </h3>

        <form @submit.prevent="saveService" class="space-y-4 text-xs">
          <div>
            <label class="block text-zinc-400 mb-1 font-medium">Tên dịch vụ *</label>
            <input
              v-model="form.name"
              type="text"
              required
              placeholder="VD: Cắt tóc Fade & Tạo kiểu"
              class="w-full bg-barber-dark border border-zinc-700 rounded-xl px-3 py-2 text-white focus:outline-none focus:border-barber-gold"
            />
          </div>

          <div class="grid grid-cols-2 gap-3">
            <div>
              <label class="block text-zinc-400 mb-1 font-medium">Đơn giá (VNĐ) *</label>
              <input
                v-model.number="form.price"
                type="number"
                min="0"
                step="5000"
                required
                class="w-full bg-barber-dark border border-zinc-700 rounded-xl px-3 py-2 text-white focus:outline-none focus:border-barber-gold"
              />
            </div>
            <div>
              <label class="block text-zinc-400 mb-1 font-medium">Thời gian (phút)</label>
              <input
                v-model.number="form.durationMinutes"
                type="number"
                min="5"
                step="5"
                class="w-full bg-barber-dark border border-zinc-700 rounded-xl px-3 py-2 text-white focus:outline-none focus:border-barber-gold"
              />
            </div>
          </div>

          <div>
            <label class="block text-zinc-400 mb-1 font-medium">Phân loại</label>
            <select
              v-model="form.category"
              class="w-full bg-barber-dark border border-zinc-700 rounded-xl px-3 py-2 text-white focus:outline-none focus:border-barber-gold"
            >
              <option value="Cắt tóc">Cắt tóc</option>
              <option value="Combo">Combo</option>
              <option value="Hóa chất">Hóa chất (Uốn / Nhuộm)</option>
              <option value="Gội & Chăm sóc">Gội & Chăm sóc</option>
              <option value="Cạo & Rái tai">Cạo & Rái tai</option>
            </select>
          </div>

          <div>
            <label class="block text-zinc-400 mb-1 font-medium">Mô tả chi tiết</label>
            <textarea
              v-model="form.description"
              rows="2"
              placeholder="Quy trình thực hiện hoặc điểm nhấn của dịch vụ..."
              class="w-full bg-barber-dark border border-zinc-700 rounded-xl px-3 py-2 text-white focus:outline-none focus:border-barber-gold"
            ></textarea>
          </div>

          <div class="flex items-center gap-2 pt-1">
            <input
              type="checkbox"
              id="isActive"
              v-model="form.isActive"
              class="w-4 h-4 rounded text-barber-gold focus:ring-amber-500 bg-zinc-800 border-zinc-700"
            />
            <label for="isActive" class="text-zinc-300 font-medium select-none">Đang hoạt động (hiển thị trên POS)</label>
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
              Lưu Dịch Vụ
            </button>
          </div>
        </form>
      </div>
    </div>

  </div>
</template>

<script setup>
import { ref, onMounted } from 'vue'
import api from '@/services/api'
import { Plus, Edit3, Trash2 } from 'lucide-vue-next'

const services = ref([])
const isModalOpen = ref(false)
const editingId = ref(null)

const form = ref({
  name: '',
  price: 100000,
  durationMinutes: 30,
  category: 'Cắt tóc',
  description: '',
  isActive: true
})

async function loadServices() {
  try {
    const res = await api.getServices()
    services.value = res.data
  } catch (err) {
    console.error('Lỗi khi tải dịch vụ:', err)
  }
}

onMounted(() => {
  loadServices()
})

function formatCurrency(val) {
  return new Intl.NumberFormat('vi-VN', { style: 'currency', currency: 'VND' }).format(val || 0)
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
      category: 'Cắt tóc',
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
    alert('Không thể xóa dịch vụ: ' + err.message)
  }
}
</script>
