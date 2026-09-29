<template>
  <div 
    v-if="isOpen" 
    class="fixed inset-0 z-[100] flex items-center justify-center p-4 bg-slate-900/70 backdrop-blur-sm animate-fade-in"
  >
    <div class="bg-white border border-slate-200 rounded-3xl max-w-lg w-full p-6 sm:p-8 space-y-5 shadow-2xl text-slate-800 animate-scale-up">
      
      <!-- Header -->
      <div class="text-center space-y-2">
        <div class="w-14 h-14 mx-auto rounded-2xl bg-gradient-to-br from-indigo-500 to-emerald-500 flex items-center justify-center text-white shadow-lg shadow-indigo-500/25">
          <Store class="w-7 h-7" />
        </div>
        <h2 class="text-2xl font-black text-slate-900 tracking-tight">
          Chào Mừng Đến Với DiroPos! 🎉
        </h2>
        <p class="text-xs sm:text-sm text-slate-500 max-w-md mx-auto">
          Vui lòng nhập thông tin cơ bản của tiệm để hoàn tất cài đặt. Hệ thống sẽ tự động đồng bộ lên máy chủ quản trị DiroAdmin.
        </p>
      </div>

      <!-- Form -->
      <form @submit.prevent="handleSubmit" class="space-y-4 text-xs sm:text-sm">
        <!-- Tên tiệm -->
        <div>
          <label class="block font-bold text-slate-700 mb-1.5">
            Tên Quán / Cửa Hàng <span class="text-rose-500">*</span>
          </label>
          <div class="relative">
            <input
              v-model="form.shopName"
              type="text"
              required
              placeholder="VD: Barber Hoàng Long, Salon Mai Anh, Spa Venus..."
              class="w-full bg-slate-50 border border-slate-200 rounded-xl px-3.5 py-2.5 text-slate-900 placeholder:text-slate-400 font-medium focus:outline-none focus:border-indigo-500 focus:bg-white focus:ring-2 focus:ring-indigo-500/20 transition"
            />
          </div>
        </div>

        <!-- SĐT & Mô hình -->
        <div class="grid grid-cols-1 sm:grid-cols-2 gap-3.5">
          <div>
            <label class="block font-bold text-slate-700 mb-1.5">
              Số Điện Thoại Liên Hệ <span class="text-rose-500">*</span>
            </label>
            <input
              v-model="form.phone"
              type="tel"
              required
              placeholder="VD: 0987654321"
              class="w-full bg-slate-50 border border-slate-200 rounded-xl px-3.5 py-2.5 text-slate-900 placeholder:text-slate-400 font-medium focus:outline-none focus:border-indigo-500 focus:bg-white focus:ring-2 focus:ring-indigo-500/20 transition"
            />
          </div>

          <div>
            <label class="block font-bold text-slate-700 mb-1.5">
              Mô Hình Kinh Doanh <span class="text-rose-500">*</span>
            </label>
            <select
              v-model="form.businessModel"
              required
              class="w-full bg-slate-50 border border-slate-200 rounded-xl px-3.5 py-2.5 text-slate-900 font-medium focus:outline-none focus:border-indigo-500 focus:bg-white focus:ring-2 focus:ring-indigo-500/20 transition"
            >
              <option value="Barber">Barber / Tiệm Cắt Tóc Nam</option>
              <option value="Salon">Salon Tóc Nữ</option>
              <option value="Spa">Spa / Thẩm Mỹ / Nail</option>
              <option value="Retail">Cửa Hàng Bán Lẻ / Tạp Hóa</option>
              <option value="Cafe">Quán Cafe / Trà Sữa</option>
              <option value="Other">Mô Hình Khác</option>
            </select>
          </div>
        </div>

        <!-- Chủ tiệm & Địa chỉ -->
        <div class="grid grid-cols-1 sm:grid-cols-2 gap-3.5">
          <div>
            <label class="block font-bold text-slate-700 mb-1.5">
              Tên Chủ Tiệm / Quản Lý
            </label>
            <input
              v-model="form.ownerName"
              type="text"
              placeholder="VD: Nguyễn Văn A"
              class="w-full bg-slate-50 border border-slate-200 rounded-xl px-3.5 py-2.5 text-slate-900 placeholder:text-slate-400 font-medium focus:outline-none focus:border-indigo-500 focus:bg-white focus:ring-2 focus:ring-indigo-500/20 transition"
            />
          </div>

          <div>
            <label class="block font-bold text-slate-700 mb-1.5">
              Địa Chỉ Quán
            </label>
            <input
              v-model="form.address"
              type="text"
              placeholder="VD: 123 Đường Lê Lợi, TP.HCM"
              class="w-full bg-slate-50 border border-slate-200 rounded-xl px-3.5 py-2.5 text-slate-900 placeholder:text-slate-400 font-medium focus:outline-none focus:border-indigo-500 focus:bg-white focus:ring-2 focus:ring-indigo-500/20 transition"
            />
          </div>
        </div>

        <!-- Note info -->
        <div class="p-3 bg-indigo-50/70 border border-indigo-100 rounded-2xl flex items-center gap-2.5 text-[11px] sm:text-xs text-indigo-700">
          <CheckCircle2 class="w-4 h-4 shrink-0 text-indigo-600" />
          <span>Thông tin này sẽ xuất hiện trên hóa đơn thanh toán và kết nối tự động với hệ thống quản trị DiroAdmin.</span>
        </div>

        <!-- Error Message -->
        <p v-if="errorMsg" class="text-xs text-rose-600 font-bold animate-shake">
          ⚠ {{ errorMsg }}
        </p>

        <!-- Submit Button -->
        <button
          type="submit"
          :disabled="loading || !form.shopName.trim() || !form.phone.trim()"
          class="w-full py-3.5 px-5 rounded-2xl bg-gradient-to-r from-indigo-600 to-emerald-600 hover:from-indigo-500 hover:to-emerald-500 text-white font-extrabold text-sm shadow-lg shadow-indigo-600/30 transition transform active:scale-[0.98] disabled:opacity-50 disabled:cursor-not-allowed flex items-center justify-center gap-2 cursor-pointer"
        >
          <span v-if="loading" class="w-4 h-4 border-2 border-white/30 border-t-white rounded-full animate-spin"></span>
          <span>{{ loading ? 'Đang khởi tạo hệ thống...' : 'Hoàn Tất & Bắt Đầu Bán Hàng Ngay 🚀' }}</span>
        </button>
      </form>

    </div>
  </div>
</template>

<script setup>
import { ref, onMounted } from 'vue'
import api from '@/services/api'
import { Store, CheckCircle2 } from 'lucide-vue-next'

const isOpen = ref(false)
const loading = ref(false)
const errorMsg = ref('')

const form = ref({
  shopName: '',
  phone: '',
  businessModel: 'Barber',
  ownerName: '',
  address: ''
})

async function checkInitialization() {
  try {
    const res = await api.getLicenseStatus()
    if (res?.data) {
      // Nếu chưa được khởi tạo (isInitialized = false hoặc tên quán mặc định chưa đổi)
      if (res.data.isInitialized === false) {
        isOpen.value = true
        form.value.shopName = res.data.shopName === 'DiroPos Store' || res.data.shopName === 'Quán Dùng Thử' ? '' : res.data.shopName
        form.value.phone = res.data.contactPhone || ''
        form.value.businessModel = res.data.businessModel || 'Barber'
        form.value.ownerName = res.data.ownerName || ''
        form.value.address = res.data.address || ''
      } else {
        isOpen.value = false
      }
    }
  } catch (err) {
    console.warn('Lỗi kiểm tra trạng thái khởi tạo:', err)
  }
}

async function handleSubmit() {
  if (!form.value.shopName.trim() || !form.value.phone.trim()) {
    errorMsg.value = 'Vui lòng nhập tên quán và số điện thoại.'
    return
  }

  loading.value = true
  errorMsg.value = ''

  try {
    const res = await api.initShop(form.value)
    if (res?.data?.success) {
      isOpen.value = false
      alert(`Khởi tạo thành công quán "${form.value.shopName}"! Chúc bạn kinh doanh hồng phát!`)
      window.location.reload()
    }
  } catch (err) {
    errorMsg.value = err.response?.data?.message || err.message || 'Lỗi thiết lập thông tin quán.'
  } finally {
    loading.value = false
  }
}

onMounted(() => {
  checkInitialization()
})
</script>

<style scoped>
@keyframes fadeIn {
  from { opacity: 0; }
  to { opacity: 1; }
}
@keyframes scaleUp {
  from { transform: scale(0.95); opacity: 0; }
  to { transform: scale(1); opacity: 1; }
}
.animate-fade-in {
  animation: fadeIn 0.2s ease-out forwards;
}
.animate-scale-up {
  animation: scaleUp 0.25s cubic-bezier(0.16, 1, 0.3, 1) forwards;
}
</style>
