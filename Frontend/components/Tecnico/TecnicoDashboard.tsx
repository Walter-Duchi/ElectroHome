import React, { useState, useEffect } from 'react';
import {
    Box,
    Typography,
    Card,
    CardContent,
    Grid,
    Paper,
    Table,
    TableBody,
    TableCell,
    TableContainer,
    TableHead,
    TableRow,
    Chip,
    Button,
    Alert,
    CircularProgress,
    IconButton,
    Tooltip,
    Dialog,
    DialogTitle,
    DialogContent,
    DialogActions,
    TextField,
    FormControl,
    InputLabel,
    Select,
    MenuItem,
    FormHelperText,
    Stack,
    Divider
} from '@mui/material';
import {
    Visibility,
    CheckCircle,
    Cancel,
    Assignment,
    CalendarToday,
    Person,
    Receipt,
    Paid,
    Build,
    Engineering,
    Upload
} from '@mui/icons-material';
import { useAuth } from '../../services/authContext';
import { tecnicoService } from '../../services/tecnicoService';
import { type TecnicoProducto } from '../../src/types/tecnico';

const TecnicoDashboard: React.FC = () => {
    const { auth } = useAuth();
    const [productos, setProductos] = useState<TecnicoProducto[]>([]);
    const [proximoProducto, setProximoProducto] = useState<TecnicoProducto | null>(null);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState('');
    const [showRevisarDialog, setShowRevisarDialog] = useState(false);
    const [showFinalizarDialog, setShowFinalizarDialog] = useState(false);
    const [productoSeleccionado, setProductoSeleccionado] = useState<TecnicoProducto | null>(null);
    const [formData, setFormData] = useState({
        estado: 'Aprobado',
        explicacion: '',
        archivo: null as File | null
    });
    const [submitting, setSubmitting] = useState(false);
    const [ordenValido, setOrdenValido] = useState(false);

    // Cargar productos al iniciar
    useEffect(() => {
        cargarProductos();
    }, []);

    const cargarProductos = async () => {
        try {
            setLoading(true);
            setError('');
            
            // Cargar todos los productos
            const productosData = await tecnicoService.obtenerProductosAsignados();
            setProductos(productosData);
            
            // Cargar próximo producto
            const proximo = await tecnicoService.obtenerProximoProducto();
            setProximoProducto(proximo);
            
        } catch (err: any) {
            setError(err.message);
        } finally {
            setLoading(false);
        }
    };

    const handleIniciarRevision = async (producto: TecnicoProducto) => {
        try {
            setError('');
            
            // Validar orden de revisión
            const validacion = await tecnicoService.validarOrdenRevisacion(producto.id);
            
            if (!validacion.valido) {
                setError(`No puedes revisar este producto. ${validacion.message}`);
                return;
            }

            setOrdenValido(true);
            setProductoSeleccionado(producto);
            setShowRevisarDialog(true);
            
        } catch (err: any) {
            setError(err.message);
        }
    };

    const confirmarIniciarRevision = async () => {
        try {
            if (!productoSeleccionado) return;

            setSubmitting(true);
            
            await tecnicoService.iniciarRevision({
                reclamoProductoSnId: productoSeleccionado.id,
                tecnicoId: auth.user?.id || 0
            });

            // Actualizar estado local
            setProductos(prev => prev.map(p => 
                p.id === productoSeleccionado.id 
                    ? { ...p, estado: 'En Revision' as const }
                    : p
            ));

            setProximoProducto(prev => 
                prev?.id === productoSeleccionado.id ? null : prev
            );

            setShowRevisarDialog(false);
            setProductoSeleccionado(null);
            
            // Recargar productos
            await cargarProductos();
            
        } catch (err: any) {
            setError(err.message);
        } finally {
            setSubmitting(false);
        }
    };

    const handleFinalizarRevision = (producto: TecnicoProducto) => {
        setProductoSeleccionado(producto);
        setFormData({
            estado: 'Aprobado',
            explicacion: '',
            archivo: null
        });
        setShowFinalizarDialog(true);
    };

    const confirmarFinalizarRevision = async () => {
        try {
            if (!productoSeleccionado) return;

            setSubmitting(true);
            setError('');

            // Validaciones
            if (!formData.explicacion.trim()) {
                setError('La explicación es requerida');
                return;
            }

            // Convertir archivo a base64 si existe
            let pdfBase64 = undefined;
            let pdfFileName = undefined;

            if (formData.archivo) {
                pdfBase64 = await tecnicoService.convertirArchivoABase64(formData.archivo);
                pdfFileName = formData.archivo.name;
            }

            await tecnicoService.finalizarRevision({
                reclamoProductoSnId: productoSeleccionado.id,
                tecnicoId: auth.user?.id || 0,
                estado: formData.estado as 'Aprobado' | 'Rechazado',
                explicacion: formData.explicacion,
                pdfBase64,
                pdfFileName
            });

            // Actualizar estado local
            setProductos(prev => prev.map(p => 
                p.id === productoSeleccionado.id 
                    ? { ...p, estado: formData.estado as 'Aprobado' | 'Rechazado' }
                    : p
            ));

            setShowFinalizarDialog(false);
            setProductoSeleccionado(null);
            
            // Recargar productos
            await cargarProductos();
            
        } catch (err: any) {
            setError(err.message);
        } finally {
            setSubmitting(false);
        }
    };

    const getEstadoChip = (estado: string) => {
        switch (estado) {
            case 'Pendiente':
                return <Chip label="Pendiente" color="warning" size="small" />;
            case 'En Revision':
                return <Chip label="En Revisión" color="info" size="small" />;
            case 'Aprobado':
                return <Chip label="Aprobado" color="success" size="small" />;
            case 'Rechazado':
                return <Chip label="Rechazado" color="error" size="small" />;
            default:
                return <Chip label={estado} size="small" />;
        }
    };

    const getFormaCompensacionIcon = (forma: string) => {
        return forma === 'Reembolso' ? <Paid /> : <Build />;
    };

    if (loading) {
        return (
            <Box sx={{ display: 'flex', justifyContent: 'center', alignItems: 'center', minHeight: '400px' }}>
                <CircularProgress />
            </Box>
        );
    }

    return (
        <Box sx={{ p: 3 }}>
            {/* Header */}
            <Box sx={{ mb: 4 }}>
                <Typography variant="h4" fontWeight={600} gutterBottom>
                    <Engineering sx={{ verticalAlign: 'middle', mr: 2 }} />
                    Panel del Técnico
                </Typography>
                <Typography variant="body1" color="text.secondary">
                    Gestiona los productos asignados para revisión técnica
                </Typography>
            </Box>

            {error && (
                <Alert severity="error" sx={{ mb: 3 }} onClose={() => setError('')}>
                    {error}
                </Alert>
            )}

            {/* Próximo producto a revisar */}
            {proximoProducto && (
                <Card sx={{ mb: 4, bgcolor: 'primary.light', color: 'white' }}>
                    <CardContent>
                        <Grid container alignItems="center" spacing={2}>
                            <Grid>
                                <Assignment sx={{ fontSize: 40 }} />
                            </Grid>
                            <Grid size={{ xs: 'auto' }}>
                                <Typography variant="h6" gutterBottom>
                                    Próximo Producto para Revisión
                                </Typography>
                                <Typography variant="body2">
                                    <strong>Producto:</strong> {proximoProducto.marca} {proximoProducto.modelo}
                                </Typography>
                                <Typography variant="body2">
                                    <strong>N° Serie:</strong> {proximoProducto.numeroSerie}
                                </Typography>
                                <Typography variant="body2">
                                    <strong>Cliente:</strong> {proximoProducto.clienteNombre}
                                </Typography>
                                <Typography variant="body2">
                                    <strong>Fecha Reclamo:</strong> {new Date(proximoProducto.fechaReclamoClienteFinal).toLocaleDateString()}
                                </Typography>
                            </Grid>
                            <Grid>
                                <Button
                                    variant="contained"
                                    color="secondary"
                                    startIcon={<Visibility />}
                                    onClick={() => handleIniciarRevision(proximoProducto)}
                                    size="large"
                                >
                                    Iniciar Revisión
                                </Button>
                            </Grid>
                        </Grid>
                    </CardContent>
                </Card>
            )}

            {/* Estadísticas */}
            <Grid container spacing={3} sx={{ mb: 4 }}>
                <Grid size={{ xs: 12, sm: 6, md: 3 }}>
                    <Card>
                        <CardContent>
                            <Typography color="text.secondary" gutterBottom>
                                Total Asignados
                            </Typography>
                            <Typography variant="h4">
                                {productos.length}
                            </Typography>
                        </CardContent>
                    </Card>
                </Grid>
                <Grid size={{ xs: 12, sm: 6, md: 3 }}>
                    <Card>
                        <CardContent>
                            <Typography color="text.secondary" gutterBottom>
                                Pendientes
                            </Typography>
                            <Typography variant="h4" color="warning.main">
                                {productos.filter(p => p.estado === 'Pendiente').length}
                            </Typography>
                        </CardContent>
                    </Card>
                </Grid>
                <Grid size={{ xs: 12, sm: 6, md: 3 }}>
                    <Card>
                        <CardContent>
                            <Typography color="text.secondary" gutterBottom>
                                En Revisión
                            </Typography>
                            <Typography variant="h4" color="info.main">
                                {productos.filter(p => p.estado === 'En Revision').length}
                            </Typography>
                        </CardContent>
                    </Card>
                </Grid>
                <Grid size={{ xs: 12, sm: 6, md: 3 }}>
                    <Card>
                        <CardContent>
                            <Typography color="text.secondary" gutterBottom>
                                Completados
                            </Typography>
                            <Typography variant="h4" color="success.main">
                                {productos.filter(p => p.estado === 'Aprobado' || p.estado === 'Rechazado').length}
                            </Typography>
                        </CardContent>
                    </Card>
                </Grid>
            </Grid>

            {/* Tabla de productos */}
            <Card>
                <CardContent>
                    <Typography variant="h6" gutterBottom sx={{ display: 'flex', alignItems: 'center' }}>
                        <Assignment sx={{ mr: 1 }} />
                        Productos Asignados
                    </Typography>
                    
                    {productos.length === 0 ? (
                        <Alert severity="info">
                            No hay productos asignados para revisión en este momento.
                        </Alert>
                    ) : (
                        <TableContainer component={Paper} sx={{ mt: 2 }}>
                            <Table>
                                <TableHead>
                                    <TableRow>
                                        <TableCell>N° Serie</TableCell>
                                        <TableCell>Producto</TableCell>
                                        <TableCell>Cliente</TableCell>
                                        <TableCell>Fecha Reclamo</TableCell>
                                        <TableCell>Estado</TableCell>
                                        <TableCell>Compensación</TableCell>
                                        <TableCell>Garantía</TableCell>
                                        <TableCell>Acciones</TableCell>
                                    </TableRow>
                                </TableHead>
                                <TableBody>
                                    {productos.map((producto) => (
                                        <TableRow key={producto.id}>
                                            <TableCell>
                                                <Typography variant="body2" fontWeight={500}>
                                                    {producto.numeroSerie}
                                                </Typography>
                                            </TableCell>
                                            <TableCell>
                                                <Box>
                                                    <Typography variant="body2" fontWeight={500}>
                                                        {producto.marca} {producto.modelo}
                                                    </Typography>
                                                    <Typography variant="caption" color="text.secondary">
                                                        {producto.especificacion.substring(0, 50)}...
                                                    </Typography>
                                                </Box>
                                            </TableCell>
                                            <TableCell>
                                                <Typography variant="body2">
                                                    {producto.clienteNombre}
                                                </Typography>
                                                <Typography variant="caption" color="text.secondary">
                                                    {producto.clienteRuc}
                                                </Typography>
                                            </TableCell>
                                            <TableCell>
                                                {new Date(producto.fechaReclamoClienteFinal).toLocaleDateString()}
                                            </TableCell>
                                            <TableCell>
                                                {getEstadoChip(producto.estado)}
                                            </TableCell>
                                            <TableCell>
                                                <Tooltip title={producto.formaCompensacion}>
                                                    <IconButton size="small">
                                                        {getFormaCompensacionIcon(producto.formaCompensacion)}
                                                    </IconButton>
                                                </Tooltip>
                                            </TableCell>
                                            <TableCell>
                                                {producto.garantiaValida ? (
                                                    <Chip label="Válida" color="success" size="small" />
                                                ) : (
                                                    <Chip label="Vencida" color="error" size="small" />
                                                )}
                                            </TableCell>
                                            <TableCell>
                                                {producto.estado === 'Pendiente' && (
                                                    <Button
                                                        variant="outlined"
                                                        size="small"
                                                        startIcon={<Visibility />}
                                                        onClick={() => handleIniciarRevision(producto)}
                                                        disabled={producto.id !== proximoProducto?.id}
                                                    >
                                                        Revisar
                                                    </Button>
                                                )}
                                                {producto.estado === 'En Revision' && (
                                                    <Button
                                                        variant="contained"
                                                        size="small"
                                                        color="primary"
                                                        startIcon={<CheckCircle />}
                                                        onClick={() => handleFinalizarRevision(producto)}
                                                    >
                                                        Finalizar
                                                    </Button>
                                                )}
                                                {producto.estado === 'Aprobado' || producto.estado === 'Rechazado' ? (
                                                    <Chip 
                                                        label={producto.estado === 'Aprobado' ? 'Aprobado' : 'Rechazado'} 
                                                        color={producto.estado === 'Aprobado' ? 'success' : 'error'}
                                                        size="small"
                                                    />
                                                ) : null}
                                            </TableCell>
                                        </TableRow>
                                    ))}
                                </TableBody>
                            </Table>
                        </TableContainer>
                    )}
                </CardContent>
            </Card>

            {/* Dialog para iniciar revisión */}
            <Dialog 
                open={showRevisarDialog} 
                onClose={() => !submitting && setShowRevisarDialog(false)}
                maxWidth="md"
                fullWidth
            >
                <DialogTitle>
                    <Typography variant="h6">
                        <Visibility sx={{ verticalAlign: 'middle', mr: 1 }} />
                        Iniciar Revisión Técnica
                    </Typography>
                </DialogTitle>
                <DialogContent>
                    {productoSeleccionado && (
                        <Box sx={{ mt: 2 }}>
                            <Alert 
                                severity={ordenValido ? "success" : "warning"}
                                sx={{ mb: 3 }}
                            >
                                {ordenValido 
                                    ? "Este producto está en el orden correcto para revisión."
                                    : "Validando orden de revisión..."
                                }
                            </Alert>

                            <Grid container spacing={3}>
                                <Grid size={{ xs: 12, md: 6 }}>
                                    <Typography variant="subtitle2" color="text.secondary" gutterBottom>
                                        Información del Producto
                                    </Typography>
                                    <Stack spacing={1}>
                                        <Box sx={{ display: 'flex', alignItems: 'center' }}>
                                            <Receipt sx={{ mr: 1, color: 'text.secondary' }} />
                                            <Typography variant="body2">
                                                <strong>N° Serie:</strong> {productoSeleccionado.numeroSerie}
                                            </Typography>
                                        </Box>
                                        <Box sx={{ display: 'flex', alignItems: 'center' }}>
                                            <Build sx={{ mr: 1, color: 'text.secondary' }} />
                                            <Typography variant="body2">
                                                <strong>Producto:</strong> {productoSeleccionado.marca} {productoSeleccionado.modelo}
                                            </Typography>
                                        </Box>
                                        <Box>
                                            <Typography variant="body2">
                                                <strong>Especificación:</strong>
                                            </Typography>
                                            <Typography variant="body2" color="text.secondary">
                                                {productoSeleccionado.especificacion}
                                            </Typography>
                                        </Box>
                                    </Stack>
                                </Grid>
                                
                                <Grid size={{ xs: 12, md: 6 }}>
                                    <Typography variant="subtitle2" color="text.secondary" gutterBottom>
                                        Información del Cliente
                                    </Typography>
                                    <Stack spacing={1}>
                                        <Box sx={{ display: 'flex', alignItems: 'center' }}>
                                            <Person sx={{ mr: 1, color: 'text.secondary' }} />
                                            <Typography variant="body2">
                                                <strong>Cliente:</strong> {productoSeleccionado.clienteNombre}
                                            </Typography>
                                        </Box>
                                        <Box sx={{ display: 'flex', alignItems: 'center' }}>
                                            <Receipt sx={{ mr: 1, color: 'text.secondary' }} />
                                            <Typography variant="body2">
                                                <strong>RUC:</strong> {productoSeleccionado.clienteRuc}
                                            </Typography>
                                        </Box>
                                        <Box sx={{ display: 'flex', alignItems: 'center' }}>
                                            <CalendarToday sx={{ mr: 1, color: 'text.secondary' }} />
                                            <Typography variant="body2">
                                                <strong>Fecha Reclamo:</strong> {new Date(productoSeleccionado.fechaReclamoClienteFinal).toLocaleDateString()}
                                            </Typography>
                                        </Box>
                                    </Stack>
                                </Grid>
                            </Grid>

                            <Divider sx={{ my: 3 }} />

                            <Alert severity="warning">
                                <Typography variant="subtitle2" gutterBottom>
                                    ¡Atención!
                                </Typography>
                                <Typography variant="body2">
                                    Al iniciar la revisión, el producto pasará a estado "En Revisión" y no podrás 
                                    atender otros productos hasta finalizar esta revisión. Asegúrate de tener 
                                    todo listo antes de continuar.
                                </Typography>
                            </Alert>
                        </Box>
                    )}
                </DialogContent>
                <DialogActions>
                    <Button 
                        onClick={() => setShowRevisarDialog(false)}
                        disabled={submitting}
                    >
                        Cancelar
                    </Button>
                    <Button
                        variant="contained"
                        onClick={confirmarIniciarRevision}
                        disabled={submitting || !ordenValido}
                        startIcon={submitting ? <CircularProgress size={20} /> : <Visibility />}
                    >
                        {submitting ? 'Iniciando...' : 'Iniciar Revisión'}
                    </Button>
                </DialogActions>
            </Dialog>

            {/* Dialog para finalizar revisión */}
            <Dialog 
                open={showFinalizarDialog} 
                onClose={() => !submitting && setShowFinalizarDialog(false)}
                maxWidth="md"
                fullWidth
            >
                <DialogTitle>
                    <Typography variant="h6">
                        <CheckCircle sx={{ verticalAlign: 'middle', mr: 1 }} />
                        Finalizar Revisión Técnica
                    </Typography>
                </DialogTitle>
                <DialogContent>
                    {productoSeleccionado && (
                        <Box sx={{ mt: 2 }}>
                            <Grid container spacing={3}>
                                <Grid size={{ xs: 12 }}>
                                    <Typography variant="subtitle2" color="text.secondary" gutterBottom>
                                        Producto: {productoSeleccionado.marca} {productoSeleccionado.modelo} 
                                        (N° Serie: {productoSeleccionado.numeroSerie})
                                    </Typography>
                                </Grid>

                                <Grid size={{ xs: 12 }}>
                                    <FormControl fullWidth sx={{ mb: 3 }}>
                                        <InputLabel>Resultado de la Revisión</InputLabel>
                                        <Select
                                            value={formData.estado}
                                            label="Resultado de la Revisión"
                                            onChange={(e) => setFormData({...formData, estado: e.target.value})}
                                            disabled={submitting}
                                        >
                                            <MenuItem value="Aprobado">
                                                <CheckCircle sx={{ mr: 1, color: 'success.main' }} />
                                                Aprobado
                                            </MenuItem>
                                            <MenuItem value="Rechazado">
                                                <Cancel sx={{ mr: 1, color: 'error.main' }} />
                                                Rechazado
                                            </MenuItem>
                                        </Select>
                                        <FormHelperText>
                                            Seleccione el resultado de su revisión técnica
                                        </FormHelperText>
                                    </FormControl>
                                </Grid>

                                <Grid size={{ xs: 12 }}>
                                    <TextField
                                        label="Explicación Técnica"
                                        multiline
                                        rows={4}
                                        fullWidth
                                        value={formData.explicacion}
                                        onChange={(e) => setFormData({...formData, explicacion: e.target.value})}
                                        disabled={submitting}
                                        placeholder="Describa detalladamente los hallazgos de la revisión, pruebas realizadas y justificación del resultado..."
                                        helperText="Esta explicación será visible para el cliente y otros departamentos"
                                    />
                                </Grid>

                                <Grid size={{ xs: 12 }}>
                                    <Card variant="outlined">
                                        <CardContent>
                                            <Typography variant="subtitle2" gutterBottom sx={{ display: 'flex', alignItems: 'center' }}>
                                                <Upload sx={{ mr: 1 }} />
                                                Evidencia Técnica (PDF)
                                            </Typography>
                                            <Typography variant="body2" color="text.secondary" paragraph>
                                                Suba un archivo PDF con la evidencia de la revisión (opcional pero recomendado).
                                                Incluya fotos, mediciones, pruebas realizadas, etc.
                                            </Typography>
                                            <Button
                                                variant="outlined"
                                                component="label"
                                                startIcon={<Upload />}
                                                disabled={submitting}
                                            >
                                                Seleccionar PDF
                                                <input
                                                    type="file"
                                                    hidden
                                                    accept=".pdf"
                                                    onChange={(e) => {
                                                        if (e.target.files && e.target.files[0]) {
                                                            setFormData({...formData, archivo: e.target.files[0]});
                                                        }
                                                    }}
                                                />
                                            </Button>
                                            {formData.archivo && (
                                                <Typography variant="body2" sx={{ mt: 2 }}>
                                                    Archivo seleccionado: {formData.archivo.name}
                                                </Typography>
                                            )}
                                        </CardContent>
                                    </Card>
                                </Grid>
                            </Grid>

                            <Alert severity="info" sx={{ mt: 3 }}>
                                <Typography variant="body2">
                                    <strong>Importante:</strong> Una vez finalizada la revisión, no podrás 
                                    modificar el resultado. Asegúrate de que toda la información sea correcta.
                                </Typography>
                            </Alert>
                        </Box>
                    )}
                </DialogContent>
                <DialogActions>
                    <Button 
                        onClick={() => setShowFinalizarDialog(false)}
                        disabled={submitting}
                    >
                        Cancelar
                    </Button>
                    <Button
                        variant="contained"
                        color={formData.estado === 'Aprobado' ? 'success' : 'error'}
                        onClick={confirmarFinalizarRevision}
                        disabled={submitting || !formData.explicacion.trim()}
                        startIcon={submitting ? <CircularProgress size={20} /> : 
                            (formData.estado === 'Aprobado' ? <CheckCircle /> : <Cancel />)}
                    >
                        {submitting ? 'Procesando...' : 
                            `Finalizar como ${formData.estado === 'Aprobado' ? 'Aprobado' : 'Rechazado'}`}
                    </Button>
                </DialogActions>
            </Dialog>
        </Box>
    );
};

export default TecnicoDashboard;
