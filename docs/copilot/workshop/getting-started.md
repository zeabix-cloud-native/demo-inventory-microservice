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

## 📥 Repository Setup

Before starting the exercises, you need to clone the repository and set up your local environment:

### Clone and Setup Local Repository

1. **Clone the repository:**
```bash
git clone https://github.com/zeabix-cloud-native/demo-inventory-microservice.git
cd demo-inventory-microservice
```

2. **Create and checkout to a feature branch:**
```bash
# Create a new feature branch for your exercises
git checkout -b feature/copilot-exercises

# Or if you want to work on a specific feature
git checkout -b feature/your-feature-name
```

3. **Verify your setup:**
```bash
# Check current branch
git branch

# Ensure all dependencies are ready
dotnet restore
cd frontend && npm install && cd ..
```

**Why use a feature branch?**
- Keeps your changes isolated from the main codebase
- Allows you to experiment safely with Copilot-generated code
- Follows Git best practices for feature development
- Makes it easy to reset if needed: `git checkout main && git branch -D feature/your-branch-name`

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

GitHub Copilot offers multiple interaction modes, each designed for different development scenarios. Understanding when and how to use each mode will maximize your productivity.

### 2.1 Auto-Complete Mode (Inline Suggestions)

**What it is:**
- Real-time code suggestions that appear as you type
- Shows up as gray "ghost text" in your editor
- Powered by machine learning trained on billions of lines of code

**How to use:**
- Simply start typing - suggestions appear automatically
- Accept with `Tab`, reject with `Esc`
- Cycle through alternatives with `Alt+]` and `Alt+[`
- Works in any file type (code, configs, documentation)

**Best for:**
- **Quick code completion** - method implementations, property definitions
- **Boilerplate code** - constructors, getters/setters, common patterns
- **API calls** - HTTP requests, database queries
- **Unit test scaffolding** - test method structures
- **Documentation** - comments, README sections

**Example scenarios:**
```csharp
// Type this...
public class Product
{
    // Copilot suggests properties based on class name
    public int Id { get; set; }
    public string Name { get; set; }
    public decimal Price { get; set; }
}

// Or this...
public async Task<IActionResult> GetProducts()
{
    // Copilot suggests the entire method body
}
```

### 2.2 Edit Mode (Code Modification)

**What it is:**
- Copilot can modify existing code in-place
- Triggered through editor commands or chat interface
- Understands context of existing code structure

**How to use:**
- Select code and use `Ctrl+I` for inline editing
- Use chat commands like `/fix` or `/optimize`
- Highlight problematic code and ask for improvements

**Best for:**
- **Refactoring existing code** - improving structure, performance
- **Bug fixing** - correcting logic errors, handling edge cases
- **Code optimization** - improving performance, readability
- **Adding features to existing methods** - extending functionality
- **Converting between patterns** - changing from sync to async, etc.

**Example scenarios:**
```csharp
// Select this existing code and ask Copilot to optimize it
public List<Product> FilterProducts(List<Product> products, string category)
{
    var result = new List<Product>();
    foreach(var product in products)
    {
        if(product.Category == category)
            result.Add(product);
    }
    return result;
}

// Copilot can convert it to LINQ, add null checks, improve performance
```

### 2.3 Agent Mode (VS Code Chat Interface)

**What it is:**
- Conversational AI assistant within VS Code
- Contextually aware of your entire workspace
- Can perform complex, multi-step development tasks

**How to use:**
- Open with `Ctrl+Shift+I` (or `Cmd+Shift+I` on Mac)
- Use `@workspace` for project-specific assistance
- Ask questions, request explanations, get code reviews
- Have extended conversations about architecture and design

**Best for:**
- **Complex feature development** - multi-file, multi-layer implementations
- **Architecture discussions** - design patterns, project structure
- **Code reviews and analysis** - understanding existing code
- **Debugging assistance** - analyzing stack traces, finding root causes
- **Learning and explanations** - understanding how code works
- **Project-wide refactoring** - changes across multiple files

**Example interactions:**
```
@workspace I need to implement user authentication following Clean Architecture. 
Create the domain entities, application services, and API controllers with proper 
validation and JWT token generation.

@workspace Analyze this error and suggest fixes: [paste stack trace]

@workspace Review this pull request for security vulnerabilities and performance issues
```

### 2.4 Code Agent (GitHub Portal)

**What it is:**
- GitHub Copilot integrated directly into GitHub.com
- Web-based AI assistant for repository management
- Works with pull requests, issues, and repository exploration

**How to use:**
- Available directly in GitHub.com interface
- Access through repository pages, PR reviews, issue discussions
- Ask questions about repository structure and history
- Get help with GitHub-specific tasks

**Best for:**
- **Repository exploration** - understanding unfamiliar codebases
- **Pull request reviews** - automated code analysis and suggestions
- **Issue triage** - analyzing bug reports and feature requests
- **Documentation generation** - creating README files, API docs
- **Release planning** - analyzing changes between versions
- **Code search and analysis** - finding patterns across repositories

**Example use cases:**
- "Explain the architecture of this repository"
- "Review this pull request for breaking changes"
- "Generate release notes for version 2.1.0"
- "Find all usages of deprecated methods"
- "Suggest labels for this issue"

## 🎯 Choosing the Right Mode for Your Task

| Task Type | Recommended Mode | Why This Mode? |
|-----------|------------------|----------------|
| **Writing new methods/classes** | Auto-Complete Mode | Fast, real-time suggestions as you type |
| **Fixing bugs in existing code** | Edit Mode | Focused on modifying specific code sections |
| **Complex feature development** | Agent Mode (VS Code) | Handles multi-file, architectural decisions |
| **Code reviews and analysis** | Agent Mode (VS Code) | Deep understanding of codebase context |
| **Repository exploration** | Code Agent (GitHub) | Best for understanding unfamiliar repos |
| **PR reviews and documentation** | Code Agent (GitHub) | Integrated with GitHub workflow |
| **Learning how something works** | Agent Mode (VS Code) | Conversational explanations |
| **Quick code snippets** | Auto-Complete Mode | Immediate, contextual suggestions |
| **Refactoring large sections** | Edit Mode + Agent Mode | Combination approach for complex changes |

## 💡 Pro Tips for Mode Selection

### Start Simple, Scale Up
1. **Begin with Auto-Complete** for basic code writing
2. **Switch to Edit Mode** when you need to modify existing code
3. **Use Agent Mode** for complex discussions and multi-step tasks
4. **Leverage Code Agent** for repository-level understanding

### Context Matters
- **Auto-Complete** works best when you have a clear idea of what to write
- **Edit Mode** excels when you know what needs to change but not how
- **Agent Mode** shines when you need to think through problems
- **Code Agent** is perfect for understanding larger codebases

### Combine Modes Effectively
```
Typical workflow:
1. Use Code Agent (GitHub) to understand the repository structure
2. Use Agent Mode (VS Code) to plan your feature implementation
3. Use Auto-Complete Mode to write the initial code
4. Use Edit Mode to refine and optimize
5. Use Agent Mode to generate tests and documentation
```

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