import { useState } from "react";
import type { SubmitEvent } from "react";
import { useNavigate } from "react-router-dom";
import { useAuth } from "../auth/useAuth";

// A controlled form: username and password that users type in are React state,
// updated per keytroke. Submiting calls the login function from Context, that fires the POST
// stores the token and decodes identity

// admin user seeded
// ada 
// pass123!

export function LoginPage() {

    const { login, status } = useAuth(); //grabbing stuff from context with our hook

    // Creating some local state for this component for the user entered fields
    const [username, setUsername] = useState("");
    const [password, setPassword] = useState("");
    const [error, setError] = useState<string | null>(null);

    // Creating an object to let us navigate to another page automatically
    const navigate = useNavigate(); // tells the router to send us to another view

    // Creating a function to call when the submit/login button on the form is pressed
    async function onSubmit(e: SubmitEvent<HTMLFormElement>) {
        
        // HTML forms have default behavior - they try to send a POST 
        // to a URL with a string encoded in the body. Useless to us, we want 
        // to  stop it from even trying this.
        e.preventDefault(); 

        setError(null);

        const ok = await login(username, password);

        if(ok) navigate("/"); // if we logged in fine, send user to the catalog page

        else setError("Invalid username or password.");
    }

    return (
        <form className="login" onSubmit={onSubmit}>
            <h2>Sign in</h2>
            <label>
                Username
                <input 
                    type="text" 
                    value={username}
                    onChange={(e) => setUsername(e.target.value)}
                />
            </label>
            <label>
                Password
                <input 
                    type="password" 
                    value={password}
                    onChange={(e) => setPassword(e.target.value)}
                />
            </label>
            <button type="submit" disabled={status === "authenticating"}>
                {status === "authenticating" ? "Signing in..." : "Sign in"}
            </button>
            {error && <p className="error">{error}</p>}
        </form>
    )
}