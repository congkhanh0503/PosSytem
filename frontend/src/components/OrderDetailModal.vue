<template>
  <div v-if="isOpen && order" class="fixed inset-0 z-50 flex items-center justify-center p-4 bg-black/80 backdrop-blur-sm">
    <div class="relative w-full max-w-lg bg-barber-card border border-barber-border rounded-2xl shadow-2xl p-6 overflow-hidden">
      <!-- Close Button -->
      <button 
        @click="$emit('close')" 
        class="absolute top-4 right-4 text-zinc-400 hover:text-white p-2 rounded-lg hover:bg-white/5 transition"
      >
        ✕
      </button>

      <!-- Header -->
      <div class="border-b border-barber-border pb-4 mb-4">
        <div class="flex items-center justify-between">
          <span class="text-xs font-mono font-bold px-2.5 py-1 rounded bg-amber-500/10 text-barber-gold border border-amber-500/30">
            {{ order.orderCode }}
          </span>
          <span 
            class="text-xs font-semibold px-2.5 py-1 rounded-full"
            :class="order.paymentStatus === 'Completed' ? 'bg-emerald-500/10 text-emerald-400 border border-emerald-500/20' : 'bg-rose-500/10 text-rose-400 border border-rose-500/20'"
          >
            {{ order.paymentStatus === 'Completed' ? 'Đã thanh toán' : 'Đã hủy' }}
          </span>
        </div>
        <h3 class="text-lg font-bold text-white mt-2">Chi Tiết Hóa Đơn Điện Tử</h3>
        <p class="text-xs text-zinc-400">{{ formatDate(order.createdAt) }} • Hình thức: {{ order.paymentMethod }}</p>
      </div>

      <!-- Customer & Note -->
      <div class="bg-barber-dark/60 p-3 rounded-xl border border-barber-border text-xs space-y-1.5 mb-4">
        <div class="flex justify-between">
          <span class="text-zinc-400">Khách hàng:</span>
          <span class="font-medium text-white">{{ order.customerName || 'Khách vãng lai' }} {{ order.customerPhone ? `(${order.customerPhone})` : '' }}</span>
        </div>
        <div v-if="order.note" class="pt-1.5 border-t border-zinc-800/80">
          <span class="text-zinc-400 block mb-0.5">Ghi chú đơn hàng:</span>
          <p class="text-amber-200/90 italic bg-black/30 p-2 rounded">{{ order.note }}</p>
        </div>
      </div>

      <!-- Items List -->
      <div class="space-y-2 max-h-56 overflow-y-auto pr-1">
        <div 
          v-for="(item, idx) in order.items" 
          :key="idx"
          class="flex justify-between items-center p-2.5 rounded-lg bg-zinc-900/50 border border-zinc-800/80 text-xs"
        >
          <div class="flex items-center gap-2">
            <span 
              class="px-1.5 py-0.5 rounded text-[10px] font-bold"
              :class="item.itemType === 'Service' ? 'bg-amber-500/20 text-barber-gold' : 'bg-cyan-500/20 text-cyan-400'"
            >
              {{ item.itemType === 'Service' ? 'Dịch vụ' : 'Sản phẩm' }}
            </span>
            <span class="text-white font-medium">{{ item.itemName }}</span>
            <span class="text-zinc-400">x{{ item.quantity }}</span>
          </div>
          <span class="text-amber-200 font-semibold">{{ formatCurrency(item.totalPrice) }}</span>
        </div>
      </div>

      <!-- Calculation breakdown -->
      <div class="mt-4 pt-3 border-t border-barber-border space-y-1.5 text-xs">
        <div class="flex justify-between text-zinc-400">
          <span>Tạm tính (tiền gốc):</span>
          <span>{{ formatCurrency(order.subTotal) }}</span>
        </div>
        <div v-if="order.discountPercent > 0" class="flex justify-between text-emerald-400 font-medium">
          <span>Giảm giá ({{ order.discountPercent }}%):</span>
          <span>-{{ formatCurrency(order.discountAmount) }}</span>
        </div>
        <div class="flex justify-between text-sm font-bold text-white pt-2 border-t border-zinc-800">
          <span>Tổng thực thu:</span>
          <span class="text-barber-gold text-base">{{ formatCurrency(order.finalAmount) }}</span>
        </div>
      </div>

      <!-- Actions -->
      <div class="mt-6 flex gap-3">
        <button
          v-if="order.paymentStatus === 'Completed'"
          @click="$emit('cancel-order', order.id)"
          class="py-2.5 px-4 rounded-xl bg-rose-500/10 hover:bg-rose-500/20 text-rose-400 border border-rose-500/30 font-semibold text-xs transition"
        >
          Hủy Đơn Hàng Này
        </button>
        <button
          @click="$emit('close')"
          class="flex-1 py-2.5 px-4 rounded-xl bg-zinc-800 hover:bg-zinc-700 text-white font-semibold text-xs transition"
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

defineEmits(['close', 'cancel-order'])

function formatCurrency(val) {
  return new Intl.NumberFormat('vi-VN', { style: 'currency', currency: 'VND' }).format(val || 0)
}

function formatDate(dateStr) {
  if (!dateStr) return ''
  const d = new Date(dateStr)
  return d.toLocaleString('vi-VN', { 
    hour: '2-digit', 
    minute: '2-digit', 
    day: '2-digit', 
    month: '2-digit', 
    year: 'numeric' 
  })
}
</script>
