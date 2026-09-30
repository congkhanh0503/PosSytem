# ==============================================================================
# SCRIPT TU DONG BIEN DICH VA DONG GOI BO CAI DAT DIROPOS PRO (EXE 1-CLICK)
# ==============================================================================

$ErrorActionPreference = "Stop"
$root = "d:\Project\pos\PosSytem"
$frontendDir = Join-Path $root "frontend"
$backendDir = Join-Path $root "backend\DiroPos.Api"
$wwwrootDir = Join-Path $backendDir "wwwroot"
$publishDir = Join-Path $backendDir "publish"
$outputDir = Join-Path $root "installer_output"

Write-Host "==========================================================" -ForegroundColor Cyan
Write-Host "BAT DAU QUY TRINH DONG GOI DIROPOS PRO THUONG MAI" -ForegroundColor Yellow
Write-Host "==========================================================" -ForegroundColor Cyan

# 1. Tim trinh bien dich Inno Setup (ISCC.exe)
$isccPath = $null
$candidatePaths = @(
    "C:\Users\$env:USERNAME\AppData\Local\Programs\Inno Setup 6\ISCC.exe",
    "C:\Program Files (x86)\Inno Setup 6\ISCC.exe",
    "C:\Program Files\Inno Setup 6\ISCC.exe"
)
foreach ($cand in $candidatePaths) {
    if (Test-Path $cand) {
        $isccPath = $cand
        break
    }
}

if (-not $isccPath) {
    Write-Error "Khong tim thay Inno Setup compiler (ISCC.exe)!"
    exit 1
}
Write-Host "Da tim thay Inno Setup compiler tai: $isccPath" -ForegroundColor Green

# 2. Don dep thu muc cu
Write-Host ""
Write-Host "[1/4] Don dep cac thu muc build cu..." -ForegroundColor Cyan
if (Test-Path $wwwrootDir) { Remove-Item $wwwrootDir -Recurse -Force }
if (Test-Path $publishDir) { Remove-Item $publishDir -Recurse -Force }
if (-not (Test-Path $outputDir)) { New-Item -ItemType Directory -Path $outputDir | Out-Null }
New-Item -ItemType Directory -Path $wwwrootDir | Out-Null
Write-Host "Don dep hoan tat." -ForegroundColor Green

# 3. Bien dich Frontend (Vue.js) o che do Production
Write-Host ""
Write-Host "[2/4] Bien dich Frontend Vue 3 (Minify, Drop console, No sourcemap)..." -ForegroundColor Cyan
Push-Location $frontendDir
try {
    npm run build
    if ($LASTEXITCODE -ne 0) { throw "Loi build frontend!" }
}
finally {
    Pop-Location
}

# Copy dist vao wwwroot cua Backend
$distDir = Join-Path $frontendDir "dist"
Copy-Item "$distDir\*" -Destination $wwwrootDir -Recurse -Force
Write-Host "Da dong goi toan bo giao dien Frontend vao wwwroot Backend." -ForegroundColor Green

# 4. Bien dich Backend C# .NET 10 (Single-File Self-Contained win-x64)
Write-Host ""
Write-Host "[3/4] Bien dich Backend .NET 10 thanh 1 file EXE doc lap (win-x64)..." -ForegroundColor Cyan
Push-Location $backendDir
try {
    dotnet publish -c Release -r win-x64 --self-contained true /p:PublishSingleFile=true /p:EnableCompressionInSingleFile=true /p:IncludeNativeLibrariesForSelfExtract=true -o $publishDir
    if ($LASTEXITCODE -ne 0) { throw "Loi publish backend .NET!" }
}
finally {
    Pop-Location
}

$exeFile = Join-Path $publishDir "DiroPos.exe"
if (-not (Test-Path $exeFile)) {
    Write-Error "Loi: Khong tim thay file $exeFile sau khi publish!"
    exit 1
}
$exeSizeMb = [math]::Round(((Get-Item $exeFile).Length / 1MB), 2)
Write-Host "File DiroPos.exe doc lap da san sang ($exeSizeMb MB)." -ForegroundColor Green

# 5. Dong goi bo cai dat Inno Setup
Write-Host ""
Write-Host "[4/4] Dong goi thanh DiroPos_Setup_v1.0.0.exe bang Inno Setup..." -ForegroundColor Cyan
Push-Location $root
try {
    $issFile = Join-Path $root "DiroPos_Setup.iss"
    & "$isccPath" "$issFile"
    if ($LASTEXITCODE -ne 0) { throw "Loi bien dich Inno Setup!" }
}
finally {
    Pop-Location
}

$setupFile = Join-Path $outputDir "DiroPos_Setup_v1.0.0.exe"
if (Test-Path $setupFile) {
    $setupSizeMb = [math]::Round(((Get-Item $setupFile).Length / 1MB), 2)
    Write-Host ""
    Write-Host "==========================================================" -ForegroundColor Green
    Write-Host "THANH CONG RUC RO! DA TAO BO CAI DAT THUONG MAI:" -ForegroundColor Yellow
    Write-Host "Duong dan: $setupFile" -ForegroundColor Cyan
    Write-Host "Dung luong: $setupSizeMb MB" -ForegroundColor Cyan
    Write-Host "Dac tinh:" -ForegroundColor White
    Write-Host " - Code C# va Vue.js duoc dong goi nguyen khoi trong 1 file EXE duy nhat" -ForegroundColor Gray
    Write-Host " - 1-Click Install: Khong can quyen Admin, cai vao C:\DiroPos\" -ForegroundColor Gray
    Write-Host " - Tu dong tao Desktop Shortcut voi Icon DiroPos chinh thuc" -ForegroundColor Gray
    Write-Host " - Tu dong dang ky Startup (Bat may tinh la tu chay ngam & tu mo trang POS)" -ForegroundColor Gray
    Write-Host " - Hoan toan khong co mat khau bo cai (Trai nghiem muot ma 100%)" -ForegroundColor Gray
    Write-Host "==========================================================" -ForegroundColor Green
} else {
    Write-Error "Loi: Khong tim thay file $setupFile sau khi dong goi!"
}
