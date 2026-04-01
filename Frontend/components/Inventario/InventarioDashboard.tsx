import React, { useState, useEffect } from 'react';
import {
  Container, Typography, Box, Tabs, Tab, Paper, Button, Alert,
  CircularProgress, Dialog, DialogTitle, DialogContent, DialogActions,
  TextField, MenuItem, FormControlLabel, Switch, IconButton, Tooltip
} from '@mui/material';
import { Add, Refresh, Delete } from '@mui/icons-material';
import { inventarioService } from '../../services/inventarioService';
import { productoManagementService } from '../../services/productoManagementService';
import type { ProductoManagement } from '../../src/types/producto';
import type { Ubicacion, Proveedor } from '../../src/types/inventario';
import UbicacionesList from './UbicacionesList';
import MovimientosList from './MovimientosList';
import EntradaProductoForm from './EntradaProductoForm';
import NumerosSerieList from './NumerosSerieList';

interface TabPanelProps {
  children?: React.ReactNode;
  index: number;
  value: number;
}

function TabPanel(props: TabPanelProps) {
  const { children, value, index, ...other } = props;
  return (
    <div role="tabpanel" hidden={value !== index} {...other}>
      {value === index && <Box sx={{ py: 3 }}>{children}</Box>}
    </div>
  );
}

const InventarioDashboard: React.FC = () => {
  const [tabValue, setTabValue] = useState(0);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [success, setSuccess] = useState<string | null>(null);

  // Datos
  const [ubicaciones, setUbicaciones] = useState<Ubicacion[]>([]);
  const [proveedores, setProveedores] = useState<Proveedor[]>([]);
  const [productos, setProductos] = useState<ProductoManagement[]>([]);
  const [mostrarInactivos, setMostrarInactivos] = useState(false); // Nuevo estado para filtro

  // Diálogo para nueva ubicación
  const [openUbicacionDialog, setOpenUbicacionDialog] = useState(false);
  const [editUbicacion, setEditUbicacion] = useState<Ubicacion | null>(null);
  const [ubicacionForm, setUbicacionForm] = useState({
    codigo: '',
    nombre: '',
    tipo: '',
    ubicacionPadreId: undefined as number | undefined,
    capacidadMaxima: undefined as number | undefined,
    activo: true
  });

  // Diálogo para nuevo proveedor
  const [openProveedorDialog, setOpenProveedorDialog] = useState(false);
  const [editProveedor, setEditProveedor] = useState<Proveedor | null>(null);
  const [proveedorForm, setProveedorForm] = useState({
    nombre: '',
    cedula: '',
    ruc: '',
    direccion: '',
    telefono: '',
    email: '',
    contactoPrincipal: '',
    plazoEntregaDias: 7,
    activo: true
  });

  useEffect(() => {
    cargarDatos();
  }, [mostrarInactivos]); // Recargar cuando cambia el filtro

  const cargarDatos = async () => {
    setLoading(true);
    setError(null);
    try {
      const [ubis, provs, prods] = await Promise.all([
        inventarioService.getUbicaciones(),
        inventarioService.getProveedores(!mostrarInactivos), // Si mostrarInactivos es true, traer todos (soloActivos=false)
        productoManagementService.getProductos(false)
      ]);
      setUbicaciones(ubis);
      setProveedores(provs);
      setProductos(prods);
    } catch (err) {
      setError('Error al cargar datos');
      console.error(err);
    } finally {
      setLoading(false);
    }
  };

  const handleTabChange = (_: React.SyntheticEvent, newValue: number) => {
    setTabValue(newValue);
  };

  // --- Ubicaciones ---
  const handleOpenUbicacionDialog = (ubicacion?: Ubicacion) => {
    if (ubicacion) {
      setEditUbicacion(ubicacion);
      setUbicacionForm({
        codigo: ubicacion.codigo,
        nombre: ubicacion.nombre,
        tipo: ubicacion.tipo || '',
        ubicacionPadreId: ubicacion.ubicacionPadreId || undefined,
        capacidadMaxima: ubicacion.capacidadMaxima || undefined,
        activo: ubicacion.activo
      });
    } else {
      setEditUbicacion(null);
      setUbicacionForm({
        codigo: '',
        nombre: '',
        tipo: '',
        ubicacionPadreId: undefined,
        capacidadMaxima: undefined,
        activo: true
      });
    }
    setOpenUbicacionDialog(true);
  };

  const handleSaveUbicacion = async () => {
    setLoading(true);
    setError(null);
    try {
      if (editUbicacion) {
        await inventarioService.updateUbicacion({ id: editUbicacion.id, ...ubicacionForm });
        setSuccess('Ubicación actualizada');
      } else {
        await inventarioService.createUbicacion(ubicacionForm);
        setSuccess('Ubicación creada');
      }
      setOpenUbicacionDialog(false);
      cargarDatos();
    } catch (err: any) {
      setError(err.response?.data?.message || 'Error al guardar ubicación');
    } finally {
      setLoading(false);
    }
  };

  const handleDeleteUbicacion = async (id: number) => {
    if (!confirm('¿Eliminar ubicación?')) return;
    setLoading(true);
    setError(null);
    try {
      await inventarioService.deleteUbicacion(id);
      setSuccess('Ubicación eliminada');
      cargarDatos();
    } catch (err: any) {
      setError(err.response?.data?.message || 'Error al eliminar ubicación');
    } finally {
      setLoading(false);
    }
  };

  // --- Proveedores ---
  const handleOpenProveedorDialog = (proveedor?: Proveedor) => {
    if (proveedor) {
      setEditProveedor(proveedor);
      setProveedorForm({
        nombre: proveedor.nombre,
        cedula: proveedor.cedula,
        ruc: proveedor.ruc,
        direccion: proveedor.direccion || '',
        telefono: proveedor.telefono || '',
        email: proveedor.email || '',
        contactoPrincipal: proveedor.contactoPrincipal || '',
        plazoEntregaDias: proveedor.plazoEntregaDias || 7,
        activo: proveedor.activo
      });
    } else {
      setEditProveedor(null);
      setProveedorForm({
        nombre: '',
        cedula: '',
        ruc: '',
        direccion: '',
        telefono: '',
        email: '',
        contactoPrincipal: '',
        plazoEntregaDias: 7,
        activo: true
      });
    }
    setOpenProveedorDialog(true);
  };

  const handleSaveProveedor = async () => {
    setLoading(true);
    setError(null);
    try {
      if (editProveedor) {
        await inventarioService.updateProveedor({ id: editProveedor.id, ...proveedorForm });
        setSuccess('Proveedor actualizado');
      } else {
        await inventarioService.createProveedor(proveedorForm);
        setSuccess('Proveedor creado');
      }
      setOpenProveedorDialog(false);
      cargarDatos();
    } catch (err: any) {
      setError(err.response?.data?.message || 'Error al guardar proveedor');
    } finally {
      setLoading(false);
    }
  };

  const handleToggleProveedor = async (id: number, activo: boolean) => {
    try {
      await inventarioService.toggleProveedorActivo(id, activo);
      setSuccess(`Proveedor ${activo ? 'activado' : 'desactivado'}`);
      cargarDatos();
    } catch (err) {
      setError('Error al cambiar estado');
    }
  };

  const handleDeleteProveedor = async (id: number, nombre: string) => {
    if (!confirm(`¿Eliminar el proveedor "${nombre}"? Esta acción solo es posible si no tiene productos asociados.`)) return;
    setLoading(true);
    setError(null);
    try {
      await inventarioService.deleteProveedor(id);
      setSuccess(`Proveedor "${nombre}" eliminado correctamente`);
      cargarDatos();
    } catch (err: any) {
      const mensaje = err.response?.data?.message || 'Error al eliminar proveedor';
      setError(mensaje);
    } finally {
      setLoading(false);
    }
  };

  return (
    <Container maxWidth="xl">
      <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 3 }}>
        <Typography variant="h4">Panel de Inventario</Typography>
        <Box>
          <Button startIcon={<Refresh />} onClick={cargarDatos} sx={{ mr: 1 }}>
            Refrescar
          </Button>
          <Button variant="contained" startIcon={<Add />} onClick={() => handleOpenUbicacionDialog()}>
            Nueva Ubicación
          </Button>
        </Box>
      </Box>

      {error && <Alert severity="error" sx={{ mb: 2 }} onClose={() => setError(null)}>{error}</Alert>}
      {success && <Alert severity="success" sx={{ mb: 2 }} onClose={() => setSuccess(null)}>{success}</Alert>}

      <Paper sx={{ width: '100%' }}>
        <Tabs value={tabValue} onChange={handleTabChange} sx={{ borderBottom: 1, borderColor: 'divider' }}>
          <Tab label="Ubicaciones" />
          <Tab label="Movimientos" />
          <Tab label="Registrar Entrada" />
          <Tab label="Números de Serie" />
          <Tab label="Proveedores" />
        </Tabs>

        <TabPanel value={tabValue} index={0}>
          <UbicacionesList
            ubicaciones={ubicaciones}
            onEdit={handleOpenUbicacionDialog}
            onDelete={handleDeleteUbicacion}
          />
        </TabPanel>

        <TabPanel value={tabValue} index={1}>
          <MovimientosList productos={productos} />
        </TabPanel>

        <TabPanel value={tabValue} index={2}>
          <EntradaProductoForm
            productos={productos}
            proveedores={proveedores}
            ubicaciones={ubicaciones}
            onSuccess={() => {
              setSuccess('Entrada registrada');
              cargarDatos();
            }}
          />
        </TabPanel>

        <TabPanel value={tabValue} index={3}>
          <NumerosSerieList
            productos={productos}
            ubicaciones={ubicaciones}
            onRefresh={cargarDatos}
          />
        </TabPanel>

        <TabPanel value={tabValue} index={4}>
          <Box sx={{ mb: 2, display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
            <FormControlLabel
              control={
                <Switch
                  checked={mostrarInactivos}
                  onChange={(e) => setMostrarInactivos(e.target.checked)}
                  color="primary"
                />
              }
              label="Mostrar inactivos"
            />
            <Button variant="contained" startIcon={<Add />} onClick={() => handleOpenProveedorDialog()}>
              Nuevo Proveedor
            </Button>
          </Box>

          {proveedores.map(p => (
            <Paper key={p.id} sx={{ p: 2, mb: 1, display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
              <Box>
                <Typography variant="subtitle1">{p.nombre}</Typography>
                <Typography variant="body2" color="text.secondary">RUC: {p.ruc}</Typography>
                {p.activo === false && (
                  <Typography variant="caption" color="error">Inactivo</Typography>
                )}
              </Box>
              <Box sx={{ display: 'flex', gap: 1 }}>
                <Button size="small" onClick={() => handleOpenProveedorDialog(p)}>Editar</Button>
                <Tooltip title="Eliminar solo si no tiene productos asociados">
                  <IconButton
                    size="small"
                    color="error"
                    onClick={() => handleDeleteProveedor(p.id, p.nombre)}
                  >
                    <Delete />
                  </IconButton>
                </Tooltip>
                <FormControlLabel
                  control={
                    <Switch
                      checked={p.activo}
                      onChange={(e) => handleToggleProveedor(p.id, e.target.checked)}
                    />
                  }
                  label={p.activo ? 'Activo' : 'Inactivo'}
                />
              </Box>
            </Paper>
          ))}
        </TabPanel>
      </Paper>

      {/* Diálogo Ubicación */}
      <Dialog open={openUbicacionDialog} onClose={() => setOpenUbicacionDialog(false)} maxWidth="sm" fullWidth>
        <DialogTitle>{editUbicacion ? 'Editar Ubicación' : 'Nueva Ubicación'}</DialogTitle>
        <DialogContent>
          <TextField
            label="Código"
            fullWidth
            margin="normal"
            value={ubicacionForm.codigo}
            onChange={(e) => setUbicacionForm({ ...ubicacionForm, codigo: e.target.value })}
            required
          />
          <TextField
            label="Nombre"
            fullWidth
            margin="normal"
            value={ubicacionForm.nombre}
            onChange={(e) => setUbicacionForm({ ...ubicacionForm, nombre: e.target.value })}
            required
          />
          <TextField
            label="Tipo"
            select
            fullWidth
            margin="normal"
            value={ubicacionForm.tipo}
            onChange={(e) => setUbicacionForm({ ...ubicacionForm, tipo: e.target.value })}
          >
            <MenuItem value="">Ninguno</MenuItem>
            <MenuItem value="Bodega">Bodega</MenuItem>
            <MenuItem value="Estante">Estante</MenuItem>
            <MenuItem value="Pasillo">Pasillo</MenuItem>
            <MenuItem value="Caja">Caja</MenuItem>
          </TextField>
          <TextField
            label="Ubicación Padre"
            select
            fullWidth
            margin="normal"
            value={ubicacionForm.ubicacionPadreId || ''}
            onChange={(e) => setUbicacionForm({ ...ubicacionForm, ubicacionPadreId: e.target.value ? Number(e.target.value) : undefined })}
          >
            <MenuItem value="">Ninguna</MenuItem>
            {ubicaciones.map(u => (
              <MenuItem key={u.id} value={u.id}>{u.nombre} ({u.codigo})</MenuItem>
            ))}
          </TextField>
          <TextField
            label="Capacidad Máxima"
            type="number"
            fullWidth
            margin="normal"
            value={ubicacionForm.capacidadMaxima || ''}
            onChange={(e) => setUbicacionForm({ ...ubicacionForm, capacidadMaxima: e.target.value ? Number(e.target.value) : undefined })}
          />
          <FormControlLabel
            control={<Switch checked={ubicacionForm.activo} onChange={(e) => setUbicacionForm({ ...ubicacionForm, activo: e.target.checked })} />}
            label="Activo"
          />
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setOpenUbicacionDialog(false)}>Cancelar</Button>
          <Button onClick={handleSaveUbicacion} variant="contained" disabled={loading}>
            {loading ? <CircularProgress size={24} /> : 'Guardar'}
          </Button>
        </DialogActions>
      </Dialog>

      {/* Diálogo Proveedor */}
      <Dialog open={openProveedorDialog} onClose={() => setOpenProveedorDialog(false)} maxWidth="sm" fullWidth>
        <DialogTitle>{editProveedor ? 'Editar Proveedor' : 'Nuevo Proveedor'}</DialogTitle>
        <DialogContent>
          <TextField label="Nombre" fullWidth margin="normal" value={proveedorForm.nombre} onChange={(e) => setProveedorForm({ ...proveedorForm, nombre: e.target.value })} required />
          <TextField label="Cédula" fullWidth margin="normal" value={proveedorForm.cedula} onChange={(e) => setProveedorForm({ ...proveedorForm, cedula: e.target.value })} required />
          <TextField label="RUC" fullWidth margin="normal" value={proveedorForm.ruc} onChange={(e) => setProveedorForm({ ...proveedorForm, ruc: e.target.value })} required />
          <TextField label="Dirección" fullWidth margin="normal" value={proveedorForm.direccion} onChange={(e) => setProveedorForm({ ...proveedorForm, direccion: e.target.value })} />
          <TextField label="Teléfono" fullWidth margin="normal" value={proveedorForm.telefono} onChange={(e) => setProveedorForm({ ...proveedorForm, telefono: e.target.value })} />
          <TextField label="Email" fullWidth margin="normal" value={proveedorForm.email} onChange={(e) => setProveedorForm({ ...proveedorForm, email: e.target.value })} />
          <TextField label="Contacto Principal" fullWidth margin="normal" value={proveedorForm.contactoPrincipal} onChange={(e) => setProveedorForm({ ...proveedorForm, contactoPrincipal: e.target.value })} />
          <TextField label="Plazo Entrega (días)" type="number" fullWidth margin="normal" value={proveedorForm.plazoEntregaDias} onChange={(e) => setProveedorForm({ ...proveedorForm, plazoEntregaDias: Number(e.target.value) })} />
          <FormControlLabel control={<Switch checked={proveedorForm.activo} onChange={(e) => setProveedorForm({ ...proveedorForm, activo: e.target.checked })} />} label="Activo" />
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setOpenProveedorDialog(false)}>Cancelar</Button>
          <Button onClick={handleSaveProveedor} variant="contained" disabled={loading}>
            {loading ? <CircularProgress size={24} /> : 'Guardar'}
          </Button>
        </DialogActions>
      </Dialog>
    </Container>
  );
};

export default InventarioDashboard;