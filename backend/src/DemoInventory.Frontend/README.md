# Demo Inventory Frontend

A React frontend application for the Demo Inventory Microservice, built with TypeScript, Vite, and Tailwind CSS.

## Technologies Used

- **React 19** - Component-based UI library
- **TypeScript 5** - Type-safe JavaScript
- **Vite 6** - Fast build tool and development server
- **Tailwind CSS 4** - Utility-first CSS framework
- **PostCSS** - CSS post-processor

## Available Scripts

- `npm run dev` - Start development server
- `npm run build` - Build for production
- `npm run lint` - Type check with TypeScript
- `npm run preview` - Preview production build

## Development

1. Install dependencies:
   ```bash
   npm install
   ```

2. Start the development server:
   ```bash
   npm run dev
   ```

3. Open [http://localhost:5173](http://localhost:5173) to view it in the browser.

## Building for Production

```bash
npm run build
```

The build artifacts will be stored in the `dist/` directory.

## Project Structure

```
src/
├── main.tsx          # Application entry point
├── App.tsx           # Main App component
├── App.css           # App-specific styles
└── index.css         # Global styles with Tailwind imports
```

## Future Enhancements

- Connect to Demo Inventory API
- Add product management components
- Implement state management
- Add unit tests with Vitest
- Add component documentation with Storybook