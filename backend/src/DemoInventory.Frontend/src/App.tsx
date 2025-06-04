import { useState } from 'react'
import './App.css'

function App() {
  const [count, setCount] = useState(0)

  return (
    <div className="min-h-screen bg-gray-100">
      <div className="container mx-auto px-4 py-8">
        <header className="text-center mb-8">
          <h1 className="text-4xl font-bold text-gray-800 mb-2">
            Demo Inventory Frontend
          </h1>
          <p className="text-gray-600">
            React + TypeScript + Vite + Tailwind CSS
          </p>
        </header>
        
        <main className="max-w-md mx-auto bg-white rounded-lg shadow-md p-6">
          <div className="text-center">
            <button
              className="bg-blue-500 hover:bg-blue-700 text-white font-bold py-2 px-4 rounded mb-4"
              onClick={() => setCount((count) => count + 1)}
            >
              Count is {count}
            </button>
            <p className="text-gray-600">
              Click the button to test React functionality
            </p>
          </div>
        </main>
        
        <footer className="text-center mt-8 text-gray-500">
          <p>Ready to connect to the Demo Inventory API</p>
        </footer>
      </div>
    </div>
  )
}

export default App