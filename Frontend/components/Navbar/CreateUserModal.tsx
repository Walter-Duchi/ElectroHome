import React, { useState, useEffect } from 'react';
import { userService } from '../../services/userService';
import './CreateUserModal.css';

interface CreateUserModalProps {
    allowedRoles: string[];
    onClose: () => void;
    currentUserRole: string;
}

const CreateUserModal: React.FC<CreateUserModalProps> = ({
    allowedRoles,
    onClose,
    currentUserRole
}) => {
    const [formData, setFormData] = useState({
        nombres: '',
        apellidos: '',
        correo: '',
        contrasena: '',
        confirmarContrasena: '',
        celular: '',
        convencional: '',
        ruc: '',
        rol: allowedRoles[0] || '',
        numCuentaBancaria: ''
    });

    const [errors, setErrors] = useState<Record<string, string>>({});
    const [loading, setLoading] = useState(false);
    const [cardType, setCardType] = useState<string>('');

    useEffect(() => {
        if (formData.rol !== 'Cliente') {
            setFormData(prev => ({ ...prev, numCuentaBancaria: '' }));
        }
    }, [formData.rol]);

    const handleInputChange = (e: React.ChangeEvent<HTMLInputElement | HTMLSelectElement>) => {
        const { name, value } = e.target;
        setFormData(prev => ({ ...prev, [name]: value }));

        if (name === 'numCuentaBancaria') {
            if (value.trim() === '') {
                setCardType('');
            } else {
                const type = userService.getCardType(value);
                setCardType(type);
            }
        }

        if (errors[name]) {
            setErrors(prev => ({ ...prev, [name]: '' }));
        }
    };

    const validateForm = (): boolean => {
        const newErrors: Record<string, string> = {};

        if (!formData.nombres.trim()) newErrors.nombres = 'Nombres son requeridos';
        if (!formData.apellidos.trim()) newErrors.apellidos = 'Apellidos son requeridos';
        if (!formData.correo.trim()) newErrors.correo = 'Correo es requerido';
        if (!/\S+@\S+\.\S+/.test(formData.correo)) newErrors.correo = 'Correo no válido';
        if (formData.contrasena.length < 6) newErrors.contrasena = 'La contraseña debe tener al menos 6 caracteres';
        if (formData.contrasena !== formData.confirmarContrasena) newErrors.confirmarContrasena = 'Las contraseñas no coinciden';
        if (!formData.celular.trim()) newErrors.celular = 'Celular es requerido';
        if (!formData.ruc.trim()) newErrors.ruc = 'RUC es requerido';
        if (formData.ruc.length !== 13) newErrors.ruc = 'RUC debe tener 13 dígitos';
        if (!formData.rol) newErrors.rol = 'Rol es requerido';

        if (formData.rol === 'Cliente') {
            if (!formData.numCuentaBancaria?.trim()) {
                newErrors.numCuentaBancaria = 'Número de cuenta bancaria es obligatorio para clientes';
            } else if (!userService.validateCardNumber(formData.numCuentaBancaria)) {
                newErrors.numCuentaBancaria = 'Número de tarjeta inválido';
            }
        }

        setErrors(newErrors);
        return Object.keys(newErrors).length === 0;
    };

    const handleSubmit = async (e: React.FormEvent) => {
        e.preventDefault();

        if (!validateForm()) {
            return;
        }

        setLoading(true);

        try {
            const userData = {
                nombres: formData.nombres,
                apellidos: formData.apellidos,
                correo: formData.correo,
                contrasena: formData.contrasena,
                celular: formData.celular,
                convencional: formData.convencional || undefined,
                ruc: formData.ruc,
                rol: formData.rol,
                numCuentaBancaria: formData.rol === 'Cliente' ? formData.numCuentaBancaria : undefined
            };

            const response = await userService.createUser(userData);

            alert(`Usuario ${response.nombres} creado exitosamente!`);
            onClose();

        } catch (error: any) {
            alert(`Error: ${error.message}`);
        } finally {
            setLoading(false);
        }
    };

    return (
        <div className="modal-overlay">
            <div className="modal-content">
                <div className="modal-header">
                    <h2>Crear Nuevo Usuario</h2>
                    <button className="modal-close" onClick={onClose}>×</button>
                </div>

                <form onSubmit={handleSubmit}>
                    <div className="form-row">
                        <div className="form-group">
                            <label>Nombres *</label>
                            <input
                                type="text"
                                name="nombres"
                                value={formData.nombres}
                                onChange={handleInputChange}
                                disabled={loading}
                            />
                            {errors.nombres && <span className="error">{errors.nombres}</span>}
                        </div>

                        <div className="form-group">
                            <label>Apellidos *</label>
                            <input
                                type="text"
                                name="apellidos"
                                value={formData.apellidos}
                                onChange={handleInputChange}
                                disabled={loading}
                            />
                            {errors.apellidos && <span className="error">{errors.apellidos}</span>}
                        </div>
                    </div>

                    <div className="form-group">
                        <label>Correo Electrónico *</label>
                        <input
                            type="email"
                            name="correo"
                            value={formData.correo}
                            onChange={handleInputChange}
                            disabled={loading}
                        />
                        {errors.correo && <span className="error">{errors.correo}</span>}
                    </div>

                    <div className="form-row">
                        <div className="form-group">
                            <label>Contraseña *</label>
                            <input
                                type="password"
                                name="contrasena"
                                value={formData.contrasena}
                                onChange={handleInputChange}
                                disabled={loading}
                            />
                            {errors.contrasena && <span className="error">{errors.contrasena}</span>}
                        </div>

                        <div className="form-group">
                            <label>Confirmar Contraseña *</label>
                            <input
                                type="password"
                                name="confirmarContrasena"
                                value={formData.confirmarContrasena}
                                onChange={handleInputChange}
                                disabled={loading}
                            />
                            {errors.confirmarContrasena && <span className="error">{errors.confirmarContrasena}</span>}
                        </div>
                    </div>

                    <div className="form-row">
                        <div className="form-group">
                            <label>Celular *</label>
                            <input
                                type="tel"
                                name="celular"
                                value={formData.celular}
                                onChange={handleInputChange}
                                disabled={loading}
                            />
                            {errors.celular && <span className="error">{errors.celular}</span>}
                        </div>

                        <div className="form-group">
                            <label>Convencional (Opcional)</label>
                            <input
                                type="tel"
                                name="convencional"
                                value={formData.convencional}
                                onChange={handleInputChange}
                                disabled={loading}
                            />
                        </div>
                    </div>

                    <div className="form-group">
                        <label>RUC *</label>
                        <input
                            type="text"
                            name="ruc"
                            value={formData.ruc}
                            onChange={handleInputChange}
                            maxLength={13}
                            disabled={loading}
                        />
                        {errors.ruc && <span className="error">{errors.ruc}</span>}
                    </div>

                    <div className="form-group">
                        <label>Rol *</label>
                        <select
                            name="rol"
                            value={formData.rol}
                            onChange={handleInputChange}
                            disabled={loading}
                        >
                            {allowedRoles.map(role => (
                                <option key={role} value={role}>
                                    {role}
                                </option>
                            ))}
                        </select>
                        {errors.rol && <span className="error">{errors.rol}</span>}
                    </div>

                    {formData.rol === 'Cliente' && (
                        <div className="form-group">
                            <label>Número de Cuenta Bancaria *</label>
                            <input
                                type="text"
                                name="numCuentaBancaria"
                                value={formData.numCuentaBancaria}
                                onChange={handleInputChange}
                                placeholder="1234 5678 9012 3456"
                                disabled={loading}
                            />
                            {cardType && (
                                <div className="card-type-indicator">
                                    Tipo: <span className={`card-type ${cardType.toLowerCase().replace(' ', '-')}`}>
                                        {cardType}
                                    </span>
                                </div>
                            )}
                            {errors.numCuentaBancaria && <span className="error">{errors.numCuentaBancaria}</span>}
                            <div className="card-hint">
                                Aceptamos: Visa, Mastercard, American Express, etc.
                            </div>
                        </div>
                    )}

                    <div className="modal-actions">
                        <button type="button" className="btn-secondary" onClick={onClose} disabled={loading}>
                            Cancelar
                        </button>
                        <button type="submit" className="btn-primary" disabled={loading}>
                            {loading ? 'Creando...' : 'Crear Usuario'}
                        </button>
                    </div>
                </form>
            </div>
        </div>
    );
};

export default CreateUserModal;
