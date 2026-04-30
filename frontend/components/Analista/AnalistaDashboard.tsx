import React, { useState, useEffect } from 'react';
import {
  Container, Grid, Paper, Typography, Box, Card, CardContent,
  CircularProgress, Alert, Button, Chip, Divider, LinearProgress,
  Table, TableBody, TableCell, TableContainer, TableHead, TableRow
} from '@mui/material';
import {
  TrendingUp, TrendingDown, AttachMoney, ShoppingCart,
  Inventory, ReportProblem, People, Download
} from '@mui/icons-material';
import {
  LineChart, Line, BarChart, Bar, XAxis, YAxis, CartesianGrid,
  Tooltip as RechartsTooltip, Legend, PieChart, Pie, Cell
} from 'recharts';
import { analistaService } from '../../services/analistaService';
import type {
  DashboardAnalista, VentaDiaria, ProductoMasVendido,
  CategoriaVentas, ReclamoEstado, ProductoBajoStock, UsuariosPorRol
} from '../../src/types/analista';

const COLORS = ['#0088FE', '#00C49F', '#FFBB28', '#FF8042', '#8884d8', '#82ca9d'];

// Funciones de formato
const formatCurrency = (value: number) => {
  return new Intl.NumberFormat('es-EC', { style: 'currency', currency: 'USD' }).format(value);
};

const formatNumber = (value: number) => {
  return new Intl.NumberFormat('es-EC').format(value);
};

// Funciones seguras para tooltips
const currencyFormatter = (value: any) => {
  if (value === undefined || value === null) return '-';
  const num = typeof value === 'number' ? value : parseFloat(value);
  return isNaN(num) ? '-' : formatCurrency(num);
};

const numberFormatter = (value: any) => {
  if (value === undefined || value === null) return '-';
  const num = typeof value === 'number' ? value : parseFloat(value);
  return isNaN(num) ? '-' : formatNumber(num);
};

// Label personalizado para pie chart
const renderCustomLabel = ({ name, percent }: { name?: string; percent?: number }) => {
  if (!name || percent === undefined) return null;
  return `${name}: ${(percent * 100).toFixed(0)}%`;
};

const AnalistaDashboard: React.FC = () => {
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [data, setData] = useState<DashboardAnalista | null>(null);

  useEffect(() => {
    cargarDashboard();
  }, []);

  const cargarDashboard = async () => {
    setLoading(true);
    setError(null);
    try {
      const result = await analistaService.getDashboard();
      setData(result);
    } catch (err) {
      setError('Error al cargar el dashboard');
      console.error(err);
    } finally {
      setLoading(false);
    }
  };

  const handleExportVentas = async () => {
    try {
      await analistaService.exportarVentas();
    } catch (err) {
      setError('Error al exportar ventas');
    }
  };

  const handleExportInventario = async () => {
    try {
      await analistaService.exportarInventario();
    } catch (err) {
      setError('Error al exportar inventario');
    }
  };

  if (loading) {
    return (
      <Box sx={{ display: 'flex', justifyContent: 'center', alignItems: 'center', height: '50vh' }}>
        <CircularProgress />
      </Box>
    );
  }

  if (error) {
    return (
      <Alert severity="error" sx={{ m: 2 }}>
        {error}
        <Button onClick={cargarDashboard} size="small" sx={{ ml: 2 }}>
          Reintentar
        </Button>
      </Alert>
    );
  }

  if (!data) return null;

  return (
    <Container maxWidth="xl" sx={{ py: 3 }}>
      <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 3 }}>
        <Typography variant="h4">Dashboard de Análisis</Typography>
        <Box>
          <Button
            variant="outlined"
            startIcon={<Download />}
            onClick={handleExportVentas}
            sx={{ mr: 1 }}
          >
            Exportar Ventas
          </Button>
          <Button
            variant="outlined"
            startIcon={<Download />}
            onClick={handleExportInventario}
          >
            Exportar Inventario
          </Button>
        </Box>
      </Box>

      {/* Tarjetas de resumen */}
      <Grid container spacing={3} sx={{ mb: 4 }}>
        <Grid size={{ xs: 12, sm: 6, md: 3 }}>
          <Card>
            <CardContent>
              <Box sx={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between' }}>
                <Box>
                  <Typography color="textSecondary" gutterBottom variant="body2">
                    Ingresos Totales
                  </Typography>
                  <Typography variant="h5">
                    {formatCurrency(data.resumen.totalIngresos)}
                  </Typography>
                  <Box sx={{ display: 'flex', alignItems: 'center', mt: 1 }}>
                    {data.ventasUltimos30Dias.variacionPorcentual > 0 ? (
                      <TrendingUp color="success" fontSize="small" />
                    ) : (
                      <TrendingDown color="error" fontSize="small" />
                    )}
                    <Typography variant="body2" color={data.ventasUltimos30Dias.variacionPorcentual > 0 ? 'success.main' : 'error.main'}>
                      {Math.abs(data.ventasUltimos30Dias.variacionPorcentual).toFixed(1)}%
                    </Typography>
                    <Typography variant="caption" color="textSecondary" sx={{ ml: 0.5 }}>
                      vs periodo anterior
                    </Typography>
                  </Box>
                </Box>
                <AttachMoney sx={{ fontSize: 48, color: 'primary.main', opacity: 0.7 }} />
              </Box>
            </CardContent>
          </Card>
        </Grid>
        <Grid size={{ xs: 12, sm: 6, md: 3 }}>
          <Card>
            <CardContent>
              <Box sx={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between' }}>
                <Box>
                  <Typography color="textSecondary" gutterBottom variant="body2">
                    Ventas (30 días)
                  </Typography>
                  <Typography variant="h5">
                    {formatNumber(data.resumen.totalVentas)}
                  </Typography>
                  <Typography variant="body2" color="textSecondary">
                    Promedio: {formatCurrency(data.resumen.promedioVenta)}
                  </Typography>
                </Box>
                <ShoppingCart sx={{ fontSize: 48, color: 'secondary.main', opacity: 0.7 }} />
              </Box>
            </CardContent>
          </Card>
        </Grid>
        <Grid size={{ xs: 12, sm: 6, md: 3 }}>
          <Card>
            <CardContent>
              <Box sx={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between' }}>
                <Box>
                  <Typography color="textSecondary" gutterBottom variant="body2">
                    Productos en Inventario
                  </Typography>
                  <Typography variant="h5">
                    {formatNumber(data.resumen.productosEnInventario)}
                  </Typography>
                  <Typography variant="body2" color="textSecondary">
                    {data.inventario.totalProductos} productos diferentes
                  </Typography>
                </Box>
                <Inventory sx={{ fontSize: 48, color: '#ff9800', opacity: 0.7 }} />
              </Box>
            </CardContent>
          </Card>
        </Grid>
        <Grid size={{ xs: 12, sm: 6, md: 3 }}>
          <Card>
            <CardContent>
              <Box sx={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between' }}>
                <Box>
                  <Typography color="textSecondary" gutterBottom variant="body2">
                    Reclamos Pendientes
                  </Typography>
                  <Typography variant="h5">
                    {formatNumber(data.resumen.reclamosPendientes)}
                  </Typography>
                  <Typography variant="body2" color="textSecondary">
                    {data.reclamosPorEstado.length} estados diferentes
                  </Typography>
                </Box>
                <ReportProblem sx={{ fontSize: 48, color: '#f44336', opacity: 0.7 }} />
              </Box>
            </CardContent>
          </Card>
        </Grid>
      </Grid>

      {/* Gráficos principales */}
      <Grid container spacing={3}>
        {/* Ventas diarias */}
        <Grid size={{ xs: 12, md: 8 }}>
          <Paper sx={{ p: 2 }}>
            <Typography variant="h6" gutterBottom>Ventas Diarias (últimos 30 días)</Typography>
            <LineChart
              width={window.innerWidth < 800 ? 500 : 700}
              height={300}
              data={data.ventasUltimos30Dias.ventasDiarias.map((v: VentaDiaria) => ({
                fecha: new Date(v.fecha).toLocaleDateString('es-EC', { day: '2-digit', month: 'short' }),
                total: v.total,
                cantidad: v.cantidadVentas
              }))}
              margin={{ top: 5, right: 30, left: 20, bottom: 5 }}
            >
              <CartesianGrid strokeDasharray="3 3" />
              <XAxis dataKey="fecha" />
              <YAxis yAxisId="left" />
              <YAxis yAxisId="right" orientation="right" />
              <RechartsTooltip formatter={(value, name) => {
                if (name === 'Ingresos') return currencyFormatter(value);
                if (name === 'Cantidad') return numberFormatter(value);
                return value;
              }} />
              <Legend />
              <Line yAxisId="left" type="monotone" dataKey="total" stroke="#8884d8" name="Ingresos" />
              <Line yAxisId="right" type="monotone" dataKey="cantidad" stroke="#82ca9d" name="Cantidad" />
            </LineChart>
          </Paper>
        </Grid>

        {/* Productos más vendidos */}
        <Grid size={{ xs: 12, md: 4 }}>
          <Paper sx={{ p: 2 }}>
            <Typography variant="h6" gutterBottom>Productos Más Vendidos</Typography>
            <TableContainer>
              <Table size="small">
                <TableHead>
                  <TableRow>
                    <TableCell>Producto</TableCell>
                    <TableCell align="right">Unidades</TableCell>
                    <TableCell align="right">Ingreso</TableCell>
                  </TableRow>
                </TableHead>
                <TableBody>
                  {data.productosMasVendidos.slice(0, 5).map((p: ProductoMasVendido) => (
                    <TableRow key={p.productoId}>
                      <TableCell>
                        <Typography variant="body2">{p.nombreProducto}</Typography>
                        <Typography variant="caption" color="textSecondary">{p.marca}</Typography>
                      </TableCell>
                      <TableCell align="right">{p.unidadesVendidas}</TableCell>
                      <TableCell align="right">{formatCurrency(p.ingresoGenerado)}</TableCell>
                    </TableRow>
                  ))}
                </TableBody>
              </Table>
            </TableContainer>
          </Paper>
        </Grid>

        {/* Ventas por categoría */}
        <Grid size={{ xs: 12, md: 6 }}>
          <Paper sx={{ p: 2 }}>
            <Typography variant="h6" gutterBottom>Ventas por Categoría</Typography>
            <BarChart
              width={window.innerWidth < 800 ? 500 : 600}
              height={300}
              data={data.ventasPorCategoria.map((c: CategoriaVentas) => ({
                nombre: c.nombreCategoria.length > 15 ? c.nombreCategoria.substring(0, 12) + '...' : c.nombreCategoria,
                unidades: c.unidadesVendidas,
                ingreso: c.ingresoGenerado
              }))}
              margin={{ top: 5, right: 30, left: 20, bottom: 5 }}
            >
              <CartesianGrid strokeDasharray="3 3" />
              <XAxis dataKey="nombre" />
              <YAxis />
              <RechartsTooltip formatter={(value, name) => {
                if (name === 'Ingresos') return currencyFormatter(value);
                if (name === 'Unidades') return numberFormatter(value);
                return value;
              }} />
              <Legend />
              <Bar dataKey="ingreso" fill="#8884d8" name="Ingresos" />
              <Bar dataKey="unidades" fill="#82ca9d" name="Unidades" />
            </BarChart>
          </Paper>
        </Grid>

        {/* Reclamos por estado */}
        <Grid size={{ xs: 12, md: 6 }}>
          <Paper sx={{ p: 2, display: 'flex', flexDirection: 'column', alignItems: 'center' }}>
            <Typography variant="h6" gutterBottom>Reclamos por Estado</Typography>
            <Box sx={{ display: 'flex', justifyContent: 'center', width: '100%' }}>
              <PieChart width={500} height={300}>
                <Pie
                  data={data.reclamosPorEstado}
                  cx="50%"
                  cy="50%"
                  labelLine={false}
                  label={renderCustomLabel}
                  outerRadius={80}
                  fill="#8884d8"
                  dataKey="cantidad"
                  nameKey="estado"
                >
                  {data.reclamosPorEstado.map((_entry, index) => (
                    <Cell key={`cell-${index}`} fill={COLORS[index % COLORS.length]} />
                  ))}
                </Pie>
                <RechartsTooltip formatter={(value: unknown) => `${value as number} reclamos`} />
              </PieChart>
            </Box>
            <Divider sx={{ my: 2, width: '100%' }} />
            <TableContainer sx={{ maxHeight: 200 }}>
              <Table size="small" stickyHeader>
                <TableHead>
                  <TableRow>
                    <TableCell>Estado</TableCell>
                    <TableCell align="right">Cantidad</TableCell>
                    <TableCell align="right">Porcentaje</TableCell>
                  </TableRow>
                </TableHead>
                <TableBody>
                  {data.reclamosPorEstado.map((estado: ReclamoEstado) => (
                    <TableRow key={estado.estado}>
                      <TableCell>{estado.estado}</TableCell>
                      <TableCell align="right">{estado.cantidad}</TableCell>
                      <TableCell align="right">{estado.porcentaje.toFixed(1)}%</TableCell>
                    </TableRow>
                  ))}
                </TableBody>
              </Table>
            </TableContainer>
          </Paper>
        </Grid>

        {/* Inventario - productos bajo stock */}
        <Grid size={{ xs: 12, md: 6 }}>
          <Paper sx={{ p: 2 }}>
            <Typography variant="h6" gutterBottom>Productos con Bajo Stock</Typography>
            {data.inventario.productosBajoStock.length === 0 ? (
              <Alert severity="success">No hay productos con bajo stock</Alert>
            ) : (
              <TableContainer>
                <Table size="small">
                  <TableHead>
                    <TableRow>
                      <TableCell>Producto</TableCell>
                      <TableCell align="right">Stock Actual</TableCell>
                      <TableCell align="right">Umbral</TableCell>
                      <TableCell align="right">Estado</TableCell>
                    </TableRow>
                  </TableHead>
                  <TableBody>
                    {data.inventario.productosBajoStock.map((p: ProductoBajoStock) => (
                      <TableRow key={p.productoId}>
                        <TableCell>{p.nombreProducto}</TableCell>
                        <TableCell align="right">{p.stockActual}</TableCell>
                        <TableCell align="right">{p.umbralMinimo}</TableCell>
                        <TableCell align="right">
                          <Chip
                            size="small"
                            label={p.stockActual === 0 ? 'Agotado' : 'Bajo stock'}
                            color={p.stockActual === 0 ? 'error' : 'warning'}
                          />
                        </TableCell>
                      </TableRow>
                    ))}
                  </TableBody>
                </Table>
              </TableContainer>
            )}
          </Paper>
        </Grid>

        {/* Usuarios activos */}
        <Grid size={{ xs: 12, md: 6 }}>
          <Paper sx={{ p: 2 }}>
            <Typography variant="h6" gutterBottom>Usuarios Activos</Typography>
            <Box sx={{ display: 'flex', alignItems: 'center', mb: 2 }}>
              <People sx={{ fontSize: 40, mr: 2, color: 'primary.main' }} />
              <Box>
                <Typography variant="h4">{data.usuarios.total}</Typography>
                <Typography variant="body2" color="textSecondary">
                  +{data.usuarios.nuevosUltimoMes} nuevos en el último mes
                </Typography>
              </Box>
            </Box>
            <Divider sx={{ my: 2 }} />
            <Typography variant="subtitle2" gutterBottom>Distribución por rol</Typography>
            {data.usuarios.porRol.map((r: UsuariosPorRol) => (
              <Box key={r.rol} sx={{ mb: 1 }}>
                <Box sx={{ display: 'flex', justifyContent: 'space-between' }}>
                  <Typography variant="body2">{r.rol}</Typography>
                  <Typography variant="body2">{r.cantidad}</Typography>
                </Box>
                <LinearProgress
                  variant="determinate"
                  value={(r.cantidad / data.usuarios.total) * 100}
                  sx={{ height: 6, borderRadius: 3 }}
                />
              </Box>
            ))}
          </Paper>
        </Grid>
      </Grid>
    </Container>
  );
};

export default AnalistaDashboard;