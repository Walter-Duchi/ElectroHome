/* eslint-disable react-hooks/set-state-in-effect */
import React, { useState, useEffect } from 'react';
import {
  Box, TextField, MenuItem, FormControl, InputLabel, Select,
  Grid, Button, FormControlLabel, Switch
} from '@mui/material';
import type { Categoria } from '../../src/types/producto';

interface CategoriaFormProps {
  categoria?: Categoria | null;
  categoriasPadre: Categoria[];
  onSave: (data: any) => Promise<void>;
  onCancel: () => void;
}

const CategoriaForm: React.FC<CategoriaFormProps> = ({
  categoria, categoriasPadre, onSave, onCancel
}) => {
  const [form, setForm] = useState({
    nombre: '',
    descripcion: '',
    categoriaPadreId: '',
    activo: true
  });

  useEffect(() => {
    if (categoria) {
      setForm({
        nombre: categoria.nombre,
        descripcion: categoria.descripcion || '',
        categoriaPadreId: categoria.categoriaPadreId?.toString() || '',
        activo: categoria.activo
      });
    }
  }, [categoria]);

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

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    const data: any = {
      nombre: form.nombre,
      descripcion: form.descripcion || undefined,
      categoriaPadreId: form.categoriaPadreId ? Number(form.categoriaPadreId) : undefined,
      activo: form.activo
    };
    await onSave(data);
  };

  return (
    <Box component="form" onSubmit={handleSubmit} sx={{ mt: 2 }}>
      <Grid container spacing={2}>
        <Grid size={{ xs: 12}}>
          <TextField
            label="Nombre"
            name="nombre"
            value={form.nombre}
            onChange={handleChange}
            fullWidth
            required
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
            rows={2}
            size="small"
          />
        </Grid>
        <Grid size={{ xs: 12 }}>
          <FormControl fullWidth size="small">
            <InputLabel>Categoría Padre</InputLabel>
            <Select
              name="categoriaPadreId"
              value={form.categoriaPadreId}
              label="Categoría Padre"
              onChange={handleSelectChange('categoriaPadreId')}
            >
              <MenuItem value="">Ninguna</MenuItem>
              {categoriasPadre.map(c => (
                <MenuItem key={c.id} value={c.id}>{c.nombre}</MenuItem>
              ))}
            </Select>
          </FormControl>
        </Grid>
        {categoria && (
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
          <Button onClick={onCancel} variant="outlined">
            Cancelar
          </Button>
          <Button type="submit" variant="contained">
            {categoria ? 'Actualizar' : 'Crear'}
          </Button>
        </Grid>
      </Grid>
    </Box>
  );
};

export default CategoriaForm;
