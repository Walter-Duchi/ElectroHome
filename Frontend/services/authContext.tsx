import React, { createContext, useState, useContext, useEffect, type ReactNode } from 'react';
import type { AuthState } from '../src/types/auth';
import { authService } from './authService';

interface AuthContextType {
  auth: AuthState;
  login: (correo: string, contrasena: string) => Promise<void>;
  logout: () => void;
  loading: boolean;
}

const AuthContext = createContext<AuthContextType | undefined>(undefined);

// eslint-disable-next-line react-refresh/only-export-components
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

  // SOLUCIÓN: Usar un efecto con cleanup y prevenir renders en cascada
  useEffect(() => {
    // Variable para trackear si el componente está montado
    let isMounted = true;

    const initializeAuth = async () => {
      // Pequeño delay para asegurar que React esté listo
      await new Promise(resolve => setTimeout(resolve, 0));

      if (!isMounted) return;

      const token = localStorage.getItem('token');
      const userStr = localStorage.getItem('user');

      if (token && userStr) {
        try {
          // SOLUCIÓN: Usar una función de actualización
          setAuth(prevAuth => {
            // Solo actualizar si realmente hay cambios
            if (prevAuth.isAuthenticated && prevAuth.token === token) {
              return prevAuth;
            }

            return {
              user: JSON.parse(userStr),
              token,
              isAuthenticated: true,
            };
          });
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

    // Cleanup function
    return () => {
      isMounted = false;
    };
  }, []); // Empty dependency array - solo se ejecuta al montar

  const login = async (correo: string, contrasena: string): Promise<void> => {
    try {
      const response = await authService.login({ correo, contrasena });

      // Actualizar estado de manera segura
      setAuth({
        user: {
          id: response.id,
          correo: response.correo,
          rol: response.rol,
        },
        token: response.token,
        isAuthenticated: true,
      });
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
  };

  return (
    <AuthContext.Provider value={{ auth, login, logout, loading }}>
      {children}
    </AuthContext.Provider>
  );
};
