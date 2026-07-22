// This file will handled decoding the claims/info encoded into the JWT
// In order to do that, we need to know the full XML schema URI's 
// that .NET writes claims with
const NAME_CLAIM = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name";
const ROLE_CLAIM = "http://schemas.microsoft.com/ws/2008/06/identity/claims/role";

// We need an object for our decoded JWT
interface JwtPayload {
    [claim: string]: string | number | undefined;
}

export interface Identity {
    name: string,
    role: string
} 

export function decodeToken(token: string): Identity | null {

    try{
        // JWTs are 3 segment encoded strings
        // 2342fsdfsdf.234234t3tv.asfaw4534
        const segment = token.split(".")[1];
        if(!segment) return null; // if we didn't have a token to begin with, return null

        // It starts to get a little weird
        const base64 = segment.replace(/-/g, "+").replace(/_/g, "/");
        const payload = JSON.parse(atob(base64)) as JwtPayload;

        // Now that we have the payload, we can grab the info from the claims
        const name = payload[NAME_CLAIM];
        const role = payload[ROLE_CLAIM];

        if(typeof name !== "string" || typeof role !== "string") return null;

        return {name, role};

    }catch {
        return null
    }
}