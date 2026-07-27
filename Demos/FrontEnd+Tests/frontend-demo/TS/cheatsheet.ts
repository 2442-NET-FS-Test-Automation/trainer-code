// This will be a TS cheatsheet
// Just language basics. 

// Simple + array types
// type annotation goes after the name

// Imports go at the top - let us bring in code (objects, types, functions, etc)
// from other .TS files. This syntax will come up ALOT in react.
import { Sku, FetchState } from "./types.js";

let sku: string = "BK-101"
let price: number = 29.99
let inStock: boolean = true;

let name = "jon"; // I actually don't stricly need the type annotation
// but if I try to shove an integer into this variable later, I won't compile.

let tags: string[] = ["architecture", "classic"];

console.log(sku, price, inStock, tags, name);


// We do have an escape from the strict type system - like var in C#
// "any" can let us opt out of type checking. Every time you use it
// your app gets more tech debt
let anything: any = "escape hatch";
anything = 42; // This becomes okay - thats the potential issue here
console.log(anything);

// There is another type that's more "honest" than any
// "unknown" - "we have no idea what this might be" - but the compiler
// makes you prove a shape before you can use it

// passing in a singular string to JSON.parse
let incoming: unknown = JSON.parse('"?"');


// Type checking that "unknown" variable's type before use
// TS lets me use it 
if(typeof incoming === "string") {
    console.log(incoming.toUpperCase());
}
// If I don't check it - it complains
//incoming.toUpperCase();

// Lets mix some types and also introduce tuples

// Labled tuple, fixed length, fixed element types
type PriceRange = [min: number, max: number];

// Lets use those types we imported
let state : FetchState = "loading"; // only the 4 options I defined will compile
const range: PriceRange = [0, 50]; 
const bkSku: Sku = "BK-101"; // still a string, signals intent
console.log(state, range[0], range[1], bkSku);