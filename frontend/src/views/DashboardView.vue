<template>
  <div class="p-6 space-y-6 overflow-y-auto h-screen max-w-7xl mx-auto bg-slate-50 text-slate-800">
    <!-- Header with Month Switcher -->
    <div class="flex flex-col sm:flex-row justify-between sm:items-center gap-4 border-b border-slate-200 pb-4">
      <div>
        <h2 class="text-2xl font-black text-slate-900 tracking-tight">Báo Cáo Hoạt Động & Doanh Thu</h2>
        <p class="text-xs text-slate-500 mt-0.5 font-medium">Theo dõi doanh thu, chi phí và lợi nhuận ròng thời gian thực</p>
      </div>

      <!-- Quick Month Switcher -->
      <div class="flex items-center gap-2 text-xs">
        <div class="flex bg-slate-100 p-1 rounded-xl border border-slate-200">
          <button
            @click="switchMonthMode('current')"
            class="px-3.5 py-1.5 rounded-lg font-bold transition-all"
            :class="selectedMonthMode === 'current' ? 'bg-white text-indigo-600 shadow-sm' : 'text-slate-600 hover:text-slate-900'"
          >
            Tháng Này
          </button>
          <button
            @click="switchMonthMode('previous')"
            class="px-3.5 py-1.5 rounded-lg font-bold transition-all"
            :class="selectedMonthMode === 'previous' ? 'bg-white text-indigo-600 shadow-sm' : 'text-slate-600 hover:text-slate-900'"
          >
            Tháng Trước
          </button>
        </div>

        <!-- Month Picker -->
        <input
          v-model="customMonthInput"
          type="month"
          @change="onCustomMonthChange"
          class="bg-white border border-slate-200 rounded-xl px-3 py-1.5 text-slate-800 focus:outline-none focus:border-indigo-500 text-xs shadow-2xs"
        />

        <button 
          @click="loadSummary" 
          class="p-2 rounded-xl bg-white border border-slate-200 hover:bg-slate-100 text-slate-600 transition shadow-2xs"
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
        :subtext="`Tổng ${summary.todayOrdersCount} lượt đơn`"
        iconBgClass="bg-indigo-50 text-indigo-600"
      >
        <template #icon>
          <Coins class="w-5 h-5 text-indigo-600" />
        </template>
      </StatCard>

      <StatCard
        title="Chi Phí Hôm Nay"
        :value="formatCurrency(summary.todayExpense)"
        subtext="Mặt bằng, phụ liệu, vận hành"
        iconBgClass="bg-rose-50 text-rose-600"
      >
        <template #icon>
          <ArrowDownRight class="w-5 h-5 text-rose-600" />
        </template>
      </StatCard>

      <StatCard
        title="Lợi Nhuận Hôm Nay"
        :value="formatCurrency(summary.todayNetProfit)"
        :subtext="summary.todayNetProfit >= 0 ? 'Thực lãi sau khi trừ chi phí' : 'Tạm thời âm do khoản chi lớn'"
        :iconBgClass="summary.todayNetProfit >= 0 ? 'bg-emerald-50 text-emerald-600' : 'bg-rose-50 text-rose-600'"
      >
        <template #icon>
          <TrendingUp class="w-5 h-5" :class="summary.todayNetProfit >= 0 ? 'text-emerald-600' : 'text-rose-600'" />
        </template>
      </StatCard>

      <StatCard
        :title="`Lợi Nhuận ${summary.selectedMonthName || 'Tháng'}`"
        :value="formatCurrency(summary.monthNetProfit)"
        :subtext="`Doanh thu tháng: ${formatCurrency(summary.monthRevenue)}`"
        iconBgClass="bg-blue-50 text-blue-600"
      >
        <template #icon>
          <CalendarDays class="w-5 h-5 text-blue-600" />
        </template>
      </StatCard>
    </div>

    <!-- KHỐI ĐỐI SOÁT & SO SÁNH THÁNG NÀY VS THÁNG TRƯỚC -->
    <div class="bg-white border border-slate-200 rounded-2xl p-5 shadow-xs space-y-4">
      <div class="flex flex-col sm:flex-row justify-between sm:items-center gap-2 border-b border-slate-100 pb-3">
        <div class="flex items-center gap-2">
          <Scale class="w-5 h-5 text-indigo-600" />
          <h3 class="font-extrabold text-slate-900 text-base">
            Bảng Đối Soát Tài Chính: {{ summary.selectedMonthName }} so với {{ summary.lastMonthName }}
          </h3>
        </div>
        <span class="text-xs text-slate-500 font-medium">Tự động tính toán & đối chiếu với tháng trước</span>
      </div>

      <div class="grid grid-cols-1 md:grid-cols-3 gap-4 text-xs">
        
        <!-- 1. So Sánh Doanh Thu -->
        <div class="p-4 rounded-xl bg-slate-50 border border-slate-200/80 space-y-2">
          <span class="text-slate-500 font-semibold uppercase tracking-wider block">1. Doanh Thu Bán Hàng</span>
          <div class="flex justify-between items-baseline">
            <span class="text-slate-500">{{ summary.selectedMonthName }}:</span>
            <span class="text-base font-extrabold text-slate-900">{{ formatCurrency(summary.monthRevenue) }}</span>
          </div>
          <div class="flex justify-between items-baseline pt-1 border-t border-slate-200">
            <span class="text-slate-400">{{ summary.lastMonthName }}:</span>
            <span class="text-xs font-semibold text-slate-600">{{ formatCurrency(summary.lastMonthRevenue) }}</span>
          </div>
          <div class="pt-1.5 flex items-center justify-between text-[11px]">
            <span class="text-slate-500">Tăng trưởng:</span>
            <span 
              class="font-bold px-2 py-0.5 rounded"
              :class="summary.revenueGrowthPercent >= 0 ? 'bg-emerald-50 text-emerald-600 border border-emerald-200' : 'bg-rose-50 text-rose-600 border border-rose-200'"
            >
              {{ summary.revenueGrowthPercent >= 0 ? '+' : '' }}{{ summary.revenueGrowthPercent }}%
            </span>
          </div>
        </div>

        <!-- 2. So Sánh Chi Tiêu -->
        <div class="p-4 rounded-xl bg-slate-50 border border-slate-200/80 space-y-2">
          <span class="text-slate-500 font-semibold uppercase tracking-wider block">2. Chi Phí Vận Hành</span>
          <div class="flex justify-between items-baseline">
            <span class="text-slate-500">{{ summary.selectedMonthName }}:</span>
            <span class="text-base font-extrabold text-rose-600">{{ formatCurrency(summary.monthExpense) }}</span>
          </div>
          <div class="flex justify-between items-baseline pt-1 border-t border-slate-200">
            <span class="text-slate-400">{{ summary.lastMonthName }}:</span>
            <span class="text-xs font-semibold text-slate-600">{{ formatCurrency(summary.lastMonthExpense) }}</span>
          </div>
          <div class="pt-1.5 flex items-center justify-between text-[11px]">
            <span class="text-slate-500">Chênh lệch chi phí:</span>
            <span class="text-slate-700 font-semibold">
              {{ summary.monthExpense >= summary.lastMonthExpense ? '+' : '' }}{{ formatCurrency(summary.monthExpense - summary.lastMonthExpense) }}
            </span>
          </div>
        </div>

        <!-- 3. So Sánh Lợi Nhuận Thực Tế -->
        <div class="p-4 rounded-xl bg-indigo-50/50 border border-indigo-100 space-y-2">
          <span class="text-indigo-700 font-bold uppercase tracking-wider block">3. Lợi Nhuận Ròng (Thực Lãi)</span>
          <div class="flex justify-between items-baseline">
            <span class="text-slate-600">{{ summary.selectedMonthName }}:</span>
            <span class="text-lg font-black text-indigo-600">{{ formatCurrency(summary.monthNetProfit) }}</span>
          </div>
          <div class="flex justify-between items-baseline pt-1 border-t border-indigo-100">
            <span class="text-slate-400">{{ summary.lastMonthName }}:</span>
            <span class="text-xs font-semibold text-slate-600">{{ formatCurrency(summary.lastMonthNetProfit) }}</span>
          </div>
          <div class="pt-1.5 flex items-center justify-between text-[11px]">
            <span class="text-slate-500">Tăng trưởng lãi:</span>
            <span 
              class="font-bold px-2 py-0.5 rounded"
              :class="summary.netProfitGrowthPercent >= 0 ? 'bg-emerald-100 text-emerald-700' : 'bg-rose-100 text-rose-700'"
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
      <div class="lg:col-span-2 p-5 rounded-2xl bg-white border border-slate-200 flex flex-col justify-between shadow-xs">
        <div class="flex justify-between items-center mb-4">
          <div>
            <h3 class="font-extrabold text-base text-slate-900">Biến Động Doanh Thu 7 Ngày Gần Nhất</h3>
            <p class="text-xs text-slate-500">Doanh thu thực nhận sau khi giảm giá</p>
          </div>
        </div>

        <div class="h-64 relative">
          <Bar v-if="chartData.labels?.length" :data="chartData" :options="chartOptions" />
          <div v-else class="h-full flex items-center justify-center text-slate-400 text-xs">
            Chưa có dữ liệu giao dịch 7 ngày qua
          </div>
        </div>
      </div>

      <!-- Tỷ trọng Nguồn thu & Cơ cấu chi phí -->
      <div class="p-5 rounded-2xl bg-white border border-slate-200 flex flex-col justify-between space-y-4 shadow-xs">
        <div>
          <h3 class="font-extrabold text-base text-slate-900 mb-1">Cơ Cấu Thu Nhập {{ summary.selectedMonthName }}</h3>
          <p class="text-xs text-slate-500 mb-3">Tỷ trọng Dịch vụ vs Sản phẩm bán lẻ</p>

          <div class="space-y-3">
            <!-- Dịch vụ -->
            <div class="p-3 rounded-xl bg-indigo-50/50 border border-indigo-100">
              <div class="flex justify-between items-center text-xs mb-1">
                <span class="font-bold text-indigo-700">Dịch Vụ</span>
                <span class="font-extrabold text-slate-900">{{ formatCurrency(summary.serviceRevenueTotal) }}</span>
              </div>
              <div class="w-full bg-slate-200 h-2 rounded-full overflow-hidden">
                <div 
                  class="bg-indigo-600 h-full rounded-full transition-all duration-500"
                  :style="{ width: `${serviceRatio}%` }"
                ></div>
              </div>
              <span class="text-[11px] text-slate-500 mt-1 block">{{ serviceRatio }}% tổng doanh thu</span>
            </div>

            <!-- Sản phẩm -->
            <div class="p-3 rounded-xl bg-blue-50/50 border border-blue-100">
              <div class="flex justify-between items-center text-xs mb-1">
                <span class="font-bold text-blue-700">Sản Phẩm & Hàng Hóa</span>
                <span class="font-extrabold text-slate-900">{{ formatCurrency(summary.productRevenueTotal) }}</span>
              </div>
              <div class="w-full bg-slate-200 h-2 rounded-full overflow-hidden">
                <div 
                  class="bg-blue-600 h-full rounded-full transition-all duration-500"
                  :style="{ width: `${productRatio}%` }"
                ></div>
              </div>
              <span class="text-[11px] text-slate-500 mt-1 block">{{ productRatio }}% tổng doanh thu</span>
            </div>
          </div>
        </div>

        <!-- Cơ cấu chi phí nếu có -->
        <div v-if="summary.expenseCategories?.length" class="pt-3 border-t border-slate-100">
          <p class="text-xs font-bold text-slate-700 mb-2">Chi phí nhiều nhất:</p>
          <div class="space-y-1.5">
            <div 
              v-for="cat in summary.expenseCategories.slice(0, 3)" 
              :key="cat.category" 
              class="flex justify-between items-center text-xs text-slate-600"
            >
              <span class="truncate">{{ cat.category }}</span>
              <span class="text-rose-600 font-bold">{{ formatCurrency(cat.totalAmount) }}</span>
            </div>
          </div>
        </div>
      </div>

    </div>

    <!-- KHỐI BIỂU ĐỒ BIẾN ĐỘNG CHI TIÊU TỪNG THÁNG -->
    <div class="bg-white border border-slate-200 rounded-2xl p-5 shadow-xs space-y-4">
      <div class="flex flex-col sm:flex-row justify-between sm:items-center gap-3 border-b border-slate-100 pb-3">
        <div class="flex items-center gap-2.5">
          <Wallet class="w-5 h-5 text-rose-500" />
          <div>
            <h3 class="font-extrabold text-slate-900 text-base">
              Biến Động Chi Tiêu Từng Tháng
            </h3>
            <p class="text-xs text-slate-500">Xu hướng chi phí vận hành qua các tháng gần nhất & đối chiếu doanh thu</p>
          </div>
        </div>

        <div class="flex flex-wrap items-center gap-2.5 text-xs">
          <!-- Chỉ số thống kê nhanh -->
          <div class="hidden md:flex items-center gap-3 pr-2 border-r border-slate-200 text-[11px]">
            <div class="text-slate-500">
              TB Chi phí: <span class="font-bold text-rose-600">{{ formatCurrency(avgMonthlyExpense) }}</span>/tháng
            </div>
            <div v-if="highestExpenseMonth" class="text-slate-500">
              Chi nhiều nhất: <span class="font-bold text-slate-900">{{ highestExpenseMonth.monthName }}</span> (<span class="text-rose-600 font-semibold">{{ formatCurrency(highestExpenseMonth.totalExpense) }}</span>)
            </div>
          </div>

          <!-- Nút chuyển Cột vs Đường -->
          <div class="flex bg-slate-100 p-1 rounded-xl border border-slate-200">
            <button
              @click="expenseChartType = 'line'"
              class="px-3 py-1 rounded-lg font-bold transition flex items-center gap-1.5"
              :class="expenseChartType === 'line' ? 'bg-white text-rose-600 shadow-2xs' : 'text-slate-600 hover:text-slate-900'"
            >
              <LineChart class="w-3.5 h-3.5" />
              Đường
            </button>
            <button
              @click="expenseChartType = 'bar'"
              class="px-3 py-1 rounded-lg font-bold transition flex items-center gap-1.5"
              :class="expenseChartType === 'bar' ? 'bg-white text-rose-600 shadow-2xs' : 'text-slate-600 hover:text-slate-900'"
            >
              <BarChart3 class="w-3.5 h-3.5" />
              Cột
            </button>
          </div>
        </div>
      </div>

      <!-- Biểu đồ Chart -->
      <div class="h-72 relative">
        <component 
          :is="expenseChartType === 'line' ? Line : Bar" 
          v-if="summary.monthlyExpenseTrend?.length" 
          :data="expenseChartData" 
          :options="expenseChartOptions" 
        />
        <div v-else class="h-full flex items-center justify-center text-slate-400 text-xs">
          Chưa có dữ liệu biến động chi tiêu các tháng
        </div>
      </div>

      <!-- Danh sách thẻ tóm tắt 6 tháng bên dưới -->
      <div v-if="summary.monthlyExpenseTrend?.length" class="grid grid-cols-2 sm:grid-cols-3 lg:grid-cols-6 gap-2.5 pt-2 border-t border-slate-100">
        <div 
          v-for="item in summary.monthlyExpenseTrend" 
          :key="item.monthKey"
          class="p-2.5 rounded-xl bg-slate-50 border border-slate-200/80 hover:border-slate-300 transition text-xs"
        >
          <div class="flex items-center justify-between mb-1">
            <span class="font-bold text-slate-900 text-xs">{{ item.monthName }}</span>
            <span class="text-[10px] text-slate-400">{{ item.expensesCount }} mục</span>
          </div>
          <div class="space-y-0.5">
            <div class="flex justify-between items-center text-[11px]">
              <span class="text-slate-500">Chi:</span>
              <span class="font-bold text-rose-600">{{ formatCurrency(item.totalExpense) }}</span>
            </div>
            <div class="flex justify-between items-center text-[10px]">
              <span class="text-slate-400">Thu:</span>
              <span class="font-semibold text-indigo-600">{{ formatCurrency(item.totalRevenue) }}</span>
            </div>
            <div class="flex justify-between items-center text-[10px] pt-0.5 border-t border-slate-200">
              <span class="text-slate-400">Lãi:</span>
              <span :class="item.netProfit >= 0 ? 'text-emerald-600 font-semibold' : 'text-rose-600 font-semibold'">
                {{ formatCurrency(item.netProfit) }}
              </span>
            </div>
          </div>
        </div>
      </div>
    </div>

    <!-- KHỐI DOANH THU TỪNG HÔM (DAILY REVENUE BREAKDOWN) -->
    <div class="bg-white border border-slate-200 rounded-2xl p-5 shadow-xs space-y-4">
      <div class="flex flex-col sm:flex-row justify-between sm:items-center gap-3 border-b border-slate-100 pb-3">
        <div class="flex items-center gap-2.5">
          <CalendarDays class="w-5 h-5 text-indigo-600" />
          <div>
            <h3 class="font-extrabold text-slate-900 text-base">
              Bảng Kê Doanh Thu & Lợi Nhuận Từng Ngày ({{ summary.selectedMonthName }})
            </h3>
            <p class="text-xs text-slate-500">Chi tiết doanh thu, số lượt khách, chi phí và lợi nhuận ròng của từng hôm</p>
          </div>
        </div>

        <div class="flex flex-wrap items-center gap-2 text-xs">
          <!-- Toggle: Tất cả vs Chỉ ngày có khách -->
          <div class="flex bg-slate-100 p-1 rounded-xl border border-slate-200">
            <button
              @click="onFilterModeChange('active')"
              class="px-3 py-1 rounded-lg font-bold transition"
              :class="dailyFilterMode === 'active' ? 'bg-white text-indigo-600 shadow-2xs' : 'text-slate-600 hover:text-slate-900'"
            >
              Chỉ Ngày Có Đơn ({{ activeDaysCount }})
            </button>
            <button
              @click="onFilterModeChange('all')"
              class="px-3 py-1 rounded-lg font-bold transition"
              :class="dailyFilterMode === 'all' ? 'bg-white text-indigo-600 shadow-2xs' : 'text-slate-600 hover:text-slate-900'"
            >
              Tất Cả Các Ngày ({{ summary.dailyBreakdown?.length || 0 }})
            </button>
          </div>

          <!-- Nút chọn ngày nhanh để xem trực tiếp -->
          <div class="flex items-center gap-1.5 bg-white px-3 py-1.5 rounded-xl border border-slate-200 shadow-2xs">
            <Calendar class="w-3.5 h-3.5 text-indigo-600" />
            <input
              type="date"
              v-model="quickInspectDate"
              @change="onQuickInspectDateChange"
              title="Chọn một ngày cụ thể để xem chi tiết"
              class="bg-transparent text-slate-800 text-xs focus:outline-none cursor-pointer"
            />
          </div>
        </div>
      </div>

      <!-- Danh Sách Bảng Kê Từng Ngày (Desktop & Tablet Table) -->
      <div class="hidden sm:block overflow-x-auto">
        <table class="w-full text-left text-xs">
          <thead>
            <tr class="bg-slate-50 border-b border-slate-200 text-slate-500 uppercase font-bold text-[11px]">
              <th class="py-3 px-3">Ngày / Thứ</th>
              <th class="py-3 px-3 text-center">Lượt Khách</th>
              <th class="py-3 px-3 text-right">Dịch Vụ</th>
              <th class="py-3 px-3 text-right">Sản Phẩm</th>
              <th class="py-3 px-3 text-right">Giảm Giá</th>
              <th class="py-3 px-3 text-right text-indigo-600">Doanh Thu Thuần</th>
              <th class="py-3 px-3 text-right text-rose-600">Chi Phí</th>
              <th class="py-3 px-3 text-right text-emerald-600">Thực Lãi (Net)</th>
              <th class="py-3 px-3 text-center">Thao Tác</th>
            </tr>
          </thead>
          <tbody class="divide-y divide-slate-100 font-medium">
            <tr v-if="!filteredDailyList.length">
              <td colspan="9" class="py-8 text-center text-slate-400">
                Không có ngày nào phát sinh doanh thu trong tháng này
              </td>
            </tr>

            <tr 
              v-for="day in paginatedDailyList" 
              :key="day.date"
              class="hover:bg-slate-50 transition group cursor-pointer"
              @click="openDayDetail(day.date)"
            >
              <!-- Cột Ngày -->
              <td class="py-3.5 px-3">
                <div class="flex items-center gap-2">
                  <span 
                    class="font-mono font-bold text-sm"
                    :class="day.isToday ? 'text-indigo-600' : 'text-slate-900'"
                  >
                    {{ day.dateFormatted }}
                  </span>
                  <span class="text-[11px] text-slate-400 font-normal">({{ day.dayOfWeek }})</span>
                  <span 
                    v-if="day.isToday" 
                    class="px-2 py-0.5 rounded-full text-[10px] font-extrabold bg-indigo-50 text-indigo-600 border border-indigo-200"
                  >
                    Hôm nay
                  </span>
                </div>
              </td>

              <!-- Lượt Khách -->
              <td class="py-3.5 px-3 text-center">
                <span 
                  class="inline-flex items-center gap-1 px-2.5 py-1 rounded-lg text-xs font-bold"
                  :class="day.ordersCount > 0 ? 'bg-slate-100 text-slate-800' : 'text-slate-400'"
                >
                  <Users class="w-3.5 h-3.5 text-slate-400" />
                  {{ day.ordersCount }}
                </span>
              </td>

              <!-- Dịch vụ -->
              <td class="py-3.5 px-3 text-right text-slate-600">
                {{ day.serviceRevenue > 0 ? formatCurrency(day.serviceRevenue) : '-' }}
              </td>

              <!-- Sản phẩm -->
              <td class="py-3.5 px-3 text-right text-blue-600">
                {{ day.productRevenue > 0 ? formatCurrency(day.productRevenue) : '-' }}
              </td>

              <!-- Giảm giá -->
              <td class="py-3.5 px-3 text-right text-slate-400">
                {{ day.discountTotal > 0 ? `-${formatCurrency(day.discountTotal)}` : '-' }}
              </td>

              <!-- Doanh thu thuần -->
              <td class="py-3.5 px-3 text-right">
                <span 
                  class="font-extrabold text-sm"
                  :class="day.revenue > 0 ? 'text-indigo-600' : 'text-slate-400'"
                >
                  {{ formatCurrency(day.revenue) }}
                </span>
              </td>

              <!-- Chi phí ngày -->
              <td class="py-3.5 px-3 text-right">
                <span :class="day.expense > 0 ? 'text-rose-600 font-bold' : 'text-slate-400'">
                  {{ day.expense > 0 ? `-${formatCurrency(day.expense)}` : '-' }}
                </span>
              </td>

              <!-- Lợi nhuận ròng -->
              <td class="py-3.5 px-3 text-right font-bold">
                <span 
                  v-if="day.revenue > 0 || day.expense > 0"
                  :class="day.netProfit >= 0 ? 'text-emerald-600' : 'text-rose-600'"
                >
                  {{ formatCurrency(day.netProfit) }}
                </span>
                <span v-else class="text-slate-400">-</span>
              </td>

              <!-- Nút Thao tác -->
              <td class="py-3.5 px-3 text-center">
                <button
                  type="button"
                  @click.stop="openDayDetail(day.date)"
                  class="px-2.5 py-1.5 rounded-lg bg-slate-100 hover:bg-indigo-50 hover:text-indigo-600 text-slate-700 font-semibold text-xs transition flex items-center gap-1 mx-auto"
                >
                  <Eye class="w-3.5 h-3.5 text-indigo-600" />
                  <span>Xem đơn</span>
                </button>
              </td>
            </tr>
          </tbody>
        </table>
      </div>

      <!-- Danh Sách Dạng Card (Cho Mobile / iPhone) -->
      <div class="sm:hidden space-y-3">
        <div v-if="!filteredDailyList.length" class="py-8 text-center text-slate-400 text-xs">
          Không có ngày nào phát sinh doanh thu trong tháng này
        </div>

        <div
          v-for="day in paginatedDailyList"
          :key="day.date"
          @click="openDayDetail(day.date)"
          class="p-4 rounded-xl bg-slate-50 border border-slate-200 space-y-3 active:scale-[0.99] transition cursor-pointer"
        >
          <div class="flex justify-between items-center">
            <div class="flex items-center gap-2">
              <span class="font-bold text-slate-900 text-sm" :class="{ 'text-indigo-600': day.isToday }">
                {{ day.dateFormatted }}
              </span>
              <span class="text-xs text-slate-400">({{ day.dayOfWeek }})</span>
              <span v-if="day.isToday" class="px-1.5 py-0.5 rounded text-[10px] font-bold bg-indigo-50 text-indigo-600">
                Hôm nay
              </span>
            </div>
            <span class="inline-flex items-center gap-1 text-xs text-slate-700 bg-white border border-slate-200 px-2 py-0.5 rounded">
              <Users class="w-3 h-3 text-slate-400" />
              {{ day.ordersCount }} đơn
            </span>
          </div>

          <div class="grid grid-cols-2 gap-2 text-xs pt-1 border-t border-slate-200">
            <div>
              <span class="text-slate-500 text-[11px] block">Doanh thu:</span>
              <span class="font-black text-indigo-600 text-base">{{ formatCurrency(day.revenue) }}</span>
            </div>
            <div class="text-right">
              <span class="text-slate-500 text-[11px] block">Thực lãi (Net):</span>
              <span 
                class="font-bold text-sm"
                :class="day.netProfit >= 0 ? 'text-emerald-600' : 'text-rose-600'"
              >
                {{ formatCurrency(day.netProfit) }}
              </span>
            </div>
          </div>

          <div class="flex items-center justify-between text-[11px] text-slate-500 pt-1 border-t border-slate-200">
            <span>Chi phí: {{ day.expense > 0 ? formatCurrency(day.expense) : '0 ₫' }}</span>
            <span class="text-indigo-600 font-semibold flex items-center gap-1">
              Xem {{ day.ordersCount }} đơn <ChevronRight class="w-3 h-3" />
            </span>
          </div>
        </div>
      </div>

      <!-- Thanh Phân Trang (Pagination Controls) -->
      <div 
        v-if="filteredDailyList.length > 0"
        class="flex flex-col sm:flex-row items-center justify-between gap-3 pt-3 border-t border-slate-200 text-xs text-slate-500"
      >
        <!-- Thông tin số lượng & Chọn số dòng/trang -->
        <div class="flex items-center gap-3">
          <span>
            Hiển thị 
            <strong class="text-slate-800">
              {{ pageSize === -1 ? 1 : (currentPage - 1) * pageSize + 1 }}
            </strong>
            -
            <strong class="text-slate-800">
              {{ pageSize === -1 ? filteredDailyList.length : Math.min(currentPage * pageSize, filteredDailyList.length) }}
            </strong>
            trên tổng 
            <strong class="text-indigo-600">{{ filteredDailyList.length }}</strong> ngày
          </span>

          <div class="flex items-center gap-1.5">
            <span class="text-[11px] text-slate-400">Mỗi trang:</span>
            <select
              v-model.number="pageSize"
              @change="onPageSizeChange"
              class="bg-white border border-slate-200 rounded-lg px-2 py-1 text-slate-800 text-xs focus:outline-none focus:border-indigo-500 cursor-pointer"
            >
              <option :value="10">10 ngày</option>
              <option :value="15">15 ngày</option>
              <option :value="20">20 ngày</option>
              <option :value="-1">Tất cả</option>
            </select>
          </div>
        </div>

        <!-- Các nút bấm chuyển trang -->
        <div v-if="totalPages > 1 && pageSize !== -1" class="flex items-center gap-1">
          <!-- Đầu trang -->
          <button
            @click="goToPage(1)"
            :disabled="currentPage === 1"
            class="px-2.5 py-1 rounded-lg border border-slate-200 bg-white hover:bg-slate-50 disabled:opacity-30 disabled:cursor-not-allowed text-slate-700 font-bold transition"
            title="Về trang đầu"
          >
            &laquo;
          </button>

          <!-- Trang trước -->
          <button
            @click="goToPage(currentPage - 1)"
            :disabled="currentPage === 1"
            class="px-3 py-1 rounded-lg border border-slate-200 bg-white hover:bg-slate-50 disabled:opacity-30 disabled:cursor-not-allowed text-slate-700 font-medium transition"
          >
            Trước
          </button>

          <!-- Các số trang -->
          <button
            v-for="page in totalPages"
            :key="page"
            @click="goToPage(page)"
            class="w-7 h-7 rounded-lg text-xs font-bold transition flex items-center justify-center cursor-pointer"
            :class="currentPage === page ? 'bg-indigo-600 text-white shadow-2xs' : 'bg-white border border-slate-200 text-slate-600 hover:bg-slate-50'"
          >
            {{ page }}
          </button>

          <!-- Trang sau -->
          <button
            @click="goToPage(currentPage + 1)"
            :disabled="currentPage === totalPages"
            class="px-3 py-1 rounded-lg border border-slate-200 bg-white hover:bg-slate-50 disabled:opacity-30 disabled:cursor-not-allowed text-slate-700 font-medium transition"
          >
            Sau
          </button>

          <!-- Cuối trang -->
          <button
            @click="goToPage(totalPages)"
            :disabled="currentPage === totalPages"
            class="px-2.5 py-1 rounded-lg border border-slate-200 bg-white hover:bg-slate-50 disabled:opacity-30 disabled:cursor-not-allowed text-slate-700 font-bold transition"
            title="Đến trang cuối"
          >
            &raquo;
          </button>
        </div>
      </div>

    </div>

    <!-- MODAL XEM CHI TIẾT DOANH THU & ĐƠN HÀNG TRONG NGÀY -->
    <div
      v-if="dayDetailModalOpen"
      class="fixed inset-0 z-50 flex items-center justify-center bg-slate-900/40 backdrop-blur-sm p-3 sm:p-4 animate-fade-in"
      @click.self="dayDetailModalOpen = false"
    >
      <div class="bg-white border border-slate-200 rounded-2xl max-w-2xl w-full max-h-[90vh] flex flex-col shadow-2xl overflow-hidden">
        
        <!-- Modal Header -->
        <div class="p-4 sm:p-5 border-b border-slate-100 flex justify-between items-start bg-slate-50">
          <div>
            <div class="flex items-center gap-2">
              <CalendarDays class="w-5 h-5 text-indigo-600" />
              <h3 class="text-base sm:text-lg font-extrabold text-slate-900">
                Báo Cáo Chi Tiết: {{ selectedDayDetail?.dateFormatted || 'Đang tải...' }}
              </h3>
              <span class="text-xs text-slate-500">({{ selectedDayDetail?.dayOfWeek }})</span>
            </div>
            <p class="text-xs text-slate-500 mt-1">
              Toàn bộ hóa đơn khách hàng và các khoản chi tiêu phát sinh trong ngày
            </p>
          </div>
          <button
            @click="dayDetailModalOpen = false"
            class="p-2 text-slate-400 hover:text-slate-600 rounded-lg hover:bg-slate-100 transition"
          >
            <X class="w-5 h-5" />
          </button>
        </div>

        <!-- Modal Body (Scrollable) -->
        <div class="p-4 sm:p-5 overflow-y-auto space-y-5 text-xs">
          
          <!-- Loading state -->
          <div v-if="loadingDayDetail" class="py-12 text-center text-slate-400 space-y-2">
            <RotateCcw class="w-6 h-6 animate-spin mx-auto text-indigo-600" />
            <p>Đang tải dữ liệu doanh thu của ngày...</p>
          </div>

          <template v-else-if="selectedDayDetail">
            
            <!-- 4 Thẻ Thống Kê Nhanh Của Ngày -->
            <div class="grid grid-cols-2 sm:grid-cols-4 gap-2.5">
              <div class="p-3 rounded-xl bg-slate-50 border border-slate-200">
                <span class="text-[10px] text-slate-500 block mb-0.5">Doanh Thu Thuần</span>
                <span class="text-sm font-extrabold text-indigo-600">
                  {{ formatCurrency(selectedDayDetail.revenue) }}
                </span>
                <span class="text-[10px] text-slate-400 block mt-0.5">{{ selectedDayDetail.ordersCount }} lượt đơn</span>
              </div>

              <div class="p-3 rounded-xl bg-slate-50 border border-slate-200">
                <span class="text-[10px] text-slate-500 block mb-0.5">Chi Phí Ngày</span>
                <span class="text-sm font-bold text-rose-600">
                  {{ formatCurrency(selectedDayDetail.expense) }}
                </span>
                <span class="text-[10px] text-slate-400 block mt-0.5">{{ selectedDayDetail.expenses?.length || 0 }} khoản chi</span>
              </div>

              <div class="p-3 rounded-xl bg-slate-50 border border-slate-200">
                <span class="text-[10px] text-slate-500 block mb-0.5">Thực Lãi (Net)</span>
                <span 
                  class="text-sm font-extrabold"
                  :class="selectedDayDetail.netProfit >= 0 ? 'text-emerald-600' : 'text-rose-600'"
                >
                  {{ formatCurrency(selectedDayDetail.netProfit) }}
                </span>
                <span class="text-[10px] text-slate-400 block mt-0.5">Sau khi trừ chi</span>
              </div>

              <div class="p-3 rounded-xl bg-slate-50 border border-slate-200">
                <span class="text-[10px] text-slate-500 block mb-0.5">Nguồn Thu</span>
                <div class="text-[10px] text-slate-700 space-y-0.5">
                  <p class="flex justify-between">
                    <span class="text-blue-600">VietQR:</span>
                    <span class="font-bold">{{ formatCurrency(selectedDayDetail.vietQrTotal) }}</span>
                  </p>
                  <p class="flex justify-between">
                    <span class="text-emerald-600">Tiền mặt:</span>
                    <span class="font-bold">{{ formatCurrency(selectedDayDetail.cashTotal) }}</span>
                  </p>
                </div>
              </div>
            </div>

            <!-- Danh Sách Đơn Hàng Trong Ngày -->
            <div class="space-y-3">
              <div class="flex items-center justify-between border-b border-slate-200 pb-2">
                <h4 class="font-bold text-slate-800 text-xs uppercase tracking-wider flex items-center gap-1.5">
                  <Sparkles class="w-4 h-4 text-indigo-600" />
                  Danh Sách Đơn Hàng ({{ selectedDayDetail.orders?.length || 0 }} đơn)
                </h4>
              </div>

              <div v-if="!selectedDayDetail.orders?.length" class="py-6 text-center text-slate-400 text-xs">
                Ngày này không có lượt đơn hàng nào.
              </div>

              <div class="space-y-2.5">
                <div
                  v-for="order in selectedDayDetail.orders"
                  :key="order.id"
                  class="p-3.5 rounded-xl bg-slate-50 border border-slate-200/80 space-y-2"
                >
                  <div class="flex justify-between items-center">
                    <div class="flex items-center gap-2">
                      <span class="font-mono font-bold text-indigo-700 bg-white border border-slate-200 px-2 py-0.5 rounded text-xs shadow-2xs">
                        {{ order.orderCode }}
                      </span>
                      <span class="text-slate-800 font-semibold">{{ order.customerName || 'Khách vãng lai' }}</span>
                      <span 
                        class="text-[10px] px-2 py-0.5 rounded font-bold"
                        :class="order.paymentMethod === 'VietQR' ? 'bg-blue-50 text-blue-600 border border-blue-200' : 'bg-emerald-50 text-emerald-600 border border-emerald-200'"
                      >
                        {{ order.paymentMethod }}
                      </span>
                    </div>
                    <span class="font-black text-indigo-600 text-sm">
                      {{ formatCurrency(order.finalAmount) }}
                    </span>
                  </div>

                  <!-- Danh sách món trong đơn -->
                  <div class="pl-2 border-l-2 border-indigo-200 space-y-1 text-[11px] text-slate-600">
                    <div v-for="(it, idx) in order.items" :key="idx" class="flex justify-between">
                      <span>• {{ it.itemName }} (x{{ it.quantity }})</span>
                      <span class="font-medium text-slate-800">{{ formatCurrency(it.totalPrice) }}</span>
                    </div>
                  </div>
                </div>
              </div>
            </div>

            <!-- Danh Sách Chi Phí Trong Ngày (nếu có) -->
            <div v-if="selectedDayDetail.expenses?.length" class="space-y-2.5 pt-2 border-t border-slate-200">
              <h4 class="font-bold text-slate-800 text-xs uppercase tracking-wider flex items-center gap-1.5 text-rose-600">
                <ArrowDownRight class="w-4 h-4" />
                Các Khoản Chi Tiêu Trong Ngày ({{ selectedDayDetail.expenses.length }} khoản)
              </h4>

              <div class="space-y-1.5">
                <div
                  v-for="exp in selectedDayDetail.expenses"
                  :key="exp.id"
                  class="p-2.5 rounded-xl bg-rose-50/50 border border-rose-100 flex justify-between items-center text-xs"
                >
                  <div>
                    <span class="font-bold text-slate-900 block">{{ exp.title }}</span>
                    <span class="text-[10px] text-slate-500">Danh mục: {{ exp.category }}</span>
                  </div>
                  <span class="font-bold text-rose-600 font-mono">-{{ formatCurrency(exp.amount) }}</span>
                </div>
              </div>
            </div>

          </template>
        </div>

        <!-- Modal Footer -->
        <div class="p-3 sm:p-4 border-t border-slate-100 bg-slate-50 flex justify-end">
          <button
            @click="dayDetailModalOpen = false"
            class="px-5 py-2 rounded-xl bg-slate-900 hover:bg-slate-800 text-white font-bold text-xs transition shadow-sm"
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
  CalendarDays, 
  Calendar,
  RotateCcw, 
  Sparkles, 
  ArrowDownRight,
  TrendingUp,
  Scale,
  Eye,
  ChevronRight,
  X,
  Wallet,
  BarChart3,
  LineChart
} from 'lucide-vue-next'

import {
  Chart as ChartJS,
  Title,
  Tooltip,
  Legend,
  BarElement,
  PointElement,
  LineElement,
  Filler,
  CategoryScale,
  LinearScale
} from 'chart.js'
import { Bar, Line } from 'vue-chartjs'

ChartJS.register(Title, Tooltip, Legend, BarElement, PointElement, LineElement, Filler, CategoryScale, LinearScale)

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
  monthlyExpenseTrend: [],
  dailyBreakdown: []
})

const expenseChartType = ref('line') // 'line' | 'bar'

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

// State & Logic Phân trang Bảng kê từng ngày
const currentPage = ref(1)
const pageSize = ref(10) // 10 ngày / trang

const totalPages = computed(() => {
  if (pageSize.value === -1) return 1
  return Math.ceil(filteredDailyList.value.length / pageSize.value) || 1
})

const paginatedDailyList = computed(() => {
  if (pageSize.value === -1) return filteredDailyList.value
  const start = (currentPage.value - 1) * pageSize.value
  return filteredDailyList.value.slice(start, start + pageSize.value)
})

function goToPage(p) {
  if (p >= 1 && p <= totalPages.value) {
    currentPage.value = p
  }
}

function onPageSizeChange() {
  currentPage.value = 1
}

function onFilterModeChange(mode) {
  dailyFilterMode.value = mode
  currentPage.value = 1
}

async function loadSummary() {
  try {
    currentPage.value = 1
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
        backgroundColor: '#4f46e5',
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
      backgroundColor: '#0f172a',
      titleColor: '#ffffff',
      bodyColor: '#f8fafc',
      padding: 10,
      cornerRadius: 8,
      callbacks: {
        label: (ctx) => ` Doanh thu: ${new Intl.NumberFormat('vi-VN').format(ctx.raw)} đ`
      }
    }
  },
  scales: {
    x: {
      grid: { display: false },
      ticks: { color: '#64748b', font: { size: 11, weight: '600' } }
    },
    y: {
      grid: { color: '#f1f5f9' },
      ticks: {
        color: '#64748b',
        font: { size: 10 },
        callback: (value) => `${value / 1000}k`
      }
    }
  }
}

// Thống kê nhanh biến động chi tiêu
const avgMonthlyExpense = computed(() => {
  const trend = summary.value.monthlyExpenseTrend || []
  if (!trend.length) return 0
  const total = trend.reduce((sum, item) => sum + item.totalExpense, 0)
  return Math.round(total / trend.length)
})

const highestExpenseMonth = computed(() => {
  const trend = summary.value.monthlyExpenseTrend || []
  if (!trend.length) return null
  return [...trend].sort((a, b) => b.totalExpense - a.totalExpense)[0]
})

// Cấu hình Biểu đồ Biến động Chi tiêu từng tháng
const expenseChartData = computed(() => {
  const trend = summary.value.monthlyExpenseTrend || []
  const isLine = expenseChartType.value === 'line'

  return {
    labels: trend.map(t => t.monthName),
    datasets: [
      {
        type: expenseChartType.value,
        label: 'Chi phí (VNĐ)',
        borderColor: '#f43f5e', // Rose 500
        backgroundColor: isLine ? 'rgba(244, 63, 94, 0.12)' : '#f43f5e',
        data: trend.map(t => t.totalExpense),
        fill: isLine,
        tension: 0.35,
        borderWidth: isLine ? 3 : 0,
        pointBackgroundColor: '#f43f5e',
        pointBorderColor: '#ffffff',
        pointBorderWidth: 2,
        pointRadius: 5,
        pointHoverRadius: 7,
        borderRadius: isLine ? 0 : 8
      },
      {
        type: expenseChartType.value,
        label: 'Doanh thu (VNĐ)',
        borderColor: '#4f46e5', // Diro Indigo
        backgroundColor: isLine ? 'rgba(79, 70, 229, 0.08)' : '#4f46e5',
        data: trend.map(t => t.totalRevenue),
        fill: false,
        tension: 0.35,
        borderWidth: isLine ? 2 : 0,
        borderDash: isLine ? [5, 5] : undefined,
        pointBackgroundColor: '#4f46e5',
        pointBorderColor: '#ffffff',
        pointBorderWidth: 1.5,
        pointRadius: 4,
        pointHoverRadius: 6,
        borderRadius: isLine ? 0 : 8
      }
    ]
  }
})

const expenseChartOptions = {
  responsive: true,
  maintainAspectRatio: false,
  interaction: {
    mode: 'index',
    intersect: false
  },
  plugins: {
    legend: {
      display: true,
      position: 'top',
      align: 'end',
      labels: {
        color: '#475569',
        boxWidth: 12,
        boxHeight: 12,
        font: { size: 11, weight: 'bold' }
      }
    },
    tooltip: {
      backgroundColor: '#0f172a',
      titleColor: '#ffffff',
      bodyColor: '#f8fafc',
      padding: 10,
      cornerRadius: 8,
      callbacks: {
        label: (ctx) => ` ${ctx.dataset.label}: ${new Intl.NumberFormat('vi-VN').format(ctx.raw)} đ`
      }
    }
  },
  scales: {
    x: {
      grid: { color: '#f1f5f9' },
      ticks: { color: '#64748b', font: { size: 11, weight: '600' } }
    },
    y: {
      grid: { color: '#f1f5f9' },
      ticks: {
        color: '#64748b',
        font: { size: 10 },
        callback: (value) => `${value / 1000}k`
      }
    }
  }
}
</script>
