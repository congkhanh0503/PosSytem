-- ==============================================================================
-- BẢNG QUẢN LÝ PHIÊN BẢN ỨNG DỤNG DIROPOS TRÊN SUPABASE (POSTGRESQL)
-- Copy toàn bộ đoạn này và dán vào SQL Editor trên Supabase -> Bấm RUN
-- ==============================================================================

-- 1. Tạo bảng app_versions
CREATE TABLE IF NOT EXISTS public.app_versions (
    id BIGSERIAL PRIMARY KEY,
    version VARCHAR(20) NOT NULL UNIQUE,
    release_date TIMESTAMPTZ DEFAULT NOW(),
    changelog TEXT,
    download_url TEXT,
    is_mandatory BOOLEAN DEFAULT FALSE,
    min_version VARCHAR(20) DEFAULT '1.0.0',
    created_at TIMESTAMPTZ DEFAULT NOW()
);

-- 2. Cấp quyền truy cập đầy đủ (Đọc, Thêm, Sửa, Xóa) cho DiroAdmin và DiroPos
ALTER TABLE public.app_versions ENABLE ROW LEVEL SECURITY;

DROP POLICY IF EXISTS "Cho phép toàn quyền app_versions" ON public.app_versions;

CREATE POLICY "Cho phép toàn quyền app_versions" 
ON public.app_versions 
FOR ALL 
USING (true) 
WITH CHECK (true);

-- Cấp quyền thao tác cho anon và authenticated
GRANT ALL ON TABLE public.app_versions TO anon, authenticated, service_role;
GRANT ALL ON SEQUENCE public.app_versions_id_seq TO anon, authenticated, service_role;

-- 3. Tạo sẵn bản ghi phiên bản v1.0.0 chính thức đầu tiên
INSERT INTO public.app_versions (version, release_date, changelog, download_url, is_mandatory, min_version)
VALUES (
    '1.0.0',
    NOW(),
    '• Phát hành phiên bản thương mại DiroPos PRO đầu tiên.\n• Hỗ trợ đầy đủ bán hàng POS, VietQR động, đóng ca Z-Report.\n• Sao lưu tự động lên Cloud Supabase an toàn và khôi phục 1 chạm.',
    'https://github.com/diropos/releases/download/v1.0.0/DiroPos_Setup_v1.0.0.exe',
    false,
    '1.0.0'
)
ON CONFLICT (version) DO NOTHING;
