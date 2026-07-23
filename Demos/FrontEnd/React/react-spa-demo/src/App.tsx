import './App.css'
import { CatalogPage } from './components/CatalogPage';
import { NavLink, Route, Routes } from 'react-router-dom';
import { About } from './pages/About';
import { BookDetail } from './pages/BookDetail';
import { LoginPage } from './pages/LoginPage';
import { RequireAuth } from './components/RequireAuth';
import { AdminPage } from './pages/AdminPage';
import { useAuth } from './auth/useAuth';

function App() {

  const { status, user, logout } = useAuth()

  return (
    <div className="app">
      <header className='app-header'>
        <h1>Library</h1>
        <nav className='app-header'>
          <NavLink to="/">Catalog</NavLink>
          <NavLink to="/about">About</NavLink>
          {/* Only admins can see the admin link */}
          {user?.role === "admin" && <NavLink to="/admin">Admin</NavLink>}
        </nav>

      <div className='auth-box'>
        {status === "authenticated" ? (
          <>
            <span>
              {user?.name} ({user?.role})
            </span>
            <button type='button' onClick={logout}>
              Sign out
            </button>
          </>
        ) : (
          <NavLink to="/login">Sign in</NavLink>
        )}
      </div>


      </header>

      <main>
        <Routes>
          <Route path='/' element={<CatalogPage />} />
          {/* I want to make two more pages - an about page (static - easy)
              and a BookDetail page for more info about a specific book (still easy - needs its own
                api call)*/} 
          <Route path='/inventory/:sku' element={<BookDetail />} />
          <Route path='/about' element= {<About /> } />
          <Route path='/login' element= {<LoginPage />} />
          {/* Guarded admin-page - must be signed in AND role = "admin" */}
          <Route 
            path='/admin'
            element={
              <RequireAuth role="admin">
                <AdminPage />
              </RequireAuth>
            }
          />
          <Route path='*' element={<p>Page not found</p>} /> {/* consider a NotFound.tsx page? */}
        </Routes>
      </main>    
    </div>
  );

}

export default App
