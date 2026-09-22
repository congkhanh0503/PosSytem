<template>
  <aside class="w-64 bg-barber-card border-r border-barber-border flex flex-col justify-between flex-shrink-0 min-h-screen">
    <!-- Top Branding -->
    <div>
      <div class="p-6 border-b border-barber-border/60">
        <div class="flex items-center gap-3">
          <div class="w-10 h-10 rounded-xl bg-gradient-to-br from-barber-gold to-amber-700 flex items-center justify-center text-black font-extrabold text-xl shadow-lg shadow-amber-900/30">
            CB
          </div>
          <div>
            <h1 class="font-bold text-lg tracking-wider text-white">CÔNG BARBER</h1>
            <p class="text-xs text-barber-gold font-medium uppercase tracking-widest">Solo POS System</p>
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
              ? 'bg-barber-gold text-black font-semibold shadow-md shadow-amber-500/20'
              : 'text-zinc-400 hover:text-white hover:bg-white/5'
          ]"
        >
          <component
            :is="item.icon"
            class="w-5 h-5 transition-transform group-hover:scale-110"
            :class="[$route.path === item.path ? 'text-black' : 'text-zinc-400 group-hover:text-barber-gold']"
          />
          <span>{{ item.label }}</span>
        </RouterLink>
      </nav>
    </div>

    <!-- Bottom Status / Quick Info -->
    <div class="p-4 border-t border-barber-border/60">
      <div class="p-3.5 rounded-xl bg-barber-dark/80 border border-barber-border flex items-center gap-3">
        <div class="relative">
          <div class="w-9 h-9 rounded-full bg-zinc-800 border border-barber-gold/40 flex items-center justify-center text-sm font-bold text-barber-gold">
            C
          </div>
          <span class="w-2.5 h-2.5 rounded-full bg-emerald-500 absolute -top-0.5 -right-0.5 ring-2 ring-barber-card"></span>
        </div>
        <div class="overflow-hidden">
          <p class="text-xs font-semibold text-white truncate">Barber Công</p>
          <p class="text-[11px] text-emerald-400">Đang hoạt động</p>
        </div>
      </div>
    </div>
  </aside>
</template>

<script setup>
import { 
  Scissors, 
  LayoutDashboard, 
  Package, 
  ReceiptText, 
  Settings,
  Sparkles,
  WalletCards
} from 'lucide-vue-next'

const navItems = [
  { path: '/', label: 'Bán Hàng POS', icon: Scissors },
  { path: '/dashboard', label: 'Báo Cáo Doanh Thu', icon: LayoutDashboard },
  { path: '/expenses', label: 'Quản Lý Chi Tiêu', icon: WalletCards },
  { path: '/services', label: 'Quản Lý Dịch Vụ', icon: Sparkles },
  { path: '/products', label: 'Sản Phẩm & Kho', icon: Package },
  { path: '/orders', label: 'Lịch Sử Đơn Hàng', icon: ReceiptText },
  { path: '/settings', label: 'Cài Đặt VietQR', icon: Settings }
]
</script>
