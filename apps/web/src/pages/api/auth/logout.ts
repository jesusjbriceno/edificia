import type { APIRoute } from 'astro';

/**
 * Endpoint de logout vestigial.
 * En Opción B la sesión vive en el cliente (Zustand/localStorage),
 * así que este endpoint simplemente devuelve 200.
 * El store del cliente se encarga de limpiar el estado.
 */
export const POST: APIRoute = async () => {
  return new Response(JSON.stringify({ success: true }), { status: 200 });
};