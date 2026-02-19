import { defineMiddleware } from 'astro:middleware';

/**
 * Middleware simplificado para Opción B (tokens en cliente).
 * 
 * La protección real de rutas la hace AuthGuard en el cliente.
 * Este middleware solo garantiza que las rutas públicas sean accesibles
 * y añade isAuthenticated = false a locals para SSR.
 */

const PUBLIC_ROUTES = ['/', '/forgot-password', '/404'];
const PUBLIC_PREFIXES = ['/api/', '/_image', '/favicon.ico', '/images/', '/logo-completo.webp'];

export const onRequest = defineMiddleware(async (context, next) => {
  const { url, locals } = context;

  // Siempre inicializar locals para que Astro no falle
  locals.user = null;
  locals.isAuthenticated = false;

  // Las rutas públicas y prefijos públicos siempre pasan
  if (
    PUBLIC_ROUTES.includes(url.pathname) ||
    PUBLIC_PREFIXES.some(prefix => url.pathname.startsWith(prefix))
  ) {
    return next();
  }

  // Para rutas protegidas: dejamos pasar igualmente
  // El AuthGuard del cliente se encargará de redirigir si no hay sesión
  return next();
});