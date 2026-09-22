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
  RotateCcw, 
  Sparkles, 
  Package,
  ArrowDownRight,
  TrendingUp,
  Scale
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
  expenseCategories: []
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
