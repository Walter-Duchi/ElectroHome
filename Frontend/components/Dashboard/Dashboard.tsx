import React from 'react';
import { useAuth } from '../../services/authContext';
import './Dashboard.css';

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
        return 'No puedes crear cuentas de ningún tipo';
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
          'Verificar productos defectuosos'
        ];
      case 'Tecnico':
        return [
          'Crear cuentas de Técnicos',
          'Revisar productos técnicamente',
          'Generar informes técnicos',
          'Aprobar/Rechazar reclamos'
        ];
      case 'Personal de Entrega':
        return [
          'Crear cuentas de Personal de Entrega',
          'Gestionar entregas',
          'Registrar entregas de reemplazo',
          'Actualizar estados de productos'
        ];
      case 'Cliente':
        return [
          'Crear reclamos de productos',
          'Ver estado de reclamos',
          'Recibir reembolsos/reemplazos',
          'Ver historial de compras'
        ];
      default:
        return [];
    }
  };

  return (
    <div className="dashboard">
      <main className="dashboard-content">
        <div className="welcome-card">
          <h2>¡Bienvenido, {auth.user?.correo}!</h2>
          <div className="role-badge-large">{auth.user?.rol}</div>

          <div className="user-info-grid">
            <div className="info-item">
              <span className="info-label">ID de usuario:</span>
              <span className="info-value">{auth.user?.id}</span>
            </div>
            <div className="info-item">
              <span className="info-label">Permisos:</span>
              <span className="info-value">{getRoleDescription()}</span>
            </div>
          </div>

          <div className="permissions-section">
            <h3>Funcionalidades disponibles:</h3>
            <ul className="permissions-list">
              {getRolePermissions().map((permission, index) => (
                <li key={index} className="permission-item">
                  <span className="permission-icon">✓</span>
                  {permission}
                </li>
              ))}
            </ul>
          </div>

          <div className="info-box">
            <h3>Sistema de Reclamos</h3>
            <p>
              Este sistema permite gestionar reclamos de productos electrónicos,
              desde la creación del reclamo hasta su resolución mediante
              reembolso o reemplazo del producto.
            </p>
          </div>
        </div>
      </main>
    </div>
  );
};

export default Dashboard;
