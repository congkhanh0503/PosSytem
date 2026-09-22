<template>
  <div class="p-6 space-y-6 overflow-y-auto h-screen max-w-6xl mx-auto">
    <!-- Header -->
    <div class="flex justify-between items-center border-b border-barber-border pb-4">
      <div>
        <h2 class="text-2xl font-extrabold text-white">Quản Lý Sản Phẩm & Tồn Kho</h2>
        <p class="text-xs text-zinc-400 mt-0.5">Theo dõi sáp, pomade, gôm xịt, tinh dầu dưỡng và số lượng tồn</p>
      </div>
      <button
        @click="openModal()"
        class="py-2.5 px-4 rounded-xl bg-barber-gold hover:bg-amber-400 text-black font-bold text-xs shadow-lg shadow-amber-500/20 transition flex items-center gap-2 active:scale-95"
      >
        <Plus class="w-4 h-4" />
        Thêm Sản Phẩm Mới
      </button>
    </div>

    <!-- Products Table -->
    <div class="bg-barber-card border border-barber-border rounded-2xl overflow-hidden shadow-xl">
      <div class="overflow-x-auto">
        <table class="w-full text-left text-xs">
          <thead class="bg-zinc-900/80 text-zinc-400 uppercase tracking-wider font-semibold border-b border-barber-border">
            <tr>
              <th class="py-3.5 px-4">Sản Phẩm</th>
              <th class="py-3.5 px-4">SKU</th>
              <th class="py-3.5 px-4">Phân Loại</th>
              <th class="py-3.5 px-4">Giá Nhập</th>
              <th class="py-3.5 px-4">Giá Bán Lẻ</th>
              <th class="py-3.5 px-4">Tồn Kho</th>
              <th class="py-3.5 px-4 text-right">Thao Tác</th>
            </tr>
          </thead>
          <tbody class="divide-y divide-zinc-800/60">
            <tr v-for="prod in products" :key="prod.id" class="hover:bg-zinc-800/40 transition">
              <td class="py-3 px-4">
                <p class="font-bold text-white text-sm">{{ prod.name }}</p>
                <p v-if="prod.description" class="text-zinc-400 text-[11px] mt-0.5 line-clamp-1">{{ prod.description }}</p>
              </td>
              <td class="py-3 px-4 font-mono text-zinc-400">
                {{ prod.sku || '—' }}
              </td>
              <td class="py-3 px-4">
                <span class="px-2 py-0.5 rounded bg-zinc-800 text-cyan-300 border border-zinc-700 font-medium">
                  {{ prod.category }}
                </span>
              </td>
              <td class="py-3 px-4 text-zinc-400">
                {{ formatCurrency(prod.costPrice) }}
              </td>
              <td class="py-3 px-4 font-extrabold text-amber-300 text-sm">
                {{ formatCurrency(prod.salePrice) }}
              </td>
              <td class="py-3 px-4">
                <div class="flex items-center gap-2">
                  <div class="flex items-center bg-zinc-900 border border-zinc-700 rounded-lg">
                    <button 
                      @click="adjustStock(prod, -1)" 
                      class="px-2 py-1 text-zinc-400 hover:text-white font-bold"
                    >
                      -
                    </button>
                    <span 
                      class="px-2 font-bold text-xs"
                      :class="prod.stockQuantity <= prod.lowStockAlert ? 'text-rose-400' : 'text-white'"
                    >
                      {{ prod.stockQuantity }}
                    </span>
                    <button 
                      @click="adjustStock(prod, 1)" 
                      class="px-2 py-1 text-zinc-400 hover:text-white font-bold"
                    >
                      +
                    </button>
                  </div>
                  <span 
                    v-if="prod.stockQuantity <= prod.lowStockAlert" 
                    class="text-[10px] font-bold text-rose-400 bg-rose-500/10 px-1.5 py-0.5 rounded border border-rose-500/20"
                  >
                    Sắp hết!
                  </span>
                </div>
              </td>
              <td class="py-3 px-4 text-right space-x-2">
                <button 
                  @click="openModal(prod)"
                  class="p-1.5 rounded-lg bg-zinc-800 hover:bg-zinc-700 text-zinc-300 hover:text-white transition"
                  title="Sửa"
                >
                  <Edit3 class="w-4 h-4" />
                </button>
                <button 
                  @click="deleteProduct(prod.id)"
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
          {{ editingId ? 'Chỉnh Sửa Sản Phẩm' : 'Thêm Sản Phẩm Mới' }}
        </h3>

        <form @submit.prevent="saveProduct" class="space-y-4 text-xs">
          <div>
            <label class="block text-zinc-400 mb-1 font-medium">Tên sản phẩm *</label>
            <input
              v-model="form.name"
              type="text"
              required
              placeholder="VD: Sáp Volcanic Clay V5"
              class="w-full bg-barber-dark border border-zinc-700 rounded-xl px-3 py-2 text-white focus:outline-none focus:border-barber-gold"
            />
          </div>

          <div class="grid grid-cols-2 gap-3">
            <div>
              <label class="block text-zinc-400 mb-1 font-medium">Mã SKU</label>
              <input
                v-model="form.sku"
                type="text"
                placeholder="VD: SAP-01"
                class="w-full bg-barber-dark border border-zinc-700 rounded-xl px-3 py-2 text-white focus:outline-none focus:border-barber-gold"
              />
            </div>
            <div>
              <label class="block text-zinc-400 mb-1 font-medium">Phân loại</label>
              <select
                v-model="form.category"
                class="w-full bg-barber-dark border border-zinc-700 rounded-xl px-3 py-2 text-white focus:outline-none focus:border-barber-gold"
              >
                <option value="Sáp vuốt tóc">Sáp vuốt tóc</option>
                <option value="Pomade">Pomade</option>
                <option value="Gôm xịt">Gôm xịt</option>
                <option value="Dưỡng tóc">Dưỡng tóc</option>
                <option value="Dầu gội/xả">Dầu gội/xả</option>
              </select>
            </div>
          </div>

          <div class="grid grid-cols-2 gap-3">
            <div>
              <label class="block text-zinc-400 mb-1 font-medium">Giá nhập (VNĐ)</label>
              <input
                v-model.number="form.costPrice"
                type="number"
                min="0"
                step="5000"
                class="w-full bg-barber-dark border border-zinc-700 rounded-xl px-3 py-2 text-white focus:outline-none focus:border-barber-gold"
              />
            </div>
            <div>
              <label class="block text-zinc-400 mb-1 font-medium">Giá bán lẻ (VNĐ) *</label>
              <input
                v-model.number="form.salePrice"
                type="number"
                min="0"
                step="5000"
                required
                class="w-full bg-barber-dark border border-zinc-700 rounded-xl px-3 py-2 text-white focus:outline-none focus:border-barber-gold"
              />
            </div>
          </div>

          <div class="grid grid-cols-2 gap-3">
            <div>
              <label class="block text-zinc-400 mb-1 font-medium">Số lượng tồn kho</label>
              <input
                v-model.number="form.stockQuantity"
                type="number"
                min="0"
                class="w-full bg-barber-dark border border-zinc-700 rounded-xl px-3 py-2 text-white focus:outline-none focus:border-barber-gold"
              />
            </div>
            <div>
              <label class="block text-zinc-400 mb-1 font-medium">Cảnh báo khi dưới</label>
              <input
                v-model.number="form.lowStockAlert"
                type="number"
                min="1"
                class="w-full bg-barber-dark border border-zinc-700 rounded-xl px-3 py-2 text-white focus:outline-none focus:border-barber-gold"
              />
            </div>
          </div>

          <div>
            <label class="block text-zinc-400 mb-1 font-medium">Mô tả đặc tính</label>
            <textarea
              v-model="form.description"
              rows="2"
              placeholder="Độ giữ nếp, độ bóng, mùi hương..."
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
              Lưu Sản Phẩm
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

const products = ref([])
const isModalOpen = ref(false)
const editingId = ref(null)

const form = ref({
  name: '',
  sku: '',
  costPrice: 0,
  salePrice: 200000,
  stockQuantity: 10,
  lowStockAlert: 3,
  category: 'Sáp vuốt tóc',
  description: '',
  isActive: true
})

async function loadProducts() {
  try {
    const res = await api.getProducts()
    products.value = res.data
  } catch (err) {
    console.error('Lỗi khi tải sản phẩm:', err)
  }
}

onMounted(() => {
  loadProducts()
})

function formatCurrency(val) {
  return new Intl.NumberFormat('vi-VN', { style: 'currency', currency: 'VND' }).format(val || 0)
}

function openModal(prod = null) {
  if (prod) {
    editingId.value = prod.id
    form.value = { ...prod }
  } else {
    editingId.value = null
    form.value = {
      name: '',
      sku: '',
      costPrice: 0,
      salePrice: 200000,
      stockQuantity: 10,
      lowStockAlert: 3,
      category: 'Sáp vuốt tóc',
      description: '',
      isActive: true
    }
  }
  isModalOpen.value = true
}

async function saveProduct() {
  try {
    if (editingId.value) {
      await api.updateProduct(editingId.value, { ...form.value, id: editingId.value })
    } else {
      await api.createProduct(form.value)
    }
    isModalOpen.value = false
    loadProducts()
  } catch (err) {
    alert('Lỗi lưu sản phẩm: ' + err.message)
  }
}

async function adjustStock(prod, delta) {
  const newStock = Math.max(0, prod.stockQuantity + delta)
  try {
    await api.updateStock(prod.id, newStock)
    prod.stockQuantity = newStock
  } catch (err) {
    alert('Không thể cập nhật kho: ' + err.message)
  }
}

async function deleteProduct(id) {
  if (!confirm('Bạn có chắc chắn muốn xóa sản phẩm này?')) return
  try {
    await api.deleteProduct(id)
    loadProducts()
  } catch (err) {
    alert('Không thể xóa: ' + err.message)
  }
}
</script>
