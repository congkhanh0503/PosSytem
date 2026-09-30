<template>
  <Transition
    enter-active-class="transition duration-200 ease-out"
    enter-from-class="opacity-0"
    enter-to-class="opacity-100"
    leave-active-class="transition duration-150 ease-in"
    leave-from-class="opacity-100"
    leave-to-class="opacity-0"
  >
    <div
      v-if="modal.isOpen"
      class="fixed inset-0 z-[99998] flex items-center justify-center p-4 bg-slate-900/40 backdrop-blur-sm"
      @click.self="handleConfirmAction(false)"
    >
      <div
        class="bg-white rounded-3xl max-w-md w-full shadow-2xl border border-slate-100 overflow-hidden transform transition-all p-6 sm:p-7 animate-in fade-in zoom-in-95 duration-200"
      >
        <!-- Icon & Header -->
        <div class="flex items-start gap-4">
          <div
            class="w-12 h-12 rounded-2xl flex items-center justify-center flex-shrink-0"
            :class="getIconWrapperClass(modal.type)"
          >
            <component :is="getIcon(modal.type)" class="w-6 h-6" :class="getIconColor(modal.type)" />
          </div>

          <div class="flex-1 min-w-0">
            <h3 class="text-lg font-bold text-slate-900 tracking-tight leading-snug">
              {{ modal.title }}
            </h3>
            <p class="text-sm text-slate-500 font-medium mt-1.5 leading-relaxed break-words whitespace-pre-line">
              {{ modal.message }}
            </p>
          </div>
        </div>

        <!-- Action Buttons -->
        <div class="mt-7 flex items-center justify-end gap-3">
          <button
            type="button"
            @click="handleConfirmAction(false)"
            class="px-5 py-2.5 rounded-xl border border-slate-200 text-slate-700 font-semibold text-sm hover:bg-slate-50 active:bg-slate-100 transition-colors shadow-sm"
          >
            {{ modal.cancelText || 'Hủy' }}
          </button>

          <button
            type="button"
            @click="handleConfirmAction(true)"
            class="px-5 py-2.5 rounded-xl font-semibold text-sm text-white shadow-md transition-all active:scale-95"
            :class="getButtonClass(modal.type)"
          >
            {{ modal.confirmText || 'Xác nhận' }}
          </button>
        </div>
      </div>
    </div>
  </Transition>
</template>

<script setup>
import { computed } from 'vue'
import { useNotify } from '@/composables/useNotify'
import {
  AlertTriangle,
  AlertCircle,
  HelpCircle,
  CheckCircle2
} from 'lucide-vue-next'

const { state, handleConfirmAction } = useNotify()
const modal = computed(() => state.confirmModal)

const getIcon = (type) => {
  switch (type) {
    case 'danger':
      return AlertCircle
    case 'warning':
      return AlertTriangle
    case 'success':
      return CheckCircle2
    default:
      return HelpCircle
  }
}

const getIconWrapperClass = (type) => {
  switch (type) {
    case 'danger':
      return 'bg-rose-50 border border-rose-100'
    case 'warning':
      return 'bg-amber-50 border border-amber-100'
    case 'success':
      return 'bg-emerald-50 border border-emerald-100'
    default:
      return 'bg-indigo-50 border border-indigo-100'
  }
}

const getIconColor = (type) => {
  switch (type) {
    case 'danger':
      return 'text-rose-600'
    case 'warning':
      return 'text-amber-600'
    case 'success':
      return 'text-emerald-600'
    default:
      return 'text-indigo-600'
  }
}

const getButtonClass = (type) => {
  switch (type) {
    case 'danger':
      return 'bg-rose-600 hover:bg-rose-700 shadow-rose-500/25'
    case 'warning':
      return 'bg-amber-600 hover:bg-amber-700 shadow-amber-500/25'
    case 'success':
      return 'bg-emerald-600 hover:bg-emerald-700 shadow-emerald-500/25'
    default:
      return 'bg-indigo-600 hover:bg-indigo-700 shadow-indigo-500/25'
  }
}
</script>
