<template>
  <div class="h-screen flex flex-col lg:flex-row overflow-hidden bg-slate-50">
    <!-- CỘT TRÁI: DANH SÁCH DỊCH VỤ & SẢN PHẨM (62%) -->
    <div class="flex-1 flex flex-col border-r border-slate-200 overflow-hidden">
      <!-- Top header with search & tabs -->
      <div class="p-4 bg-white border-b border-slate-200 flex flex-col gap-3 shadow-2xs">
        <div class="flex items-center justify-between gap-4">
          <!-- Switch View Mode: Dịch vụ vs Sản phẩm -->
          <div class="flex bg-slate-100 p-1 rounded-xl border border-slate-200">
            <button
              @click="activeMainTab = 'services'"
              class="px-5 py-2 rounded-lg text-sm font-bold transition-all duration-200 flex items-center gap-2"
              :class="activeMainTab === 'services' ? 'bg-white text-indigo-600 shadow-sm' : 'text-slate-600 hover:text-slate-900'"
            >
              <Sparkles class="w-4 h-4" />
              Dịch Vụ ({{ services.length }})
            </button>
            <button
              @click="activeMainTab = 'products'"
              class="px-5 py-2 rounded-lg text-sm font-bold transition-all duration-200 flex items-center gap-2"
              :class="activeMainTab === 'products' ? 'bg-white text-indigo-600 shadow-sm' : 'text-slate-600 hover:text-slate-900'"
            >
              <Package class="w-4 h-4" />
              Sản Phẩm & Hàng Hóa ({{ products.length }})
            </button>
          </div>

          <!-- Quick Search Input -->
          <div class="relative w-72">
            <input
              v-model="searchQuery"
              type="text"
              placeholder="Tìm kiếm dịch vụ, sản phẩm..."
              class="w-full bg-slate-50 border border-slate-200 rounded-xl px-3.5 py-2 pl-9 text-xs text-slate-800 placeholder-slate-400 focus:outline-none focus:border-indigo-500 focus:bg-white transition"
            />
            <Search class="w-4 h-4 text-slate-400 absolute left-3 top-2.5" />
            <button 
              v-if="searchQuery" 
              @click="searchQuery = ''" 
              class="absolute right-3 top-2 text-slate-400 hover:text-slate-600 text-xs"
            >
              ✕
            </button>
          </div>
        </div>

        <!-- Sub-Category Filter Badges -->
        <div class="flex items-center gap-2 overflow-x-auto pb-1 text-xs no-scrollbar">
          <button
            @click="selectedCategory = 'All'"
            class="px-3.5 py-1.5 rounded-lg font-semibold transition whitespace-nowrap"
            :class="selectedCategory === 'All' ? 'bg-indigo-50 text-indigo-600 border border-indigo-200 font-bold shadow-2xs' : 'bg-white text-slate-600 border border-slate-200 hover:bg-slate-50 hover:text-slate-900'"
          >
            Tất cả
          </button>
          <button
            v-for="cat in availableCategories"
            :key="cat"
            @click="selectedCategory = cat"
            class="px-3.5 py-1.5 rounded-lg font-semibold transition whitespace-nowrap"
            :class="selectedCategory === cat ? 'bg-indigo-50 text-indigo-600 border border-indigo-200 font-bold shadow-2xs' : 'bg-white text-slate-600 border border-slate-200 hover:bg-slate-50 hover:text-slate-900'"
          >
            {{ cat }}
          </button>
        </div>
      </div>

      <!-- Items Grid Area -->
      <div class="flex-1 p-4 overflow-y-auto">
        <!-- Dịch Vụ List -->
        <div v-if="activeMainTab === 'services'" class="grid grid-cols-2 md:grid-cols-3 xl:grid-cols-3 gap-3.5">
          <div
            v-for="svc in filteredServices"
            :key="svc.id"
            @click="pos.addItem(svc, 'Service')"
            class="group bg-white hover:bg-slate-50/80 border border-slate-200 hover:border-indigo-300 rounded-2xl p-4 cursor-pointer transition-all duration-200 flex flex-col justify-between select-none active:scale-[0.98] shadow-2xs hover:shadow-md"
          >
            <div>
              <div class="flex justify-between items-start gap-2 mb-2">
                <span 
                  class="text-[11px] font-bold px-2 py-0.5 rounded border inline-flex items-center gap-1 shadow-2xs"
                  :style="{
                    backgroundColor: getCategoryColor(svc.category) + '15',
                    borderColor: getCategoryColor(svc.category) + '40',
                    color: getCategoryColor(svc.category)
                  }"
                >
                  <span class="w-1.5 h-1.5 rounded-full inline-block" :style="{ backgroundColor: getCategoryColor(svc.category) }"></span>
                  {{ svc.category }}
                </span>
                <span class="text-xs text-slate-400 flex items-center gap-1">
                  <Clock class="w-3 h-3" />
                  {{ svc.durationMinutes }}p
                </span>
              </div>
              <h4 class="font-bold text-slate-900 text-sm group-hover:text-indigo-600 transition line-clamp-2">
                {{ svc.name }}
              </h4>
              <p v-if="svc.description" class="text-xs text-slate-500 mt-1 line-clamp-2">
                {{ svc.description }}
              </p>
            </div>

            <div class="mt-4 pt-2.5 border-t border-slate-100 flex justify-between items-center">
              <span class="text-sm font-extrabold text-indigo-600">
                {{ formatCurrency(svc.price) }}
              </span>
              <div class="flex items-center gap-1.5">
                <span 
                  v-if="getItemQuantity(svc.id, 'Service') > 0" 
                  class="px-2 py-0.5 rounded-lg bg-indigo-600 text-white font-extrabold text-xs shadow-sm animate-fade-in"
                >
                  x{{ getItemQuantity(svc.id, 'Service') }}
                </span>
                <span class="w-7 h-7 rounded-lg bg-indigo-50 text-indigo-600 group-hover:bg-indigo-600 group-hover:text-white flex items-center justify-center font-bold text-sm transition shadow-2xs">
                  +
                </span>
              </div>
            </div>
          </div>
        </div>

        <!-- Sản Phẩm List -->
        <div v-if="activeMainTab === 'products'" class="grid grid-cols-2 md:grid-cols-3 xl:grid-cols-3 gap-3.5">
          <div
            v-for="prod in filteredProducts"
            :key="prod.id"
            @click="pos.addItem(prod, 'Product')"
            class="group bg-white hover:bg-slate-50/80 border border-slate-200 hover:border-indigo-300 rounded-2xl p-4 cursor-pointer transition-all duration-200 flex flex-col justify-between select-none active:scale-[0.98] shadow-2xs hover:shadow-md"
          >
            <div>
              <div class="flex justify-between items-start gap-2 mb-2">
                <span 
                  class="text-[11px] font-bold px-2 py-0.5 rounded border inline-flex items-center gap-1 shadow-2xs"
                  :style="{
                    backgroundColor: getProductCategoryColor(prod.category) + '15',
                    borderColor: getProductCategoryColor(prod.category) + '40',
                    color: getProductCategoryColor(prod.category)
                  }"
                >
                  <span class="w-1.5 h-1.5 rounded-full inline-block" :style="{ backgroundColor: getProductCategoryColor(prod.category) }"></span>
                  {{ prod.category }}
                </span>
                <span 
                  class="text-[11px] font-bold px-1.5 py-0.5 rounded"
                  :class="prod.stockQuantity <= prod.lowStockAlert ? 'bg-rose-50 text-rose-600 border border-rose-200' : 'bg-emerald-50 text-emerald-600 border border-emerald-200'"
                >
                  Kho: {{ prod.stockQuantity }}
                </span>
              </div>
              <h4 class="font-bold text-slate-900 text-sm group-hover:text-indigo-600 transition line-clamp-2">
                {{ prod.name }}
              </h4>
            </div>

            <div class="mt-4 pt-2.5 border-t border-slate-100 flex justify-between items-center">
              <span class="text-sm font-extrabold text-indigo-600">
                {{ formatCurrency(prod.salePrice) }}
              </span>
              <div class="flex items-center gap-1.5">
                <span 
                  v-if="getItemQuantity(prod.id, 'Product') > 0" 
                  class="px-2 py-0.5 rounded-lg bg-indigo-600 text-white font-extrabold text-xs shadow-sm animate-fade-in"
                >
                  x{{ getItemQuantity(prod.id, 'Product') }}
                </span>
                <span class="w-7 h-7 rounded-lg bg-indigo-50 text-indigo-600 group-hover:bg-indigo-600 group-hover:text-white flex items-center justify-center font-bold text-sm transition shadow-2xs">
                  +
                </span>
              </div>
            </div>
          </div>
        </div>

        <!-- Empty state when no items found -->
        <div v-if="(activeMainTab === 'services' && filteredServices.length === 0) || (activeMainTab === 'products' && filteredProducts.length === 0)" class="h-64 flex flex-col items-center justify-center text-slate-400 text-sm">
          <p>Không tìm thấy mục phù hợp với "{{ searchQuery }}"</p>
        </div>
      </div>
    </div>

    <!-- CỘT PHẢI: GIỎ HÀNG BÁN HÀNG & THANH TOÁN (38%) -->
    <div class="w-full lg:w-[420px] xl:w-[460px] bg-white flex flex-col justify-between h-full border-t lg:border-t-0 lg:border-l border-slate-200 shadow-md flex-shrink-0">
      
      <!-- Cart Header & Customer input -->
      <div class="p-4 border-b border-slate-200">
        <div class="flex items-center justify-between mb-3">
          <div class="flex items-center gap-2">
            <ShoppingCart class="w-5 h-5 text-indigo-600" />
            <h3 class="font-extrabold text-slate-900 text-base">Đơn Phục Vụ Hiện Tại</h3>
          </div>
          <button 
            v-if="pos.cart.length > 0" 
            @click="pos.clearCart" 
            class="text-xs text-rose-500 hover:text-rose-600 font-semibold transition"
          >
            Xóa trắng
          </button>
        </div>

        <!-- Quick Customer Name/Phone Inputs -->
        <div class="grid grid-cols-2 gap-2">
          <input
            v-model="pos.customerName"
            type="text"
            placeholder="Tên khách hàng"
            class="bg-slate-50 border border-slate-200 rounded-lg px-2.5 py-1.5 text-xs text-slate-800 placeholder-slate-400 focus:outline-none focus:border-indigo-500 focus:bg-white transition"
          />
          <input
            v-model="pos.customerPhone"
            type="text"
            placeholder="Số điện thoại"
            class="bg-slate-50 border border-slate-200 rounded-lg px-2.5 py-1.5 text-xs text-slate-800 placeholder-slate-400 focus:outline-none focus:border-indigo-500 focus:bg-white transition"
          />
        </div>
      </div>

      <!-- Cart Items List (Scrollable) -->
      <div class="flex-1 p-4 overflow-y-auto space-y-2.5">
        <div v-if="pos.cart.length === 0" class="h-44 flex flex-col items-center justify-center text-slate-400 text-xs">
          <Store class="w-8 h-8 text-slate-300 mb-2 stroke-1" />
          <p class="font-medium">Chưa chọn dịch vụ hay sản phẩm nào</p>
          <p class="text-[11px] text-slate-400 mt-1">Chạm vào dịch vụ hoặc sản phẩm bên trái để tính tiền</p>
        </div>

        <div
          v-for="(item, index) in pos.cart"
          :key="index"
          class="p-3 bg-slate-50 rounded-xl border border-slate-200/80 flex justify-between items-center gap-3 animate-fade-in text-xs"
        >
          <div class="flex-1 overflow-hidden">
            <div class="flex items-center gap-1.5 mb-1">
              <span
                class="px-1.5 py-0.5 rounded text-[10px] font-bold"
                :class="item.itemType === 'Service' ? 'bg-indigo-50 text-indigo-600' : 'bg-blue-50 text-blue-600'"
              >
                {{ item.itemType === 'Service' ? 'Dịch vụ' : 'Sản phẩm' }}
              </span>
              <p class="font-bold text-slate-900 truncate">{{ item.itemName }}</p>
            </div>
            <div class="flex items-center gap-2 flex-wrap">
              <p class="text-slate-500 font-medium">{{ formatCurrency(item.unitPrice) }}</p>
              <span v-if="item.quantity > 1" class="text-slate-500 font-medium">
                x {{ item.quantity }} = <span class="text-indigo-600 font-bold">{{ formatCurrency(item.unitPrice * item.quantity) }}</span>
              </span>
            </div>
          </div>

          <!-- Quantity Controls for Services & Products -->
          <div class="flex items-center gap-2">
            <div class="flex items-center bg-white rounded-lg p-0.5 border border-slate-200 shadow-2xs">
              <button 
                @click="pos.updateQuantity(index, -1)" 
                class="w-6 h-6 flex items-center justify-center text-slate-600 hover:text-slate-900 font-bold transition hover:bg-slate-100 rounded"
                title="Giảm 1"
              >
                -
              </button>
              <span class="w-6 text-center font-bold text-slate-900">{{ item.quantity }}</span>
              <button 
                @click="pos.updateQuantity(index, 1)" 
                class="w-6 h-6 flex items-center justify-center text-slate-600 hover:text-slate-900 font-bold transition hover:bg-slate-100 rounded"
                title="Tăng 1"
              >
                +
              </button>
            </div>

            <button 
              @click="pos.removeItem(index)" 
              class="text-slate-400 hover:text-rose-500 p-1" 
              title="Xóa"
            >
              ✕
            </button>
          </div>
        </div>
      </div>

      <!-- Calculation & Discounts & Note & Checkout Section -->
      <div class="p-4 bg-white border-t border-slate-200 space-y-3 shadow-lg">
        
        <!-- Mục GIẢM GIÁ (%) THEO YÊU CẦU -->
        <div class="bg-slate-50 p-2.5 rounded-xl border border-slate-200">
          <div class="flex justify-between items-center mb-1.5">
            <span class="text-xs font-semibold text-slate-700 flex items-center gap-1.5">
              <Tag class="w-3.5 h-3.5 text-indigo-600" />
              Giảm giá (%):
            </span>
            <span v-if="pos.discountPercent > 0" class="text-xs text-emerald-600 font-bold">
              -{{ formatCurrency(pos.discountAmount) }} ({{ pos.discountPercent }}%)
            </span>
          </div>
          <!-- Quick percent discount buttons -->
          <div class="flex items-center gap-1.5">
            <button
              v-for="p in [0, 5, 10, 15, 20]"
              :key="p"
              @click="pos.setDiscount(p)"
              class="flex-1 py-1 rounded text-xs font-bold transition"
              :class="pos.discountPercent === p ? 'bg-indigo-600 text-white shadow-2xs' : 'bg-white border border-slate-200 text-slate-600 hover:text-slate-900 hover:bg-slate-100'"
            >
              {{ p }}%
            </button>
            <div class="w-16 relative">
              <input
                v-model.number="pos.discountPercent"
                type="number"
                min="0"
                max="100"
                placeholder="Khác"
                class="w-full bg-white border border-slate-200 rounded py-1 px-1.5 text-xs text-center text-slate-800 focus:outline-none focus:border-indigo-500"
              />
            </div>
          </div>
        </div>

        <!-- Mục GHI CHÚ ĐƠN HÀNG -->
        <div>
          <div class="relative">
            <input
              v-model="pos.note"
              type="text"
              placeholder="Ghi chú đơn hàng (nếu có)..."
              class="w-full bg-slate-50 border border-slate-200 rounded-xl px-3 py-2 text-xs text-slate-800 placeholder-slate-400 focus:outline-none focus:border-indigo-500 focus:bg-white transition"
            />
          </div>
        </div>

        <!-- Total Price Summary -->
        <div class="pt-2 border-t border-slate-100 space-y-1.5 text-xs">
          <div class="flex justify-between text-slate-500">
            <span>Tạm tính:</span>
            <span>{{ formatCurrency(pos.subTotal) }}</span>
          </div>
          <div v-if="pos.discountPercent > 0" class="flex justify-between text-emerald-600 font-medium">
            <span>Đã giảm ({{ pos.discountPercent }}%):</span>
            <span>-{{ formatCurrency(pos.discountAmount) }}</span>
          </div>
          <div class="flex justify-between items-baseline pt-1">
            <span class="text-sm font-bold text-slate-800">Khách cần trả:</span>
            <span class="text-2xl font-black text-indigo-600 tracking-tight">
              {{ formatCurrency(pos.finalAmount) }}
            </span>
          </div>
        </div>

        <!-- Checkout Buttons -->
        <div class="grid grid-cols-2 gap-2 pt-1">
          <!-- Nút VietQR (Ưu tiên) -->
          <button
            @click="handleCheckout('VietQR')"
            :disabled="pos.cart.length === 0 || pos.isLoading"
            class="py-3 px-3 rounded-xl bg-gradient-to-r from-indigo-600 to-blue-600 hover:from-indigo-500 hover:to-blue-500 disabled:opacity-50 disabled:cursor-not-allowed text-white font-extrabold text-xs sm:text-sm shadow-md shadow-indigo-500/25 transition flex items-center justify-center gap-1.5 active:scale-95"
          >
            <QrCode class="w-4 h-4" />
            <span>Mã VietQR</span>
          </button>

          <!-- Nút Tiền mặt -->
          <button
            @click="handleCheckout('Cash')"
            :disabled="pos.cart.length === 0 || pos.isLoading"
            class="py-3 px-3 rounded-xl bg-slate-100 hover:bg-slate-200 disabled:opacity-50 disabled:cursor-not-allowed text-slate-800 font-bold text-xs sm:text-sm border border-slate-200 transition flex items-center justify-center gap-1.5 active:scale-95"
          >
            <Banknote class="w-4 h-4 text-emerald-600" />
            <span>Tiền Mặt</span>
          </button>
        </div>

      </div>

    </div>

    <!-- Popup Modal VietQR -->
    <VietQrModal
      :isOpen="isQrModalOpen"
      :qrData="currentVietQr"
      @close="isQrModalOpen = false"
      @complete="onQrPaymentComplete"
    />

  </div>
</template>

<script setup>
import { ref, computed, onMounted } from 'vue'
import { usePosStore } from '@/stores/posStore'
import api from '@/services/api'
import { useNotify } from '@/composables/useNotify'
import VietQrModal from '@/components/VietQrModal.vue'
import { 
  Sparkles,
  Package, 
  Search, 
  Clock, 
  ShoppingCart, 
  Tag, 
  QrCode, 
  Banknote,
  Store 
} from 'lucide-vue-next'

const { toast: notify } = useNotify()

const pos = usePosStore()

const services = ref([])
const products = ref([])
const activeMainTab = ref('services')
const selectedCategory = ref('All')
const searchQuery = ref('')

const isQrModalOpen = ref(false)
const currentVietQr = ref(null)
const currentCreatedOrder = ref(null)

const serviceCategories = ref([])
const productCategories = ref([])

function getCategoryColor(catName) {
  const found = serviceCategories.value.find(c => c.name === catName)
  if (found?.color) return found.color
  
  switch (catName) {
    case 'Dịch vụ chính': return '#4f46e5'
    case 'Combo': return '#8b5cf6'
    case 'Chăm sóc': return '#10b981'
    case 'Thư giãn': return '#06b6d4'
    default: return '#4f46e5'
  }
}

function getProductCategoryColor(catName) {
  const found = productCategories.value.find(c => c.name === catName)
  if (found?.color) return found.color
  
  switch (catName) {
    case 'Mỹ phẩm': return '#06b6d4'
    case 'Dụng cụ': return '#f59e0b'
    case 'Phụ kiện': return '#8b5cf6'
    default: return '#4f46e5'
  }
}

const availableCategories = computed(() => {
  if (activeMainTab.value === 'services') {
    return Array.from(new Set(services.value.map(s => s.category).filter(Boolean)))
  } else {
    return Array.from(new Set(products.value.map(p => p.category).filter(Boolean)))
  }
})

const filteredServices = computed(() => {
  return services.value.filter(s => {
    const matchCat = selectedCategory.value === 'All' || s.category === selectedCategory.value
    const matchSearch = !searchQuery.value || s.name.toLowerCase().includes(searchQuery.value.toLowerCase())
    return matchCat && matchSearch
  })
})

const filteredProducts = computed(() => {
  return products.value.filter(p => {
    // Chỉ lấy sản phẩm được đánh dấu bán tại POS
    if (p.showOnPos === false) return false

    const matchCat = selectedCategory.value === 'All' || p.category === selectedCategory.value
    const matchSearch = !searchQuery.value || p.name.toLowerCase().includes(searchQuery.value.toLowerCase())
    return matchCat && matchSearch
  })
})

function getItemQuantity(id, type) {
  const found = pos.cart.find(item => item.itemId === id && item.itemType === type)
  return found ? found.quantity : 0
}

function formatCurrency(val) {
  return new Intl.NumberFormat('vi-VN', { style: 'currency', currency: 'VND' }).format(val || 0)
}

function showToast(msg) {
  notify.success(msg)
}

async function loadData() {
  try {
    const [svcRes, prodRes, svcCatRes, prodCatRes] = await Promise.all([
      api.getServices(),
      api.getProducts({ onlyPos: true }),
      api.getServiceCategories(),
      api.getProductCategories()
    ])
    services.value = svcRes.data.filter(s => s.isActive)
    products.value = prodRes.data.filter(p => p.isActive)
    serviceCategories.value = svcCatRes.data || []
    productCategories.value = prodCatRes.data || []
  } catch (err) {
    console.error('Lỗi tải dữ liệu POS:', err)
  }
}

async function handleCheckout(method) {
  if (pos.cart.length === 0 || pos.isLoading) return

  try {
    pos.isLoading = true
    if (method === 'Cash') {
      const res = await pos.checkout('Cash')
      const order = res.order || res
      pos.clearCart()
      showToast(`Tạo đơn ${order.orderCode} tiền mặt thành công!`)
      await loadData()
    } else if (method === 'VietQR') {
      const res = await pos.checkout('VietQR')
      const order = res.order || res
      currentCreatedOrder.value = order

      if (res.vietQr) {
        currentVietQr.value = res.vietQr
      } else {
        const qrRes = await api.generateVietQr(order.finalAmount, order.orderCode, order.customerName)
        currentVietQr.value = qrRes.data
      }

      isQrModalOpen.value = true
      pos.clearCart()
    }
  } catch (err) {
    notify.error(`Lỗi tạo đơn (${method}): ` + err.message)
  } finally {
    pos.isLoading = false
  }
}

async function onQrPaymentComplete() {
  if (currentCreatedOrder.value) {
    showToast(`Đã xác nhận thanh toán đơn ${currentCreatedOrder.value.orderCode}!`)
    isQrModalOpen.value = false
    currentCreatedOrder.value = null
    currentVietQr.value = null
    pos.clearCart()
    await loadData()
  }
}

onMounted(() => {
  loadData()
})
</script>
