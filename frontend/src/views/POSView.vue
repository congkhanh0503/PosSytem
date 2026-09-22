<template>
  <div class="h-screen flex flex-col lg:flex-row overflow-hidden bg-barber-dark">
    <!-- CỘT TRÁI: DANH SÁCH DỊCH VỤ & SẢN PHẨM (62%) -->
    <div class="flex-1 flex flex-col border-r border-barber-border overflow-hidden">
      <!-- Top header with search & tabs -->
      <div class="p-4 bg-barber-card/80 border-b border-barber-border/80 flex flex-col gap-3">
        <div class="flex items-center justify-between gap-4">
          <!-- Switch View Mode: Dịch vụ vs Sản phẩm -->
          <div class="flex bg-zinc-900 p-1 rounded-xl border border-zinc-800">
            <button
              @click="activeMainTab = 'services'"
              class="px-5 py-2 rounded-lg text-sm font-bold transition-all duration-200 flex items-center gap-2"
              :class="activeMainTab === 'services' ? 'bg-barber-gold text-black shadow-md' : 'text-zinc-400 hover:text-white'"
            >
              <Scissors class="w-4 h-4" />
              Dịch Vụ Cắt Tóc ({{ services.length }})
            </button>
            <button
              @click="activeMainTab = 'products'"
              class="px-5 py-2 rounded-lg text-sm font-bold transition-all duration-200 flex items-center gap-2"
              :class="activeMainTab === 'products' ? 'bg-barber-gold text-black shadow-md' : 'text-zinc-400 hover:text-white'"
            >
              <Package class="w-4 h-4" />
              Sản Phẩm & Sáp Gôm ({{ products.length }})
            </button>
          </div>

          <!-- Quick Search Input -->
          <div class="relative w-72">
            <input
              v-model="searchQuery"
              type="text"
              placeholder="Tìm dịch vụ, sáp, gôm..."
              class="w-full bg-barber-dark border border-zinc-700/80 rounded-xl px-3.5 py-2 pl-9 text-xs text-white placeholder-zinc-500 focus:outline-none focus:border-barber-gold"
            />
            <Search class="w-4 h-4 text-zinc-500 absolute left-3 top-2.5" />
            <button 
              v-if="searchQuery" 
              @click="searchQuery = ''" 
              class="absolute right-3 top-2 text-zinc-400 hover:text-white text-xs"
            >
              ✕
            </button>
          </div>
        </div>

        <!-- Sub-Category Filter Badges -->
        <div class="flex items-center gap-2 overflow-x-auto pb-1 text-xs no-scrollbar">
          <button
            @click="selectedCategory = 'All'"
            class="px-3.5 py-1.5 rounded-lg font-medium transition whitespace-nowrap"
            :class="selectedCategory === 'All' ? 'bg-amber-500/20 text-barber-gold border border-amber-500/40 font-bold' : 'bg-zinc-800/60 text-zinc-400 hover:bg-zinc-800 hover:text-white'"
          >
            Tất cả
          </button>
          <button
            v-for="cat in availableCategories"
            :key="cat"
            @click="selectedCategory = cat"
            class="px-3.5 py-1.5 rounded-lg font-medium transition whitespace-nowrap"
            :class="selectedCategory === cat ? 'bg-amber-500/20 text-barber-gold border border-amber-500/40 font-bold' : 'bg-zinc-800/60 text-zinc-400 hover:bg-zinc-800 hover:text-white'"
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
            class="group bg-barber-card hover:bg-zinc-800/90 border border-barber-border hover:border-barber-gold/50 rounded-2xl p-4 cursor-pointer transition-all duration-200 flex flex-col justify-between select-none active:scale-[0.98] shadow-sm hover:shadow-lg"
          >
            <div>
              <div class="flex justify-between items-start gap-2 mb-2">
                <span class="text-[11px] font-semibold px-2 py-0.5 rounded bg-zinc-800 text-zinc-300 border border-zinc-700">
                  {{ svc.category }}
                </span>
                <span class="text-xs text-zinc-500 flex items-center gap-1">
                  <Clock class="w-3 h-3" />
                  {{ svc.durationMinutes }}p
                </span>
              </div>
              <h4 class="font-bold text-white text-sm group-hover:text-barber-gold transition line-clamp-2">
                {{ svc.name }}
              </h4>
              <p v-if="svc.description" class="text-xs text-zinc-400 mt-1 line-clamp-2">
                {{ svc.description }}
              </p>
            </div>

            <div class="mt-4 pt-2.5 border-t border-zinc-800/80 flex justify-between items-center">
              <span class="text-sm font-extrabold text-amber-300">
                {{ formatCurrency(svc.price) }}
              </span>
              <span class="w-7 h-7 rounded-lg bg-amber-500/10 text-barber-gold group-hover:bg-barber-gold group-hover:text-black flex items-center justify-center font-bold text-sm transition">
                +
              </span>
            </div>
          </div>
        </div>

        <!-- Sản Phẩm List -->
        <div v-if="activeMainTab === 'products'" class="grid grid-cols-2 md:grid-cols-3 xl:grid-cols-3 gap-3.5">
          <div
            v-for="prod in filteredProducts"
            :key="prod.id"
            @click="pos.addItem(prod, 'Product')"
            class="group bg-barber-card hover:bg-zinc-800/90 border border-barber-border hover:border-barber-gold/50 rounded-2xl p-4 cursor-pointer transition-all duration-200 flex flex-col justify-between select-none active:scale-[0.98] shadow-sm hover:shadow-lg"
          >
            <div>
              <div class="flex justify-between items-start gap-2 mb-2">
                <span class="text-[11px] font-semibold px-2 py-0.5 rounded bg-zinc-800 text-cyan-300 border border-zinc-700">
                  {{ prod.category }}
                </span>
                <span 
                  class="text-[11px] font-bold px-1.5 py-0.5 rounded"
                  :class="prod.stockQuantity <= prod.lowStockAlert ? 'bg-rose-500/20 text-rose-300' : 'bg-emerald-500/10 text-emerald-400'"
                >
                  Kho: {{ prod.stockQuantity }}
                </span>
              </div>
              <h4 class="font-bold text-white text-sm group-hover:text-barber-gold transition line-clamp-2">
                {{ prod.name }}
              </h4>
              <p v-if="prod.sku" class="text-[11px] font-mono text-zinc-500 mt-0.5">
                SKU: {{ prod.sku }}
              </p>
            </div>

            <div class="mt-4 pt-2.5 border-t border-zinc-800/80 flex justify-between items-center">
              <span class="text-sm font-extrabold text-amber-300">
                {{ formatCurrency(prod.salePrice) }}
              </span>
              <span class="w-7 h-7 rounded-lg bg-amber-500/10 text-barber-gold group-hover:bg-barber-gold group-hover:text-black flex items-center justify-center font-bold text-sm transition">
                +
              </span>
            </div>
          </div>
        </div>

        <!-- Empty state when no items found -->
        <div v-if="(activeMainTab === 'services' && filteredServices.length === 0) || (activeMainTab === 'products' && filteredProducts.length === 0)" class="h-64 flex flex-col items-center justify-center text-zinc-500 text-sm">
          <p>Không tìm thấy mục phù hợp với "{{ searchQuery }}"</p>
        </div>
      </div>
    </div>

    <!-- CỘT PHẢI: GIỎ HÀNG BÁN HÀNG & THANH TOÁN (38%) -->
    <div class="w-full lg:w-[420px] xl:w-[460px] bg-barber-card flex flex-col justify-between h-full border-t lg:border-t-0 shadow-2xl flex-shrink-0">
      
      <!-- Cart Header & Customer input -->
      <div class="p-4 border-b border-barber-border">
        <div class="flex items-center justify-between mb-3">
          <div class="flex items-center gap-2">
            <ShoppingCart class="w-5 h-5 text-barber-gold" />
            <h3 class="font-bold text-white text-base">Đơn Phục Vụ Hiện Tại</h3>
          </div>
          <button 
            v-if="pos.cart.length > 0" 
            @click="pos.clearCart" 
            class="text-xs text-rose-400 hover:text-rose-300 transition"
          >
            Xóa trắng
          </button>
        </div>

        <!-- Quick Customer Name/Phone Inputs -->
        <div class="grid grid-cols-2 gap-2">
          <input
            v-model="pos.customerName"
            type="text"
            placeholder="Tên khách (VD: Anh Tuấn)"
            class="bg-barber-dark border border-zinc-800 rounded-lg px-2.5 py-1.5 text-xs text-white placeholder-zinc-500 focus:outline-none focus:border-barber-gold"
          />
          <input
            v-model="pos.customerPhone"
            type="text"
            placeholder="SĐT khách (nếu có)"
            class="bg-barber-dark border border-zinc-800 rounded-lg px-2.5 py-1.5 text-xs text-white placeholder-zinc-500 focus:outline-none focus:border-barber-gold"
          />
        </div>
      </div>

      <!-- Cart Items List (Scrollable) -->
      <div class="flex-1 p-4 overflow-y-auto space-y-2.5">
        <div v-if="pos.cart.length === 0" class="h-44 flex flex-col items-center justify-center text-zinc-500 text-xs">
          <Scissors class="w-8 h-8 text-zinc-600 mb-2 stroke-1" />
          <p>Chưa chọn dịch vụ hay sản phẩm nào</p>
          <p class="text-[11px] text-zinc-600 mt-1">Chạm vào dịch vụ bên trái để tính tiền</p>
        </div>

        <div
          v-for="(item, index) in pos.cart"
          :key="index"
          class="p-3 bg-barber-dark/70 rounded-xl border border-barber-border/80 flex justify-between items-center gap-3 animate-fade-in text-xs"
        >
          <div class="flex-1 overflow-hidden">
            <div class="flex items-center gap-1.5 mb-1">
              <span
                class="px-1.5 py-0.5 rounded text-[10px] font-bold"
                :class="item.itemType === 'Service' ? 'bg-amber-500/20 text-barber-gold' : 'bg-cyan-500/20 text-cyan-400'"
              >
                {{ item.itemType === 'Service' ? 'Dịch vụ' : 'Sản phẩm' }}
              </span>
              <p class="font-bold text-white truncate">{{ item.itemName }}</p>
            </div>
            <p class="text-zinc-400 font-medium">{{ formatCurrency(item.unitPrice) }}</p>
          </div>

          <!-- Quantity Controls for Products -->
          <div class="flex items-center gap-2">
            <div v-if="item.itemType === 'Product'" class="flex items-center bg-zinc-800 rounded-lg p-0.5 border border-zinc-700">
              <button 
                @click="pos.updateQuantity(index, -1)" 
                class="w-6 h-6 flex items-center justify-center text-zinc-300 hover:text-white font-bold"
              >
                -
              </button>
              <span class="w-6 text-center font-bold text-white">{{ item.quantity }}</span>
              <button 
                @click="pos.updateQuantity(index, 1)" 
                class="w-6 h-6 flex items-center justify-center text-zinc-300 hover:text-white font-bold"
              >
                +
              </button>
            </div>

            <button 
              @click="pos.removeItem(index)" 
              class="text-zinc-500 hover:text-rose-400 p-1" 
              title="Xóa"
            >
              ✕
            </button>
          </div>
        </div>
      </div>

      <!-- Calculation & Discounts & Note & Checkout Section -->
      <div class="p-4 bg-barber-card border-t border-barber-border space-y-3">
        
        <!-- Mục GIẢM GIÁ (%) THEO YÊU CẦU -->
        <div class="bg-barber-dark/50 p-2.5 rounded-xl border border-zinc-800/80">
          <div class="flex justify-between items-center mb-1.5">
            <span class="text-xs font-semibold text-zinc-300 flex items-center gap-1.5">
              <Tag class="w-3.5 h-3.5 text-barber-gold" />
              Giảm giá (%):
            </span>
            <span v-if="pos.discountPercent > 0" class="text-xs text-emerald-400 font-bold">
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
              :class="pos.discountPercent === p ? 'bg-barber-gold text-black' : 'bg-zinc-800 text-zinc-400 hover:text-white'"
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
                class="w-full bg-zinc-800 border border-zinc-700 rounded py-1 px-1.5 text-xs text-center text-white focus:outline-none focus:border-barber-gold"
              />
            </div>
          </div>
        </div>

        <!-- Mục GHI CHÚ ĐƠN HÀNG THEO YÊU CẦU -->
        <div>
          <div class="relative">
            <input
              v-model="pos.note"
              type="text"
              placeholder="Ghi chú (VD: Khách quen anh Tuấn, nhuộm nâu tây...)"
              class="w-full bg-barber-dark border border-zinc-800 rounded-xl px-3 py-2 text-xs text-white placeholder-zinc-500 focus:outline-none focus:border-barber-gold"
            />
          </div>
        </div>

        <!-- Total Price Summary -->
        <div class="pt-2 border-t border-zinc-800/80 space-y-1.5 text-xs">
          <div class="flex justify-between text-zinc-400">
            <span>Tạm tính:</span>
            <span>{{ formatCurrency(pos.subTotal) }}</span>
          </div>
          <div v-if="pos.discountPercent > 0" class="flex justify-between text-emerald-400 font-medium">
            <span>Đã giảm ({{ pos.discountPercent }}%):</span>
            <span>-{{ formatCurrency(pos.discountAmount) }}</span>
          </div>
          <div class="flex justify-between items-baseline pt-1">
            <span class="text-sm font-bold text-white">Khách cần trả:</span>
            <span class="text-xl font-extrabold text-barber-gold tracking-tight">
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
            class="py-3 px-3 rounded-xl bg-gradient-to-r from-amber-500 to-amber-600 hover:from-amber-400 hover:to-amber-500 disabled:opacity-50 disabled:cursor-not-allowed text-black font-extrabold text-xs sm:text-sm shadow-lg shadow-amber-500/20 transition flex items-center justify-center gap-1.5 active:scale-95"
          >
            <QrCode class="w-4 h-4" />
            <span>Mã VietQR</span>
          </button>

          <!-- Nút Tiền mặt -->
          <button
            @click="handleCheckout('Cash')"
            :disabled="pos.cart.length === 0 || pos.isLoading"
            class="py-3 px-3 rounded-xl bg-zinc-800 hover:bg-zinc-700 disabled:opacity-50 disabled:cursor-not-allowed text-white font-bold text-xs sm:text-sm border border-zinc-700 transition flex items-center justify-center gap-1.5 active:scale-95"
          >
            <Banknote class="w-4 h-4 text-emerald-400" />
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

    <!-- Quick Toast / Notification -->
    <div 
      v-if="toast" 
      class="fixed bottom-6 right-6 z-50 bg-emerald-600 text-white font-bold text-xs px-4 py-3 rounded-xl shadow-2xl flex items-center gap-2 animate-bounce"
    >
      <span>✓ {{ toast }}</span>
    </div>

  </div>
</template>

<script setup>
import { ref, computed, onMounted } from 'vue'
import { usePosStore } from '@/stores/posStore'
import api from '@/services/api'
import VietQrModal from '@/components/VietQrModal.vue'
import { 
  Scissors, 
  Package, 
  Search, 
  Clock, 
  ShoppingCart, 
  Tag, 
  QrCode, 
  Banknote 
} from 'lucide-vue-next'

const pos = usePosStore()

const services = ref([])
const products = ref([])
const activeMainTab = ref('services')
const selectedCategory = ref('All')
const searchQuery = ref('')

const isQrModalOpen = ref(false)
const currentVietQr = ref(null)
const currentCreatedOrder = ref(null)
const toast = ref('')

// Load Services & Products
async function loadData() {
  try {
    const [svcRes, prodRes] = await Promise.all([
      api.getServices(true),
      api.getProducts(true)
    ])
    services.value = svcRes.data
    products.value = prodRes.data
  } catch (err) {
    console.error('Lỗi khi tải dữ liệu dịch vụ/sản phẩm:', err)
  }
}

onMounted(() => {
  loadData()
})

// Categories
const availableCategories = computed(() => {
  if (activeMainTab.value === 'services') {
    return [...new Set(services.value.map(s => s.category))]
  } else {
    return [...new Set(products.value.map(p => p.category))]
  }
})

// Filtered Lists
const filteredServices = computed(() => {
  return services.value.filter(s => {
    const matchCat = selectedCategory.value === 'All' || s.category === selectedCategory.value
    const matchQuery = !searchQuery.value.trim() || 
      s.name.toLowerCase().includes(searchQuery.value.toLowerCase()) ||
      (s.description && s.description.toLowerCase().includes(searchQuery.value.toLowerCase()))
    return matchCat && matchQuery
  })
})

const filteredProducts = computed(() => {
  return products.value.filter(p => {
    const matchCat = selectedCategory.value === 'All' || p.category === selectedCategory.value
    const matchQuery = !searchQuery.value.trim() || 
      p.name.toLowerCase().includes(searchQuery.value.toLowerCase()) ||
      (p.sku && p.sku.toLowerCase().includes(searchQuery.value.toLowerCase()))
    return matchCat && matchQuery
  })
})

function formatCurrency(val) {
  return new Intl.NumberFormat('vi-VN', { style: 'currency', currency: 'VND' }).format(val || 0)
}

function showToast(msg) {
  toast.value = msg
  setTimeout(() => {
    toast.value = ''
  }, 3500)
}

// Handle Checkout
async function handleCheckout(method) {
  if (pos.cart.length === 0) return

  try {
    const result = await pos.checkout(method)
    currentCreatedOrder.value = result.order

    if (method === 'VietQR' && result.vietQr) {
      currentVietQr.value = result.vietQr
      isQrModalOpen.value = true
    } else {
      // Tiền mặt: Hoàn tất ngay
      showToast(`Đã thanh toán đơn ${result.order.orderCode} (Tiền mặt)!`)
      pos.clearCart()
      loadData() // Refresh tồn kho
    }
  } catch (err) {
    alert('Không thể tạo đơn hàng: ' + (err.response?.data || err.message))
  }
}

// When user clicks "Xác nhận đã nhận tiền" in QR Modal
function onQrPaymentComplete() {
  isQrModalOpen.value = false
  showToast(`Đã nhận tiền VietQR cho đơn ${currentCreatedOrder.value?.orderCode}!`)
  pos.clearCart()
  loadData() // Refresh tồn kho
}
</script>
