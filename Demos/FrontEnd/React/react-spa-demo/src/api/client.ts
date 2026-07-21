import axios from "axios";

// One axios object/instance  for the whole app. Since we're only hitting one api
// we can centralize its url here. If you are hitting multiple api's just make
// separate clients. Can all be in the same file or one per file - up to you

// Later when we do login and we have to attach that JWT bearer token - 
// it'll be centralized in one place! No having to chase down every fetch call.
export const api = axios.create({
    baseURL: "http://localhost:5137"
})