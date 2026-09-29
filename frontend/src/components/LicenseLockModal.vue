<template>
  <div>
    <!-- 1. BANNER CẢNH BÁO SẮP HẾT HẠN (Nếu còn <= 5 ngày) -->
    <div
      v-if="license && license.isValid && license.daysRemaining <= 5 && !bannerDismissed"
      class="fixed top-0 left-0 right-0 z-40 bg-gradient-to-r from-amber-500 via-orange-500 to-amber-600 text-white px-4 py-2 text-xs font-semibold shadow-md flex items-center justify-between"
    >
      <div class="flex items-center gap-2 max-w-4xl mx-auto">
        <AlertTriangle class="w-4 h-4 shrink-0 animate-pulse text-amber-200" />
        <span>
          <strong>Lưu ý bản quyền:</strong> Quán của bạn còn 
          <span class="underline font-black">{{ license.daysRemaining }} ngày</span> sử dụng (hết hạn ngày {{ formatDate(license.expiresAt) }}). Vui lòng liên hệ Admin (<b>{{ license.supportHotline }}</b>) để gia hạn kịp thời.
        </span>
      </div>
      <div class="flex items-center gap-2">
        <button
          @click="handleSync"
          :disabled="syncing"
          class="bg-white/20 hover:bg-white/30 text-white px-2.5 py-1 rounded-lg font-bold text-[11px] transition active:scale-95 cursor-pointer shrink-0 flex items-center gap-1"
        >
          <RefreshCw class="w-3 h-3" :class="{ 'animate-spin': syncing }" />
          <span>Đồng bộ hạn mới</span>
        </button>
        <button
          @click="showActivateDialog = true"
          class="bg-white text-orange-600 hover:bg-orange-50 px-3 py-1 rounded-lg font-bold text-[11px] shadow-xs transition active:scale-95 cursor-pointer shrink-0"
        >
          Nhập Key
        </button>
        <button
          @click="bannerDismissed = true"
          title="Đóng thông báo này"
          class="p-1 rounded-md text-amber-100 hover:text-white hover:bg-white/20 transition cursor-pointer"
        >
          ✕
        </button>
      </div>
    </div>

    <!-- 2. MÀN HÌNH KHÓA BẢN QUYỀN TOÀN CỤC (Nếu hết hạn hoặc bị khóa) -->
    <div
      v-if="license && !license.isValid"
      class="fixed inset-0 z-[9999] flex items-center justify-center p-4 bg-slate-950/85 backdrop-blur-md select-none"
    >
      <div class="bg-white rounded-3xl shadow-2xl border border-slate-200 w-full max-w-lg overflow-hidden flex flex-col text-slate-800 animate-scale-up">
        
        <!-- Header cảnh báo -->
        <div class="p-6 bg-gradient-to-b from-rose-50 to-white text-center border-b border-rose-100 flex flex-col items-center">
          <div class="w-16 h-16 rounded-2xl bg-rose-600 text-white flex items-center justify-center shadow-lg shadow-rose-600/30 mb-3 animate-bounce">
            <Lock class="w-8 h-8" />
          </div>
          <h2 class="text-xl font-black text-rose-950 tracking-tight">
            {{ license.status === 'Suspended' ? 'BẢN QUYỀN ĐANG TẠM KHÓA' : 'BẢN QUYỀN ĐÃ HẾT HẠN' }}
          </h2>
          <p class="text-xs text-slate-600 mt-1 max-w-sm">
            {{ license.message || 'Phần mềm DiroPos đã hết hạn sử dụng. Vui lòng liên hệ Admin gia hạn để tiếp tục bán hàng.' }}
          </p>
        </div>

        <!-- Chi tiết máy & thông tin tiệm -->
        <div class="p-6 space-y-4 text-xs">
          <div class="bg-slate-50 border border-slate-200/80 rounded-2xl p-4 space-y-2.5">
            <div class="flex justify-between items-center text-slate-600">
              <span>Tên quán / Tiệm:</span>
              <b class="text-slate-900">{{ license.shopName || 'DiroPos Store' }}</b>
            </div>
            <div class="flex justify-between items-center text-slate-600">
              <span>Mã định danh quán:</span>
              <span class="font-mono font-bold text-indigo-600 bg-indigo-50 px-2 py-0.5 rounded border border-indigo-100">
                {{ license.shopCode }}
              </span>
            </div>
            <div class="flex justify-between items-center text-slate-600">
              <span>Mã phần cứng (Hardware ID):</span>
              <span class="font-mono font-bold text-slate-800 bg-slate-200/60 px-2 py-0.5 rounded">
                {{ license.hardwareId }}
              </span>
            </div>
            <div class="flex justify-between items-center text-slate-600">
              <span>Hotline hỗ trợ gia hạn:</span>
              <a :href="'tel:' + license.supportHotline" class="font-bold text-emerald-600 hover:underline">
                {{ license.supportHotline }}
              </a>
            </div>
          </div>

          <!-- 1. Tùy chọn Mở Khóa Tự Động (Khách không cần nhập key) -->
          <div class="space-y-2 pt-1">
            <button
              @click="handleSync"
              :disabled="syncing"
              class="w-full py-3.5 rounded-2xl bg-gradient-to-r from-emerald-600 via-teal-600 to-indigo-600 hover:from-emerald-500 hover:to-indigo-500 text-white font-black text-xs shadow-xl shadow-emerald-600/30 transition flex items-center justify-center gap-2 cursor-pointer active:scale-95 disabled:opacity-50"
            >
              <RefreshCw class="w-4 h-4" :class="{ 'animate-spin': syncing }" />
              <span>{{ syncing ? 'Đang kiểm tra với hệ thống...' : '⚡ BẤM MỞ KHÓA TỰ ĐỘNG (KHI ĐÃ GIA HẠN)' }}</span>
            </button>
            <p class="text-[11px] text-slate-500 text-center">
              Chỉ cần báo Admin gia hạn, sau đó bấm nút trên (hoặc đợi 5 giây hệ thống tự nhận) để vào bán hàng!
            </p>
          </div>

          <!-- 2. Hoặc Nhập Key Thủ Công (Cho quán offline không có mạng) -->
          <div class="border-t border-slate-200/80 pt-3">
            <button
              type="button"
              @click="showManualKeyInput = !showManualKeyInput"
              class="text-[11px] text-slate-500 hover:text-indigo-600 font-medium flex items-center gap-1 cursor-pointer mx-auto"
            >
              <span>{{ showManualKeyInput ? 'Ẩn ô nhập key thủ công' : '👉 Nhập mã Key thủ công (nếu quán mất mạng Internet)' }}</span>
            </button>

            <div v-if="showManualKeyInput" class="space-y-3 pt-3">
              <textarea
                v-model="inputKey"
                rows="3"
                placeholder="Dán mã bản quyền do Admin cấp vào đây (VD: eyJzaG9wQ29kZ...)"
                class="w-full bg-slate-50 border border-slate-200 rounded-xl p-3 text-xs font-mono text-slate-900 focus:outline-none focus:border-indigo-500 focus:bg-white resize-none shadow-2xs"
              ></textarea>
              
              <div v-if="errorMsg" class="p-2.5 rounded-lg bg-rose-50 border border-rose-200 text-rose-700 text-[11px] font-medium flex items-center gap-1.5">
                <AlertCircle class="w-3.5 h-3.5 shrink-0" />
                <span>{{ errorMsg }}</span>
              </div>

              <button
                @click="handleActivate"
                :disabled="loading || !inputKey.trim()"
                class="w-full py-2.5 rounded-xl bg-slate-800 hover:bg-slate-700 text-white font-bold text-xs shadow-md transition flex items-center justify-center gap-2 cursor-pointer active:scale-95 disabled:opacity-50"
              >
                <Key class="w-3.5 h-3.5" />
                <span>{{ loading ? 'Đang xác thực...' : 'Kích Hoạt Bằng Key Thủ Công' }}</span>
              </button>
            </div>
          </div>
        </div>

        <!-- Footer -->
        <div class="px-6 py-3.5 bg-slate-50 border-t border-slate-100 text-center text-[11px] text-slate-400">
          Hệ thống quản lý bán hàng thông minh DiroPos PRO
        </div>
      </div>
    </div>

    <!-- 3. DIALOG KÍCH HOẠT THỦ CÔNG (Khi bấm từ Banner hoặc Cài đặt) -->
    <div
      v-if="showActivateDialog"
      class="fixed inset-0 z-50 flex items-center justify-center p-4 bg-slate-900/60 backdrop-blur-xs"
    >
      <div class="bg-white rounded-2xl shadow-2xl border border-slate-200 w-full max-w-md p-6 space-y-4 animate-scale-up">
        <div class="flex items-center justify-between border-b border-slate-100 pb-3">
          <div class="flex items-center gap-2">
            <Key class="w-5 h-5 text-indigo-600" />
            <h3 class="font-extrabold text-slate-900 text-sm">Gia Hạn Bản Quyền DiroPos</h3>
          </div>
          <button
            @click="showActivateDialog = false"
            class="text-slate-400 hover:text-slate-600 p-1 rounded-lg"
          >
            ✕
          </button>
        </div>

        <div class="space-y-3 text-xs">
          <p class="text-slate-600 leading-relaxed">
            Dán chuỗi khóa bản quyền mới do Admin cấp để gia hạn thêm thời gian sử dụng:
          </p>

          <textarea
            v-model="inputKey"
            rows="3"
            placeholder="Dán mã bản quyền mới vào đây..."
            class="w-full bg-slate-50 border border-slate-200 rounded-xl p-3 text-xs font-mono text-slate-900 focus:outline-none focus:border-indigo-500 focus:bg-white resize-none"
          ></textarea>

          <div v-if="errorMsg" class="p-2.5 rounded-lg bg-rose-50 border border-rose-200 text-rose-700 text-[11px] font-medium flex items-center gap-1.5">
            <AlertCircle class="w-3.5 h-3.5 shrink-0" />
            <span>{{ errorMsg }}</span>
          </div>

          <div class="flex items-center justify-end gap-2 pt-2">
            <button
              @click="showActivateDialog = false"
              class="px-4 py-2 rounded-xl bg-slate-100 hover:bg-slate-200 text-slate-700 font-bold text-xs"
            >
              Hủy
            </button>
            <button
              @click="handleActivate"
              :disabled="loading || !inputKey.trim()"
              class="px-4 py-2 rounded-xl bg-indigo-600 hover:bg-indigo-500 text-white font-bold text-xs shadow-md shadow-indigo-600/20 flex items-center gap-1.5"
            >
              <Key class="w-3.5 h-3.5" />
              <span>{{ loading ? 'Đang kích hoạt...' : 'Kích Hoạt' }}</span>
            </button>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted, onUnmounted } from 'vue'
import api from '@/services/api'
import {
  Lock,
  Key,
  AlertTriangle,
  AlertCircle,
  RefreshCw
} from 'lucide-vue-next'

const license = ref(null)
const inputKey = ref('')
const loading = ref(false)
const syncing = ref(false)
const showManualKeyInput = ref(false)
const errorMsg = ref('')
const showActivateDialog = ref(false)
const bannerDismissed = ref(false)

let autoSyncInterval = null

function formatDate(isoStr) {
  if (!isoStr) return '---'
  try {
    const d = new Date(isoStr)
    return `${String(d.getDate()).padStart(2, '0')}/${String(d.getMonth() + 1).padStart(2, '0')}/${d.getFullYear()}`
  } catch {
    return isoStr
  }
}

async function checkLicenseStatus() {
  try {
    const res = await api.getLicenseStatus()
    if (res?.data) {
      license.value = res.data
    }
  } catch (err) {
    console.error('Lỗi kiểm tra bản quyền:', err)
  }
}

async function handleSync() {
  syncing.value = true
  try {
    const res = await api.syncLicense()
    if (res?.data?.status) {
      license.value = res.data.status
      if (res.data.status.isValid) {
        alert('Chúc mừng! Máy POS đã nhận bản quyền mới và tự động mở khóa!')
      } else {
        alert(res.data.status.message || 'Bản quyền chưa được gia hạn trên máy chủ Admin.')
      }
    }
  } catch (err) {
    console.warn('Không thể đồng bộ bản quyền với server:', err)
  } finally {
    syncing.value = false
  }
}

async function handleActivate() {
  if (!inputKey.value.trim()) return
  loading.value = true
  errorMsg.value = ''

  try {
    const res = await api.activateLicense(inputKey.value.trim())
    if (res?.data) {
      alert(res.data.message || 'Kích hoạt bản quyền thành công!')
      license.value = res.data.status
      showActivateDialog.value = false
      inputKey.value = ''
    }
  } catch (err) {
    errorMsg.value = err.response?.data?.message || err.message || 'Mã bản quyền không hợp lệ.'
  } finally {
    loading.value = false
  }
}

// Kiểm tra định kỳ và tự động sync ngầm khi bị khóa
onMounted(() => {
  checkLicenseStatus()
  
  let activeHeartbeatCounter = 0
  // Tự động kiểm tra định kỳ
  autoSyncInterval = setInterval(async () => {
    // Nếu màn hình đang bị khóa, tự động gọi sync mỗi 8 giây để mở khóa tức thì khi Admin vừa bấm gia hạn
    if (license.value && !license.value.isValid) {
      try {
        const res = await api.syncLicense()
        if (res?.data?.status?.isValid) {
          license.value = res.data.status
        }
      } catch { }
    } else {
      activeHeartbeatCounter++
      // Cứ mỗi 56s (~1 phút) gửi 1 heartbeat ping lên Cloud để DiroAdmin luôn nhận diện Online
      if (activeHeartbeatCounter >= 7) {
        activeHeartbeatCounter = 0
        try {
          await api.syncLicense()
        } catch { }
      }
      checkLicenseStatus()
    }
  }, 8000)
})

onUnmounted(() => {
  if (autoSyncInterval) clearInterval(autoSyncInterval)
})

defineExpose({
  openActivateDialog: () => { showActivateDialog.value = true },
  checkLicenseStatus,
  handleSync
})
</script>

<style scoped>
@keyframes scaleUp {
  from { transform: scale(0.95); opacity: 0; }
  to { transform: scale(1); opacity: 1; }
}
.animate-scale-up {
  animation: scaleUp 0.2s cubic-bezier(0.16, 1, 0.3, 1) forwards;
}
</style>
