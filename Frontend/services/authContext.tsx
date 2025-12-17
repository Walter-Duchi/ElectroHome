// src/services/authContext.tsx
import React, { createContext, useState, useContext, useEffect, ReactNode } from 'react';
import { User, AuthState } from '../types/auth';
import { authService } from './authService';

interface AuthContextType {
    auth: AuthState;
    login: (correo: string, contrasena: string) => Promise<void>;
    logout: () => void;
    loading: boolean;
}

const AuthContext = createContext<AuthContextType | undefined>(undefined);

export const useAuth = () => {
    const context = useContext(AuthContext);
    if (!context) {
        throw new Error('useAuth debe ser usado dentro de AuthProvider');
    }
    return context;
};

interface AuthProviderProps {
    children: ReactNode;
}

export const AuthProvider: React.FC<AuthProviderProps> = ({ children }) => {
    const [auth, setAuth] = useState<AuthState>({
        user: null,
        token: null,
        isAuthenticated: false,
    });
    const [loading, setLoading] = useState(true);

    useEffect(() => {
        const token = localStorage.getItem('token');
        const userStr = localStorage.getItem('user');

        if (token && userStr) {
            setAuth({
                user: JSON.parse(userStr),
                token,
                isAuthenticated: true,
            });
        }
        setLoading(false);
    }, []);

    const login = async (correo: string, contrasena: string) => {
        try {
            const response = await authService.login({ correo, contrasena });

            setAuth({
                user: {
                    id: response.id,
                    correo: response.correo,
                    rol: response.rol,
                },
                token: response.token,
                isAuthenticated: true,
            });

        } catch (error: any) {
            throw error;
        }
    };

    const logout = () => {
        authService.logout();
        setAuth({
            user: null,
            token: null,
            isAuthenticated: false,
        });
    };

    return (
        <AuthContext.Provider value={{ auth, login, logout, loading }}>
            {children}
        </AuthContext.Provider>
    );
};