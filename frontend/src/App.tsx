import React from 'react';
import { BrowserRouter as Router, Routes, Route, Navigate } from 'react-router-dom';
import { AuthProvider, useAuth } from '../services/authContext';
import LoginForm from '../components/Login/LoginForm';
import './App.css';

// Componente protegido
const ProtectedRoute: React.FC<{ children: React.ReactNode }> = ({ children }) => {
    const { auth } = useAuth();

    if (!auth.isAuthenticated) {
        return <Navigate to="/login" replace />;
    }

    return <>{children}</>;
};

// Dashboard principal
const Dashboard: React.FC = () => {
    const { auth, logout } = useAuth();

    return (
        <div className="dashboard">
            <header className="dashboard-header">
                <h1>Dashboard del Sistema</h1>
                <div className="user-info">
                    <span className="user-email">{auth.user?.correo}</span>
                    <span className="user-role">{auth.user?.rol}</span>
                    <button onClick={logout} className="logout-button">
                        Cerrar Sesión
                    </button>
                </div>
            </header>

            <main className="dashboard-content">
                <div className="welcome-card">
                    <h2>¡Bienvenido, {auth.user?.correo}!</h2>
                    <p>Rol: <strong>{auth.user?.rol}</strong></p>
                    <p>ID de usuario: <strong>{auth.user?.id}</strong></p>
                    <p>Token almacenado correctamente ✅</p>

                    <div className="info-box">
                        <h3>Próximos pasos:</h3>
                        <ul>
                            <li>Desarrollar interfaz específica para el rol "{auth.user?.rol}"</li>
                            <li>Implementar gestión de reclamos</li>
                            <li>Agregar funcionalidades según permisos</li>
                        </ul>
                    </div>
                </div>
            </main>
        </div>
    );
};

// Componente principal
const AppContent: React.FC = () => {
    const { auth, loading } = useAuth();

    if (loading) {
        return (
            <div className="loading-screen">
                <div className="spinner-large"></div>
                <p>Cargando...</p>
            </div>
        );
    }

    return (
        <Router>
            <Routes>
                <Route path="/login" element={
                    auth.isAuthenticated ? <Navigate to="/" replace /> : <LoginForm />
                } />
                <Route path="/" element={
                    <ProtectedRoute>
                        <Dashboard />
                    </ProtectedRoute>
                } />
                <Route path="*" element={<Navigate to="/" replace />} />
            </Routes>
        </Router>
    );
};

// App principal envuelta en AuthProvider
const App: React.FC = () => {
    return (
        <AuthProvider>
            <AppContent />
        </AuthProvider>
    );
};

export default App;