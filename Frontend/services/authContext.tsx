import React, { createContext, useState, useContext, useEffect, type ReactNode } from 'react';
import type { AuthState } from '../src/types/auth';
import { authService } from './authService';

interface AuthContextType {
  auth: AuthState;
  login: (correo: string, contrasena: string) => Promise<void>;
  logout: () => void;
  loading: boolean;
  userRole: string | null;
}

const AuthContext = createContext<AuthContextType | undefined>(undefined);

export const useAuth = (): AuthContextType => {
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
  const [userRole, setUserRole] = useState<string | null>(null);

  useEffect(() => {
    let isMounted = true;

    const initializeAuth = async () => {
      await new Promise(resolve => setTimeout(resolve, 0));

      if (!isMounted) return;

      const token = localStorage.getItem('token');
      const userStr = localStorage.getItem('user');

      if (token && userStr) {
        try {
          const user = JSON.parse(userStr);
          setAuth({
            user: {
              id: user.id,
              correo: user.correo,
              rol: user.rol,
              nombres: user.nombres,
              apellidos: user.apellidos
            },
            token,
            isAuthenticated: true,
          });
          setUserRole(user.rol);
        } catch (error) {
          console.error('Error parsing user data:', error);
          localStorage.removeItem('token');
          localStorage.removeItem('user');
        }
      }

      if (isMounted) {
        setLoading(false);
      }
    };

    initializeAuth();

    return () => {
      isMounted = false;
    };
  }, []);

  const login = async (correo: string, contrasena: string): Promise<void> => {
    try {
      const response = await authService.login({ correo, contrasena });

      setAuth({
        user: {
          id: response.id,
          correo: response.correo,
          rol: response.rol,
          nombres: response.nombres,
          apellidos: response.apellidos
        },
        token: response.token,
        isAuthenticated: true,
      });
      setUserRole(response.rol);
    } catch (error: unknown) {
      if (error instanceof Error) {
        throw error;
      }
      throw new Error('Error desconocido al iniciar sesión');
    }
  };

  const logout = (): void => {
    authService.logout();
    setAuth({
      user: null,
      token: null,
      isAuthenticated: false,
    });
    setUserRole(null);
    window.location.href = '/';
  };

  return (
    <AuthContext.Provider value={{ auth, login, logout, loading, userRole }}>
      {children}
    </AuthContext.Provider>
  );
};