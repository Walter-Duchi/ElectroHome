Write-Host "========================================" -ForegroundColor Cyan
Write-Host "INICIANDO SISTEMA DE RECLAMOS (WATCH MODE)" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan

Write-Host "`n[1/2] Iniciando Backend (dotnet watch)..." -ForegroundColor Green

$backendJob = Start-Job -ScriptBlock {
    Set-Location "$using:PSScriptRoot\API"
    dotnet watch run
}

Write-Host "`n[2/2] Iniciando Frontend (hot reload)..." -ForegroundColor Green

$frontendJob = Start-Job -ScriptBlock {
    Set-Location "$using:PSScriptRoot\Frontend"
    npm run dev
}

Write-Host "`n========================================" -ForegroundColor Cyan
Write-Host "🚀 SISTEMA INICIADO INMEDIATAMENTE" -ForegroundColor Green
Write-Host "`nURLs:" -ForegroundColor White
Write-Host "• Backend API:  http://localhost:5298" -ForegroundColor Yellow
Write-Host "• Frontend:     http://localhost:3000" -ForegroundColor Yellow
Write-Host "`nModo desarrollo activo: cambios en tiempo real." -ForegroundColor Cyan
Write-Host "Presiona ENTER para detener todo." -ForegroundColor Red
Write-Host "========================================" -ForegroundColor Cyan

Read-Host

Write-Host "`nDeteniendo servicios..." -ForegroundColor Red
Stop-Job $backendJob
Stop-Job $frontendJob
Remove-Job $backendJob
Remove-Job $frontendJob

Write-Host "`n✅ Servicios detenidos correctamente" -ForegroundColor Green
