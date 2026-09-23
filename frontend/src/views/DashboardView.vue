<template>
  <div class="p-6 space-y-6 overflow-y-auto h-screen max-w-7xl mx-auto">
    <!-- Header with Month Switcher -->
    <div class="flex flex-col sm:flex-row justify-between sm:items-center gap-4 border-b border-barber-border/80 pb-4">
      <div>
        <h2 class="text-2xl font-extrabold text-white">Báo Cáo Hoạt Động & Doanh Thu</h2>
        <p class="text-xs text-zinc-400 mt-0.5">Theo dõi doanh thu, chi tiêu, lợi nhuận tháng này và tháng trước</p>
      </div>

      <!-- Quick Month Switcher -->
      <div class="flex items-center gap-2 text-xs">
        <div class="flex bg-zinc-900 p-1 rounded-xl border border-zinc-800">
          <button
            @click="switchMonthMode('current')"
            class="px-3.5 py-1.5 rounded-lg font-bold transition-all"
            :class="selectedMonthMode === 'current' ? 'bg-barber-gold text-black shadow-md' : 'text-zinc-400 hover:text-white'"
          >
            Tháng Này
          </button>
          <button
            @click="switchMonthMode('previous')"
            class="px-3.5 py-1.5 rounded-lg font-bold transition-all"
            :class="selectedMonthMode === 'previous' ? 'bg-barber-gold text-black shadow-md' : 'text-zinc-400 hover:text-white'"
          >
            Tháng Trước
          </button>
        </div>

        <!-- Month Picker -->
        <input
          v-model="customMonthInput"
          type="month"
          @change="onCustomMonthChange"
          class="bg-barber-card border border-zinc-700 rounded-xl px-3 py-1.5 text-white focus:outline-none focus:border-barber-gold text-xs"
        />

        <button 
          @click="loadSummary" 
          class="p-2 rounded-xl bg-zinc-800 hover:bg-zinc-700 text-zinc-300 transition"
          title="Làm mới dữ liệu"
        >
          <RotateCcw class="w-4 h-4" />
        </button>
      </div>
    </div>

    <!-- Stat Cards (4 Cards: Hôm nay & Tháng đang xem) -->
    <div class="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-4">
      <StatCard
        title="Doanh Thu Hôm Nay"
        :value="formatCurrency(summary.todayRevenue)"
        :subtext="`Tổng ${summary.todayOrdersCount} lượt khách`"
        iconBgClass="bg-amber-500/10 text-barber-gold"
      >
        <template #icon>
          <Coins class="w-5 h-5 text-barber-gold" />
        </template>
      </StatCard>

      <StatCard
        title="Chi Phí Hôm Nay"
        :value="formatCurrency(summary.todayExpense)"
        subtext="Điện nước, phụ liệu, ăn uống"
        iconBgClass="bg-rose-500/10 text-rose-400"
      >
        <template #icon>
          <ArrowDownRight class="w-5 h-5 text-rose-400" />
        </template>
      </StatCard>

      <StatCard
        title="Lợi Nhuận Hôm Nay"
        :value="formatCurrency(summary.todayNetProfit)"
        :subtext="summary.todayNetProfit >= 0 ? 'Thực lãi sau khi trừ chi phí' : 'Tạm thời âm do khoản chi lớn'"
        :iconBgClass="summary.todayNetProfit >= 0 ? 'bg-emerald-500/10 text-emerald-400' : 'bg-rose-500/10 text-rose-400'"
      >
        <template #icon>
          <TrendingUp class="w-5 h-5" :class="summary.todayNetProfit >= 0 ? 'text-emerald-400' : 'text-rose-400'" />
        </template>
      </StatCard>

      <StatCard
        :title="`Lợi Nhuận ${summary.selectedMonthName || 'Tháng'}`"
        :value="formatCurrency(summary.monthNetProfit)"
        :subtext="`Doanh thu tháng: ${formatCurrency(summary.monthRevenue)}`"
        iconBgClass="bg-cyan-500/10 text-cyan-400"
      >
        <template #icon>
          <CalendarDays class="w-5 h-5 text-cyan-400" />
        </template>
      </StatCard>
    </div>

    <!-- KHỐI ĐỐI SOÁT & SO SÁNH THÁNG NÀY VS THÁNG TRƯỚC (MONTH COMPARISON) -->
    <div class="bg-barber-card border border-barber-border rounded-2xl p-5 shadow-xl space-y-4">
      <div class="flex flex-col sm:flex-row justify-between sm:items-center gap-2 border-b border-zinc-800 pb-3">
        <div class="flex items-center gap-2">
          <Scale class="w-5 h-5 text-barber-gold" />
          <h3 class="font-bold text-white text-base">
            Bảng Đối Soát Tài Chính: {{ summary.selectedMonthName }} so với {{ summary.lastMonthName }}
          </h3>
        </div>
        <span class="text-xs text-zinc-400">Tự động tính toán & đối chiếu tháng trước</span>
      </div>

      <div class="grid grid-cols-1 md:grid-cols-3 gap-4 text-xs">
        
        <!-- 1. So Sánh Doanh Thu -->
        <div class="p-4 rounded-xl bg-barber-dark/70 border border-zinc-800 space-y-2">
          <span class="text-zinc-400 font-semibold uppercase tracking-wider block">1. Doanh Thu Bán Hàng</span>
          <div class="flex justify-between items-baseline">
            <span class="text-zinc-400">{{ summary.selectedMonthName }}:</span>
            <span class="text-base font-extrabold text-white">{{ formatCurrency(summary.monthRevenue) }}</span>
          </div>
          <div class="flex justify-between items-baseline pt-1 border-t border-zinc-800/80">
            <span class="text-zinc-500">{{ summary.lastMonthName }}:</span>
            <span class="text-xs font-semibold text-zinc-300">{{ formatCurrency(summary.lastMonthRevenue) }}</span>
          </div>
          <div class="pt-1.5 flex items-center justify-between text-[11px]">
            <span class="text-zinc-400">Tăng trưởng:</span>
            <span 
              class="font-bold px-2 py-0.5 rounded"
              :class="summary.revenueGrowthPercent >= 0 ? 'bg-emerald-500/10 text-emerald-400' : 'bg-rose-500/10 text-rose-400'"
            >
              {{ summary.revenueGrowthPercent >= 0 ? '+' : '' }}{{ summary.revenueGrowthPercent }}%
            </span>
          </div>
        </div>

        <!-- 2. So Sánh Chi Tiêu -->
        <div class="p-4 rounded-xl bg-barber-dark/70 border border-zinc-800 space-y-2">
          <span class="text-zinc-400 font-semibold uppercase tracking-wider block">2. Chi Phí Vận Hành</span>
          <div class="flex justify-between items-baseline">
            <span class="text-zinc-400">{{ summary.selectedMonthName }}:</span>
            <span class="text-base font-extrabold text-rose-400">{{ formatCurrency(summary.monthExpense) }}</span>
          </div>
          <div class="flex justify-between items-baseline pt-1 border-t border-zinc-800/80">
            <span class="text-zinc-500">{{ summary.lastMonthName }}:</span>
            <span class="text-xs font-semibold text-zinc-300">{{ formatCurrency(summary.lastMonthExpense) }}</span>
          </div>
          <div class="pt-1.5 flex items-center justify-between text-[11px]">
            <span class="text-zinc-400">Chênh lệch chi phí:</span>
            <span class="text-zinc-300 font-medium">
              {{ summary.monthExpense >= summary.lastMonthExpense ? '+' : '' }}{{ formatCurrency(summary.monthExpense - summary.lastMonthExpense) }}
            </span>
          </div>
        </div>

        <!-- 3. So Sánh Lợi Nhuận Thực Tế -->
        <div class="p-4 rounded-xl bg-amber-500/5 border border-amber-500/30 space-y-2">
          <span class="text-barber-gold font-bold uppercase tracking-wider block">3. Lợi Nhuận Ròng (Thực Lãi)</span>
          <div class="flex justify-between items-baseline">
            <span class="text-zinc-300">{{ summary.selectedMonthName }}:</span>
            <span class="text-lg font-extrabold text-barber-gold">{{ formatCurrency(summary.monthNetProfit) }}</span>
          </div>
          <div class="flex justify-between items-baseline pt-1 border-t border-amber-500/20">
            <span class="text-zinc-400">{{ summary.lastMonthName }}:</span>
            <span class="text-xs font-semibold text-zinc-300">{{ formatCurrency(summary.lastMonthNetProfit) }}</span>
          </div>
          <div class="pt-1.5 flex items-center justify-between text-[11px]">
            <span class="text-zinc-400">Tăng trưởng lãi:</span>
            <span 
              class="font-bold px-2 py-0.5 rounded"
              :class="summary.netProfitGrowthPercent >= 0 ? 'bg-emerald-500/20 text-emerald-300' : 'bg-rose-500/20 text-rose-300'"
            >
              {{ summary.netProfitGrowthPercent >= 0 ? '+' : '' }}{{ summary.netProfitGrowthPercent }}%
            </span>
          </div>
        </div>

      </div>
    </div>

    <!-- Charts Row: 7 Days Trend & Revenue Breakdown -->
    <div class="grid grid-cols-1 lg:grid-cols-3 gap-6">
      
      <!-- Biểu đồ 7 ngày gần nhất (2 Cột) -->
      <div class="lg:col-span-2 p-5 rounded-2xl bg-barber-card border border-barber-border flex flex-col justify-between">
        <div class="flex justify-between items-center mb-4">
          <div>
            <h3 class="font-bold text-base text-white">Biến Động Doanh Thu 7 Ngày Gần Nhất</h3>
            <p class="text-xs text-zinc-400">Doanh thu thực nhận sau khi giảm giá</p>
          </div>
        </div>

        <div class="h-64 relative">
          <Bar v-if="chartData.labels?.length" :data="chartData" :options="chartOptions" />
          <div v-else class="h-full flex items-center justify-center text-zinc-500 text-xs">
            Chưa có dữ liệu giao dịch 7 ngày qua
          </div>
        </div>
      </div>

      <!-- Tỷ trọng Nguồn thu & Cơ cấu chi phí -->
      <div class="p-5 rounded-2xl bg-barber-card border border-barber-border flex flex-col justify-between space-y-4">
        <div>
          <h3 class="font-bold text-base text-white mb-1">Cơ Cấu Thu Nhập {{ summary.selectedMonthName }}</h3>
          <p class="text-xs text-zinc-400 mb-3">Tiền công dịch vụ vs Bán sáp/gôm</p>

          <div class="space-y-3">
            <!-- Dịch vụ -->
            <div class="p-3 rounded-xl bg-amber-500/5 border border-amber-500/20">
              <div class="flex justify-between items-center text-xs mb-1">
                <span class="font-bold text-barber-gold">Dịch Vụ Cắt & Làm Đẹp</span>
                <span class="font-bold text-white">{{ formatCurrency(summary.serviceRevenueTotal) }}</span>
              </div>
              <div class="w-full bg-zinc-800 h-2 rounded-full overflow-hidden">
                <div 
                  class="bg-barber-gold h-full rounded-full transition-all duration-500"
                  :style="{ width: `${serviceRatio}%` }"
                ></div>
              </div>
              <span class="text-[11px] text-zinc-400 mt-1 block">{{ serviceRatio }}% tổng doanh thu</span>
            </div>

            <!-- Sản phẩm -->
            <div class="p-3 rounded-xl bg-cyan-500/5 border border-cyan-500/20">
              <div class="flex justify-between items-center text-xs mb-1">
                <span class="font-bold text-cyan-400">Sản Phẩm Bán Thêm</span>
                <span class="font-bold text-white">{{ formatCurrency(summary.productRevenueTotal) }}</span>
              </div>
              <div class="w-full bg-zinc-800 h-2 rounded-full overflow-hidden">
                <div 
                  class="bg-cyan-400 h-full rounded-full transition-all duration-500"
                  :style="{ width: `${productRatio}%` }"
                ></div>
              </div>
              <span class="text-[11px] text-zinc-400 mt-1 block">{{ productRatio }}% tổng doanh thu</span>
            </div>
          </div>
        </div>

        <!-- Cơ cấu chi phí nếu có -->
        <div v-if="summary.expenseCategories?.length" class="pt-3 border-t border-zinc-800">
          <p class="text-xs font-semibold text-zinc-300 mb-2">Chi phí nhiều nhất:</p>
          <div class="space-y-1.5">
            <div 
              v-for="cat in summary.expenseCategories.slice(0, 3)" 
              :key="cat.category" 
              class="flex justify-between items-center text-xs text-zinc-400"
            >
              <span class="truncate">{{ cat.category }}</span>
              <span class="text-rose-300 font-semibold">{{ formatCurrency(cat.totalAmount) }}</span>
            </div>
          </div>
        </div>
      </div>

    </div>

    <!-- KHỐI DOANH THU TỪNG HÔM (DAILY REVENUE BREAKDOWN) -->
    <div class="bg-barber-card border border-barber-border rounded-2xl p-5 shadow-xl space-y-4">
      <div class="flex flex-col sm:flex-row justify-between sm:items-center gap-3 border-b border-zinc-800 pb-3">
        <div class="flex items-center gap-2.5">
          <CalendarDays class="w-5 h-5 text-barber-gold" />
          <div>
            <h3 class="font-bold text-white text-base">
              Bảng Kê Doanh Thu & Lợi Nhuận Từng Ngày ({{ summary.selectedMonthName }})
            </h3>
            <p class="text-xs text-zinc-400">Xem chi tiết doanh thu, số lượt khách, chi phí và lợi nhuận ròng của từng hôm</p>
          </div>
        </div>

        <div class="flex flex-wrap items-center gap-2 text-xs">
          <!-- Toggle: Tất cả vs Chỉ ngày có khách -->
          <div class="flex bg-zinc-900 p-1 rounded-xl border border-zinc-800">
            <button
              @click="dailyFilterMode = 'active'"
              class="px-3 py-1 rounded-lg font-bold transition"
              :class="dailyFilterMode === 'active' ? 'bg-amber-500/20 text-barber-gold border border-amber-500/30' : 'text-zinc-400 hover:text-white'"
            >
              Chỉ Ngày Có Đơn ({{ activeDaysCount }})
            </button>
            <button
              @click="dailyFilterMode = 'all'"
              class="px-3 py-1 rounded-lg font-bold transition"
              :class="dailyFilterMode === 'all' ? 'bg-amber-500/20 text-barber-gold border border-amber-500/30' : 'text-zinc-400 hover:text-white'"
            >
              Tất Cả Các Ngày ({{ summary.dailyBreakdown?.length || 0 }})
            </button>
          </div>

          <!-- Nút chọn ngày nhanh để xem trực tiếp -->
          <div class="flex items-center gap-1.5 bg-zinc-900/90 px-3 py-1.5 rounded-xl border border-zinc-700/80">
            <Calendar class="w-3.5 h-3.5 text-barber-gold" />
            <input
              type="date"
              v-model="quickInspectDate"
              @change="onQuickInspectDateChange"
              title="Chọn một ngày cụ thể để xem chi tiết"
              class="bg-transparent text-white text-xs focus:outline-none cursor-pointer"
            />
          </div>
        </div>
      </div>

      <!-- Danh Sách Bảng Kê Từng Ngày (Desktop & Tablet Table) -->
      <div class="hidden sm:block overflow-x-auto">
        <table class="w-full text-left text-xs">
          <thead>
            <tr class="border-b border-zinc-800 text-zinc-400 uppercase font-semibold text-[11px]">
              <th class="py-3 px-3">Ngày / Thứ</th>
              <th class="py-3 px-3 text-center">Lượt Khách</th>
              <th class="py-3 px-3 text-right">Dịch Vụ Cắt</th>
              <th class="py-3 px-3 text-right">Sản Phẩm</th>
              <th class="py-3 px-3 text-right">Giảm Giá</th>
              <th class="py-3 px-3 text-right text-amber-300">Doanh Thu Thuần</th>
              <th class="py-3 px-3 text-right text-rose-400">Chi Phí</th>
              <th class="py-3 px-3 text-right text-emerald-400">Thực Lãi (Net)</th>
              <th class="py-3 px-3 text-center">Thao Tác</th>
            </tr>
          </thead>
          <tbody class="divide-y divide-zinc-800/60 font-medium">
            <tr v-if="!filteredDailyList.length">
              <td colspan="9" class="py-8 text-center text-zinc-500">
                Không có ngày nào phát sinh doanh thu trong tháng này
              </td>
            </tr>

            <tr 
              v-for="day in filteredDailyList" 
              :key="day.date"
              class="hover:bg-zinc-900/60 transition group cursor-pointer"
              @click="openDayDetail(day.date)"
            >
              <!-- Cột Ngày -->
              <td class="py-3.5 px-3">
                <div class="flex items-center gap-2">
                  <span 
                    class="font-mono font-bold text-white text-sm"
                    :class="{ 'text-barber-gold': day.isToday }"
                  >
                    {{ day.dateFormatted }}
                  </span>
                  <span class="text-[11px] text-zinc-400 font-normal">({{ day.dayOfWeek }})</span>
                  <span 
                    v-if="day.isToday" 
                    class="px-2 py-0.5 rounded-full text-[10px] font-extrabold bg-amber-500/20 text-amber-300 border border-amber-500/30"
                  >
                    Hôm nay
                  </span>
                </div>
              </td>

              <!-- Lượt Khách -->
              <td class="py-3.5 px-3 text-center">
                <span 
                  class="inline-flex items-center gap-1 px-2.5 py-1 rounded-lg text-xs font-bold"
                  :class="day.ordersCount > 0 ? 'bg-zinc-800 text-white' : 'text-zinc-600'"
                >
                  <Users class="w-3.5 h-3.5 text-zinc-400" />
                  {{ day.ordersCount }}
                </span>
              </td>

              <!-- Dịch vụ -->
              <td class="py-3.5 px-3 text-right text-zinc-300">
                {{ day.serviceRevenue > 0 ? formatCurrency(day.serviceRevenue) : '-' }}
              </td>

              <!-- Sản phẩm -->
              <td class="py-3.5 px-3 text-right text-cyan-300">
                {{ day.productRevenue > 0 ? formatCurrency(day.productRevenue) : '-' }}
              </td>

              <!-- Giảm giá -->
              <td class="py-3.5 px-3 text-right text-zinc-400">
                {{ day.discountTotal > 0 ? `-${formatCurrency(day.discountTotal)}` : '-' }}
              </td>

              <!-- Doanh thu thuần -->
              <td class="py-3.5 px-3 text-right">
                <span 
                  class="font-extrabold text-sm"
                  :class="day.revenue > 0 ? 'text-barber-gold' : 'text-zinc-600'"
                >
                  {{ formatCurrency(day.revenue) }}
                </span>
              </td>

              <!-- Chi phí ngày -->
              <td class="py-3.5 px-3 text-right">
                <span :class="day.expense > 0 ? 'text-rose-400 font-bold' : 'text-zinc-600'">
                  {{ day.expense > 0 ? `-${formatCurrency(day.expense)}` : '-' }}
                </span>
              </td>

              <!-- Lợi nhuận ròng -->
              <td class="py-3.5 px-3 text-right font-bold">
                <span 
                  v-if="day.revenue > 0 || day.expense > 0"
                  :class="day.netProfit >= 0 ? 'text-emerald-400' : 'text-rose-400'"
                >
                  {{ formatCurrency(day.netProfit) }}
                </span>
                <span v-else class="text-zinc-600">-</span>
              </td>

              <!-- Nút Thao tác -->
              <td class="py-3.5 px-3 text-center">
                <button
                  type="button"
                  @click.stop="openDayDetail(day.date)"
                  class="px-2.5 py-1.5 rounded-lg bg-zinc-800 hover:bg-zinc-700 text-zinc-300 hover:text-white font-semibold text-xs transition flex items-center gap-1 mx-auto"
                >
                  <Eye class="w-3.5 h-3.5 text-barber-gold" />
                  <span>Xem đơn</span>
                </button>
              </td>
            </tr>
          </tbody>
        </table>
      </div>

      <!-- Danh Sách Dạng Card (Cho Mobile / iPhone) -->
      <div class="sm:hidden space-y-3">
        <div v-if="!filteredDailyList.length" class="py-8 text-center text-zinc-500 text-xs">
          Không có ngày nào phát sinh doanh thu trong tháng này
        </div>

        <div
          v-for="day in filteredDailyList"
          :key="day.date"
          @click="openDayDetail(day.date)"
          class="p-4 rounded-xl bg-barber-dark/80 border border-zinc-800 space-y-3 active:scale-[0.99] transition cursor-pointer"
        >
          <div class="flex justify-between items-center">
            <div class="flex items-center gap-2">
              <span class="font-bold text-white text-sm" :class="{ 'text-barber-gold': day.isToday }">
                {{ day.dateFormatted }}
              </span>
              <span class="text-xs text-zinc-400">({{ day.dayOfWeek }})</span>
              <span v-if="day.isToday" class="px-1.5 py-0.5 rounded text-[10px] font-bold bg-amber-500/20 text-amber-300">
                Hôm nay
              </span>
            </div>
            <span class="inline-flex items-center gap-1 text-xs text-zinc-300 bg-zinc-800/80 px-2 py-0.5 rounded">
              <Users class="w-3 h-3 text-zinc-400" />
              {{ day.ordersCount }} khách
            </span>
          </div>

          <div class="grid grid-cols-2 gap-2 text-xs pt-1 border-t border-zinc-800/60">
            <div>
              <span class="text-zinc-400 text-[11px] block">Doanh thu:</span>
              <span class="font-extrabold text-barber-gold text-base">{{ formatCurrency(day.revenue) }}</span>
            </div>
            <div class="text-right">
              <span class="text-zinc-400 text-[11px] block">Thực lãi (Net):</span>
              <span 
                class="font-bold text-sm"
                :class="day.netProfit >= 0 ? 'text-emerald-400' : 'text-rose-400'"
              >
                {{ formatCurrency(day.netProfit) }}
              </span>
            </div>
          </div>

          <div class="flex items-center justify-between text-[11px] text-zinc-400 pt-1 border-t border-zinc-800/40">
            <span>Chi phí: {{ day.expense > 0 ? formatCurrency(day.expense) : '0 ₫' }}</span>
            <span class="text-amber-300 font-semibold flex items-center gap-1">
              Xem {{ day.ordersCount }} đơn <ChevronRight class="w-3 h-3" />
            </span>
          </div>
        </div>
      </div>

    </div>

    <!-- Top Dịch Vụ & Top Sản Phẩm -->
    <div class="grid grid-cols-1 md:grid-cols-2 gap-6">
      <!-- Top 5 Dịch vụ -->
      <div class="p-5 rounded-2xl bg-barber-card border border-barber-border">
        <div class="flex items-center gap-2 mb-4">
          <Sparkles class="w-4 h-4 text-barber-gold" />
          <h3 class="font-bold text-base text-white">Top Dịch Vụ Cắt Được Chuộng Nhất</h3>
        </div>

        <div class="space-y-2.5">
          <div v-if="!summary.topServices?.length" class="text-xs text-zinc-500 py-6 text-center">
            Chưa có số liệu dịch vụ
          </div>

          <div
            v-for="(svc, idx) in summary.topServices"
            :key="idx"
            class="flex items-center justify-between p-3 rounded-xl bg-zinc-900/60 border border-zinc-800 text-xs"
          >
            <div class="flex items-center gap-3">
              <span class="w-6 h-6 rounded-lg bg-amber-500/10 text-barber-gold font-bold flex items-center justify-center text-xs">
                #{{ idx + 1 }}
              </span>
              <span class="font-bold text-white">{{ svc.name }}</span>
            </div>
            <div class="text-right">
              <p class="text-amber-200 font-extrabold">{{ svc.quantity }} lượt</p>
              <p class="text-[10px] text-zinc-400">{{ formatCurrency(svc.totalAmount) }}</p>
            </div>
          </div>
        </div>
      </div>

      <!-- Top 5 Sản phẩm -->
      <div class="p-5 rounded-2xl bg-barber-card border border-barber-border">
        <div class="flex items-center gap-2 mb-4">
          <Package class="w-4 h-4 text-cyan-400" />
          <h3 class="font-bold text-base text-white">Top Sản Phẩm Sáp/Gôm Bán Chạy</h3>
        </div>

        <div class="space-y-2.5">
          <div v-if="!summary.topProducts?.length" class="text-xs text-zinc-500 py-6 text-center">
            Chưa có số liệu sản phẩm bán lẻ
          </div>

          <div
            v-for="(prod, idx) in summary.topProducts"
            :key="idx"
            class="flex items-center justify-between p-3 rounded-xl bg-zinc-900/60 border border-zinc-800 text-xs"
          >
            <div class="flex items-center gap-3">
              <span class="w-6 h-6 rounded-lg bg-cyan-500/10 text-cyan-400 font-bold flex items-center justify-center text-xs">
                #{{ idx + 1 }}
              </span>
              <span class="font-bold text-white">{{ prod.name }}</span>
            </div>
            <div class="text-right">
              <p class="text-cyan-300 font-extrabold">{{ prod.quantity }} món</p>
              <p class="text-[10px] text-zinc-400">{{ formatCurrency(prod.totalAmount) }}</p>
            </div>
          </div>
        </div>
      </div>
    </div>

    <!-- MODAL XEM CHI TIẾT DOANH THU & ĐƠN HÀNG TRONG NGÀY -->
    <div
      v-if="dayDetailModalOpen"
      class="fixed inset-0 z-50 flex items-center justify-center bg-black/80 backdrop-blur-sm p-3 sm:p-4 animate-fade-in"
      @click.self="dayDetailModalOpen = false"
    >
      <div class="bg-barber-card border border-zinc-700 rounded-2xl max-w-2xl w-full max-h-[90vh] flex flex-col shadow-2xl overflow-hidden">
        
        <!-- Modal Header -->
        <div class="p-4 sm:p-5 border-b border-zinc-800 flex justify-between items-start bg-zinc-900/50">
          <div>
            <div class="flex items-center gap-2">
              <CalendarDays class="w-5 h-5 text-barber-gold" />
              <h3 class="text-base sm:text-lg font-extrabold text-white">
                Báo Cáo Chi Tiết: {{ selectedDayDetail?.dateFormatted || 'Đang tải...' }}
              </h3>
              <span class="text-xs text-zinc-400">({{ selectedDayDetail?.dayOfWeek }})</span>
            </div>
            <p class="text-xs text-zinc-400 mt-1">
              Toàn bộ hóa đơn khách hàng và các khoản chi tiêu phát sinh trong ngày
            </p>
          </div>
          <button
            @click="dayDetailModalOpen = false"
            class="p-2 text-zinc-400 hover:text-white rounded-lg hover:bg-zinc-800 transition"
          >
            <X class="w-5 h-5" />
          </button>
        </div>

        <!-- Modal Body (Scrollable) -->
        <div class="p-4 sm:p-5 overflow-y-auto space-y-5 text-xs">
          
          <!-- Loading state -->
          <div v-if="loadingDayDetail" class="py-12 text-center text-zinc-400 space-y-2">
            <RotateCcw class="w-6 h-6 animate-spin mx-auto text-barber-gold" />
            <p>Đang tải dữ liệu doanh thu của ngày...</p>
          </div>

          <template v-else-if="selectedDayDetail">
            
            <!-- 4 Thẻ Thống Kê Nhanh Của Ngày -->
            <div class="grid grid-cols-2 sm:grid-cols-4 gap-2.5">
              <div class="p-3 rounded-xl bg-barber-dark/70 border border-zinc-800">
                <span class="text-[10px] text-zinc-400 block mb-0.5">Doanh Thu Thuần</span>
                <span class="text-sm font-extrabold text-barber-gold">
                  {{ formatCurrency(selectedDayDetail.revenue) }}
                </span>
                <span class="text-[10px] text-zinc-500 block mt-0.5">{{ selectedDayDetail.ordersCount }} lượt khách</span>
              </div>

              <div class="p-3 rounded-xl bg-barber-dark/70 border border-zinc-800">
                <span class="text-[10px] text-zinc-400 block mb-0.5">Chi Phí Ngày</span>
                <span class="text-sm font-bold text-rose-400">
                  {{ formatCurrency(selectedDayDetail.expense) }}
                </span>
                <span class="text-[10px] text-zinc-500 block mt-0.5">{{ selectedDayDetail.expenses?.length || 0 }} khoản chi</span>
              </div>

              <div class="p-3 rounded-xl bg-barber-dark/70 border border-zinc-800">
                <span class="text-[10px] text-zinc-400 block mb-0.5">Thực Lãi (Net)</span>
                <span 
                  class="text-sm font-extrabold"
                  :class="selectedDayDetail.netProfit >= 0 ? 'text-emerald-400' : 'text-rose-400'"
                >
                  {{ formatCurrency(selectedDayDetail.netProfit) }}
                </span>
                <span class="text-[10px] text-zinc-500 block mt-0.5">Sau khi trừ chi</span>
              </div>

              <div class="p-3 rounded-xl bg-barber-dark/70 border border-zinc-800">
                <span class="text-[10px] text-zinc-400 block mb-0.5">Nguồn Thu</span>
                <div class="text-[10px] text-zinc-300 space-y-0.5">
                  <p class="flex justify-between">
                    <span class="text-cyan-400">VietQR:</span>
                    <span class="font-bold">{{ formatCurrency(selectedDayDetail.vietQrTotal) }}</span>
                  </p>
                  <p class="flex justify-between">
                    <span class="text-emerald-400">Tiền mặt:</span>
                    <span class="font-bold">{{ formatCurrency(selectedDayDetail.cashTotal) }}</span>
                  </p>
                </div>
              </div>
            </div>

            <!-- Danh Sách Đơn Hàng Trong Ngày -->
            <div class="space-y-3">
              <div class="flex items-center justify-between border-b border-zinc-800 pb-2">
                <h4 class="font-bold text-white text-xs uppercase tracking-wider flex items-center gap-1.5">
                  <Scissors class="w-4 h-4 text-barber-gold" />
                  Danh Sách Đơn Hàng ({{ selectedDayDetail.orders?.length || 0 }} lượt khách)
                </h4>
              </div>

              <div v-if="!selectedDayDetail.orders?.length" class="py-6 text-center text-zinc-500 text-xs">
                Ngày này không có lượt đơn hàng nào.
              </div>

              <div class="space-y-2.5">
                <div
                  v-for="order in selectedDayDetail.orders"
                  :key="order.id"
                  class="p-3.5 rounded-xl bg-barber-dark/90 border border-zinc-800/80 space-y-2"
                >
                  <div class="flex flex-wrap justify-between items-center gap-2">
                    <div class="flex items-center gap-2">
                      <span class="font-mono font-bold text-amber-300">{{ order.orderCode }}</span>
                      <span class="text-white font-bold">{{ order.customerName || 'Khách vãng lai' }}</span>
                      <span v-if="order.customerPhone" class="text-zinc-500 text-[11px]">({{ order.customerPhone }})</span>
                    </div>

                    <div class="flex items-center gap-2">
                      <span
                        class="px-2 py-0.5 rounded text-[10px] font-bold"
                        :class="order.paymentMethod === 'VietQR' ? 'bg-cyan-500/10 text-cyan-400 border border-cyan-500/20' : 'bg-emerald-500/10 text-emerald-400 border border-emerald-500/20'"
                      >
                        {{ order.paymentMethod === 'VietQR' ? 'VietQR Napas' : 'Tiền mặt' }}
                      </span>
                      <span class="text-barber-gold font-extrabold text-sm">
                        {{ formatCurrency(order.finalAmount) }}
                      </span>
                    </div>
                  </div>

                  <!-- Danh sách món đã làm / mua -->
                  <div class="bg-zinc-900/60 p-2.5 rounded-lg border border-zinc-800/60 text-[11px] space-y-1">
                    <div 
                      v-for="(item, idx) in order.items" 
                      :key="idx"
                      class="flex justify-between items-center text-zinc-300"
                    >
                      <div class="flex items-center gap-1.5">
                        <span 
                          class="px-1.5 py-0.2 rounded text-[9px] font-bold"
                          :class="item.itemType === 'Service' ? 'bg-amber-500/10 text-barber-gold' : 'bg-cyan-500/10 text-cyan-400'"
                        >
                          {{ item.itemType === 'Service' ? 'Dịch vụ' : 'Sản phẩm' }}
                        </span>
                        <span>{{ item.itemName }} x{{ item.quantity }}</span>
                      </div>
                      <span class="font-mono text-zinc-400">{{ formatCurrency(item.totalPrice) }}</span>
                    </div>

                    <div v-if="order.discountPercent > 0" class="pt-1 border-t border-zinc-800 flex justify-between text-zinc-400 text-[10px]">
                      <span>Giảm giá {{ order.discountPercent }}%:</span>
                      <span class="text-rose-400">-{{ formatCurrency(order.discountAmount) }}</span>
                    </div>
                  </div>

                  <!-- Ghi chú nếu có -->
                  <div v-if="order.note" class="text-[10px] text-zinc-400 italic">
                    Ghi chú: "{{ order.note }}"
                  </div>
                </div>
              </div>
            </div>

            <!-- Danh Sách Chi Phí Trong Ngày (nếu có) -->
            <div v-if="selectedDayDetail.expenses?.length" class="space-y-2.5 pt-2 border-t border-zinc-800">
              <h4 class="font-bold text-white text-xs uppercase tracking-wider flex items-center gap-1.5 text-rose-400">
                <ArrowDownRight class="w-4 h-4" />
                Các Khoản Chi Tiêu Trong Ngày ({{ selectedDayDetail.expenses.length }} khoản)
              </h4>

              <div class="space-y-1.5">
                <div
                  v-for="exp in selectedDayDetail.expenses"
                  :key="exp.id"
                  class="p-2.5 rounded-xl bg-rose-950/10 border border-rose-900/20 flex justify-between items-center text-xs"
                >
                  <div>
                    <span class="font-bold text-white block">{{ exp.title }}</span>
                    <span class="text-[10px] text-zinc-400">Danh mục: {{ exp.category }}</span>
                  </div>
                  <span class="font-bold text-rose-400 font-mono">-{{ formatCurrency(exp.amount) }}</span>
                </div>
              </div>
            </div>

          </template>
        </div>

        <!-- Modal Footer -->
        <div class="p-3 sm:p-4 border-t border-zinc-800 bg-zinc-900/50 flex justify-end">
          <button
            @click="dayDetailModalOpen = false"
            class="px-5 py-2 rounded-xl bg-zinc-800 hover:bg-zinc-700 text-white font-bold text-xs transition"
          >
            Đóng
          </button>
        </div>

      </div>
    </div>

  </div>
</template>

<script setup>
import { ref, computed, onMounted } from 'vue'
import api from '@/services/api'
import StatCard from '@/components/StatCard.vue'
import { 
  Coins, 
  Users, 
  Tag, 
  CalendarDays, 
  Calendar,
  RotateCcw, 
  Sparkles, 
  Package,
  ArrowDownRight,
  TrendingUp,
  Scale,
  Eye,
  ChevronRight,
  X,
  Scissors
} from 'lucide-vue-next'

import {
  Chart as ChartJS,
  Title,
  Tooltip,
  Legend,
  BarElement,
  CategoryScale,
  LinearScale
} from 'chart.js'
import { Bar } from 'vue-chartjs'

ChartJS.register(Title, Tooltip, Legend, BarElement, CategoryScale, LinearScale)

const selectedMonthMode = ref('current')
const now = new Date()
const customMonthInput = ref(`${now.getFullYear()}-${String(now.getMonth() + 1).padStart(2, '0')}`)

// State cho Bảng kê Doanh thu từng hôm
const dailyFilterMode = ref('active') // 'active' | 'all'
const quickInspectDate = ref('')
const dayDetailModalOpen = ref(false)
const selectedDayDetail = ref(null)
const loadingDayDetail = ref(false)

const summary = ref({
  todayRevenue: 0,
  todayOrdersCount: 0,
  todayDiscountTotal: 0,
  todayExpense: 0,
  todayNetProfit: 0,
  selectedMonthName: '',
  lastMonthName: '',
  monthRevenue: 0,
  monthOrdersCount: 0,
  monthExpense: 0,
  monthNetProfit: 0,
  lastMonthRevenue: 0,
  lastMonthOrdersCount: 0,
  lastMonthExpense: 0,
  lastMonthNetProfit: 0,
  revenueGrowthPercent: 0,
  netProfitGrowthPercent: 0,
  serviceRevenueTotal: 0,
  productRevenueTotal: 0,
  last7DaysSales: [],
  topServices: [],
  topProducts: [],
  expenseCategories: [],
  dailyBreakdown: []
})

// Số ngày có doanh thu hoặc đơn hàng
const activeDaysCount = computed(() => {
  return (summary.value.dailyBreakdown || []).filter(d => d.ordersCount > 0 || d.expense > 0).length
})

// Danh sách ngày theo bộ lọc
const filteredDailyList = computed(() => {
  const list = summary.value.dailyBreakdown || []
  if (dailyFilterMode.value === 'active') {
    return list.filter(d => d.ordersCount > 0 || d.expense > 0)
  }
  return list
})

async function loadSummary() {
  try {
    let month = null
    let year = null

    if (customMonthInput.value) {
      const parts = customMonthInput.value.split('-')
      year = parseInt(parts[0])
      month = parseInt(parts[1])
    }

    const res = await api.getDashboardSummary({ month, year })
    summary.value = res.data
  } catch (err) {
    console.error('Lỗi khi tải dashboard:', err)
  }
}

// Mở modal xem chi tiết 1 ngày bất kỳ
async function openDayDetail(dateStr) {
  loadingDayDetail.value = true
  dayDetailModalOpen.value = true
  selectedDayDetail.value = null

  try {
    const res = await api.getDayDetail(dateStr)
    selectedDayDetail.value = res.data
  } catch (err) {
    console.error('Lỗi tải chi tiết ngày:', err)
  } finally {
    loadingDayDetail.value = false
  }
}

// Khi người dùng chọn 1 ngày từ ô input date
function onQuickInspectDateChange() {
  if (quickInspectDate.value) {
    openDayDetail(quickInspectDate.value)
  }
}

function switchMonthMode(mode) {
  selectedMonthMode.value = mode
  const d = new Date()

  if (mode === 'current') {
    customMonthInput.value = `${d.getFullYear()}-${String(d.getMonth() + 1).padStart(2, '0')}`
  } else if (mode === 'previous') {
    const prev = new Date(d.getFullYear(), d.getMonth() - 1, 1)
    customMonthInput.value = `${prev.getFullYear()}-${String(prev.getMonth() + 1).padStart(2, '0')}`
  }

  loadSummary()
}

function onCustomMonthChange() {
  selectedMonthMode.value = 'custom'
  loadSummary()
}

onMounted(() => {
  loadSummary()
})

function formatCurrency(val) {
  return new Intl.NumberFormat('vi-VN', { style: 'currency', currency: 'VND' }).format(val || 0)
}

const serviceRatio = computed(() => {
  const total = summary.value.serviceRevenueTotal + summary.value.productRevenueTotal
  if (total === 0) return 0
  return Math.round((summary.value.serviceRevenueTotal / total) * 100)
})

const productRatio = computed(() => {
  const total = summary.value.serviceRevenueTotal + summary.value.productRevenueTotal
  if (total === 0) return 0
  return Math.round((summary.value.productRevenueTotal / total) * 100)
})

const chartData = computed(() => {
  const days = summary.value.last7DaysSales || []
  return {
    labels: days.map(d => d.date),
    datasets: [
      {
        label: 'Doanh thu (VNĐ)',
        backgroundColor: '#e5a93c',
        borderRadius: 8,
        data: days.map(d => d.revenue)
      }
    ]
  }
})

const chartOptions = {
  responsive: true,
  maintainAspectRatio: false,
  plugins: {
    legend: { display: false },
    tooltip: {
      callbacks: {
        label: (ctx) => ` Doanh thu: ${new Intl.NumberFormat('vi-VN').format(ctx.raw)} đ`
      }
    }
  },
  scales: {
    x: {
      grid: { display: false },
      ticks: { color: '#a1a1aa', font: { size: 11 } }
    },
    y: {
      grid: { color: '#272732' },
      ticks: {
        color: '#a1a1aa',
        font: { size: 10 },
        callback: (value) => `${value / 1000}k`
      }
    }
  }
}
</script>

