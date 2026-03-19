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
import EcommerceHome from '../components/Ecommerce/EcommerceHome';
import Cart from '../components/Ecommerce/Cart';
import ProductDetail from '../components/Ecommerce/ProductDetail';
import Checkout from '../components/Payphone/Checkout';
import PayphoneResponse from '../components/Payphone/PayphoneResponse';

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

// Componente que decide qué dashboard mostrar dentro de /app según el rol
const DashboardRouter: React.FC = () => {
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
    case 'Administrador':
      return <DatosEmpresaConfig />;
    default:
      return (
        <Box sx={{ p: 3, textAlign: 'center' }}>
          <h2>Rol no reconocido</h2>
          <p>Contacte al administrador del sistema.</p>
        </Box>
      );
  }
};

const AppContent: React.FC = () => {
  const { loading } = useAuth();

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
          ElectroHome - E-commerce
        </Box>
      </Box>
    );
  }

  return (
    <Router>
      <Routes>
        {/* Rutas públicas */}
        <Route path="/" element={<EcommerceHome />} />
        <Route path="/login" element={<LoginForm />} />
        <Route path="/forgot-password" element={<ForgotPasswordForm />} />
        <Route path="/reset-password" element={<ResetPasswordForm />} />
        <Route path="/producto/:id" element={<ProductDetail />} />
        <Route path="/cart" element={
          <ProtectedRoute>
            <Cart />
          </ProtectedRoute>
        } />

        {/* Rutas protegidas del dashboard (bajo /app) */}
        <Route path="/app" element={
          <ProtectedRoute>
            <DashboardLayout>
              <DashboardRouter />
            </DashboardLayout>
          </ProtectedRoute>
        } />
        <Route path="/app/tecnico/revisar/:id" element={
          <ProtectedRoute allowedRoles={['Tecnico']}>
            <DashboardLayout>
              <RevisarProducto />
            </DashboardLayout>
          </ProtectedRoute>
        } />

        {/* Ruta por defecto */}
        <Route path="*" element={<Navigate to="/" replace />} />

        <Route path="/checkout" element={
          <ProtectedRoute>
            <Checkout />
          </ProtectedRoute>
        } />
        <Route path="/payphone-response" element={
          <ProtectedRoute>
            <PayphoneResponse />
          </ProtectedRoute>
        } />
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
