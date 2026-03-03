import React, { useState, useEffect } from 'react';
import {
    Box,
    Typography,
    Card,
    CardContent,
    Grid,
    Button,
    Alert,
    CircularProgress,
    Divider,
    List,
    ListItem,
    ListItemIcon,
    ListItemText,
    Chip,
    Stack,
    TextField,
    FormControl,
    InputLabel,
    Select,
    MenuItem,
    FormHelperText,
} from '@mui/material';
import {
    Build,
    CalendarToday,
    Person,
    Receipt,
    Paid,
    Engineering,
    Warning,
    CheckCircle,
    Cancel,
    Upload,
    ArrowBack
} from '@mui/icons-material';
import { useParams, useNavigate } from 'react-router-dom';
import { useAuth } from '../../services/authContext';
import { tecnicoService } from '../../services/tecnicoService';
import { type TecnicoProducto } from '../../src/types/tecnico';

const RevisarProducto: React.FC = () => {
    const { id } = useParams<{ id: string }>();
    const navigate = useNavigate();
    const { auth } = useAuth();
    const [producto, setProducto] = useState<TecnicoProducto | null>(null);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState('');
    const [formData, setFormData] = useState({
        estado: 'Aprobado',
        explicacion: '',
        archivo: null as File | null
    });
    const [submitting, setSubmitting] = useState(false);
    const [revisionIniciada, setRevisionIniciada] = useState(false);

    useEffect(() => {
        cargarProducto();
    }, [id]);

    const cargarProducto = async () => {
        try {
            setLoading(true);
            setError('');
            
            // En un caso real, obtendríamos el producto específico
            // Por ahora, simulamos obteniendo todos y filtrando
            const productos = await tecnicoService.obtenerProductosAsignados();
            const productoEncontrado = productos.find(p => p.id === parseInt(id || '0'));
            
            if (!productoEncontrado) {
                throw new Error('Producto no encontrado');
            }
            
            setProducto(productoEncontrado);
            setRevisionIniciada(productoEncontrado.estado === 'En Revision');
            
        } catch (err: any) {
            setError(err.message);
        } finally {
            setLoading(false);
        }
    };

    const handleIniciarRevision = async () => {
        try {
            if (!producto) return;
            
            setSubmitting(true);
            
            await tecnicoService.iniciarRevision({
                reclamoProductoSnId: producto.id,
                tecnicoId: auth.user?.id || 0
            });
            
            setRevisionIniciada(true);
            await cargarProducto();
            
        } catch (err: any) {
            setError(err.message);
        } finally {
            setSubmitting(false);
        }
    };

    const handleFinalizarRevision = async () => {
        try {
            if (!producto) return;

            // Validaciones
            if (!formData.explicacion.trim()) {
                setError('La explicación es requerida');
                return;
            }

            setSubmitting(true);
            setError('');

            // Convertir archivo a base64 si existe
            let pdfBase64 = undefined;
            let pdfFileName = undefined;

            if (formData.archivo) {
                pdfBase64 = await tecnicoService.convertirArchivoABase64(formData.archivo);
                pdfFileName = formData.archivo.name;
            }

            await tecnicoService.finalizarRevision({
                reclamoProductoSnId: producto.id,
                tecnicoId: auth.user?.id || 0,
                estado: formData.estado as 'Aprobado' | 'Rechazado',
                explicacion: formData.explicacion,
                pdfBase64,
                pdfFileName
            });

            // Redirigir al dashboard
            navigate('/tecnico');
            
        } catch (err: any) {
            setError(err.message);
        } finally {
            setSubmitting(false);
        }
    };

    const getEstadoColor = (estado: string) => {
        switch (estado) {
            case 'Pendiente': return 'warning';
            case 'En Revision': return 'info';
            case 'Aprobado': return 'success';
            case 'Rechazado': return 'error';
            default: return 'default';
        }
    };

    if (loading) {
        return (
            <Box sx={{ display: 'flex', justifyContent: 'center', alignItems: 'center', minHeight: '400px' }}>
                <CircularProgress />
            </Box>
        );
    }

    if (!producto) {
        return (
            <Box sx={{ p: 3 }}>
                <Alert severity="error">
                    Producto no encontrado
                </Alert>
                <Button
                    startIcon={<ArrowBack />}
                    onClick={() => navigate('/app/tecnico')}
                    sx={{ mt: 2 }}
                >
                    Volver al Dashboard
                </Button>
            </Box>
        );
    }

    return (
        <Box sx={{ p: 3 }}>
            {/* Header */}
            <Box sx={{ mb: 4 }}>
                <Button
                    startIcon={<ArrowBack />}
                    onClick={() => navigate('/app/tecnico')}
                    sx={{ mb: 2 }}
                >
                    Volver
                </Button>
                
                <Typography variant="h4" fontWeight={600} gutterBottom>
                    <Engineering sx={{ verticalAlign: 'middle', mr: 2 }} />
                    Revisión Técnica
                </Typography>
                
                <Stack direction="row" spacing={2} alignItems="center">
                    <Chip 
                        label={producto.estado}
                        color={getEstadoColor(producto.estado) as any}
                        size="medium"
                    />
                    <Typography variant="body1" color="text.secondary">
                        {producto.marca} {producto.modelo} - {producto.numeroSerie}
                    </Typography>
                </Stack>
            </Box>

            {error && (
                <Alert severity="error" sx={{ mb: 3 }} onClose={() => setError('')}>
                    {error}
                </Alert>
            )}

            <Grid container spacing={3}>
          {/* Columna izquierda: Información del producto */}
          <Grid size={{xs: 12, md: 6}}>
                    <Card sx={{ mb: 3 }}>
                        <CardContent>
                            <Typography variant="h6" gutterBottom sx={{ display: 'flex', alignItems: 'center' }}>
                                <Build sx={{ mr: 1 }} />
                                Información del Producto
                            </Typography>
                            
                            <List>
                                <ListItem>
                                    <ListItemIcon>
                                        <Receipt />
                                    </ListItemIcon>
                                    <ListItemText 
                                        primary="Número de Serie"
                                        secondary={producto.numeroSerie}
                                    />
                                </ListItem>
                                
                                <ListItem>
                                    <ListItemIcon>
                                        <Build />
                                    </ListItemIcon>
                                    <ListItemText 
                                        primary="Producto"
                                        secondary={`${producto.marca} ${producto.modelo}`}
                                    />
                                </ListItem>
                                
                                <ListItem>
                                    <ListItemText 
                                        primary="Especificación"
                                        secondary={producto.especificacion}
                                        primaryTypographyProps={{ variant: 'body2', color: 'text.secondary' }}
                                        secondaryTypographyProps={{ variant: 'body1' }}
                                    />
                                </ListItem>
                                
                                <ListItem>
                                    <ListItemIcon>
                                        {producto.formaCompensacion === 'Reembolso' ? <Paid /> : <Build />}
                                    </ListItemIcon>
                                    <ListItemText 
                                        primary="Forma de Compensación Solicitada"
                                        secondary={producto.formaCompensacion}
                                    />
                                </ListItem>
                                
                                <ListItem>
                                    <ListItemText 
                                        primary="Precio"
                                        secondary={`$${producto.precio.toFixed(2)}`}
                                        primaryTypographyProps={{ variant: 'body2', color: 'text.secondary' }}
                                        secondaryTypographyProps={{ variant: 'body1' }}
                                    />
                                </ListItem>
                            </List>
                        </CardContent>
                    </Card>
                    
                    <Card>
                        <CardContent>
                            <Typography variant="h6" gutterBottom sx={{ display: 'flex', alignItems: 'center' }}>
                                <Person sx={{ mr: 1 }} />
                                Información del Cliente
                            </Typography>
                            
                            <List>
                                <ListItem>
                                    <ListItemIcon>
                                        <Person />
                                    </ListItemIcon>
                                    <ListItemText 
                                        primary="Cliente"
                                        secondary={producto.clienteNombre}
                                    />
                                </ListItem>
                                
                                <ListItem>
                                    <ListItemIcon>
                                        <Receipt />
                                    </ListItemIcon>
                                    <ListItemText 
                                        primary="RUC"
                                        secondary={producto.clienteRuc}
                                    />
                                </ListItem>
                                
                                <ListItem>
                                    <ListItemIcon>
                                        <CalendarToday />
                                    </ListItemIcon>
                                    <ListItemText 
                                        primary="Fecha de Reclamo"
                                        secondary={new Date(producto.fechaReclamoClienteFinal).toLocaleDateString()}
                                    />
                                </ListItem>
                                
                                <ListItem>
                                    <ListItemIcon>
                                        <CalendarToday />
                                    </ListItemIcon>
                                    <ListItemText 
                                        primary="Fecha de Venta"
                                        secondary={new Date(producto.fechaVentaClienteFinal).toLocaleDateString()}
                                    />
                                </ListItem>
                                
                                <ListItem>
                                    <ListItemText 
                                        primary="Garantía"
                                        secondary={
                                            <Stack direction="row" spacing={1} alignItems="center">
                                                <Chip 
                                                    label={producto.garantiaValida ? 'Válida' : 'Vencida'}
                                                    color={producto.garantiaValida ? 'success' : 'error'}
                                                    size="small"
                                                />
                                                <Typography variant="body2" color="text.secondary">
                                                    ({producto.diasGarantia} días)
                                                </Typography>
                                            </Stack>
                                        }
                                        primaryTypographyProps={{ variant: 'body2', color: 'text.secondary' }}
                                    />
                                </ListItem>
                            </List>
                        </CardContent>
                    </Card>
                </Grid>

          {/* Columna derecha: Formulario de revisión */}
                <Grid size={{ xs: 12, md: 6 }}>
                    {!revisionIniciada ? (
                        <Card>
                            <CardContent>
                                <Typography variant="h6" gutterBottom color="warning.main">
                                    <Warning sx={{ verticalAlign: 'middle', mr: 1 }} />
                                    Iniciar Revisión
                                </Typography>
                                
                                <Alert severity="warning" sx={{ mb: 3 }}>
                                    <Typography variant="body2">
                                        <strong>Importante:</strong> Al iniciar la revisión, este producto 
                                        pasará a estado "En Revisión" y no podrás atender otros productos 
                                        hasta finalizar esta revisión.
                                    </Typography>
                                </Alert>
                                
                                <Box sx={{ textAlign: 'center', py: 4 }}>
                                    <Engineering sx={{ fontSize: 60, color: 'warning.main', mb: 2 }} />
                                    <Typography variant="body1" paragraph>
                                        ¿Estás listo para comenzar la revisión técnica de este producto?
                                    </Typography>
                                    <Typography variant="body2" color="text.secondary" paragraph>
                                        Asegúrate de tener todo el equipo necesario y el área de trabajo preparada.
                                    </Typography>
                                    
                                    <Button
                                        variant="contained"
                                        color="warning"
                                        size="large"
                                        onClick={handleIniciarRevision}
                                        disabled={submitting}
                                        startIcon={submitting ? <CircularProgress size={20} /> : <Engineering />}
                                    >
                                        {submitting ? 'Iniciando...' : 'Iniciar Revisión Técnica'}
                                    </Button>
                                </Box>
                            </CardContent>
                        </Card>
                    ) : (
                        <Card>
                            <CardContent>
                                <Typography variant="h6" gutterBottom>
                                    <Engineering sx={{ verticalAlign: 'middle', mr: 1 }} />
                                    Revisión en Progreso
                                </Typography>
                                
                                <Alert severity="info" sx={{ mb: 3 }}>
                                    <Typography variant="body2">
                                        <strong>Estado:</strong> Revisión en progreso. No puedes salir de esta 
                                        revisión hasta completarla.
                                    </Typography>
                                </Alert>
                                
                                <Box sx={{ mb: 3 }}>
                                    <FormControl fullWidth>
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
                                            Seleccione el resultado basado en su evaluación técnica
                                        </FormHelperText>
                                    </FormControl>
                                </Box>
                                
                                <Box sx={{ mb: 3 }}>
                                    <TextField
                                        label="Explicación Técnica Detallada"
                                        multiline
                                        rows={6}
                                        fullWidth
                                        value={formData.explicacion}
                                        onChange={(e) => setFormData({...formData, explicacion: e.target.value})}
                                        disabled={submitting}
                                        placeholder="Describa detalladamente:
                                        1. Pruebas realizadas
                                        2. Hallazgos encontrados
                                        3. Mediciones obtenidas
                                        4. Justificación del resultado
                                        5. Recomendaciones (si aplica)"
                                        helperText="Esta explicación será parte del historial permanente del reclamo"
                                    />
                                </Box>
                                
                                <Card variant="outlined" sx={{ mb: 3 }}>
                                    <CardContent>
                                        <Typography variant="subtitle2" gutterBottom sx={{ display: 'flex', alignItems: 'center' }}>
                                            <Upload sx={{ mr: 1 }} />
                                            Evidencia Técnica (PDF Opcional)
                                        </Typography>
                                        <Typography variant="body2" color="text.secondary" paragraph>
                                            Puede adjuntar un informe técnico en PDF con fotos, mediciones, 
                                            resultados de pruebas, etc.
                                        </Typography>
                                        <Button
                                            variant="outlined"
                                            component="label"
                                            startIcon={<Upload />}
                                            disabled={submitting}
                                            fullWidth
                                        >
                                            {formData.archivo ? 'Cambiar PDF' : 'Seleccionar PDF'}
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
                                            <Typography variant="body2" sx={{ mt: 2, textAlign: 'center' }}>
                                                📄 {formData.archivo.name}
                                            </Typography>
                                        )}
                                    </CardContent>
                                </Card>
                                
                                <Divider sx={{ my: 2 }} />
                                
                                <Box sx={{ textAlign: 'center' }}>
                                    <Button
                                        variant="contained"
                                        color={formData.estado === 'Aprobado' ? 'success' : 'error'}
                                        size="large"
                                        onClick={handleFinalizarRevision}
                                        disabled={submitting || !formData.explicacion.trim()}
                                        startIcon={submitting ? <CircularProgress size={20} /> : 
                                            (formData.estado === 'Aprobado' ? <CheckCircle /> : <Cancel />)}
                                        sx={{ minWidth: 200 }}
                                    >
                                        {submitting ? 'Procesando...' : 
                                            `Finalizar como ${formData.estado === 'Aprobado' ? 'Aprobado' : 'Rechazado'}`}
                                    </Button>
                                    
                                    <Typography variant="caption" color="text.secondary" sx={{ display: 'block', mt: 2 }}>
                                        Al finalizar, no podrás modificar esta revisión
                                    </Typography>
                                </Box>
                            </CardContent>
                        </Card>
                    )}
                </Grid>
            </Grid>
        </Box>
    );
};

export default RevisarProducto;
