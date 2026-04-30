import React, { useState, useEffect } from 'react';
import {
  Container,
  Paper,
  Box,
  Typography,
  TextField,
  Button,
  Alert,
  CircularProgress,
  Link,
  InputAdornment,
  IconButton,
} from '@mui/material';
import { VpnKey, Visibility, VisibilityOff, ArrowBack } from '@mui/icons-material';
import { Link as RouterLink, useSearchParams, useNavigate } from 'react-router-dom';

const ResetPasswordForm: React.FC = () => {
  const [searchParams] = useSearchParams();
  const navigate = useNavigate();
  const token = searchParams.get('token');

  const [nuevaContrasena, setNuevaContrasena] = useState('');
  const [confirmarContrasena, setConfirmarContrasena] = useState('');
  const [showPassword, setShowPassword] = useState(false);
  const [loading, setLoading] = useState(false);
  const [validating, setValidating] = useState(true);
  const [tokenValid, setTokenValid] = useState(false);
  const [message, setMessage] = useState('');
  const [error, setError] = useState('');

  useEffect(() => {
    const validateToken = async () => {
      if (!token) {
        navigate('/login');
        return;
      }

      try {
        const response = await fetch(
          `http://localhost:5298/api/auth/validate-reset-token?token=${token}`
        );

        if (response.ok) {
          const data = await response.json();
          setTokenValid(data.valid);
          if (!data.valid) {
            setError('El enlace de restablecimiento ha expirado o es inválido.');
          }
        } else {
          setTokenValid(false);
          setError('Error al validar el enlace.');
        }
      } catch (err) {
        setTokenValid(false);
        setError('Error de conexión con el servidor.');
      } finally {
        setValidating(false);
      }
    };

    validateToken();
  }, [token, navigate]);

  const validatePassword = (password: string): string[] => {
    const errors: string[] = [];

    if (password.length < 8) {
      errors.push('Al menos 8 caracteres');
    }
    if (!/[a-z]/.test(password)) {
      errors.push('Al menos una letra minúscula');
    }
    if (!/[A-Z]/.test(password)) {
      errors.push('Al menos una letra mayúscula');
    }
    if (!/\d/.test(password)) {
      errors.push('Al menos un número');
    }
    if (!/[@$!%*?&]/.test(password)) {
      errors.push('Al menos un carácter especial (@$!%*?&)');
    }

    return errors;
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError('');
    setMessage('');

    // Validar contraseña
    const passwordErrors = validatePassword(nuevaContrasena);
    if (passwordErrors.length > 0) {
      setError(`La contraseña debe tener: ${passwordErrors.join(', ')}`);
      return;
    }

    if (nuevaContrasena !== confirmarContrasena) {
      setError('Las contraseñas no coinciden.');
      return;
    }

    setLoading(true);

    try {
      const response = await fetch('http://localhost:5298/api/auth/reset-password', {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
        },
        body: JSON.stringify({
          token,
          nuevaContrasena,
          confirmarContrasena,
        }),
      });

      if (response.ok) {
        setMessage('¡Contraseña restablecida exitosamente! Serás redirigido al inicio de sesión.');
        setTimeout(() => {
          navigate('/login');
        }, 3000);
      } else {
        const errorData = await response.json();
        setError(errorData.message || 'Error al restablecer la contraseña.');
      }
    } catch (err) {
      setError('Error de conexión con el servidor.');
    } finally {
      setLoading(false);
    }
  };

  const handleTogglePassword = () => {
    setShowPassword(!showPassword);
  };

  if (validating) {
    return (
      <Container maxWidth="sm">
        <Box
          sx={{
            minHeight: '100vh',
            display: 'flex',
            alignItems: 'center',
            justifyContent: 'center',
            py: 4,
            background: 'linear-gradient(135deg, #0056b3 0%, #003b82 100%)',
          }}
        >
          <Paper
            elevation={24}
            sx={{
              p: { xs: 3, md: 4 },
              width: '100%',
              maxWidth: 450,
              borderRadius: 4,
              border: '1px solid',
              borderColor: 'divider',
              textAlign: 'center',
            }}
          >
            <CircularProgress sx={{ mb: 3 }} />
            <Typography variant="h6" gutterBottom>
              Validando Enlace...
            </Typography>
            <Typography variant="body2" color="text.secondary">
              Verificando tu enlace de restablecimiento...
            </Typography>
          </Paper>
        </Box>
      </Container>
    );
  }

  if (!tokenValid) {
    return (
      <Container maxWidth="sm">
        <Box
          sx={{
            minHeight: '100vh',
            display: 'flex',
            alignItems: 'center',
            justifyContent: 'center',
            py: 4,
            background: 'linear-gradient(135deg, #0056b3 0%, #003b82 100%)',
          }}
        >
          <Paper
            elevation={24}
            sx={{
              p: { xs: 3, md: 4 },
              width: '100%',
              maxWidth: 450,
              borderRadius: 4,
              border: '1px solid',
              borderColor: 'divider',
              textAlign: 'center',
            }}
          >
            <Alert severity="error" sx={{ mb: 3, borderRadius: 2 }}>
              {error || 'Este enlace de restablecimiento ha expirado o es inválido.'}
            </Alert>
            <Box sx={{ display: 'flex', flexDirection: 'column', gap: 2, alignItems: 'center' }}>
              <Link
                component={RouterLink}
                to="/forgot-password"
                variant="body2"
                sx={{
                  textDecoration: 'none',
                  fontWeight: 600,
                  color: 'primary.main'
                }}
              >
                Solicitar nuevo enlace
              </Link>
              <Link
                component={RouterLink}
                to="/login"
                variant="body2"
                sx={{
                  textDecoration: 'none',
                  fontWeight: 600,
                  color: 'primary.main'
                }}
              >
                Volver al Inicio de Sesión
              </Link>
            </Box>
          </Paper>
        </Box>
      </Container>
    );
  }

  return (
    <Container maxWidth="sm">
      <Box
        sx={{
          minHeight: '100vh',
          display: 'flex',
          alignItems: 'center',
          justifyContent: 'center',
          py: 4,
          background: 'linear-gradient(135deg, #0056b3 0%, #003b82 100%)',
        }}
      >
        <Paper
          elevation={24}
          sx={{
            p: { xs: 3, md: 4 },
            width: '100%',
            maxWidth: 450,
            borderRadius: 4,
            border: '1px solid',
            borderColor: 'divider',
          }}
        >
          <Box sx={{ textAlign: 'center', mb: 4 }}>
            <Typography variant="h4" component="h1" fontWeight={700} gutterBottom>
              Crear Nueva Contraseña
            </Typography>
            <Typography variant="subtitle1" color="text.secondary">
              Crea una contraseña segura para tu cuenta.
            </Typography>
          </Box>

          {error && (
            <Alert
              severity="error"
              sx={{ mb: 3, borderRadius: 2 }}
              onClose={() => setError('')}
            >
              {error}
            </Alert>
          )}

          {message && (
            <Alert
              severity="success"
              sx={{ mb: 3, borderRadius: 2 }}
            >
              {message}
            </Alert>
          )}

          <form onSubmit={handleSubmit}>
            <TextField
              label="Nueva Contraseña"
              type={showPassword ? 'text' : 'password'}
              fullWidth
              margin="normal"
              value={nuevaContrasena}
              onChange={(e) => setNuevaContrasena(e.target.value)}
              placeholder="••••••••"
              required
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
              helperText="Mínimo 8 caracteres, con mayúsculas, minúsculas, números y caracteres especiales"
            />

            <TextField
              label="Confirmar Contraseña"
              type={showPassword ? 'text' : 'password'}
              fullWidth
              margin="normal"
              value={confirmarContrasena}
              onChange={(e) => setConfirmarContrasena(e.target.value)}
              placeholder="••••••••"
              required
              disabled={loading}
              InputProps={{
                startAdornment: (
                  <InputAdornment position="start">
                    <VpnKey color="action" />
                  </InputAdornment>
                ),
              }}
            />

            <Button
              type="submit"
              fullWidth
              variant="contained"
              size="large"
              disabled={loading || !nuevaContrasena || !confirmarContrasena}
              sx={{ mt: 3, mb: 3 }}
            >
              {loading ? (
                <CircularProgress size={24} sx={{ color: 'white' }} />
              ) : (
                'Restablecer Contraseña'
              )}
            </Button>
          </form>

          <Box sx={{ textAlign: 'center' }}>
            <Link
              component={RouterLink}
              to="/login"
              variant="body2"
              sx={{
                textDecoration: 'none',
                display: 'inline-flex',
                alignItems: 'center',
                gap: 1,
                fontWeight: 600,
                color: 'primary.main'
              }}
            >
              <ArrowBack fontSize="small" />
              Volver al Inicio de Sesión
            </Link>
          </Box>
        </Paper>
      </Box>
    </Container>
  );
};

export default ResetPasswordForm;
