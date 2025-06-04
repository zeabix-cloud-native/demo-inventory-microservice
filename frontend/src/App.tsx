import { BrowserRouter as Router, Routes, Route } from 'react-router-dom';
import ProductList from './components/ProductList';
import ProductForm from './components/ProductForm';
import './App.css'

function App() {
  return (
    <Router>
      <div className="app">
        <Routes>
          <Route path="/" element={<ProductList />} />
          <Route path="/product/new" element={<ProductForm />} />
          <Route path="/product/:id" element={<ProductForm />} />
        </Routes>
      </div>
    </Router>
  );
}

export default App
