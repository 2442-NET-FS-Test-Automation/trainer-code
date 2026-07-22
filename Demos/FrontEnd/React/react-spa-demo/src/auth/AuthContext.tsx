import { createContext, useEffect, useReducer } from "react";
import type { ReactNode } from "react";
import { login as loginRequest } from "../api/auth";
import { decodeToken } from "./jwt";
import { getToken, setToken, clearToken } from "./storage";
import { authReducer, initialAuthState } from "./authReducer";
import type { AuthState } from "./authReducer";

// Context distributes our AuthState to any components rendered within it 
// (children of those components included). Any component can call useAuth (our hook)
// to read the user or trigger login/logout
interface AuthContextValue extends AuthState {
    login: (username: string, password: string) => Promise<boolean>;
    logout: () => void;
}

export const AuthContext = createContext<AuthContextValue | null>(null);

// Finally our provider - the "component" that wraps other components 
// and lets them see the state inside of AuthContext
export function AuthProvider({ children }: {children: ReactNode}) {

    // Lets call our reducer via the useReducer hook
    const [state, dispatch] = useReducer(authReducer, initialAuthState);

    // Since we're using localStorage - we can actually persist the logged in user
    // even through browser refresh (state is wiped when someone refreshes the page)
    // lets use useEffect to grab a logged in user if there is one on page load
    useEffect(() => {

        const token = getToken();

        if(!token) return; // if there is no token

        const user = decodeToken(token);

        // Calling the login_success case of our reducer function via dispatch
        if (user) dispatch( {type: "login_success", user});
        else clearToken();

    }, []);

    // Our login method 
    async function login(username: string, password: string): Promise<boolean> {

        dispatch({type: "login_start"});

        try{
            const token = await loginRequest(username, password);
            const user = decodeToken(token);

            if (!user) throw new Error("token missing expected claims");
            
            // If our token is present with the correct claims - we can now store it
            setToken(token);
            dispatch({type: "login_success", user})
            return true; 

        }catch{
            dispatch({type:"login_failure", error: "Invalid username or password"})
            return false;
        }
    }

    function logout() {
        clearToken();
        dispatch({ type: "logout"});
    }

    return (
        <AuthContext.Provider value={{...state, login, logout}}>
            {children} {/* Children represents any components rendering inside
             of the AuthContext.Provider */}
        </AuthContext.Provider>
    )
}