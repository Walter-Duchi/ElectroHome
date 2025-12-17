import { useState } from 'react';
import * as React from 'react';
import { useAuth } from '../../services/authContext';
import './LoginForm.css';

const LoginForm: React.FC = () => {
    const [correo, setCorreo] = useState('');
    const [contrasena, setContrasena] = useState('');
    const [error, setError] = useState('');
    const [loading, setLoading] = useState(false);
    const { login } = useAuth();

    const handleSubmit = async (e: React.FormEvent) => {
        e.preventDefault();
        setError('');
        setLoading(true);

        try {
            await login(correo, contrasena);
            // Redirigir después de login exitoso
            window.location.href = '/';
        } catch (err: unknown) {
            if (err instanceof Error) {
                setError(err.message);
            } else {
                setError('Error al iniciar sesión');
            }
        } finally {
            setLoading(false);
        }
    };

    return (
        <div className="login-container">
            <div className="login-card">
                <h2 className="login-title">Sistema de Reclamos</h2>
                <p className="login-subtitle">Iniciar Sesión</p>

                {error && (
                    <div className="error-message">
                        <span className="error-icon">⚠️</span>
                        {error}
                    </div>
                )}

                <form onSubmit={handleSubmit} className="login-form">
                    <div className="form-group">
                        <label htmlFor="correo" className="form-label">
                            Correo Electrónico
                        </label>
                        <input
                            type="email"
                            id="correo"
                            value={correo}
                            onChange={(e) => setCorreo(e.target.value)}
                            className="form-input"
                            placeholder="usuario@empresa.com"
                            required
                            disabled={loading}
                        />
                    </div>

                    <div className="form-group">
                        <label htmlFor="contrasena" className="form-label">
                            Contraseña
                        </label>
                        <input
                            type="password"
                            id="contrasena"
                            value={contrasena}
                            onChange={(e) => setContrasena(e.target.value)}
                            className="form-input"
                            placeholder="••••••••"
                            required
                            disabled={loading}
                        />
                    </div>

                    <button
                        type="submit"
                        className="login-button"
                        disabled={loading || !correo || !contrasena}
                    >
                        {loading ? (
                            <>
                                <span className="spinner"></span>
                                Procesando...
                            </>
                        ) : (
                            'Iniciar Sesión'
                        )}
                    </button>
                </form>

                <div className="login-info">
                    <p className="info-text">
                        <strong>Credenciales de prueba:</strong>
                    </p>
                    <div className="credentials-grid">
                        <div className="credential-item">
                            <span className="credential-role">Cliente:</span>
                            <span>juan.perez@gmail.com / Juan123*</span>
                        </div>
                        <div className="credential-item">
                            <span className="credential-role">Revisor:</span>
                            <span>maria.gomez@empresa.com / Maria456*</span>
                        </div>
                        <div className="credential-item">
                            <span className="credential-role">Técnico:</span>
                            <span>carlos.ramirez@soporte.com / Carlos789*</span>
                        </div>
                        <div className="credential-item">
                            <span className="credential-role">Personal Entrega:</span>
                            <span>luis.mendoza@logistica.com / Luis321*</span>
                        </div>
                    </div>
                </div>
            </div>
        </div>
    );
};

export default LoginForm;
