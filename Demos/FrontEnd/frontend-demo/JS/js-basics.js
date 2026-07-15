// ============================================================
// js-basics - the JavaScript language cheatsheet (Library data).
// Lectured in class, kept for study. Run it any time:
//   node js/js-basics.js
// (or paste it into the browser DevTools console, top-down - later
// sections use the data and helpers declared above them).
// Sections: 1 datatypes, 2 coercion + truthy/falsy,
// 3 let/const/scope, 4 array methods, 5 errors, 6 functions,
// 7 this + closures. js/app.js puts all of it to work.
// ============================================================

// The same catalog shape js/app.js renders - and GET /api/Inventory returns.
const catalogItems = [
    { sku: "BK-101", name: "Clean Code",                price: 29.99, currentStock: 12 },
    { sku: "BK-102", name: "The Pragmatic Programmer",  price: 34.99, currentStock: 7 },
    { sku: "BK-103", name: "Design Patterns",           price: 44.99, currentStock: 3 },
    { sku: "BK-104", name: "Refactoring",               price: 39.99, currentStock: 0 },
];

// ================ 1. datatypes: the VALUE has the type ================
// Dynamic typing: in C# the variable has a type; here the value does.
// A variable can hold a string now and a number a line later - the
// language won't stop you. TypeScript (04) gives the seatbelt back.
console.log("--- 1. datatypes ---");

let title = "Clean Code";         // string
let price = 29.99;                // number - no int/decimal split; one floating-point type
let inStock = true;               // boolean
let discount = null;              // null = "deliberately empty" - a value you ASSIGN
let isbn;                         // undefined = "never assigned"
let skuKey = Symbol("BK-101");    // symbol (rare - guaranteed-unique keys)
let bigCount = 9007199254740993n; // bigint (beyond Number.MAX_SAFE_INTEGER)

console.log(typeof title, typeof price, typeof inStock); // string number boolean
console.log(typeof isbn);         // undefined
console.log(typeof discount);     // "object" - the null bug, standardized forever (interview classic)
console.log(0.1 + 0.2 === 0.3);   // false - same floating-point story as C#'s double

// ================ 2. coercion + truthy/falsy: always === ================
console.log("--- 2. coercion ---");

console.log("29.99" == 29.99);   // true  - == coerces before comparing
console.log("29.99" === 29.99);  // false - === compares type AND value. Default to ===.
console.log(1 + "1");            // "11"  - + with a string concatenates
console.log("3" * "4");          // 12    - * has no string meaning, so it converts

// The WHOLE falsy list: false, 0 (and -0), "", null, undefined, NaN (bigint adds 0n).
// Everything else is truthy - including "0" and [].
console.log(Boolean(""), Boolean("0"), Boolean(0), Boolean([])); // false true false true

// Why it matters in the page: everything read from an <input> is a STRING.
// qty * price works by accident; qty + 1 concatenates. Convert deliberately: Number(input.value).

// ================ 3. let / const / var and scope ================
console.log("--- 3. scope ---");

const TAX_RATE = 0.07;          // const = the BINDING can't change...
const branches = ["Central"];
branches.push("Northside");     // ...but the object it points at can. const != immutable.
console.log(TAX_RATE, branches); // 0.07 [ 'Central', 'Northside' ]

function scopeDemo() {
    if (true) {
        var oldStyle = "function-scoped";  // var ignores the block
        let modern = "block-scoped";       // let/const live and die in the { }
    }
    console.log(oldStyle);        // works - var leaked out of the block
    // console.log(modern);       // ReferenceError - uncomment and re-run to prove it
}
scopeDemo();

// House rule: const by default, let when reassignment is the point, var never
// (it's in the language for history, in this file for recognition).

// ================ 4. array methods: the LINQ cousins ================
// map = Select, filter = Where, find = FirstOrDefault, reduce = Aggregate.
// The => is the same lambda arrow you've typed in LINQ all along.
console.log("--- 4. array methods ---");

const names = catalogItems.map(item => item.name);
const available = catalogItems.filter(item => item.currentStock > 0);
const clean = catalogItems.find(item => item.sku === "BK-101");
const stockValue = catalogItems.reduce((sum, item) => sum + item.price * item.currentStock, 0);

console.log(names);                                   // all four titles
// backtick strings below = template literals - C#'s $""; named properly in section 6
console.log(`${available.length} of ${catalogItems.length} titles in stock`); // 3 of 4
console.log(clean.name, "found by sku");              // Clean Code found by sku
console.log("total stock value:", stockValue.toFixed(2)); // 739.78

// sort MUTATES - spread into a copy first (bitten-by-this is a rite of passage)
const byPrice = [...catalogItems].sort((a, b) => a.price - b.price);
console.log("cheapest:", byPrice[0].name);            // Clean Code

// classic loops still exist - methods are our default, but read (and write) both
for (const item of available) {           // for...of walks the VALUES of an iterable
    console.log(`${item.sku}: ${item.currentStock} in stock`);
}
for (let i = 0; i < byPrice.length; i++) { // index loop - when you need i itself
    console.log(`#${i + 1} by price: ${byPrice[i].name}`);
}

// ================ 5. errors: throw / try / catch / finally ================
// Same three keywords as C#, one looseness: JS lets you throw ANYTHING -
// always throw Error objects anyway; they carry a message and a stack.
console.log("--- 5. errors ---");

function requireInStock(item) {
    if (item.currentStock === 0) {
        throw new Error(`${item.sku} is out of stock`);  // you THROW an Error object
    }
    return item;
}

try {
    requireInStock(catalogItems[3]);         // BK-104 has 0 stock - this throws
} catch (e) {
    console.log("caught:", e.message);       // same try/catch shape as C#
} finally {
    console.log("finally always runs - cleanup lives here, throw or not");
}

// ================ 6. functions: three spellings + modern parameters ================
// Functions are VALUES - assign them, pass them, return them. C# delegates/Func<>,
// except here it's the default, not the feature.
console.log("--- 6. functions ---");

// declaration - hoisted (callable before its line)
function formatPrice(amount) {
    return `$${amount.toFixed(2)}`;   // template literal: backticks + ${} = C#'s $""
}

// expression - an ANONYMOUS function held in a variable
const isAvailable = function (item) {
    return item.currentStock > 0;
};

// arrow - concise; implicit return when there's no { }
const lineTotal = (item, qty = 1) => item.price * qty;   // default parameter

console.log(formatPrice(lineTotal(clean, 3)));        // $89.97
console.log("available?", isAvailable(catalogItems[3])); // available? false

// rest + spread: gather args in, spread arrays out
const skuList = (...skus) => skus.join(", ");
console.log(skuList("BK-101", "BK-102", "BK-103"));   // BK-101, BK-102, BK-103

// destructuring: unpack what you need (used everywhere in React)
const { name, currentStock: stock } = clean;
console.log(`${name} has ${stock} on the shelf`);     // Clean Code has 12 on the shelf

// ================ 7. `this` + closures - the load-bearing pair ================
// In C#, `this` is fixed at compile time. Here `this` is decided at CALL time:
// it's whatever is LEFT OF THE DOT.
console.log("--- 7. this + closures ---");

const cart = {
    items: [],
    add(item) {                       // method shorthand
        this.items.push(item);        // `this` = whatever is left of the dot at call time
        return this;                  // returning this enables chaining
    },
    total() {
        return this.items.reduce((sum, i) => sum + i.price, 0);
    },
};

cart.add(catalogItems[0]).add(catalogItems[1]);
console.log("cart total:", formatPrice(cart.total()));    // cart total: $64.98

const detached = cart.total;          // took the function, LOST the receiver
try {
    // sloppy-mode `this` falls back to the global object (window in a page,
    // globalThis under node) - its .items is undefined, so .reduce explodes
    detached();
} catch (e) {
    console.log("detached call throws:", e.constructor.name); // TypeError
}
console.log("rebound:", formatPrice(detached.call(cart)));   // .call/.bind fix it explicitly

// Arrow functions have NO OWN `this` - they capture the enclosing one.
// That's why the reduce callback above never needed rebinding, and why
// every handler in js/app.js's wiring block is an arrow.

// A CLOSURE: a function REMEMBERS the variables where it was BORN - even
// after the outer function returned. Function + captured environment.
function makeStockCounter(item) {
    let remaining = item.currentStock;      // captured - private to this counter
    return function checkout(qty) {
        if (qty > remaining) {
            return `only ${remaining} left of ${item.sku}`;
        }
        remaining -= qty;                   // the captured variable LIVES between calls
        return `${item.sku}: ${remaining} remaining`;
    };
}

const sellCleanCode = makeStockCounter(catalogItems[0]);
console.log(sellCleanCode(5));   // BK-101: 7 remaining
console.log(sellCleanCode(5));   // BK-101: 2 remaining - it remembered
console.log(sellCleanCode(5));   // only 2 left of BK-101 - and enforced

// No class, no field - the closure IS the state, genuinely private.
// The search handler in js/app.js is a closure over catalogItems,
// working exactly the same way.

// ================ end - js/app.js applies all of this to the page ================
