import React, { useState, useEffect } from 'react';
import {
  Container, Typography, Box, Tabs, Tab, Paper, Button, Alert,
  Dialog, DialogTitle, DialogContent,
  IconButton,
  Card, CardMedia, CardContent, CardActions, Grid, Chip
} from '@mui/material';
import { Add, Refresh, Edit, Delete, Visibility, VisibilityOff } from '@mui/icons-material';
import { productoManagementService } from '../../services/productoManagementService';
import type { ProductoManagement, Categoria, Marca } from '../../src/types/producto';
import ProductoForm from './ProductoForm';
import CategoriaForm from './CategoriaForm';
import MarcaForm from './MarcaForm';

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

const ProductosDashboard: React.FC = () => {
  const [tabValue, setTabValue] = useState(0);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [success, setSuccess] = useState<string | null>(null);

  const [productos, setProductos] = useState<ProductoManagement[]>([]);
  const [categorias, setCategorias] = useState<Categoria[]>([]);
  const [marcas, setMarcas] = useState<Marca[]>([]);

  const [openProductoDialog, setOpenProductoDialog] = useState(false);
  const [editProducto, setEditProducto] = useState<ProductoManagement | null>(null);

  const [openCategoriaDialog, setOpenCategoriaDialog] = useState(false);
  const [editCategoria, setEditCategoria] = useState<Categoria | null>(null);

  const [openMarcaDialog, setOpenMarcaDialog] = useState(false);
  const [editMarca, setEditMarca] = useState<Marca | null>(null);

  useEffect(() => {
    cargarDatos();
  }, []);

  const cargarDatos = async () => {
    setLoading(true);
    setError(null);
    try {
      const [prods, cats, marcasData] = await Promise.all([
        productoManagementService.getProductos(true),
        productoManagementService.getCategorias(true),
        productoManagementService.getMarcas()
      ]);
      setProductos(prods);
      setCategorias(cats);
      setMarcas(marcasData);
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

  const handleToggleActivo = async (id: number, activo: boolean) => {
    try {
      await productoManagementService.toggleProductoActivo(id, activo);
      setSuccess(`Producto ${activo ? 'activado' : 'desactivado'}`);
      cargarDatos();
    } catch (err) {
      setError('Error al cambiar estado');
    }
  };

  return (
    <Container maxWidth="xl">
      <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 3 }}>
        <Typography variant="h4">Gestión de Productos</Typography>
        <Box>
          <Button startIcon={<Refresh />} onClick={cargarDatos} sx={{ mr: 1 }}>
            Refrescar
          </Button>
        </Box>
      </Box>

      {error && <Alert severity="error" sx={{ mb: 2 }} onClose={() => setError(null)}>{error}</Alert>}
      {success && <Alert severity="success" sx={{ mb: 2 }} onClose={() => setSuccess(null)}>{success}</Alert>}

      <Paper sx={{ width: '100%' }}>
        <Tabs value={tabValue} onChange={handleTabChange} sx={{ borderBottom: 1, borderColor: 'divider' }}>
          <Tab label="Productos" />
          <Tab label="Categorías" />
          <Tab label="Marcas" />
        </Tabs>

        <TabPanel value={tabValue} index={0}>
          <Box sx={{ mb: 2, display: 'flex', justifyContent: 'flex-end' }}>
            <Button variant="contained" startIcon={<Add />} onClick={() => setOpenProductoDialog(true)}>
              Nuevo Producto
            </Button>
          </Box>
          <Grid container spacing={2}>
            {productos.map(p => (
              <Grid size={{ xs: 12, sm: 6, md: 4 }} key={p.id}>
                <Card>
                  {p.imagenUrl && (
                    <CardMedia
                      component="img"
                      height="140"
                      image={p.imagenUrl}
                      alt={p.nombreCompleto}
                    />
                  )}
                  <CardContent>
                    <Typography variant="h6">{p.nombreCompleto}</Typography>
                    <Typography variant="body2" color="text.secondary" gutterBottom>
                      SKU: {p.sku} | Código: {p.codigo}
                    </Typography>
                    <Typography variant="body2">
                      Precio: ${p.precio.toFixed(2)}
                    </Typography>
                    <Box sx={{ mt: 1 }}>
                      <Chip size="small" label={p.marcaNombre} sx={{ mr: 0.5 }} />
                      {p.categoriaNombre && <Chip size="small" label={p.categoriaNombre} />}
                    </Box>
                    <Box sx={{ mt: 1 }}>
                      <Chip
                        size="small"
                        label={p.activo ? 'Activo' : 'Inactivo'}
                        color={p.activo ? 'success' : 'default'}
                      />
                      <Chip
                        size="small"
                        label={p.visibilidad}
                        variant="outlined"
                        sx={{ ml: 0.5 }}
                      />
                    </Box>
                  </CardContent>
                  <CardActions>
                    <IconButton size="small" onClick={() => { setEditProducto(p); setOpenProductoDialog(true); }}>
                      <Edit />
                    </IconButton>
                    <IconButton size="small" onClick={() => handleToggleActivo(p.id, !p.activo)}>
                      {p.activo ? <VisibilityOff /> : <Visibility />}
                    </IconButton>
                  </CardActions>
                </Card>
              </Grid>
            ))}
          </Grid>
        </TabPanel>

        <TabPanel value={tabValue} index={1}>
          <Box sx={{ mb: 2, display: 'flex', justifyContent: 'flex-end' }}>
            <Button variant="contained" startIcon={<Add />} onClick={() => setOpenCategoriaDialog(true)}>
              Nueva Categoría
            </Button>
          </Box>
          {/* Lista de categorías */}
          {categorias.map(c => (
            <Paper key={c.id} sx={{ p: 2, mb: 1, display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
              <Box>
                <Typography variant="subtitle1">{c.nombre}</Typography>
                <Typography variant="body2" color="text.secondary">{c.descripcion}</Typography>
                {c.categoriaPadreNombre && <Typography variant="caption">Padre: {c.categoriaPadreNombre}</Typography>}
              </Box>
              <Box>
                <Chip label={c.activo ? 'Activo' : 'Inactivo'} size="small" sx={{ mr: 1 }} />
                <IconButton size="small" onClick={() => { setEditCategoria(c); setOpenCategoriaDialog(true); }}>
                  <Edit />
                </IconButton>
                <IconButton size="small" color="error" onClick={async () => {
                  if (confirm('¿Eliminar categoría?')) {
                    try {
                      await productoManagementService.deleteCategoria(c.id);
                      setSuccess('Categoría eliminada');
                      cargarDatos();
                    } catch (err: any) {
                      setError(err.response?.data?.message || 'Error al eliminar');
                    }
                  }
                }}>
                  <Delete />
                </IconButton>
              </Box>
            </Paper>
          ))}
        </TabPanel>

        <TabPanel value={tabValue} index={2}>
          <Box sx={{ mb: 2, display: 'flex', justifyContent: 'flex-end' }}>
            <Button variant="contained" startIcon={<Add />} onClick={() => setOpenMarcaDialog(true)}>
              Nueva Marca
            </Button>
          </Box>
          {marcas.map(m => (
            <Paper key={m.id} sx={{ p: 2, mb: 1, display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
              <Typography variant="subtitle1">{m.nombre}</Typography>
              <Box>
                <IconButton size="small" onClick={() => { setEditMarca(m); setOpenMarcaDialog(true); }}>
                  <Edit />
                </IconButton>
                <IconButton size="small" color="error" onClick={async () => {
                  if (confirm('¿Eliminar marca?')) {
                    try {
                      await productoManagementService.deleteMarca(m.id);
                      setSuccess('Marca eliminada');
                      cargarDatos();
                    } catch (err: any) {
                      setError(err.response?.data?.message || 'Error al eliminar');
                    }
                  }
                }}>
                  <Delete />
                </IconButton>
              </Box>
            </Paper>
          ))}
        </TabPanel>
      </Paper>

      {/* Diálogo Producto */}
      <Dialog open={openProductoDialog} onClose={() => setOpenProductoDialog(false)} maxWidth="md" fullWidth>
        <DialogTitle>{editProducto ? 'Editar Producto' : 'Nuevo Producto'}</DialogTitle>
        <DialogContent>
          <ProductoForm
            producto={editProducto}
            marcas={marcas}
            categorias={categorias.filter(c => c.activo)}
            onSave={async (data) => {
              setLoading(true);
              try {
                if (editProducto) {
                  await productoManagementService.updateProducto({ ...data, id: editProducto.id });
                } else {
                  await productoManagementService.createProducto(data);
                }
                setSuccess(`Producto ${editProducto ? 'actualizado' : 'creado'}`);
                setOpenProductoDialog(false);
                cargarDatos();
              } catch (err: any) {
                setError(err.response?.data?.message || 'Error al guardar');
              } finally {
                setLoading(false);
              }
            }}
            onCancel={() => setOpenProductoDialog(false)}
            loading={loading}
          />
        </DialogContent>
      </Dialog>

      {/* Diálogo Categoría */}
      <Dialog open={openCategoriaDialog} onClose={() => setOpenCategoriaDialog(false)} maxWidth="sm" fullWidth>
        <DialogTitle>{editCategoria ? 'Editar Categoría' : 'Nueva Categoría'}</DialogTitle>
        <DialogContent>
          <CategoriaForm
            categoria={editCategoria}
            categoriasPadre={categorias.filter(c => c.activo && c.id !== editCategoria?.id)}
            onSave={async (data) => {
              try {
                if (editCategoria) {
                  await productoManagementService.updateCategoria({ ...data, id: editCategoria.id });
                } else {
                  await productoManagementService.createCategoria(data);
                }
                setSuccess(`Categoría ${editCategoria ? 'actualizada' : 'creada'}`);
                setOpenCategoriaDialog(false);
                cargarDatos();
              } catch (err: any) {
                setError(err.response?.data?.message || 'Error al guardar');
              }
            }}
            onCancel={() => setOpenCategoriaDialog(false)}
          />
        </DialogContent>
      </Dialog>

      {/* Diálogo Marca */}
      <Dialog open={openMarcaDialog} onClose={() => setOpenMarcaDialog(false)} maxWidth="sm" fullWidth>
        <DialogTitle>{editMarca ? 'Editar Marca' : 'Nueva Marca'}</DialogTitle>
        <DialogContent>
          <MarcaForm
            marca={editMarca}
            onSave={async (data) => {
              try {
                if (editMarca) {
                  await productoManagementService.updateMarca({ ...data, id: editMarca.id });
                } else {
                  await productoManagementService.createMarca(data);
                }
                setSuccess(`Marca ${editMarca ? 'actualizada' : 'creada'}`);
                setOpenMarcaDialog(false);
                cargarDatos();
              } catch (err: any) {
                setError(err.response?.data?.message || 'Error al guardar');
              }
            }}
            onCancel={() => setOpenMarcaDialog(false)}
          />
        </DialogContent>
      </Dialog>
    </Container>
  );
};

export default ProductosDashboard;