import QRCode from 'qrcode'

// Danh sách mã BIN chuẩn NAPAS của các ngân hàng tại Việt Nam
export const BANK_BIN_MAP = {
  MB: '970422',
  MBBANK: '970422',
  VCB: '970436',
  VIETCOMBANK: '970436',
  TCB: '970407',
  TECHCOMBANK: '970407',
  VPB: '970432',
  VPBANK: '970432',
  TPB: '970423',
  TPBANK: '970423',
  ACB: '970416',
  BIDV: '970418',
  VIB: '970441',
  CTG: '970415',
  ICB: '970415',
  VIETINBANK: '970415',
  STB: '970403',
  SACOMBANK: '970403',
  MSB: '970426',
  OCB: '970448',
  SHB: '970443',
  HDB: '970437',
  HDBANK: '970437',
  LPB: '970449',
  LPBANK: '970449',
  VBA: '970405',
  AGRIBANK: '970405',
  SCB: '970429',
  SEAB: '970440',
  SEABANK: '970440',
  BVB: '970454',
  BVBANK: '970454',
  KLB: '970452',
  KIENLONGBANK: '970452',
  NAB: '970428',
  NAMABANK: '970428',
  PGB: '970430',
  PGBANK: '970430',
  PVB: '970412',
  PVCOMBANK: '970412',
  BAB: '970409',
  BACABANK: '970409',
  BAOVIETBANK: '970438',
  SAIGONBANK: '970400',
  SGB: '970400',
  WOORI: '970457',
  SHINHAN: '970424',
  UOB: '970458',
  CIMB: '422589',
  CAKE: '546034',
  TIMO: '963388',
  VIETTELMONEY: '971005',
  VTL: '971005',
  VNPTMONEY: '971011'
}

/**
 * Đóng gói dữ liệu chuẩn TLV (Tag-Length-Value) theo EMVCo
 */
function formatTlv(tag, value) {
  if (value === undefined || value === null || value === '') return ''
  const val = String(value)
  const len = val.length.toString().padStart(2, '0')
  return `${tag}${len}${val}`
}

/**
 * Tính mã kiểm tra CRC16-CCITT chuẩn EMVCo (Poly: 0x1021, Init: 0xFFFF)
 */
function crc16Ccitt(str) {
  let crc = 0xFFFF
  for (let i = 0; i < str.length; i++) {
    crc ^= (str.charCodeAt(i) << 8)
    for (let j = 0; j < 8; j++) {
      if ((crc & 0x8000) !== 0) {
        crc = ((crc << 1) ^ 0x1021) & 0xFFFF
      } else {
        crc = (crc << 1) & 0xFFFF
      }
    }
  }
  return crc.toString(16).toUpperCase().padStart(4, '0')
}

/**
 * Lấy mã BIN ngân hàng từ BankId (ví dụ: "MB" -> "970422")
 */
export function getBankBin(bankId) {
  if (!bankId) return '970422'
  const normalized = bankId.trim().toUpperCase().replace(/[^A-Z0-9]/g, '')
  return BANK_BIN_MAP[normalized] || bankId
}

/**
 * Sinh chuỗi payload EMVCo VietQR chuẩn NAPAS 247 hoàn toàn Offline
 */
export function generateVietQrPayload({ bankId, accountNo, amount, orderCode, description }) {
  const bin = getBankBin(bankId)
  const cleanAccount = String(accountNo || '').trim().replace(/[^0-9a-zA-Z]/g, '')

  // 1. Tag 38: Merchant Account Information (NAPAS 247)
  const sub00 = formatTlv('00', bin)
  const sub01 = formatTlv('01', cleanAccount)
  const subBeneficiary = formatTlv('01', sub00 + sub01)
  const subService = formatTlv('02', 'QRIBFTTA') // Chuyển nhanh 247 đến tài khoản
  const tag38Value = formatTlv('00', 'A000000727') + subBeneficiary + subService

  // 2. Xây dựng các Tag chính
  let payload = ''
  payload += formatTlv('00', '01') // Payload Format Indicator
  payload += formatTlv('01', amount && Number(amount) > 0 ? '12' : '11') // 12: Dynamic, 11: Static
  payload += formatTlv('38', tag38Value)
  payload += formatTlv('53', '704') // Đồng Việt Nam (VND)

  if (amount && Number(amount) > 0) {
    payload += formatTlv('54', Math.round(Number(amount)).toString())
  }

  payload += formatTlv('58', 'VN') // Quốc gia Việt Nam

  // 3. Tag 62: Thông tin bổ sung (Mã đơn hàng & nội dung)
  const addInfo = (orderCode ? orderCode : (description || 'DIROPOS')).trim()
  // Lọc ký tự tiếng Việt có dấu thành không dấu và giới hạn tối đa 25 ký tự theo quy định Napas
  const cleanInfo = addInfo
    .normalize('NFD')
    .replace(/[\u0300-\u036f]/g, '')
    .replace(/đ/g, 'd')
    .replace(/Đ/g, 'D')
    .replace(/[^a-zA-Z0-9 ]/g, '')
    .substring(0, 25)

  if (cleanInfo) {
    payload += formatTlv('62', formatTlv('08', cleanInfo))
  }

  // 4. Tag 63: Checksum CRC16
  payload += '6304'
  const crc = crc16Ccitt(payload)
  return payload + crc
}

/**
 * Sinh Data URL ảnh QR (Base64 PNG) trực tiếp trên trình duyệt, không cần Internet
 */
export async function generateOfflineQrImage(qrPayload, options = {}) {
  const defaultOptions = {
    width: 320,
    margin: 1,
    color: {
      dark: '#0f172a', // Slate 900
      light: '#ffffff'
    },
    errorCorrectionLevel: 'M'
  }

  return await QRCode.toDataURL(qrPayload, { ...defaultOptions, ...options })
}
