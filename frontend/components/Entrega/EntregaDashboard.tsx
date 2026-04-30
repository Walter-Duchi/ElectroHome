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
  Chip,
  Stepper,
  Step,
  StepLabel,
  StepContent,
  CircularProgress,
  Tabs,
  Tab,
} from '@mui/material';
import {
  Search,
  CheckCircle,
  Error as ErrorIcon,
  Download,
  Upload,
  Check,
} from '@mui/icons-material';
import { entregaService } from '../../services/entregaService';
import { type BuscarReclamoResponse, type ProductoEntregaDTO, type ReclamoPendienteEntregaDTO } from '../../src/types/entrega';

const EntregaDashboard: React.FC = () => {
  const [codigoReclamo, setCodigoReclamo] = useState('');
  const [reclamo, setReclamo] = useState<BuscarReclamoResponse | null>(null);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [success, setSuccess] = useState<string | null>(null);
  const [activeStep, setActiveStep] = useState(0);
  const [pdfGenerating, setPdfGenerating] = useState(false);
  const [uploading, setUploading] = useState(false);
  const [confirming, setConfirming] = useState(false);
  const [selectedFile, setSelectedFile] = useState<File | null>(null);
  const [pdfUrl, setPdfUrl] = useState<string | null>(null);
  const [fileBase64, setFileBase64] = useState<string>('');
  const [reclamosPendientes, setReclamosPendientes] = useState<ReclamoPendienteEntregaDTO[]>([]);
  const [cargandoPendientes, setCargandoPendientes] = useState(false);
  const [tabBusqueda, setTabBusqueda] = useState(0); // 0 = código, 1 = lista
  const [asignandoAutomatico, setAsignandoAutomatico] = useState(false);

  const steps = [
    'Buscar Reclamo',
    'Verificar Reemplazos Asignados',
    'Generar Comprobante',
    'Subir Comprobante Firmado',
    'Confirmar Entrega'
  ];

  // Cargar reclamos pendientes al montar el componente
  useEffect(() => {
    cargarReclamosPendientes();
  }, []);

  const cargarReclamosPendientes = async () => {
    setCargandoPendientes(true);
    try {
      const pendientes = await entregaService.obtenerReclamosPendientes();
      setReclamosPendientes(pendientes);
    } catch (err: any) {
      console.error('Error cargando reclamos pendientes:', err);
    } finally {
      setCargandoPendientes(false);
    }
  };

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
        if (!response.productos || response.productos.length === 0) {
          setError('No hay productos para entregar en este reclamo. Los productos deben estar aprobados y con forma de compensación "Reemplazo".');
          setReclamo(response);
          return;
        }

        // Asignar reemplazos automáticamente
        await asignarReemplazosYContinuar(codigoReclamo, response);
      } else {
        setError(response.mensaje);
      }
    } catch (err: any) {
      setError(err.response?.data?.detail || 'Error al buscar el reclamo');
    } finally {
      setLoading(false);
    }
  };

  const handleSeleccionarReclamoPendiente = async (codigo: string) => {
    setCodigoReclamo(codigo);
    setLoading(true);
    setError(null);
    setReclamo(null);
    setActiveStep(0);
    setPdfUrl(null);
    setSelectedFile(null);
    setFileBase64('');

    try {
      const response = await entregaService.buscarReclamo(codigo);

      if (response.exito) {
        if (!response.productos || response.productos.length === 0) {
          setError('No hay productos para entregar en este reclamo.');
          setReclamo(response);
          return;
        }

        // Asignar reemplazos automáticamente
        await asignarReemplazosYContinuar(codigo, response);
      } else {
        setError(response.mensaje);
      }
    } catch (err: any) {
      setError(err.response?.data?.detail || 'Error al buscar el reclamo');
    } finally {
      setLoading(false);
    }
  };

  const asignarReemplazosYContinuar = async (codigo: string, response: BuscarReclamoResponse) => {
    setAsignandoAutomatico(true);
    try {
      // Llamar al endpoint de asignación automática
      const resultado = await entregaService.asignarReemplazosAutomatico(codigo);
      if (!resultado.exito) {
        throw new Error('No se pudieron asignar los reemplazos automáticamente.');
      }

      // Recargar el reclamo para obtener los reemplazos asignados
      const responseActualizada = await entregaService.buscarReclamo(codigo);
      setReclamo(responseActualizada);
      setActiveStep(1);
      setSuccess('Reclamo encontrado y reemplazos asignados automáticamente.');

      // Verificar si todos los productos ya tienen reemplazo
      const todosTienenReemplazo = responseActualizada.productos.every(p => p.reemplazoValido);
      if (todosTienenReemplazo) {
        // Podríamos avanzar automáticamente al paso 2, pero dejamos que el usuario vea la info
      }
    } catch (err: any) {
      setError(err.message || 'Error al asignar reemplazos automáticamente. Verifique el stock disponible.');
      setReclamo(response); // Mostrar el reclamo sin reemplazos
      setActiveStep(1); // Aún así mostramos el paso para que vea el error
    } finally {
      setAsignandoAutomatico(false);
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
        // Recargar lista de pendientes
        cargarReclamosPendientes();
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
              Seleccione un reclamo pendiente de la lista o ingrese el código manualmente.
            </Typography>

            <Tabs value={tabBusqueda} onChange={(_, newValue) => setTabBusqueda(newValue)} sx={{ mb: 2 }}>
              <Tab label="Ingresar Código" />
              <Tab label="Reclamos Pendientes" />
            </Tabs>

            {tabBusqueda === 0 && (
              <Grid container spacing={2} alignItems="center">
                <Grid size={{ xs: 12, sm: 8 }}>
                  <TextField
                    label="Código de Reclamo"
                    value={codigoReclamo}
                    onChange={(e) => setCodigoReclamo(e.target.value)}
                    fullWidth
                    disabled={loading || asignandoAutomatico}
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
                    disabled={loading || asignandoAutomatico || !codigoReclamo.trim()}
                    fullWidth
                    startIcon={<Search />}
                  >
                    {loading || asignandoAutomatico ? 'Procesando...' : 'Buscar Reclamo'}
                  </Button>
                </Grid>
              </Grid>
            )}

            {tabBusqueda === 1 && (
              <Box>
                {cargandoPendientes ? (
                  <Box sx={{ display: 'flex', justifyContent: 'center', py: 4 }}>
                    <CircularProgress />
                  </Box>
                ) : reclamosPendientes.length === 0 ? (
                  <Alert severity="info">No hay reclamos pendientes de entrega.</Alert>
                ) : (
                  <TableContainer component={Paper}>
                    <Table>
                      <TableHead>
                        <TableRow>
                          <TableCell>Código</TableCell>
                          <TableCell>Cliente</TableCell>
                          <TableCell>RUC</TableCell>
                          <TableCell>Fecha Creación</TableCell>
                          <TableCell>Productos Pendientes</TableCell>
                          <TableCell>Acción</TableCell>
                        </TableRow>
                      </TableHead>
                      <TableBody>
                        {reclamosPendientes.map((r) => (
                          <TableRow key={r.id}>
                            <TableCell>{r.codigoReclamo}</TableCell>
                            <TableCell>{r.cliente}</TableCell>
                            <TableCell>{r.ruc}</TableCell>
                            <TableCell>{new Date(r.fechaCreacion).toLocaleDateString('es-EC')}</TableCell>
                            <TableCell>{r.cantidadProductosPendientes}</TableCell>
                            <TableCell>
                              <Button
                                variant="outlined"
                                size="small"
                                onClick={() => handleSeleccionarReclamoPendiente(r.codigoReclamo)}
                                disabled={loading || asignandoAutomatico}
                              >
                                Seleccionar
                              </Button>
                            </TableCell>
                          </TableRow>
                        ))}
                      </TableBody>
                    </Table>
                  </TableContainer>
                )}
              </Box>
            )}
          </Box>
        );
      case 1:
        return (
          <Box>
            <Typography variant="body1" paragraph>
              Los siguientes productos han sido aprobados para reemplazo. El sistema ha asignado automáticamente
              productos de reemplazo del inventario disponible (misma marca y modelo). Verifique la información.
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

            {asignandoAutomatico && (
              <Box sx={{ display: 'flex', justifyContent: 'center', my: 4 }}>
                <CircularProgress />
                <Typography sx={{ ml: 2 }}>Asignando reemplazos automáticamente...</Typography>
              </Box>
            )}

            <TableContainer component={Paper}>
              <Table>
                <TableHead>
                  <TableRow>
                    <TableCell>Producto Defectuoso</TableCell>
                    <TableCell>Marca/Modelo</TableCell>
                    <TableCell>Reemplazo Asignado</TableCell>
                    <TableCell>Estado</TableCell>
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
                            icon={<ErrorIcon />}
                          />
                        )}
                      </TableCell>
                      <TableCell>
                        <Typography variant="caption" color={producto.reemplazoValido ? "success.main" : "error.main"}>
                          {producto.mensajeValidacion || (producto.reemplazoValido ? "Asignado" : "Pendiente")}
                        </Typography>
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
                disabled={asignandoAutomatico}
              >
                Volver
              </Button>
              <Button
                variant="contained"
                onClick={() => setActiveStep(2)}
                disabled={asignandoAutomatico || !reclamo?.productos.every(p => p.reemplazoValido)}
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
    </Box>
  );
};

export default EntregaDashboard;