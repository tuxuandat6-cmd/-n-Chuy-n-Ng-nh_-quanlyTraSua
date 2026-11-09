param([string]$Database = 'DuyCustomerTest_20261109', [string]$SqlServer = '(localdb)\MSSQLLocalDB')
$ErrorActionPreference = 'Stop'
if ($Database -notmatch '^DuyCustomerTest_[A-Za-z0-9_]+$' -or $SqlServer -ne '(localdb)\MSSQLLocalDB') {
    throw 'This fixture only supports a named DuyCustomerTest_ database on local MSSQLLocalDB.'
}
$existing = & sqlcmd -S $SqlServer -E -b -h -1 -Q "SET NOCOUNT ON; SELECT COUNT(*) FROM sys.databases WHERE name=N'$Database';"
if ($LASTEXITCODE -ne 0 -or ($existing -join '').Trim() -ne '0') { throw 'Database exists or connection failed; no data was replaced.' }
$root = Split-Path $PSScriptRoot -Parent
$work = Join-Path $root '.review-source'
New-Item -ItemType Directory -Force -Path $work | Out-Null
$scripts = @('MixueDatMonDb.sql','CapNhat_VanHanh.sql','CapNhat_MaGiamGia_TichDiem.sql',
    'CapNhat_ThemDanhGia.sql','CapNhat_KhoHaiTang.sql','CapNhat_DoiSoat_LichSu.sql','CapNhat_ChiPhiVanHanh.sql','CapNhat_KichCoMon.sql')
foreach ($name in $scripts) {
    # Use the existing team's schema without modifying or claiming its source files.
    $sql = (Get-Content (Join-Path $root "MixueDatMon/Database/$name") -Raw).Replace('MixueDatMonDb', $Database)
    $fixturePath = Join-Path $work ('fixture-' + $name)
    $logPath = Join-Path $work ($name + '.log')
    Set-Content -LiteralPath $fixturePath -Value $sql -Encoding utf8
    & sqlcmd -S $SqlServer -E -b -f 65001 -i $fixturePath -o $logPath
    if ($LASTEXITCODE -ne 0) { throw "Fixture failed: $name; inspect $logPath before retrying." }
}
# Fixture-only settings: open all day, no automatic expiry, one known size for the smoke test.
& sqlcmd -S $SqlServer -E -b -d $Database -Q "UPDATE CaiDatCuaHang SET GioMo='00:00',GioDong='00:00',TamNgungNhanDon=0,SoPhutTuHuyDonMangDi=0; INSERT MonAnKichCo (MaMonAn,MaKichCo,Gia) SELECT 1,MaKichCo,25000 FROM KichCo WHERE TenKichCo=N'M';"
if ($LASTEXITCODE -ne 0) { throw 'Fixture seed failed.' }
Write-Output "Created isolated fixture: $Database. No existing database was changed or deleted."
