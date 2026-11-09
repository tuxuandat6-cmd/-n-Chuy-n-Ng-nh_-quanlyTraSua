param(
    [string]$BaseUrl = 'http://127.0.0.1:5187',
    [string]$SqlServer = '(localdb)\MSSQLLocalDB',
    [string]$Database = 'DuyCustomerTest_20261109',
    [string]$OutputPath = '.review-source/customer-http-results.json'
)
$ErrorActionPreference = 'Stop'
if ($BaseUrl -notmatch '^http://(127\.0\.0\.1|localhost):\d+$' -or $Database -notmatch '^DuyCustomerTest_[A-Za-z0-9_]+$') {
    throw 'Only a loopback app and a named DuyCustomerTest_ database are supported.'
}
$results = [Collections.Generic.List[object]]::new()
function Check([string]$Name, [bool]$Passed) {
    $results.Add([pscustomobject]@{ test = $Name; status = $(if ($Passed) { 'PASS' } else { 'FAIL' }) })
    if (-not $Passed) { throw "Failed: $Name" }
}
try {
    $session = [Microsoft.PowerShell.Commands.WebRequestSession]::new()
    $homepageResponse = Invoke-WebRequest "$BaseUrl/" -WebSession $session
    Check 'Homepage reads SQL-backed featured menu' ($homepageResponse.StatusCode -eq 200 -and $homepageResponse.Content.Contains('Món bán chạy nhất'))
    $menu = Invoke-WebRequest "$BaseUrl/ThucDon" -WebSession $session
    Check 'Menu and checkout form render' ($menu.StatusCode -eq 200 -and $menu.Content.Contains('id="menu-data"'))
    $match = [regex]::Match($menu.Content, 'name="__RequestVerificationToken"[^>]*value="([^"]+)"')
    if (-not $match.Success) { throw 'Antiforgery token missing' }
    $token = [Net.WebUtility]::HtmlDecode($match.Groups[1].Value)
    $body = @{
        tenKhach = ('DUY_HTTP_TEST_' + [DateTime]::UtcNow.ToString('HHmmss'))
        soDienThoai = '0900000000'
        danhSachMon = @(@{ maMonAn = 1; maKichCo = 1; soLuong = 2; maMucDa = 2; maMucDuong = 3; danhSachMaTopping = @(1) })
    } | ConvertTo-Json -Depth 5
    $withoutToken = Invoke-WebRequest "$BaseUrl/DonHang/DatHang" -Method Post -WebSession $session -ContentType 'application/json' -Body $body -SkipHttpErrorCheck
    Check 'Checkout without antiforgery token is rejected' ($withoutToken.StatusCode -eq 400)
    $saved = Invoke-RestMethod "$BaseUrl/DonHang/DatHang" -Method Post -WebSession $session -ContentType 'application/json' -Headers @{ RequestVerificationToken = $token } -Body $body
    Check 'Checkout succeeds against actual SQL Server' ([bool]$saved.thanhCong)
    $code = $saved.maTheoDoi
    if ($code -notmatch '^[A-Z0-9-]+$') { throw 'Unexpected tracking code format' }
    $query = "SET NOCOUNT ON; SELECT c.SoLuong,c.DonGia,c.MaKichCo,c.MaMucDa,c.MaMucDuong,t.DonGia AS ToppingGia,v.TongTien FROM DonHang d JOIN ChiTietDonHang c ON c.MaDonHang=d.MaDonHang JOIN ChiTietTopping t ON t.MaChiTiet=c.MaChiTiet JOIN vw_TongTienDonHang v ON v.MaDonHang=d.MaDonHang WHERE d.MaTheoDoi='$code' FOR JSON PATH;"
    $raw = & sqlcmd -S $SqlServer -E -b -d $Database -h -1 -w 65535 -Q $query
    if ($LASTEXITCODE -ne 0) { throw 'SQL verification failed' }
    $record = ($raw -join '') | ConvertFrom-Json
    Check 'SQL persists quantity, authoritative price, size, ice, sugar and topping' ($record.Count -eq 1 -and $record.SoLuong -eq 2 -and $record.DonGia -eq 25000 -and $record.MaKichCo -eq 1 -and $record.MaMucDa -eq 2 -and $record.MaMucDuong -eq 3 -and $record.ToppingGia -eq 3000 -and $record.TongTien -eq 56000)
    $tracking = Invoke-WebRequest "$BaseUrl/DonHang/TheoDoi?ma=$code" -WebSession $session
    Check 'Saved order renders on tracking page' ($tracking.StatusCode -eq 200 -and $tracking.Content.Contains($code))
    $history = Invoke-WebRequest "$BaseUrl/DonHang/DonCuaToi" -WebSession $session
    Check 'Ordering browser session sees its order in history' ($history.Content.Contains($code))
    Check 'History total includes topping prices' ([Net.WebUtility]::HtmlDecode($history.Content).Contains('56.000'))
    $outsider = Invoke-WebRequest "$BaseUrl/DonHang/DonCuaToi"
    Check 'A fresh browser session does not list the saved order' (-not $outsider.Content.Contains($code))
    # Guest tracking links are bearer links in the existing design, not account authentication.
    $state = Invoke-RestMethod "$BaseUrl/DonHang/LayTrangThai?ma=$code"
    Check 'Status endpoint reports pending and unpaid' ($state.maTrangThai -eq 1 -and -not $state.daThanhToan)
    $missing = Invoke-WebRequest "$BaseUrl/DonHang/LayTrangThai?ma=NONEXISTENT" -SkipHttpErrorCheck
    Check 'Unknown tracking code returns 404' ($missing.StatusCode -eq 404)
    [pscustomobject]@{ checkedUtc = [DateTime]::UtcNow.ToString('o'); database = $Database; trackingCode = $code; tests = $results; sqlRecord = $record } |
        ConvertTo-Json -Depth 6 | Set-Content -LiteralPath $OutputPath -Encoding utf8
    $results | Format-Table -AutoSize
} catch {
    [pscustomobject]@{ checkedUtc = [DateTime]::UtcNow.ToString('o'); tests = $results; error = $_.Exception.Message } |
        ConvertTo-Json -Depth 5 | Set-Content -LiteralPath $OutputPath -Encoding utf8
    throw
}
