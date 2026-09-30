<template>
  <div
    class="fixed top-5 right-5 z-[99999] flex flex-col gap-2.5 max-w-sm w-full pointer-events-none px-4 sm:px-0"
    aria-live="polite"
  >
    <TransitionGroup
      enter-active-class="transform ease-out duration-300 transition"
      enter-from-class="translate-y-2 opacity-0 sm:translate-y-0 sm:translate-x-4 scale-95"
      enter-to-class="translate-y-0 opacity-100 sm:translate-x-0 scale-100"
      leave-active-class="transition ease-in duration-200"
      leave-from-class="opacity-100 scale-100"
      leave-to-class="opacity-0 scale-95"
      move-class="transition ease-in-out duration-300"
    >
      <div
        v-for="item in state.toasts"
        :key="item.id"
        class="pointer-events-auto flex items-start gap-3 p-3.5 rounded-2xl shadow-xl border backdrop-blur-md transition-all duration-200"
        :class="getToastClasses(item.type)"
      >
        <!-- Icon -->
        <div class="flex-shrink-0 mt-0.5">
          <component :is="getIcon(item.type)" class="w-5 h-5" :class="getIconColor(item.type)" />
        </div>

        <!-- Text -->
        <div class="flex-1 min-w-0">
          <h4 v-if="item.title" class="text-xs font-bold leading-tight" :class="getTitleColor(item.type)">
            {{ item.title }}
          </h4>
          <p class="text-xs leading-relaxed mt-0.5 text-slate-700 font-medium break-words">
            {{ item.message }}
          </p>
        </div>

        <!-- Close Button -->
        <button
          type="button"
          @click="removeToast(item.id)"
          class="flex-shrink-0 text-slate-400 hover:text-slate-600 p-1 -mr-1 -mt-1 rounded-lg transition-colors hover:bg-black/5"
          title="Đóng"
        >
          <X class="w-3.5 h-3.5" />
        </button>
      </div>
    </TransitionGroup>
  </div>
</template>

<script setup>
import { useNotify } from '@/composables/useNotify'
import {
  CheckCircle2,
  AlertCircle,
  AlertTriangle,
  Info,
  X
} from 'lucide-vue-next'

const { state, removeToast } = useNotify()

const getIcon = (type) => {
  switch (type) {
    case 'success':
      return CheckCircle2
    case 'error':
      return AlertCircle
    case 'warning':
      return AlertTriangle
    default:
      return Info
  }
}

const getToastClasses = (type) => {
  switch (type) {
    case 'success':
      return 'bg-emerald-50/95 border-emerald-200/80 text-emerald-900 shadow-emerald-500/10'
    case 'error':
      return 'bg-rose-50/95 border-rose-200/80 text-rose-900 shadow-rose-500/10'
    case 'warning':
      return 'bg-amber-50/95 border-amber-200/80 text-amber-900 shadow-amber-500/10'
    default:
      return 'bg-indigo-50/95 border-indigo-200/80 text-indigo-900 shadow-indigo-500/10'
  }
}

const getIconColor = (type) => {
  switch (type) {
    case 'success':
      return 'text-emerald-600'
    case 'error':
      return 'text-rose-600'
    case 'warning':
      return 'text-amber-600'
    default:
      return 'text-indigo-600'
  }
}

const getTitleColor = (type) => {
  switch (type) {
    case 'success':
      return 'text-emerald-950 font-bold'
    case 'error':
      return 'text-rose-950 font-bold'
    case 'warning':
      return 'text-amber-950 font-bold'
    default:
      return 'text-indigo-950 font-bold'
  }
}
</script>
