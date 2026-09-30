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

        <!-- Card 3: Đóng Ca Cuối Ngày & Dữ Liệu Đám Mây (Z-Report & Cloud Backup) -->
        <div class="p-5 rounded-2xl bg-white border border-slate-200 shadow-xs space-y-4">
          <!-- Header Card 3 -->
          <div class="flex items-center justify-between border-b border-slate-100 pb-3">
            <div class="flex items-center gap-2.5">
              <div class="w-8 h-8 rounded-xl bg-indigo-50 border border-indigo-100/80 flex items-center justify-center text-indigo-600">
                <Moon class="w-4 h-4" />
              </div>
              <div>
                <h3 class="font-bold text-slate-900 text-sm">Đóng Ca Cuối Ngày & Dữ Liệu Đám Mây</h3>
                <p class="text-[11px] text-slate-500 font-medium">Chốt ca làm việc, đối soát két tiền và tự động bảo vệ dữ liệu trên Cloud</p>
              </div>
            </div>

            <button
              type="button"
              @click="loadBackupInfo(); loadCloudBackups();"
              title="Làm mới trạng thái dữ liệu"
              class="p-2 text-slate-400 hover:text-slate-600 rounded-xl hover:bg-slate-100 transition cursor-pointer"
            >
              <RefreshCw class="w-4 h-4" :class="{ 'animate-spin': loadingBackupInfo || loadingCloudBackups }" />
            </button>
          </div>

          <!-- 1. Hero Action Banner: Đóng Ca Làm Việc -->
          <div class="p-4 rounded-2xl bg-gradient-to-r from-indigo-50/80 via-slate-50/60 to-emerald-50/60 border border-indigo-100/80 flex flex-col sm:flex-row sm:items-center justify-between gap-3.5">
            <div class="space-y-1">
              <div class="flex items-center gap-2">
                <span class="font-bold text-slate-900 text-xs flex items-center gap-1.5">
                  <Moon class="w-3.5 h-3.5 text-indigo-600" />
                  <span>Chốt Sổ Ca & Bàn Giao (Z-Report)</span>
                </span>
                <span class="px-2 py-0.5 rounded-full text-[9px] font-extrabold bg-emerald-100 text-emerald-700 border border-emerald-200/60">
                  TỰ ĐỘNG ĐỒNG BỘ CLOUD
                </span>
              </div>
              <p class="text-[11px] text-slate-500 leading-relaxed max-w-md">
                Tổng hợp tiền mặt trong két, doanh thu VietQR và tự động lưu bản sao lưu nén an toàn lên Cloud.
              </p>
            </div>

            <button
              type="button"
              @click="isShiftModalOpen = true"
              class="py-2.5 px-5 rounded-xl bg-gradient-to-r from-emerald-600 to-teal-600 hover:from-emerald-500 hover:to-teal-500 text-white font-bold text-xs shadow-md shadow-emerald-600/20 transition flex items-center justify-center gap-2 cursor-pointer active:scale-95 shrink-0"
            >
              <Moon class="w-4 h-4" />
              <span>Mở Giao Diện Đóng Ca</span>
            </button>
          </div>

          <!-- 2. Thống kê mini 4 cột đồng bộ -->
          <div class="grid grid-cols-2 sm:grid-cols-4 gap-2.5">
            <div class="bg-slate-50 p-2.5 rounded-xl border border-slate-200/80">
              <span class="text-[10px] text-slate-400 block mb-0.5">Dung lượng Database</span>
              <span class="text-xs font-bold text-slate-800">{{ backupInfo?.fileSizeFormatted || '---' }}</span>
            </div>
            <div class="bg-slate-50 p-2.5 rounded-xl border border-slate-200/80">
              <span class="text-[10px] text-slate-400 block mb-0.5">Tổng đơn hoàn tất</span>
              <span class="text-xs font-bold text-indigo-600">{{ backupInfo?.totalOrders ?? 0 }} đơn</span>
            </div>
            <div class="bg-slate-50 p-2.5 rounded-xl border border-slate-200/80">
              <span class="text-[10px] text-slate-400 block mb-0.5">Khoản mục chi tiêu</span>
              <span class="text-xs font-bold text-rose-600">{{ backupInfo?.totalExpenses ?? 0 }} khoản</span>
            </div>
            <div class="bg-slate-50 p-2.5 rounded-xl border border-slate-200/80">
              <span class="text-[10px] text-slate-400 block mb-0.5">Lần đóng ca gần nhất</span>
              <span class="text-xs font-bold text-slate-700 truncate block">{{ lastShiftCloseFormatted }}</span>
            </div>
          </div>

          <!-- 3. Khu vực Bản Sao Lưu Đám Mây (Cloud Supabase) -->
          <div class="space-y-2.5 pt-2">
            <div class="flex items-center justify-between">
              <div class="flex items-center gap-2">
                <Cloud class="w-4 h-4 text-indigo-600" />
                <h4 class="font-bold text-slate-900 text-xs">Bản Sao Lưu Cloud Supabase</h4>
                <span class="px-2 py-0.5 rounded-full text-[9px] font-semibold bg-slate-100 text-slate-600 border border-slate-200">
                  {{ cloudBackups.length }}/3 bản gần nhất
                </span>
              </div>

              <!-- Nút Sao Lưu Ngay & Khôi Phục File .db Thủ Công -->
              <div class="flex items-center gap-2">
                <button
                  type="button"
                  @click="triggerManualCloudBackup"
                  :disabled="uploadingCloudBackup"
                  class="py-1.5 px-3 rounded-xl bg-indigo-50 hover:bg-indigo-100/80 text-indigo-700 border border-indigo-200/70 font-bold text-[11px] transition flex items-center gap-1.5 cursor-pointer disabled:opacity-50 active:scale-95"
                  title="Sao lưu ngay lập tức lên Cloud mà không cần đóng ca"
                >
                  <CloudUpload class="w-3.5 h-3.5 text-indigo-600" />
                  <span>{{ uploadingCloudBackup ? 'Đang gửi...' : 'Sao Lưu Ngay' }}</span>
                </button>

                <button
                  type="button"
                  @click="triggerFileInput"
                  :disabled="restoringBackup"
                  class="py-1.5 px-3 rounded-xl bg-white hover:bg-slate-50 text-slate-700 border border-slate-200 font-bold text-[11px] transition flex items-center gap-1.5 cursor-pointer disabled:opacity-50 active:scale-95 shadow-2xs"
                  title="Khôi phục từ file .db thủ công có sẵn trên máy"
                >
                  <Upload class="w-3.5 h-3.5 text-slate-500" />
                  <span class="hidden sm:inline">Khôi Phục File .db</span>
                  <span class="sm:hidden">Nạp File</span>
                </button>
              </div>
            </div>

            <!-- Hidden File Input for Manual .db Restore -->
            <input
              ref="fileInputRef"
              type="file"
              accept=".db"
              @change="onFileSelected"
              class="hidden"
            />

            <!-- Danh sách các file backup dạng Bảng thẻ tinh tế -->
            <div v-if="loadingCloudBackups" class="py-8 text-center text-slate-400 text-xs flex flex-col items-center gap-2 bg-slate-50/50 rounded-xl border border-slate-200/60">
              <RefreshCw class="w-5 h-5 animate-spin text-indigo-500" />
              <span>Đang kiểm tra danh sách bản sao lưu từ Cloud...</span>
            </div>

            <div v-else-if="cloudBackups.length === 0" class="p-6 rounded-xl bg-slate-50/70 border border-dashed border-slate-200 text-center text-xs">
              <Cloud class="w-6 h-6 mx-auto mb-1 text-slate-400 stroke-1" />
              <p class="font-medium text-slate-600">Chưa có bản sao lưu nào trên Đám mây</p>
              <p class="text-[11px] text-slate-400 mt-0.5">Mỗi lần bạn đóng ca, hệ thống sẽ tự động đồng bộ file nén lên đây.</p>
            </div>

            <div v-else class="divide-y divide-slate-100 border border-slate-200 rounded-xl overflow-hidden bg-white shadow-2xs">
              <div
                v-for="(b, idx) in cloudBackups"
                :key="b.fileName"
                class="flex flex-col sm:flex-row sm:items-center justify-between p-3 hover:bg-slate-50/80 transition gap-2"
              >
                <div class="flex items-center gap-3">
                  <div 
                    class="w-7 h-7 rounded-lg flex items-center justify-center font-bold text-[10px] shrink-0"
                    :class="idx === 0 ? 'bg-indigo-600 text-white shadow-xs' : 'bg-slate-100 text-slate-500'"
                  >
                    {{ idx === 0 ? 'MỚI' : `#${idx + 1}` }}
                  </div>
                  <div>
                    <div class="flex items-center gap-2">
                      <p class="font-semibold text-slate-800 font-mono text-xs">{{ b.fileName }}</p>
                      <span class="px-1.5 py-0.2 rounded text-[10px] font-bold bg-slate-100 text-slate-600 font-mono">
                        {{ b.fileSizeFormatted }}
                      </span>
                    </div>
                    <p class="text-[11px] text-slate-400 mt-0.5">
                      Thời gian sao lưu: {{ formatBackupDate(b.createdAt) }}
                    </p>
                  </div>
                </div>

                <div class="flex items-center gap-2 self-end sm:self-center shrink-0">
                  <a
                    :href="b.downloadUrl"
                    target="_blank"
                    class="px-2.5 py-1.5 rounded-lg text-slate-600 hover:text-slate-900 hover:bg-slate-100 text-[11px] font-semibold transition flex items-center gap-1 border border-slate-200 cursor-pointer"
                    title="Tải file zip về máy tính"
                  >
                    <Download class="w-3.5 h-3.5" />
                    <span>Tải về</span>
                  </a>

                  <button
                    type="button"
                    @click="confirmRestoreCloudBackup(b)"
                    :disabled="restoringCloudBackup"
                    class="px-3 py-1.5 rounded-lg bg-emerald-600 hover:bg-emerald-500 text-white text-[11px] font-bold transition flex items-center gap-1.5 cursor-pointer disabled:opacity-50 active:scale-95 shadow-2xs"
                    title="Nạp lại toàn bộ dữ liệu từ bản sao lưu này"
                  >
                    <RefreshCw class="w-3.5 h-3.5" :class="{ 'animate-spin': restoringCloudBackup }" />
                    <span>Khôi phục</span>
                  </button>
                </div>
              </div>
            </div>
          </div>

          <!-- 4. Ghi chú bảo mật tối giản & thanh lịch -->
          <div class="flex items-center justify-between text-[11px] text-slate-500 px-1 pt-1 border-t border-slate-100">
            <div class="flex items-center gap-1.5">
              <ShieldCheck class="w-3.5 h-3.5 text-emerald-600 shrink-0" />
              <span>Dữ liệu được nén tự động mỗi khi đóng ca. Lưu trữ tối đa 3 bản mới nhất trên Supabase Cloud.</span>
            </div>
            <span class="text-[10px] text-slate-400 hidden sm:inline">An toàn • Không chiếm bộ nhớ máy</span>
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

        <!-- Card Giấy Phép Bản Quyền DiroPos -->
        <div class="p-5 rounded-2xl bg-white border border-slate-200 shadow-xs space-y-3.5">
          <div class="flex items-center justify-between border-b border-slate-100 pb-2.5">
            <div class="flex items-center gap-2">
              <ShieldCheck class="w-4 h-4 text-indigo-600" />
              <h3 class="font-bold text-slate-900 text-xs">Giấy Phép Bản Quyền DiroPos</h3>
            </div>
            <span
              class="px-2 py-0.5 rounded text-[10px] font-extrabold"
              :class="licenseInfo?.isValid ? 'bg-emerald-50 text-emerald-600 border border-emerald-200' : 'bg-rose-50 text-rose-600 border border-rose-200'"
            >
              {{ licenseInfo?.status === 'Active' ? 'HOẠT ĐỘNG' : licenseInfo?.status || 'TRIAL' }}
            </span>
          </div>

          <div class="space-y-2 text-xs">
            <div class="flex justify-between items-center text-slate-600">
              <span>Gói cước:</span>
              <b class="text-slate-900 uppercase">{{ licenseInfo?.planType || 'Dùng Thử' }}</b>
            </div>
            <div class="flex justify-between items-center text-slate-600">
              <span>Hạn sử dụng:</span>
              <span class="font-bold text-slate-800">{{ formatDate(licenseInfo?.expiresAt) }}</span>
            </div>
            <div class="flex justify-between items-center text-slate-600">
              <span>Thời gian còn lại:</span>
              <span class="font-black text-indigo-600">{{ licenseInfo?.daysRemaining ?? 0 }} ngày</span>
            </div>
            <div class="flex justify-between items-center text-slate-600">
              <span>Mã thiết bị:</span>
              <span class="font-mono text-[10px] text-slate-500 bg-slate-100 px-1.5 py-0.5 rounded">{{ licenseInfo?.hardwareId }}</span>
            </div>
          </div>

          <button
            type="button"
            @click="showLicenseModal = true"
            class="w-full py-2 px-3 rounded-xl bg-slate-100 hover:bg-slate-200 text-slate-700 font-bold text-xs transition flex items-center justify-center gap-1.5 cursor-pointer active:scale-95 border border-slate-200"
          >
            <Key class="w-3.5 h-3.5 text-indigo-600" />
            <span>Nhập Mã Kích Hoạt / Gia Hạn</span>
          </button>
        </div>

        <!-- Card Phiên Bản Phần Mềm & Cập Nhật -->
        <div class="p-5 rounded-2xl bg-white border border-slate-200 shadow-xs space-y-3.5">
          <div class="flex items-center justify-between border-b border-slate-100 pb-2.5">
            <div class="flex items-center gap-2">
              <Sparkles class="w-4 h-4 text-indigo-600" />
              <h3 class="font-bold text-slate-900 text-xs">Phiên Bản Phần Mềm</h3>
            </div>
            <span class="px-2 py-0.5 rounded text-[10px] font-extrabold bg-indigo-50 text-indigo-600 border border-indigo-200">
              v{{ systemVersionInfo.version }}
            </span>
          </div>

          <div class="space-y-2 text-xs">
            <div class="flex justify-between items-center text-slate-600">
              <span>Hệ thống:</span>
              <b class="text-slate-900">{{ systemVersionInfo.appName }}</b>
            </div>
            <div class="flex justify-between items-center text-slate-600">
              <span>Phiên bản:</span>
              <span class="font-bold text-slate-800">v{{ systemVersionInfo.version }} ({{ systemVersionInfo.edition }})</span>
            </div>
            <div class="flex justify-between items-center text-slate-600">
              <span>Ngày build:</span>
              <span class="text-slate-700">{{ systemVersionInfo.buildDate }}</span>
            </div>
          </div>

          <button
            type="button"
            @click="checkManualUpdate"
            :disabled="checkingUpdate"
            class="w-full py-2 px-3 rounded-xl bg-indigo-50 hover:bg-indigo-100 text-indigo-700 font-bold text-xs transition flex items-center justify-center gap-1.5 cursor-pointer active:scale-95 border border-indigo-200 disabled:opacity-50"
          >
            <RefreshCw class="w-3.5 h-3.5" :class="{ 'animate-spin': checkingUpdate }" />
            <span>{{ checkingUpdate ? 'Đang kiểm tra...' : 'Kiểm Tra Bản Cập Nhật Mới' }}</span>
          </button>
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

    <!-- Modal Đóng Ca Cuối Ngày -->
    <ShiftCloseModal
      :isOpen="isShiftModalOpen"
      @close="isShiftModalOpen = false"
      @completed="onShiftCompleted"
    />

    <!-- Modal Kích Hoạt / Gia Hạn Bản Quyền -->
    <div
      v-if="showLicenseModal"
      class="fixed inset-0 z-50 flex items-center justify-center bg-slate-900/40 backdrop-blur-xs p-4 animate-fade-in"
    >
      <div class="bg-white border border-slate-200 rounded-2xl max-w-md w-full p-6 space-y-4 shadow-2xl">
        <div class="flex items-center justify-between border-b border-slate-100 pb-3">
          <div class="flex items-center gap-2">
            <Key class="w-5 h-5 text-indigo-600" />
            <h3 class="font-extrabold text-slate-900 text-sm">Kích Hoạt / Gia Hạn Bản Quyền DiroPos</h3>
          </div>
          <button
            type="button"
            @click="showLicenseModal = false"
            class="text-slate-400 hover:text-slate-600 p-1 rounded-lg"
          >
            ✕
          </button>
        </div>

        <div class="space-y-3 text-xs">
          <div class="bg-slate-50 p-3 rounded-xl border border-slate-200 space-y-1 text-slate-600">
            <p>Mã quán: <b class="text-indigo-600 font-mono">{{ licenseInfo?.shopCode }}</b></p>
            <p>Mã thiết bị: <span class="font-mono text-slate-800">{{ licenseInfo?.hardwareId }}</span></p>
            <p>Hotline hỗ trợ: <b class="text-emerald-600">{{ licenseInfo?.supportHotline || '0987.654.321' }}</b></p>
          </div>

          <div>
            <label class="block font-medium text-slate-700 mb-1">Dán mã kích hoạt (License Key) do Admin cấp:</label>
            <textarea
              v-model="licenseInputKey"
              rows="3"
              placeholder="VD: eyJzaG9wQ29kZSI6..."
              class="w-full bg-slate-50 border border-slate-200 rounded-xl p-2.5 text-xs font-mono text-slate-900 focus:outline-none focus:border-indigo-500 focus:bg-white resize-none"
            ></textarea>
          </div>

          <div class="flex items-center justify-end gap-2 pt-2">
            <button
              type="button"
              @click="showLicenseModal = false"
              class="px-4 py-2 rounded-xl bg-slate-100 hover:bg-slate-200 text-slate-700 font-bold text-xs"
            >
              Hủy
            </button>
            <button
              type="button"
              @click="handleActivateLicense"
              :disabled="activatingLicense || !licenseInputKey.trim()"
              class="px-4 py-2 rounded-xl bg-indigo-600 hover:bg-indigo-500 text-white font-bold text-xs shadow-md shadow-indigo-600/20 flex items-center gap-1.5 cursor-pointer disabled:opacity-50"
            >
              <Key class="w-3.5 h-3.5" />
              <span>{{ activatingLicense ? 'Đang kích hoạt...' : 'Xác Nhận Kích Hoạt' }}</span>
            </button>
          </div>
        </div>
      </div>
    </div>

  </div>
</template>

<script setup>
import { ref, computed, onMounted } from 'vue'
import api from '@/services/api'
import { useNotify } from '@/composables/useNotify'
import ShiftCloseModal from '@/components/ShiftCloseModal.vue'
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
  ShieldCheck,
  Moon,
  Key,
  Cloud,
  CloudUpload,
  Sparkles
} from 'lucide-vue-next'

const { toast: notify, confirm: askConfirm } = useNotify()

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
const saving = ref(false)

// System Version State
const systemVersionInfo = ref({
  version: '1.0.0',
  appName: 'DiroPos PRO',
  buildDate: '2026-09-30',
  edition: 'PRO Commercial'
})
const checkingUpdate = ref(false)

async function loadVersionInfo() {
  try {
    const res = await api.getSystemVersion()
    if (res?.data) {
      systemVersionInfo.value = res.data
    }
  } catch (err) {
    console.warn('Lỗi lấy thông tin phiên bản:', err)
  }
}

async function checkManualUpdate() {
  checkingUpdate.value = true
  try {
    const res = await api.checkUpdate()
    if (res?.data?.hasUpdate) {
      notify.info(`Đã có bản cập nhật mới v${res.data.latestVersion}! Vui lòng bấm thông báo để nâng cấp.`, 'Có Bản Cập Nhật Mới!')
    } else {
      notify.success(`Bạn đang sử dụng phiên bản mới nhất (v${systemVersionInfo.value.version}).`, 'Hệ Thống Đã Cập Nhật')
    }
  } catch (err) {
    notify.error('Không thể kiểm tra bản cập nhật lúc này.')
  } finally {
    checkingUpdate.value = false
  }
}

// License State
const licenseInfo = ref(null)
const showLicenseModal = ref(false)
const licenseInputKey = ref('')
const activatingLicense = ref(false)

function formatDate(isoStr) {
  if (!isoStr) return '---'
  try {
    const d = new Date(isoStr)
    return `${String(d.getDate()).padStart(2, '0')}/${String(d.getMonth() + 1).padStart(2, '0')}/${d.getFullYear()}`
  } catch {
    return isoStr
  }
}

async function loadLicenseInfo() {
  try {
    const res = await api.getLicenseStatus()
    if (res?.data) {
      licenseInfo.value = res.data
    }
  } catch (err) {
    console.error('Lỗi khi tải thông tin bản quyền:', err)
  }
}

async function handleActivateLicense() {
  if (!licenseInputKey.value.trim()) return
  activatingLicense.value = true
  try {
    const res = await api.activateLicense(licenseInputKey.value.trim())
    if (res?.data) {
      notify.success(res.data.message || 'Kích hoạt bản quyền thành công!')
      showLicenseModal.value = false
      licenseInputKey.value = ''
      await loadLicenseInfo()
    }
  } catch (err) {
    notify.error(err.response?.data?.message || err.message || 'Mã bản quyền không hợp lệ.')
  } finally {
    activatingLicense.value = false
  }
}

// Đóng Ca & Backup State
const isShiftModalOpen = ref(false)
const lastShiftClose = ref(localStorage.getItem('diropos_last_shift_close'))
const backupInfo = ref(null)
const loadingBackupInfo = ref(false)
const downloadingBackup = ref(false)
const restoringBackup = ref(false)
const showRestoreModal = ref(false)
const selectedFile = ref(null)
const fileInputRef = ref(null)

// Cloud Backup State
const cloudBackups = ref([])
const loadingCloudBackups = ref(false)
const uploadingCloudBackup = ref(false)
const restoringCloudBackup = ref(false)

const lastShiftCloseFormatted = computed(() => {
  if (!lastShiftClose.value) return 'Chưa đóng ca'
  try {
    const d = new Date(lastShiftClose.value)
    const day = String(d.getDate()).padStart(2, '0')
    const month = String(d.getMonth() + 1).padStart(2, '0')
    const hours = String(d.getHours()).padStart(2, '0')
    const mins = String(d.getMinutes()).padStart(2, '0')
    return `${hours}:${mins} - ${day}/${month}`
  } catch {
    return 'Chưa đóng ca'
  }
})

function onShiftCompleted(event) {
  lastShiftClose.value = event.time
  notify.success('Đã đóng ca thành công! Dữ liệu đã được đồng bộ lên Cloud an toàn.')
  loadBackupInfo()
  loadCloudBackups()
}

function showToast(msg) {
  notify.success(msg)
}

function formatBackupDate(isoStr) {
  if (!isoStr) return '---'
  try {
    const d = new Date(isoStr)
    const hh = String(d.getHours()).padStart(2, '0')
    const mm = String(d.getMinutes()).padStart(2, '0')
    const day = String(d.getDate()).padStart(2, '0')
    const month = String(d.getMonth() + 1).padStart(2, '0')
    const year = d.getFullYear()
    return `${hh}:${mm} - ${day}/${month}/${year}`
  } catch {
    return isoStr
  }
}

async function loadCloudBackups() {
  loadingCloudBackups.value = true
  try {
    const res = await api.getCloudBackups()
    if (res.data) {
      cloudBackups.value = res.data
    }
  } catch (err) {
    console.error('Lỗi khi tải danh sách backup cloud:', err)
  } finally {
    loadingCloudBackups.value = false
  }
}

async function triggerManualCloudBackup() {
  uploadingCloudBackup.value = true
  try {
    const res = await api.uploadCloudBackup()
    if (res.data?.success) {
      notify.success(res.data.message || 'Đã sao lưu lên Cloud thành công!')
      await loadCloudBackups()
    }
  } catch (err) {
    notify.error('Lỗi khi sao lưu lên Cloud: ' + (err.response?.data || err.message))
  } finally {
    uploadingCloudBackup.value = false
  }
}

async function confirmRestoreCloudBackup(b) {
  const ok = await askConfirm({
    title: 'Xác Nhận Khôi Phục Dữ Liệu Cloud?',
    message: `Bạn đang chọn khôi phục từ bản sao lưu Cloud:\n• File: ${b.fileName}\n• Ngày tạo: ${formatBackupDate(b.createdAt)}\n\nLưu ý: Toàn bộ dữ liệu bán hàng hiện tại sẽ được nạp lại theo bản này (hệ thống sẽ tự động lưu 1 bản dự phòng trước khi khôi phục). Bạn có chắc chắn muốn tiếp tục?`,
    type: 'warning',
    confirmText: 'Khôi phục ngay',
    cancelText: 'Hủy bỏ'
  })
  if (!ok) return

  restoringCloudBackup.value = true
  try {
    const res = await api.restoreCloudBackup({ fileName: b.fileName })
    notify.success(res.data?.message || 'Khôi phục dữ liệu từ Cloud thành công!')
    setTimeout(() => {
      window.location.reload()
    }, 1200)
  } catch (err) {
    notify.error('Lỗi khôi phục từ Cloud: ' + (err.response?.data || err.message))
  } finally {
    restoringCloudBackup.value = false
  }
}

async function loadData() {
  try {
    const [setRes, bankRes] = await Promise.all([
      api.getSettings(),
      api.getPopularBanks()
    ])
    if (setRes.data) setting.value = setRes.data
    if (bankRes.data) banks.value = bankRes.data
    await Promise.all([
      loadLicenseInfo(),
      loadVersionInfo()
    ])
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
  loadCloudBackups()
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
    // Đồng bộ thông tin tên tiệm, SĐT, địa chỉ lên DiroAdmin Cloud (KHÔNG chạm vào thời hạn bản quyền)
    try {
      await api.updateShopProfile({
        shopName: setting.value.shopName,
        phone: setting.value.phone,
        address: setting.value.address,
        businessModel: licenseInfo.value?.businessModel || 'Barber',
        ownerName: licenseInfo.value?.ownerName || ''
      })
    } catch { }
    notify.success('Đã lưu cấu hình DiroPos & đồng bộ lên hệ thống quản trị thành công!')
    await loadLicenseInfo()
  } catch (err) {
    notify.error('Lỗi khi lưu cài đặt: ' + (err.response?.data || err.message))
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

    notify.success('Đã tải bản sao lưu cơ sở dữ liệu thành công!')
    loadBackupInfo()
  } catch (err) {
    console.error('Lỗi sao lưu:', err)
    notify.error('Không thể tải file sao lưu: ' + (err.response?.data || err.message))
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
    notify.warning('Vui lòng chọn file có phần mở rộng .db')
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
    notify.success(res.data?.message || 'Đã khôi phục dữ liệu thành công!')
    
    await Promise.all([
      loadData(),
      loadBackupInfo()
    ])
  } catch (err) {
    console.error('Lỗi khi khôi phục:', err)
    notify.error('Lỗi khôi phục dữ liệu: ' + (err.response?.data || err.message))
  } finally {
    restoringBackup.value = false
  }
}
</script>
