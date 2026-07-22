import type { Identity } from "./jwt";

// Our reducer will act as a small state machine for auth, driven
// by useReducer calls in components. Every transition is a named action,
// similar to switch cases. We call those actions and state is 
// transitioned for us - less user error
export interface AuthState {
    status: "anonymous" | "authenticating" | "authenticated" | "error"
    user: Identity | null;
    error: string | null;
}

// union type representing actions that a user can take that affect AuthState
export type AuthAction = 
    | { type: "login_start" }
    | { type: "login_success"; user: Identity }
    | { type: "login_failure"; error: string }
    | { type: "logout" };

// It helps to encode a default or initial state value
export const initialAuthState: AuthState = {
    status: "anonymous",
    user: null,
    error: null
};

// Finally we write our reducer function
export function authReducer(state: AuthState, action: AuthAction): AuthState {

    switch(action.type) {

        case "login_start":
            return {...state, status:"authenticating", error:null};

        case "login_success":
            return {status: "authenticated", user: action.user, error:null}

        case "login_failure":
            return {status: "error", user:null, error: action.error}

        case "logout":
            return {...initialAuthState};
    }
}