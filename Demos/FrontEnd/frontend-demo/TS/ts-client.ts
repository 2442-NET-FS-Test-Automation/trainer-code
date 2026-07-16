// This will act as our "ApiClient" class
// next week we'll use Axios to do this - as it comes with alot more functionality
// but it lets us see TS classes

export interface ApiError {
    status: number;
    message: string;
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

        } catch {
            return { status: 0, message:"Cannot reach the API. Check if it's on, or CORS"};
        }

    }
}