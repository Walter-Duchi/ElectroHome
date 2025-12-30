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
} from '@mui/material';
import {
  Delete as DeleteIcon,
  CheckCircle as CheckCircleIcon,
  Error as ErrorIcon,
  Add as AddIcon,
  Print as PrintIcon,
  Person as PersonIcon,
  List as ListIcon,
} from '@mui/icons-material';
import { useNavigate } from 'react-router-dom';
import { reclamoService } from '../../services/reclamoService';
import { ProductoReclamado, ValidarClienteResponse } from '../../src/types/reclamo';

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
  const [rucCliente, setRucCliente] = useState<string>('');
  const [clienteValidado, setClienteValidado] = useState<ClienteValidadoType | null>(null);

  // Paso 2: Agregar productos
  const [numeroSerie, setNumeroSerie] = useState<string>('');
  const [productos, setProductos] = useState<ProductoReclamado[]>([]);
  const [formaCompensacion, setFormaCompensacion] = useState<'Reembolso' | 'Reemplazo'>('Reembolso');

  // Paso 3: Confirmar
  const [confirmDialogOpen, setConfirmDialogOpen] = useState<boolean>(false);

  const steps = ['Validar Cliente', 'Agregar Productos', 'Confirmar Reclamo'];

  const handleValidarCliente = async (): Promise<void> => {
    if (!rucCliente.trim()) {
      setErrorMessage('Por favor ingrese el RUC del cliente');
      return;
    }

    setLoading(true);
    setErrorMessage('');
    setSuccessMessage('');

    try {
      const response: ValidarClienteResponse = await reclamoService.validarCliente({ ruc: rucCliente });

      if (response.esValido) {
        setClienteValidado(response);
        setActiveStep(1);
        setSuccessMessage('Cliente validado correctamente');
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

      if (!response.esValido) {
        setErrorMessage(response.mensaje || 'Producto no válido');
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
      const request = {
        rucCliente,
        productos: productosValidos.map(p => ({
          numeroSerie: p.numeroSerie,
          formaCompensacion: p.formaCompensacion
        }))
      };

      const response = await reclamoService.crearReclamo(request);

      if (response.exito && response.pdfBase64 && response.pdfFileName) {
        setSuccessMessage('Reclamo creado exitosamente. Descargando comprobante...');

        reclamoService.descargarPdf(response.pdfBase64, response.pdfFileName);

        setTimeout(() => {
          navigate('/');
        }, 3000);
      } else {
        setErrorMessage(response.mensaje || 'Error al crear el reclamo');
      }
    } catch (err: unknown) {
      console.error('Error creando reclamo:', err);
      setErrorMessage('Error al crear el reclamo');
    } finally {
      setLoading(false);
    }
  };

  const handleRucClienteChange = (e: React.ChangeEvent<HTMLInputElement>): void => {
    setRucCliente(e.target.value);
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
              label="RUC del Cliente"
              value={rucCliente}
              onChange={handleRucClienteChange}
              fullWidth
              margin="normal"
              placeholder="Ingrese el RUC del cliente"
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
              disabled={loading || !rucCliente.trim()}
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

            <Grid container spacing={2} sx={{ mb: 3 }}>
              <Grid size={{ xs:12, sm:6 }}>
                <TextField
                  label="Número de Serie"
                  value={numeroSerie}
                  onChange={handleNumeroSerieChange}
                  fullWidth
                  placeholder="Ingrese el número de serie del producto"
                  disabled={loading}
                />
              </Grid>
              <Grid size={{ xs:12, sm:4 }}>
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
              <Grid size={{ xs:12, sm:2 }}>
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
                  <Grid size={{ xs:12, sm:6 }}>
                    <Typography variant="body2" color="text.secondary">
                      RUC
                    </Typography>
                    <Typography variant="body1">
                      {rucCliente}
                    </Typography>
                  </Grid>
                  <Grid size={{ xs:12, sm:6 }}>
                    <Typography variant="body2" color="text.secondary">
                      Razón Social
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
              Al confirmar, se generará un comprobante PDF y se asignarán técnicos para revisión.
              El sistema distribuirá equitativamente la carga de trabajo entre técnicos certificados.
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
            ¿Está seguro de generar el reclamo?
            Se imprimirá el comprobante de reclamo y se asignarán distintos técnicos para
            la revisión de cada uno de los productos.
          </DialogContentText>
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setConfirmDialogOpen(false)}>Cancelar</Button>
          <Button
            onClick={handleConfirmarReclamo}
            variant="contained"
            autoFocus
          >
            Confirmar
          </Button>
        </DialogActions>
      </Dialog>
    </Box>
  );
};

export default CrearReclamo;
