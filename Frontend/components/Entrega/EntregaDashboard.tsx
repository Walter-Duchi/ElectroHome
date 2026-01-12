import React, { useState, useEffect } from 'react';
import {
  Box,
  TextField,
  Button,
  Card,
  CardContent,
  Typography,
  Alert,
  Grid,
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TableRow,
  Paper,
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
  LinearProgress,
  Chip,
  IconButton,
  Tooltip,
  Stepper,
  Step,
  StepLabel,
  StepContent,
  CircularProgress,
  Snackbar,
} from '@mui/material';
import {
  Search,
  CheckCircle,
  Error,
  Warning,
  Info,
  Assignment,
  Download,
  Upload,
  Check,
  Close,
  Edit,
  Visibility,
} from '@mui/icons-material';
import { entregaService } from '../../services/entregaService';
import { type BuscarReclamoResponse, type ProductoEntregaDTO } from '../../src/types/entrega';

const EntregaDashboard: React.FC = () => {
  const [codigoReclamo, setCodigoReclamo] = useState('');
  const [reclamo, setReclamo] = useState<BuscarReclamoResponse | null>(null);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [success, setSuccess] = useState<string | null>(null);
  const [activeStep, setActiveStep] = useState(0);
  const [selectedProduct, setSelectedProduct] = useState<ProductoEntregaDTO | null>(null);
  const [reemplazoInput, setReemplazoInput] = useState('');
  const [validating, setValidating] = useState(false);
  const [validationResult, setValidationResult] = useState<any>(null);
  const [dialogOpen, setDialogOpen] = useState(false);
  const [pdfGenerating, setPdfGenerating] = useState(false);
  const [uploading, setUploading] = useState(false);
  const [confirming, setConfirming] = useState(false);
  const [asignando, setAsignando] = useState(false);
  const [selectedFile, setSelectedFile] = useState<File | null>(null);
  const [pdfUrl, setPdfUrl] = useState<string | null>(null);
  const [fileBase64, setFileBase64] = useState<string>('');


  const steps = [
    'Buscar Reclamo',
    'Asignar Reemplazos',
    'Generar Comprobante',
    'Subir Comprobante Firmado',
    'Confirmar Entrega'
  ];

  const handleBuscarReclamo = async () => {
    if (!codigoReclamo.trim()) {
      setError('Ingrese un código de reclamo');
      return;
    }

    setLoading(true);
    setError(null);
    setReclamo(null);
    setActiveStep(0);
    setPdfUrl(null);
    setSelectedFile(null);
    setFileBase64('');

    try {
      const response = await entregaService.buscarReclamo(codigoReclamo);

      if (response.exito) {
        // VERIFICACIÓN CRÍTICA: Si no hay productos para entregar, NO avanzar
        if (!response.productos || response.productos.length === 0) {
          setError('No hay productos para entregar en este reclamo. Los productos deben estar aprobados y con forma de compensación "Reemplazo".');
          setReclamo(response);
          return;
        }

        setReclamo(response);
        setActiveStep(1);
        setSuccess('Reclamo encontrado exitosamente');

        // Verificar si todos los productos ya tienen reemplazo
        const todosTienenReemplazo = response.productos.every(p => p.reemplazoValido);
        if (todosTienenReemplazo) {
          setActiveStep(2);
        }
      } else {
        setError(response.mensaje);
      }
    } catch (err: any) {
      setError(err.response?.data?.detail || 'Error al buscar el reclamo');
    } finally {
      setLoading(false);
    }
  };

  const handleOpenDialog = (producto: ProductoEntregaDTO) => {
    setSelectedProduct(producto);
    setReemplazoInput(producto.numeroSerieReemplazo || '');
    setValidationResult(null);
    setDialogOpen(true);
  };

  const handleValidarReemplazo = async () => {
    if (!selectedProduct || !reemplazoInput.trim()) {
      setError('Ingrese un número de serie para validar');
      return;
    }

    setValidating(true);
    setValidationResult(null);

    try {
      const response = await entregaService.validarReemplazo(
        selectedProduct.reclamoProductoSnId,
        reemplazoInput
      );

      setValidationResult(response);

      if (response.valido) {
        setSuccess('Producto válido. Ahora puede asignarlo.');
      } else {
        setError(response.mensaje);
      }
    } catch (err: any) {
      setError(err.response?.data?.detail || 'Error al validar el reemplazo');
    } finally {
      setValidating(false);
    }
  };

  const handleAsignarReemplazo = async () => {
    if (!selectedProduct || !reemplazoInput.trim() || !validationResult?.valido) {
      setError('Primero debe validar un producto válido');
      return;
    }

    setAsignando(true);
    try {
      await entregaService.seleccionarReemplazo(
        selectedProduct.reclamoProductoSnId,
        reemplazoInput
      );

      // Actualizar la lista de productos
      const response = await entregaService.buscarReclamo(codigoReclamo);
      if (response.exito) {
        setReclamo(response);
        setSuccess('Reemplazo asignado exitosamente');
        setDialogOpen(false);

        // Verificar si todos los productos ya tienen reemplazo
        const todosTienenReemplazo = response.productos.every(p => p.reemplazoValido);
        if (todosTienenReemplazo) {
          setActiveStep(2);
        }
      } else {
        setError(response.mensaje);
      }
    } catch (err: any) {
      setError(err.response?.data?.detail || 'Error al asignar el reemplazo');
    } finally {
      setAsignando(false);
    }
  };

  const handleGenerarComprobante = async () => {
    if (!reclamo) return;

    setPdfGenerating(true);
    try {
      // Primero obtener los datos del comprobante
      const datosComprobante = await entregaService.generarDatosComprobante(codigoReclamo);

      // Luego generar el PDF
      const { rutaPdf } = await entregaService.generarPdfComprobante(datosComprobante);

      // Crear URL para descargar/ver el PDF
      const fullUrl = `http://localhost:5298${rutaPdf}`;
      setPdfUrl(fullUrl);

      // Abrir el PDF en una nueva pestaña
      window.open(fullUrl, '_blank');

      setActiveStep(3);
      setSuccess('Comprobante generado exitosamente. Ahora puede imprimirlo y hacerlo firmar por el cliente.');
    } catch (err: any) {
      setError(err.response?.data?.detail || 'Error al generar el comprobante');
    } finally {
      setPdfGenerating(false);
    }
  };

  const handleFileChange = (event: React.ChangeEvent<HTMLInputElement>) => {
    const file = event.target.files?.[0];
    if (file) {
      if (file.type !== 'application/pdf') {
        setError('Por favor, seleccione un archivo PDF');
        return;
      }

      if (file.size > 10 * 1024 * 1024) { // 10MB límite
        setError('El archivo es demasiado grande. Máximo 10MB.');
        return;
      }

      setSelectedFile(file);

      // Convertir a base64
      const reader = new FileReader();
      reader.onload = (e) => {
        const base64 = e.target?.result as string;
        // Remover el prefijo (data:application/pdf;base64,)
        const base64Content = base64.split(',')[1];
        setFileBase64(base64Content);
      };
      reader.readAsDataURL(file);
    }
  };

  const handleSubirComprobante = async () => {
    if (!reclamo || !fileBase64) {
      setError('Por favor, seleccione un archivo PDF firmado');
      return;
    }

    setUploading(true);
    try {
      await entregaService.subirComprobante(codigoReclamo, fileBase64);

      setActiveStep(4);
      setSuccess('Comprobante firmado subido exitosamente');
      setSelectedFile(null);
      setFileBase64('');
    } catch (err: any) {
      setError(err.response?.data?.detail || 'Error al subir el comprobante');
    } finally {
      setUploading(false);
    }
  };

  const handleConfirmarEntrega = async () => {
    if (!reclamo) return;

    setConfirming(true);
    try {
      await entregaService.confirmarEntrega(codigoReclamo);

      setSuccess('Entrega confirmada exitosamente');

      // Reiniciar el estado
      setTimeout(() => {
        setReclamo(null);
        setCodigoReclamo('');
        setActiveStep(0);
        setPdfUrl(null);
        setSelectedFile(null);
        setFileBase64('');
      }, 2000);
    } catch (err: any) {
      setError(err.response?.data?.detail || 'Error al confirmar la entrega');
    } finally {
      setConfirming(false);
    }
  };

  const getStepContent = (step: number) => {
    switch (step) {
      case 0:
        return (
          <Box>
            <Typography variant="body1" paragraph>
              Ingrese el código del reclamo que desea procesar para entrega.
            </Typography>
            <Grid container spacing={2} alignItems="center">
              <Grid size={{ xs: 12, sm: 8 }}>
                <TextField
                  label="Código de Reclamo"
                  value={codigoReclamo}
                  onChange={(e) => setCodigoReclamo(e.target.value)}
                  fullWidth
                  disabled={loading}
                  placeholder="Ej: REC-ENTREGA-001"
                  onKeyPress={(e) => {
                    if (e.key === 'Enter') {
                      handleBuscarReclamo();
                    }
                  }}
                />
              </Grid>
              <Grid size={{ xs: 12, sm: 4 }}>
                <Button
                  variant="contained"
                  onClick={handleBuscarReclamo}
                  disabled={loading || !codigoReclamo.trim()}
                  fullWidth
                  startIcon={<Search />}
                >
                  {loading ? 'Buscando...' : 'Buscar Reclamo'}
                </Button>
              </Grid>
            </Grid>
          </Box>
        );
      case 1:
        return (
          <Box>
            <Typography variant="body1" paragraph>
              Asigne un producto de reemplazo para cada producto defectuoso.
              El producto de reemplazo debe ser de la misma marca y modelo,
              y estar en estado "Se_Puede_Vender".
            </Typography>

            {reclamo && !reclamo.todosProductosRevisados && (
              <Alert severity="warning" sx={{ mb: 3 }}>
                <Typography variant="subtitle2">
                  Atención: No todos los productos del reclamo han sido revisados
                </Typography>
                <Typography variant="body2">
                  Productos pendientes de revisión: {reclamo.productosPendientesRevision} de {reclamo.totalProductosReclamo}
                </Typography>
              </Alert>
            )}

            <TableContainer component={Paper}>
              <Table>
                <TableHead>
                  <TableRow>
                    <TableCell>Producto Defectuoso</TableCell>
                    <TableCell>Marca/Modelo</TableCell>
                    <TableCell>Estado</TableCell>
                    <TableCell>Reemplazo Asignado</TableCell>
                    <TableCell>Validación</TableCell>
                    <TableCell>Acciones</TableCell>
                  </TableRow>
                </TableHead>
                <TableBody>
                  {reclamo?.productos.map((producto: ProductoEntregaDTO) => (
                    <TableRow key={producto.reclamoProductoSnId}>
                      <TableCell>
                        <Typography variant="body2" fontWeight="medium">
                          {producto.numeroSerieProductoDefectuoso}
                        </Typography>
                      </TableCell>
                      <TableCell>
                        <Typography variant="body2">{producto.marca}</Typography>
                        <Typography variant="caption" color="text.secondary">
                          {producto.modelo}
                        </Typography>
                      </TableCell>
                      <TableCell>
                        <Chip
                          label={producto.estado}
                          size="small"
                          color="primary"
                          variant="outlined"
                        />
                      </TableCell>
                      <TableCell>
                        {producto.numeroSerieReemplazo ? (
                          <Chip
                            label={producto.numeroSerieReemplazo}
                            size="small"
                            color="success"
                            icon={<CheckCircle />}
                          />
                        ) : (
                          <Chip
                            label="Sin asignar"
                            size="small"
                            color="error"
                            icon={<Error />}
                          />
                        )}
                      </TableCell>
                      <TableCell>
                        <Typography
                          variant="caption"
                          color={producto.reemplazoValido ? "success.main" : "warning.main"}
                        >
                          {producto.mensajeValidacion}
                        </Typography>
                      </TableCell>
                      <TableCell>
                        <Tooltip title="Asignar reemplazo">
                          <IconButton
                            size="small"
                            onClick={() => handleOpenDialog(producto)}
                            color={producto.reemplazoValido ? "success" : "primary"}
                          >
                            {producto.reemplazoValido ? <Edit /> : <Assignment />}
                          </IconButton>
                        </Tooltip>
                      </TableCell>
                    </TableRow>
                  ))}
                </TableBody>
              </Table>
            </TableContainer>

            <Box sx={{ mt: 3, display: 'flex', justifyContent: 'space-between' }}>
              <Button
                variant="outlined"
                onClick={() => setActiveStep(0)}
              >
                Volver
              </Button>
              <Button
                variant="contained"
                onClick={() => {
                  const todosTienenReemplazo = reclamo?.productos.every((p: { reemplazoValido: any; }) => p.reemplazoValido);
                  if (todosTienenReemplazo) {
                    setActiveStep(2);
                  } else {
                    setError('Todos los productos deben tener un reemplazo asignado');
                  }
                }}
                disabled={!reclamo?.productos.every((p: { reemplazoValido: any; }) => p.reemplazoValido)}
              >
                Continuar
              </Button>
            </Box>
          </Box>
        );
      case 2:
        return (
          <Box>
            <Typography variant="body1" paragraph>
              Genere el comprobante de entrega que será firmado por el cliente.
              Este comprobante incluye todos los productos defectuosos y sus reemplazos.
            </Typography>

            <Card sx={{ mb: 3 }}>
              <CardContent>
                <Typography variant="h6" gutterBottom>
                  Resumen del Comprobante
                </Typography>
                <Grid container spacing={2}>
                  <Grid size={{ xs: 12, sm: 6 }}>
                    <Typography variant="body2" color="text.secondary">
                      Cliente
                    </Typography>
                    <Typography variant="body1">{reclamo?.cliente}</Typography>
                  </Grid>
                  <Grid size={{ xs: 12, sm: 6 }}>
                    <Typography variant="body2" color="text.secondary">
                      RUC
                    </Typography>
                    <Typography variant="body1">{reclamo?.ruc}</Typography>
                  </Grid>
                  <Grid size={{ xs: 12 }}>
                    <Typography variant="body2" color="text.secondary">
                      Productos a Entregar
                    </Typography>
                    <Typography variant="body1">{reclamo?.productos.length} productos</Typography>
                  </Grid>
                </Grid>
              </CardContent>
            </Card>

            {pdfUrl && (
              <Alert severity="success" sx={{ mb: 2 }}>
                <Typography variant="body2">
                  Comprobante generado:
                  <Button
                    size="small"
                    href={pdfUrl}
                    target="_blank"
                    sx={{ ml: 1 }}
                  >
                    Ver/Descargar PDF
                  </Button>
                </Typography>
              </Alert>
            )}

            <Box sx={{ display: 'flex', justifyContent: 'space-between', mt: 3 }}>
              <Button
                variant="outlined"
                onClick={() => setActiveStep(1)}
              >
                Volver
              </Button>
              <Button
                variant="contained"
                onClick={handleGenerarComprobante}
                disabled={pdfGenerating}
                startIcon={pdfGenerating ? <CircularProgress size={20} /> : <Download />}
              >
                {pdfGenerating ? 'Generando...' : 'Generar Comprobante PDF'}
              </Button>
            </Box>
          </Box>
        );
      case 3:
        return (
          <Box>
            <Typography variant="body1" paragraph>
              Suba el comprobante firmado por el cliente.
              Asegúrese de que el comprobante esté completamente firmado antes de continuar.
            </Typography>

            <Alert severity="info" sx={{ mb: 3 }}>
              <Typography variant="body2">
                1. Imprima el comprobante generado<br />
                2. Haga firmar el comprobante al cliente<br />
                3. Escanee el comprobante firmado<br />
                4. Suba el archivo escaneado
              </Typography>
            </Alert>

            <input
              accept="application/pdf"
              style={{ display: 'none' }}
              id="upload-pdf"
              type="file"
              onChange={handleFileChange}
            />
            <label htmlFor="upload-pdf">
              <Button
                variant="contained"
                component="span"
                startIcon={<Upload />}
                sx={{ mb: 2, mr: 2 }}
              >
                Seleccionar PDF Firmado
              </Button>
            </label>

            {selectedFile && (
              <Alert severity="success" sx={{ mb: 2 }}>
                <Typography variant="body2">
                  Archivo seleccionado: {selectedFile.name} ({Math.round(selectedFile.size / 1024)} KB)
                </Typography>
              </Alert>
            )}

            <Box sx={{ display: 'flex', justifyContent: 'space-between', mt: 3 }}>
              <Button
                variant="outlined"
                onClick={() => setActiveStep(2)}
              >
                Volver
              </Button>
              <Button
                variant="contained"
                onClick={handleSubirComprobante}
                disabled={uploading || !selectedFile}
                startIcon={uploading ? <CircularProgress size={20} /> : <Upload />}
              >
                {uploading ? 'Subiendo...' : 'Subir Comprobante Firmado'}
              </Button>
            </Box>
          </Box>
        );
      case 4:
        return (
          <Box>
            <Typography variant="body1" paragraph>
              Confirme la entrega de los productos. Esta acción cambiará el estado
              de los productos a "Compensado" y registrará la entrega en el sistema.
            </Typography>

            <Alert severity="warning" sx={{ mb: 3 }}>
              <Typography variant="subtitle2">
                Confirmación Final
              </Typography>
              <Typography variant="body2">
                Al confirmar la entrega:<br />
                • Los productos defectuosos cambiarán a estado "Compensado"<br />
                • Los productos de reemplazo cambiarán a estado "Entregado_Como_Reemplazo_Al_Cliente"<br />
                • Se registrará la entrega en el historial del sistema
              </Typography>
            </Alert>

            <Box sx={{ display: 'flex', justifyContent: 'space-between', mt: 3 }}>
              <Button
                variant="outlined"
                onClick={() => setActiveStep(3)}
              >
                Volver
              </Button>
              <Button
                variant="contained"
                color="success"
                onClick={handleConfirmarEntrega}
                disabled={confirming}
                startIcon={confirming ? <CircularProgress size={20} /> : <Check />}
              >
                {confirming ? 'Confirmando...' : 'Confirmar Entrega'}
              </Button>
            </Box>
          </Box>
        );
      default:
        return null;
    }
  };

  return (
    <Box>
      <Typography variant="h4" gutterBottom>
        Módulo de Personal de Entrega
      </Typography>

      <Card sx={{ mb: 3 }}>
        <CardContent>
          <Stepper activeStep={activeStep} orientation="vertical">
            {steps.map((label, index) => (
              <Step key={label}>
                <StepLabel>{label}</StepLabel>
                <StepContent>
                  {getStepContent(index)}
                </StepContent>
              </Step>
            ))}
          </Stepper>
        </CardContent>
      </Card>

      {error && (
        <Alert
          severity="error"
          sx={{ mb: 2 }}
          onClose={() => setError(null)}
        >
          {error}
        </Alert>
      )}

      {success && (
        <Alert
          severity="success"
          sx={{ mb: 2 }}
          onClose={() => setSuccess(null)}
        >
          {success}
        </Alert>
      )}

      {/* Dialog para asignar reemplazo */}
      <Dialog
        open={dialogOpen}
        onClose={() => setDialogOpen(false)}
        maxWidth="sm"
        fullWidth
      >
        <DialogTitle>
          Asignar Producto de Reemplazo
        </DialogTitle>
        <DialogContent>
          {selectedProduct && (
            <Box sx={{ mt: 2 }}>
              <Typography variant="subtitle2" gutterBottom>
                Producto Defectuoso
              </Typography>
              <Typography variant="body2" color="text.secondary">
                {selectedProduct.marca} {selectedProduct.modelo} - {selectedProduct.numeroSerieProductoDefectuoso}
              </Typography>

              <TextField
                label="Número de Serie del Reemplazo"
                value={reemplazoInput}
                onChange={(e) => setReemplazoInput(e.target.value)}
                fullWidth
                sx={{ mt: 3 }}
                disabled={validating}
                placeholder="Ej: SM-S911BZKD-REP01"
              />

              {validationResult && (
                <Alert
                  severity={validationResult.valido ? "success" : "error"}
                  sx={{ mt: 2 }}
                >
                  {validationResult.mensaje}
                  {validationResult.valido && validationResult.productoReemplazo && (
                    <Typography variant="body2" sx={{ mt: 1 }}>
                      Marca: {validationResult.productoReemplazo.marca}<br />
                      Modelo: {validationResult.productoReemplazo.modelo}<br />
                      Estado: {validationResult.productoReemplazo.estadoInventario}
                    </Typography>
                  )}
                </Alert>
              )}
            </Box>
          )}
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setDialogOpen(false)}>
            Cancelar
          </Button>
          <Button
            onClick={handleValidarReemplazo}
            disabled={validating || !reemplazoInput.trim()}
            startIcon={validating ? <CircularProgress size={20} /> : <Check />}
          >
            {validating ? 'Validando...' : 'Validar'}
          </Button>
          <Button
            onClick={handleAsignarReemplazo}
            disabled={!validationResult?.valido || asignando}
            startIcon={asignando ? <CircularProgress size={20} /> : <Assignment />}
            color="success"
          >
            {asignando ? 'Asignando...' : 'Asignar'}
          </Button>
        </DialogActions>
      </Dialog>
    </Box>
  );
};

export default EntregaDashboard;
