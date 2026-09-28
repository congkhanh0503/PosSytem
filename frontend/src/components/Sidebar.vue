<template>
  <aside class="w-64 bg-white border-r border-slate-200 flex flex-col justify-between flex-shrink-0 min-h-screen shadow-sm">
    <!-- Top Branding -->
    <div>
      <div class="p-6 border-b border-slate-100">
        <div class="flex items-center gap-3">
          <div class="w-11 h-11 rounded-2xl bg-gradient-to-br from-indigo-600 via-indigo-500 to-blue-600 flex items-center justify-center text-white font-black text-xl shadow-lg shadow-indigo-500/25 tracking-tighter">
            DP
          </div>
          <div>
            <div class="flex items-center gap-1.5">
              <h1 class="font-extrabold text-xl tracking-tight text-slate-900">
                Diro<span class="text-indigo-600">Pos</span>
              </h1>
              <span class="px-1.5 py-0.5 rounded text-[10px] font-extrabold bg-indigo-50 text-indigo-600 border border-indigo-100">
                PRO
              </span>
            </div>
            <p class="text-[11px] text-slate-400 font-medium tracking-wide">Smart Retail & POS OS</p>
          </div>
        </div>
      </div>

      <!-- Navigation Links -->
      <nav class="p-4 space-y-1.5">
        <RouterLink
          v-for="item in navItems"
          :key="item.path"
          :to="item.path"
          class="flex items-center gap-3.5 px-4 py-3 rounded-xl font-medium text-sm transition-all duration-200 group"
          :class="[
            $route.path === item.path
              ? 'bg-indigo-600 text-white font-semibold shadow-md shadow-indigo-500/20'
              : 'text-slate-600 hover:text-slate-900 hover:bg-slate-100/80'
          ]"
        >
          <component
            :is="item.icon"
            class="w-5 h-5 transition-transform group-hover:scale-110"
            :class="[$route.path === item.path ? 'text-white' : 'text-slate-400 group-hover:text-indigo-600']"
          />
          <span>{{ item.label }}</span>
        </RouterLink>
      </nav>
    </div>

    <!-- Bottom Status / Quick Info -->
    <div class="p-4 border-t border-slate-100">
      <div class="p-3.5 rounded-xl bg-slate-50 border border-slate-200/80 flex items-center gap-3">
        <div class="relative">
          <div class="w-9 h-9 rounded-full bg-indigo-100 border border-indigo-200 flex items-center justify-center text-sm font-bold text-indigo-600">
            D
          </div>
          <span class="w-2.5 h-2.5 rounded-full bg-emerald-500 absolute -top-0.5 -right-0.5 ring-2 ring-white"></span>
        </div>
        <div class="overflow-hidden">
          <p class="text-xs font-bold text-slate-800 truncate">Quản Trị Viên</p>
          <p class="text-[11px] text-emerald-600 font-medium">Hệ thống sẵn sàng</p>
        </div>
      </div>
    </div>
  </aside>
</template>

<script setup>
import { 
  Store, 
  LayoutDashboard, 
  Package, 
  ReceiptText, 
  Settings,
  Sparkles,
  WalletCards
} from 'lucide-vue-next'

const navItems = [
  { path: '/', label: 'Bán Hàng POS', icon: Store },
  { path: '/dashboard', label: 'Báo Cáo Doanh Thu', icon: LayoutDashboard },
  { path: '/expenses', label: 'Quản Lý Chi Tiêu', icon: WalletCards },
  { path: '/services', label: 'Quản Lý Dịch Vụ', icon: Sparkles },
  { path: '/products', label: 'Sản Phẩm & Kho', icon: Package },
  { path: '/orders', label: 'Lịch Sử Đơn Hàng', icon: ReceiptText },
  { path: '/settings', label: 'Cài Đặt VietQR', icon: Settings }
]
</script>
