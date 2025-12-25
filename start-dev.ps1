# start-simple.ps1
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "INICIANDO SISTEMA DE RECLAMOS" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan

Write-Host "`n[1/2] Iniciando Backend (API)..." -ForegroundColor Green

# Iniciar backend en nueva ventana
$backendJob = Start-Job -ScriptBlock {
    Set-Location "$using:PSScriptRoot\API"
    dotnet run
}

Write-Host "Esperando 10 segundos para que el backend inicie..." -ForegroundColor Yellow
Start-Sleep -Seconds 10

Write-Host "`n[2/2] Iniciando Frontend..." -ForegroundColor Green

# Iniciar frontend en nueva ventana
$frontendJob = Start-Job -ScriptBlock {
    Set-Location "$using:PSScriptRoot\Frontend"
    npm run dev
}

Write-Host "`n========================================" -ForegroundColor Cyan
Write-Host "✅ AMBOS PROYECTOS INICIADOS" -ForegroundColor Green
Write-Host "`nURLs de acceso:" -ForegroundColor White
Write-Host "• Backend API:  http://localhost:5298" -ForegroundColor Yellow
Write-Host "• Frontend:     http://localhost:5173" -ForegroundColor Yellow
Write-Host "`n========================================" -ForegroundColor Cyan
Write-Host "Presiona ENTER para detener ambos servicios..." -ForegroundColor Red

# Esperar a que el usuario presione ENTER
Read-Host

# Detener ambos servicios
Write-Host "`nDeteniendo servicios..." -ForegroundColor Red
Stop-Job $backendJob
Stop-Job $frontendJob
Remove-Job $backendJob
Remove-Job $frontendJob

Write-Host "`n✅ Servicios detenidos" -ForegroundColor Green