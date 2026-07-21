// A plain static page - just a component the router mounts
// at a specific path. No data, no state
export function About() {
    return(
        <article>
            <h2>About</h2>
            <p>
                A React single-page app over our demo LibraryAPI - the client 
                half of our web app.
            </p>
        </article>
    )
}