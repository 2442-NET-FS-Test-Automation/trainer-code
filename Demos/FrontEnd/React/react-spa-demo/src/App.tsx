import './App.css'
import { CatalogPage } from './components/CatalogPage';

function App() {

  return (
    <div className="app">
      <header className='app-header'>
        <h1>Library</h1>
      </header>

      <main>
        <CatalogPage />
      </main>    

    </div>
  );

}

export default App
