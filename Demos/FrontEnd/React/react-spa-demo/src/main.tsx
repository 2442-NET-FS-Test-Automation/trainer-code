import { StrictMode } from 'react'
import { createRoot } from 'react-dom/client'
import { BrowserRouter } from 'react-router-dom'
import './index.css'
import App from './App.tsx'
import { AuthProvider } from './auth/AuthContext.tsx'

// BrowserRouter is a premade component that we can use to add routing to our application
// It wrapps the whole app so that any components can declaure routes or link between them
// It uses the HTML5 history api (the thing that gives you browser history) - the URL changes
// but there's never a request for a new page. Its always that same index.html

createRoot(document.getElementById('root')!).render(
  <StrictMode>
    <BrowserRouter>
      <AuthProvider>
        <App />
      </AuthProvider>
    </BrowserRouter>
  </StrictMode>,
)
