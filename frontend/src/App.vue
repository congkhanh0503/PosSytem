<template>
  <div class="flex h-screen w-screen overflow-hidden bg-barber-dark text-zinc-100 selection:bg-barber-gold selection:text-black">
    <!-- Left Navigation Bar -->
    <Sidebar />

    <!-- Main Content Area -->
    <main class="flex-1 h-screen overflow-hidden bg-barber-dark relative">
      <RouterView />
    </main>
  </div>
</template>

<script setup>
import Sidebar from '@/components/Sidebar.vue'
</script>
