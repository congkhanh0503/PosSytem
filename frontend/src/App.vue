<template>
  <div class="flex h-screen w-screen overflow-hidden bg-slate-50 text-slate-800 selection:bg-indigo-600 selection:text-white">
    <!-- Left Navigation Bar -->
    <Sidebar />

    <!-- Main Content Area -->
    <main class="flex-1 h-screen overflow-hidden bg-slate-50 relative">
      <RouterView />
    </main>

    <!-- Modal thiết lập thông tin ban đầu khi mới cài đặt -->
    <InitialSetupModal />

    <!-- Hệ thống kiểm soát & khóa bản quyền toàn cục -->
    <LicenseLockModal />
  </div>
</template>

<script setup>
import Sidebar from '@/components/Sidebar.vue'
import LicenseLockModal from '@/components/LicenseLockModal.vue'
import InitialSetupModal from '@/components/InitialSetupModal.vue'
</script>
