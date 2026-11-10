param([string]$Database = 'DuyCustomerTest_20261110', [string]$OutputPath = '.review-source/restart-replay-results.json')
$ErrorActionPreference = 'Stop'
if ($Database -notmatch '^DuyCustomerTest_[A-Za-z0-9_]+$') { throw 'Only an isolated customer test database is allowed.' }
$probe = [Net.Sockets.TcpClient]::new()
try { $probe.Connect('127.0.0.1', 5189); throw 'Test port 5189 is already in use.' }
catch [Net.Sockets.SocketException] { } finally { $probe.Dispose() }
$root = Split-Path $PSScriptRoot -Parent
$appRoot = Join-Path $root 'MixueDatMon'
$dll = Join-Path $appRoot 'bin/Debug/net8.0/MixueDatMon.dll'
if (-not (Test-Path $dll)) { throw 'Build the application before this test.' }
$dotnet = (Get-Command dotnet).Source
$env:DOTNET_ROLL_FORWARD = 'Major'
$env:DOTNET_CLI_TELEMETRY_OPTOUT = '1'
$env:ConnectionStrings__MacDinh = "Server=(localdb)\MSSQLLocalDB;Database=$Database;Trusted_Connection=True;TrustServerCertificate=True"
$env:ASPNETCORE_ENVIRONMENT = 'Development'
$env:Logging__LogLevel__Default = 'Warning'
$env:NganHang__MaBin = ''; $env:NganHang__SoTaiKhoan = ''
$base = 'http://127.0.0.1:5189'
$process = $null
function Start-TestServer {
    $script:process = Start-Process -FilePath $dotnet -ArgumentList @('"'+$dll+'"','--urls',$base) -WorkingDirectory $appRoot -WindowStyle Hidden -PassThru
    for ($attempt = 0; $attempt -lt 40; $attempt++) {
        if ($script:process.HasExited) { throw 'Owned test server exited before readiness.' }
        try { $null = Invoke-WebRequest "$base/" -TimeoutSec 2; return } catch { Start-Sleep -Milliseconds 250 }
    }
    throw 'Owned test server did not become ready.'
}
function Stop-TestServer {
    if ($script:process -and -not $script:process.HasExited) {
        Stop-Process -Id $script:process.Id
        $script:process.WaitForExit(5000) | Out-Null
    }
}
try {
    Start-TestServer
    $firstPid = $process.Id
    $session = [Microsoft.PowerShell.Commands.WebRequestSession]::new()
    $menu = Invoke-WebRequest "$base/ThucDon" -WebSession $session
    $token = [Net.WebUtility]::HtmlDecode([regex]::Match($menu.Content, 'name="__RequestVerificationToken"[^>]*value="([^"]+)"').Groups[1].Value)
    $body = @{ tenKhach = 'DUY_RESTART_REPLAY_TEST'; danhSachMon = @(@{ maMonAn = 24; soLuong = 1 }) } | ConvertTo-Json -Depth 4
    $ticket = Invoke-RestMethod "$base/DonHang/ChuanBiDatHang" -WebSession $session -Method Post -ContentType application/json -Headers @{ RequestVerificationToken = $token } -Body $body
    $headers = @{ RequestVerificationToken = $token; 'Checkout-Token' = $ticket.phieu }
    $first = Invoke-RestMethod "$base/DonHang/DatHang" -WebSession $session -Method Post -ContentType application/json -Headers $headers -Body $body
    Stop-TestServer
    Start-TestServer
    $second = Invoke-RestMethod "$base/DonHang/DatHang" -WebSession $session -Method Post -ContentType application/json -Headers $headers -Body $body
    if ($first.maTheoDoi -ne $second.maTheoDoi -or $process.Id -eq $firstPid) { throw 'Replay did not survive a new server process.' }
    $code = $first.maTheoDoi
    if ($code -notmatch '^[A-Z0-9-]+$') { throw 'Unexpected test tracking code' }
    $counts = ((& sqlcmd -S '(localdb)\MSSQLLocalDB' -E -b -d $Database -h -1 -Q "SET NOCOUNT ON; SELECT (SELECT COUNT(*) FROM DonHang WHERE MaTheoDoi='$code') AS Orders,(SELECT COUNT(*) FROM ChiTietDonHang c JOIN DonHang d ON d.MaDonHang=c.MaDonHang WHERE d.MaTheoDoi='$code') AS Lines FOR JSON PATH;") -join '') | ConvertFrom-Json
    if ($LASTEXITCODE -ne 0 -or $counts.Orders -ne 1 -or $counts.Lines -ne 1) { throw 'Restart replay created duplicate SQL records.' }
    [pscustomobject]@{ status = 'PASS'; sourceSha = (git -C $root rev-parse HEAD); checkedUtc = [DateTime]::UtcNow.ToString('o'); restartedProcess = $true; sameTrackingCode = $true; orders = $counts.Orders; lines = $counts.Lines; database = $Database } |
        ConvertTo-Json | Set-Content (Join-Path $root $OutputPath) -Encoding utf8
    Write-Output 'PASS: same checkout ticket after server restart maps to exactly one SQL order and one line.'
} finally { Stop-TestServer }
