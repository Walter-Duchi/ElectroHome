import React, { useState, useEffect } from 'react';
import { useSearchParams, useNavigate, Link } from 'react-router-dom';
import './LoginForm.css';

const ResetPasswordForm: React.FC = () => {
  const [searchParams] = useSearchParams();
  const navigate = useNavigate();
  const token = searchParams.get('token');

  const [nuevaContrasena, setNuevaContrasena] = useState('');
  const [confirmarContrasena, setConfirmarContrasena] = useState('');
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

  if (validating) {
    return (
      <div className="login-container">
        <div className="login-card">
          <h2 className="login-title">Validando Enlace...</h2>
          <div style={{ textAlign: 'center', padding: '40px 0' }}>
            <div className="spinner" style={{ margin: '0 auto' }}></div>
            <p style={{ marginTop: '20px', color: '#666' }}>
              Verificando tu enlace de restablecimiento...
            </p>
          </div>
        </div>
      </div>
    );
  }

  if (!tokenValid) {
    return (
      <div className="login-container">
        <div className="login-card">
          <h2 className="login-title">Enlace Inválido</h2>
          <div className="error-message">
            <span className="error-icon">⚠️</span>
            {error || 'Este enlace de restablecimiento ha expirado o es inválido.'}
          </div>
          <div className="login-info" style={{ textAlign: 'center', marginTop: '30px' }}>
            <p className="info-text">
              <Link to="/forgot-password" style={{ color: '#667eea', textDecoration: 'none' }}>
                Solicitar nuevo enlace
              </Link>
            </p>
            <p className="info-text" style={{ marginTop: '10px' }}>
              <Link to="/login" style={{ color: '#667eea', textDecoration: 'none' }}>
                Volver al Inicio de Sesión
              </Link>
            </p>
          </div>
        </div>
      </div>
    );
  }

  return (
    <div className="login-container">
      <div className="login-card">
        <h2 className="login-title">Crear Nueva Contraseña</h2>
        <p className="login-subtitle">
          Crea una contraseña segura para tu cuenta.
        </p>

        {error && (
          <div className="error-message">
            <span className="error-icon">⚠️</span>
            {error}
          </div>
        )}

        {message && (
          <div className="success-message" style={{
            backgroundColor: '#d4edda',
            borderColor: '#c3e6cb',
            color: '#155724',
            padding: '12px 16px',
            borderRadius: '8px',
            marginBottom: '24px'
          }}>
            <span style={{ marginRight: '8px' }}>✅</span>
            {message}
          </div>
        )}

        <form onSubmit={handleSubmit} className="login-form">
          <div className="form-group">
            <label htmlFor="nuevaContrasena" className="form-label">
              Nueva Contraseña
            </label>
            <input
              type="password"
              id="nuevaContrasena"
              value={nuevaContrasena}
              onChange={(e) => setNuevaContrasena(e.target.value)}
              className="form-input"
              placeholder="••••••••"
              required
              disabled={loading}
            />
            <div className="password-hint" style={{
              fontSize: '12px',
              color: '#666',
              marginTop: '5px',
              fontStyle: 'italic'
            }}>
              Mínimo 8 caracteres, con mayúsculas, minúsculas, números y caracteres especiales
            </div>
          </div>

          <div className="form-group">
            <label htmlFor="confirmarContrasena" className="form-label">
              Confirmar Contraseña
            </label>
            <input
              type="password"
              id="confirmarContrasena"
              value={confirmarContrasena}
              onChange={(e) => setConfirmarContrasena(e.target.value)}
              className="form-input"
              placeholder="••••••••"
              required
              disabled={loading}
            />
          </div>

          <button
            type="submit"
            className="login-button"
            disabled={loading || !nuevaContrasena || !confirmarContrasena}
          >
            {loading ? 'Restableciendo...' : 'Restablecer Contraseña'}
          </button>
        </form>

        <div className="login-info">
          <p className="info-text">
            <Link to="/login" style={{ color: '#667eea', textDecoration: 'none' }}>
              ← Volver al Inicio de Sesión
            </Link>
          </p>
        </div>
      </div>
    </div>
  );
};

export default ResetPasswordForm;
