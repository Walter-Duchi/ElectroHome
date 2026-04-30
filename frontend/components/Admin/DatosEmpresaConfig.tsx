import React, { useState, useEffect } from 'react';
import {
  Box,
  Card,
  CardContent,
  Typography,
  TextField,
  Button,
  Alert,
  CircularProgress,
  Grid,
  Divider,
  Paper,
  InputAdornment,
  Snackbar
} from '@mui/material';
import {
  Business,
  Storefront,
  Description,
  LocationOn,
  Save,
  Edit,
  Cancel
} from '@mui/icons-material';
import { empresaService } from '../../services/empresaService';
import type { DatosEmpresa, UpdateDatosEmpresaRequest } from '../../src/types/empresa';

const DatosEmpresaConfig: React.FC = () => {
  const [datos, setDatos] = useState<DatosEmpresa>({
    id: 0,
    rucEmpresa: '',
    nombreComercial: '',
    razonSocial: '',
    direccionMatriz: ''
  });
  const [originalDatos, setOriginalDatos] = useState<DatosEmpresa | null>(null);
  const [loading, setLoading] = useState(true);
  const [saving, setSaving] = useState(false);
  const [error, setError] = useState('');
  const [success, setSuccess] = useState('');
  const [isEditing, setIsEditing] = useState(false);
  const [snackbarOpen, setSnackbarOpen] = useState(false);

  useEffect(() => {
    cargarDatos();
  }, []);

  const cargarDatos = async () => {
    try {
      setLoading(true);
      setError('');
      const data = await empresaService.obtenerDatosEmpresa();
      setDatos(data);
      setOriginalDatos(data);
      // Si no hay datos (id = 0), se considera que no hay configuración previa y se activa edición
      if (data.id === 0) {
        setIsEditing(true);
      }
    } catch (err: any) {
      setError(err.message || 'Error al cargar los datos de la empresa');
    } finally {
      setLoading(false);
    }
  };

  const handleInputChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    const { name, value } = e.target;
    setDatos(prev => ({
      ...prev,
      [name]: value
    }));
  };

  const handleSave = async () => {
    // Validaciones básicas
    if (!datos.rucEmpresa.trim()) {
      setError('El RUC de la empresa es obligatorio');
      return;
    }
    if (!datos.nombreComercial.trim()) {
      setError('El nombre comercial es obligatorio');
      return;
    }
    if (!datos.razonSocial.trim()) {
      setError('La razón social es obligatoria');
      return;
    }
    if (!datos.direccionMatriz.trim()) {
      setError('La dirección matriz es obligatoria');
      return;
    }

    setSaving(true);
    setError('');
    setSuccess('');

    try {
      const request: UpdateDatosEmpresaRequest = {
        rucEmpresa: datos.rucEmpresa,
        nombreComercial: datos.nombreComercial,
        razonSocial: datos.razonSocial,
        direccionMatriz: datos.direccionMatriz
      };

      const response = await empresaService.actualizarDatosEmpresa(request);
      setDatos(response);
      setOriginalDatos(response);
      setIsEditing(false);
      setSuccess('Datos de la empresa actualizados correctamente');
      setSnackbarOpen(true);
    } catch (err: any) {
      setError(err.response?.data?.message || err.message || 'Error al guardar los datos');
    } finally {
      setSaving(false);
    }
  };

  const handleCancel = () => {
    if (originalDatos) {
      setDatos(originalDatos);
    }
    setIsEditing(false);
    setError('');
  };

  const handleEdit = () => {
    setIsEditing(true);
  };

  const handleSnackbarClose = () => {
    setSnackbarOpen(false);
  };

  if (loading) {
    return (
      <Box sx={{ display: 'flex', justifyContent: 'center', alignItems: 'center', minHeight: '60vh' }}>
        <CircularProgress size={60} />
      </Box>
    );
  }

  return (
    <Box sx={{ maxWidth: 800, mx: 'auto', mt: 4, p: 2 }}>
      <Paper elevation={3} sx={{ p: 3, borderRadius: 3 }}>
        <Box sx={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between', mb: 3 }}>
          <Box sx={{ display: 'flex', alignItems: 'center', gap: 2 }}>
            <Business sx={{ fontSize: 40, color: 'primary.main' }} />
            <Typography variant="h5" component="h1" fontWeight={600}>
              Configuración de la Empresa
            </Typography>
          </Box>
          {!isEditing && (
            <Button
              variant="contained"
              startIcon={<Edit />}
              onClick={handleEdit}
              sx={{ borderRadius: 2 }}
            >
              Editar
            </Button>
          )}
        </Box>

        <Divider sx={{ mb: 3 }} />

        {error && (
          <Alert severity="error" sx={{ mb: 3, borderRadius: 2 }}>
            {error}
          </Alert>
        )}

        <Card variant="outlined" sx={{ borderRadius: 2, mb: 3 }}>
          <CardContent>
            <Typography variant="body2" color="text.secondary" sx={{ mb: 2 }}>
              Los siguientes datos corresponden a la información legal y comercial de la empresa.
              Solo un administrador puede modificarlos y siempre existirá una única fila en la base de datos.
            </Typography>

            <Grid container spacing={3}>
              <Grid size={{ xs: 12, sm: 6 }}>
                <TextField
                  fullWidth
                  label="RUC de la Empresa"
                  name="rucEmpresa"
                  value={datos.rucEmpresa}
                  onChange={handleInputChange}
                  disabled={!isEditing || saving}
                  required
                  variant="outlined"
                  InputProps={{
                    startAdornment: (
                      <InputAdornment position="start">
                        <Description color="action" />
                      </InputAdornment>
                    ),
                  }}
                />
              </Grid>
              <Grid size={{ xs: 12, sm: 6 }}>
                <TextField
                  fullWidth
                  label="Nombre Comercial"
                  name="nombreComercial"
                  value={datos.nombreComercial}
                  onChange={handleInputChange}
                  disabled={!isEditing || saving}
                  required
                  variant="outlined"
                  InputProps={{
                    startAdornment: (
                      <InputAdornment position="start">
                        <Storefront color="action" />
                      </InputAdornment>
                    ),
                  }}
                />
              </Grid>
              <Grid size={{ xs: 12 }}>
                <TextField
                  fullWidth
                  label="Razón Social"
                  name="razonSocial"
                  value={datos.razonSocial}
                  onChange={handleInputChange}
                  disabled={!isEditing || saving}
                  required
                  variant="outlined"
                  InputProps={{
                    startAdornment: (
                      <InputAdornment position="start">
                        <Business color="action" />
                      </InputAdornment>
                    ),
                  }}
                />
              </Grid>
              <Grid size={{ xs: 12 }}>
                <TextField
                  fullWidth
                  label="Dirección Matriz"
                  name="direccionMatriz"
                  value={datos.direccionMatriz}
                  onChange={handleInputChange}
                  disabled={!isEditing || saving}
                  required
                  multiline
                  rows={2}
                  variant="outlined"
                  InputProps={{
                    startAdornment: (
                      <InputAdornment position="start">
                        <LocationOn color="action" />
                      </InputAdornment>
                    ),
                  }}
                />
              </Grid>
            </Grid>
          </CardContent>
        </Card>

        {isEditing && (
          <Box sx={{ display: 'flex', justifyContent: 'flex-end', gap: 2 }}>
            <Button
              variant="outlined"
              color="inherit"
              startIcon={<Cancel />}
              onClick={handleCancel}
              disabled={saving}
              sx={{ borderRadius: 2 }}
            >
              Cancelar
            </Button>
            <Button
              variant="contained"
              color="primary"
              startIcon={saving ? <CircularProgress size={20} color="inherit" /> : <Save />}
              onClick={handleSave}
              disabled={saving}
              sx={{ borderRadius: 2, minWidth: 120 }}
            >
              {saving ? 'Guardando...' : 'Guardar'}
            </Button>
          </Box>
        )}

        <Box sx={{ mt: 4 }}>
          <Alert severity="info" sx={{ borderRadius: 2 }}>
            <Typography variant="body2">
              <strong>Nota:</strong> Esta configuración solo puede ser modificada por usuarios con rol <strong>Administrador</strong>.
              Siempre existirá una única fila en la tabla <strong>Datos_Empresa</strong>.
            </Typography>
          </Alert>
        </Box>
      </Paper>

      <Snackbar
        open={snackbarOpen}
        autoHideDuration={4000}
        onClose={handleSnackbarClose}
        anchorOrigin={{ vertical: 'bottom', horizontal: 'center' }}
      >
        <Alert onClose={handleSnackbarClose} severity="success" sx={{ width: '100%', borderRadius: 2 }}>
          {success}
        </Alert>
      </Snackbar>
    </Box>
  );
};

export default DatosEmpresaConfig;