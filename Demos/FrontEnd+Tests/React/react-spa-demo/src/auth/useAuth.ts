import { useContext } from "react";
import { AuthContext } from "./AuthContext";

// This will be the hook that components call to gain access
// to auth context
export function useAuth() {
    // This AuthContext will only be visible if a component is wrapped by the context
    // if some outside component calls useAuth - nothing happens by default. No error.
    // We probably want to check and throw an error manually
    const ctx = useContext(AuthContext);

    if (!ctx) throw new Error("useAuth must be used inside an AuthProvider");

    return ctx;
}