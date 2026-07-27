import { createContext, useReducer } from "react";
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

// Lets write a function to read the stored token BEFORE the first render. 
// that way the guard can read the authenticated user and not see InitialAuthState values
function initAuthState(): AuthState {
    const token = getToken();
    const user = token ? decodeToken(token) : null;

    if (!user) return initialAuthState; // if nobody is logged in - no token in LocalStorage - THEN return initial auth state

    return { status: "authenticated", user, error: null};

}

// Finally our provider - the "component" that wraps other components 
// and lets them see the state inside of AuthContext
export function AuthProvider({ children }: {children: ReactNode}) {

    // Lets call our reducer via the useReducer hook
    // Using a Lazy Initiator function with our reducer. Because that init function takes 0 args
    // we pass an undefined for the initial value - React discards it, runs the function, gets the initial state for the reducer
    const [state, dispatch] = useReducer(authReducer, undefined, initAuthState);

    // Since we're using localStorage - we can actually persist the logged in user
    // even through browser refresh (state is wiped when someone refreshes the page)
    // lets use useEffect to grab a logged in user if there is one on page load

    // DEPRECATED - Bad use of useEffect
    // useEffect(() => {

    //     const token = getToken();

    //     if(!token) return; // if there is no token

    //     const user = decodeToken(token);

    //     // Calling the login_success case of our reducer function via dispatch
    //     if (user) dispatch( {type: "login_success", user});
    //     else clearToken();

    // }, []);

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