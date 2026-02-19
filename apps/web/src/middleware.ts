import { defineMiddleware } from 'astro:middleware';
import { AUTH_COOKIE_NAME, parseJwt } from './lib/cookie-utils';

// La raíz '/' es ahora pública (Login page)
const PUBLIC_ROUTES = ['/', '/forgot-password', '/404'];
const PUBLIC_PREFIXES = ['/api/auth', '/_image', '/favicon.svg', '/images/'];

export const onRequest = defineMiddleware(async (context, next) => {
  const { cookies, url, locals, redirect } = context;

  if (
    PUBLIC_ROUTES.includes(url.pathname) ||
    PUBLIC_PREFIXES.some(prefix => url.pathname.startsWith(prefix))
  ) {
    // Si estamos en el Login ('/') y YA estamos autenticados, mandar al Dashboard
    const token = cookies.get(AUTH_COOKIE_NAME)?.value;
    if (url.pathname === '/' && token) {
       // Opcional: Podrías validar el token aquí también para estar seguro
       return redirect('/dashboard');
    }
    return next();
  }

  const token = cookies.get(AUTH_COOKIE_NAME)?.value;
  locals.user = null;
  locals.isAuthenticated = false;

  if (token) {
    const decoded = parseJwt(token);
    const now = Math.floor(Date.now() / 1000);
    
    if (decoded && decoded.exp && decoded.exp > now) {
      locals.user = {
        id: decoded.sub || decoded.uid || decoded.id,
        email: decoded.email,
        role: decoded.role,
        name: decoded.name,
      };
      locals.isAuthenticated = true;
    }
  }

  // Si intenta acceder a ruta protegida sin auth, mandar a '/'
  if (!locals.isAuthenticated) {
    return redirect('/');
  }

  return next();
});