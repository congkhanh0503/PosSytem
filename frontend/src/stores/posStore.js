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

  // Add Item to Cart (Kiểm soát tồn kho nghiêm ngặt)
  function addItem(item, type = 'Service') {
    const existingIndex = cart.value.findIndex(
      cartItem => cartItem.itemType === type && cartItem.itemId === item.id
    )

    if (type === 'Product') {
      const stock = Number(item.stockQuantity) || 0
      if (stock <= 0) {
        return { 
          success: false, 
          message: `Sản phẩm "${item.name}" đã HẾT HÀNG trong kho (Tồn kho: 0)!` 
        }
      }

      if (existingIndex > -1) {
        const currentQty = cart.value[existingIndex].quantity
        if (currentQty >= stock) {
          return { 
            success: false, 
            message: `Sản phẩm "${item.name}" chỉ còn ${stock} trong kho! Không thể thêm nữa.` 
          }
        }
        cart.value[existingIndex].quantity += 1
        cart.value[existingIndex].stockQuantity = stock
        return { success: true }
      } else {
        cart.value.push({
          itemType: type,
          itemId: item.id,
          itemName: item.name,
          unitPrice: item.salePrice,
          quantity: 1,
          category: item.category,
          stockQuantity: stock
        })
        return { success: true }
      }
    } else {
      // Dịch vụ: thêm bình thường
      if (existingIndex > -1) {
        cart.value[existingIndex].quantity += 1
      } else {
        cart.value.push({
          itemType: type,
          itemId: item.id,
          itemName: item.name,
          unitPrice: item.price,
          quantity: 1,
          category: item.category,
          stockQuantity: null
        })
      }
      return { success: true }
    }
  }

  function removeItem(index) {
    cart.value.splice(index, 1)
  }

  function updateQuantity(index, delta) {
    const target = cart.value[index]
    if (!target) return { success: false }

    if (delta > 0 && target.itemType === 'Product' && target.stockQuantity !== null && target.stockQuantity !== undefined) {
      if (target.quantity + delta > target.stockQuantity) {
        return { 
          success: false, 
          message: `Sản phẩm "${target.itemName}" chỉ còn ${target.stockQuantity} trong kho! Không thể tăng thêm.` 
        }
      }
    }

    const newQty = target.quantity + delta
    if (newQty <= 0) {
      removeItem(index)
      return { success: true }
    } else {
      target.quantity = newQty
      return { success: true }
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

    // Kiểm tra tồn kho trước khi gửi request thanh toán
    for (const item of cart.value) {
      if (item.itemType === 'Product' && item.stockQuantity !== null && item.stockQuantity !== undefined) {
        if (item.stockQuantity <= 0) {
          throw new Error(`Sản phẩm "${item.itemName}" đã HẾT HÀNG trong kho (Tồn kho: 0). Vui lòng xóa khỏi đơn!`)
        }
        if (item.quantity > item.stockQuantity) {
          throw new Error(`Sản phẩm "${item.itemName}" vượt quá tồn kho (Trong kho còn: ${item.stockQuantity}, bạn đang bán: ${item.quantity}).`)
        }
      }
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
