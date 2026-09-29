<template>
  <div
    v-if="isOpen"
    class="fixed inset-0 z-50 flex items-center justify-center p-4 bg-slate-900/60 backdrop-blur-xs animate-fade-in"
  >
    <!-- Modal Card -->
    <div
      class="bg-white rounded-2xl shadow-2xl border border-slate-200 w-full max-w-2xl overflow-hidden flex flex-col max-h-[92vh] animate-scale-up"
    >
      <!-- Modal Header -->
      <div class="px-6 py-4 bg-gradient-to-r from-slate-900 via-indigo-950 to-slate-900 text-white flex items-center justify-between">
        <div class="flex items-center gap-3">
          <div class="w-10 h-10 rounded-xl bg-indigo-500/20 border border-indigo-400/30 flex items-center justify-center text-indigo-300">
            <Moon class="w-5 h-5" />
          </div>
          <div>
            <div class="flex items-center gap-2">
              <h3 class="font-extrabold text-base tracking-tight">Chốt Sổ & Đóng Ca Làm Việc</h3>
              <span class="px-2 py-0.5 rounded text-[10px] font-bold bg-amber-400/20 text-amber-300 border border-amber-400/30">
                Z-REPORT
              </span>
            </div>
            <p class="text-xs text-slate-300 mt-0.5">
              {{ currentDateTimeStr }}
            </p>
          </div>
        </div>

        <button
          @click="emitClose"
          class="p-2 text-slate-400 hover:text-white rounded-xl hover:bg-white/10 transition"
          title="Đóng modal"
        >
          <X class="w-5 h-5" />
        </button>
      </div>

      <!-- Modal Body -->
      <div class="p-6 overflow-y-auto space-y-5 text-slate-800 text-xs flex-1">
        
        <!-- Loading State -->
        <div v-if="loading" class="py-12 flex flex-col items-center justify-center space-y-3">
          <RefreshCw class="w-8 h-8 text-indigo-600 animate-spin" />
          <p class="text-slate-500 font-medium">Đang tổng hợp dữ liệu ca hôm nay...</p>
        </div>

        <div v-else class="space-y-5">
          <!-- Banner Tóm tắt Doanh thu & Két tiền -->
          <div class="grid grid-cols-2 sm:grid-cols-4 gap-3">
            <!-- Tổng Doanh Thu -->
            <div class="p-3.5 rounded-xl bg-indigo-50 border border-indigo-100">
              <span class="text-[11px] font-medium text-indigo-600 block mb-1">Tổng Doanh Thu</span>
              <span class="text-base font-black text-indigo-900 tracking-tight block">
                {{ formatCurrency(summary.revenue) }}
              </span>
              <span class="text-[10px] text-indigo-500 font-medium">{{ summary.ordersCount }} đơn hoàn tất</span>
            </div>

            <!-- Tiền Mặt Đã Thu -->
            <div class="p-3.5 rounded-xl bg-emerald-50 border border-emerald-100">
              <span class="text-[11px] font-medium text-emerald-700 block mb-1">Tiền Mặt Đã Thu</span>
              <span class="text-base font-black text-emerald-800 tracking-tight block">
                {{ formatCurrency(summary.cashTotal) }}
              </span>
              <span class="text-[10px] text-emerald-600 font-medium">Khách trả tiền mặt</span>
            </div>

            <!-- Chuyển Khoản VietQR -->
            <div class="p-3.5 rounded-xl bg-blue-50 border border-blue-100">
              <span class="text-[11px] font-medium text-blue-700 block mb-1">Chuyển Khoản VietQR</span>
              <span class="text-base font-black text-blue-800 tracking-tight block">
                {{ formatCurrency(summary.vietQrTotal) }}
              </span>
              <span class="text-[10px] text-blue-600 font-medium">Tiền vào tài khoản</span>
            </div>

            <!-- Chi Tiêu Trong Ca -->
            <div class="p-3.5 rounded-xl bg-rose-50 border border-rose-100">
              <span class="text-[11px] font-medium text-rose-700 block mb-1">Chi Tiêu Trong Ca</span>
              <span class="text-base font-black text-rose-800 tracking-tight block">
                {{ formatCurrency(summary.expense) }}
              </span>
              <span class="text-[10px] text-rose-600 font-medium">{{ summary.expenses?.length || 0 }} khoản chi</span>
            </div>
          </div>

          <!-- Hộp Đối Soát Két Tiền Bàn Giao (Rất quan trọng cho chủ tiệm & thu ngân) -->
          <div class="p-4 rounded-xl bg-slate-900 text-white space-y-3">
            <div class="flex items-center justify-between border-b border-slate-800 pb-2.5">
              <div class="flex items-center gap-2">
                <Wallet class="w-4 h-4 text-emerald-400" />
                <span class="font-bold text-xs">TIỀN MẶT CẦN KIỂM ĐẾM TRONG KÉT</span>
              </div>
              <span class="text-[11px] text-slate-400 font-mono">(Tiền mặt thu - Tiền mặt chi)</span>
            </div>

            <div class="flex items-baseline justify-between">
              <div>
                <p class="text-[11px] text-slate-300">Số tiền mặt thực tế phải có trong ngăn kéo:</p>
                <p class="text-[10px] text-slate-400 mt-0.5">Không bao gồm tiền lẻ đầu ca (nếu có)</p>
              </div>
              <div class="text-right">
                <span class="text-2xl font-black text-emerald-400 tracking-tight">
                  {{ formatCurrency(expectedDrawerCash) }}
                </span>
              </div>
            </div>
          </div>

          <!-- Thông tin Bàn Giao Ca -->
          <div class="bg-slate-50 border border-slate-200 rounded-xl p-4 space-y-3">
            <h4 class="font-bold text-slate-900 text-xs flex items-center gap-1.5">
              <FileText class="w-3.5 h-3.5 text-indigo-600" />
              Thông Tin Bàn Giao Ca
            </h4>

            <div class="grid grid-cols-1 sm:grid-cols-2 gap-3">
              <div>
                <label class="block text-slate-600 mb-1 font-medium text-[11px]">Người đóng ca / Thu ngân</label>
                <input
                  v-model="shiftStaff"
                  type="text"
                  placeholder="Họ tên người trực ca..."
                  class="w-full bg-white border border-slate-200 rounded-lg px-3 py-2 text-slate-800 font-medium focus:outline-none focus:border-indigo-500"
                />
              </div>

              <div>
                <label class="block text-slate-600 mb-1 font-medium text-[11px]">Tiền mặt lẻ để lại ca sau (nếu có)</label>
                <input
                  v-model="initialFloat"
                  type="number"
                  placeholder="VD: 500000"
                  class="w-full bg-white border border-slate-200 rounded-lg px-3 py-2 text-slate-800 font-mono font-medium focus:outline-none focus:border-indigo-500"
                />
              </div>
            </div>

            <div>
              <label class="block text-slate-600 mb-1 font-medium text-[11px]">Ghi chú bàn giao ca</label>
              <input
                v-model="shiftNote"
                type="text"
                placeholder="VD: Đã kiểm đếm đủ két, kho còn 5 lọ sáp, bàn giao cho ca tối..."
                class="w-full bg-white border border-slate-200 rounded-lg px-3 py-2 text-slate-800 focus:outline-none focus:border-indigo-500"
              />
            </div>
          </div>

          <!-- Thông Báo Đóng Ca Thành Công (nếu đã tải) -->
          <div
            v-if="backupCompleted"
            class="p-4 rounded-xl bg-emerald-50 border border-emerald-200 text-emerald-800 space-y-2 animate-fade-in"
          >
            <div class="flex items-center gap-2 font-bold text-xs text-emerald-900">
              <CheckCircle2 class="w-4 h-4 text-emerald-600" />
              <span>Đóng ca thành công! File dữ liệu (.db) đã được lưu vào máy tính</span>
            </div>
            <p class="text-[11px] leading-relaxed text-emerald-700">
              Hệ thống đã tự động xuất snapshot dữ liệu an toàn. Bạn có thể in phiếu kết ca bên dưới để lưu vào sổ thu chi hoặc bàn giao cho chủ quán.
            </p>
          </div>

        </div>
      </div>

      <!-- Modal Footer -->
      <div class="p-4 bg-slate-50 border-t border-slate-200 flex flex-wrap items-center justify-between gap-3">
        <!-- Nút In Phiếu Kết Ca -->
        <button
          @click="printShiftReport"
          :disabled="loading"
          class="py-2.5 px-4 rounded-xl bg-white hover:bg-slate-100 border border-slate-200 text-slate-700 font-bold text-xs transition flex items-center gap-2 shadow-2xs cursor-pointer active:scale-95 disabled:opacity-50"
        >
          <Printer class="w-4 h-4 text-indigo-600" />
          <span>In Phiếu Kết Ca</span>
        </button>

        <div class="flex items-center gap-2">
          <button
            @click="emitClose"
            class="py-2.5 px-4 rounded-xl bg-slate-200 hover:bg-slate-300 text-slate-700 font-semibold text-xs transition cursor-pointer"
          >
            Bỏ qua
          </button>

          <!-- Nút Xác Nhận Đóng Ca & Tải File Sao Lưu -->
          <button
            @click="executeShiftCloseAndBackup"
            :disabled="loading || downloading"
            class="py-2.5 px-5 rounded-xl bg-gradient-to-r from-emerald-600 to-teal-600 hover:from-emerald-500 hover:to-teal-500 text-white font-extrabold text-xs shadow-md shadow-emerald-600/25 transition flex items-center gap-2 cursor-pointer active:scale-95 disabled:opacity-50"
          >
            <Download v-if="!downloading" class="w-4 h-4" />
            <RefreshCw v-else class="w-4 h-4 animate-spin" />
            <span>{{ downloading ? 'Đang xuất dữ liệu...' : 'Xác Nhận Đóng Ca & Tải File (.db)' }}</span>
          </button>
        </div>
      </div>
    </div>

    <!-- Hidden Print Template (K80 / A4) -->
    <div id="shift-print-template" class="hidden">
      <div style="font-family: Arial, sans-serif; padding: 20px; max-width: 320px; margin: 0 auto; color: #111;">
        <div style="text-align: center; border-bottom: 2px dashed #000; padding-bottom: 12px; margin-bottom: 12px;">
          <h2 style="margin: 0; font-size: 18px; font-weight: bold; text-transform: uppercase;">PHIẾU BÀN GIAO KẾT CA</h2>
          <p style="margin: 4px 0 0 0; font-size: 13px; font-weight: bold;">(Z-REPORT DIROPOS)</p>
          <p style="margin: 4px 0 0 0; font-size: 11px;">Thời gian: {{ currentDateTimeStr }}</p>
          <p style="margin: 2px 0 0 0; font-size: 11px;">Nhân viên trực ca: <b>{{ shiftStaff || 'Thu ngân' }}</b></p>
        </div>

        <div style="font-size: 12px; line-height: 1.6; border-bottom: 1px dashed #000; padding-bottom: 10px; margin-bottom: 10px;">
          <div style="display: flex; justify-content: space-between;">
            <span>Tổng số đơn hàng:</span>
            <b>{{ summary.ordersCount }} đơn</b>
          </div>
          <div style="display: flex; justify-content: space-between;">
            <span>Tổng doanh thu:</span>
            <b style="font-size: 14px;">{{ formatCurrency(summary.revenue) }}</b>
          </div>
          <div style="display: flex; justify-content: space-between; padding-left: 8px; color: #333;">
            <span>+ Tiền mặt thu:</span>
            <span>{{ formatCurrency(summary.cashTotal) }}</span>
          </div>
          <div style="display: flex; justify-content: space-between; padding-left: 8px; color: #333;">
            <span>+ VietQR chuyển khoản:</span>
            <span>{{ formatCurrency(summary.vietQrTotal) }}</span>
          </div>
          <div style="display: flex; justify-content: space-between; color: #c00;">
            <span>- Tổng chi tiêu trong ca:</span>
            <span>-{{ formatCurrency(summary.expense) }}</span>
          </div>
        </div>

        <div style="font-size: 13px; border-bottom: 2px dashed #000; padding-bottom: 10px; margin-bottom: 12px;">
          <div style="display: flex; justify-content: space-between; font-weight: bold; font-size: 14px;">
            <span>TIỀN MẶT BÀN GIAO:</span>
            <span>{{ formatCurrency(expectedDrawerCash) }}</span>
          </div>
          <div v-if="initialFloat > 0" style="display: flex; justify-content: space-between; font-size: 11px; margin-top: 4px;">
            <span>Tiền thối để lại ca sau:</span>
            <span>{{ formatCurrency(initialFloat) }}</span>
          </div>
          <div v-if="shiftNote" style="margin-top: 6px; font-size: 11px; font-style: italic;">
            Ghi chú: {{ shiftNote }}
          </div>
        </div>

        <div style="display: flex; justify-content: space-between; text-align: center; font-size: 11px; margin-top: 20px;">
          <div>
            <b>Người Giao Ca</b><br/>
            <span style="font-size: 9px; font-style: italic;">(Ký & ghi rõ họ tên)</span>
            <br/><br/><br/>
            <span>{{ shiftStaff || '......................' }}</span>
          </div>
          <div>
            <b>Người Nhận Ca / Quản Lý</b><br/>
            <span style="font-size: 9px; font-style: italic;">(Ký & ghi rõ họ tên)</span>
            <br/><br/><br/>
            <span>......................</span>
          </div>
        </div>

        <div style="text-align: center; margin-top: 25px; font-size: 10px; color: #666; border-top: 1px dotted #ccc; padding-top: 8px;">
          Dữ liệu đã sao lưu an toàn vào hệ thống DiroPos
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, computed, watch } from 'vue'
import api from '@/services/api'
import {
  Moon,
  X,
  RefreshCw,
  Wallet,
  FileText,
  CheckCircle2,
  Printer,
  Download
} from 'lucide-vue-next'

const props = defineProps({
  isOpen: {
    type: Boolean,
    default: false
  }
})

const emit = defineEmits(['close', 'completed'])

const loading = ref(false)
const downloading = ref(false)
const backupCompleted = ref(false)

const shiftStaff = ref(localStorage.getItem('diropos_staff_name') || 'Thu ngân')
const shiftNote = ref('')
const initialFloat = ref(0)

const summary = ref({
  dateFormatted: '',
  ordersCount: 0,
  revenue: 0,
  expense: 0,
  netProfit: 0,
  vietQrTotal: 0,
  cashTotal: 0,
  expenses: []
})

const currentDateTimeStr = computed(() => {
  const d = new Date()
  const day = String(d.getDate()).padStart(2, '0')
  const month = String(d.getMonth() + 1).padStart(2, '0')
  const year = d.getFullYear()
  const hours = String(d.getHours()).padStart(2, '0')
  const mins = String(d.getMinutes()).padStart(2, '0')
  return `${hours}:${mins} - Ngày ${day}/${month}/${year}`
})

const expectedDrawerCash = computed(() => {
  const cashIn = summary.value.cashTotal || 0
  const cashOut = summary.value.expense || 0
  return Math.max(0, cashIn - cashOut)
})

function formatCurrency(val) {
  if (!val) return '0 ₫'
  return new Intl.NumberFormat('vi-VN', { style: 'currency', currency: 'VND' }).format(val)
}

function emitClose() {
  emit('close')
}

async function loadShiftData() {
  loading.value = true
  backupCompleted.value = false
  try {
    const today = new Date().toISOString().slice(0, 10)
    const res = await api.getDayDetail(today)
    if (res?.data) {
      summary.value = res.data
    }
  } catch (err) {
    console.error('Lỗi tải dữ liệu đóng ca:', err)
  } finally {
    loading.value = false
  }
}

watch(
  () => props.isOpen,
  (newVal) => {
    if (newVal) {
      loadShiftData()
    }
  }
)

async function executeShiftCloseAndBackup() {
  downloading.value = true
  try {
    // 1. Tải bản sao lưu SQLite database
    const response = await api.downloadBackup()
    const blob = new Blob([response.data], { type: 'application/octet-stream' })
    const downloadUrl = window.URL.createObjectURL(blob)
    const link = document.createElement('a')
    
    // Tên file rõ ràng: DiroPos_DongCa_2026-09-29_14h30.db
    const now = new Date()
    const yyyy = now.getFullYear()
    const mm = String(now.getMonth() + 1).padStart(2, '0')
    const dd = String(now.getDate()).padStart(2, '0')
    const hh = String(now.getHours()).padStart(2, '0')
    const min = String(now.getMinutes()).padStart(2, '0')
    const fileName = `DiroPos_DongCa_${yyyy}-${mm}-${dd}_${hh}h${min}.db`

    link.href = downloadUrl
    link.setAttribute('download', fileName)
    document.body.appendChild(link)
    link.click()
    document.body.removeChild(link)
    window.URL.revokeObjectURL(downloadUrl)

    // 2. Lưu trạng thái
    backupCompleted.value = true
    if (shiftStaff.value) {
      localStorage.setItem('diropos_staff_name', shiftStaff.value)
    }
    localStorage.setItem('diropos_last_shift_close', now.toISOString())

    emit('completed', {
      fileName,
      time: now.toISOString(),
      summary: summary.value
    })
  } catch (err) {
    console.error('Lỗi khi đóng ca & tải sao lưu:', err)
    alert('Không thể xuất file sao lưu: ' + (err.response?.data || err.message))
  } finally {
    downloading.value = false
  }
}

function printShiftReport() {
  const printContent = document.getElementById('shift-print-template')
  if (!printContent) return

  const printWindow = window.open('', '', 'width=400,height=600')
  printWindow.document.write(`
    <html>
      <head>
        <title>Phiếu Bàn Giao Kết Ca</title>
        <style>
          body { margin: 0; font-family: -apple-system, BlinkMacSystemFont, "Segoe UI", Roboto, sans-serif; }
          @media print {
            @page { margin: 0; }
            body { margin: 10mm; }
          }
        </style>
      </head>
      <body>
        ${printContent.innerHTML}
        <script>
          window.onload = function() {
            window.print();
            window.close();
          };
        <\/script>
      </body>
    </html>
  `)
  printWindow.document.close()
}
</script>

<style scoped>
@keyframes fadeIn {
  from { opacity: 0; }
  to { opacity: 1; }
}
@keyframes scaleUp {
  from { transform: scale(0.96); opacity: 0; }
  to { transform: scale(1); opacity: 1; }
}
.animate-fade-in {
  animation: fadeIn 0.15s ease-out forwards;
}
.animate-scale-up {
  animation: scaleUp 0.2s cubic-bezier(0.16, 1, 0.3, 1) forwards;
}
</style>
