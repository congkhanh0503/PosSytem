<template>
  <div v-if="isOpen" class="fixed inset-0 z-50 flex items-center justify-center p-4 bg-slate-900/50 backdrop-blur-sm animate-fade-in">
    <div class="relative w-full max-w-md bg-white border border-slate-200 rounded-3xl shadow-2xl overflow-hidden text-center p-6 sm:p-7">
      
      <!-- Close Button -->
      <button 
        @click="$emit('close')" 
        class="absolute top-4 right-4 text-slate-400 hover:text-slate-700 p-2 rounded-xl hover:bg-slate-100 transition"
      >
        <svg class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
          <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M6 18L18 6M6 6l12 12" />
        </svg>
      </button>

      <!-- Header -->
      <div class="mb-3">
        <div class="inline-flex items-center gap-2 px-3 py-1 rounded-full bg-indigo-50 border border-indigo-200 text-indigo-700 text-xs font-semibold mb-1.5 shadow-2xs">
          <span class="w-2 h-2 rounded-full" :class="isOfflineMode ? 'bg-amber-500' : 'bg-emerald-500 animate-pulse'"></span>
          {{ isOfflineMode ? 'VietQR Ngoại Tuyến (Offline)' : 'Thanh Toán VietQR NAPAS 247' }}
        </div>
        <h3 class="text-xl font-black text-slate-900 tracking-tight">Quét Mã Thanh Toán</h3>
        <p class="text-xs text-slate-500 mt-0.5">Mở ứng dụng ngân hàng hoặc ví điện tử bất kỳ để quét</p>
      </div>

      <!-- QR Code Container -->
      <div class="relative mx-auto my-2 p-3 bg-white rounded-2xl shadow-inner max-w-[270px] border-2 border-indigo-500/20 flex flex-col items-center justify-center min-h-[270px]">
        
        <!-- Chế độ Offline QR hoặc khi ảnh Online lỗi -->
        <div v-if="isOfflineMode || !onlineImageLoaded" class="w-full flex flex-col items-center">
          <div v-if="offlineQrImage" class="relative group">
            <img 
              :src="offlineQrImage" 
              alt="Mã VietQR Offline" 
              class="w-[230px] h-[230px] object-contain rounded-xl"
            />
            <!-- Tag nhận diện Offline an toàn -->
            <div class="absolute bottom-1 right-1 bg-slate-900/80 backdrop-blur-xs text-amber-300 text-[10px] font-bold px-1.5 py-0.5 rounded shadow">
              ⚡ Offline QR
            </div>
          </div>
          <div v-else class="h-56 flex flex-col items-center justify-center gap-2 text-slate-400 text-xs">
            <div class="w-6 h-6 border-2 border-indigo-600 border-t-transparent rounded-full animate-spin"></div>
            <span>Đang tạo mã QR...</span>
          </div>
        </div>

        <!-- Chế độ Online: Thử tải ảnh template chuẩn từ VietQR.io -->
        <img 
          v-show="!isOfflineMode && onlineImageLoaded"
          :src="qrData?.qrImageUrl" 
          @load="onOnlineImageLoad"
          @error="onOnlineImageError"
          alt="Mã VietQR" 
          class="w-full h-auto object-contain rounded-xl"
        />

        <!-- Ảnh ẩn để kiểm tra xem URL online có tải được không -->
        <img 
          v-if="!isOfflineMode && !onlineImageLoaded && qrData?.qrImageUrl"
          :src="qrData.qrImageUrl"
          @load="onOnlineImageLoad"
          @error="onOnlineImageError"
          class="hidden"
        />
      </div>

      <!-- Nút chuyển đổi chế độ QR tiện lợi -->
      <div class="flex items-center justify-center gap-2 mb-2">
        <button 
          @click="toggleMode"
          type="button"
          class="text-[11px] font-medium px-2.5 py-1 rounded-lg border transition flex items-center gap-1.5"
          :class="isOfflineMode 
            ? 'bg-amber-50 border-amber-300 text-amber-800 hover:bg-amber-100' 
            : 'bg-slate-50 border-slate-200 text-slate-600 hover:bg-slate-100'"
        >
          <span>{{ isOfflineMode ? '⚡ Đang dùng QR Cục Bộ (Không cần mạng)' : '🌐 Đang dùng QR Online' }}</span>
          <span class="underline text-indigo-600 hover:text-indigo-800">Đổi</span>
        </button>
      </div>

      <!-- Payment Details Summary -->
      <div class="mt-2.5 p-3.5 bg-slate-50 rounded-2xl border border-slate-200 text-left space-y-2 text-xs">
        <div class="flex justify-between items-center">
          <span class="text-slate-500 font-medium">Số tiền thanh toán:</span>
          <span class="text-lg font-black text-indigo-600 tracking-tight">
            {{ formatCurrency(qrData?.amount || 0) }}
          </span>
        </div>

        <div class="flex justify-between items-center pt-1.5 border-t border-slate-200">
          <span class="text-slate-500">Ngân hàng:</span>
          <span class="font-semibold text-slate-800">{{ qrData?.bankId }} ({{ qrData?.bankName }})</span>
        </div>

        <div class="flex justify-between items-center">
          <span class="text-slate-500">Số tài khoản:</span>
          <div class="flex items-center gap-1.5">
            <span class="font-bold text-slate-900 font-mono text-sm tracking-wide">{{ qrData?.accountNo }}</span>
            <button 
              @click="copyText(qrData?.accountNo, 'Đã sao chép số tài khoản')" 
              class="text-indigo-600 hover:text-indigo-700 p-0.5 transition active:scale-90"
              title="Sao chép"
            >
              📋
            </button>
          </div>
        </div>

        <div class="flex justify-between items-center">
          <span class="text-slate-500">Chủ tài khoản:</span>
          <span class="font-bold text-slate-900 uppercase">{{ qrData?.accountName }}</span>
        </div>

        <div class="flex justify-between items-center pt-1.5 border-t border-slate-200">
          <span class="text-slate-500">Nội dung CK:</span>
          <div class="flex items-center gap-1.5">
            <span class="font-mono font-bold text-indigo-700 bg-white border border-slate-200 px-2 py-0.5 rounded shadow-2xs">
              {{ qrData?.orderCode }}
            </span>
            <button 
              @click="copyText(qrData?.orderCode, 'Đã sao chép nội dung chuyển khoản')" 
              class="text-indigo-600 hover:text-indigo-700 p-0.5 transition active:scale-90"
              title="Sao chép"
            >
              📋
            </button>
          </div>
        </div>
      </div>

      <!-- Action Buttons -->
      <div class="mt-4 flex gap-3">
        <button
          @click="$emit('complete')"
          class="flex-1 py-3 px-4 rounded-xl bg-gradient-to-r from-emerald-600 to-teal-600 hover:from-emerald-500 hover:to-teal-500 text-white font-extrabold text-sm shadow-md shadow-emerald-600/25 transition transform active:scale-95 flex items-center justify-center gap-2"
        >
          <svg class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2.5" d="M5 13l4 4L19 7" />
          </svg>
          Xác Nhận Đã Nhận Tiền
        </button>
      </div>

      <!-- Notification Toast inside modal if copied -->
      <p v-if="toastMsg" class="mt-2 text-xs text-emerald-600 animate-fade-in font-medium">
        ✓ {{ toastMsg }}
      </p>

    </div>
  </div>
</template>

<script setup>
import { ref, watch } from 'vue'
import { generateVietQrPayload, generateOfflineQrImage } from '@/services/vietqr'

const props = defineProps({
  isOpen: Boolean,
  qrData: Object
})

defineEmits(['close', 'complete'])

const toastMsg = ref('')
const offlineQrImage = ref('')
const onlineImageLoaded = ref(false)
const isOfflineMode = ref(!navigator.onLine)

// Khi mở modal hoặc dữ liệu đơn thay đổi -> tự động sinh QR offline dự phòng
watch(
  () => [props.isOpen, props.qrData],
  async ([open, data]) => {
    if (open && data) {
      onlineImageLoaded.value = false
      isOfflineMode.value = !navigator.onLine

      try {
        // Sinh payload EMVCo chuẩn NAPAS 247 và ảnh Base64 offline
        const payload = generateVietQrPayload({
          bankId: data.bankId,
          accountNo: data.accountNo,
          amount: data.amount,
          orderCode: data.orderCode,
          description: data.description
        })
        offlineQrImage.value = await generateOfflineQrImage(payload)
      } catch (err) {
        console.error('Lỗi sinh mã QR offline:', err)
      }

      // Đặt timeout nếu sau 2.5s ảnh online chưa tải xong thì tự động chuyển sang offline
      setTimeout(() => {
        if (!onlineImageLoaded.value) {
          isOfflineMode.value = true
        }
      }, 2500)
    }
  },
  { immediate: true, deep: true }
)

function onOnlineImageLoad() {
  onlineImageLoaded.value = true
}

function onOnlineImageError() {
  // Khi không có mạng hoặc img.vietqr.io bị lỗi, lập tức bật chế độ offline
  onlineImageLoaded.value = false
  isOfflineMode.value = true
}

function toggleMode() {
  isOfflineMode.value = !isOfflineMode.value
}

function formatCurrency(val) {
  return new Intl.NumberFormat('vi-VN', { style: 'currency', currency: 'VND' }).format(val || 0)
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
