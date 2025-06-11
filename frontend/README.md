# Demo Inventory Frontend

React + TypeScript + Vite frontend for the Demo Inventory Microservice.

## Getting Started

### Prerequisites
- Node.js 24 or higher
- npm

### Setup

1. Install dependencies:
```bash
npm install
```

2. Configure API URL:
```bash
# Copy the example environment file
cp .env.example .env

# Edit .env if needed (default is http://localhost:5126/api)
```

3. Start development server:
```bash
npm run dev
```

### Available Scripts

- `npm run dev` - Start development server
- `npm run build` - Build for production
- `npm run lint` - Type check with TypeScript
- `npm run preview` - Preview production build

### Environment Configuration

The frontend connects to the backend API using a configurable URL:

- **Local Development**: Uses `http://localhost:5126/api` (set in `.env`)
- **Docker Environment**: Uses `http://localhost:5000/api` (set via build args)

To change the API URL, modify the `VITE_API_BASE_URL` environment variable.

## Docker Support

The frontend includes Docker support with multi-stage builds:

```bash
# Build with custom API URL
docker build --build-arg VITE_API_BASE_URL=http://localhost:5000/api -t frontend .

# Or use docker-compose (automatically sets correct URL)
docker-compose up
```

## Technical Details

This template provides a minimal setup to get React working in Vite with HMR and some ESLint rules.

Currently, two official plugins are available:

- [@vitejs/plugin-react](https://github.com/vitejs/vite-plugin-react/blob/main/packages/plugin-react) uses [Babel](https://babeljs.io/) for Fast Refresh
- [@vitejs/plugin-react-swc](https://github.com/vitejs/vite-plugin-react/blob/main/packages/plugin-react-swc) uses [SWC](https://swc.rs/) for Fast Refresh

## Expanding the ESLint configuration

If you are developing a production application, we recommend updating the configuration to enable type-aware lint rules:

```js
export default tseslint.config({
  extends: [
    // Remove ...tseslint.configs.recommended and replace with this
    ...tseslint.configs.recommendedTypeChecked,
    // Alternatively, use this for stricter rules
    ...tseslint.configs.strictTypeChecked,
    // Optionally, add this for stylistic rules
    ...tseslint.configs.stylisticTypeChecked,
  ],
  languageOptions: {
    // other options...
    parserOptions: {
      project: ['./tsconfig.node.json', './tsconfig.app.json'],
      tsconfigRootDir: import.meta.dirname,
    },
  },
})
```

You can also install [eslint-plugin-react-x](https://github.com/Rel1cx/eslint-react/tree/main/packages/plugins/eslint-plugin-react-x) and [eslint-plugin-react-dom](https://github.com/Rel1cx/eslint-react/tree/main/packages/plugins/eslint-plugin-react-dom) for React-specific lint rules:

```js
// eslint.config.js
import reactX from 'eslint-plugin-react-x'
import reactDom from 'eslint-plugin-react-dom'

export default tseslint.config({
  plugins: {
    // Add the react-x and react-dom plugins
    'react-x': reactX,
    'react-dom': reactDom,
  },
  rules: {
    // other rules...
    // Enable its recommended typescript rules
    ...reactX.configs['recommended-typescript'].rules,
    ...reactDom.configs.recommended.rules,
  },
})
```
