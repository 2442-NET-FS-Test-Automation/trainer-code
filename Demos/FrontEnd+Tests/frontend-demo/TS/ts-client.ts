// This will act as our "ApiClient" class
// next week we'll use Axios to do this - as it comes with alot more functionality
// but it lets us see TS classes

export interface ApiError {
    status: number;
    message: string;
}

// This is an example of a type guard. The compiler knows about our types - the runtime doesn't
// This can let us have the runtime meaningfully check type for our generic functions.
// This is a scary looking example of what they look like in prod often
// export function isApiError(value: unknown): value is ApiError{
//     return typeof value === "object" && value !== null //scary looking If statement 
//         && "status" in value && "message" in value
// }

// Unknown forces us to type check before we can use whatever is in value
// "value is ApiError" - an example of a Type Predicate. It instructs the compiler 
// that if this function returns a "true" - the value that was passed in should 
// now be treated as an ApiError
export function isApiError(value: unknown): value is ApiError 
{   
    // 1. Primitives (strings, numbers, booleans) cannot be an ApiError - they aren't "objects"
    if(typeof value !== "object")
        return false;

    // 2. This will run as JS - JS evaluates "typeof null" to "object". We must exclude nulls explicitly
    if (value === null)
        return false;

    // 3. The object passed in MUST contain a 'status' property
    if(!("status" in value))
        return false;

    // 4. The object MUST contain a 'message' property
    if (!("message" in value))
        return false;

    // If all 4 constraints are satisfied (we pass to this point) - the compiler
    // is safe to treat whatever object who's reference we passed in as an ApiError
    return true;
}




export class ApiClient {

    // We can define "parameter properties" this lets us do 
    // access modifier (public/private/protected), readonly (or not)
    // name, and type ("and default value")
    constructor(private readonly baseUrl: string = "http://localhost:5137") {}


    // Methods would go down here - methods/properties in classes
    // can be static or instance methods - same logic as C#

    // TS has generics - we represent a generic with the 'T' - same as C#
    // just means it can work with any type 
    // This method returns either an object of some type OR an ApiError (we define that)
    async getJson<T>(path: string): Promise<T | ApiError> {
        // method logic goes here
        try {
            const res = await fetch(`${this.baseUrl}${path}`);
            // This goes to the console
            if(!res.ok) return { status: res.status, message: `API said: ${res.status}`};
            
            // Serialize the response make sure it matches whatever type the user gave
            // when they invoked this function
            // _client.getJson<InventoryItem>() - this is how i'd call this
            // Whatever object I got back from the API BETTER MATCH the shape of InventoryItem
            return await res.json() as T; 

        } catch (err) {
            // Is err of type Error? If it is log its message. Otherwise - idk how did that. 
            console.log(err instanceof Error ? err.message : "unknown error?");
            return { status: 0, message:"Cannot reach the API. Check if it's on, or CORS"};
        }

    }
}