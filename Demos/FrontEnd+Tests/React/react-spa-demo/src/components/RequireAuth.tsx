import { Navigate, useLocation } from "react-router-dom";
import type { ReactNode } from "react";
import { useAuth } from "../auth/useAuth";

// A route guard changes routing behavior based on auth state. 
// Not signed in -> redirect to /login - then send the user back to the page they 
// wanted to go to. 
// Lack required role -> redirect to home. The API enforces all this on the backend,
// the UI keeps users from seeing broken pages.

interface RequireAuthProps{
    children: ReactNode;
    role?: string;
}

export function RequireAuth({children, role}: RequireAuthProps) {

    // We need to see who is logged in - so we need access to auth context
    const { status, user } = useAuth();
    // We want to be able to remember where a user was trying to go before the guard
    // redirected them.
    const location = useLocation();

    if (status !== "authenticated") // replace tells the browser not to save the previously attempted
                                    // url - the user cant hit the back button and sidestep the guard
        return <Navigate to="/login" state={{from: location}} replace/>

    // When we use the guard we can specify if a specific role is required such as admin
    // if it required (it was passed in) - make sure the logged in user's role matches
    // if it DOESN'T navigate them home
    if ( role && user?.role !== role ) return <Navigate to="/" replace/>

    // If we pass both checks - we can render whatever the child component is
    return <>{children}</>

}