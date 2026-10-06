<template>
  <div 
    v-if="isOpen" 
    class="fixed inset-0 z-50 flex items-center justify-center p-3 sm:p-4 bg-slate-900/60 backdrop-blur-sm animate-fade-in"
    @keydown.esc="handleClose"
    @keydown.enter.prevent="handleConfirm"
    tabindex="0"
  >
    <div 
      class="relative w-full max-w-lg bg-white border border-slate-200 rounded-3xl shadow-2xl overflow-hidden flex flex-col max-h-[92vh] animate-scale-up"
      @click.stop
    >
      <!-- Modal Header -->
      <div class="px-5 py-4 border-b border-slate-100 flex items-center justify-between bg-slate-50/70">
        <div class="flex items-center gap-2.5">
          <div 
            class="w-10 h-10 rounded-2xl flex items-center justify-center shadow-xs"
            :class="selectedMethod === 'Cash' ? 'bg-emerald-100 text-emerald-600' : 'bg-indigo-100 text-indigo-600'"
          >
            <Banknote v-if="selectedMethod === 'Cash'" class="w-5 h-5" />
            <QrCode v-else class="w-5 h-5" />
          </div>
          <div>
            <h3 class="text-base sm:text-lg font-black text-slate-900 leading-tight">
              Xác Nhận Đơn Hàng
            </h3>
            <p class="text-xs text-slate-500">
              Kiểm tra thông tin trước khi hoàn tất thanh toán
            </p>
          </div>
        </div>

        <button 
          @click="handleClose"
          class="w-8 h-8 rounded-xl text-slate-400 hover:text-slate-700 hover:bg-slate-200/60 flex items-center justify-center transition"
          title="Đóng (Esc)"
        >
          <X class="w-5 h-5" />
        </button>
      </div>

      <!-- Payment Method Switcher Tabs -->
      <div class="px-5 pt-3 pb-1">
        <div class="grid grid-cols-2 gap-2 bg-slate-100/80 p-1 rounded-2xl">
          <button
            type="button"
            @click="selectedMethod = 'Cash'"
            class="py-2 px-3 rounded-xl text-xs font-bold transition flex items-center justify-center gap-2 shadow-2xs"
            :class="selectedMethod === 'Cash' 
              ? 'bg-white text-emerald-700 shadow-sm border border-slate-200/80' 
              : 'text-slate-600 hover:text-slate-900'"
          >
            <Banknote class="w-4 h-4 text-emerald-600" />
            <span>Tiền Mặt</span>
          </button>

          <button
            type="button"
            @click="selectedMethod = 'VietQR'"
            class="py-2 px-3 rounded-xl text-xs font-bold transition flex items-center justify-center gap-2 shadow-2xs"
            :class="selectedMethod === 'VietQR' 
              ? 'bg-white text-indigo-700 shadow-sm border border-slate-200/80' 
              : 'text-slate-600 hover:text-slate-900'"
          >
            <QrCode class="w-4 h-4 text-indigo-600" />
            <span>Mã VietQR</span>
          </button>
        </div>
      </div>

      <!-- Scrollable Content: Customer, Items, Calculation, Cash change -->
      <div class="px-5 py-3 overflow-y-auto space-y-3.5 flex-1 text-xs">
        
        <!-- Customer & Note Info Card -->
        <div class="p-3 bg-slate-50 rounded-2xl border border-slate-200/80 space-y-1.5">
          <div class="flex items-center justify-between">
            <span class="text-slate-500 flex items-center gap-1.5 font-medium">
              <User class="w-3.5 h-3.5 text-slate-400" />
              Khách hàng:
            </span>
            <span class="font-bold text-slate-800">
              {{ customerName ? customerName : 'Khách vãng lai' }}
              <span v-if="customerPhone" class="font-normal text-slate-500">({{ customerPhone }})</span>
            </span>
          </div>

          <div v-if="note" class="pt-1.5 border-t border-slate-200/70 flex items-start justify-between gap-2">
            <span class="text-slate-500 flex items-center gap-1.5 font-medium shrink-0">
              <FileText class="w-3.5 h-3.5 text-slate-400" />
              Ghi chú:
            </span>
            <span class="text-slate-700 italic text-right font-medium break-words">
              {{ note }}
            </span>
          </div>
        </div>

        <!-- Items List Section -->
        <div>
          <div class="flex items-center justify-between mb-2">
            <span class="font-bold text-slate-700 uppercase tracking-wider text-[11px] flex items-center gap-1.5">
              <ShoppingCart class="w-3.5 h-3.5 text-indigo-600" />
              Danh Sách Món ({{ cart.length }})
            </span>
            <span class="text-slate-400 text-[11px]">
              Tổng {{ totalQuantity }} món
            </span>
          </div>

          <div class="space-y-1.5 max-h-48 overflow-y-auto pr-0.5">
            <div 
              v-for="(item, idx) in cart" 
              :key="idx"
              class="p-2.5 rounded-xl bg-white border border-slate-200/70 flex justify-between items-center gap-2"
              :class="item.itemType === 'Product' && item.stockQuantity !== null && (item.stockQuantity <= 0 || item.quantity > item.stockQuantity) ? 'border-rose-300 bg-rose-50/50' : ''"
            >
              <div class="flex-1 overflow-hidden">
                <div class="flex items-center gap-1.5">
                  <span 
                    class="px-1.5 py-0.5 rounded text-[10px] font-bold shrink-0"
                    :class="item.itemType === 'Service' ? 'bg-indigo-50 text-indigo-600 border border-indigo-100' : 'bg-blue-50 text-blue-600 border border-blue-100'"
                  >
                    {{ item.itemType === 'Service' ? 'Dịch vụ' : 'Sản phẩm' }}
                  </span>
                  <p class="font-bold text-slate-900 truncate">{{ item.itemName }}</p>
                </div>
                <div class="text-[11px] text-slate-500 mt-0.5 flex items-center gap-1.5">
                  <span>{{ formatCurrency(item.unitPrice) }} x {{ item.quantity }}</span>
                  <span 
                    v-if="item.itemType === 'Product' && item.stockQuantity !== null && item.stockQuantity !== undefined"
                    class="text-[10px] font-bold px-1.5 py-0.2 rounded"
                    :class="item.stockQuantity <= 0 || item.quantity > item.stockQuantity ? 'bg-rose-100 text-rose-700 border border-rose-300' : 'bg-slate-100 text-slate-600 border border-slate-200'"
                  >
                    Kho: {{ item.stockQuantity }}
                  </span>
                </div>
              </div>

              <div class="font-black text-slate-900 text-right">
                {{ formatCurrency(item.unitPrice * item.quantity) }}
              </div>
            </div>
          </div>
        </div>

        <!-- Banner cảnh báo nếu có lỗi tồn kho -->
        <div v-if="hasStockError" class="p-2.5 rounded-xl bg-rose-50 border border-rose-200 text-rose-700 text-xs flex items-center gap-2 font-medium">
          <span class="font-bold text-base">⚠️</span>
          <span>Có sản phẩm vượt quá số lượng tồn kho. Vui lòng bấm Hủy Bỏ để điều chỉnh lại giỏ hàng!</span>
        </div>

        <!-- Total Calculation Breakdown -->
        <div class="p-3.5 bg-slate-50 rounded-2xl border border-slate-200 space-y-1.5">
          <div class="flex justify-between text-slate-500 font-medium">
            <span>Tạm tính (tiền gốc):</span>
            <span>{{ formatCurrency(subTotal) }}</span>
          </div>

          <div v-if="discountPercent > 0" class="flex justify-between text-emerald-600 font-medium">
            <span>Giảm giá ({{ discountPercent }}%):</span>
            <span>-{{ formatCurrency(discountAmount) }}</span>
          </div>

          <div class="flex justify-between items-baseline pt-2 border-t border-slate-200">
            <span class="text-sm font-black text-slate-900">Tổng Tiền Cần Thu:</span>
            <span class="text-2xl font-black text-indigo-600 tracking-tight">
              {{ formatCurrency(finalAmount) }}
            </span>
          </div>
        </div>

        <!-- Cash Calculator: Khách đưa & Tiền thối lại (Chỉ hiện khi chọn Tiền Mặt) -->
        <div v-if="selectedMethod === 'Cash'" class="p-3 bg-emerald-50/70 rounded-2xl border border-emerald-200/80 space-y-2.5">
          <div class="flex items-center justify-between">
            <span class="font-bold text-emerald-900 flex items-center gap-1.5">
              <Coins class="w-3.5 h-3.5 text-emerald-600" />
              Tiền Khách Đưa:
            </span>
            <div class="relative w-36">
              <input
                ref="cashInputRef"
                v-model.number="cashGiven"
                type="number"
                min="0"
                step="1000"
                class="w-full bg-white border border-emerald-300 rounded-xl px-2.5 py-1.5 text-right font-black text-slate-900 focus:outline-none focus:ring-2 focus:ring-emerald-500 text-sm"
              />
            </div>
          </div>

          <!-- Quick cash buttons -->
          <div class="flex items-center gap-1.5 flex-wrap">
            <button
              type="button"
              @click="cashGiven = finalAmount"
              class="py-1 px-2.5 rounded-lg bg-white border border-emerald-200 text-emerald-800 font-bold hover:bg-emerald-100 transition text-[11px]"
            >
              Đủ tiền
            </button>
            <button
              v-for="amount in quickCashAmounts"
              :key="amount"
              type="button"
              @click="cashGiven = amount"
              class="py-1 px-2.5 rounded-lg bg-white border border-emerald-200 text-slate-700 font-bold hover:bg-emerald-100 transition text-[11px]"
            >
              {{ formatQuickAmount(amount) }}
            </button>
          </div>

          <!-- Change display -->
          <div class="flex justify-between items-baseline pt-2 border-t border-emerald-200/70">
            <span class="font-bold text-slate-700">Tiền thối lại khách:</span>
            <span 
              class="text-base font-black tracking-tight"
              :class="cashChange >= 0 ? 'text-emerald-700' : 'text-rose-600'"
            >
              <span v-if="cashChange >= 0">{{ formatCurrency(cashChange) }}</span>
              <span v-else>Còn thiếu {{ formatCurrency(Math.abs(cashChange)) }}</span>
            </span>
          </div>
        </div>

      </div>

      <!-- Modal Footer Action Buttons -->
      <div class="px-5 py-4 border-t border-slate-100 bg-slate-50/70 grid grid-cols-2 gap-3">
        <!-- Nút Hủy / Quay lại -->
        <button
          type="button"
          @click="handleClose"
          :disabled="isLoading"
          class="py-3 px-4 rounded-2xl bg-white hover:bg-slate-100 border border-slate-300 text-slate-700 font-extrabold text-xs sm:text-sm transition flex items-center justify-center gap-2 active:scale-95 disabled:opacity-50"
        >
          <X class="w-4 h-4 text-slate-500" />
          <span>Hủy Bỏ (Esc)</span>
        </button>

        <!-- Nút Xác Nhận Thanh Toán -->
        <button
          type="button"
          @click="handleConfirm"
          :disabled="isLoading || (selectedMethod === 'Cash' && cashChange < 0) || hasStockError"
          class="py-3 px-4 rounded-2xl text-white font-extrabold text-xs sm:text-sm transition flex items-center justify-center gap-2 shadow-md active:scale-95 disabled:opacity-50 disabled:cursor-not-allowed"
          :class="selectedMethod === 'Cash' 
            ? 'bg-gradient-to-r from-emerald-600 to-teal-600 hover:from-emerald-500 hover:to-teal-500 shadow-emerald-500/25' 
            : 'bg-gradient-to-r from-indigo-600 to-blue-600 hover:from-indigo-500 hover:to-blue-500 shadow-indigo-500/25'"
        >
          <div v-if="isLoading" class="w-4 h-4 border-2 border-white border-t-transparent rounded-full animate-spin"></div>
          <Check v-else class="w-4 h-4" />
          <span>
            {{ selectedMethod === 'Cash' ? 'Xác Nhận Thu Tiền' : 'Tạo Mã VietQR' }}
          </span>
        </button>
      </div>

    </div>
  </div>
</template>

<script setup>
import { ref, computed, watch, nextTick } from 'vue'
import { 
  Banknote, 
  QrCode, 
  X, 
  Check, 
  User, 
  FileText, 
  ShoppingCart, 
  Coins 
} from 'lucide-vue-next'

const props = defineProps({
  isOpen: {
    type: Boolean,
    default: false
  },
  initialMethod: {
    type: String,
    default: 'Cash' // 'Cash' | 'VietQR'
  },
  cart: {
    type: Array,
    default: () => []
  },
  customerName: {
    type: String,
    default: ''
  },
  customerPhone: {
    type: String,
    default: ''
  },
  note: {
    type: String,
    default: ''
  },
  subTotal: {
    type: Number,
    default: 0
  },
  discountPercent: {
    type: Number,
    default: 0
  },
  discountAmount: {
    type: Number,
    default: 0
  },
  finalAmount: {
    type: Number,
    default: 0
  },
  isLoading: {
    type: Boolean,
    default: false
  }
})

const emit = defineEmits(['close', 'confirm'])

const selectedMethod = ref('Cash')
const cashGiven = ref(0)
const cashInputRef = ref(null)

// Tự động khởi tạo method và tiền khách đưa khi mở modal
watch(() => props.isOpen, (newVal) => {
  if (newVal) {
    selectedMethod.value = props.initialMethod || 'Cash'
    cashGiven.value = props.finalAmount || 0
    nextTick(() => {
      if (selectedMethod.value === 'Cash' && cashInputRef.value) {
        cashInputRef.value.focus()
        cashInputRef.value.select()
      }
    })
  }
})

// Cập nhật lại tiền khách đưa nếu đổi phương thức sang Cash
watch(selectedMethod, (newMethod) => {
  if (newMethod === 'Cash' && cashGiven.value < props.finalAmount) {
    cashGiven.value = props.finalAmount
  }
})

const totalQuantity = computed(() => {
  return props.cart.reduce((sum, item) => sum + (item.quantity || 1), 0)
})

const cashChange = computed(() => {
  return (Number(cashGiven.value) || 0) - (Number(props.finalAmount) || 0)
})

const quickCashAmounts = computed(() => {
  const current = props.finalAmount || 0
  const list = []
  
  const steps = [50000, 100000, 200000, 500000]
  steps.forEach(step => {
    if (step > current && !list.includes(step)) {
      list.push(step)
    }
  })

  // Nếu tổng tiền lớn hơn 500k, tạo thêm mốc làm tròn 100k tiếp theo
  if (current >= 500000) {
    const nextHundred = Math.ceil(current / 100000) * 100000
    if (nextHundred > current && !list.includes(nextHundred)) {
      list.push(nextHundred)
    }
    const nextFiveHundred = Math.ceil(current / 500000) * 500000
    if (nextFiveHundred > current && !list.includes(nextFiveHundred)) {
      list.push(nextFiveHundred)
    }
  }

  return list.slice(0, 4)
})

function formatCurrency(val) {
  return new Intl.NumberFormat('vi-VN', { style: 'currency', currency: 'VND' }).format(val || 0)
}

function formatQuickAmount(val) {
  if (val >= 1000000) return `${val / 1000000}M`
  return `${val / 1000}k`
}

const hasStockError = computed(() => {
  return props.cart.some(item => 
    item.itemType === 'Product' && 
    item.stockQuantity !== null && 
    item.stockQuantity !== undefined && 
    (item.stockQuantity <= 0 || item.quantity > item.stockQuantity)
  )
})

function handleClose() {
  if (props.isLoading) return
  emit('close')
}

function handleConfirm() {
  if (props.isLoading || hasStockError.value) return
  emit('confirm', selectedMethod.value)
}
</script>
