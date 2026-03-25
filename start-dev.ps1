# script-dev.ps1
# ==================================================
# Script para desarrollo con hot reload y build automático
# ==================================================

# Obtener el directorio donde está este script
$scriptDir = $PSScriptRoot
if (-not $scriptDir) { $scriptDir = Get-Location }

# Colores para la consola
$host.UI.RawUI.ForegroundColor = "Cyan"

# Función para verificar si un puerto está en uso y matar el proceso
function Clear-PortIfBusy {
    param([int]$Port, [string]$ServiceName)
    $connections = Get-NetTCPConnection -LocalPort $Port -ErrorAction SilentlyContinue
    if ($connections) {
        $process = Get-Process -Id $connections.OwningProcess -ErrorAction SilentlyContinue
        Write-Host "Puerto $Port ($ServiceName) está ocupado por el proceso ID $($process.Id) - $($process.ProcessName). Matando..." -ForegroundColor Yellow
        Stop-Process -Id $process.Id -Force -ErrorAction SilentlyContinue
        Start-Sleep -Milliseconds 500
        Write-Host "Proceso terminado." -ForegroundColor Green
    } else {
        Write-Host "Puerto $Port ($ServiceName) libre." -ForegroundColor Green
    }
}

# Limpiar puertos antes de empezar
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "VERIFICANDO Y LIBERANDO PUERTOS" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan

$backendPort = 5298
$frontendPort = 3000

Clear-PortIfBusy -Port $backendPort -ServiceName "Backend"
Clear-PortIfBusy -Port $frontendPort -ServiceName "Frontend"

# Verificar que npm está disponible
$npmExists = Get-Command npm -ErrorAction SilentlyContinue
if (-not $npmExists) {
    Write-Host "ERROR: No se encuentra 'npm' en el PATH. Asegúrate de tener Node.js instalado y agregado al PATH." -ForegroundColor Red
    exit 1
}
Write-Host "npm encontrado: $($npmExists.Source)" -ForegroundColor Green

# Verificar que el directorio Frontend existe y tiene package.json
$frontendDir = "$scriptDir\Frontend"
if (-not (Test-Path $frontendDir)) {
    Write-Host "ERROR: No se encuentra la carpeta Frontend en: $frontendDir" -ForegroundColor Red
    exit 1
}

if (-not (Test-Path "$frontendDir\package.json")) {
    Write-Host "ERROR: No se encuentra package.json en $frontendDir" -ForegroundColor Red
    exit 1
}
Write-Host "Frontend encontrado." -ForegroundColor Green

# Construir el backend antes de ejecutar
Write-Host "`n========================================" -ForegroundColor Cyan
Write-Host "CONSTRUYENDO BACKEND (dotnet build)" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Push-Location "$scriptDir\API"
try {
    dotnet build
    if ($LASTEXITCODE -ne 0) {
        Write-Host "ERROR: Falló la compilación del backend." -ForegroundColor Red
        Pop-Location
        exit 1
    }
    Write-Host "Build completado correctamente." -ForegroundColor Green
}
finally {
    Pop-Location
}

# Lanzar backend en nueva ventana
Write-Host "`n========================================" -ForegroundColor Cyan
Write-Host "INICIANDO BACKEND (dotnet watch run)" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
$backendProcess = Start-Process -FilePath "dotnet" `
    -ArgumentList "watch run" `
    -WorkingDirectory "$scriptDir\API" `
    -WindowStyle Normal `
    -PassThru

Write-Host "Backend iniciado en nueva ventana (puerto $backendPort)" -ForegroundColor Green

# Lanzar frontend en nueva ventana, con cmd /k para que no se cierre si falla
Write-Host "`n========================================" -ForegroundColor Cyan
Write-Host "INICIANDO FRONTEND (npm run dev)" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan

$frontendProcess = Start-Process -FilePath "cmd" `
    -ArgumentList "/k `"cd /d `"$frontendDir`" && npm run dev`"" `
    -WindowStyle Normal `
    -PassThru

Write-Host "Frontend iniciado en nueva ventana (puerto $frontendPort)" -ForegroundColor Green

# Mostrar información final
Write-Host "`n========================================" -ForegroundColor Cyan
Write-Host "🚀 SISTEMA DE RECLAMOS INICIADO" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Backend API:  https://localhost:$backendPort" -ForegroundColor Yellow
Write-Host "Frontend:     http://localhost:$frontendPort" -ForegroundColor Yellow
Write-Host "`nModo desarrollo activo: los cambios se reflejarán automáticamente." -ForegroundColor Cyan
Write-Host "Los logs de cada servicio se muestran en sus propias ventanas." -ForegroundColor Cyan
Write-Host "Presiona ENTER para detener todos los servicios y salir." -ForegroundColor Red
Write-Host "========================================" -ForegroundColor Cyan

# Esperar a que el usuario presione Enter
Read-Host

# Detener procesos
Write-Host "`nDeteniendo servicios..." -ForegroundColor Red

if ($backendProcess -and (-not $backendProcess.HasExited)) {
    Stop-Process -Id $backendProcess.Id -Force -ErrorAction SilentlyContinue
    Write-Host "Backend detenido." -ForegroundColor Green
}
if ($frontendProcess -and (-not $frontendProcess.HasExited)) {
    Stop-Process -Id $frontendProcess.Id -Force -ErrorAction SilentlyContinue
    Write-Host "Frontend detenido." -ForegroundColor Green
}

# Esperar un momento para que los procesos terminen
Start-Sleep -Milliseconds 500

# Limpiar puertos nuevamente para asegurar que todo esté liberado
Write-Host "`nLiberando puertos residuales..." -ForegroundColor Yellow
Clear-PortIfBusy -Port $backendPort -ServiceName "Backend"
Clear-PortIfBusy -Port $frontendPort -ServiceName "Frontend"

Write-Host "`n✅ Todos los servicios han sido detenidos correctamente." -ForegroundColor Green