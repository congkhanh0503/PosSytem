<template>
  <div class="p-5 rounded-2xl bg-white border border-slate-200/80 shadow-sm relative overflow-hidden group hover:border-indigo-300 hover:shadow-md transition-all duration-300">
    <!-- Top icon & badge -->
    <div class="flex items-center justify-between mb-3">
      <span class="text-xs font-semibold text-slate-500 uppercase tracking-wider">{{ title }}</span>
      <div 
        class="w-10 h-10 rounded-xl flex items-center justify-center transition-transform group-hover:scale-110"
        :class="iconBgClass || 'bg-indigo-50 text-indigo-600'"
      >
        <slot name="icon"></slot>
      </div>
    </div>

    <!-- Metric Value -->
    <div class="space-y-1">
      <h4 class="text-2xl font-black text-slate-900 tracking-tight">
        {{ value }}
      </h4>
      <p v-if="subtext" class="text-xs text-slate-500 flex items-center gap-1.5 font-medium">
        <span>{{ subtext }}</span>
      </p>
    </div>

    <!-- Subtle gradient glow in the corner -->
    <div class="absolute -right-6 -bottom-6 w-24 h-24 bg-indigo-500/5 rounded-full blur-xl group-hover:bg-indigo-500/10 transition-colors pointer-events-none"></div>
  </div>
</template>

<script setup>
defineProps({
  title: String,
  value: [String, Number],
  subtext: String,
  iconBgClass: String
})
</script>
