import React, { useState } from 'react';
import { Link, useNavigate } from 'react-router-dom';

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
    <div className="login-container">
      <div className="login-card">
        <h2 className="login-title">Restablecer Contraseña</h2>
        <p className="login-subtitle">
          Ingresa tu correo electrónico registrado y te enviaremos un enlace para crear una nueva contraseña.
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
            border: '1px solid #c3e6cb',
            color: '#155724',
            padding: '12px 16px',
            borderRadius: '8px',
            marginBottom: '24px',
            display: 'flex',
            alignItems: 'center',
            gap: '8px'
          }}>
            <span style={{ fontSize: '18px' }}>✅</span>
            <div>
              <strong>¡Solicitud procesada!</strong>
              <p style={{ margin: '5px 0 0 0', fontSize: '14px' }}>{message}</p>
              <p style={{ margin: '5px 0 0 0', fontSize: '12px', color: '#0c5460' }}>
                Serás redirigido al inicio de sesión en unos segundos...
              </p>
            </div>
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
              disabled={loading || !!message}
              autoFocus
            />
            <small style={{ color: '#666', fontSize: '12px', marginTop: '5px' }}>
              Ingresa el correo con el que te registraste en el sistema.
            </small>
          </div>

          <button
            type="submit"
            className="login-button"
            disabled={loading || !correo || !!message}
            style={{ marginTop: '10px' }}
          >
            {loading ? (
              <>
                <span className="spinner"></span>
                Procesando...
              </>
            ) : (
              'Enviar Enlace de Restablecimiento'
            )}
          </button>
        </form>

        <div className="login-info">
          <p className="info-text" style={{ textAlign: 'center' }}>
            <Link to="/login" style={{
              color: '#667eea',
              textDecoration: 'none',
              fontWeight: 600,
              display: 'inline-flex',
              alignItems: 'center',
              gap: '5px'
            }}>
              <span style={{ fontSize: '18px' }}>←</span>
              Volver al Inicio de Sesión
            </Link>
          </p>

          <div style={{ marginTop: '20px', padding: '15px', backgroundColor: '#f8f9fa', borderRadius: '8px' }}>
            <p style={{ fontSize: '12px', color: '#666', margin: 0 }}>
              <strong>📧 ¿No recibiste el correo?</strong><br />
              1. Revisa tu carpeta de spam o correo no deseado<br />
              2. Asegúrate de haber ingresado el correo correctamente<br />
              3. Espera unos minutos y vuelve a intentar
            </p>
          </div>
        </div>
      </div>
    </div>
  );
};

export default ForgotPasswordForm;
