import axios from "axios";
import { getToken } from "../auth/storage";

// One axios object/instance  for the whole app. Since we're only hitting one api
// we can centralize its url here. If you are hitting multiple api's just make
// separate clients. Can all be in the same file or one per file - up to you

// Later when we do login and we have to attach that JWT bearer token - 
// it'll be centralized in one place! No having to chase down every fetch call.
export const api = axios.create({
    baseURL: "http://localhost:5137"
})

// Request interceptor - Attack that bearer token (if we have it) to EVERY call
// that our client makes. 
api.interceptors.request.use((config) => {
    const token = getToken();
    if (token) config.headers.Authorization = `Bearer ${token}`; // writing the header
    return config;
});