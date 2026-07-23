import { Component, type ComponentType } from "react";
import { RequireAuth } from "./RequireAuth";


// A higher order component - takes a component and returns a new one 
// in our case - it's wrapped in an admin guard. Same functionality as our 
// existing guard
export function withAdminOnly<P extends object>(component: ComponentType<P>) {
    return function AdminGuarded(props: P) {
        return(
            <RequireAuth role="admin">
                <Component {...props} />
            </RequireAuth>
        )
    }
}