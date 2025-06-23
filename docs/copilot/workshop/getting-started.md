# Getting Started with GitHub Copilot
## Zero to Hero: Your First Steps in AI-Assisted Development

This guide will take you from never having used GitHub Copilot to confidently using it for full-stack development in the Demo Inventory Microservice project.

## 🎯 What You'll Learn

- How to set up and configure GitHub Copilot
- Basic to advanced prompting techniques
- Understanding project context and patterns
- Generating your first meaningful code
- Best practices for AI-assisted development

## 📋 Prerequisites Check

Before we begin, let's verify your setup:

```bash
# 1. Check VS Code version
code --version

# 2. Verify .NET SDK
dotnet --version  # Should be 9.0 or higher

# 3. Verify Node.js
node --version    # Should be 20.0 or higher

# 4. Check Docker
docker --version

# 5. Verify Git configuration
git config --list | grep user
```

## 🚀 Step 1: GitHub Copilot Setup

### 1.1 Install GitHub Copilot Extensions

Open VS Code and install these essential extensions:

1. **GitHub Copilot** - The main AI pair programmer
2. **GitHub Copilot Chat** - AI assistant for conversations
3. **C# Dev Kit** - Enhanced C# support
4. **ES7+ React/Redux/React-Native snippets** - React development
5. **Thunder Client** - API testing in VS Code

**Quick Installation:**
```bash
# Install extensions via command line
code --install-extension GitHub.copilot
code --install-extension GitHub.copilot-chat
code --install-extension ms-dotnettools.csdevkit
code --install-extension dsznajder.es7-react-js-snippets
code --install-extension rangav.vscode-thunder-client
```

### 1.2 Sign In and Verify Access

1. Open VS Code
2. Press `Ctrl+Shift+P` (or `Cmd+Shift+P` on Mac)
3. Type "GitHub Copilot: Sign In"
4. Follow the authentication flow
5. Verify the Copilot icon appears in the status bar

### 1.3 Configure Workspace Settings

Create `.vscode/settings.json` in your project root:

```json
{
  "github.copilot.enable": {
    "*": true,
    "plaintext": false,
    "markdown": true,
    "scminput": false
  },
  "github.copilot.advanced": {
    "length": 500,
    "temperature": 0.1,
    "top_p": 1,
    "listCount": 3
  },
  "dotnet.defaultSolution": "DemoInventory.sln",
  "typescript.preferences.importModuleSpecifier": "relative",
  "eslint.workingDirectories": ["frontend"],
  "prettier.configPath": "frontend/.prettierrc"
}
```

## 🎨 Step 2: Understanding Copilot Modes

GitHub Copilot works in two main modes:

### 2.1 Inline Suggestions (Ghost Text)
- Appears as you type
- Shows code completions in gray text
- Accept with `Tab`, reject with `Esc`
- Cycle through suggestions with `Alt+]` and `Alt+[`

### 2.2 Chat Mode (Copilot Chat)
- Open with `Ctrl+Shift+I` (or `Cmd+Shift+I`)
- Conversational interface for complex tasks
- Use `@workspace` for project-specific help
- Great for explanations, debugging, and code reviews

## 🏗️ Step 3: Project Context Understanding

### 3.1 Explore the Project Structure

Open the Demo Inventory project and examine:

```
demo-inventory-microservice/
├── backend/
│   ├── src/
│   │   ├── DemoInventory.Domain/        # Business logic
│   │   ├── DemoInventory.Application/   # Use cases
│   │   ├── DemoInventory.Infrastructure/# Data access
│   │   └── DemoInventory.API/          # Web API
│   └── tests/
├── frontend/
│   ├── src/
│   │   ├── components/                 # React components
│   │   ├── services/                   # API services
│   │   ├── types/                      # TypeScript types
│   │   └── pages/                      # Page components
│   └── tests/
└── docs/
```

### 3.2 Key Project Patterns

Copilot learns from your project. Key patterns to understand:

**Backend (.NET Clean Architecture):**
- Domain entities with private setters
- Repository pattern for data access
- DTOs for data transfer
- Dependency injection throughout
- Async/await for I/O operations

**Frontend (React + TypeScript):**
- Functional components with hooks
- TypeScript interfaces for type safety
- Custom hooks for shared logic
- Axios for API communication
- Error boundaries for error handling

## 💡 Step 4: Your First Copilot Experience

Let's create a simple feature to understand how Copilot works.

### 4.1 Basic Inline Suggestions

1. **Open a new file:** `backend/src/DemoInventory.Domain/Entities/Category.cs`

2. **Start typing:** Just type the class declaration
```csharp
namespace DemoInventory.Domain.Entities;

public class Category
{
    // Start typing "public int" and watch Copilot suggest
```

3. **Watch the magic:** Copilot will suggest properties and methods based on:
   - The class name "Category"
   - Existing patterns in your project
   - Common C# conventions

### 4.2 Chat-Based Code Generation

1. **Open Copilot Chat** (`Ctrl+Shift+I`)

2. **Try this prompt:**
```
@workspace I need to create a Category entity in the Domain layer following the project's Clean Architecture patterns. The entity should have:
- Id (Guid)
- Name (string, required)
- Description (string, optional)
- IsActive (bool, default true)
- CreatedAt (DateTime)
- UpdatedAt (DateTime)

Please follow the existing entity patterns in the project.
```

3. **Analyze the response:**
   - Does it follow Clean Architecture?
   - Are the property patterns consistent?
   - Is the code style matching the project?

### 4.3 Refining with Follow-up Questions

After Copilot generates code, you can refine it:

```
Great! Now can you:
1. Add validation to ensure Name is not empty
2. Add a factory method for creating new categories
3. Include proper ToString() override
4. Add domain events if needed
```

## 🎯 Step 5: Understanding Context Windows

### 5.1 What Copilot Can See

Copilot's context includes:
- **Current file** you're editing
- **Open files** in your editor
- **Project structure** (file names, folder structure)
- **Related files** (imports, references)
- **Git history** (recent commits)

### 5.2 Providing Better Context

**Instead of:**
```csharp
// Create a product service
```

**Try:**
```csharp
// Create a ProductService in the Application layer that follows Clean Architecture
// It should use dependency injection for IProductRepository
// Include proper error handling and logging
// Follow the existing service patterns in the project
```

### 5.3 Using @workspace Effectively

In Copilot Chat, use `@workspace` to leverage project context:

```
@workspace Show me how to create a new API endpoint following the existing controller patterns
@workspace Generate unit tests for the ProductService using the project's testing patterns
@workspace Help me fix this EF Core query to follow the repository pattern used in the project
```

## 🔧 Step 6: Practical Exercises

### Exercise 1: Create Your First Entity

**Task:** Create a `Supplier` entity in the Domain layer

**Requirements:**
- Follow existing entity patterns
- Include proper validation
- Add factory method
- Include audit fields

**Steps:**
1. Open `backend/src/DemoInventory.Domain/Entities/`
2. Create new file `Supplier.cs`
3. Use Copilot to generate the entity
4. Refine with follow-up prompts

**Solution Approach:**
```csharp
// Start with this and let Copilot complete
namespace DemoInventory.Domain.Entities;

public class Supplier : BaseEntity
{
    // Let Copilot suggest properties and methods
```

### Exercise 2: Create a React Component

**Task:** Create a `SupplierCard` component

**Requirements:**
- TypeScript interface for props
- Responsive design
- Click handlers
- Error states

**Steps:**
1. Open `frontend/src/components/`
2. Create `SupplierCard.tsx`
3. Use Copilot to generate component
4. Refine the implementation

**Solution Approach:**
```typescript
// Start with this and let Copilot complete
interface SupplierCardProps {
  // Let Copilot suggest props
}

export const SupplierCard: React.FC<SupplierCardProps> = ({ 
  // Let Copilot complete
}) => {
  // Component logic
};
```

### Exercise 3: Generate Tests

**Task:** Create unit tests for your Supplier entity

**Steps:**
1. Open Copilot Chat
2. Ask for comprehensive tests
3. Review and refine the generated tests

**Prompt:**
```
@workspace Generate comprehensive unit tests for the Supplier entity using xUnit, NSubstitute, and FluentAssertions. Include tests for:
- Entity creation and validation
- Business rule enforcement
- Factory method behavior
- Property updates
Follow the existing test patterns in the project.
```

## 🎓 Step 7: Best Practices for Beginners

### 7.1 Do's ✅
- **Start simple:** Begin with basic code generation
- **Provide context:** Always explain what layer/pattern you're working with
- **Review everything:** Never accept code without understanding it
- **Ask questions:** Use chat mode to understand generated code
- **Iterate:** Refine code through follow-up conversations

### 7.2 Don'ts ❌
- **Don't blindly accept:** Always review and understand the code
- **Don't ignore architecture:** Ensure code follows Clean Architecture
- **Don't skip tests:** Always generate tests for new code
- **Don't ignore errors:** Address compilation errors immediately
- **Don't ignore security:** Review for security implications

### 7.3 Prompting Tips

**Effective Prompts:**
```
// Good: Specific and contextual
"Create a ProductService in the Application layer that implements CQRS pattern using MediatR, following the existing service patterns in the project"

// Good: Includes requirements
"Generate a React component for product listing with TypeScript, pagination, search functionality, and error handling"

// Good: References existing patterns
"Create unit tests for ProductService using the same testing patterns as OrderService tests"
```

**Ineffective Prompts:**
```
// Too vague
"Create a service"

// No context
"Make a component"

// Too broad
"Build the entire feature"
```

## 🚀 Next Steps

Congratulations! You've completed the getting started guide. You should now:

✅ Have GitHub Copilot configured and working  
✅ Understand the basic modes of operation  
✅ Know how to provide effective context  
✅ Have generated your first meaningful code  
✅ Understand best practices for AI-assisted development  

### Continue Your Learning

1. **[Backend Development Exercise](exercises/backend-exercise.md)** - Build a complete API feature
2. **[Frontend Development Exercise](exercises/frontend-exercise.md)** - Create React components
3. **[Testing Exercise](exercises/testing-exercise.md)** - Master AI-assisted testing
4. **[Best Practices Guide](best-practices.md)** - Advanced techniques and patterns

### Quick Reference

**Keyboard Shortcuts:**
- `Tab` - Accept inline suggestion
- `Esc` - Reject inline suggestion  
- `Alt+]` - Next suggestion
- `Alt+[` - Previous suggestion
- `Ctrl+Shift+I` - Open Copilot Chat
- `Ctrl+I` - Inline chat in editor

**Useful Chat Commands:**
- `@workspace` - Project-specific help
- `/explain` - Explain selected code
- `/fix` - Fix issues in code
- `/tests` - Generate tests
- `/doc` - Generate documentation

---

## 🎯 Checkpoint

Before moving on, make sure you can:

- [ ] Generate basic code using inline suggestions
- [ ] Use Copilot Chat for complex tasks
- [ ] Provide effective context in prompts
- [ ] Review and refine generated code
- [ ] Follow the project's architectural patterns

**Having trouble?** Check the [Troubleshooting Guide](troubleshooting.md) or ask in Copilot Chat using `@workspace Help me troubleshoot GitHub Copilot setup issues`.

Ready for the next challenge? Let's dive into building complete features! 🚀