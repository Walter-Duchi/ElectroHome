import type React from 'react';
import { useState } from 'react';
import {
  Box,
  Paper,
  Typography,
  TextField,
  Button,
  Stepper,
  Step,
  StepLabel,
  Alert,
  CircularProgress,
  Grid,
  Card,
  CardContent,
  IconButton,
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TableRow,
  Chip,
  Dialog,
  DialogTitle,
  DialogContent,
  DialogContentText,
  DialogActions,
  MenuItem,
  FormControl,
  InputLabel,
  Select,
  SelectChangeEvent,
  Tabs,
  Tab,
} from '@mui/material';
import {
  Delete as DeleteIcon,
  CheckCircle as CheckCircleIcon,
  Error as ErrorIcon,
  Add as AddIcon,
  Print as PrintIcon,
  Person as PersonIcon,
  List as ListIcon,
  ShoppingCart as ShoppingCartIcon,
} from '@mui/icons-material';
import { useNavigate } from 'react-router-dom';
import { reclamoService } from '../../services/reclamoService';
import { ProductoReclamado, ValidarClienteResponse, ProductoCompradoDTO } from '../../src/types/reclamo';

interface ClienteValidadoType {
  esValido: boolean;
  mensaje?: string;
  clienteId?: number;
  razonSocial?: string;
}

const CrearReclamo = () => {
  const navigate = useNavigate();
  const [activeStep, setActiveStep] = useState<number>(0);
  const [loading, setLoading] = useState<boolean>(false);
  const [errorMessage, setErrorMessage] = useState<string>('');
  const [successMessage, setSuccessMessage] = useState<string>('');

  // Paso 1: Validar cliente
  const [identificadorCliente, setIdentificadorCliente] = useState<string>('');
  const [clienteValidado, setClienteValidado] = useState<ClienteValidadoType | null>(null);

  // Paso 2: Agregar productos
  const [tabValue, setTabValue] = useState(0); // 0: escribir serie, 1: historial
  const [numeroSerie, setNumeroSerie] = useState<string>('');
  const [productos, setProductos] = useState<ProductoReclamado[]>([]);
  const [formaCompensacion, setFormaCompensacion] = useState<'Reembolso' | 'Reemplazo'>('Reembolso');
  const [productosComprados, setProductosComprados] = useState<ProductoCompradoDTO[]>([]);
  const [cargandoHistorial, setCargandoHistorial] = useState(false);

  // Paso 3: Confirmar
  const [confirmDialogOpen, setConfirmDialogOpen] = useState<boolean>(false);

  const steps = ['Validar Cliente', 'Agregar Productos', 'Confirmar Reclamo'];

  const handleValidarCliente = async (): Promise<void> => {
    if (!identificadorCliente.trim()) {
      setErrorMessage('Por favor ingrese cédula, RUC o pasaporte del cliente');
      return;
    }

    setLoading(true);
    setErrorMessage('');
    setSuccessMessage('');

    try {
      const response: ValidarClienteResponse = await reclamoService.validarCliente({ identificador: identificadorCliente });

      if (response.esValido) {
        setClienteValidado(response);
        setActiveStep(1);
        setSuccessMessage('Cliente validado correctamente');
        // Cargar productos comprados para la pestaña de historial
        cargarProductosComprados();
      } else {
        setErrorMessage(response.mensaje || 'Error al validar cliente');
      }
    } catch (err: unknown) {
      console.error('Error validando cliente:', err);
      setErrorMessage('Error al validar cliente');
    } finally {
      setLoading(false);
    }
  };

  const cargarProductosComprados = async () => {
    if (!identificadorCliente) return;
    setCargandoHistorial(true);
    try {
      const data = await reclamoService.obtenerProductosComprados(identificadorCliente);
      setProductosComprados(data);
    } catch (error) {
      console.error('Error cargando historial:', error);
    } finally {
      setCargandoHistorial(false);
    }
  };

  const handleAgregarProducto = async (): Promise<void> => {
    if (!numeroSerie.trim()) {
      setErrorMessage('Por favor ingrese un número de serie');
      return;
    }

    // Verificar si el producto ya fue agregado
    if (productos.some(p => p.numeroSerie === numeroSerie)) {
      setErrorMessage('Este producto ya ha sido agregado');
      return;
    }

    setLoading(true);
    setErrorMessage('');

    const productoId = `producto-${Date.now()}`;
    setProductos(prev => [...prev, {
      id: productoId,
      numeroSerie,
      formaCompensacion,
      tieneGarantia: false,
      validando: true
    }]);

    try {
      const response = await reclamoService.validarProducto({ numeroSerie });

      setProductos(prev => prev.map(p =>
        p.id === productoId ? {
          ...p,
          validando: false,
          esValido: response.esValido,
          tieneGarantia: response.tieneGarantia,
          marca: response.marca,
          modelo: response.modelo,
          estadoInventario: response.estadoInventario,
          especificacion: response.especificacion,
          precio: response.precio,
          error: response.mensaje
        } : p
      ));

      if (!response.esValido || !response.tieneGarantia) {
        setErrorMessage(response.mensaje || 'Producto no válido');
        // Remover producto inválido
        setProductos(prev => prev.filter(p => p.id !== productoId));
      } else {
        setNumeroSerie('');
        setFormaCompensacion('Reembolso');
      }
    } catch (err: unknown) {
      console.error('Error validando producto:', err);
      setProductos(prev => prev.map(p =>
        p.id === productoId ? {
          ...p,
          validando: false,
          error: 'Error al validar producto'
        } : p
      ));
      setErrorMessage('Error al validar producto');
    } finally {
      setLoading(false);
    }
  };

  const handleAgregarDesdeHistorial = (producto: ProductoCompradoDTO) => {
    // Verificar si ya fue agregado
    if (productos.some(p => p.numeroSerie === producto.numeroSerie)) {
      setErrorMessage('Este producto ya ha sido agregado');
      return;
    }

    const productoId = `producto-${Date.now()}`;
    const nuevoProducto: ProductoReclamado = {
      id: productoId,
      numeroSerie: producto.numeroSerie,
      marca: producto.marca,
      modelo: producto.modelo,
      tieneGarantia: producto.tieneGarantia,
      formaCompensacion: formaCompensacion,
      especificacion: `${producto.marca} ${producto.modelo}`,
      precio: producto.precio,
      validando: false,
    };

    if (!producto.tieneGarantia) {
      setErrorMessage('Este producto no tiene garantía válida');
      return;
    }

    setProductos(prev => [...prev, nuevoProducto]);
  };

  const handleEliminarProducto = (id: string): void => {
    setProductos(prev => prev.filter(p => p.id !== id));
  };

  const handleConfirmarReclamo = async (): Promise<void> => {
    setConfirmDialogOpen(false);
    setLoading(true);
    setErrorMessage('');
    setSuccessMessage('');

    const productosValidos = productos.filter(p => p.tieneGarantia);

    if (productosValidos.length === 0) {
      setErrorMessage('Debe agregar al menos un producto válido con garantía');
      setLoading(false);
      return;
    }

    try {
      console.log('=== DEPURACIÓN - ANTES DE ENVIAR ===');
      console.log('Cliente Identificador:', identificadorCliente);
      console.log('Productos válidos:', productosValidos);

      const productosParaEnviar = productosValidos.map(p => ({
        numeroSerie: p.numeroSerie.trim(),
        formaCompensacion: p.formaCompensacion
      }));

      const request = {
        identificadorCliente: identificadorCliente.trim(),
        productos: productosParaEnviar
      };

      console.log('Enviando solicitud:', JSON.stringify(request, null, 2));

      const response = await reclamoService.crearReclamo(request);

      console.log('Respuesta del servidor:', response);

      if (response.exito) {
        setSuccessMessage('¡Reclamo creado exitosamente! Todos los productos tienen técnicos asignados.');

        if (response.pdfBase64 && response.pdfFileName) {
          setTimeout(() => {
            reclamoService.descargarPdf(response.pdfBase64!, response.pdfFileName!);

            // Mostrar mensaje de éxito
            setTimeout(() => {
              setSuccessMessage('¡Reclamo creado exitosamente! PDF descargado en Documentos/reclamos');
              setTimeout(() => {
                navigate('/');
              }, 3000);
            }, 1000);
          }, 1000);
        } else {
          setTimeout(() => {
            navigate('/');
          }, 2000);
        }
      } else {
        setErrorMessage(response.mensaje || 'Error al crear el reclamo');
      }
    } catch (err: unknown) {
      console.error('Error completo:', err);
      if (err instanceof Error) {
        setErrorMessage(`Error: ${err.message}`);
      } else {
        setErrorMessage('Error desconocido al crear el reclamo');
      }
    } finally {
      setLoading(false);
    }
  };

  const handleIdentificadorChange = (e: React.ChangeEvent<HTMLInputElement>): void => {
    setIdentificadorCliente(e.target.value);
  };

  const handleNumeroSerieChange = (e: React.ChangeEvent<HTMLInputElement>): void => {
    setNumeroSerie(e.target.value);
  };

  const handleFormaCompensacionChange = (e: SelectChangeEvent<'Reembolso' | 'Reemplazo'>): void => {
    setFormaCompensacion(e.target.value as 'Reembolso' | 'Reemplazo');
  };

  const getStepContent = (step: number) => {
    const productosValidos = productos.filter(p => p.tieneGarantia);

    switch (step) {
      case 0:
        return (
          <Box sx={{ maxWidth: 500, mx: 'auto' }}>
            <Typography variant="h6" gutterBottom sx={{ mb: 3 }}>
              Validar Cliente
            </Typography>

            <TextField
              label="Cédula / RUC / Pasaporte"
              value={identificadorCliente}
              onChange={handleIdentificadorChange}
              fullWidth
              margin="normal"
              placeholder="Ingrese la identificación del cliente"
              disabled={loading}
            />

            {clienteValidado && (
              <Alert severity="success" sx={{ mt: 2 }}>
                Cliente validado: {clienteValidado.razonSocial}
              </Alert>
            )}

            <Button
              variant="contained"
              onClick={handleValidarCliente}
              disabled={loading || !identificadorCliente.trim()}
              sx={{ mt: 3 }}
              startIcon={<PersonIcon />}
            >
              {loading ? <CircularProgress size={24} /> : 'Validar Cliente'}
            </Button>
          </Box>
        );

      case 1:
        return (
          <Box>
            <Typography variant="h6" gutterBottom sx={{ mb: 3 }}>
              Agregar Productos
            </Typography>

            <Tabs value={tabValue} onChange={(_, newValue) => setTabValue(newValue)} sx={{ mb: 2 }}>
              <Tab label="Escribir Número de Serie" />
              <Tab label="Seleccionar del Historial" />
            </Tabs>

            {tabValue === 0 && (
              <>
                <Grid container spacing={2} sx={{ mb: 3 }}>
                  <Grid size={{ xs: 12, sm: 6 }}>
                    <TextField
                      label="Número de Serie"
                      value={numeroSerie}
                      onChange={handleNumeroSerieChange}
                      fullWidth
                      placeholder="Ingrese el número de serie del producto"
                      disabled={loading}
                    />
                  </Grid>
                  <Grid size={{ xs: 12, sm: 4 }}>
                    <FormControl fullWidth>
                      <InputLabel>Forma de Compensación</InputLabel>
                      <Select
                        value={formaCompensacion}
                        onChange={handleFormaCompensacionChange}
                        label="Forma de Compensación"
                      >
                        <MenuItem value="Reembolso">Reembolso</MenuItem>
                        <MenuItem value="Reemplazo">Reemplazo</MenuItem>
                      </Select>
                    </FormControl>
                  </Grid>
                  <Grid size={{ xs: 12, sm: 2 }}>
                    <Button
                      variant="contained"
                      onClick={handleAgregarProducto}
                      disabled={loading || !numeroSerie.trim()}
                      fullWidth
                      sx={{ height: '56px' }}
                      startIcon={<AddIcon />}
                    >
                      Agregar
                    </Button>
                  </Grid>
                </Grid>
              </>
            )}

            {tabValue === 1 && (
              <Box sx={{ mb: 3 }}>
                {cargandoHistorial ? (
                  <CircularProgress />
                ) : (
                  <TableContainer component={Paper} sx={{ maxHeight: 300 }}>
                    <Table stickyHeader size="small">
                      <TableHead>
                        <TableRow>
                          <TableCell>N° Serie</TableCell>
                          <TableCell>Producto</TableCell>
                          <TableCell>Fecha Compra</TableCell>
                          <TableCell>Garantía</TableCell>
                          <TableCell>Acción</TableCell>
                        </TableRow>
                      </TableHead>
                      <TableBody>
                        {productosComprados.map((prod) => (
                          <TableRow key={prod.numeroSerie}>
                            <TableCell>{prod.numeroSerie}</TableCell>
                            <TableCell>{prod.marca} {prod.modelo}</TableCell>
                            <TableCell>{prod.fechaCompra ? new Date(prod.fechaCompra).toLocaleDateString('es-EC') : '-'}</TableCell>
                            <TableCell>
                              <Chip
                                label={prod.tieneGarantia ? 'Válida' : 'Vencida'}
                                color={prod.tieneGarantia ? 'success' : 'error'}
                                size="small"
                              />
                            </TableCell>
                            <TableCell>
                              <Button
                                size="small"
                                variant="outlined"
                                onClick={() => handleAgregarDesdeHistorial(prod)}
                                disabled={!prod.tieneGarantia || productos.some(p => p.numeroSerie === prod.numeroSerie)}
                              >
                                Agregar
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

            {productos.length > 0 && (
              <Card sx={{ mt: 3 }}>
                <CardContent>
                  <Typography variant="subtitle1" gutterBottom>
                    Productos Agregados ({productos.filter(p => p.tieneGarantia).length} válidos)
                  </Typography>

                  <TableContainer>
                    <Table size="small">
                      <TableHead>
                        <TableRow>
                          <TableCell>N° Serie</TableCell>
                          <TableCell>Marca/Modelo</TableCell>
                          <TableCell>Estado</TableCell>
                          <TableCell>Compensación</TableCell>
                          <TableCell>Acciones</TableCell>
                        </TableRow>
                      </TableHead>
                      <TableBody>
                        {productos.map((producto) => (
                          <TableRow key={producto.id}>
                            <TableCell>{producto.numeroSerie}</TableCell>
                            <TableCell>
                              {producto.marca && producto.modelo ? (
                                `${producto.marca} ${producto.modelo}`
                              ) : producto.validando ? (
                                <CircularProgress size={20} />
                              ) : (
                                'No válido'
                              )}
                            </TableCell>
                            <TableCell>
                              {producto.validando ? (
                                <Chip label="Validando..." size="small" />
                              ) : producto.tieneGarantia ? (
                                <Chip
                                  label="Con Garantía"
                                  color="success"
                                  size="small"
                                  icon={<CheckCircleIcon />}
                                />
                              ) : (
                                <Chip
                                  label="Sin Garantía"
                                  color="error"
                                  size="small"
                                  icon={<ErrorIcon />}
                                />
                              )}
                            </TableCell>
                            <TableCell>
                              <Chip
                                label={producto.formaCompensacion}
                                variant="outlined"
                                size="small"
                              />
                            </TableCell>
                            <TableCell>
                              <IconButton
                                size="small"
                                onClick={() => handleEliminarProducto(producto.id)}
                                color="error"
                              >
                                <DeleteIcon />
                              </IconButton>
                            </TableCell>
                          </TableRow>
                        ))}
                      </TableBody>
                    </Table>
                  </TableContainer>

                  <Box sx={{ mt: 2, display: 'flex', justifyContent: 'space-between' }}>
                    <Typography variant="body2" color="text.secondary">
                      Total productos: {productos.length}
                    </Typography>
                    <Typography variant="body2" color="text.secondary">
                      Productos con garantía: {productos.filter(p => p.tieneGarantia).length}
                    </Typography>
                  </Box>
                </CardContent>
              </Card>
            )}

            <Box sx={{ mt: 3, display: 'flex', justifyContent: 'space-between' }}>
              <Button
                variant="outlined"
                onClick={() => setActiveStep(0)}
              >
                Atrás
              </Button>
              <Button
                variant="contained"
                onClick={() => setActiveStep(2)}
                disabled={productos.filter(p => p.tieneGarantia).length === 0}
                startIcon={<ListIcon />}
              >
                Continuar
              </Button>
            </Box>
          </Box>
        );

      case 2:
        return (
          <Box>
            <Typography variant="h6" gutterBottom sx={{ mb: 3 }}>
              Confirmar Reclamo
            </Typography>

            <Card sx={{ mb: 3 }}>
              <CardContent>
                <Typography variant="subtitle1" gutterBottom>
                  Información del Cliente
                </Typography>
                <Grid container spacing={2}>
                  <Grid size={{ xs: 12, sm: 6 }}>
                    <Typography variant="body2" color="text.secondary">
                      Identificación
                    </Typography>
                    <Typography variant="body1">
                      {identificadorCliente}
                    </Typography>
                  </Grid>
                  <Grid size={{ xs: 12, sm: 6 }}>
                    <Typography variant="body2" color="text.secondary">
                      Nombre
                    </Typography>
                    <Typography variant="body1">
                      {clienteValidado?.razonSocial || 'No validado'}
                    </Typography>
                  </Grid>
                </Grid>
              </CardContent>
            </Card>

            <Card sx={{ mb: 3 }}>
              <CardContent>
                <Typography variant="subtitle1" gutterBottom>
                  Productos a Reclamar
                </Typography>
                <TableContainer>
                  <Table size="small">
                    <TableHead>
                      <TableRow>
                        <TableCell>N° Serie</TableCell>
                        <TableCell>Producto</TableCell>
                        <TableCell>Compensación</TableCell>
                        <TableCell>Precio</TableCell>
                      </TableRow>
                    </TableHead>
                    <TableBody>
                      {productosValidos.map((producto) => (
                        <TableRow key={producto.id}>
                          <TableCell>{producto.numeroSerie}</TableCell>
                          <TableCell>
                            {producto.marca} {producto.modelo}
                          </TableCell>
                          <TableCell>
                            <Chip
                              label={producto.formaCompensacion}
                              variant="outlined"
                              size="small"
                            />
                          </TableCell>
                          <TableCell>
                            ${producto.precio?.toFixed(2) || '0.00'}
                          </TableCell>
                        </TableRow>
                      ))}
                    </TableBody>
                  </Table>
                </TableContainer>

                <Box sx={{ mt: 2, textAlign: 'right' }}>
                  <Typography variant="h6">
                    Total Productos: {productosValidos.length}
                  </Typography>
                </Box>
              </CardContent>
            </Card>

            <Alert severity="info" sx={{ mb: 3 }}>
              <Typography variant="subtitle2" gutterBottom>
                Verificación de Asignación de Técnicos
              </Typography>
              <Typography variant="body2">
                1. El sistema verificará que existan técnicos certificados para cada marca de producto.<br />
                2. Se asignará un técnico específico a cada producto según su marca.<br />
                3. La carga de trabajo se distribuirá equitativamente entre técnicos certificados.<br />
                4. Si algún producto no puede tener técnico asignado, el reclamo NO se creará.<br />
                5. Se generará un PDF con todos los detalles y se guardará en Documentos/reclamos.
              </Typography>
            </Alert>

            <Alert severity="warning" sx={{ mb: 3 }}>
              <Typography variant="body2">
                <strong>Importante:</strong> Este proceso garantiza que cada producto sea revisado por un técnico
                certificado en la marca correspondiente. Si falla la asignación de algún técnico,
                todo el reclamo será cancelado automáticamente.
              </Typography>
            </Alert>

            <Box sx={{ display: 'flex', justifyContent: 'space-between' }}>
              <Button
                variant="outlined"
                onClick={() => setActiveStep(1)}
              >
                Atrás
              </Button>
              <Button
                variant="contained"
                color="primary"
                onClick={() => setConfirmDialogOpen(true)}
                disabled={loading || productosValidos.length === 0}
                startIcon={<PrintIcon />}
              >
                {loading ? <CircularProgress size={24} /> : 'Crear Reclamo'}
              </Button>
            </Box>
          </Box>
        );

      default:
        return <></>;
    }
  };

  return (
    <Box sx={{ maxWidth: 1200, mx: 'auto', p: 3 }}>
      <Paper sx={{ p: 3 }}>
        <Typography variant="h5" gutterBottom sx={{ mb: 4, fontWeight: 600 }}>
          Crear Nuevo Reclamo
        </Typography>

        <Stepper activeStep={activeStep} sx={{ mb: 4 }}>
          {steps.map((label) => (
            <Step key={label}>
              <StepLabel>{label}</StepLabel>
            </Step>
          ))}
        </Stepper>

        {errorMessage && (
          <Alert severity="error" sx={{ mb: 3 }} onClose={() => setErrorMessage('')}>
            {errorMessage}
          </Alert>
        )}

        {successMessage && (
          <Alert severity="success" sx={{ mb: 3 }}>
            {successMessage}
          </Alert>
        )}

        {getStepContent(activeStep)}
      </Paper>

      <Dialog
        open={confirmDialogOpen}
        onClose={() => setConfirmDialogOpen(false)}
      >
        <DialogTitle>Confirmar Creación de Reclamo</DialogTitle>
        <DialogContent>
          <DialogContentText>
            <strong>Verificación de Asignación de Técnicos:</strong><br /><br />

            ¿Está seguro de crear el reclamo? El sistema realizará las siguientes verificaciones:<br /><br />

            1. Validará que existan técnicos certificados para cada marca de producto.<br />
            2. Asignará un técnico específico a cada producto (distribución equitativa).<br />
            3. Si algún producto no puede tener técnico asignado, el reclamo NO se creará.<br />
            4. Generará un PDF real (no HTML) con todos los detalles.<br />
            5. El PDF se guardará en Documentos/reclamos/<br /><br />

            <strong>¿Desea continuar?</strong>
          </DialogContentText>
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setConfirmDialogOpen(false)}>Cancelar</Button>
          <Button
            onClick={handleConfirmarReclamo}
            variant="contained"
            color="primary"
            autoFocus
          >
            Sí, crear reclamo
          </Button>
        </DialogActions>
      </Dialog>
    </Box>
  );
};

export default CrearReclamo;