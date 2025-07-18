using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.CommandLine;

namespace CodeValidator.StaticAnalysis;

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
                classNode.GetLocation(), IssueSeverity.High);
        }
        else if (methods.Count > 15)
        {
            result.AddIssue($"Class '{className}' has {methods.Count} methods. Consider refactoring for better maintainability", 
                classNode.GetLocation(), IssueSeverity.Medium);
        }

        // Check class naming convention
        if (!char.IsUpper(className[0]))
        {
            result.AddIssue($"Class '{className}' should start with uppercase letter", 
                classNode.Identifier.GetLocation(), IssueSeverity.Low);
        }

        // Check for proper documentation
        if (!HasXmlDocumentation(classNode))
        {
            var severity = classNode.Modifiers.Any(m => m.IsKind(SyntaxKind.PublicKeyword)) 
                ? IssueSeverity.Medium 
                : IssueSeverity.Low;
            result.AddIssue($"Class '{className}' is missing XML documentation", 
                classNode.GetLocation(), severity);
        }
    }

    private void AnalyzeMethod(MethodDeclarationSyntax method, FileAnalysisResult result)
    {
        var methodName = method.Identifier.ValueText;

        // Check cyclomatic complexity
        var complexity = CalculateCyclomaticComplexity(method);
        if (complexity > 15)
        {
            result.AddIssue($"Method '{methodName}' has very high cyclomatic complexity: {complexity} (should be ≤ 10)", 
                method.GetLocation(), IssueSeverity.High);
        }
        else if (complexity > 10)
        {
            result.AddIssue($"Method '{methodName}' has high cyclomatic complexity: {complexity} (should be ≤ 10)", 
                method.GetLocation(), IssueSeverity.Medium);
        }
        else if (complexity > 7)
        {
            result.AddIssue($"Method '{methodName}' has elevated cyclomatic complexity: {complexity} (consider simplifying)", 
                method.GetLocation(), IssueSeverity.Low);
        }

        // Check method length
        var lineCount = method.GetText().Lines.Count;
        if (lineCount > 100)
        {
            result.AddIssue($"Method '{methodName}' is very long: {lineCount} lines (should be ≤ 50)", 
                method.GetLocation(), IssueSeverity.High);
        }
        else if (lineCount > 50)
        {
            result.AddIssue($"Method '{methodName}' is too long: {lineCount} lines (should be ≤ 50)", 
                method.GetLocation(), IssueSeverity.Medium);
        }
        else if (lineCount > 30)
        {
            result.AddIssue($"Method '{methodName}' is getting long: {lineCount} lines (consider breaking down)", 
                method.GetLocation(), IssueSeverity.Low);
        }

        // Check parameter count
        var parameterCount = method.ParameterList.Parameters.Count;
        if (parameterCount > 10)
        {
            result.AddIssue($"Method '{methodName}' has too many parameters: {parameterCount} (should be ≤ 7)", 
                method.GetLocation(), IssueSeverity.High);
        }
        else if (parameterCount > 7)
        {
            result.AddIssue($"Method '{methodName}' has many parameters: {parameterCount} (should be ≤ 7)", 
                method.GetLocation(), IssueSeverity.Medium);
        }
        else if (parameterCount > 5)
        {
            result.AddIssue($"Method '{methodName}' has several parameters: {parameterCount} (consider parameter object)", 
                method.GetLocation(), IssueSeverity.Low);
        }

        // Check nesting depth
        var nestingDepth = CalculateNestingDepth(method);
        if (nestingDepth > 5)
        {
            result.AddIssue($"Method '{methodName}' has excessive nesting depth: {nestingDepth} (should be ≤ 3)", 
                method.GetLocation(), IssueSeverity.High);
        }
        else if (nestingDepth > 3)
        {
            result.AddIssue($"Method '{methodName}' has high nesting depth: {nestingDepth} (should be ≤ 3)", 
                method.GetLocation(), IssueSeverity.Medium);
        }

        // Check method naming convention
        if (!char.IsUpper(methodName[0]))
        {
            result.AddIssue($"Method '{methodName}' should start with uppercase letter", 
                method.Identifier.GetLocation(), IssueSeverity.Low);
        }

        // Check for proper documentation on public methods
        if (method.Modifiers.Any(m => m.IsKind(SyntaxKind.PublicKeyword)) && !HasXmlDocumentation(method))
        {
            result.AddIssue($"Public method '{methodName}' is missing XML documentation", 
                method.GetLocation(), IssueSeverity.Medium);
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
                            variable.GetLocation(), IssueSeverity.Low);
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
                    property.Identifier.GetLocation(), IssueSeverity.Low);
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

        if (totalIssues == 0)
        {
            Console.WriteLine("✅ No issues found! Code quality looks good.");
            return;
        }

        // Show severity breakdown
        var allIssues = _results.OfType<FileAnalysisResult>().SelectMany(r => r.Issues).ToList();
        var criticalCount = allIssues.Count(i => i.Severity == IssueSeverity.Critical);
        var highCount = allIssues.Count(i => i.Severity == IssueSeverity.High);
        var mediumCount = allIssues.Count(i => i.Severity == IssueSeverity.Medium);
        var lowCount = allIssues.Count(i => i.Severity == IssueSeverity.Low);

        Console.WriteLine();
        Console.WriteLine("📊 Issues by severity:");
        Console.WriteLine($"  Critical: {criticalCount}");
        Console.WriteLine($"  High:     {highCount}");
        Console.WriteLine($"  Medium:   {mediumCount}");
        Console.WriteLine($"  Low:      {lowCount}");
        Console.WriteLine();

        // Group issues by category
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
                foreach (var issue in result.Issues.OrderByDescending(i => i.Severity))
                {
                    var location = issue.Location;
                    var line = location.GetLineSpan().StartLinePosition.Line + 1;
                    var severityIcon = issue.Severity switch
                    {
                        IssueSeverity.Critical => "🔴",
                        IssueSeverity.High => "🟠",
                        IssueSeverity.Medium => "🟡",
                        IssueSeverity.Low => "🔵",
                        _ => "⚪"
                    };
                    Console.WriteLine($"  {severityIcon} {issue.Severity} - Line {line}: {issue.Message}");
                }
                Console.WriteLine();
            }
        }

        // Show severity-specific recommendations
        ShowSeverityRecommendations(allIssues);

        // Exit with error code if critical or high issues found
        if (criticalCount > 0 || highCount > 0)
        {
            Environment.ExitCode = 1;
        }
    }

    private void ShowSeverityRecommendations(List<CodeIssue> allIssues)
    {
        Console.WriteLine("💡 Severity-Specific Recommendations");
        Console.WriteLine("====================================");

        var criticalIssues = allIssues.Where(i => i.Severity == IssueSeverity.Critical).ToList();
        var highIssues = allIssues.Where(i => i.Severity == IssueSeverity.High).ToList();
        var mediumIssues = allIssues.Where(i => i.Severity == IssueSeverity.Medium).ToList();
        var lowIssues = allIssues.Where(i => i.Severity == IssueSeverity.Low).ToList();

        if (criticalIssues.Any())
        {
            Console.WriteLine("🔴 CRITICAL Issues (Immediate Action Required):");
            ShowRecommendationsForSeverity(criticalIssues, IssueSeverity.Critical);
            Console.WriteLine();
        }

        if (highIssues.Any())
        {
            Console.WriteLine("🟠 HIGH Priority Issues (Address Soon):");
            ShowRecommendationsForSeverity(highIssues, IssueSeverity.High);
            Console.WriteLine();
        }

        if (mediumIssues.Any())
        {
            Console.WriteLine("🟡 MEDIUM Priority Issues (Plan for Next Sprint):");
            ShowRecommendationsForSeverity(mediumIssues, IssueSeverity.Medium);
            Console.WriteLine();
        }

        if (lowIssues.Any())
        {
            Console.WriteLine("🔵 LOW Priority Issues (Code Hygiene):");
            ShowRecommendationsForSeverity(lowIssues, IssueSeverity.Low);
            Console.WriteLine();
        }

        if (!allIssues.Any())
        {
            Console.WriteLine("🎉 Excellent! No static analysis issues found.");
            Console.WriteLine("  • Continue following coding standards");
            Console.WriteLine("  • Maintain current code quality practices");
            Console.WriteLine("  • Consider code reviews for new changes");
        }
    }

    private void ShowRecommendationsForSeverity(List<CodeIssue> issues, IssueSeverity severity)
    {
        var recommendations = GetRecommendationsForSeverity(issues, severity);
        
        foreach (var recommendation in recommendations.Take(5)) // Limit to top 5 recommendations
        {
            Console.WriteLine($"  • {recommendation}");
        }

        if (recommendations.Count > 5)
        {
            Console.WriteLine($"  ... and {recommendations.Count - 5} more recommendations");
        }
    }

    private List<string> GetRecommendationsForSeverity(List<CodeIssue> issues, IssueSeverity severity)
    {
        var recommendations = new List<string>();
        var issueGroups = issues.GroupBy(i => GetIssueType(i.Message));

        foreach (var group in issueGroups.OrderByDescending(g => g.Count()))
        {
            var category = group.Key;
            var count = group.Count();

            switch (severity)
            {
                case IssueSeverity.Critical:
                    recommendations.AddRange(GetCriticalRecommendations(category, count));
                    break;
                case IssueSeverity.High:
                    recommendations.AddRange(GetHighRecommendations(category, count));
                    break;
                case IssueSeverity.Medium:
                    recommendations.AddRange(GetMediumRecommendations(category, count));
                    break;
                case IssueSeverity.Low:
                    recommendations.AddRange(GetLowRecommendations(category, count));
                    break;
            }
        }

        return recommendations.Distinct().ToList();
    }

    private List<string> GetCriticalRecommendations(string category, int count)
    {
        return category.ToLower() switch
        {
            "complexity" => new List<string> 
            { 
                $"URGENT: Refactor {count} extremely complex methods immediately to prevent maintenance disasters",
                "Break down complex methods using Extract Method pattern",
                "Consider using Strategy or Command patterns for complex business logic"
            },
            "solid principles" => new List<string> 
            { 
                $"URGENT: {count} severe SOLID violations detected - architectural review required",
                "Break down large classes into focused, single-responsibility components",
                "Schedule immediate architectural refactoring session"
            },
            _ => new List<string> { $"URGENT: Address {count} critical {category.ToLower()} issues immediately" }
        };
    }

    private List<string> GetHighRecommendations(string category, int count)
    {
        return category.ToLower() switch
        {
            "complexity" => new List<string> 
            { 
                $"HIGH PRIORITY: Simplify {count} complex methods this sprint",
                "Use early returns to reduce nesting depth",
                "Extract complex conditions into well-named boolean methods"
            },
            "length" => new List<string> 
            { 
                $"HIGH PRIORITY: Break down {count} long methods/classes",
                "Apply Single Responsibility Principle",
                "Extract related functionality into separate methods or classes"
            },
            "parameters" => new List<string> 
            { 
                $"HIGH PRIORITY: Reduce parameter count in {count} methods",
                "Create parameter objects for related parameters",
                "Use dependency injection for service dependencies"
            },
            "nesting" => new List<string> 
            { 
                $"HIGH PRIORITY: Reduce nesting in {count} methods",
                "Use guard clauses and early returns",
                "Extract nested logic into separate methods"
            },
            _ => new List<string> { $"HIGH PRIORITY: Address {count} {category.ToLower()} issues soon" }
        };
    }

    private List<string> GetMediumRecommendations(string category, int count)
    {
        return category.ToLower() switch
        {
            "documentation" => new List<string> 
            { 
                $"Add XML documentation to {count} public APIs for better maintainability",
                "Document public methods, classes, and properties",
                "Include parameter descriptions and return value information"
            },
            "complexity" => new List<string> 
            { 
                $"Consider simplifying {count} moderately complex methods",
                "Review business logic and extract reusable components"
            },
            "naming" => new List<string> 
            { 
                $"Improve naming conventions for {count} items",
                "Follow PascalCase for public members, camelCase for private fields"
            },
            _ => new List<string> { $"Plan to address {count} {category.ToLower()} issues in upcoming sprints" }
        };
    }

    private List<string> GetLowRecommendations(string category, int count)
    {
        return category.ToLower() switch
        {
            "naming" => new List<string> 
            { 
                $"Polish naming conventions for {count} items during code reviews",
                "Ensure consistent naming patterns across the codebase"
            },
            "documentation" => new List<string> 
            { 
                $"Add documentation to {count} internal components when time permits",
                "Document complex business logic for future maintainers"
            },
            "complexity" => new List<string> 
            { 
                $"Monitor {count} methods that are approaching complexity limits",
                "Keep an eye on these methods during future changes"
            },
            _ => new List<string> { $"Address {count} minor {category.ToLower()} issues during regular maintenance" }
        };
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

    public void AddIssue(string message, Location location, IssueSeverity severity = IssueSeverity.Medium)
    {
        Issues.Add(new CodeIssue(message, location, severity));
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
public record CodeIssue(string Message, Location Location, IssueSeverity Severity);

/// <summary>
/// Severity levels for static analysis issues
/// </summary>
public enum IssueSeverity
{
    Low,
    Medium,
    High,
    Critical
}