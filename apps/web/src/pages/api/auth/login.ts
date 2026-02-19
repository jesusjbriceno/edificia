import type { APIRoute } from 'astro';
import { setAuthCookie } from '@/lib/cookie-utils';

export const POST: APIRoute = async ({ request, cookies }) => {
  try {
    const body = await request.json();
    
    // Llamada real al Backend .NET
    // Ajusta la URL según tu variable de entorno real
    const apiUrl = import.meta.env.PUBLIC_API_URL || 'http://localhost:5000';
    
    const response = await fetch(`${apiUrl}/api/auth/login`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(body),
    });

    if (!response.ok) {
      return new Response(JSON.stringify({ error: 'Credenciales inválidas' }), { status: 401 });
    }

    const data = await response.json();
    // Asumiendo que data tiene { token: "...", ... }
    
    // AQUÍ OCURRE LA MAGIA: Astro establece la cookie
    setAuthCookie(cookies, data.token);

    return new Response(JSON.stringify({ success: true, user: data.user }), { status: 200 });
  } catch (error) {
    return new Response(JSON.stringify({ error: 'Error del servidor' }), { status: 500 });
  }
};