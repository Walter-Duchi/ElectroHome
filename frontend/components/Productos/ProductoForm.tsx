/* eslint-disable react-hooks/set-state-in-effect */
import React, { useState, useEffect } from 'react';
import {
  Box, TextField, MenuItem, FormControl, InputLabel, Select,
  Grid, Button, FormControlLabel, Switch, Typography,
  Chip, Stack
} from '@mui/material';
import { CloudUpload } from '@mui/icons-material';
import type { ProductoManagement, Marca, Categoria } from '../../src/types/producto';

interface ProductoFormProps {
  producto?: ProductoManagement | null;
  marcas: Marca[];
  categorias: Categoria[];
  onSave: (data: any) => Promise<void>;
  onCancel: () => void;
  loading?: boolean;
}

const ProductoForm: React.FC<ProductoFormProps> = ({
  producto, marcas, categorias, onSave, onCancel, loading
}) => {
  const [form, setForm] = useState({
    sku: '',
    codigo: '',
    marcaId: '',
    categoriaId: '',
    modelo: '',
    especificacion: '',
    descripcion: '',
    precio: '',
    diasGarantia: '365',
    visibilidad: 'Publico',
    pesoKg: '',
    altoCm: '',
    anchoCm: '',
    profundidadCm: '',
    activo: true
  });

  const [imagenPrincipal, setImagenPrincipal] = useState<File | null>(null);
  const [imagenesAdicionales, setImagenesAdicionales] = useState<File[]>([]);
  const [imagenesActuales, setImagenesActuales] = useState<string[]>([]);
  const [imagenesAEliminar, setImagenesAEliminar] = useState<string[]>([]);

  useEffect(() => {
    if (producto) {
      setForm({
        sku: producto.sku,
        codigo: producto.codigo,
        marcaId: producto.marcaId.toString(),
        categoriaId: producto.categoriaId?.toString() || '',
        modelo: producto.modelo,
        especificacion: producto.especificacion,
        descripcion: producto.descripcion || '',
        precio: producto.precio.toString(),
        diasGarantia: producto.diasGarantia.toString(),
        visibilidad: producto.visibilidad,
        pesoKg: producto.pesoKg?.toString() || '',
        altoCm: producto.altoCm.toString(),
        anchoCm: producto.anchoCm.toString(),
        profundidadCm: producto.profundidadCm.toString(),
        activo: producto.activo
      });
      setImagenesActuales(producto.imagenesAdicionales || []);
    }
  }, [producto]);

  const handleChange = (e: React.ChangeEvent<HTMLInputElement | HTMLTextAreaElement>) => {
    const { name, value, type } = e.target;
    setForm(prev => ({
      ...prev,
      [name]: type === 'checkbox' ? (e.target as HTMLInputElement).checked : value
    }));
  };

  const handleSelectChange = (name: string) => (e: any) => {
    setForm(prev => ({ ...prev, [name]: e.target.value }));
  };

  const handleFileChange = (e: React.ChangeEvent<HTMLInputElement>, isPrincipal: boolean) => {
    const files = e.target.files;
    if (!files) return;
    if (isPrincipal) {
      setImagenPrincipal(files[0]);
    } else {
      setImagenesAdicionales(prev => [...prev, ...Array.from(files)]);
    }
  };

  const handleRemoveImagenActual = (url: string) => {
    setImagenesAEliminar(prev => [...prev, url]);
    setImagenesActuales(prev => prev.filter(u => u !== url));
  };

  const handleRemoveNuevaImagen = (index: number) => {
    setImagenesAdicionales(prev => prev.filter((_, i) => i !== index));
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    const formData: any = {
      ...form,
      marcaId: Number(form.marcaId),
      categoriaId: form.categoriaId ? Number(form.categoriaId) : undefined,
      precio: Number(form.precio),
      diasGarantia: Number(form.diasGarantia),
      pesoKg: form.pesoKg ? Number(form.pesoKg) : undefined,
      altoCm: Number(form.altoCm),
      anchoCm: Number(form.anchoCm),
      profundidadCm: Number(form.profundidadCm),
      imagenPrincipal: imagenPrincipal || undefined,
      imagenesAdicionales: imagenesAdicionales.length ? imagenesAdicionales : undefined,
      imagenesAEliminar: imagenesAEliminar.length ? imagenesAEliminar : undefined
    };
    await onSave(formData);
  };

  return (
    <Box component="form" onSubmit={handleSubmit} sx={{ mt: 2 }}>
      <Grid container spacing={2}>
        <Grid size={{ xs: 12, md: 6 }}>
          <TextField
            label="SKU"
            name="sku"
            value={form.sku}
            onChange={handleChange}
            fullWidth
            required
            size="small"
          />
        </Grid>
        <Grid size={{ xs: 12, md: 6 }}>
          <TextField
            label="Código"
            name="codigo"
            value={form.codigo}
            onChange={handleChange}
            fullWidth
            required
            size="small"
          />
        </Grid>

        <Grid size={{ xs: 12, md: 6 }}>
          <FormControl fullWidth size="small" required>
            <InputLabel>Marca</InputLabel>
            <Select
              name="marcaId"
              value={form.marcaId}
              label="Marca"
              onChange={handleSelectChange('marcaId')}
            >
              {marcas.map(m => (
                <MenuItem key={m.id} value={m.id}>{m.nombre}</MenuItem>
              ))}
            </Select>
          </FormControl>
        </Grid>

        <Grid size={{ xs: 12, md: 6 }}>
          <FormControl fullWidth size="small">
            <InputLabel>Categoría</InputLabel>
            <Select
              name="categoriaId"
              value={form.categoriaId}
              label="Categoría"
              onChange={handleSelectChange('categoriaId')}
            >
              <MenuItem value="">Sin categoría</MenuItem>
              {categorias.map(c => (
                <MenuItem key={c.id} value={c.id}>{c.nombre}</MenuItem>
              ))}
            </Select>
          </FormControl>
        </Grid>

        <Grid size={{ xs: 12, md: 6 }}>
          <TextField
            label="Modelo"
            name="modelo"
            value={form.modelo}
            onChange={handleChange}
            fullWidth
            required
            size="small"
          />
        </Grid>

        <Grid size={{ xs: 12, md: 6 }}>
          <TextField
            label="Precio"
            name="precio"
            type="number"
            value={form.precio}
            onChange={handleChange}
            fullWidth
            required
            size="small"
            inputProps={{ step: '0.01', min: 0 }}
          />
        </Grid>

        <Grid size={{ xs: 12, md: 6 }}>
          <TextField
            label="Días de Garantía"
            name="diasGarantia"
            type="number"
            value={form.diasGarantia}
            onChange={handleChange}
            fullWidth
            required
            size="small"
            inputProps={{ min: 0 }}
          />
        </Grid>

        <Grid size={{ xs: 12, md: 6 }}>
          <FormControl fullWidth size="small">
            <InputLabel>Visibilidad</InputLabel>
            <Select
              name="visibilidad"
              value={form.visibilidad}
              label="Visibilidad"
              onChange={handleSelectChange('visibilidad')}
            >
              <MenuItem value="Publico">Público</MenuItem>
              <MenuItem value="Privado">Privado</MenuItem>
              <MenuItem value="Oculta">Oculta</MenuItem>
            </Select>
          </FormControl>
        </Grid>

        <Grid size={{ xs: 12, md: 3 }}>
          <TextField
            label="Peso (kg)"
            name="pesoKg"
            type="number"
            value={form.pesoKg}
            onChange={handleChange}
            fullWidth
            size="small"
            inputProps={{ step: '0.001', min: 0 }}
          />
        </Grid>

        <Grid size={{ xs: 12, md: 3 }}>
          <TextField
            label="Alto (cm)"
            name="altoCm"
            type="number"
            value={form.altoCm}
            onChange={handleChange}
            fullWidth
            required
            size="small"
            inputProps={{ step: '0.01', min: 0 }}
          />
        </Grid>

        <Grid size={{ xs: 12, md: 3 }}>
          <TextField
            label="Ancho (cm)"
            name="anchoCm"
            type="number"
            value={form.anchoCm}
            onChange={handleChange}
            fullWidth
            required
            size="small"
            inputProps={{ step: '0.01', min: 0 }}
          />
        </Grid>

        <Grid size={{ xs: 12, md: 3 }}>
          <TextField
            label="Profundidad (cm)"
            name="profundidadCm"
            type="number"
            value={form.profundidadCm}
            onChange={handleChange}
            fullWidth
            required
            size="small"
            inputProps={{ step: '0.01', min: 0 }}
          />
        </Grid>

        <Grid size={{ xs: 12 }}>
          <TextField
            label="Especificaciones"
            name="especificacion"
            value={form.especificacion}
            onChange={handleChange}
            fullWidth
            required
            multiline
            rows={2}
            size="small"
          />
        </Grid>

        <Grid size={{ xs: 12 }}>
          <TextField
            label="Descripción"
            name="descripcion"
            value={form.descripcion}
            onChange={handleChange}
            fullWidth
            multiline
            rows={3}
            size="small"
          />
        </Grid>

        {/* Imágenes */}
        <Grid size={{ xs: 12 }}>
          <Typography variant="subtitle2" gutterBottom>Imagen Principal</Typography>
          <Button
            variant="outlined"
            component="label"
            startIcon={<CloudUpload />}
            size="small"
          >
            Seleccionar archivo
            <input type="file" hidden accept="image/*" onChange={(e) => handleFileChange(e, true)} />
          </Button>
          {imagenPrincipal && (
            <Box sx={{ mt: 1, display: 'flex', alignItems: 'center' }}>
              <Chip
                label={imagenPrincipal.name}
                onDelete={() => setImagenPrincipal(null)}
                size="small"
              />
            </Box>
          )}
          {producto?.imagenUrl && !imagenPrincipal && (
            <Box sx={{ mt: 1 }}>
              <Typography variant="caption">Actual: {producto.imagenUrl.split('/').pop()}</Typography>
            </Box>
          )}
        </Grid>

        <Grid size={{ xs: 12 }}>
          <Typography variant="subtitle2" gutterBottom>Imágenes Adicionales</Typography>
          <Button
            variant="outlined"
            component="label"
            startIcon={<CloudUpload />}
            size="small"
          >
            Agregar imágenes
            <input type="file" hidden multiple accept="image/*" onChange={(e) => handleFileChange(e, false)} />
          </Button>

          <Stack direction="row" spacing={1} sx={{ mt: 1, flexWrap: 'wrap' }}>
            {imagenesActuales.map((url, idx) => (
              <Chip
                key={url}
                label={`img_${idx + 1}`}
                onDelete={() => handleRemoveImagenActual(url)}
                size="small"
              />
            ))}
            {imagenesAdicionales.map((file, idx) => (
              <Chip
                key={idx}
                label={file.name}
                onDelete={() => handleRemoveNuevaImagen(idx)}
                size="small"
                color="primary"
              />
            ))}
          </Stack>
        </Grid>

        {producto && (
          <Grid size={{ xs: 12 }}>
            <FormControlLabel
              control={
                <Switch
                  checked={form.activo}
                  onChange={(e) => setForm({ ...form, activo: e.target.checked })}
                  name="activo"
                />
              }
              label="Activo"
            />
          </Grid>
        )}

        <Grid size={{ xs: 12 }} sx={{ display: 'flex', justifyContent: 'flex-end', gap: 2, mt: 2 }}>
          <Button onClick={onCancel} variant="outlined" disabled={loading}>
            Cancelar
          </Button>
          <Button type="submit" variant="contained" disabled={loading}>
            {loading ? 'Guardando...' : (producto ? 'Actualizar' : 'Crear')}
          </Button>
        </Grid>
      </Grid>
    </Box>
  );
};

export default ProductoForm;