<template>
  <div class="p-6 space-y-6 overflow-y-auto h-screen max-w-5xl mx-auto bg-slate-50 text-slate-800">
    <!-- Header -->
    <div class="border-b border-slate-200 pb-4">
      <h2 class="text-2xl font-black text-slate-900 tracking-tight">Cài Đặt Hệ Thống & VietQR</h2>
      <p class="text-xs text-slate-500 mt-0.5 font-medium">Thiết lập tài khoản nhận tiền ngân hàng, thông tin thương hiệu và an toàn dữ liệu</p>
    </div>

    <div class="grid grid-cols-1 lg:grid-cols-3 gap-6">
      
      <!-- Cột Trái & Giữa: Form Cài Đặt (2 Cột) -->
      <div class="lg:col-span-2 space-y-6">
        <form @submit.prevent="saveSettings" class="space-y-6">
          
          <!-- Card 1: Cấu hình VietQR Ngân Hàng -->
          <div class="p-5 rounded-2xl bg-white border border-slate-200 shadow-xs space-y-4">
            <div class="flex items-center gap-2 border-b border-slate-100 pb-3">
              <QrCode class="w-5 h-5 text-indigo-600" />
              <h3 class="font-bold text-slate-900 text-sm">Tài Khoản Nhận Tiền VietQR (NAPAS 247)</h3>
            </div>

            <div class="space-y-3.5 text-xs">
              <div>
                <label class="block text-slate-600 mb-1 font-medium">Ngân hàng thụ hưởng *</label>
                <select
                  v-model="setting.bankId"
                  @change="onBankChange"
                  class="w-full bg-white border border-slate-200 rounded-xl px-3 py-2.5 text-slate-800 font-medium focus:outline-none focus:border-indigo-500 shadow-2xs"
                >
                  <option v-for="b in banks" :key="b.id" :value="b.id">
                    {{ b.name }} ({{ b.id }})
                  </option>
                </select>
              </div>

              <div class="grid grid-cols-1 sm:grid-cols-2 gap-3.5">
                <div>
                  <label class="block text-slate-600 mb-1 font-medium">Số tài khoản ngân hàng *</label>
                  <input
                    v-model="setting.accountNo"
                    type="text"
                    required
                    placeholder="VD: 0987654321"
                    class="w-full bg-white border border-slate-200 rounded-xl px-3 py-2.5 text-slate-900 font-mono font-bold focus:outline-none focus:border-indigo-500 shadow-2xs"
                  />
                </div>
                <div>
                  <label class="block text-slate-600 mb-1 font-medium">Tên chủ tài khoản (In hoa) *</label>
                  <input
                    v-model="setting.accountName"
                    type="text"
                    required
                    placeholder="VD: NGUYEN VAN A"
                    class="w-full bg-white border border-slate-200 rounded-xl px-3 py-2.5 text-slate-900 uppercase font-bold focus:outline-none focus:border-indigo-500 shadow-2xs"
                  />
                </div>
              </div>

              <div>
                <label class="block text-slate-600 mb-1 font-medium">Mẫu giao diện mã VietQR</label>
                <div class="grid grid-cols-3 gap-2">
                  <button
                    type="button"
                    v-for="tpl in [
                      { id: 'compact2', label: 'Hiện đại (compact2)' },
                      { id: 'compact', label: 'Gọn gàng (compact)' },
                      { id: 'qr_only', label: 'Chỉ mã QR (qr_only)' }
                    ]"
                    :key="tpl.id"
                    @click="setting.qrTemplate = tpl.id"
                    class="py-2 px-2 rounded-xl text-xs font-semibold border transition text-center"
                    :class="setting.qrTemplate === tpl.id ? 'bg-indigo-50 border-indigo-200 text-indigo-600 font-bold shadow-2xs' : 'bg-slate-50 border-slate-200 text-slate-600 hover:text-slate-900 hover:bg-slate-100'"
                  >
                    {{ tpl.label }}
                  </button>
                </div>
              </div>
            </div>
          </div>

          <!-- Card 2: Thông Tin Doanh Nghiệp / Cửa Hàng -->
          <div class="p-5 rounded-2xl bg-white border border-slate-200 shadow-xs space-y-4">
            <div class="flex items-center gap-2 border-b border-slate-100 pb-3">
              <Store class="w-5 h-5 text-indigo-600" />
              <h3 class="font-bold text-slate-900 text-sm">Thông Tin Cửa Hàng / Doanh Nghiệp</h3>
            </div>

            <div class="space-y-3.5 text-xs">
              <div class="grid grid-cols-1 sm:grid-cols-2 gap-3.5">
                <div>
                  <label class="block text-slate-600 mb-1 font-medium">Tên cửa hàng / Thương hiệu *</label>
                  <input
                    v-model="setting.shopName"
                    type="text"
                    required
                    placeholder="VD: DIRO POS STORE"
                    class="w-full bg-white border border-slate-200 rounded-xl px-3 py-2.5 text-slate-900 font-bold focus:outline-none focus:border-indigo-500 shadow-2xs"
                  />
                </div>
                <div>
                  <label class="block text-slate-600 mb-1 font-medium">Số điện thoại hotline</label>
                  <input
                    v-model="setting.phone"
                    type="text"
                    placeholder="VD: 0987.654.321"
                    class="w-full bg-white border border-slate-200 rounded-xl px-3 py-2.5 text-slate-800 focus:outline-none focus:border-indigo-500 shadow-2xs"
                  />
                </div>
              </div>

              <div>
                <label class="block text-slate-600 mb-1 font-medium">Địa chỉ kinh doanh</label>
                <input
                  v-model="setting.address"
                  type="text"
                  placeholder="VD: 128 Nguyễn Văn Cừ, Quận 5, TP. HCM"
                  class="w-full bg-white border border-slate-200 rounded-xl px-3 py-2.5 text-slate-800 focus:outline-none focus:border-indigo-500 shadow-2xs"
                />
              </div>

              <div>
                <label class="block text-slate-600 mb-1 font-medium">Slogan / Khẩu hiệu</label>
                <input
                  v-model="setting.slogan"
                  type="text"
                  placeholder="VD: Giải pháp bán hàng thông minh - Tối ưu vận hành"
                  class="w-full bg-white border border-slate-200 rounded-xl px-3 py-2.5 text-slate-800 focus:outline-none focus:border-indigo-500 shadow-2xs"
                />
              </div>
            </div>
          </div>

          <!-- Submit Button Cài Đặt -->
          <div class="flex justify-end">
            <button
              type="submit"
              :disabled="saving"
              class="py-3 px-6 rounded-xl bg-indigo-600 hover:bg-indigo-700 disabled:opacity-50 text-white font-bold text-sm shadow-md shadow-indigo-500/25 transition flex items-center gap-2 active:scale-95 cursor-pointer"
            >
              <Save class="w-4 h-4" />
              {{ saving ? 'Đang lưu...' : 'Lưu Cấu Hình' }}
            </button>
          </div>

        </form>

        <!-- Card 3: Sao Lưu & Khôi Phục Dữ Liệu (Backup & Restore) -->
        <div class="p-5 rounded-2xl bg-white border border-slate-200 shadow-xs space-y-5">
          <div class="flex items-center justify-between border-b border-slate-100 pb-3">
            <div class="flex items-center gap-2">
              <Database class="w-5 h-5 text-emerald-600" />
              <div>
                <h3 class="font-bold text-slate-900 text-sm">Sao Lưu & Khôi Phục Dữ Liệu (SQLite Database)</h3>
                <p class="text-[11px] text-slate-500 font-medium">Bảo vệ an toàn cơ sở dữ liệu hóa đơn, chi phí, dịch vụ và sản phẩm</p>
              </div>
            </div>
            <button
              @click="loadBackupInfo"
              title="Làm mới trạng thái DB"
              class="p-2 text-slate-400 hover:text-slate-600 rounded-lg hover:bg-slate-100 transition"
            >
              <RefreshCw class="w-4 h-4" :class="{ 'animate-spin': loadingBackupInfo }" />
            </button>
          </div>

          <!-- Thống kê Database -->
          <div class="grid grid-cols-2 sm:grid-cols-4 gap-3">
            <div class="bg-slate-50 p-3 rounded-xl border border-slate-200/80">
              <span class="text-[10px] text-slate-500 block mb-0.5">Dung lượng file DB</span>
              <span class="text-sm font-bold text-emerald-600">{{ backupInfo?.fileSizeFormatted || '---' }}</span>
            </div>
            <div class="bg-slate-50 p-3 rounded-xl border border-slate-200/80">
              <span class="text-[10px] text-slate-500 block mb-0.5">Tổng số Đơn hàng</span>
              <span class="text-sm font-bold text-indigo-600">{{ backupInfo?.totalOrders ?? 0 }} đơn</span>
            </div>
            <div class="bg-slate-50 p-3 rounded-xl border border-slate-200/80">
              <span class="text-[10px] text-slate-500 block mb-0.5">Khoản mục Chi phí</span>
              <span class="text-sm font-bold text-rose-600">{{ backupInfo?.totalExpenses ?? 0 }} khoản</span>
            </div>
            <div class="bg-slate-50 p-3 rounded-xl border border-slate-200/80">
              <span class="text-[10px] text-slate-500 block mb-0.5">Dịch vụ & Sản phẩm</span>
              <span class="text-sm font-bold text-blue-600">{{ (backupInfo?.totalServices ?? 0) + (backupInfo?.totalProducts ?? 0) }} món</span>
            </div>
          </div>

          <!-- Các nút thao tác Sao lưu & Khôi phục -->
          <div class="grid grid-cols-1 sm:grid-cols-2 gap-4 pt-1">
            <!-- Nút Tải Sao Lưu -->
            <div class="p-4 rounded-xl bg-slate-50 border border-slate-200 flex flex-col justify-between space-y-3">
              <div>
                <div class="flex items-center gap-2 text-slate-900 font-bold text-xs mb-1">
                  <Download class="w-4 h-4 text-emerald-600" />
                  <span>1. Tải Bản Sao Lưu (.db)</span>
                </div>
                <p class="text-[11px] text-slate-500 leading-relaxed">
                  Xuất toàn bộ cơ sở dữ liệu hệ thống ra file nhị phân SQLite snapshot. Khuyến nghị tải về lưu trữ định kỳ hàng tuần.
                </p>
              </div>

              <button
                @click="downloadDbBackup"
                :disabled="downloadingBackup"
                class="w-full py-2.5 px-4 rounded-xl bg-emerald-600 hover:bg-emerald-500 disabled:opacity-50 text-white font-bold text-xs shadow-md shadow-emerald-600/20 transition flex items-center justify-center gap-2 cursor-pointer active:scale-95"
              >
                <Download class="w-4 h-4" />
                <span>{{ downloadingBackup ? 'Đang sao lưu...' : 'Tải File Sao Lưu (.db)' }}</span>
              </button>
            </div>

            <!-- Nút Khôi Phục Dữ Liệu -->
            <div class="p-4 rounded-xl bg-slate-50 border border-slate-200 flex flex-col justify-between space-y-3">
              <div>
                <div class="flex items-center gap-2 text-slate-900 font-bold text-xs mb-1">
                  <Upload class="w-4 h-4 text-indigo-600" />
                  <span>2. Khôi Phục Dữ Liệu (Restore)</span>
                </div>
                <p class="text-[11px] text-slate-500 leading-relaxed">
                  Chọn file <code class="text-indigo-600 font-bold">.db</code> đã sao lưu trước đó để nạp lại dữ liệu cũ hoặc chuyển sang máy tính mới.
                </p>
              </div>

              <div>
                <input
                  ref="fileInputRef"
                  type="file"
                  accept=".db"
                  @change="onFileSelected"
                  class="hidden"
                />
                <button
                  @click="triggerFileInput"
                  :disabled="restoringBackup"
                  class="w-full py-2.5 px-4 rounded-xl bg-white hover:bg-slate-100 border border-slate-200 text-indigo-600 font-bold text-xs transition flex items-center justify-center gap-2 cursor-pointer active:scale-95 shadow-2xs"
                >
                  <Upload class="w-4 h-4" />
                  <span>{{ restoringBackup ? 'Đang xử lý...' : 'Chọn File .db Để Khôi Phục' }}</span>
                </button>
              </div>
            </div>
          </div>

          <!-- Ghi chú bảo mật & an toàn -->
          <div class="flex items-start gap-2.5 bg-emerald-50 border border-emerald-200 p-3 rounded-xl text-emerald-800 text-[11px] leading-relaxed">
            <ShieldCheck class="w-4 h-4 shrink-0 mt-0.5 text-emerald-600" />
            <span>
              <strong>An toàn tuyệt đối:</strong> Hệ thống tự động tạo một bản sao dự phòng <code class="text-emerald-900 font-mono">.bak</code> trước khi tiến hành ghi đè dữ liệu mới, đảm bảo không bao giờ bị mất mát dữ liệu do nhầm lẫn.
            </span>
          </div>
        </div>

      </div>

      <!-- Cột Phải: Xem Trước Mã VietQR Test Trực Tiếp (Live Preview) -->
      <div class="space-y-4">
        <div class="p-5 rounded-2xl bg-white border border-slate-200 text-center shadow-xs">
          <h3 class="font-bold text-slate-900 text-sm mb-1">Xem Trước Mã VietQR</h3>
          <p class="text-[11px] text-slate-500 mb-3">Thử quét kiểm tra thông tin tài khoản và số tiền</p>

          <div class="p-3 bg-white rounded-xl shadow-inner max-w-[240px] mx-auto border-2 border-indigo-200">
            <img
              :src="previewQrUrl"
              alt="Mã QR xem trước"
              class="w-full h-auto object-contain rounded"
            />
          </div>

          <div class="mt-3.5 text-left text-xs bg-slate-50 p-3 rounded-xl border border-slate-200 space-y-1">
            <p class="text-slate-500">Ngân hàng: <span class="text-slate-900 font-bold">{{ setting.bankId }}</span></p>
            <p class="text-slate-500">Số tài khoản: <span class="text-indigo-600 font-mono font-bold">{{ setting.accountNo }}</span></p>
            <p class="text-slate-500">Chủ tài khoản: <span class="text-slate-900 uppercase font-bold">{{ setting.accountName }}</span></p>
            <p class="text-slate-500">Số tiền mẫu: <span class="text-indigo-600 font-bold">100.000 ₫</span></p>
          </div>
        </div>
      </div>

    </div>

    <!-- Modal Xác Nhận Khôi Phục Dữ Liệu -->
    <div
      v-if="showRestoreModal"
      class="fixed inset-0 z-50 flex items-center justify-center bg-slate-900/40 backdrop-blur-sm p-4 animate-fade-in"
    >
      <div class="bg-white border border-slate-200 rounded-2xl max-w-md w-full p-6 space-y-5 shadow-2xl">
        <div class="flex items-center gap-3 text-amber-500">
          <div class="p-3 bg-amber-50 rounded-xl border border-amber-200">
            <AlertTriangle class="w-6 h-6" />
          </div>
          <div>
            <h3 class="text-base font-bold text-slate-900">Xác Nhận Khôi Phục Dữ Liệu</h3>
            <p class="text-xs text-slate-500">Hành động này sẽ thay thế cơ sở dữ liệu hiện tại</p>
          </div>
        </div>

        <div class="bg-slate-50 p-3.5 rounded-xl border border-slate-200 text-xs space-y-2">
          <div class="flex justify-between">
            <span class="text-slate-500">File được chọn:</span>
            <span class="text-slate-900 font-mono font-bold">{{ selectedFile?.name }}</span>
          </div>
          <div class="flex justify-between">
            <span class="text-slate-500">Dung lượng:</span>
            <span class="text-emerald-600 font-mono">{{ Math.round((selectedFile?.size || 0) / 1024) }} KB</span>
          </div>
        </div>

        <p class="text-xs text-slate-600 leading-relaxed">
          Cơ sở dữ liệu của hệ thống sẽ được phục hồi theo file đã chọn. Bạn có chắc chắn muốn tiếp tục không?
        </p>

        <div class="flex items-center justify-end gap-3 pt-2">
          <button
            type="button"
            @click="cancelRestore"
            :disabled="restoringBackup"
            class="px-4 py-2.5 rounded-xl bg-slate-100 hover:bg-slate-200 text-slate-700 font-bold text-xs transition"
          >
            Hủy Bỏ
          </button>
          <button
            type="button"
            @click="confirmRestore"
            :disabled="restoringBackup"
            class="px-5 py-2.5 rounded-xl bg-indigo-600 hover:bg-indigo-700 text-white font-bold text-xs shadow-md shadow-indigo-500/25 transition flex items-center gap-2"
          >
            <RefreshCw v-if="restoringBackup" class="w-3.5 h-3.5 animate-spin" />
            <span>{{ restoringBackup ? 'Đang Khôi Phục...' : 'Đồng Ý Khôi Phục' }}</span>
          </button>
        </div>
      </div>
    </div>

    <!-- Toast message -->
    <div 
      v-if="toast" 
      class="fixed bottom-6 right-6 z-50 bg-emerald-600 text-white font-bold text-xs px-4 py-3 rounded-xl shadow-2xl flex items-center gap-2 animate-bounce-short"
    >
      <CheckCircle2 class="w-4 h-4" />
      <span>{{ toast }}</span>
    </div>

  </div>
</template>

<script setup>
import { ref, computed, onMounted } from 'vue'
import api from '@/services/api'
import {
  QrCode,
  Store,
  Save,
  Database,
  Download,
  Upload,
  RefreshCw,
  AlertTriangle,
  CheckCircle2,
  ShieldCheck
} from 'lucide-vue-next'

const setting = ref({
  shopName: 'DiroPos Store',
  address: '',
  phone: '',
  slogan: 'Nền tảng quản lý bán hàng thông minh',
  bankId: 'MB',
  bankName: 'MBBank',
  accountNo: '0987654321',
  accountName: 'NGUYEN THANH CONG',
  qrTemplate: 'compact2'
})

const banks = ref([])
const toast = ref('')
const saving = ref(false)

// Backup & Restore State
const backupInfo = ref(null)
const loadingBackupInfo = ref(false)
const downloadingBackup = ref(false)
const restoringBackup = ref(false)
const showRestoreModal = ref(false)
const selectedFile = ref(null)
const fileInputRef = ref(null)

function showToast(msg) {
  toast.value = msg
  setTimeout(() => {
    toast.value = ''
  }, 3500)
}

async function loadData() {
  try {
    const [setRes, bankRes] = await Promise.all([
      api.getSettings(),
      api.getPopularBanks()
    ])
    if (setRes.data) setting.value = setRes.data
    if (bankRes.data) banks.value = bankRes.data
  } catch (err) {
    console.error('Lỗi khi tải cài đặt:', err)
  }
}

async function loadBackupInfo() {
  loadingBackupInfo.value = true
  try {
    const res = await api.getBackupInfo()
    if (res.data) {
      backupInfo.value = res.data
    }
  } catch (err) {
    console.error('Lỗi khi tải thông tin backup:', err)
  } finally {
    loadingBackupInfo.value = false
  }
}

onMounted(() => {
  loadData()
  loadBackupInfo()
})

function onBankChange() {
  const found = banks.value.find(b => b.id === setting.value.bankId)
  if (found) {
    setting.value.bankName = found.name
  }
}

const previewQrUrl = computed(() => {
  const bank = setting.value.bankId || 'MB'
  const acc = setting.value.accountNo || '0987654321'
  const tpl = setting.value.qrTemplate || 'compact2'
  const name = encodeURIComponent(setting.value.accountName || 'DIRO POS')
  return `https://img.vietqr.io/image/${bank}-${acc}-${tpl}.png?amount=100000&addInfo=TEST%20POS&accountName=${name}`
})

async function saveSettings() {
  saving.value = true
  try {
    await api.updateSettings(setting.value)
    showToast('Đã lưu cấu hình DiroPos & VietQR thành công!')
  } catch (err) {
    alert('Lỗi khi lưu cài đặt: ' + (err.response?.data || err.message))
  } finally {
    saving.value = false
  }
}

// Chức năng tải bản sao lưu (.db)
async function downloadDbBackup() {
  downloadingBackup.value = true
  try {
    const response = await api.downloadBackup()
    const blob = new Blob([response.data], { type: 'application/octet-stream' })
    const downloadUrl = window.URL.createObjectURL(blob)
    const link = document.createElement('a')
    
    // Đặt tên file theo định dạng DiroPos
    const dateStr = new Date().toISOString().replace(/[-:T]/g, '').slice(0, 14)
    link.href = downloadUrl
    link.setAttribute('download', `diropos_backup_${dateStr}.db`)
    document.body.appendChild(link)
    link.click()
    document.body.removeChild(link)
    window.URL.revokeObjectURL(downloadUrl)

    showToast('Đã tải bản sao lưu cơ sở dữ liệu thành công!')
    loadBackupInfo()
  } catch (err) {
    console.error('Lỗi sao lưu:', err)
    alert('Không thể tải file sao lưu: ' + (err.response?.data || err.message))
  } finally {
    downloadingBackup.value = false
  }
}

// Chức năng khôi phục (.db)
function triggerFileInput() {
  if (fileInputRef.value) {
    fileInputRef.value.value = ''
    fileInputRef.value.click()
  }
}

function onFileSelected(event) {
  const file = event.target.files?.[0]
  if (!file) return

  if (!file.name.endsWith('.db')) {
    alert('Vui lòng chọn file có phần mở rộng .db')
    return
  }

  selectedFile.value = file
  showRestoreModal.value = true
}

function cancelRestore() {
  showRestoreModal.value = false
  selectedFile.value = null
}

async function confirmRestore() {
  if (!selectedFile.value) return
  restoringBackup.value = true

  try {
    const formData = new FormData()
    formData.append('file', selectedFile.value)

    const res = await api.restoreBackup(formData)
    showRestoreModal.value = false
    selectedFile.value = null
    showToast(res.data?.message || 'Đã khôi phục dữ liệu thành công!')
    
    await Promise.all([
      loadData(),
      loadBackupInfo()
    ])
  } catch (err) {
    console.error('Lỗi khi khôi phục:', err)
    alert('Lỗi khôi phục dữ liệu: ' + (err.response?.data || err.message))
  } finally {
    restoringBackup.value = false
  }
}
</script>
