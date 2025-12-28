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
} from '@mui/material';
import {
  Person,
  Email,
  Phone,
  VpnKey,
  Business,
  AccountBalance,
  Visibility,
  VisibilityOff,
} from '@mui/icons-material';
import { userService } from '../../services/userService';

interface CreateUserModalProps {
  allowedRoles: string[];
  onClose: () => void;
  currentUserRole: string;
}

const steps = ['Información Personal', 'Datos de Contacto', 'Información Bancaria'];

const CreateUserModal: React.FC<CreateUserModalProps> = ({ allowedRoles, onClose }) => {
  const [activeStep, setActiveStep] = useState(0);
  const [formData, setFormData] = useState({
    nombres: '',
    apellidos: '',
    correo: '',
    contrasena: '',
    confirmarContrasena: '',
    celular: '',
    convencional: '',
    ruc: '',
    rol: allowedRoles[0] || '',
    numCuentaBancaria: '',
    tipoCuentaBancaria: '',
  });

  const [errors, setErrors] = useState<Record<string, string>>({});
  const [loading, setLoading] = useState(false);
  const [submitError, setSubmitError] = useState('');
  const [showPassword, setShowPassword] = useState(false);

  useEffect(() => {
    if (formData.rol !== 'Cliente') {
      setFormData(prev => ({
        ...prev,
        numCuentaBancaria: '',
        tipoCuentaBancaria: '',
      }));
    }
  }, [formData.rol]);

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
        if (!formData.correo.trim()) newErrors.correo = 'Correo es requerido';
        if (!/\S+@\S+\.\S+/.test(formData.correo)) newErrors.correo = 'Correo no válido';
        if (formData.contrasena.length < 8)
          newErrors.contrasena = 'La contraseña debe tener al menos 8 caracteres';
        if (formData.contrasena !== formData.confirmarContrasena)
          newErrors.confirmarContrasena = 'Las contraseñas no coinciden';
        break;
      case 1: // Datos de Contacto
        if (!formData.celular.trim()) newErrors.celular = 'Celular es requerido';
        if (!formData.ruc.trim()) newErrors.ruc = 'RUC es requerido';
        if (formData.ruc.length !== 13) newErrors.ruc = 'RUC debe tener 13 dígitos';
        if (!formData.rol) newErrors.rol = 'Rol es requerido';
        break;
      case 2: // Información Bancaria
        if (formData.rol === 'Cliente') {
          if (!formData.numCuentaBancaria?.trim()) {
            newErrors.numCuentaBancaria = 'Número de cuenta bancaria es obligatorio para clientes';
          } else if (!userService.validateBankAccount(formData.numCuentaBancaria)) {
            newErrors.numCuentaBancaria =
              'Número de cuenta bancaria inválido (debe tener 10-20 dígitos)';
          }

          if (!formData.tipoCuentaBancaria) {
            newErrors.tipoCuentaBancaria = 'Tipo de cuenta bancaria es obligatorio para clientes';
          } else if (!userService.validateAccountType(formData.tipoCuentaBancaria)) {
            newErrors.tipoCuentaBancaria = 'Tipo de cuenta debe ser Ahorro o Corriente';
          }
        }
        break;
    }

    setErrors(newErrors);
    return Object.keys(newErrors).length === 0;
  };

  const handleInputChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    const { name, value } = e.target;
    setFormData((prev) => ({ ...prev, [name]: value }));

    if (errors[name]) {
      setErrors((prev) => ({ ...prev, [name]: '' }));
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
        const userData = {
          nombres: formData.nombres,
          apellidos: formData.apellidos,
          correo: formData.correo,
          contrasena: formData.contrasena,
          celular: formData.celular,
          convencional: formData.convencional || undefined,
          ruc: formData.ruc,
          rol: formData.rol,
          numCuentaBancaria:
            formData.rol === 'Cliente' ? formData.numCuentaBancaria : undefined,
          tipoCuentaBancaria:
            formData.rol === 'Cliente' ? formData.tipoCuentaBancaria : undefined,
        };

        const response = await userService.createUser(userData);

        alert(`Usuario ${response.nombres} creado exitosamente!`);
        onClose();
      } catch (error: any) {
        setSubmitError(error.message);
      } finally {
        setLoading(false);
      }
    } else {
      handleNext();
    }
  };

  const handleTogglePassword = () => {
    setShowPassword(!showPassword);
  };

  const getStepContent = (step: number) => {
    switch (step) {
      case 0:
        return (
          <Grid container spacing={2}>
            <Grid size={{xs:12, sm: 6}}>
              <TextField
                label="Nombres *"
                name="nombres"
                value={formData.nombres}
                onChange={handleInputChange}
                error={!!errors.nombres}
                helperText={errors.nombres}
                disabled={loading}
                InputProps={{
                  startAdornment: (
                    <InputAdornment position="start">
                      <Person color="action" />
                    </InputAdornment>
                  ),
                }}
              />
            </Grid>
            <Grid size={{xs:12, sm: 6}}>
              <TextField
                label="Apellidos *"
                name="apellidos"
                value={formData.apellidos}
                onChange={handleInputChange}
                error={!!errors.apellidos}
                helperText={errors.apellidos}
                disabled={loading}
                InputProps={{
                  startAdornment: (
                    <InputAdornment position="start">
                      <Person color="action" />
                    </InputAdornment>
                  ),
                }}
              />
            </Grid>
            <Grid size={{xs:12}}>
              <TextField
                label="Correo Electrónico *"
                name="correo"
                type="email"
                value={formData.correo}
                onChange={handleInputChange}
                error={!!errors.correo}
                helperText={errors.correo}
                disabled={loading}
                InputProps={{
                  startAdornment: (
                    <InputAdornment position="start">
                      <Email color="action" />
                    </InputAdornment>
                  ),
                }}
              />
            </Grid>
            <Grid size={{xs:12, sm: 6}}>
              <TextField
                label="Contraseña *"
                name="contrasena"
                type={showPassword ? 'text' : 'password'}
                value={formData.contrasena}
                onChange={handleInputChange}
                error={!!errors.contrasena}
                helperText={errors.contrasena}
                disabled={loading}
                InputProps={{
                  startAdornment: (
                    <InputAdornment position="start">
                      <VpnKey color="action" />
                    </InputAdornment>
                  ),
                  endAdornment: (
                    <InputAdornment position="end">
                      <IconButton onClick={handleTogglePassword} edge="end">
                        {showPassword ? <VisibilityOff /> : <Visibility />}
                      </IconButton>
                    </InputAdornment>
                  ),
                }}
              />
            </Grid>
            <Grid size={{xs:12, sm: 6}}>
              <TextField
                label="Confirmar Contraseña *"
                name="confirmarContrasena"
                type={showPassword ? 'text' : 'password'}
                value={formData.confirmarContrasena}
                onChange={handleInputChange}
                error={!!errors.confirmarContrasena}
                helperText={errors.confirmarContrasena}
                disabled={loading}
                InputProps={{
                  startAdornment: (
                    <InputAdornment position="start">
                      <VpnKey color="action" />
                    </InputAdornment>
                  ),
                }}
              />
            </Grid>
          </Grid>
        );
      case 1:
        return (
          <Grid container spacing={2}>
            <Grid size={{xs:12, sm: 6}}>
              <TextField
                label="Celular *"
                name="celular"
                value={formData.celular}
                onChange={handleInputChange}
                error={!!errors.celular}
                helperText={errors.celular}
                disabled={loading}
                InputProps={{
                  startAdornment: (
                    <InputAdornment position="start">
                      <Phone color="action" />
                    </InputAdornment>
                  ),
                }}
              />
            </Grid>
            <Grid size={{xs:12, sm: 6}}>
              <TextField
                label="Convencional (Opcional)"
                name="convencional"
                value={formData.convencional}
                onChange={handleInputChange}
                disabled={loading}
                InputProps={{
                  startAdornment: (
                    <InputAdornment position="start">
                      <Phone color="action" />
                    </InputAdornment>
                  ),
                }}
              />
            </Grid>
            <Grid size={{xs:12}}>
              <TextField
                label="RUC *"
                name="ruc"
                value={formData.ruc}
                onChange={handleInputChange}
                inputProps={{ maxLength: 13 }}
                error={!!errors.ruc}
                helperText={errors.ruc}
                disabled={loading}
                InputProps={{
                  startAdornment: (
                    <InputAdornment position="start">
                      <Business color="action" />
                    </InputAdornment>
                  ),
                }}
              />
            </Grid>
            <Grid size={{xs:12}}>
              <TextField
                select
                label="Rol *"
                name="rol"
                value={formData.rol}
                onChange={handleInputChange}
                error={!!errors.rol}
                helperText={errors.rol}
                disabled={loading}
              >
                {allowedRoles.map((role) => (
                  <MenuItem key={role} value={role}>
                    {role}
                  </MenuItem>
                ))}
              </TextField>
            </Grid>
          </Grid>
        );
      case 2:
        return formData.rol === 'Cliente' ? (
          <Grid container spacing={2}>
            <Grid size={{xs:12}}>
              <TextField
                label="Número de Cuenta Bancaria *"
                name="numCuentaBancaria"
                value={formData.numCuentaBancaria}
                onChange={handleInputChange}
                placeholder="Ej: 12345678901234567890"
                error={!!errors.numCuentaBancaria}
                helperText={errors.numCuentaBancaria}
                disabled={loading}
                InputProps={{
                  startAdornment: (
                    <InputAdornment position="start">
                      <AccountBalance color="action" />
                    </InputAdornment>
                  ),
                }}
              />
              <Typography variant="caption" color="text.secondary" sx={{ mt: 1, display: 'block' }}>
                Número de cuenta bancaria (10-20 dígitos)
              </Typography>
            </Grid>
            <Grid size={{xs:12}}>
              <TextField
                select
                label="Tipo de Cuenta Bancaria *"
                name="tipoCuentaBancaria"
                value={formData.tipoCuentaBancaria}
                onChange={handleInputChange}
                error={!!errors.tipoCuentaBancaria}
                helperText={errors.tipoCuentaBancaria}
                disabled={loading}
              >
                <MenuItem value="">Seleccione...</MenuItem>
                <MenuItem value="Ahorro">Ahorro</MenuItem>
                <MenuItem value="Corriente">Corriente</MenuItem>
              </TextField>
            </Grid>
          </Grid>
        ) : (
          <Box sx={{ textAlign: 'center', py: 4 }}>
            <AccountBalance sx={{ fontSize: 60, color: 'text.disabled', mb: 2 }} />
            <Typography variant="body1" color="text.secondary">
              Información bancaria no requerida para el rol de {formData.rol}
            </Typography>
          </Box>
        );
      default:
        return null;
    }
  };

  return (
    <Dialog open={true} onClose={onClose} maxWidth="md" fullWidth>
      <DialogTitle>
        <Typography variant="h6" fontWeight={600}>
          Crear Nuevo Usuario
        </Typography>
        <Typography variant="caption" color="text.secondary">
          Complete la información requerida en cada paso
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

      <form onSubmit={handleSubmit}>
        <DialogContent>
          {submitError && (
            <Alert severity="error" sx={{ mb: 2, borderRadius: 2 }}>
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
      </form>
    </Dialog>
  );
};

export default CreateUserModal;
