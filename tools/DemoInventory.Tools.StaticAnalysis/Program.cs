using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.CommandLine;

namespace DemoInventory.Tools.StaticAnalysis;

/// <summary>
/// Static analysis tool for validating code quality based on the code-verification.md guidelines
/// </summary>
public class Program
{
    public static async Task<int> Main(string[] args)
    {
        var pathOption = new Option<string>(
            name: "--path",
            description: "Path to analyze (file or directory)",
            getDefaultValue: () => Directory.GetCurrentDirectory());

        var verboseOption = new Option<bool>(
            name: "--verbose",
            description: "Enable verbose output");

        var rootCommand = new RootCommand("Static Analysis Tool for Code Quality Validation")
        {
            pathOption,
            verboseOption
        };

        rootCommand.SetHandler(async (path, verbose) =>
        {
            var analyzer = new StaticAnalyzer(verbose);
            await analyzer.AnalyzeAsync(path);
        }, pathOption, verboseOption);

        return await rootCommand.InvokeAsync(args);
    }
}

/// <summary>
/// Performs static analysis on C# code files
/// </summary>
public class StaticAnalyzer
{
    private readonly bool _verbose;
    private readonly List<AnalysisResult> _results = new();

    public StaticAnalyzer(bool verbose = false)
    {
        _verbose = verbose;
    }

    public async Task AnalyzeAsync(string path)
    {
        Console.WriteLine($"🔍 Starting static analysis on: {path}");
        Console.WriteLine();

        if (File.Exists(path) && path.EndsWith(".cs"))
        {
            await AnalyzeFileAsync(path);
        }
        else if (Directory.Exists(path))
        {
            await AnalyzeDirectoryAsync(path);
        }
        else
        {
            Console.WriteLine($"❌ Invalid path: {path}");
            return;
        }

        PrintSummary();
    }

    private async Task AnalyzeDirectoryAsync(string directoryPath)
    {
        var csFiles = Directory.GetFiles(directoryPath, "*.cs", SearchOption.AllDirectories)
            .Where(file => !file.Contains("bin") && !file.Contains("obj"))
            .ToList();

        Console.WriteLine($"Found {csFiles.Count} C# files to analyze");
        Console.WriteLine();

        foreach (var file in csFiles)
        {
            await AnalyzeFileAsync(file);
        }
    }

    private async Task AnalyzeFileAsync(string filePath)
    {
        if (_verbose)
            Console.WriteLine($"📄 Analyzing: {Path.GetRelativePath(Directory.GetCurrentDirectory(), filePath)}");

        var content = await File.ReadAllTextAsync(filePath);
        var tree = CSharpSyntaxTree.ParseText(content, path: filePath);
        var root = await tree.GetRootAsync();

        var fileResult = new FileAnalysisResult(filePath);

        // Analyze classes
        var classes = root.DescendantNodes().OfType<ClassDeclarationSyntax>();
        foreach (var classNode in classes)
        {
            AnalyzeClass(classNode, fileResult);
        }

        // Analyze methods
        var methods = root.DescendantNodes().OfType<MethodDeclarationSyntax>();
        foreach (var method in methods)
        {
            AnalyzeMethod(method, fileResult);
        }

        // Analyze naming conventions
        AnalyzeNamingConventions(root, fileResult);

        _results.Add(fileResult);

        if (_verbose && fileResult.Issues.Any())
        {
            Console.WriteLine($"  ⚠️  Found {fileResult.Issues.Count} issues");
        }
    }

    private void AnalyzeClass(ClassDeclarationSyntax classNode, FileAnalysisResult result)
    {
        var className = classNode.Identifier.ValueText;

        // Check Single Responsibility Principle
        var methods = classNode.Members.OfType<MethodDeclarationSyntax>().ToList();
        if (methods.Count > 20)
        {
            result.AddIssue($"Class '{className}' has {methods.Count} methods. Consider breaking it down (SRP violation)", 
                classNode.GetLocation());
        }

        // Check class naming convention
        if (!char.IsUpper(className[0]))
        {
            result.AddIssue($"Class '{className}' should start with uppercase letter", 
                classNode.Identifier.GetLocation());
        }

        // Check for proper documentation
        if (!HasXmlDocumentation(classNode))
        {
            result.AddIssue($"Class '{className}' is missing XML documentation", 
                classNode.GetLocation());
        }
    }

    private void AnalyzeMethod(MethodDeclarationSyntax method, FileAnalysisResult result)
    {
        var methodName = method.Identifier.ValueText;

        // Check cyclomatic complexity
        var complexity = CalculateCyclomaticComplexity(method);
        if (complexity > 10)
        {
            result.AddIssue($"Method '{methodName}' has high cyclomatic complexity: {complexity} (should be ≤ 10)", 
                method.GetLocation());
        }

        // Check method length
        var lineCount = method.GetText().Lines.Count;
        if (lineCount > 50)
        {
            result.AddIssue($"Method '{methodName}' is too long: {lineCount} lines (should be ≤ 50)", 
                method.GetLocation());
        }

        // Check parameter count
        var parameterCount = method.ParameterList.Parameters.Count;
        if (parameterCount > 7)
        {
            result.AddIssue($"Method '{methodName}' has too many parameters: {parameterCount} (should be ≤ 7)", 
                method.GetLocation());
        }

        // Check nesting depth
        var nestingDepth = CalculateNestingDepth(method);
        if (nestingDepth > 3)
        {
            result.AddIssue($"Method '{methodName}' has excessive nesting depth: {nestingDepth} (should be ≤ 3)", 
                method.GetLocation());
        }

        // Check method naming convention
        if (!char.IsUpper(methodName[0]))
        {
            result.AddIssue($"Method '{methodName}' should start with uppercase letter", 
                method.Identifier.GetLocation());
        }

        // Check for proper documentation on public methods
        if (method.Modifiers.Any(m => m.IsKind(SyntaxKind.PublicKeyword)) && !HasXmlDocumentation(method))
        {
            result.AddIssue($"Public method '{methodName}' is missing XML documentation", 
                method.GetLocation());
        }
    }

    private void AnalyzeNamingConventions(SyntaxNode root, FileAnalysisResult result)
    {
        // Check field naming (should be camelCase with _prefix for private fields)
        var fields = root.DescendantNodes().OfType<FieldDeclarationSyntax>();
        foreach (var field in fields)
        {
            foreach (var variable in field.Declaration.Variables)
            {
                var fieldName = variable.Identifier.ValueText;
                if (field.Modifiers.Any(m => m.IsKind(SyntaxKind.PrivateKeyword)) || 
                    !field.Modifiers.Any(m => m.IsKind(SyntaxKind.PublicKeyword)))
                {
                    if (!fieldName.StartsWith("_"))
                    {
                        result.AddIssue($"Private field '{fieldName}' should start with underscore", 
                            variable.GetLocation());
                    }
                }
            }
        }

        // Check property naming (should be PascalCase)
        var properties = root.DescendantNodes().OfType<PropertyDeclarationSyntax>();
        foreach (var property in properties)
        {
            var propertyName = property.Identifier.ValueText;
            if (!char.IsUpper(propertyName[0]))
            {
                result.AddIssue($"Property '{propertyName}' should start with uppercase letter", 
                    property.Identifier.GetLocation());
            }
        }
    }

    private int CalculateCyclomaticComplexity(MethodDeclarationSyntax method)
    {
        int complexity = 1; // Base complexity

        var decisionPoints = method.DescendantNodes().Where(node =>
            node.IsKind(SyntaxKind.IfStatement) ||
            node.IsKind(SyntaxKind.WhileStatement) ||
            node.IsKind(SyntaxKind.ForStatement) ||
            node.IsKind(SyntaxKind.ForEachStatement) ||
            node.IsKind(SyntaxKind.SwitchStatement) ||
            node.IsKind(SyntaxKind.CatchClause) ||
            node.IsKind(SyntaxKind.ConditionalExpression));

        complexity += decisionPoints.Count();

        return complexity;
    }

    private int CalculateNestingDepth(MethodDeclarationSyntax method)
    {
        return CalculateNestingDepthRecursive(method.Body, 0);
    }

    private int CalculateNestingDepthRecursive(SyntaxNode? node, int currentDepth)
    {
        if (node == null) return currentDepth;

        int maxDepth = currentDepth;

        foreach (var child in node.ChildNodes())
        {
            int childDepth = currentDepth;
            
            if (child.IsKind(SyntaxKind.IfStatement) ||
                child.IsKind(SyntaxKind.WhileStatement) ||
                child.IsKind(SyntaxKind.ForStatement) ||
                child.IsKind(SyntaxKind.ForEachStatement) ||
                child.IsKind(SyntaxKind.SwitchStatement) ||
                child.IsKind(SyntaxKind.TryStatement))
            {
                childDepth++;
            }

            int depth = CalculateNestingDepthRecursive(child, childDepth);
            maxDepth = Math.Max(maxDepth, depth);
        }

        return maxDepth;
    }

    private bool HasXmlDocumentation(SyntaxNode node)
    {
        var documentationComment = node.GetLeadingTrivia()
            .FirstOrDefault(t => t.IsKind(SyntaxKind.SingleLineDocumentationCommentTrivia) ||
                                t.IsKind(SyntaxKind.MultiLineDocumentationCommentTrivia));
        
        return !documentationComment.IsKind(SyntaxKind.None);
    }

    private void PrintSummary()
    {
        Console.WriteLine();
        Console.WriteLine("📊 Analysis Summary");
        Console.WriteLine("==================");

        var totalIssues = _results.OfType<FileAnalysisResult>().Sum(r => r.Issues.Count);
        var filesWithIssues = _results.OfType<FileAnalysisResult>().Count(r => r.Issues.Any());

        Console.WriteLine($"Files analyzed: {_results.Count}");
        Console.WriteLine($"Files with issues: {filesWithIssues}");
        Console.WriteLine($"Total issues found: {totalIssues}");
        Console.WriteLine();

        if (totalIssues == 0)
        {
            Console.WriteLine("✅ No issues found! Code quality looks good.");
            return;
        }

        // Group issues by severity
        var allIssues = _results.OfType<FileAnalysisResult>().SelectMany(r => r.Issues).ToList();
        var issuesByType = allIssues.GroupBy(i => GetIssueType(i.Message))
            .OrderByDescending(g => g.Count());

        Console.WriteLine("📋 Issues by category:");
        foreach (var group in issuesByType)
        {
            Console.WriteLine($"  {group.Key}: {group.Count()}");
        }

        Console.WriteLine();

        // Show detailed issues if verbose
        if (_verbose)
        {
            Console.WriteLine("🔍 Detailed Issues:");
            foreach (var result in _results.OfType<FileAnalysisResult>().Where(r => r.Issues.Any()))
            {
                Console.WriteLine($"📄 {Path.GetRelativePath(Directory.GetCurrentDirectory(), result.FilePath)}");
                foreach (var issue in result.Issues)
                {
                    var location = issue.Location;
                    var line = location.GetLineSpan().StartLinePosition.Line + 1;
                    Console.WriteLine($"  ⚠️  Line {line}: {issue.Message}");
                }
                Console.WriteLine();
            }
        }

        // Exit with error code if issues found
        Environment.ExitCode = 1;
    }

    private string GetIssueType(string message)
    {
        if (message.Contains("complexity")) return "Complexity";
        if (message.Contains("naming") || message.Contains("uppercase") || message.Contains("underscore")) return "Naming";
        if (message.Contains("documentation")) return "Documentation";
        if (message.Contains("parameters")) return "Parameters";
        if (message.Contains("nesting")) return "Nesting";
        if (message.Contains("length") || message.Contains("long")) return "Length";
        if (message.Contains("SRP")) return "SOLID Principles";
        return "Other";
    }
}

/// <summary>
/// Represents analysis results for a file
/// </summary>
public class FileAnalysisResult : AnalysisResult
{
    public string FilePath { get; }
    public List<CodeIssue> Issues { get; } = new();

    public FileAnalysisResult(string filePath)
    {
        FilePath = filePath;
    }

    public void AddIssue(string message, Location location)
    {
        Issues.Add(new CodeIssue(message, location));
    }
}

/// <summary>
/// Base class for analysis results
/// </summary>
public abstract class AnalysisResult
{
}

/// <summary>
/// Represents a code quality issue
/// </summary>
public record CodeIssue(string Message, Location Location);