<template>
  <div v-if="isOpen && order" class="fixed inset-0 z-50 flex items-center justify-center p-4 bg-slate-900/40 backdrop-blur-sm">
    <div class="relative w-full max-w-lg bg-white border border-slate-200 rounded-2xl shadow-2xl p-6 overflow-hidden">
      <!-- Close Button -->
      <button 
        @click="$emit('close')" 
        class="absolute top-4 right-4 text-slate-400 hover:text-slate-600 p-2 rounded-lg hover:bg-slate-100 transition"
      >
        ✕
      </button>

      <!-- Header -->
      <div class="border-b border-slate-100 pb-4 mb-4">
        <div class="flex items-center justify-between">
          <span class="text-xs font-mono font-bold px-2.5 py-1 rounded bg-indigo-50 text-indigo-600 border border-indigo-200">
            {{ order.orderCode }}
          </span>
          <span 
            class="text-xs font-semibold px-2.5 py-1 rounded-full"
            :class="order.paymentStatus === 'Completed' ? 'bg-emerald-50 text-emerald-600 border border-emerald-200' : 'bg-rose-50 text-rose-600 border border-rose-200'"
          >
            {{ order.paymentStatus === 'Completed' ? 'Đã thanh toán' : 'Đã hủy' }}
          </span>
        </div>
        <h3 class="text-lg font-bold text-slate-900 mt-2">Chi Tiết Hóa Đơn Điện Tử</h3>
        <p class="text-xs text-slate-500">{{ formatDate(order.createdAt) }} • Hình thức: {{ order.paymentMethod }}</p>
      </div>

      <!-- Cancelled Alert Banner -->
      <div v-if="order.paymentStatus === 'Cancelled'" class="mb-4 p-3 rounded-xl bg-rose-50 border border-rose-200 text-xs text-rose-700 flex items-start gap-2">
        <span class="text-base">⚠️</span>
        <div>
          <p class="font-bold">Đơn hàng này đã bị HỦY</p>
          <p class="text-[11px] text-rose-600 mt-0.5">Doanh thu đơn này không tính vào sổ sách tài chính. Tồn kho sản phẩm đã được hoàn lại.</p>
        </div>
      </div>

      <!-- Customer & Note -->
      <div class="bg-slate-50 p-3 rounded-xl border border-slate-200/80 text-xs space-y-1.5 mb-4">
        <div class="flex justify-between">
          <span class="text-slate-500">Khách hàng:</span>
          <span class="font-semibold text-slate-800">{{ order.customerName || 'Khách vãng lai' }} {{ order.customerPhone ? `(${order.customerPhone})` : '' }}</span>
        </div>
        <div v-if="order.note" class="pt-1.5 border-t border-slate-200">
          <span class="text-slate-500 block mb-0.5">Ghi chú đơn hàng:</span>
          <p class="text-slate-700 italic bg-white p-2 rounded border border-slate-200/60 whitespace-pre-wrap">{{ order.note }}</p>
        </div>
      </div>

      <!-- Items List -->
      <div class="space-y-2 max-h-56 overflow-y-auto pr-1">
        <div 
          v-for="(item, idx) in order.items" 
          :key="idx"
          class="flex justify-between items-center p-2.5 rounded-lg bg-slate-50 border border-slate-200/60 text-xs"
        >
          <div class="flex items-center gap-2">
            <span 
              class="px-1.5 py-0.5 rounded text-[10px] font-bold"
              :class="item.itemType === 'Service' ? 'bg-indigo-50 text-indigo-600' : 'bg-blue-50 text-blue-600'"
            >
              {{ item.itemType === 'Service' ? 'Dịch vụ' : 'Sản phẩm' }}
            </span>
            <span class="text-slate-800 font-medium" :class="order.paymentStatus === 'Cancelled' ? 'line-through text-slate-400' : ''">{{ item.itemName }}</span>
            <span class="text-slate-400">x{{ item.quantity }}</span>
          </div>
          <span class="text-slate-900 font-semibold" :class="order.paymentStatus === 'Cancelled' ? 'line-through text-slate-400' : ''">{{ formatCurrency(item.totalPrice) }}</span>
        </div>
      </div>

      <!-- Calculation breakdown -->
      <div class="mt-4 pt-3 border-t border-slate-100 space-y-1.5 text-xs">
        <div class="flex justify-between text-slate-500">
          <span>Tạm tính (tiền gốc):</span>
          <span :class="order.paymentStatus === 'Cancelled' ? 'line-through text-slate-400' : ''">{{ formatCurrency(order.subTotal) }}</span>
        </div>
        <div v-if="order.discountPercent > 0" class="flex justify-between text-emerald-600 font-medium">
          <span>Giảm giá ({{ order.discountPercent }}%):</span>
          <span>-{{ formatCurrency(order.discountAmount) }}</span>
        </div>
        <div class="flex justify-between text-sm font-bold text-slate-900 pt-2 border-t border-slate-200">
          <span>Tổng thực thu:</span>
          <span :class="order.paymentStatus === 'Cancelled' ? 'line-through text-slate-400 text-sm' : 'text-indigo-600 text-base'">
            {{ formatCurrency(order.finalAmount) }}
          </span>
        </div>
      </div>

      <!-- Actions -->
      <div class="mt-6 flex flex-wrap gap-2.5">
        <button
          v-if="order.paymentStatus === 'Completed'"
          @click="$emit('edit-order', order)"
          class="py-2 px-3 rounded-xl bg-indigo-50 hover:bg-indigo-100 text-indigo-600 border border-indigo-200 font-semibold text-xs transition flex items-center gap-1.5"
        >
          <span>✏️</span> Sửa Đơn
        </button>
        <button
          v-if="order.paymentStatus === 'Completed'"
          @click="$emit('cancel-order', order)"
          class="py-2 px-3.5 rounded-xl bg-rose-50 hover:bg-rose-100 text-rose-600 border border-rose-200 font-semibold text-xs transition flex items-center gap-1.5"
          title="Hủy đơn hàng và hoàn tồn kho"
        >
          <span>🚫</span> Hủy Đơn Hàng
        </button>
        <button
          @click="$emit('close')"
          class="flex-1 py-2 px-4 rounded-xl bg-slate-900 hover:bg-slate-800 text-white font-semibold text-xs transition text-center shadow-sm"
        >
          Đóng
        </button>
      </div>

    </div>
  </div>
</template>

<script setup>
defineProps({
  isOpen: Boolean,
  order: Object
})

defineEmits(['close', 'cancel-order', 'edit-order'])

function formatCurrency(val) {
  return new Intl.NumberFormat('vi-VN', { style: 'currency', currency: 'VND' }).format(val || 0)
}

function formatDate(dateStr) {
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
