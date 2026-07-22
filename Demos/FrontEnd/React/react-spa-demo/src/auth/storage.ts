// This will be the file responsible for holding my token grabbing/storing/deleting methods
// Later, my authcontext write/reads/clears it, via the methods in this file
const TOKEN_KEY = "library.token"; 

// alternative function declaration syntax - these are one liners
export const getToken = (): string | null => localStorage.getItem(TOKEN_KEY);
export const setToken = (token: string): void => localStorage.setItem(TOKEN_KEY, token);
export const clearToken = (): void => localStorage.removeItem(TOKEN_KEY);
