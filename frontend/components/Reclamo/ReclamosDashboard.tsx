import React, { useState, useEffect, useCallback } from 'react';
import {
    Box,
    Card,
    CardContent,
    Typography,
    Grid,
    TextField,
    Button,
    Chip,
    CircularProgress,
    Alert,
    Collapse,
    IconButton,
    Dialog,
    DialogTitle,
    DialogContent,
    DialogActions,
    Table,
    TableBody,
    TableCell,
    TableContainer,
    TableHead,
    TableRow,
    Paper,
    Accordion,
    AccordionSummary,
    AccordionDetails,
    FormControl,
    InputLabel,
    Select,
    MenuItem,
    InputAdornment,
    Tabs,
    Tab,
    Divider,
    Tooltip,
} from '@mui/material';
import {
    Search,
    FilterList,
    ExpandMore,
    ExpandLess,
    PictureAsPdf,
    Download,
    Visibility,
    Receipt,
    LocalShipping,
    CheckCircle,
    Pending,
    Build,
    Block,
    Paid,
    Refresh,
    Clear,
    Assessment,
    Timeline,
    Info
} from '@mui/icons-material';
import { clienteService } from '../../services/clienteService';
import {
    type ClienteDashboardResponse,
    type ClienteProductoDTO
} from '../../src/types/cliente';
import { DatePicker } from '@mui/x-date-pickers/DatePicker';
import { LocalizationProvider } from '@mui/x-date-pickers/LocalizationProvider';
import { AdapterDateFns } from '@mui/x-date-pickers/AdapterDateFns';
import { es } from 'date-fns/locale';

const ReclamosDashboard: React.FC = () => {
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState<string | null>(null);
    const [data, setData] = useState<ClienteDashboardResponse | null>(null);
    const [filtros, setFiltros] = useState({
        codigoReclamo: '',
        numeroSerie: '',
        tipoReclamo: '',
        estado: '',
        fechaDesde: null as Date | null,
        fechaHasta: null as Date | null,
        soloPendientes: false,
        soloAprobados: false,
        soloCompensados: false,
        soloReembolsos: false,
        soloReemplazos: false
    });
    const [showFiltros, setShowFiltros] = useState(false);
    const [expandedReclamo, setExpandedReclamo] = useState<number | null>(null);
    const [expandedProductos, setExpandedProductos] = useState<number[]>([]);
    const [pdfDialog, setPdfDialog] = useState<{
        open: boolean;
        tipo: 'tecnico' | 'entrega';
        nombreArchivo: string;
        base64?: string;
    }>({ open: false, tipo: 'tecnico', nombreArchivo: '' });
    const [activeTab, setActiveTab] = useState(0);

    const cargarDashboard = useCallback(async () => {
        try {
            setLoading(true);
            setError(null);

            const request = {
                ...filtros,
                fechaDesde: filtros.fechaDesde?.toISOString(),
                fechaHasta: filtros.fechaHasta?.toISOString()
            };

            const dashboardData = await clienteService.obtenerDashboard(request);
            setData(dashboardData);
        } catch (err: any) {
            setError(err.message || 'Error al cargar el dashboard');
        } finally {
            setLoading(false);
        }
    }, [filtros]);

    useEffect(() => {
        cargarDashboard();
    }, [cargarDashboard]);

    const handleFiltroChange = (campo: string, valor: any) => {
        setFiltros(prev => ({ ...prev, [campo]: valor }));
    };

    const limpiarFiltros = () => {
        setFiltros({
            codigoReclamo: '',
            numeroSerie: '',
            tipoReclamo: '',
            estado: '',
            fechaDesde: null,
            fechaHasta: null,
            soloPendientes: false,
            soloAprobados: false,
            soloCompensados: false,
            soloReembolsos: false,
            soloReemplazos: false
        });
    };

    const toggleReclamoExpand = (reclamoId: number) => {
        setExpandedReclamo(prev => prev === reclamoId ? null : reclamoId);
    };

    const toggleProductoExpand = (productoId: number) => {
        setExpandedProductos(prev =>
            prev.includes(productoId)
                ? prev.filter(id => id !== productoId)
                : [...prev, productoId]
        );
    };

    const abrirPdf = async (tipo: 'tecnico' | 'entrega', nombreArchivo: string) => {
        try {
            const pdfData = await clienteService.obtenerPdf(tipo, nombreArchivo);
            setPdfDialog({
                open: true,
                tipo,
                nombreArchivo,
                base64: pdfData.pdfBase64
            });
        } catch (err: any) {
            setError(err.message || 'Error al cargar el PDF');
        }
    };

    const descargarPdf = () => {
        if (pdfDialog.base64) {
            clienteService.descargarPdf(pdfDialog.base64, pdfDialog.nombreArchivo);
        }
    };

    const verPdfEnVentana = () => {
        if (pdfDialog.base64) {
            clienteService.verPdfEnNuevaVentana(pdfDialog.base64, pdfDialog.nombreArchivo);
        }
    };

    const getEstadoIcon = (estado: string) => {
        switch (estado) {
            case 'Pendiente': return <Pending color="warning" />;
            case 'En Revision': return <Build color="info" />;
            case 'Aprobado': return <CheckCircle color="success" />;
            case 'Rechazado': return <Block color="error" />;
            case 'Compensado': return <Paid color="secondary" />;
            default: return <Info color="action" />;
        }
    };

    const renderEstadisticas = () => {
        if (!data) return null;

        const stats = data.estadisticas;
        const statItems = [
            { label: 'Total Reclamos', value: stats.totalReclamos, icon: <Assessment />, color: 'primary' },
            { label: 'Pendientes', value: stats.productosPendientes, icon: <Pending />, color: 'warning' },
            { label: 'En Revisión', value: stats.productosEnRevision, icon: <Build />, color: 'info' },
            { label: 'Aprobados', value: stats.productosAprobados, icon: <CheckCircle />, color: 'success' },
            { label: 'Rechazados', value: stats.productosRechazados, icon: <Block />, color: 'error' },
            { label: 'Compensados', value: stats.productosCompensados, icon: <Paid />, color: 'secondary' },
            { label: 'Reembolsos', value: stats.reembolsosTotales, icon: <Receipt />, color: 'primary' },
            { label: 'Reemplazos', value: stats.reemplazosTotales, icon: <LocalShipping />, color: 'secondary' }
        ];

        return (
            <Grid container spacing={2} sx={{ mb: 3 }}>
                {statItems.map((stat, index) => (
                    <Grid size={{ xs: 6, sm: 4, md: 3 }} key={index}>
                        <Card sx={{ height: '100%' }}>
                            <CardContent sx={{ textAlign: 'center' }}>
                                <Box sx={{ color: `${stat.color}.main`, mb: 1 }}>
                                    {React.cloneElement(stat.icon, { fontSize: 'large' })}
                                </Box>
                                <Typography variant="h4" fontWeight="bold">
                                    {stat.value}
                                </Typography>
                                <Typography variant="body2" color="text.secondary">
                                    {stat.label}
                                </Typography>
                            </CardContent>
                        </Card>
                    </Grid>
                ))}
            </Grid>
        );
    };

    const renderProductoDetalle = (producto: ClienteProductoDTO) => {
        const showTecnicoInfo = ['Aprobado', 'Rechazado', 'Compensado'].includes(producto.estado);
        const showCompensacionInfo = producto.estado === 'Compensado' && producto.compensacion;

        return (
            <Box sx={{ mt: 2, p: 2, bgcolor: 'background.default', borderRadius: 1 }}>
                <Grid container spacing={2}>
                    {showTecnicoInfo && (
                        <>
                            <Grid size={{ xs: 12 }}>
                                <Typography variant="subtitle2" gutterBottom color="primary">
                                    <Build sx={{ verticalAlign: 'middle', mr: 1 }} />
                                    Información del Técnico
                                </Typography>
                                <Divider sx={{ mb: 2 }} />
                            </Grid>

                            <Grid size={{ xs: 12, md: 6 }}>
                                <Typography variant="body2" color="text.secondary">
                                    Técnico Asignado:
                                </Typography>
                                <Typography variant="body1">
                                    {producto.tecnicoNombre || 'No asignado'}
                                </Typography>
                            </Grid>

                            <Grid size={{ xs: 12, md: 6 }}>
                                <Typography variant="body2" color="text.secondary">
                                    Fecha de Revisión:
                                </Typography>
                                <Typography variant="body1">
                                    {clienteService.formatearFecha(producto.fechaRevisionTecnico)}
                                </Typography>
                            </Grid>

                            <Grid size={{ xs: 12 }}>
                                <Typography variant="body2" color="text.secondary">
                                    Explicación del Técnico:
                                </Typography>
                                <Typography variant="body1" sx={{ mt: 1 }}>
                                    {producto.explicacionRespuestaTecnico || 'Sin explicación'}
                                </Typography>
                            </Grid>

                            {producto.pdfRevisionTecnico && (
                                <Grid size={{ xs: 12 }}>
                                    <Button
                                        variant="outlined"
                                        startIcon={<PictureAsPdf />}
                                        onClick={() => abrirPdf('tecnico', producto.pdfRevisionTecnico!)}
                                        sx={{ mr: 2 }}
                                    >
                                        Ver PDF de Revisión
                                    </Button>
                                </Grid>
                            )}
                        </>
                    )}

                    {showCompensacionInfo && (
                        <>
                            <Grid size={{ xs: 12 }} sx={{ mt: 2 }}>
                                <Typography variant="subtitle2" gutterBottom color="secondary">
                                    {producto.compensacion?.tipo === 'Reembolso' ? (
                                        <><Receipt sx={{ verticalAlign: 'middle', mr: 1 }} />Información de Reembolso</>
                                    ) : (
                                        <><LocalShipping sx={{ verticalAlign: 'middle', mr: 1 }} />Información de Reemplazo</>
                                    )}
                                </Typography>
                                <Divider sx={{ mb: 2 }} />
                            </Grid>

                            {producto.compensacion?.tipo === 'Reembolso' ? (
                                <>
                                    <Grid size={{ xs: 12, md: 4 }}>
                                        <Typography variant="body2" color="text.secondary">
                                            N° Comprobante:
                                        </Typography>
                                        <Typography variant="body1">
                                            {producto.compensacion.numeroComprobanteReembolso}
                                        </Typography>
                                    </Grid>
                                    <Grid size={{ xs: 12, md: 4 }}>
                                        <Typography variant="body2" color="text.secondary">
                                            Fecha Reembolso:
                                        </Typography>
                                        <Typography variant="body1">
                                            {clienteService.formatearFecha(producto.compensacion.fechaReembolso)}
                                        </Typography>
                                    </Grid>
                                    <Grid size={{ xs: 12, md: 4 }}>
                                        <Typography variant="body2" color="text.secondary">
                                            Cuenta Bancaria:
                                        </Typography>
                                        <Typography variant="body1">
                                            {producto.compensacion.numCuentaBancariaReembolso}
                                        </Typography>
                                    </Grid>
                                </>
                            ) : (
                                <>
                                    <Grid size={{ xs: 12, md: 6 }}>
                                        <Typography variant="body2" color="text.secondary">
                                            Producto de Reemplazo:
                                        </Typography>
                                        <Typography variant="body1">
                                            {producto.compensacion?.numeroSerieReemplazo}
                                        </Typography>
                                    </Grid>
                                    <Grid size={{ xs: 12, md: 6 }}>
                                        <Typography variant="body2" color="text.secondary">
                                            Personal de Entrega:
                                        </Typography>
                                        <Typography variant="body1">
                                            {producto.compensacion?.personalEntregaNombre}
                                        </Typography>
                                    </Grid>
                                    {producto.compensacion?.pdfComprobanteEntrega && (
                                        <Grid size={{ xs: 12 }}>
                                            <Button
                                                variant="outlined"
                                                startIcon={<PictureAsPdf />}
                                                onClick={() => abrirPdf('entrega', producto.compensacion!.pdfComprobanteEntrega!)}
                                            >
                                                Ver Comprobante de Entrega
                                            </Button>
                                        </Grid>
                                    )}
                                </>
                            )}
                        </>
                    )}
                </Grid>
            </Box>
        );
    };

    const renderReclamos = () => {
        if (!data || data.reclamos.length === 0) {
            return (
                <Alert severity="info" sx={{ mt: 2 }}>
                    No se encontraron reclamos con los filtros aplicados.
                </Alert>
            );
        }

        return (
            <Box sx={{ mt: 3 }}>
                {data.reclamos.map((reclamo) => (
                    <Accordion
                        key={reclamo.reclamoId}
                        expanded={expandedReclamo === reclamo.reclamoId}
                        onChange={() => toggleReclamoExpand(reclamo.reclamoId)}
                        sx={{ mb: 2 }}
                    >
                        <AccordionSummary expandIcon={<ExpandMore />}>
                            <Box sx={{ display: 'flex', alignItems: 'center', flexGrow: 1 }}>
                                <Box sx={{ flexGrow: 1 }}>
                                    <Typography variant="h6">
                                        {reclamo.codigoReclamo}
                                    </Typography>
                                    <Typography variant="body2" color="text.secondary">
                                        Fecha: {clienteService.formatearFecha(reclamo.fechaCreacion)} |
                                        Productos: {reclamo.productos.length}
                                    </Typography>
                                </Box>
                                <Chip
                                    label={`${reclamo.productos.length} productos`}
                                    size="small"
                                    sx={{ ml: 2 }}
                                />
                            </Box>
                        </AccordionSummary>

                        <AccordionDetails>
                            <TableContainer component={Paper} variant="outlined">
                                <Table>
                                    <TableHead>
                                        <TableRow>
                                            <TableCell>Producto</TableCell>
                                            <TableCell>Estado</TableCell>
                                            <TableCell>Técnico</TableCell>
                                            <TableCell>Fecha Revisión</TableCell>
                                            <TableCell>Acciones</TableCell>
                                        </TableRow>
                                    </TableHead>
                                    <TableBody>
                                        {reclamo.productos.map((producto) => (
                                            <React.Fragment key={producto.reclamoProductoId}>
                                                <TableRow>
                                                    <TableCell>
                                                        <Box>
                                                            <Typography variant="body2" fontWeight="bold">
                                                                {producto.marca} {producto.modelo}
                                                            </Typography>
                                                            <Typography variant="caption" color="text.secondary">
                                                                Serie: {producto.numeroSerie} |
                                                                Tipo: <Chip
                                                                    label={producto.tipoReclamo}
                                                                    size="small"
                                                                    color={clienteService.getTipoReclamoColor(producto.tipoReclamo) as any}
                                                                    sx={{ ml: 1 }}
                                                                />
                                                            </Typography>
                                                        </Box>
                                                    </TableCell>
                                                    <TableCell>
                                                        <Chip
                                                            icon={getEstadoIcon(producto.estado)}
                                                            label={producto.estado}
                                                            color={clienteService.getEstadoColor(producto.estado) as any}
                                                            size="small"
                                                        />
                                                    </TableCell>
                                                    <TableCell>
                                                        {producto.tecnicoNombre || '-'}
                                                    </TableCell>
                                                    <TableCell>
                                                        {clienteService.formatearFecha(producto.fechaRevisionTecnico)}
                                                    </TableCell>
                                                    <TableCell>
                                                        <Tooltip title="Ver detalles">
                                                            <IconButton
                                                                size="small"
                                                                onClick={() => toggleProductoExpand(producto.reclamoProductoId)}
                                                            >
                                                                {expandedProductos.includes(producto.reclamoProductoId) ?
                                                                    <ExpandLess /> : <ExpandMore />
                                                                }
                                                            </IconButton>
                                                        </Tooltip>
                                                    </TableCell>
                                                </TableRow>

                                                {expandedProductos.includes(producto.reclamoProductoId) && (
                                                    <TableRow>
                                                        <TableCell colSpan={5}>
                                                            {renderProductoDetalle(producto)}
                                                        </TableCell>
                                                    </TableRow>
                                                )}
                                            </React.Fragment>
                                        ))}
                                    </TableBody>
                                </Table>
                            </TableContainer>
                        </AccordionDetails>
                    </Accordion>
                ))}
            </Box>
        );
    };

    if (loading) {
        return (
            <Box sx={{ display: 'flex', justifyContent: 'center', alignItems: 'center', height: '50vh' }}>
                <CircularProgress />
            </Box>
        );
    }

    return (
        <LocalizationProvider dateAdapter={AdapterDateFns} adapterLocale={es}>
            <Box>
                <Box sx={{ mb: 3 }}>
                    <Typography variant="h4" fontWeight="bold" gutterBottom>
                        Dashboard de Reclamos
                    </Typography>
                    <Typography variant="body1" color="text.secondary">
                        Visualiza y gestiona todos tus reclamos de productos
                    </Typography>
                </Box>

                <Card sx={{ mb: 3 }}>
                    <CardContent>
                        <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 2 }}>
                            <Typography variant="h6">
                                <FilterList sx={{ verticalAlign: 'middle', mr: 1 }} />
                                Filtros de Búsqueda
                            </Typography>
                            <Box>
                                <Button
                                    variant="outlined"
                                    onClick={() => setShowFiltros(!showFiltros)}
                                    startIcon={showFiltros ? <ExpandLess /> : <ExpandMore />}
                                    sx={{ mr: 2 }}
                                >
                                    {showFiltros ? 'Ocultar' : 'Mostrar'} Filtros
                                </Button>
                                <Button
                                    variant="contained"
                                    onClick={cargarDashboard}
                                    startIcon={<Refresh />}
                                    sx={{ mr: 2 }}
                                >
                                    Actualizar
                                </Button>
                                <Button
                                    variant="outlined"
                                    onClick={limpiarFiltros}
                                    startIcon={<Clear />}
                                >
                                    Limpiar
                                </Button>
                            </Box>
                        </Box>

                        <Collapse in={showFiltros}>
                            <Grid container spacing={2}>
                                <Grid size={{ xs: 12, sm: 6, md: 3 }}>
                                    <TextField
                                        label="Código de Reclamo"
                                        value={filtros.codigoReclamo}
                                        onChange={(e) => handleFiltroChange('codigoReclamo', e.target.value)}
                                        fullWidth
                                        size="small"
                                        InputProps={{
                                            startAdornment: (
                                                <InputAdornment position="start">
                                                    <Search />
                                                </InputAdornment>
                                            ),
                                        }}
                                    />
                                </Grid>

                                <Grid size={{ xs: 12, sm: 6, md: 3 }}>
                                    <TextField
                                        label="Número de Serie"
                                        value={filtros.numeroSerie}
                                        onChange={(e) => handleFiltroChange('numeroSerie', e.target.value)}
                                        fullWidth
                                        size="small"
                                    />
                                </Grid>

                                <Grid size={{ xs: 12, sm: 6, md: 3 }}>
                                    <FormControl fullWidth size="small">
                                        <InputLabel>Tipo de Reclamo</InputLabel>
                                        <Select
                                            value={filtros.tipoReclamo}
                                            label="Tipo de Reclamo"
                                            onChange={(e) => handleFiltroChange('tipoReclamo', e.target.value)}
                                        >
                                            <MenuItem value="">Todos</MenuItem>
                                            <MenuItem value="Reembolso">Reembolso</MenuItem>
                                            <MenuItem value="Reemplazo">Reemplazo</MenuItem>
                                        </Select>
                                    </FormControl>
                                </Grid>

                                <Grid size={{ xs: 12, sm: 6, md: 3 }}>
                                    <FormControl fullWidth size="small">
                                        <InputLabel>Estado</InputLabel>
                                        <Select
                                            value={filtros.estado}
                                            label="Estado"
                                            onChange={(e) => handleFiltroChange('estado', e.target.value)}
                                        >
                                            <MenuItem value="">Todos</MenuItem>
                                            <MenuItem value="Pendiente">Pendiente</MenuItem>
                                            <MenuItem value="En Revision">En Revisión</MenuItem>
                                            <MenuItem value="Aprobado">Aprobado</MenuItem>
                                            <MenuItem value="Rechazado">Rechazado</MenuItem>
                                            <MenuItem value="Compensado">Compensado</MenuItem>
                                        </Select>
                                    </FormControl>
                                </Grid>

                                <Grid size={{ xs: 12, sm: 6 }}>
                                    <DatePicker
                                        label="Fecha Desde"
                                        value={filtros.fechaDesde}
                                        onChange={(date) => handleFiltroChange('fechaDesde', date)}
                                        slotProps={{ textField: { fullWidth: true, size: 'small' } }}
                                    />
                                </Grid>

                                <Grid size={{ xs: 12, sm: 6 }}>
                                    <DatePicker
                                        label="Fecha Hasta"
                                        value={filtros.fechaHasta}
                                        onChange={(date) => handleFiltroChange('fechaHasta', date)}
                                        slotProps={{ textField: { fullWidth: true, size: 'small' } }}
                                    />
                                </Grid>

                                <Grid size={{ xs: 12 }}>
                                    <Box sx={{ display: 'flex', flexWrap: 'wrap', gap: 1 }}>
                                        <Chip
                                            label="Solo Pendientes"
                                            color={filtros.soloPendientes ? "primary" : "default"}
                                            onClick={() => handleFiltroChange('soloPendientes', !filtros.soloPendientes)}
                                            clickable
                                        />
                                        <Chip
                                            label="Solo Aprobados"
                                            color={filtros.soloAprobados ? "primary" : "default"}
                                            onClick={() => handleFiltroChange('soloAprobados', !filtros.soloAprobados)}
                                            clickable
                                        />
                                        <Chip
                                            label="Solo Compensados"
                                            color={filtros.soloCompensados ? "primary" : "default"}
                                            onClick={() => handleFiltroChange('soloCompensados', !filtros.soloCompensados)}
                                            clickable
                                        />
                                        <Chip
                                            label="Solo Reembolsos"
                                            color={filtros.soloReembolsos ? "primary" : "default"}
                                            onClick={() => handleFiltroChange('soloReembolsos', !filtros.soloReembolsos)}
                                            clickable
                                        />
                                        <Chip
                                            label="Solo Reemplazos"
                                            color={filtros.soloReemplazos ? "primary" : "default"}
                                            onClick={() => handleFiltroChange('soloReemplazos', !filtros.soloReemplazos)}
                                            clickable
                                        />
                                    </Box>
                                </Grid>
                            </Grid>
                        </Collapse>
                    </CardContent>
                </Card>

                {error && (
                    <Alert severity="error" sx={{ mb: 3 }}>
                        {error}
                    </Alert>
                )}

                {data && renderEstadisticas()}

                <Card>
                    <CardContent>
                        <Tabs value={activeTab} onChange={(_, newValue) => setActiveTab(newValue)} sx={{ mb: 3 }}>
                            <Tab icon={<Timeline />} label="Vista General" />
                            <Tab icon={<Assessment />} label="Reclamos Detallados" />
                        </Tabs>

                        {activeTab === 0 ? (
                            <Box>
                                <Typography variant="h6" gutterBottom>
                                    Resumen de Reclamos
                                </Typography>
                                {data && data.reclamos.slice(0, 3).map((reclamo) => (
                                    <Card key={reclamo.reclamoId} sx={{ mb: 2 }}>
                                        <CardContent>
                                            <Typography variant="subtitle1" fontWeight="bold">
                                                {reclamo.codigoReclamo}
                                            </Typography>
                                            <Typography variant="body2" color="text.secondary">
                                                {clienteService.formatearFecha(reclamo.fechaCreacion)}
                                            </Typography>

                                            <Box sx={{ mt: 2 }}>
                                                <Grid container spacing={1}>
                                                    {reclamo.productos.map((producto, idx) => (
                                                        <Grid size={{ xs: 12, sm: 6, md: 4 }} key={idx}>
                                                            <Box sx={{ p: 1, border: 1, borderColor: 'divider', borderRadius: 1 }}>
                                                                <Typography variant="body2" fontWeight="bold">
                                                                    {producto.marca} {producto.modelo}
                                                                </Typography>
                                                                <Box sx={{ display: 'flex', alignItems: 'center', mt: 0.5 }}>
                                                                    <Chip
                                                                        label={producto.estado}
                                                                        size="small"
                                                                        color={clienteService.getEstadoColor(producto.estado) as any}
                                                                        sx={{ mr: 1 }}
                                                                    />
                                                                    <Chip
                                                                        label={producto.tipoReclamo}
                                                                        size="small"
                                                                        variant="outlined"
                                                                    />
                                                                </Box>
                                                            </Box>
                                                        </Grid>
                                                    ))}
                                                </Grid>
                                            </Box>
                                        </CardContent>
                                    </Card>
                                ))}
                            </Box>
                        ) : (
                            renderReclamos()
                        )}
                    </CardContent>
                </Card>

                <Dialog
                    open={pdfDialog.open}
                    onClose={() => setPdfDialog({ open: false, tipo: 'tecnico', nombreArchivo: '' })}
                    maxWidth="md"
                    fullWidth
                >
                    <DialogTitle>
                        <PictureAsPdf sx={{ verticalAlign: 'middle', mr: 1 }} />
                        {pdfDialog.nombreArchivo}
                    </DialogTitle>
                    <DialogContent>
                        {pdfDialog.base64 ? (
                            <Box sx={{ height: '500px' }}>
                                <iframe
                                    src={`data:application/pdf;base64,${pdfDialog.base64}#toolbar=1&navpanes=1&scrollbar=1`}
                                    width="100%"
                                    height="100%"
                                    style={{ border: 'none' }}
                                    title={pdfDialog.nombreArchivo}
                                />
                            </Box>
                        ) : (
                            <CircularProgress sx={{ display: 'block', margin: 'auto' }} />
                        )}
                    </DialogContent>
                    <DialogActions>
                        <Button
                            onClick={() => setPdfDialog({ open: false, tipo: 'tecnico', nombreArchivo: '' })}
                        >
                            Cerrar
                        </Button>
                        <Button
                            onClick={verPdfEnVentana}
                            startIcon={<Visibility />}
                        >
                            Abrir en Nueva Ventana
                        </Button>
                        <Button
                            onClick={descargarPdf}
                            variant="contained"
                            startIcon={<Download />}
                        >
                            Descargar
                        </Button>
                    </DialogActions>
                </Dialog>
            </Box>
        </LocalizationProvider>
    );
};

export default ReclamosDashboard;