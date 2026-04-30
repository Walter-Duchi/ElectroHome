/* eslint-disable react-hooks/set-state-in-effect */
import React, { useState, useEffect } from 'react';
import {
  Box, TextField, Grid, Button
} from '@mui/material';
import type { Marca } from '../../src/types/producto';

interface MarcaFormProps {
  marca?: Marca | null;
  onSave: (data: any) => Promise<void>;
  onCancel: () => void;
}

const MarcaForm: React.FC<MarcaFormProps> = ({
  marca, onSave, onCancel
}) => {
  const [form, setForm] = useState({
    nombre: ''
  });

  useEffect(() => {
    if (marca) {
      setForm({ nombre: marca.nombre });
    }
  }, [marca]);

  const handleChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    setForm({ ...form, nombre: e.target.value });
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    await onSave({ nombre: form.nombre });
  };

  return (
    <Box component="form" onSubmit={handleSubmit} sx={{ mt: 2 }}>
      <Grid container spacing={2}>
        <Grid size={{ xs: 12 }}>
          <TextField
            label="Nombre de la Marca"
            name="nombre"
            value={form.nombre}
            onChange={handleChange}
            fullWidth
            required
            size="small"
          />
        </Grid>

        <Grid size={{ xs: 12 }} sx={{ display: 'flex', justifyContent: 'flex-end', gap: 2, mt: 2 }}>
          <Button onClick={onCancel} variant="outlined">
            Cancelar
          </Button>
          <Button type="submit" variant="contained">
            {marca ? 'Actualizar' : 'Crear'}
          </Button>
        </Grid>
      </Grid>
    </Box>
  );
};

export default MarcaForm;
