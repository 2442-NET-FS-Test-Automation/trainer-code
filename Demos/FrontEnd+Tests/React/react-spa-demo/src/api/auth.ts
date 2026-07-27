import { api } from "./client";

// POST /auth/login { username, password }. The API answers with { token };
// bad credentials come back 401 and Axios throws - we can print a message to the user
// in the UI when that happens. 
export async function login( username: string, password: string): Promise<string> {
    const response = await api.post<{token: string}>("/auth/login", { username, password });
    return response.data.token;
}