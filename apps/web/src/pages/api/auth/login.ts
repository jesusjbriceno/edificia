import type { APIRoute } from 'astro';

/**
 * Proxy de login VESTIGIAL.
 * 
 * En Opción B (tokens en cliente) el LoginForm llama directamente
 * al backend vía authService/Axios. Este endpoint se mantiene por
 * retrocompatibilidad pero no se usa en el flujo principal.
 * 
 * En una futura iteración (Opción A — cookies HTTP-only) este
 * archivo volverá a ser el proxy principal.
 */
export const POST: APIRoute = async ({ request }) => {
  try {
    const body = await request.json();

    const apiUrl = import.meta.env.INTERNAL_API_URL || import.meta.env.PUBLIC_API_URL || 'http://localhost:5000';

    const response = await fetch(`${apiUrl}/api/auth/login`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(body),
    });

    const rawData = await response.text();
    
    return new Response(rawData, {
      status: response.status,
      headers: { 'Content-Type': 'application/json' },
    });
  } catch (error: any) {
    console.error('[Auth Proxy] Error:', error.message);
    return new Response(JSON.stringify({ error: 'Error del proxy de autenticación' }), { status: 500 });
  }
};