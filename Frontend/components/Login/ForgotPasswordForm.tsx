import React, { useState } from 'react';
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
} from '@mui/material';
import { Email as EmailIcon, ArrowBack } from '@mui/icons-material';
import { Link as RouterLink, useNavigate } from 'react-router-dom';

const ForgotPasswordForm: React.FC = () => {
  const [correo, setCorreo] = useState('');
  const [loading, setLoading] = useState(false);
  const [message, setMessage] = useState('');
  const [error, setError] = useState('');
  const navigate = useNavigate();

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError('');
    setMessage('');

    // Validar correo
    if (!correo || !/\S+@\S+\.\S+/.test(correo)) {
      setError('Por favor, ingresa un correo electrónico válido.');
      return;
    }

    setLoading(true);

    try {
      const response = await fetch('http://localhost:5298/api/auth/forgot-password', {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
        },
        body: JSON.stringify({ correo }),
      });

      const data = await response.json();

      if (response.ok) {
        setMessage(data.message || 'Si el correo existe en nuestro sistema, recibirás instrucciones para restablecer tu contraseña en unos minutos.');
        setCorreo('');

        // Redirigir al login después de 5 segundos
        setTimeout(() => {
          navigate('/login');
        }, 5000);
      } else {
        setError(data.message || 'Error al procesar la solicitud. Por favor, intenta nuevamente.');
      }
    } catch (err: any) {
      console.error('Error:', err);
      setError('Error de conexión con el servidor. Verifica tu conexión a internet e intenta nuevamente.');
    } finally {
      setLoading(false);
    }
  };

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
              Restablecer Contraseña
            </Typography>
            <Typography variant="subtitle1" color="text.secondary">
              Ingresa tu correo electrónico registrado y te enviaremos un enlace para crear una nueva contraseña.
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
              <Typography variant="subtitle2" gutterBottom>
                ¡Solicitud procesada!
              </Typography>
              {message}
              <Typography variant="caption" display="block" sx={{ mt: 1 }}>
                Serás redirigido al inicio de sesión en unos segundos...
              </Typography>
            </Alert>
          )}

          <form onSubmit={handleSubmit}>
            <TextField
              label="Correo Electrónico"
              type="email"
              fullWidth
              margin="normal"
              value={correo}
              onChange={(e) => setCorreo(e.target.value)}
              placeholder="usuario@empresa.com"
              required
              disabled={loading || !!message}
              autoFocus
              InputProps={{
                startAdornment: (
                  <InputAdornment position="start">
                    <EmailIcon color="action" />
                  </InputAdornment>
                ),
              }}
              helperText="Ingresa el correo con el que te registraste en el sistema."
            />

            <Button
              type="submit"
              fullWidth
              variant="contained"
              size="large"
              disabled={loading || !correo || !!message}
              sx={{ mt: 3, mb: 3 }}
            >
              {loading ? (
                <CircularProgress size={24} sx={{ color: 'white' }} />
              ) : (
                'Enviar Enlace de Restablecimiento'
              )}
            </Button>
          </form>

          <Box sx={{ textAlign: 'center', mt: 3 }}>
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

          <Box sx={{ mt: 4, p: 3, bgcolor: 'action.hover', borderRadius: 2 }}>
            <Typography variant="caption" color="text.secondary">
              <strong>📧 ¿No recibiste el correo?</strong><br />
              1. Revisa tu carpeta de spam o correo no deseado<br />
              2. Asegúrate de haber ingresado el correo correctamente<br />
              3. Espera unos minutos y vuelve a intentar
            </Typography>
          </Box>
        </Paper>
      </Box>
    </Container>
  );
};

export default ForgotPasswordForm;
