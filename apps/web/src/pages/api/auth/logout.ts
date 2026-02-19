import type { APIRoute } from 'astro';
import { deleteAuthCookie } from '@/lib/cookie-utils';

export const POST: APIRoute = async ({ cookies, redirect }) => {
  deleteAuthCookie(cookies);
  // Redirigir a la raíz
  return redirect('/');
};