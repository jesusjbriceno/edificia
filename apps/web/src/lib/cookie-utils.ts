/**
 * Utility functions for managing HTTP-only cookies securely.
 * Designed to work primarily within Astro Actions or API Routes.
 */

import type { AstroCookies } from 'astro';

export const AUTH_COOKIE_NAME = '__edificia_session';

interface CookieOptions {
  httpOnly: boolean;
  secure: boolean;
  sameSite: 'strict' | 'lax';
  path: string;
  maxAge: number;
}

const DEFAULT_COOKIE_OPTIONS: CookieOptions = {
  httpOnly: true, // Crucial: JS cannot access this cookie
  secure: import.meta.env.PROD, // Only secure in production (HTTPS)
  sameSite: 'lax', // 'lax' is needed for navigation from external sites, 'strict' for high security actions
  path: '/',
  maxAge: 60 * 60 * 24 * 7, // 7 days
};

/**
 * Sets the authentication cookie securely.
 */
export const setAuthCookie = (cookies: AstroCookies, token: string) => {
  cookies.set(AUTH_COOKIE_NAME, token, DEFAULT_COOKIE_OPTIONS);
};

/**
 * Deletes the authentication cookie.
 */
export const deleteAuthCookie = (cookies: AstroCookies) => {
  cookies.delete(AUTH_COOKIE_NAME, {
    path: '/',
  });
};

/**
 * Parse JWT payload without verification (Verification happens on backend API).
 * Used only for getting user info for UI display if backend verification passes.
 */
export const parseJwt = (token: string) => {
  try {
    return JSON.parse(Buffer.from(token.split('.')[1], 'base64').toString());
  } catch (e) {
    return null;
  }
};