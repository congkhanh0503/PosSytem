<template>
  <div class="flex h-screen w-screen overflow-hidden bg-slate-50 text-slate-800 selection:bg-indigo-600 selection:text-white">
    <!-- Left Navigation Bar -->
    <Sidebar />

    <!-- Main Content Area -->
    <main class="flex-1 h-screen overflow-hidden bg-slate-50 relative">
      <RouterView />
    </main>
  </div>
</template>

<script setup>
import Sidebar from '@/components/Sidebar.vue'
</script>
