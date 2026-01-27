import React, { useState, useEffect } from 'react';
import {
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
  TextField,
  Button,
  MenuItem,
  Grid,
  Alert,
  CircularProgress,
  Typography,
  Box,
  Stepper,
  Step,
  StepLabel,
  InputAdornment,
  IconButton,
  Checkbox,
  FormControlLabel,
  FormGroup,
  Snackbar,
  Paper,
  Divider,
} from '@mui/material';
import {
  Person,
  Email,
  Phone,
  Business,
  AccountBalance,
  LocationCity,
  LocationOn,
  Home,
  Badge,
  Password,
  ContentCopy,
} from '@mui/icons-material';
import { userService } from '../../services/userService';
import { type CreateUserResponse } from '../../src/types/user';

interface CreateUserModalProps {
  onClose: () => void;
}

// Lista de 25 cantones del Guayas
const CANTONES_GUAYAS = [
  "Guayaquil", "Durán", "Milagro", "Daule", "Samborondón",
  "Naranjal", "Playas", "Balzar", "El Triunfo", "Yaguachi",
  "Naranjito", "Santa Lucía", "Pedro Carbo", "Salitre",
  "General Villamil (Playas)", "Coronel Marcelino Maridueña",
  "Nobol", "Lomas de Sargentillo", "Alfredo Baquerizo Moreno",
  "Balao", "Colimes", "Palestina", "Simón Bolívar", "Crnel. Lorenzo de Garaicoa",
  "El Empalme"
];

// Roles que puede crear el administrador
const ROLES_ADMIN = [
  "Revisor", "Tecnico", "Personal de Entrega", "Vendedor",
  "Analista_Datos", "Encargado_Inventario", "Gestor_Productos", "Administrador"
];

const steps = ['Información Personal', 'Datos de Contacto y Ubicación', 'Información Bancaria y Finalización'];

function CreateUserModal({ onClose }: CreateUserModalProps) {
  const [activeStep, setActiveStep] = useState(0);
  const [formData, setFormData] = useState({
    nombres: '',
    apellidos: '',
    razonSocial: '',
    tipoIdentificacion: 'Cedula' as 'Cedula' | 'Pasaporte',
    identificacion: '',
    ruc: '',
    correo: '',
    celular: '',
    convencional: '',
    ciudad: 'Guayaquil',
    codigoPostal: '',
    direccion: '',
    rol: 'Revisor',
    numCuentaBancaria: '',
    tipoCuentaBancaria: 'Ahorro' as 'Ahorro' | 'Corriente',
    contribuyenteEspecial: false,
    obligadoContabilidad: false,
  });

  const [errors, setErrors] = useState<Record<string, string>>({});
  const [loading, setLoading] = useState(false);
  const [submitError, setSubmitError] = useState('');
  const [successResponse, setSuccessResponse] = useState<CreateUserResponse | null>(null);
  const [showPassword, setShowPassword] = useState(false);
  const [copied, setCopied] = useState(false);

  useEffect(() => {
    // Si es pasaporte, limpiar validaciones de cédula
    if (formData.tipoIdentificacion === 'Pasaporte') {
      if (errors.identificacion?.includes('cédula')) {
        setErrors(prev => ({ ...prev, identificacion: '' }));
      }
    }
  }, [formData.tipoIdentificacion]);

  const handleNext = () => {
    if (validateStep(activeStep)) {
      setActiveStep((prevStep) => prevStep + 1);
    }
  };

  const handleBack = () => {
    setActiveStep((prevStep) => prevStep - 1);
  };

  const validateStep = (step: number): boolean => {
    const newErrors: Record<string, string> = {};

    switch (step) {
      case 0: // Información Personal
        if (!formData.nombres.trim()) newErrors.nombres = 'Nombres son requeridos';
        if (!formData.apellidos.trim()) newErrors.apellidos = 'Apellidos son requeridos';

        if (formData.tipoIdentificacion === 'Cedula') {
          if (!formData.identificacion.trim()) newErrors.identificacion = 'Cédula es requerida';
          else if (!userService.validateCedula(formData.identificacion)) {
            newErrors.identificacion = 'Cédula ecuatoriana inválida';
          }
        } else {
          if (!formData.identificacion.trim()) newErrors.identificacion = 'Pasaporte es requerido';
          else if (formData.identificacion.length < 8) {
            newErrors.identificacion = 'Pasaporte debe tener al menos 8 caracteres';
          }
        }

        if (!formData.correo.trim()) newErrors.correo = 'Correo es requerido';
        else if (!/\S+@\S+\.\S+/.test(formData.correo)) newErrors.correo = 'Correo no válido';
        break;

      case 1: // Datos de Contacto y Ubicación
        if (!formData.celular.trim()) newErrors.celular = 'Celular es requerido';
        else if (!/^09\d{8}$/.test(formData.celular.replace(/\D/g, ''))) {
          newErrors.celular = 'Celular ecuatoriano inválido (09XXXXXXXX)';
        }

        if (!formData.ciudad.trim()) newErrors.ciudad = 'Ciudad es requerida';
        if (!formData.codigoPostal.trim()) newErrors.codigoPostal = 'Código postal es requerido';
        else if (!userService.validatePostalCode(formData.codigoPostal)) {
          newErrors.codigoPostal = 'Código postal inválido (6 dígitos)';
        }

        if (!formData.direccion.trim()) newErrors.direccion = 'Dirección es requerida';
        break;

      case 2: // Información Bancaria
        if (!formData.numCuentaBancaria.trim()) {
          newErrors.numCuentaBancaria = 'Número de cuenta bancaria es obligatorio';
        } else if (!userService.validateBankAccount(formData.numCuentaBancaria)) {
          newErrors.numCuentaBancaria = 'Número de cuenta bancaria inválido (10-20 dígitos)';
        }

        if (!formData.tipoCuentaBancaria) {
          newErrors.tipoCuentaBancaria = 'Tipo de cuenta bancaria es obligatorio';
        }
        break;
    }

    setErrors(newErrors);
    return Object.keys(newErrors).length === 0;
  };

  const handleInputChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    const { name, value, type, checked } = e.target;

    setFormData(prev => ({
      ...prev,
      [name]: type === 'checkbox' ? checked : value
    }));

    if (errors[name]) {
      setErrors(prev => ({ ...prev, [name]: '' }));
    }
  };

  const handleSelectChange = (name: string, value: any) => {
    setFormData(prev => ({ ...prev, [name]: value }));

    if (errors[name]) {
      setErrors(prev => ({ ...prev, [name]: '' }));
    }
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();

    if (!validateStep(activeStep) && activeStep < steps.length - 1) {
      return;
    }

    if (activeStep === steps.length - 1) {
      setLoading(true);
      setSubmitError('');

      try {
        const response = await userService.createUser(formData);
        setSuccessResponse(response);
      } catch (error: unknown) {
        if (error instanceof Error) {
          setSubmitError(error.message);
        } else {
          setSubmitError('Error desconocido al crear usuario');
        }
      } finally {
        setLoading(false);
      }
    } else {
      handleNext();
    }
  };

  const handleCopyPassword = () => {
    if (successResponse?.contrasenaGenerada) {
      navigator.clipboard.writeText(successResponse.contrasenaGenerada);
      setCopied(true);
      setTimeout(() => setCopied(false), 2000);
    }
  };

  const handleCloseModal = () => {
    if (successResponse) {
      // Reiniciar formulario si ya se creó exitosamente
      setFormData({
        nombres: '',
        apellidos: '',
        razonSocial: '',
        tipoIdentificacion: 'Cedula',
        identificacion: '',
        ruc: '',
        correo: '',
        celular: '',
        convencional: '',
        ciudad: 'Guayaquil',
        codigoPostal: '',
        direccion: '',
        rol: 'Revisor',
        numCuentaBancaria: '',
        tipoCuentaBancaria: 'Ahorro',
        contribuyenteEspecial: false,
        obligadoContabilidad: false,
      });
      setActiveStep(0);
      setSuccessResponse(null);
    }
    onClose();
  };

  const getStepContent = (step: number) => {
    switch (step) {
      case 0:
        return (
          <Grid container spacing={3}>
            <Grid size={{ xs: 12, sm: 6 }}>
              <TextField
                label="Nombres *"
                name="nombres"
                value={formData.nombres}
                onChange={handleInputChange}
                error={!!errors.nombres}
                helperText={errors.nombres}
                disabled={loading}
                fullWidth
                InputProps={{
                  startAdornment: (
                    <InputAdornment position="start">
                      <Person color="action" />
                    </InputAdornment>
                  ),
                }}
              />
            </Grid>
            <Grid size={{ xs: 12, sm: 6 }}>
              <TextField
                label="Apellidos *"
                name="apellidos"
                value={formData.apellidos}
                onChange={handleInputChange}
                error={!!errors.apellidos}
                helperText={errors.apellidos}
                disabled={loading}
                fullWidth
                InputProps={{
                  startAdornment: (
                    <InputAdornment position="start">
                      <Person color="action" />
                    </InputAdornment>
                  ),
                }}
              />
            </Grid>
            <Grid size={{ xs: 12 }}>
              <TextField
                label="Razón Social (Opcional)"
                name="razonSocial"
                value={formData.razonSocial}
                onChange={handleInputChange}
                disabled={loading}
                fullWidth
                InputProps={{
                  startAdornment: (
                    <InputAdornment position="start">
                      <Business color="action" />
                    </InputAdornment>
                  ),
                }}
              />
            </Grid>
            <Grid size={{ xs: 12, sm: 6 }}>
              <TextField
                select
                label="Tipo de Identificación *"
                name="tipoIdentificacion"
                value={formData.tipoIdentificacion}
                onChange={(e) => handleSelectChange('tipoIdentificacion', e.target.value)}
                error={!!errors.tipoIdentificacion}
                helperText={errors.tipoIdentificacion}
                disabled={loading}
                fullWidth
              >
                <MenuItem value="Cedula">Cédula Ecuatoriana</MenuItem>
                <MenuItem value="Pasaporte">Pasaporte</MenuItem>
              </TextField>
            </Grid>
            <Grid size={{ xs: 12, sm: 6 }}>
              <TextField
                label={formData.tipoIdentificacion === 'Cedula' ? 'Cédula *' : 'Pasaporte *'}
                name="identificacion"
                value={formData.identificacion}
                onChange={handleInputChange}
                error={!!errors.identificacion}
                helperText={errors.identificacion}
                disabled={loading}
                fullWidth
                InputProps={{
                  startAdornment: (
                    <InputAdornment position="start">
                      <Badge color="action" />
                    </InputAdornment>
                  ),
                }}
              />
            </Grid>
            <Grid size={{ xs: 12, sm: 6 }}>
              <TextField
                label="RUC (Opcional)"
                name="ruc"
                value={formData.ruc}
                onChange={handleInputChange}
                inputProps={{ maxLength: 13 }}
                disabled={loading}
                fullWidth
                InputProps={{
                  startAdornment: (
                    <InputAdornment position="start">
                      <Business color="action" />
                    </InputAdornment>
                  ),
                }}
              />
            </Grid>
            <Grid size={{ xs: 12, sm: 6 }}>
              <TextField
                label="Correo Electrónico *"
                name="correo"
                type="email"
                value={formData.correo}
                onChange={handleInputChange}
                error={!!errors.correo}
                helperText={errors.correo}
                disabled={loading}
                fullWidth
                InputProps={{
                  startAdornment: (
                    <InputAdornment position="start">
                      <Email color="action" />
                    </InputAdornment>
                  ),
                }}
              />
            </Grid>
          </Grid>
        );

      case 1:
        return (
          <Grid container spacing={3}>
            <Grid size={{ xs: 12, sm: 6 }}>
              <TextField
                label="Celular *"
                name="celular"
                value={formData.celular}
                onChange={handleInputChange}
                error={!!errors.celular}
                helperText={errors.celular}
                disabled={loading}
                fullWidth
                placeholder="0991234567"
                InputProps={{
                  startAdornment: (
                    <InputAdornment position="start">
                      <Phone color="action" />
                    </InputAdornment>
                  ),
                }}
              />
            </Grid>
            <Grid size={{ xs: 12, sm: 6 }}>
              <TextField
                label="Teléfono Convencional (Opcional)"
                name="convencional"
                value={formData.convencional}
                onChange={handleInputChange}
                disabled={loading}
                fullWidth
                InputProps={{
                  startAdornment: (
                    <InputAdornment position="start">
                      <Phone color="action" />
                    </InputAdornment>
                  ),
                }}
              />
            </Grid>
            <Grid size={{ xs: 12, sm: 6 }}>
              <TextField
                select
                label="Ciudad *"
                name="ciudad"
                value={formData.ciudad}
                onChange={(e) => handleSelectChange('ciudad', e.target.value)}
                error={!!errors.ciudad}
                helperText={errors.ciudad}
                disabled={loading}
                fullWidth
                InputProps={{
                  startAdornment: (
                    <InputAdornment position="start">
                      <LocationCity color="action" />
                    </InputAdornment>
                  ),
                }}
              >
                {CANTONES_GUAYAS.map((ciudad) => (
                  <MenuItem key={ciudad} value={ciudad}>
                    {ciudad}
                  </MenuItem>
                ))}
              </TextField>
            </Grid>
            <Grid size={{ xs: 12, sm: 6 }}>
              <TextField
                label="Código Postal *"
                name="codigoPostal"
                value={formData.codigoPostal}
                onChange={handleInputChange}
                error={!!errors.codigoPostal}
                helperText={errors.codigoPostal}
                disabled={loading}
                fullWidth
                placeholder="090101"
                InputProps={{
                  startAdornment: (
                    <InputAdornment position="start">
                      <LocationOn color="action" />
                    </InputAdornment>
                  ),
                }}
              />
            </Grid>
            <Grid size={{ xs: 12 }}>
              <TextField
                label="Dirección Domiciliaria *"
                name="direccion"
                value={formData.direccion}
                onChange={handleInputChange}
                error={!!errors.direccion}
                helperText={errors.direccion}
                disabled={loading}
                fullWidth
                multiline
                rows={2}
                placeholder="Calle principal y secundaria, número de casa, sector"
                InputProps={{
                  startAdornment: (
                    <InputAdornment position="start">
                      <Home color="action" />
                    </InputAdornment>
                  ),
                }}
              />
            </Grid>
            <Grid size={{ xs: 12 }}>
              <TextField
                select
                label="Rol del Empleado *"
                name="rol"
                value={formData.rol}
                onChange={(e) => handleSelectChange('rol', e.target.value)}
                disabled={loading}
                fullWidth
              >
                {ROLES_ADMIN.map((rol) => (
                  <MenuItem key={rol} value={rol}>
                    {rol}
                  </MenuItem>
                ))}
              </TextField>
            </Grid>
          </Grid>
        );

      case 2:
        return (
          <Grid container spacing={3}>
            <Grid size={{ xs: 12, sm: 6 }}>
              <TextField
                label="Número de Cuenta Bancaria *"
                name="numCuentaBancaria"
                value={formData.numCuentaBancaria}
                onChange={handleInputChange}
                error={!!errors.numCuentaBancaria}
                helperText={errors.numCuentaBancaria}
                disabled={loading}
                fullWidth
                placeholder="12345678901234567890"
                InputProps={{
                  startAdornment: (
                    <InputAdornment position="start">
                      <AccountBalance color="action" />
                    </InputAdornment>
                  ),
                }}
              />
            </Grid>
            <Grid size={{ xs: 12, sm: 6 }}>
              <TextField
                select
                label="Tipo de Cuenta *"
                name="tipoCuentaBancaria"
                value={formData.tipoCuentaBancaria}
                onChange={(e) => handleSelectChange('tipoCuentaBancaria', e.target.value)}
                error={!!errors.tipoCuentaBancaria}
                helperText={errors.tipoCuentaBancaria}
                disabled={loading}
                fullWidth
              >
                <MenuItem value="Ahorro">Ahorro</MenuItem>
                <MenuItem value="Corriente">Corriente</MenuItem>
              </TextField>
            </Grid>
            <Grid size={{ xs: 12 }}>
              <FormGroup>
                <FormControlLabel
                  control={
                    <Checkbox
                      checked={formData.contribuyenteEspecial}
                      onChange={handleInputChange}
                      name="contribuyenteEspecial"
                      color="primary"
                    />
                  }
                  label="Contribuyente Especial"
                />
                <FormControlLabel
                  control={
                    <Checkbox
                      checked={formData.obligadoContabilidad}
                      onChange={handleInputChange}
                      name="obligadoContabilidad"
                      color="primary"
                    />
                  }
                  label="Obligado a Llevar Contabilidad"
                />
              </FormGroup>
              <Typography variant="caption" color="text.secondary">
                Nota: Estos campos tienen valor por defecto "No" (false)
              </Typography>
            </Grid>
            <Grid size={{ xs: 12 }}>
              <Alert severity="info">
                <Typography variant="body2">
                  <strong>Información fija que se asignará automáticamente:</strong>
                </Typography>
                <Typography variant="body2" sx={{ mt: 1 }}>
                  • País: Ecuador
                </Typography>
                <Typography variant="body2">
                  • División Administrativa: Guayas
                </Typography>
                <Typography variant="body2">
                  • Estado de la cuenta: Activo
                </Typography>
                <Typography variant="body2">
                  • Fecha de creación: Fecha y hora actual
                </Typography>
                <Typography variant="body2">
                  • Contraseña: Se generará automáticamente de forma segura
                </Typography>
              </Alert>
            </Grid>
          </Grid>
        );

      default:
        return null;
    }
  };

  if (successResponse) {
    return (
      <Dialog open={true} onClose={handleCloseModal} maxWidth="md" fullWidth>
        <DialogTitle>
          <Typography variant="h6" fontWeight={600} color="success.main">
            ✓ Usuario Creado Exitosamente
          </Typography>
        </DialogTitle>
        <DialogContent>
          <Paper elevation={0} sx={{ p: 3, bgcolor: 'success.light', mb: 3 }}>
            <Typography variant="body1" fontWeight={600}>
              {successResponse.mensaje}
            </Typography>
          </Paper>

          <Grid container spacing={2}>
            <Grid size={{ xs: 12 }}>
              <Alert severity="warning" sx={{ mb: 2 }}>
                <Typography variant="body2" fontWeight={600}>
                  ¡IMPORTANTE! Guarde esta contraseña. No se volverá a mostrar.
                </Typography>
              </Alert>
            </Grid>
            <Grid size={{ xs: 12 }}>
              <Box sx={{
                p: 2,
                bgcolor: 'grey.100',
                borderRadius: 1,
                border: '1px solid',
                borderColor: 'grey.300'
              }}>
                <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
                  <Typography variant="body1" fontWeight={600}>
                    Contraseña Generada:
                  </Typography>
                  <Button
                    size="small"
                    startIcon={<ContentCopy />}
                    onClick={handleCopyPassword}
                    variant="outlined"
                  >
                    {copied ? 'Copiada!' : 'Copiar'}
                  </Button>
                </Box>
                <Typography variant="h6" sx={{
                  mt: 1,
                  p: 2,
                  bgcolor: 'white',
                  borderRadius: 1,
                  fontFamily: 'monospace',
                  letterSpacing: 1
                }}>
                  {successResponse.contrasenaGenerada}
                </Typography>
              </Box>
            </Grid>
            <Grid size={{ xs: 12 }}>
              <Divider sx={{ my: 2 }} />
              <Typography variant="subtitle2" gutterBottom>
                Información del Usuario Creado:
              </Typography>
              <Box sx={{ pl: 2 }}>
                <Typography variant="body2">
                  <strong>Nombre:</strong> {successResponse.nombres} {successResponse.apellidos}
                </Typography>
                <Typography variant="body2">
                  <strong>Correo:</strong> {successResponse.correo}
                </Typography>
                <Typography variant="body2">
                  <strong>Celular:</strong> {successResponse.celular}
                </Typography>
                <Typography variant="body2">
                  <strong>Rol:</strong> {successResponse.rol}
                </Typography>
                <Typography variant="body2">
                  <strong>Fecha de creación:</strong> {new Date(successResponse.fechaCreacion).toLocaleString()}
                </Typography>
              </Box>
            </Grid>
          </Grid>
        </DialogContent>
        <DialogActions sx={{ p: 3 }}>
          <Button onClick={handleCloseModal} variant="contained" color="primary">
            Cerrar
          </Button>
        </DialogActions>
      </Dialog>
    );
  }

  return (
    <Dialog open={true} onClose={onClose} maxWidth="md" fullWidth>
      <DialogTitle>
        <Typography variant="h6" fontWeight={600}>
          Crear Nuevo Usuario (Administrador)
        </Typography>
        <Typography variant="caption" color="text.secondary">
          Complete la información del empleado
        </Typography>
      </DialogTitle>

      <Box sx={{ px: 3, pt: 2 }}>
        <Stepper activeStep={activeStep} alternativeLabel>
          {steps.map((label) => (
            <Step key={label}>
              <StepLabel>{label}</StepLabel>
            </Step>
          ))}
        </Stepper>
      </Box>

      <Box component="form" onSubmit={handleSubmit}>
        <DialogContent>
          {submitError && (
            <Alert severity="error" sx={{ mb: 3, borderRadius: 2 }}>
              {submitError}
            </Alert>
          )}

          {getStepContent(activeStep)}
        </DialogContent>

        <DialogActions sx={{ px: 3, pb: 3 }}>
          <Button
            onClick={activeStep === 0 ? onClose : handleBack}
            disabled={loading}
          >
            {activeStep === 0 ? 'Cancelar' : 'Atrás'}
          </Button>
          <Button
            type="submit"
            variant="contained"
            disabled={loading}
          >
            {loading ? (
              <CircularProgress size={24} />
            ) : activeStep === steps.length - 1 ? (
              'Crear Usuario'
            ) : (
              'Siguiente'
            )}
          </Button>
        </DialogActions>
      </Box>

      <Snackbar
        open={copied}
        autoHideDuration={2000}
        message="Contraseña copiada al portapapeles"
        anchorOrigin={{ vertical: 'bottom', horizontal: 'center' }}
      />
    </Dialog>
  );
};

export default CreateUserModal;
