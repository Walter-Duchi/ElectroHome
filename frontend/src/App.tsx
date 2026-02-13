import React from 'react';
import { BrowserRouter as Router, Routes, Route, Navigate } from 'react-router-dom';
import { Box, CircularProgress } from '@mui/material';
import { AuthProvider, useAuth } from '../services/authContext';
import LoginForm from '../components/Login/LoginForm';
import ForgotPasswordForm from '../components/Login/ForgotPasswordForm';
import ResetPasswordForm from '../components/Login/ResetPasswordForm';
import DashboardLayout from '../components/Layout/DashboardLayout';
import CrearReclamo from '../components/Reclamo/CrearReclamo';
import TecnicoDashboard from '../components/Tecnico/TecnicoDashboard';
import RevisarProducto from '../components/Tecnico/RevisarProducto';
import EntregaDashboard from '../components/Entrega/EntregaDashboard';
import ClienteDashboard from '../components/Cliente/ClienteDashboard';
import DatosEmpresaConfig from '../components/Admin/DatosEmpresaConfig';

interface ProtectedRouteProps {
  children: React.ReactNode;
  allowedRoles?: string[];
}

const ProtectedRoute: React.FC<ProtectedRouteProps> = ({ children, allowedRoles }) => {
  const { auth } = useAuth();

  if (!auth.isAuthenticated) {
    return <Navigate to="/login" replace />;
  }

  if (allowedRoles && auth.user?.rol && !allowedRoles.includes(auth.user.rol)) {
    return <Navigate to="/" replace />;
  }

  return <>{children}</>;
};

// Componente que decide qué mostrar en la ruta raíz según el rol
const RoleBasedHome: React.FC = () => {
  const { auth } = useAuth();

  switch (auth.user?.rol) {
    case 'Cliente':
      return <ClienteDashboard />;
    case 'Revisor':
      return <CrearReclamo />;
    case 'Tecnico':
      return <TecnicoDashboard />;
    case 'Personal de Entrega':
      return <EntregaDashboard />;
    case 'Administrador': // NUEVO: Para administrador se muestra la configuración de la empresa
      return <DatosEmpresaConfig />;
    default:
      // Si no hay rol definido, mostrar una pantalla de error o redirigir
      return (
        <Box sx={{ p: 3, textAlign: 'center' }}>
          <h2>Rol no reconocido</h2>
          <p>Contacte al administrador del sistema.</p>
        </Box>
      );
  }
};

const AppContent: React.FC = () => {
  const { auth, loading } = useAuth();

  if (loading) {
    return (
      <Box
        sx={{
          display: 'flex',
          flexDirection: 'column',
          alignItems: 'center',
          justifyContent: 'center',
          minHeight: '100vh',
          background: 'linear-gradient(135deg, #0056b3 0%, #003b82 100%)',
          color: 'white',
        }}
      >
        <CircularProgress sx={{ color: 'white', mb: 2 }} size={60} />
        <Box sx={{ typography: 'h6' }}>Cargando sistema...</Box>
        <Box sx={{ typography: 'body2', mt: 1, opacity: 0.8 }}>
          Sistema de Gestión Corporativa
        </Box>
      </Box>
    );
  }

  return (
    <Router>
      <Routes>
        {/* Rutas públicas */}
        <Route
          path="/login"
          element={
            auth.isAuthenticated ? <Navigate to="/" replace /> : <LoginForm />
          }
        />
        <Route
          path="/forgot-password"
          element={
            auth.isAuthenticated ? (
              <Navigate to="/" replace />
            ) : (
              <ForgotPasswordForm />
            )
          }
        />
        <Route
          path="/reset-password"
          element={
            auth.isAuthenticated ? (
              <Navigate to="/" replace />
            ) : (
              <ResetPasswordForm />
            )
          }
        />

        {/* Ruta raíz: Muestra diferente contenido según el rol */}
        <Route
          path="/"
          element={
            <ProtectedRoute>
              <DashboardLayout>
                <RoleBasedHome />
              </DashboardLayout>
            </ProtectedRoute>
          }
        />

        {/* Otras rutas específicas que pueden seguir existiendo */}
        {/* Ruta para revisar producto específico - Solo para Técnicos */}
        <Route
          path="/tecnico/revisar/:id"
          element={
            <ProtectedRoute allowedRoles={['Tecnico']}>
              <DashboardLayout>
                <RevisarProducto />
              </DashboardLayout>
            </ProtectedRoute>
          }
        />

        {/* Puedes agregar más rutas específicas aquí si las necesitas */}

        {/* Ruta por defecto para cualquier otra ruta */}
        <Route path="*" element={<Navigate to="/" replace />} />
      </Routes>
    </Router>
  );
};

const App: React.FC = () => {
  return (
    <AuthProvider>
      <AppContent />
    </AuthProvider>
  );
};

export default App;
