<template>
  <div>
    <!-- 1. Floating Banner thông báo bản cập nhật mới (khi không bắt buộc) -->
    <Transition
      enter-active-class="transform ease-out duration-300 transition"
      enter-from-class="-translate-y-full opacity-0"
      enter-to-class="translate-y-0 opacity-100"
      leave-active-class="transition ease-in duration-200"
      leave-from-class="opacity-100"
      leave-to-class="-translate-y-full opacity-0"
    >
      <div
        v-if="updateInfo?.hasUpdate && !updateInfo?.isMandatory && showBanner"
        class="fixed top-3 left-1/2 -translate-x-1/2 z-[9990] max-w-2xl w-full px-4"
      >
        <div class="bg-gradient-to-r from-indigo-900 via-indigo-800 to-slate-900 text-white p-3 sm:px-5 sm:py-3.5 rounded-2xl shadow-2xl border border-indigo-500/30 backdrop-blur-md flex items-center justify-between gap-3">
          <div class="flex items-center gap-3 min-w-0">
            <div class="w-8 h-8 rounded-xl bg-indigo-500/20 border border-indigo-400/30 flex items-center justify-center flex-shrink-0 text-amber-300 animate-pulse">
              <Sparkles class="w-4 h-4" />
            </div>
            <div class="truncate text-xs">
              <span class="font-bold text-amber-300 mr-1.5">DiroPos v{{ updateInfo.latestVersion }} đã sẵn sàng!</span>
              <span class="text-indigo-200 hidden sm:inline">Nhiều cải tiến và tính năng mới đang chờ bạn.</span>
            </div>
          </div>

          <div class="flex items-center gap-2 flex-shrink-0">
            <button
              type="button"
              @click="openModal"
              class="px-3.5 py-1.5 rounded-xl bg-indigo-500 hover:bg-indigo-400 text-white font-bold text-xs shadow-md transition active:scale-95 cursor-pointer"
            >
              Xem Chi Tiết
            </button>
            <button
              type="button"
              @click="dismissBanner"
              class="p-1.5 rounded-lg text-indigo-300 hover:text-white hover:bg-white/10 transition"
              title="Để sau"
            >
              <X class="w-4 h-4" />
            </button>
          </div>
        </div>
      </div>
    </Transition>

    <!-- 2. Modal chi tiết bản cập nhật -->
    <Transition
      enter-active-class="transition duration-200 ease-out"
      enter-from-class="opacity-0"
      enter-to-class="opacity-100"
      leave-active-class="transition duration-150 ease-in"
      leave-from-class="opacity-100"
      leave-to-class="opacity-0"
    >
      <div
        v-if="isModalOpen"
        class="fixed inset-0 z-[99999] flex items-center justify-center p-4 bg-slate-900/60 backdrop-blur-md"
        @click.self="handleBackdropClick"
      >
        <div class="bg-white rounded-3xl max-w-lg w-full shadow-2xl border border-slate-100 overflow-hidden transform transition-all p-6 sm:p-7 animate-in fade-in zoom-in-95 duration-200">
          <!-- Header -->
          <div class="flex items-start justify-between">
            <div class="flex items-center gap-3.5">
              <div class="w-12 h-12 rounded-2xl bg-gradient-to-br from-indigo-500 to-purple-600 flex items-center justify-center text-white shadow-lg shadow-indigo-500/25">
                <Rocket class="w-6 h-6" />
              </div>
              <div>
                <div class="flex items-center gap-2">
                  <h3 class="text-lg font-black text-slate-900 tracking-tight">Cập Nhật DiroPos</h3>
                  <span
                    v-if="updateInfo?.isMandatory"
                    class="px-2 py-0.5 rounded-full text-[10px] font-extrabold bg-rose-100 text-rose-700 uppercase tracking-wider"
                  >
                    Bắt Buộc
                  </span>
                  <span
                    v-else
                    class="px-2 py-0.5 rounded-full text-[10px] font-extrabold bg-emerald-100 text-emerald-700 tracking-wider"
                  >
                    Khuyến Nghị
                  </span>
                </div>
                <div class="flex items-center gap-2 mt-0.5 text-xs text-slate-500 font-medium">
                  <span>Bản mới: <b class="text-indigo-600">v{{ updateInfo?.latestVersion }}</b></span>
                  <span>•</span>
                  <span>Hiện tại: v{{ updateInfo?.currentVersion }}</span>
                </div>
              </div>
            </div>

            <button
              v-if="!updateInfo?.isMandatory"
              @click="isModalOpen = false"
              class="text-slate-400 hover:text-slate-600 p-1.5 rounded-xl hover:bg-slate-100 transition"
            >
              <X class="w-5 h-5" />
            </button>
          </div>

          <!-- Changelog -->
          <div class="mt-5">
            <h4 class="text-xs font-bold text-slate-700 uppercase tracking-wider mb-2 flex items-center gap-1.5">
              <Sparkles class="w-3.5 h-3.5 text-indigo-600" />
              Những điểm mới trong phiên bản này:
            </h4>
            <div class="p-4 rounded-2xl bg-slate-50 border border-slate-200/70 max-h-48 overflow-y-auto text-xs text-slate-700 leading-relaxed font-medium whitespace-pre-line">
              {{ updateInfo?.changelog || '• Cải thiện hiệu năng và tối ưu hóa hệ thống.\n• Nâng cao tính ổn định cho hóa đơn và sao lưu đám mây.' }}
            </div>
          </div>

          <!-- Notice Safe Upgrade -->
          <div class="mt-4 p-3 rounded-xl bg-emerald-50 border border-emerald-100 flex items-start gap-2.5 text-emerald-900 text-xs">
            <CheckCircle2 class="w-4 h-4 text-emerald-600 flex-shrink-0 mt-0.5" />
            <p class="leading-relaxed font-medium">
              Toàn bộ dữ liệu bán hàng, dịch vụ và cấu hình của quán được <b>giữ nguyên 100%</b> khi cập nhật.
            </p>
          </div>

          <!-- Action Buttons -->
          <div class="mt-6 flex items-center justify-end gap-3">
            <button
              v-if="!updateInfo?.isMandatory"
              type="button"
              @click="isModalOpen = false"
              class="px-5 py-2.5 rounded-xl border border-slate-200 text-slate-600 font-semibold text-xs hover:bg-slate-50 active:bg-slate-100 transition shadow-sm cursor-pointer"
            >
              Để Cập Nhật Sau
            </button>

            <button
              type="button"
              @click="handleDownload"
              :disabled="downloading"
              class="px-5 py-2.5 rounded-xl bg-gradient-to-r from-indigo-600 to-indigo-700 hover:from-indigo-500 hover:to-indigo-600 text-white font-bold text-xs shadow-md shadow-indigo-500/25 transition active:scale-95 flex items-center gap-2 cursor-pointer disabled:opacity-50"
            >
              <Download class="w-4 h-4" />
              <span>{{ downloading ? 'Đang mở liên kết...' : 'Tải Bản Cập Nhật Mới' }}</span>
            </button>
          </div>
        </div>
      </div>
    </Transition>
  </div>
</template>

<script setup>
import { ref, onMounted } from 'vue'
import api from '@/services/api'
import { useNotify } from '@/composables/useNotify'
import { Sparkles, Rocket, X, Download, CheckCircle2 } from 'lucide-vue-next'

const { toast } = useNotify()

const updateInfo = ref(null)
const showBanner = ref(true)
const isModalOpen = ref(false)
const downloading = ref(false)

async function checkForUpdates() {
  try {
    const res = await api.checkUpdate()
    if (res?.data?.hasUpdate) {
      updateInfo.value = res.data
      if (res.data.isMandatory) {
        isModalOpen.value = true
      }
    }
  } catch (err) {
    console.warn('Lỗi kiểm tra cập nhật:', err)
  }
}

function openModal() {
  isModalOpen.value = true
}

function dismissBanner() {
  showBanner.value = false
}

function handleBackdropClick() {
  if (!updateInfo.value?.isMandatory) {
    isModalOpen.value = false
  }
}

function handleDownload() {
  if (!updateInfo.value?.downloadUrl) {
    toast.warning('Liên kết tải về chưa sẵn sàng. Vui lòng liên hệ hỗ trợ kỹ thuật!')
    return
  }
  downloading.value = true
  window.open(updateInfo.value.downloadUrl, '_blank')
  setTimeout(() => {
    downloading.value = false
  }, 1000)
}

onMounted(() => {
  // Kiểm tra cập nhật sau 2 giây khi ứng dụng tải xong
  setTimeout(() => {
    checkForUpdates()
  }, 2000)
})

defineExpose({
  checkForUpdates,
  openModal
})
</script>
