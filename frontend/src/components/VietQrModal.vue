<template>
  <div v-if="isOpen" class="fixed inset-0 z-50 flex items-center justify-center p-4 bg-black/80 backdrop-blur-sm animate-fade-in">
    <div class="relative w-full max-w-md bg-barber-card border border-barber-border rounded-2xl shadow-2xl overflow-hidden text-center p-6 sm:p-8">
      
      <!-- Close Button -->
      <button 
        @click="$emit('close')" 
        class="absolute top-4 right-4 text-zinc-400 hover:text-white p-2 rounded-lg hover:bg-white/5 transition"
      >
        <svg class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
          <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M6 18L18 6M6 6l12 12" />
        </svg>
      </button>

      <!-- Header -->
      <div class="mb-4">
        <div class="inline-flex items-center gap-2 px-3 py-1 rounded-full bg-amber-500/10 border border-amber-500/30 text-barber-gold text-xs font-semibold mb-2">
          <span class="w-2 h-2 rounded-full bg-emerald-400 animate-pulse"></span>
          Thanh Toán VietQR NAPAS 247
        </div>
        <h3 class="text-xl font-bold text-white">Quét Mã Ngân Hàng</h3>
        <p class="text-xs text-zinc-400 mt-0.5">Khách dùng App ngân hàng bất kỳ để quét</p>
      </div>

      <!-- QR Code Image Container -->
      <div class="relative mx-auto my-3 p-3 bg-white rounded-xl shadow-inner max-w-[260px] border-2 border-amber-500/40">
        <img 
          v-if="qrData?.qrImageUrl" 
          :src="qrData.qrImageUrl" 
          alt="Mã VietQR" 
          class="w-full h-auto object-contain rounded"
        />
        <div v-else class="h-60 flex items-center justify-center text-zinc-500 text-sm">
          Đang tạo mã QR...
        </div>
      </div>

      <!-- Payment Details Summary -->
      <div class="mt-4 p-3.5 bg-barber-dark/70 rounded-xl border border-barber-border/80 text-left space-y-2 text-xs">
        <div class="flex justify-between items-center">
          <span class="text-zinc-400">Số tiền thanh toán:</span>
          <span class="text-base font-extrabold text-barber-gold">
            {{ formatCurrency(qrData?.amount || 0) }}
          </span>
        </div>

        <div class="flex justify-between items-center pt-1 border-t border-zinc-800">
          <span class="text-zinc-400">Ngân hàng:</span>
          <span class="font-medium text-white">{{ qrData?.bankId }} ({{ qrData?.bankName }})</span>
        </div>

        <div class="flex justify-between items-center">
          <span class="text-zinc-400">Số tài khoản:</span>
          <div class="flex items-center gap-1.5">
            <span class="font-bold text-amber-200">{{ qrData?.accountNo }}</span>
            <button 
              @click="copyText(qrData?.accountNo, 'Đã sao chép số tài khoản')" 
              class="text-barber-gold hover:text-amber-300 p-0.5"
              title="Sao chép"
            >
              📋
            </button>
          </div>
        </div>

        <div class="flex justify-between items-center">
          <span class="text-zinc-400">Chủ tài khoản:</span>
          <span class="font-semibold text-white uppercase">{{ qrData?.accountName }}</span>
        </div>

        <div class="flex justify-between items-center pt-1 border-t border-zinc-800">
          <span class="text-zinc-400">Nội dung CK:</span>
          <div class="flex items-center gap-1.5">
            <span class="font-mono font-bold text-white bg-zinc-800 px-2 py-0.5 rounded">
              {{ qrData?.orderCode }}
            </span>
            <button 
              @click="copyText(qrData?.orderCode, 'Đã sao chép nội dung chuyển khoản')" 
              class="text-barber-gold hover:text-amber-300 p-0.5"
              title="Sao chép"
            >
              📋
            </button>
          </div>
        </div>
      </div>

      <!-- Action Buttons -->
      <div class="mt-6 flex gap-3">
        <button
          @click="$emit('complete')"
          class="flex-1 py-3 px-4 rounded-xl bg-gradient-to-r from-emerald-600 to-teal-600 hover:from-emerald-500 hover:to-teal-500 text-white font-bold text-sm shadow-lg shadow-emerald-900/30 transition transform active:scale-95 flex items-center justify-center gap-2"
        >
          <svg class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M5 13l4 4L19 7" />
          </svg>
          Xác Nhận Đã Nhận Tiền
        </button>
      </div>

      <!-- Notification Toast inside modal if copied -->
      <p v-if="toastMsg" class="mt-2 text-xs text-emerald-400 animate-fade-in font-medium">
        ✓ {{ toastMsg }}
      </p>

    </div>
  </div>
</template>

<script setup>
import { ref } from 'vue'

const props = defineProps({
  isOpen: Boolean,
  qrData: Object
})

defineEmits(['close', 'complete'])

const toastMsg = ref('')

function formatCurrency(val) {
  return new Intl.NumberFormat('vi-VN', { style: 'currency', currency: 'VND' }).format(val)
}

function copyText(text, msg) {
  if (!text) return
  navigator.clipboard.writeText(text).then(() => {
    toastMsg.value = msg
    setTimeout(() => {
      toastMsg.value = ''
    }, 2000)
  })
}
</script>
