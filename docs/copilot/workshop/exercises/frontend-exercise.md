# Frontend Development Exercise
## Building React Components with TypeScript and Copilot

In this exercise, you'll use GitHub Copilot to build the frontend for the Category Management feature. We'll create React components with TypeScript, API integration, and modern UI patterns.

## 🎯 Learning Objectives

- Generate TypeScript interfaces from backend DTOs
- Create React functional components with hooks
- Implement API integration with error handling
- Build forms with validation and state management
- Create responsive UI components
- Generate comprehensive component tests

## 📋 Prerequisites

- Completed [Getting Started Guide](../getting-started.md) 
- Completed [Backend Development Exercise](backend-exercise.md)
- Frontend dependencies installed (`npm install`)
- API server running on localhost:5000

## 🏗️ Exercise Overview

We'll build a complete Category Management UI with:

```
📁 Types
  └── Category interfaces and types
  
📁 Services  
  └── API service for category operations
  
📁 Components
  ├── CategoryCard - Display single category
  ├── CategoryList - Display categories grid
  ├── CategoryForm - Create/edit categories
  └── CategorySearch - Search and filter
  
📁 Pages
  └── CategoriesPage - Main categories page
  
📁 Hooks
  └── useCategories - Custom hook for state management
  
📁 Tests
  └── Component tests with React Testing Library
```

## 🔧 Step 1: TypeScript Types and Interfaces

### 1.1 Create Category Types

1. **Navigate to** `frontend/src/types/`
2. **Create** `category.types.ts`

**Copilot Chat Prompt:**
```
@workspace Based on the CategoryDto and CreateCategoryDto from the backend API, create TypeScript interfaces for the frontend. Include:

1. Category interface with all properties from CategoryDto
2. CreateCategoryRequest interface for API requests  
3. UpdateCategoryRequest interface for updates
4. CategoryFilters interface for search/filtering
5. ApiResponse<T> generic type for API responses

Follow the existing TypeScript type patterns in the project and ensure proper typing for dates, optional properties, and API responses.
```

### 1.2 Create API Error Types

**Follow-up prompt:**
```
Add TypeScript interfaces for API error handling:
- ApiError interface with error message, status code, and details
- ValidationError interface for form validation errors
- CategoryError union type for category-specific errors

Follow the existing error handling patterns in the project.
```

### 1.3 Expected Result

Your types should look similar to:

```typescript
// category.types.ts
export interface Category {
  id: string;
  name: string;
  description?: string;
  isActive: boolean;
  createdAt: string;
  updatedAt: string;
  createdBy?: string;
  updatedBy?: string;
}

export interface CreateCategoryRequest {
  name: string;
  description?: string;
}

export interface UpdateCategoryRequest {
  name: string;
  description?: string;
}

export interface CategoryFilters {
  search?: string;
  isActive?: boolean;
  sortBy?: 'name' | 'createdAt' | 'updatedAt';
  sortOrder?: 'asc' | 'desc';
}

export interface ApiResponse<T> {
  data: T;
  success: boolean;
  message?: string;
}

export interface ApiError {
  message: string;
  status: number;
  details?: Record<string, string[]>;
}
```

## 🌐 Step 2: API Service Layer

### 2.1 Create Category API Service

1. **Navigate to** `frontend/src/services/`
2. **Create** `categoryApi.ts`

**Copilot Chat Prompt:**
```
@workspace Create a categoryApi service following the existing API service patterns in the project. The service should:

1. Use axios for HTTP requests
2. Include all CRUD operations for categories:
   - getCategories(filters?: CategoryFilters)
   - getCategoryById(id: string)
   - createCategory(category: CreateCategoryRequest)
   - updateCategory(id: string, category: UpdateCategoryRequest)
   - deleteCategory(id: string)
   - activateCategory(id: string)
   - deactivateCategory(id: string)

3. Include proper error handling and response typing
4. Use the base API configuration from existing services
5. Handle authentication headers if required
6. Include request/response interceptors for common operations

Follow the existing API service patterns, error handling, and TypeScript typing in the project.
```

### 2.2 Add Error Handling

**Follow-up prompt:**
```
Enhance the API service with comprehensive error handling:
- Network error handling
- HTTP status code handling
- Validation error extraction
- Retry logic for failed requests
- Loading state management

Create utility functions for common error scenarios.
```

## 🎣 Step 3: Custom Hooks

### 3.1 Create useCategories Hook

1. **Navigate to** `frontend/src/hooks/`
2. **Create** `useCategories.ts`

**Copilot Chat Prompt:**
```
@workspace Create a useCategories custom hook for managing category state following the existing custom hook patterns in the project. The hook should:

1. Manage categories state (loading, data, error)
2. Provide methods for CRUD operations
3. Handle optimistic updates
4. Include search and filtering capabilities
5. Cache data appropriately
6. Handle error states and retry logic

Return an object with:
- categories: Category[]
- loading: boolean
- error: string | null
- createCategory: (category: CreateCategoryRequest) => Promise<void>
- updateCategory: (id: string, category: UpdateCategoryRequest) => Promise<void>
- deleteCategory: (id: string) => Promise<void>
- toggleCategoryStatus: (id: string) => Promise<void>
- searchCategories: (filters: CategoryFilters) => void
- refreshCategories: () => Promise<void>

Follow React hooks best practices and existing patterns in the project.
```

### 3.2 Create Form Validation Hook

1. **Create** `useCategoryForm.ts`

**Copilot Chat Prompt:**
```
@workspace Create a useCategoryForm hook for handling category form state and validation. The hook should:

1. Manage form state (values, errors, touched fields)
2. Provide validation rules for category data
3. Handle form submission
4. Reset form state
5. Handle loading states during submission

Include validation for:
- Name is required and max 100 characters
- Description is optional and max 500 characters
- Real-time validation feedback
- Submit button state management

Follow the existing form handling patterns in the project.
```

## 🧩 Step 4: React Components

### 4.1 Create CategoryCard Component

1. **Navigate to** `frontend/src/components/categories/`
2. **Create** `CategoryCard.tsx`

**Copilot Chat Prompt:**
```
@workspace Create a CategoryCard component following the existing component patterns in the project. The component should:

1. Display category information in a card layout
2. Show name, description, status, and dates
3. Include action buttons (edit, delete, toggle status)
4. Handle click events for navigation
5. Show loading/disabled states
6. Include responsive design
7. Use proper TypeScript props interface

Props interface should include:
- category: Category
- onEdit?: (category: Category) => void
- onDelete?: (id: string) => void
- onToggleStatus?: (id: string) => void
- onClick?: (category: Category) => void
- isLoading?: boolean
- className?: string

Follow the existing component patterns, styling approach, and accessibility guidelines in the project.
```

### 4.2 Create CategoryList Component

1. **Create** `CategoryList.tsx`

**Copilot Chat Prompt:**
```
@workspace Create a CategoryList component that displays a grid of CategoryCard components. The component should:

1. Accept an array of categories
2. Handle empty states with proper messaging
3. Show loading states with skeleton placeholders
4. Include grid layout that's responsive
5. Support different view modes (grid, list)
6. Handle error states
7. Include pagination or infinite scroll if needed

Props interface should include:
- categories: Category[]
- loading?: boolean
- error?: string | null
- onCategoryEdit?: (category: Category) => void
- onCategoryDelete?: (id: string) => void
- onCategoryToggleStatus?: (id: string) => void
- onCategoryClick?: (category: Category) => void
- viewMode?: 'grid' | 'list'
- emptyMessage?: string

Follow the existing component patterns and styling in the project.
```

### 4.3 Create CategorySearch Component

1. **Create** `CategorySearch.tsx`

**Copilot Chat Prompt:**
```
@workspace Create a CategorySearch component for filtering and searching categories. The component should:

1. Include search input with debounced searching
2. Filter by active/inactive status
3. Sort options (name, date created, date updated)
4. Sort order toggle (asc/desc)
5. Clear filters functionality
6. Show active filter count
7. Responsive design for mobile

Props interface should include:
- filters: CategoryFilters
- onFiltersChange: (filters: CategoryFilters) => void
- onClear: () => void
- isLoading?: boolean
- totalCount?: number

Include proper TypeScript typing and follow existing search component patterns in the project.
```

### 4.4 Create CategoryForm Component

1. **Create** `CategoryForm.tsx`

**Copilot Chat Prompt:**
```
@workspace Create a CategoryForm component for creating and editing categories. The component should:

1. Handle both create and edit modes
2. Include form validation with real-time feedback
3. Show loading states during submission
4. Handle form submission and errors
5. Include cancel functionality
6. Auto-focus on first input
7. Keyboard navigation support
8. Proper error message display

Props interface should include:
- category?: Category (for edit mode)
- onSubmit: (data: CreateCategoryRequest | UpdateCategoryRequest) => Promise<void>
- onCancel: () => void
- isLoading?: boolean
- error?: string | null

Use the useCategoryForm hook created earlier and follow existing form patterns in the project.
```

## 📄 Step 5: Page Components

### 5.1 Create CategoriesPage

1. **Navigate to** `frontend/src/pages/`
2. **Create** `CategoriesPage.tsx`

**Copilot Chat Prompt:**
```
@workspace Create a CategoriesPage component that serves as the main page for category management. The component should:

1. Use the useCategories hook for state management
2. Include CategorySearch component for filtering
3. Display CategoryList with all categories
4. Handle create/edit/delete operations
5. Show modal or drawer for CategoryForm
6. Include page header with title and actions
7. Handle loading and error states
8. Include confirmation dialogs for destructive actions

Features to include:
- Search and filter categories
- Create new category button
- Edit category functionality
- Delete category with confirmation
- Toggle category status
- Responsive layout
- Proper error handling and user feedback

Follow the existing page component patterns and layout structure in the project.
```

### 5.2 Add Routing

**Follow-up prompt:**
```
@workspace Show me how to add the CategoriesPage to the React Router configuration following the existing routing patterns in the project. Include:

1. Route definition for /categories
2. Navigation menu item
3. Breadcrumb support if available
4. Protected route if authentication is required

Provide the code changes needed for the routing setup.
```

## 🧪 Step 6: Component Testing

### 6.1 Create CategoryCard Tests

1. **Navigate to** `frontend/src/components/categories/__tests__/`
2. **Create** `CategoryCard.test.tsx`

**Copilot Chat Prompt:**
```
@workspace Generate comprehensive tests for the CategoryCard component using React Testing Library, Jest, and following the existing test patterns in the project. Include tests for:

1. Renders category information correctly
2. Handles click events properly
3. Shows/hides action buttons based on props
4. Displays loading states
5. Handles disabled states
6. Accessibility testing
7. Responsive behavior
8. Error boundary testing

Follow the existing component testing patterns, mocking strategies, and assertions in the project.
```

### 6.2 Create CategoryForm Tests

1. **Create** `CategoryForm.test.tsx`

**Copilot Chat Prompt:**
```
@workspace Generate comprehensive tests for the CategoryForm component including:

1. Form rendering in create/edit modes
2. Input validation (required fields, max lengths)
3. Form submission handling
4. Error display and handling
5. Loading states during submission
6. Form reset functionality
7. Keyboard navigation
8. Accessibility compliance

Mock the useCategoryForm hook and follow existing form testing patterns in the project.
```

### 6.3 Create Hook Tests

1. **Navigate to** `frontend/src/hooks/__tests__/`
2. **Create** `useCategories.test.ts`

**Copilot Chat Prompt:**
```
@workspace Generate comprehensive tests for the useCategories hook using React Testing Library's renderHook utility. Include tests for:

1. Initial state loading
2. CRUD operations (create, update, delete)
3. Search and filtering functionality
4. Error handling scenarios
5. Optimistic updates
6. Cache management
7. Retry logic

Mock the categoryApi service and follow existing custom hook testing patterns in the project.
```

## 🎨 Step 7: Styling and Polish

### 7.1 Add Component Styles

**Copilot Chat Prompt:**
```
@workspace Based on the existing styling approach in the project, create styles for the category components. Include:

1. CategoryCard responsive card layout
2. CategoryList grid system
3. CategorySearch filter layout
4. CategoryForm styling
5. Loading states and animations
6. Error states styling
7. Mobile-responsive design

Follow the existing design system, color scheme, and styling patterns in the project.
```

### 7.2 Add Accessibility

**Follow-up prompt:**
```
Enhance the category components with accessibility features:
1. ARIA labels and roles
2. Keyboard navigation
3. Screen reader support
4. Focus management
5. Color contrast compliance
6. Alternative text for images

Review and update all components to meet accessibility standards.
```

## 🔍 Step 8: Integration and Testing

### 8.1 Integration Testing

1. **Start the backend API server**
2. **Start the frontend development server**
3. **Test the complete flow:**

```bash
# Terminal 1 - Backend
cd backend/src/DemoInventory.API
dotnet run

# Terminal 2 - Frontend  
cd frontend
npm run dev
```

### 8.2 Manual Testing Checklist

Navigate to `http://localhost:3000/categories` and verify:

- [ ] Categories page loads correctly
- [ ] Search functionality works
- [ ] Filter by status works
- [ ] Sort options work
- [ ] Create new category works
- [ ] Edit existing category works
- [ ] Delete category works (with confirmation)
- [ ] Toggle category status works
- [ ] Loading states display properly
- [ ] Error states are handled gracefully
- [ ] Form validation works correctly
- [ ] Responsive design works on mobile
- [ ] Accessibility features work

### 8.3 Run Automated Tests

```bash
# Run all frontend tests
npm test

# Run tests with coverage
npm run test:coverage

# Run component tests specifically
npm test -- --testPathPattern=components/categories

# Run hook tests
npm test -- --testPathPattern=hooks
```

## 🎓 Learning Reflection

### What You've Accomplished

✅ **Created TypeScript interfaces** from backend DTOs  
✅ **Built API service layer** with error handling  
✅ **Implemented custom hooks** for state management  
✅ **Created reusable components** with proper props  
✅ **Built complete page** with full functionality  
✅ **Added comprehensive tests** for all components  
✅ **Implemented responsive design** and accessibility  

### Key Frontend Copilot Patterns

1. **Component Generation**: Use specific props interfaces and existing patterns
2. **Hook Creation**: Reference existing custom hooks for consistent patterns
3. **API Integration**: Follow existing service patterns and error handling
4. **Testing**: Generate tests alongside components for comprehensive coverage
5. **TypeScript**: Leverage backend DTOs for type generation

### Common Frontend Challenges with Copilot

**Challenge**: Generated components don't match existing design system  
**Solution**: Reference existing components and styling patterns explicitly

**Challenge**: State management becomes complex  
**Solution**: Use custom hooks to encapsulate state logic

**Challenge**: TypeScript errors in generated code  
**Solution**: Provide clear type definitions and interfaces

## 🚀 Next Steps

1. **[Testing Exercise](testing-exercise.md)** - Advanced testing scenarios
2. **[Bug Fixing Exercise](bug-fixing-exercise.md)** - Debug frontend issues
3. **[Best Practices Guide](../best-practices.md)** - Advanced techniques

### Advanced Challenges

Want to extend your learning? Try these:

1. **Add Real-time Updates**: WebSocket integration for live category updates
2. **Implement Caching**: Add React Query or SWR for data caching
3. **Add Internationalization**: Multi-language support
4. **Performance Optimization**: Code splitting and lazy loading
5. **Add Data Visualization**: Charts and analytics for categories

## 💡 Frontend Pro Tips

- **Component Composition**: Build small, reusable components
- **Custom Hooks**: Extract complex logic into custom hooks
- **Error Boundaries**: Always include error boundaries for robustness
- **Loading States**: Provide feedback for all async operations
- **Accessibility First**: Consider accessibility from the beginning
- **Mobile First**: Design for mobile, then enhance for desktop

## 🔧 Troubleshooting

### Common Issues

**Issue**: Components not rendering properly  
**Solution**: Check prop types and ensure all required props are passed

**Issue**: API calls failing  
**Solution**: Verify backend is running and check CORS configuration

**Issue**: State not updating  
**Solution**: Ensure immutable updates in state setters

**Issue**: Tests failing  
**Solution**: Check mocks and ensure proper test setup

---

## 🎯 Exercise Completion

Congratulations! You've successfully built a complete frontend feature using GitHub Copilot. You should now be comfortable with:

- TypeScript interface generation
- React component development
- API integration patterns
- Custom hook creation
- Comprehensive testing
- Responsive design implementation

Ready for the next challenge? Let's master testing with AI! 🧪