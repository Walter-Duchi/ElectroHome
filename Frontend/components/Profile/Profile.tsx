import React, { useState, useEffect } from 'react';
import {
    Box,
    Paper,
    Typography,
    TextField,
    Button,
    Grid,
    Alert,
    CircularProgress,
    Divider,
    InputAdornment,
    IconButton,
    Snackbar,
    Card,
    CardContent,
} from '@mui/material';
import {
    Person,
    Email,
    Phone,
    LocationCity,
    LocationOn,
    Home,
    Badge,
    AccountBalance,
    Business,
    Edit,
    Save,
    Cancel,
    Lock,
    Visibility,
    VisibilityOff,
} from '@mui/icons-material';
import { userService } from '../../services/userService';
import { ProfileResponse } from '../../src/types/user';

const Profile: React.FC = () => {
    const [profile, setProfile] = useState<ProfileResponse | null>(null);
    const [loading, setLoading] = useState(true);
    const [editMode, setEditMode] = useState(false);
    const [formData, setFormData] = useState({
        correo: '',
        celular: '',
        convencional: '',
        ciudad: '',
        codigoPostal: '',
        direccion: '',
        currentPassword: '',
        newPassword: '',
        confirmNewPassword: '',
    });
    const [errors, setErrors] = useState<Record<string, string>>({});
    const [successMessage, setSuccessMessage] = useState('');
    const [errorMessage, setErrorMessage] = useState('');
    const [showCurrentPassword, setShowCurrentPassword] = useState(false);
    const [showNewPassword, setShowNewPassword] = useState(false);
    const [showConfirmPassword, setShowConfirmPassword] = useState(false);
    const [updating, setUpdating] = useState(false);

    useEffect(() => {
        loadProfile();
    }, []);

    const loadProfile = async () => {
        try {
            setLoading(true);
            const data = await userService.getProfile();
            setProfile(data);
            setFormData({
                correo: data.correo,
                celular: data.celular,
                convencional: data.convencional || '',
                ciudad: data.ciudad,
                codigoPostal: data.codigoPostal,
                direccion: data.direccion,
                currentPassword: '',
                newPassword: '',
                confirmNewPassword: '',
            });
        } catch (error: any) {
            setErrorMessage(error.message || 'Error al cargar perfil');
        } finally {
            setLoading(false);
        }
    };

    const handleEditMode = () => {
        setEditMode(true);
        setErrors({});
        setErrorMessage('');
    };

    const handleCancel = () => {
        if (profile) {
            setFormData({
                correo: profile.correo,
                celular: profile.celular,
                convencional: profile.convencional || '',
                ciudad: profile.ciudad,
                codigoPostal: profile.codigoPostal,
                direccion: profile.direccion,
                currentPassword: '',
                newPassword: '',
                confirmNewPassword: '',
            });
        }
        setEditMode(false);
        setErrors({});
        setErrorMessage('');
    };

    const handleChange = (e: React.ChangeEvent<HTMLInputElement>) => {
        const { name, value } = e.target;
        setFormData(prev => ({ ...prev, [name]: value }));
        if (errors[name]) {
            setErrors(prev => ({ ...prev, [name]: '' }));
        }
    };

    const validateForm = (): boolean => {
        const newErrors: Record<string, string> = {};

        if (!formData.correo.trim()) {
            newErrors.correo = 'El correo es requerido';
        } else if (!/\S+@\S+\.\S+/.test(formData.correo)) {
            newErrors.correo = 'Formato de correo inválido';
        }

        if (!formData.celular.trim()) {
            newErrors.celular = 'El celular es requerido';
        } else if (!/^09\d{8}$/.test(formData.celular.replace(/\D/g, ''))) {
            newErrors.celular = 'Celular inválido (debe ser 09XXXXXXXX)';
        }

        if (!formData.codigoPostal.trim()) {
            newErrors.codigoPostal = 'El código postal es requerido';
        } else if (!/^\d{6}$/.test(formData.codigoPostal)) {
            newErrors.codigoPostal = 'Código postal inválido (6 dígitos)';
        }

        if (!formData.ciudad.trim()) {
            newErrors.ciudad = 'La ciudad es requerida';
        }

        if (!formData.direccion.trim()) {
            newErrors.direccion = 'La dirección es requerida';
        }

        if (formData.newPassword || formData.currentPassword) {
            if (!formData.currentPassword) {
                newErrors.currentPassword = 'Debe ingresar la contraseña actual';
            }
            if (!formData.newPassword) {
                newErrors.newPassword = 'Debe ingresar la nueva contraseña';
            } else if (formData.newPassword.length < 6) {
                newErrors.newPassword = 'La nueva contraseña debe tener al menos 6 caracteres';
            }
            if (formData.newPassword !== formData.confirmNewPassword) {
                newErrors.confirmNewPassword = 'Las contraseñas no coinciden';
            }
        }

        setErrors(newErrors);
        return Object.keys(newErrors).length === 0;
    };

    const handleSubmit = async () => {
        if (!validateForm()) return;

        setUpdating(true);
        try {
            const updateData: any = {};
            if (formData.correo !== profile?.correo) updateData.correo = formData.correo;
            if (formData.celular !== profile?.celular) updateData.celular = formData.celular;
            if (formData.convencional !== (profile?.convencional || '')) updateData.convencional = formData.convencional;
            if (formData.ciudad !== profile?.ciudad) updateData.ciudad = formData.ciudad;
            if (formData.codigoPostal !== profile?.codigoPostal) updateData.codigoPostal = formData.codigoPostal;
            if (formData.direccion !== profile?.direccion) updateData.direccion = formData.direccion;

            if (formData.newPassword) {
                updateData.currentPassword = formData.currentPassword;
                updateData.newPassword = formData.newPassword;
                updateData.confirmNewPassword = formData.confirmNewPassword;
            }

            await userService.updateProfile(updateData);
            setSuccessMessage('Perfil actualizado exitosamente');
            setEditMode(false);
            await loadProfile();
        } catch (error: any) {
            setErrorMessage(error.message || 'Error al actualizar perfil');
        } finally {
            setUpdating(false);
        }
    };

    if (loading) {
        return (
            <Box sx={{ display: 'flex', justifyContent: 'center', alignItems: 'center', minHeight: '60vh' }}>
                <CircularProgress />
            </Box>
        );
    }

    if (!profile) {
        return (
            <Alert severity="error" sx={{ mt: 2 }}>
                No se pudo cargar la información del perfil.
            </Alert>
        );
    }

    return (
        <Box sx={{ maxWidth: 1000, mx: 'auto', mt: 2, mb: 4 }}>
            <Paper elevation={0} sx={{ p: 3, borderRadius: 2 }}>
                <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 3 }}>
                    <Typography variant="h5" fontWeight={600}>
                        Mi Perfil
                    </Typography>
                    {!editMode && (
                        <Button
                            variant="contained"
                            startIcon={<Edit />}
                            onClick={handleEditMode}
                            size="medium"
                        >
                            Editar Perfil
                        </Button>
                    )}
                </Box>

                <Divider sx={{ mb: 3 }} />

                {successMessage && (
                    <Alert severity="success" sx={{ mb: 3 }} onClose={() => setSuccessMessage('')}>
                        {successMessage}
                    </Alert>
                )}

                {errorMessage && (
                    <Alert severity="error" sx={{ mb: 3 }} onClose={() => setErrorMessage('')}>
                        {errorMessage}
                    </Alert>
                )}

                <Grid container spacing={3}>
                    {/* Campos no editables */}
                    <Grid size={{ xs: 12, md: 6 }}>
                        <Card variant="outlined" sx={{ height: '100%' }}>
                            <CardContent>
                                <Typography variant="subtitle2" color="text.secondary" gutterBottom>
                                    Información Personal (No editable)
                                </Typography>
                                <Box sx={{ display: 'flex', alignItems: 'center', mb: 2 }}>
                                    <Person color="action" sx={{ mr: 1 }} />
                                    <Typography variant="body2">
                                        <strong>Nombres:</strong> {profile.nombres} {profile.apellidos}
                                    </Typography>
                                </Box>
                                <Box sx={{ display: 'flex', alignItems: 'center', mb: 2 }}>
                                    <Badge color="action" sx={{ mr: 1 }} />
                                    <Typography variant="body2">
                                        <strong>{profile.tipoIdentificacion}:</strong> {profile.identificacion}
                                    </Typography>
                                </Box>
                                {profile.ruc && (
                                    <Box sx={{ display: 'flex', alignItems: 'center', mb: 2 }}>
                                        <Business color="action" sx={{ mr: 1 }} />
                                        <Typography variant="body2">
                                            <strong>RUC:</strong> {profile.ruc}
                                        </Typography>
                                    </Box>
                                )}
                                <Box sx={{ display: 'flex', alignItems: 'center', mb: 2 }}>
                                    <AccountBalance color="action" sx={{ mr: 1 }} />
                                    <Typography variant="body2">
                                        <strong>Cuenta Bancaria:</strong> {profile.numCuentaBancaria} ({profile.tipoCuentaBancaria})
                                    </Typography>
                                </Box>
                                <Box sx={{ display: 'flex', alignItems: 'center', mb: 1 }}>
                                    <Typography variant="body2">
                                        <strong>Rol:</strong> {profile.rol}
                                    </Typography>
                                </Box>
                            </CardContent>
                        </Card>
                    </Grid>

                    {/* Campos editables */}
                    <Grid size={{ xs: 12, md: 6 }}>
                        <Card variant="outlined" sx={{ height: '100%' }}>
                            <CardContent>
                                <Typography variant="subtitle2" color="text.secondary" gutterBottom>
                                    Datos de Contacto y Ubicación
                                </Typography>
                                <TextField
                                    label="Correo Electrónico"
                                    name="correo"
                                    value={formData.correo}
                                    onChange={handleChange}
                                    disabled={!editMode}
                                    fullWidth
                                    margin="normal"
                                    error={!!errors.correo}
                                    helperText={errors.correo}
                                    InputProps={{
                                        startAdornment: (
                                            <InputAdornment position="start">
                                                <Email color="action" />
                                            </InputAdornment>
                                        ),
                                    }}
                                />
                                <TextField
                                    label="Celular"
                                    name="celular"
                                    value={formData.celular}
                                    onChange={handleChange}
                                    disabled={!editMode}
                                    fullWidth
                                    margin="normal"
                                    error={!!errors.celular}
                                    helperText={errors.celular}
                                    InputProps={{
                                        startAdornment: (
                                            <InputAdornment position="start">
                                                <Phone color="action" />
                                            </InputAdornment>
                                        ),
                                    }}
                                />
                                <TextField
                                    label="Teléfono Convencional (Opcional)"
                                    name="convencional"
                                    value={formData.convencional}
                                    onChange={handleChange}
                                    disabled={!editMode}
                                    fullWidth
                                    margin="normal"
                                    InputProps={{
                                        startAdornment: (
                                            <InputAdornment position="start">
                                                <Phone color="action" />
                                            </InputAdornment>
                                        ),
                                    }}
                                />
                                <TextField
                                    label="Ciudad"
                                    name="ciudad"
                                    value={formData.ciudad}
                                    onChange={handleChange}
                                    disabled={!editMode}
                                    fullWidth
                                    margin="normal"
                                    error={!!errors.ciudad}
                                    helperText={errors.ciudad}
                                    InputProps={{
                                        startAdornment: (
                                            <InputAdornment position="start">
                                                <LocationCity color="action" />
                                            </InputAdornment>
                                        ),
                                    }}
                                />
                                <TextField
                                    label="Código Postal"
                                    name="codigoPostal"
                                    value={formData.codigoPostal}
                                    onChange={handleChange}
                                    disabled={!editMode}
                                    fullWidth
                                    margin="normal"
                                    error={!!errors.codigoPostal}
                                    helperText={errors.codigoPostal}
                                    InputProps={{
                                        startAdornment: (
                                            <InputAdornment position="start">
                                                <LocationOn color="action" />
                                            </InputAdornment>
                                        ),
                                    }}
                                />
                                <TextField
                                    label="Dirección"
                                    name="direccion"
                                    value={formData.direccion}
                                    onChange={handleChange}
                                    disabled={!editMode}
                                    fullWidth
                                    multiline
                                    rows={2}
                                    margin="normal"
                                    error={!!errors.direccion}
                                    helperText={errors.direccion}
                                    InputProps={{
                                        startAdornment: (
                                            <InputAdornment position="start">
                                                <Home color="action" />
                                            </InputAdornment>
                                        ),
                                    }}
                                />
                            </CardContent>
                        </Card>
                    </Grid>

                    {editMode && (
                        <Grid size={{ xs: 12 }}>
                            <Card variant="outlined">
                                <CardContent>
                                    <Typography variant="subtitle2" color="text.secondary" gutterBottom>
                                        Cambiar Contraseña
                                    </Typography>
                                    <Typography variant="caption" color="text.secondary" display="block" sx={{ mb: 2 }}>
                                        Deje los campos en blanco si no desea cambiar la contraseña.
                                    </Typography>
                                    <TextField
                                        label="Contraseña Actual"
                                        name="currentPassword"
                                        type={showCurrentPassword ? 'text' : 'password'}
                                        value={formData.currentPassword}
                                        onChange={handleChange}
                                        fullWidth
                                        margin="normal"
                                        error={!!errors.currentPassword}
                                        helperText={errors.currentPassword}
                                        InputProps={{
                                            endAdornment: (
                                                <InputAdornment position="end">
                                                    <IconButton onClick={() => setShowCurrentPassword(!showCurrentPassword)} edge="end">
                                                        {showCurrentPassword ? <VisibilityOff /> : <Visibility />}
                                                    </IconButton>
                                                </InputAdornment>
                                            ),
                                        }}
                                    />
                                    <TextField
                                        label="Nueva Contraseña"
                                        name="newPassword"
                                        type={showNewPassword ? 'text' : 'password'}
                                        value={formData.newPassword}
                                        onChange={handleChange}
                                        fullWidth
                                        margin="normal"
                                        error={!!errors.newPassword}
                                        helperText={errors.newPassword}
                                        InputProps={{
                                            startAdornment: (
                                                <InputAdornment position="start">
                                                    <Lock color="action" />
                                                </InputAdornment>
                                            ),
                                            endAdornment: (
                                                <InputAdornment position="end">
                                                    <IconButton onClick={() => setShowNewPassword(!showNewPassword)} edge="end">
                                                        {showNewPassword ? <VisibilityOff /> : <Visibility />}
                                                    </IconButton>
                                                </InputAdornment>
                                            ),
                                        }}
                                    />
                                    <TextField
                                        label="Confirmar Nueva Contraseña"
                                        name="confirmNewPassword"
                                        type={showConfirmPassword ? 'text' : 'password'}
                                        value={formData.confirmNewPassword}
                                        onChange={handleChange}
                                        fullWidth
                                        margin="normal"
                                        error={!!errors.confirmNewPassword}
                                        helperText={errors.confirmNewPassword}
                                        InputProps={{
                                            endAdornment: (
                                                <InputAdornment position="end">
                                                    <IconButton onClick={() => setShowConfirmPassword(!showConfirmPassword)} edge="end">
                                                        {showConfirmPassword ? <VisibilityOff /> : <Visibility />}
                                                    </IconButton>
                                                </InputAdornment>
                                            ),
                                        }}
                                    />
                                </CardContent>
                            </Card>
                        </Grid>
                    )}

                    {editMode && (
                        <Grid size={{ xs: 12 }} sx={{ display: 'flex', justifyContent: 'flex-end', gap: 2, mt: 2 }}>
                            <Button
                                variant="outlined"
                                startIcon={<Cancel />}
                                onClick={handleCancel}
                                disabled={updating}
                            >
                                Cancelar
                            </Button>
                            <Button
                                variant="contained"
                                startIcon={<Save />}
                                onClick={handleSubmit}
                                disabled={updating}
                            >
                                {updating ? <CircularProgress size={24} /> : 'Guardar Cambios'}
                            </Button>
                        </Grid>
                    )}
                </Grid>
            </Paper>
            <Snackbar
                open={!!successMessage}
                autoHideDuration={4000}
                onClose={() => setSuccessMessage('')}
                message={successMessage}
                anchorOrigin={{ vertical: 'bottom', horizontal: 'center' }}
            />
        </Box>
    );
};

export default Profile;