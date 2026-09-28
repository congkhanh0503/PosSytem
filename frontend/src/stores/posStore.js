import { defineStore } from 'pinia'
import { ref, computed } from 'vue'
import api from '@/services/api'

export const usePosStore = defineStore('pos', () => {
  const cart = ref([])
  const discountPercent = ref(0)
  const note = ref('')
  const customerName = ref('')
  const customerPhone = ref('')
  const isLoading = ref(false)

  // Subtotal
  const subTotal = computed(() => {
    return cart.value.reduce((sum, item) => sum + (item.unitPrice * item.quantity), 0)
  })

  // Discount Amount
  const discountAmount = computed(() => {
    if (discountPercent.value <= 0) return 0
    return Math.round(subTotal.value * (discountPercent.value / 100))
  })

  // Final Amount
  const finalAmount = computed(() => {
    return Math.max(0, subTotal.value - discountAmount.value)
  })

  // Add Item to Cart
  function addItem(item, type = 'Service') {
    const existingIndex = cart.value.findIndex(
      cartItem => cartItem.itemType === type && cartItem.itemId === item.id
    )

    if (existingIndex > -1) {
      cart.value[existingIndex].quantity += 1
    } else {
      cart.value.push({
        itemType: type,
        itemId: item.id,
        itemName: item.name,
        unitPrice: type === 'Service' ? item.price : item.salePrice,
        quantity: 1,
        category: item.category
      })
    }
  }

  function removeItem(index) {
    cart.value.splice(index, 1)
  }

  function updateQuantity(index, delta) {
    const target = cart.value[index]
    if (!target) return

    const newQty = target.quantity + delta
    if (newQty <= 0) {
      removeItem(index)
    } else {
      target.quantity = newQty
    }
  }

  function setDiscount(percent) {
    discountPercent.value = Number(percent) || 0
  }

  function clearCart() {
    cart.value = []
    discountPercent.value = 0
    note.value = ''
    customerName.value = ''
    customerPhone.value = ''
  }

  // Submit checkout
  async function checkout(paymentMethod = 'VietQR') {
    if (cart.value.length === 0) {
      throw new Error('Giỏ hàng trống!')
    }

    isLoading.value = true
    try {
      const payload = {
        customerName: customerName.value.trim() || 'Khách vãng lai',
        customerPhone: customerPhone.value.trim() || null,
        discountPercent: Number(discountPercent.value) || 0,
        paymentMethod,
        note: note.value.trim() || null,
        items: cart.value.map(item => ({
          itemType: item.itemType,
          itemId: item.itemId,
          itemName: item.itemName,
          quantity: item.quantity,
          unitPrice: item.unitPrice
        }))
      }

      const response = await api.createOrder(payload)
      return response.data
    } finally {
      isLoading.value = false
    }
  }

  return {
    cart,
    discountPercent,
    note,
    customerName,
    customerPhone,
    isLoading,
    subTotal,
    discountAmount,
    finalAmount,
    addItem,
    removeItem,
    updateQuantity,
    setDiscount,
    clearCart,
    checkout
  }
})
