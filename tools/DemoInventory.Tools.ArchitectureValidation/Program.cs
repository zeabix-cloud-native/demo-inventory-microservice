using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.CommandLine;

namespace DemoInventory.Tools.ArchitectureValidation;

/// <summary>
/// Architecture validation tool for Clean Architecture compliance
/// </summary>
public class Program
{
    public static async Task<int> Main(string[] args)
    {
        var pathOption = new Option<string>(
            name: "--path",
            description: "Path to validate (should be the backend src directory)",
            getDefaultValue: () => Path.Combine(Directory.GetCurrentDirectory(), "backend", "src"));

        var verboseOption = new Option<bool>(
            name: "--verbose",
            description: "Enable verbose output");

        var rootCommand = new RootCommand("Architecture Validator for Clean Architecture Compliance")
        {
            pathOption,
            verboseOption
        };

        rootCommand.SetHandler(async (path, verbose) =>
        {
            var validator = new ArchitectureValidator(verbose);
            await validator.ValidateAsync(path);
        }, pathOption, verboseOption);

        return await rootCommand.InvokeAsync(args);
    }
}

/// <summary>
/// Validates Clean Architecture principles and boundaries
/// </summary>
public class ArchitectureValidator
{
    private readonly bool _verbose;
    private readonly List<ArchitectureViolation> _violations = new();

    // Define layer dependencies (what each layer is allowed to reference)
    private readonly Dictionary<string, HashSet<string>> _allowedDependencies = new()
    {
        { "Domain", new HashSet<string>() }, // Domain should have no dependencies
        { "Application", new HashSet<string> { "Domain" } }, // Application can only depend on Domain
        { "Infrastructure", new HashSet<string> { "Domain", "Application" } }, // Infrastructure can depend on Domain and Application
        { "API", new HashSet<string> { "Domain", "Application", "Infrastructure" } } // API can depend on all layers
    };

    public ArchitectureValidator(bool verbose = false)
    {
        _verbose = verbose;
    }

    public async Task ValidateAsync(string path)
    {
        Console.WriteLine($"🏛️  Starting architecture validation on: {path}");
        Console.WriteLine();

        if (!Directory.Exists(path))
        {
            Console.WriteLine($"❌ Directory not found: {path}");
            return;
        }

        // Find all layer directories
        var layerDirs = Directory.GetDirectories(path, "DemoInventory.*")
            .Where(dir => !dir.Contains("Tests"))
            .ToList();

        if (layerDirs.Count == 0)
        {
            Console.WriteLine("❌ No layer directories found. Expected directories like DemoInventory.Domain, etc.");
            return;
        }

        Console.WriteLine($"Found {layerDirs.Count} layers to validate:");
        foreach (var dir in layerDirs)
        {
            var layerName = GetLayerName(dir);
            Console.WriteLine($"  - {layerName}");
        }
        Console.WriteLine();

        // Validate each layer
        foreach (var layerDir in layerDirs)
        {
            await ValidateLayerAsync(layerDir);
        }

        // Validate cross-layer dependencies
        await ValidateDependenciesAsync(layerDirs);

        PrintSummary();
    }

    private async Task ValidateLayerAsync(string layerDir)
    {
        var layerName = GetLayerName(layerDir);
        
        if (_verbose)
            Console.WriteLine($"🔍 Validating {layerName} layer...");

        switch (layerName)
        {
            case "Domain":
                await ValidateDomainLayerAsync(layerDir);
                break;
            case "Application":
                await ValidateApplicationLayerAsync(layerDir);
                break;
            case "Infrastructure":
                await ValidateInfrastructureLayerAsync(layerDir);
                break;
            case "API":
                await ValidateApiLayerAsync(layerDir);
                break;
        }
    }

    private async Task ValidateDomainLayerAsync(string layerDir)
    {
        var csFiles = Directory.GetFiles(layerDir, "*.cs", SearchOption.AllDirectories);

        foreach (var file in csFiles)
        {
            var content = await File.ReadAllTextAsync(file);
            var tree = CSharpSyntaxTree.ParseText(content, path: file);
            var root = await tree.GetRootAsync();

            // Check for external dependencies in Domain layer
            var usingDirectives = root.DescendantNodes().OfType<UsingDirectiveSyntax>();
            foreach (var usingDirective in usingDirectives)
            {
                var namespaceName = usingDirective.Name?.ToString() ?? "";
                
                // Domain should not reference any external frameworks except System.*
                if (!namespaceName.StartsWith("System") && 
                    !namespaceName.StartsWith("DemoInventory.Domain") &&
                    !string.IsNullOrEmpty(namespaceName))
                {
                    if (namespaceName.StartsWith("Microsoft.") || 
                        namespaceName.StartsWith("Newtonsoft.") ||
                        namespaceName.StartsWith("Entity") ||
                        namespaceName.Contains("Infrastructure") ||
                        namespaceName.Contains("Application"))
                    {
                        _violations.Add(new ArchitectureViolation(
                            file,
                            ViolationType.DependencyViolation,
                            $"Domain layer should not depend on {namespaceName}",
                            usingDirective.GetLocation().GetLineSpan().StartLinePosition.Line + 1
                        ));
                    }
                }
            }

            // Check for proper domain patterns
            await ValidateDomainPatterns(file, root);
        }
    }

    private async Task ValidateApplicationLayerAsync(string layerDir)
    {
        var csFiles = Directory.GetFiles(layerDir, "*.cs", SearchOption.AllDirectories);

        foreach (var file in csFiles)
        {
            var content = await File.ReadAllTextAsync(file);
            var tree = CSharpSyntaxTree.ParseText(content, path: file);
            var root = await tree.GetRootAsync();

            // Check that Application layer doesn't reference Infrastructure
            var usingDirectives = root.DescendantNodes().OfType<UsingDirectiveSyntax>();
            foreach (var usingDirective in usingDirectives)
            {
                var namespaceName = usingDirective.Name?.ToString() ?? "";
                
                if (namespaceName.Contains("Infrastructure"))
                {
                    _violations.Add(new ArchitectureViolation(
                        file,
                        ViolationType.DependencyViolation,
                        "Application layer should not depend on Infrastructure layer",
                        usingDirective.GetLocation().GetLineSpan().StartLinePosition.Line + 1
                    ));
                }
            }

            // Validate application patterns
            await ValidateApplicationPatterns(file, root);
        }
    }

    private async Task ValidateInfrastructureLayerAsync(string layerDir)
    {
        var csFiles = Directory.GetFiles(layerDir, "*.cs", SearchOption.AllDirectories);

        foreach (var file in csFiles)
        {
            var content = await File.ReadAllTextAsync(file);
            var tree = CSharpSyntaxTree.ParseText(content, path: file);
            var root = await tree.GetRootAsync();

            // Infrastructure should implement interfaces from Application layer
            await ValidateInfrastructurePatterns(file, root);
        }
    }

    private async Task ValidateApiLayerAsync(string layerDir)
    {
        var csFiles = Directory.GetFiles(layerDir, "*.cs", SearchOption.AllDirectories);

        foreach (var file in csFiles)
        {
            var content = await File.ReadAllTextAsync(file);
            var tree = CSharpSyntaxTree.ParseText(content, path: file);
            var root = await tree.GetRootAsync();

            // API layer should only contain presentation logic
            await ValidateApiPatterns(file, root);
        }
    }

    private async Task ValidateDomainPatterns(string file, SyntaxNode root)
    {
        // Check for entity patterns
        var classes = root.DescendantNodes().OfType<ClassDeclarationSyntax>();
        foreach (var classNode in classes)
        {
            var className = classNode.Identifier.ValueText;

            // Entities should have proper encapsulation
            if (className.EndsWith("Entity") || IsEntity(classNode))
            {
                await ValidateEntityPattern(file, classNode);
            }

            // Value objects should be immutable
            if (className.EndsWith("ValueObject") || IsValueObject(classNode))
            {
                await ValidateValueObjectPattern(file, classNode);
            }

            // Domain services should be stateless
            if (className.EndsWith("Service") && !className.EndsWith("ApplicationService"))
            {
                await ValidateDomainServicePattern(file, classNode);
            }
        }
    }

    private async Task ValidateApplicationPatterns(string file, SyntaxNode root)
    {
        var classes = root.DescendantNodes().OfType<ClassDeclarationSyntax>();
        foreach (var classNode in classes)
        {
            var className = classNode.Identifier.ValueText;

            // Application services should orchestrate domain operations
            if (className.EndsWith("Service") || className.EndsWith("Handler"))
            {
                await ValidateApplicationServicePattern(file, classNode);
            }

            // DTOs should be simple data carriers
            if (className.EndsWith("Dto") || className.EndsWith("DTO") || className.EndsWith("Request") || className.EndsWith("Response"))
            {
                await ValidateDtoPattern(file, classNode);
            }

            // Interfaces should be properly defined
            var interfaces = root.DescendantNodes().OfType<InterfaceDeclarationSyntax>();
            foreach (var interfaceNode in interfaces)
            {
                await ValidateInterfacePattern(file, interfaceNode);
            }
        }
    }

    private async Task ValidateInfrastructurePatterns(string file, SyntaxNode root)
    {
        var classes = root.DescendantNodes().OfType<ClassDeclarationSyntax>();
        foreach (var classNode in classes)
        {
            var className = classNode.Identifier.ValueText;

            // Repository implementations should implement repository interfaces
            if (className.EndsWith("Repository"))
            {
                await ValidateRepositoryImplementation(file, classNode);
            }

            // Infrastructure should not contain business logic
            await ValidateNoBusinessLogicInInfrastructure(file, classNode);
        }
    }

    private async Task ValidateApiPatterns(string file, SyntaxNode root)
    {
        var classes = root.DescendantNodes().OfType<ClassDeclarationSyntax>();
        foreach (var classNode in classes)
        {
            var className = classNode.Identifier.ValueText;

            // Controllers should be thin
            if (className.EndsWith("Controller"))
            {
                await ValidateControllerPattern(file, classNode);
            }
        }
    }

    private async Task ValidateEntityPattern(string file, ClassDeclarationSyntax classNode)
    {
        // Check for public setters (entities should have encapsulated state)
        var properties = classNode.Members.OfType<PropertyDeclarationSyntax>();
        foreach (var property in properties)
        {
            if (property.AccessorList?.Accessors.Any(a => a.IsKind(SyntaxKind.SetAccessorDeclaration) && 
                a.Modifiers.Any(m => m.IsKind(SyntaxKind.PublicKeyword))) == true)
            {
                _violations.Add(new ArchitectureViolation(
                    file,
                    ViolationType.DomainPattern,
                    $"Entity property '{property.Identifier.ValueText}' should not have public setter",
                    property.GetLocation().GetLineSpan().StartLinePosition.Line + 1
                ));
            }
        }
    }

    private async Task ValidateValueObjectPattern(string file, ClassDeclarationSyntax classNode)
    {
        // Value objects should be immutable (no setters)
        var properties = classNode.Members.OfType<PropertyDeclarationSyntax>();
        foreach (var property in properties)
        {
            if (property.AccessorList?.Accessors.Any(a => a.IsKind(SyntaxKind.SetAccessorDeclaration)) == true)
            {
                _violations.Add(new ArchitectureViolation(
                    file,
                    ViolationType.DomainPattern,
                    $"Value object property '{property.Identifier.ValueText}' should be immutable (no setter)",
                    property.GetLocation().GetLineSpan().StartLinePosition.Line + 1
                ));
            }
        }
    }

    private async Task ValidateDomainServicePattern(string file, ClassDeclarationSyntax classNode)
    {
        // Domain services should be stateless (no fields except readonly)
        var fields = classNode.Members.OfType<FieldDeclarationSyntax>();
        foreach (var field in fields)
        {
            if (!field.Modifiers.Any(m => m.IsKind(SyntaxKind.ReadOnlyKeyword)))
            {
                _violations.Add(new ArchitectureViolation(
                    file,
                    ViolationType.DomainPattern,
                    $"Domain service should be stateless - field should be readonly",
                    field.GetLocation().GetLineSpan().StartLinePosition.Line + 1
                ));
            }
        }
    }

    private async Task ValidateApplicationServicePattern(string file, ClassDeclarationSyntax classNode)
    {
        // Application services should depend on abstractions
        var methods = classNode.Members.OfType<MethodDeclarationSyntax>();
        foreach (var method in methods)
        {
            // Check that methods are properly async for I/O operations
            if (!method.Modifiers.Any(m => m.IsKind(SyntaxKind.AsyncKeyword)) && 
                method.ReturnType?.ToString().Contains("Task") != true)
            {
                var hasAsyncCall = method.Body?.DescendantNodes().OfType<InvocationExpressionSyntax>()
                    .Any(inv => inv.Expression.ToString().Contains("Async")) == true;

                if (hasAsyncCall)
                {
                    _violations.Add(new ArchitectureViolation(
                        file,
                        ViolationType.ApplicationPattern,
                        $"Method '{method.Identifier.ValueText}' should be async when calling async operations",
                        method.GetLocation().GetLineSpan().StartLinePosition.Line + 1
                    ));
                }
            }
        }
    }

    private async Task ValidateDtoPattern(string file, ClassDeclarationSyntax classNode)
    {
        // DTOs should be simple data carriers (no business logic)
        var methods = classNode.Members.OfType<MethodDeclarationSyntax>()
            .Where(m => !m.Identifier.ValueText.StartsWith("get_") && 
                       !m.Identifier.ValueText.StartsWith("set_") &&
                       !m.Identifier.ValueText.Equals("ToString") &&
                       !m.Identifier.ValueText.Equals("GetHashCode") &&
                       !m.Identifier.ValueText.Equals("Equals"));

        foreach (var method in methods)
        {
            _violations.Add(new ArchitectureViolation(
                file,
                ViolationType.ApplicationPattern,
                $"DTO '{classNode.Identifier.ValueText}' should not contain business logic method '{method.Identifier.ValueText}'",
                method.GetLocation().GetLineSpan().StartLinePosition.Line + 1
            ));
        }
    }

    private async Task ValidateInterfacePattern(string file, InterfaceDeclarationSyntax interfaceNode)
    {
        // Interfaces should follow ISP (Interface Segregation Principle)
        var methods = interfaceNode.Members.OfType<MethodDeclarationSyntax>();
        if (methods.Count() > 10)
        {
            _violations.Add(new ArchitectureViolation(
                file,
                ViolationType.ApplicationPattern,
                $"Interface '{interfaceNode.Identifier.ValueText}' has too many methods ({methods.Count()}). Consider splitting.",
                interfaceNode.GetLocation().GetLineSpan().StartLinePosition.Line + 1
            ));
        }
    }

    private async Task ValidateRepositoryImplementation(string file, ClassDeclarationSyntax classNode)
    {
        // Repository should implement an interface
        var baseList = classNode.BaseList;
        if (baseList == null || !baseList.Types.Any(t => t.Type.ToString().StartsWith("I")))
        {
            _violations.Add(new ArchitectureViolation(
                file,
                ViolationType.InfrastructurePattern,
                $"Repository '{classNode.Identifier.ValueText}' should implement an interface",
                classNode.GetLocation().GetLineSpan().StartLinePosition.Line + 1
            ));
        }
    }

    private async Task ValidateNoBusinessLogicInInfrastructure(string file, ClassDeclarationSyntax classNode)
    {
        // Infrastructure should not contain complex business logic
        var methods = classNode.Members.OfType<MethodDeclarationSyntax>();
        foreach (var method in methods)
        {
            var complexity = CalculateMethodComplexity(method);
            if (complexity > 5) // Infrastructure methods should be simple
            {
                _violations.Add(new ArchitectureViolation(
                    file,
                    ViolationType.InfrastructurePattern,
                    $"Infrastructure method '{method.Identifier.ValueText}' is too complex ({complexity}). Business logic should be in Domain layer.",
                    method.GetLocation().GetLineSpan().StartLinePosition.Line + 1
                ));
            }
        }
    }

    private async Task ValidateControllerPattern(string file, ClassDeclarationSyntax classNode)
    {
        // Controllers should be thin
        var methods = classNode.Members.OfType<MethodDeclarationSyntax>();
        foreach (var method in methods)
        {
            var statements = method.Body?.Statements.Count ?? 0;
            if (statements > 10)
            {
                _violations.Add(new ArchitectureViolation(
                    file,
                    ViolationType.ApiPattern,
                    $"Controller action '{method.Identifier.ValueText}' is too complex ({statements} statements). Should delegate to application services.",
                    method.GetLocation().GetLineSpan().StartLinePosition.Line + 1
                ));
            }
        }
    }

    private async Task ValidateDependenciesAsync(List<string> layerDirs)
    {
        foreach (var layerDir in layerDirs)
        {
            var layerName = GetLayerName(layerDir);
            var projectFile = Directory.GetFiles(layerDir, "*.csproj").FirstOrDefault();
            
            if (projectFile != null)
            {
                var content = await File.ReadAllTextAsync(projectFile);
                await ValidateProjectDependencies(projectFile, layerName, content);
            }
        }
    }

    private async Task ValidateProjectDependencies(string projectFile, string layerName, string content)
    {
        // Parse project references and validate they follow Clean Architecture rules
        var lines = content.Split('\n');
        foreach (var line in lines)
        {
            if (line.Trim().StartsWith("<ProjectReference"))
            {
                var referencedProject = ExtractProjectName(line);
                var referencedLayer = GetLayerNameFromProject(referencedProject);
                
                if (!string.IsNullOrEmpty(referencedLayer) && 
                    !_allowedDependencies[layerName].Contains(referencedLayer))
                {
                    _violations.Add(new ArchitectureViolation(
                        projectFile,
                        ViolationType.DependencyViolation,
                        $"{layerName} layer should not reference {referencedLayer} layer",
                        Array.IndexOf(lines, line) + 1
                    ));
                }
            }
        }
    }

    private string GetLayerName(string path)
    {
        var dirName = new DirectoryInfo(path).Name;
        if (dirName.EndsWith(".Domain")) return "Domain";
        if (dirName.EndsWith(".Application")) return "Application";
        if (dirName.EndsWith(".Infrastructure")) return "Infrastructure";
        if (dirName.EndsWith(".API")) return "API";
        return dirName;
    }

    private string GetLayerNameFromProject(string projectName)
    {
        if (projectName.Contains(".Domain")) return "Domain";
        if (projectName.Contains(".Application")) return "Application";
        if (projectName.Contains(".Infrastructure")) return "Infrastructure";
        if (projectName.Contains(".API")) return "API";
        return "";
    }

    private string ExtractProjectName(string line)
    {
        var start = line.IndexOf("Include=\"");
        if (start == -1) return "";
        start += 9;
        var end = line.IndexOf("\"", start);
        if (end == -1) return "";
        return Path.GetFileNameWithoutExtension(line.Substring(start, end - start));
    }

    private bool IsEntity(ClassDeclarationSyntax classNode)
    {
        // Check if class inherits from a base entity class or has entity patterns
        return classNode.BaseList?.Types.Any(t => t.Type.ToString().Contains("Entity")) == true ||
               classNode.Members.OfType<PropertyDeclarationSyntax>().Any(p => p.Identifier.ValueText == "Id");
    }

    private bool IsValueObject(ClassDeclarationSyntax classNode)
    {
        // Check if class is a value object pattern
        return classNode.BaseList?.Types.Any(t => t.Type.ToString().Contains("ValueObject")) == true ||
               classNode.Members.OfType<PropertyDeclarationSyntax>().All(p => 
                   !p.AccessorList?.Accessors.Any(a => a.IsKind(SyntaxKind.SetAccessorDeclaration)) == true);
    }

    private int CalculateMethodComplexity(MethodDeclarationSyntax method)
    {
        int complexity = 1;
        
        var decisionPoints = method.DescendantNodes().Where(node =>
            node.IsKind(SyntaxKind.IfStatement) ||
            node.IsKind(SyntaxKind.WhileStatement) ||
            node.IsKind(SyntaxKind.ForStatement) ||
            node.IsKind(SyntaxKind.ForEachStatement) ||
            node.IsKind(SyntaxKind.SwitchStatement) ||
            node.IsKind(SyntaxKind.CatchClause));

        complexity += decisionPoints.Count();
        return complexity;
    }

    private void PrintSummary()
    {
        Console.WriteLine();
        Console.WriteLine("🏛️  Architecture Validation Summary");
        Console.WriteLine("===================================");

        var dependencyViolations = _violations.Count(v => v.Type == ViolationType.DependencyViolation);
        var domainViolations = _violations.Count(v => v.Type == ViolationType.DomainPattern);
        var applicationViolations = _violations.Count(v => v.Type == ViolationType.ApplicationPattern);
        var infrastructureViolations = _violations.Count(v => v.Type == ViolationType.InfrastructurePattern);
        var apiViolations = _violations.Count(v => v.Type == ViolationType.ApiPattern);

        Console.WriteLine($"Dependency violations:     {dependencyViolations}");
        Console.WriteLine($"Domain pattern violations: {domainViolations}");
        Console.WriteLine($"Application violations:    {applicationViolations}");
        Console.WriteLine($"Infrastructure violations: {infrastructureViolations}");
        Console.WriteLine($"API violations:           {apiViolations}");
        Console.WriteLine($"Total violations:         {_violations.Count}");
        Console.WriteLine();

        if (_violations.Count == 0)
        {
            Console.WriteLine("✅ Architecture validation passed! Clean Architecture principles are being followed.");
            return;
        }

        if (_verbose)
        {
            Console.WriteLine("🚨 Architecture Violations:");
            foreach (var violation in _violations.OrderBy(v => v.Type).ThenBy(v => v.FilePath))
            {
                var typeIcon = violation.Type switch
                {
                    ViolationType.DependencyViolation => "🔗",
                    ViolationType.DomainPattern => "🏛️",
                    ViolationType.ApplicationPattern => "⚙️",
                    ViolationType.InfrastructurePattern => "🔧",
                    ViolationType.ApiPattern => "🌐",
                    _ => "⚠️"
                };

                var relativePath = Path.GetRelativePath(Directory.GetCurrentDirectory(), violation.FilePath);
                Console.WriteLine($"{typeIcon} {relativePath}:{violation.LineNumber}");
                Console.WriteLine($"   {violation.Description}");
                Console.WriteLine();
            }
        }

        // Exit with error code if violations found
        Environment.ExitCode = 1;
    }
}

/// <summary>
/// Represents an architecture violation
/// </summary>
public record ArchitectureViolation(
    string FilePath,
    ViolationType Type,
    string Description,
    int LineNumber
);

/// <summary>
/// Types of architecture violations
/// </summary>
public enum ViolationType
{
    DependencyViolation,
    DomainPattern,
    ApplicationPattern,
    InfrastructurePattern,
    ApiPattern
}