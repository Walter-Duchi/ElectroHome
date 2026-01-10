import React from 'react';
import {
  Box,
  Grid,
  Card,
  CardContent,
  Typography,
  Chip,
  Stack,
  Button,
  LinearProgress,
  Alert,
  Divider,
  List,
  ListItem,
  ListItemIcon,
  ListItemText,
} from '@mui/material';
import {
  CheckCircle,
  People,
  Assignment,
  Timeline,
  TrendingUp,
  Security,
  NotificationsActive,
  Schedule,
} from '@mui/icons-material';
import { useAuth } from '../../services/authContext';

const Dashboard: React.FC = () => {
  const { auth } = useAuth();

  const getRoleDescription = () => {
    switch (auth.user?.rol) {
      case 'Revisor':
        return 'Puedes crear cuentas para Clientes y otros Revisores';
      case 'Tecnico':
        return 'Puedes crear cuentas para otros Técnicos';
      case 'Personal de Entrega':
        return 'Puedes crear cuentas para otro Personal de Entrega';
      case 'Cliente':
        return 'Acceso a gestión de reclamos y seguimiento';
      default:
        return '';
    }
  };

  const getRolePermissions = () => {
    switch (auth.user?.rol) {
      case 'Revisor':
        return [
          'Crear cuentas de Clientes',
          'Crear cuentas de Revisores',
          'Gestionar reclamos',
          'Verificar productos defectuosos',
          'Generar reportes',
          'Aprobar solicitudes',
        ];
      case 'Tecnico':
        return [
          'Crear cuentas de Técnicos',
          'Revisar productos técnicamente',
          'Generar informes técnicos',
          'Aprobar/Rechazar reclamos',
          'Diagnóstico de productos',
          'Calibración de equipos',
        ];
      case 'Personal de Entrega':
        return [
          'Crear cuentas de Personal de Entrega',
          'Gestionar entregas',
          'Registrar entregas de reemplazo',
          'Actualizar estados de productos',
          'Seguimiento de envíos',
          'Coordinación logística',
        ];
      case 'Cliente':
        return [
          'Crear reclamos de productos',
          'Ver estado de reclamos',
          'Recibir reembolsos/reemplazos',
          'Ver historial de compras',
          'Seguimiento en tiempo real',
          'Gestión de garantías',
        ];
      default:
        return [];
    }
  };

  const getStats = () => {
    const baseStats = {
      tasks: 12,
      completed: 8,
      pending: 4,
      efficiency: 85,
    };

    switch (auth.user?.rol) {
      case 'Revisor':
        return { ...baseStats, tasks: 24, completed: 18, efficiency: 92 };
      case 'Tecnico':
        return { ...baseStats, tasks: 16, completed: 14, efficiency: 88 };
      case 'Personal de Entrega':
        return { ...baseStats, tasks: 32, completed: 28, efficiency: 94 };
      case 'Cliente':
        return { ...baseStats, tasks: 3, completed: 2, efficiency: 78 };
      default:
        return baseStats;
    }
  };

  const stats = getStats();

  return (
    <Box>
      {/* Header del Dashboard */}
      <Box sx={{ mb: 4 }}>
        <Typography variant="h4" fontWeight={700} gutterBottom>
          Panel de Control Corporativo
        </Typography>
        <Typography variant="body1" color="text.secondary">
          Bienvenido al sistema de gestión empresarial
        </Typography>
      </Box>

      {/* Información del Usuario */}
      <Card sx={{ mb: 4 }}>
        <CardContent>
          <Grid container spacing={3} alignItems="center">
            <Grid size={{xs:12, md: 8}}>
              <Stack direction="row" spacing={2} alignItems="center" mb={2}>
                <Typography variant="h5" fontWeight={600}>
                  {auth.user?.correo}
                </Typography>
                <Chip
                  label={auth.user?.rol}
                  color="primary"
                  size="medium"
                  sx={{ fontWeight: 600 }}
                />
              </Stack>
              <Typography variant="body1" color="text.secondary" paragraph>
                {getRoleDescription()}
              </Typography>
              
              <Grid container spacing={2}>
                <Grid size={{xs:6, sm: 3}}>
                  <Box>
                    <Typography variant="caption" color="text.secondary">
                      ID de Usuario
                    </Typography>
                    <Typography variant="h6">{auth.user?.id}</Typography>
                  </Box>
                </Grid>
                <Grid size={{xs:6, sm: 3}}>
                  <Box>
                    <Typography variant="caption" color="text.secondary">
                      Eficiencia
                    </Typography>
                    <Typography variant="h6">{stats.efficiency}%</Typography>
                  </Box>
                </Grid>
                <Grid size={{xs:6, sm: 3}}>
                  <Box>
                    <Typography variant="caption" color="text.secondary">
                      Tareas Completadas
                    </Typography>
                    <Typography variant="h6">{stats.completed}/{stats.tasks}</Typography>
                  </Box>
                </Grid>
                <Grid size={{xs:6, sm: 3}}>
                  <Box>
                    <Typography variant="caption" color="text.secondary">
                      Estado
                    </Typography>
                    <Chip label="Activo" color="success" size="small" />
                  </Box>
                </Grid>
              </Grid>
            </Grid>
            
            <Grid size={{xs:12, md: 4}}>
              <Box sx={{ textAlign: 'center' }}>
                <Box sx={{ position: 'relative', display: 'inline-flex' }}>
                  <Box
                    sx={{
                      width: 120,
                      height: 120,
                      borderRadius: '50%',
                      bgcolor: 'primary.light',
                      display: 'flex',
                      alignItems: 'center',
                      justifyContent: 'center',
                    }}
                  >
                    <Typography variant="h4" fontWeight={700} color="white">
                      {stats.efficiency}%
                    </Typography>
                  </Box>
                </Box>
                <Typography variant="body2" sx={{ mt: 2 }}>
                  Eficiencia del Sistema
                </Typography>
              </Box>
            </Grid>
          </Grid>
        </CardContent>
      </Card>

      {/* Stats Cards */}
      <Grid container spacing={3} sx={{ mb: 4 }}>
        <Grid size={{xs:12, sm: 6, md:3}}>
          <Card>
            <CardContent>
              <Stack direction="row" alignItems="center" spacing={2}>
                <Box
                  sx={{
                    p: 1.5,
                    borderRadius: 2,
                    bgcolor: 'primary.light',
                    color: 'white',
                  }}
                >
                  <People />
                </Box>
                <Box>
                  <Typography variant="caption" color="text.secondary">
                    Usuarios Activos
                  </Typography>
                  <Typography variant="h5">1,248</Typography>
                </Box>
              </Stack>
              <LinearProgress
                variant="determinate"
                value={75}
                sx={{ mt: 2, height: 6, borderRadius: 3 }}
              />
            </CardContent>
          </Card>
        </Grid>
        
        <Grid size={{xs:12, sm: 6, md:3}}>
          <Card>
            <CardContent>
              <Stack direction="row" alignItems="center" spacing={2}>
                <Box
                  sx={{
                    p: 1.5,
                    borderRadius: 2,
                    bgcolor: 'success.light',
                    color: 'white',
                  }}
                >
                  <Assignment />
                </Box>
                <Box>
                  <Typography variant="caption" color="text.secondary">
                    Reclamos Activos
                  </Typography>
                  <Typography variant="h5">342</Typography>
                </Box>
              </Stack>
              <LinearProgress
                variant="determinate"
                value={60}
                sx={{ mt: 2, height: 6, borderRadius: 3 }}
                color="success"
              />
            </CardContent>
          </Card>
        </Grid>
        
        <Grid size={{xs:12, sm: 6, md:3}}>
          <Card>
            <CardContent>
              <Stack direction="row" alignItems="center" spacing={2}>
                <Box
                  sx={{
                    p: 1.5,
                    borderRadius: 2,
                    bgcolor: 'warning.light',
                    color: 'white',
                  }}
                >
                  <Timeline />
                </Box>
                <Box>
                  <Typography variant="caption" color="text.secondary">
                    En Progreso
                  </Typography>
                  <Typography variant="h5">89</Typography>
                </Box>
              </Stack>
              <LinearProgress
                variant="determinate"
                value={45}
                sx={{ mt: 2, height: 6, borderRadius: 3 }}
                color="warning"
              />
            </CardContent>
          </Card>
        </Grid>
        
        <Grid size={{xs:12, sm: 6, md:3}}>
          <Card>
            <CardContent>
              <Stack direction="row" alignItems="center" spacing={2}>
                <Box
                  sx={{
                    p: 1.5,
                    borderRadius: 2,
                    bgcolor: 'info.light',
                    color: 'white',
                  }}
                >
                  <TrendingUp />
                </Box>
                <Box>
                  <Typography variant="caption" color="text.secondary">
                    Resueltos Hoy
                  </Typography>
                  <Typography variant="h5">47</Typography>
                </Box>
              </Stack>
              <LinearProgress
                variant="determinate"
                value={85}
                sx={{ mt: 2, height: 6, borderRadius: 3 }}
                color="info"
              />
            </CardContent>
          </Card>
        </Grid>
      </Grid>

      {/* Permisos y Funcionalidades */}
      <Grid container spacing={3}>
        <Grid size={{xs:12, md: 8}}>
          <Card>
            <CardContent>
              <Typography variant="h6" fontWeight={600} gutterBottom>
                Funcionalidades Disponibles
              </Typography>
              <List>
                {getRolePermissions().map((permission, index) => (
                  <ListItem key={index} sx={{ px: 0 }}>
                    <ListItemIcon sx={{ minWidth: 40 }}>
                      <CheckCircle color="success" />
                    </ListItemIcon>
                    <ListItemText
                      primary={permission}
                      primaryTypographyProps={{ variant: 'body2' }}
                    />
                  </ListItem>
                ))}
              </List>
              
              <Divider sx={{ my: 3 }} />
              
              <Alert
                severity="info"
                icon={<Security />}
                sx={{ borderRadius: 2 }}
              >
                <Typography variant="subtitle2" gutterBottom>
                  Seguridad Corporativa Nivel 4
                </Typography>
                <Typography variant="body2">
                  Tu sesión está protegida con encriptación AES-256 y autenticación
                  de dos factores. Todas las actividades son auditadas.
                </Typography>
              </Alert>
            </CardContent>
          </Card>
        </Grid>
        
        <Grid size={{xs:12, md: 4}}>
          <Card>
            <CardContent>
              <Typography variant="h6" fontWeight={600} gutterBottom>
                Acciones Rápidas
              </Typography>
              <Stack spacing={2}>
                <Button
                  variant="contained"
                  fullWidth
                  startIcon={<NotificationsActive />}
                >
                  Ver Notificaciones
                </Button>
                <Button
                  variant="outlined"
                  fullWidth
                  startIcon={<Assignment />}
                >
                  Nuevo Reclamo
                </Button>
                <Button
                  variant="outlined"
                  fullWidth
                  startIcon={<Schedule />}
                >
                  Calendario
                </Button>
                <Button
                  variant="outlined"
                  fullWidth
                  startIcon={<People />}
                >
                  Contactar Soporte
                </Button>
              </Stack>
              
              <Divider sx={{ my: 3 }} />
              
              <Box sx={{ p: 2, bgcolor: 'action.hover', borderRadius: 2 }}>
                <Typography variant="caption" color="text.secondary">
                  <strong>Sistema de Reclamos Corporativo</strong>
                </Typography>
                <Typography variant="body2" sx={{ mt: 1 }}>
                  Este sistema permite gestionar reclamos de productos electrónicos,
                  desde la creación del reclamo hasta su resolución mediante
                  reembolso o reemplazo del producto.
                </Typography>
              </Box>
            </CardContent>
          </Card>
        </Grid>
      </Grid>

      {/* Información del Sistema */}
      <Box sx={{ mt: 4, textAlign: 'center' }}>
        <Typography variant="caption" color="text.secondary">
          Sistema Corporativo v2.1.0 • Última actualización: 2026
        </Typography>
      </Box>
    </Box>
  );
};

export default Dashboard;
