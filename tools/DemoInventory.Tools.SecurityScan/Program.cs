using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.CommandLine;
using System.Text.RegularExpressions;

namespace DemoInventory.Tools.SecurityScan;

/// <summary>
/// Security scanning tool for identifying potential security vulnerabilities
/// </summary>
public class Program
{
    public static async Task<int> Main(string[] args)
    {
        var pathOption = new Option<string>(
            name: "--path",
            description: "Path to scan for security issues (file or directory)",
            getDefaultValue: () => Directory.GetCurrentDirectory());

        var verboseOption = new Option<bool>(
            name: "--verbose",
            description: "Enable verbose output");

        var rootCommand = new RootCommand("Security Scanner for Code Vulnerability Detection")
        {
            pathOption,
            verboseOption
        };

        rootCommand.SetHandler(async (path, verbose) =>
        {
            var scanner = new SecurityScanner(verbose);
            await scanner.ScanAsync(path);
        }, pathOption, verboseOption);

        return await rootCommand.InvokeAsync(args);
    }
}

/// <summary>
/// Performs security scanning on C# code files
/// </summary>
public class SecurityScanner
{
    private readonly bool _verbose;
    private readonly List<SecurityFinding> _findings = new();

    // Security patterns to detect
    private readonly Dictionary<string, string> _securityPatterns = new()
    {
        { @"password\s*[=:]\s*[""'][^""']+[""']", "Hardcoded password detected" },
        { @"api[_-]?key\s*[=:]\s*[""'][^""']+[""']", "Hardcoded API key detected" },
        { @"secret\s*[=:]\s*[""'][^""']+[""']", "Hardcoded secret detected" },
        { @"token\s*[=:]\s*[""'][^""']+[""']", "Hardcoded token detected" },
        { @"connectionstring\s*[=:]\s*[""'][^""']+[""']", "Hardcoded connection string detected" },
        { @"SELECT\s+\*\s+FROM\s+\w+\s+WHERE\s+.*\+", "Potential SQL injection via string concatenation" },
        { @"exec\s*\(\s*[""'][^""']*\+", "Potential code injection via dynamic execution" },
        { @"Process\.Start\s*\([^)]*\+", "Potential command injection via Process.Start" },
        { @"HttpClient\s*\(\s*\)\s*\.", "HttpClient without proper disposal pattern" }
    };

    public SecurityScanner(bool verbose = false)
    {
        _verbose = verbose;
    }

    public async Task ScanAsync(string path)
    {
        Console.WriteLine($"🔒 Starting security scan on: {path}");
        Console.WriteLine();

        if (File.Exists(path) && path.EndsWith(".cs"))
        {
            await ScanFileAsync(path);
        }
        else if (Directory.Exists(path))
        {
            await ScanDirectoryAsync(path);
        }
        else
        {
            Console.WriteLine($"❌ Invalid path: {path}");
            return;
        }

        PrintSummary();
    }

    private async Task ScanDirectoryAsync(string directoryPath)
    {
        var csFiles = Directory.GetFiles(directoryPath, "*.cs", SearchOption.AllDirectories)
            .Where(file => !file.Contains("bin") && !file.Contains("obj"))
            .ToList();

        Console.WriteLine($"Found {csFiles.Count} C# files to scan");
        Console.WriteLine();

        foreach (var file in csFiles)
        {
            await ScanFileAsync(file);
        }
    }

    private async Task ScanFileAsync(string filePath)
    {
        if (_verbose)
            Console.WriteLine($"🔍 Scanning: {Path.GetRelativePath(Directory.GetCurrentDirectory(), filePath)}");

        var content = await File.ReadAllTextAsync(filePath);
        var tree = CSharpSyntaxTree.ParseText(content, path: filePath);
        var root = await tree.GetRootAsync();

        // Scan for hardcoded secrets and common security issues
        await ScanForHardcodedSecrets(filePath, content);

        // Analyze syntax tree for security patterns
        await AnalyzeSyntaxTree(filePath, root);

        // Check for specific security anti-patterns
        await CheckSecurityAntiPatterns(filePath, root);
    }

    private async Task ScanForHardcodedSecrets(string filePath, string content)
    {
        foreach (var pattern in _securityPatterns)
        {
            var regex = new Regex(pattern.Key, RegexOptions.IgnoreCase | RegexOptions.Multiline);
            var matches = regex.Matches(content);

            foreach (Match match in matches)
            {
                var lines = content.Substring(0, match.Index).Split('\n');
                var lineNumber = lines.Length;
                
                _findings.Add(new SecurityFinding(
                    filePath,
                    lineNumber,
                    SecurityLevel.High,
                    pattern.Value,
                    match.Value.Trim(),
                    "Hardcoded Secrets"
                ));
            }
        }
    }

    private async Task AnalyzeSyntaxTree(string filePath, SyntaxNode root)
    {
        // Check for SQL injection vulnerabilities
        await CheckSqlInjection(filePath, root);

        // Check for authentication/authorization issues
        await CheckAuthenticationIssues(filePath, root);

        // Check for input validation issues
        await CheckInputValidation(filePath, root);

        // Check for error handling that might expose sensitive info
        await CheckErrorHandling(filePath, root);

        // Check for logging security issues
        await CheckLoggingIssues(filePath, root);
    }

    private async Task CheckSqlInjection(string filePath, SyntaxNode root)
    {
        // Check for string interpolation or concatenation in SQL queries
        var stringLiterals = root.DescendantNodes().OfType<LiteralExpressionSyntax>()
            .Where(literal => literal.Token.IsKind(SyntaxKind.StringLiteralToken));

        foreach (var literal in stringLiterals)
        {
            var value = literal.Token.ValueText.ToLower();
            if (value.Contains("select") || value.Contains("insert") || value.Contains("update") || value.Contains("delete"))
            {
                // Check if this SQL string is used with string concatenation
                var parent = literal.Parent;
                while (parent != null)
                {
                    if (parent is BinaryExpressionSyntax binaryExpr && binaryExpr.OperatorToken.IsKind(SyntaxKind.PlusToken))
                    {
                        var lineNumber = literal.GetLocation().GetLineSpan().StartLinePosition.Line + 1;
                        _findings.Add(new SecurityFinding(
                            filePath,
                            lineNumber,
                            SecurityLevel.Critical,
                            "Potential SQL injection via string concatenation",
                            binaryExpr.ToString(),
                            "SQL Injection"
                        ));
                        break;
                    }
                    parent = parent.Parent;
                }
            }
        }

        // Check for interpolated strings with SQL
        var interpolatedStrings = root.DescendantNodes().OfType<InterpolatedStringExpressionSyntax>();
        foreach (var interpolated in interpolatedStrings)
        {
            var text = interpolated.ToString().ToLower();
            if (text.Contains("select") || text.Contains("insert") || text.Contains("update") || text.Contains("delete"))
            {
                var lineNumber = interpolated.GetLocation().GetLineSpan().StartLinePosition.Line + 1;
                _findings.Add(new SecurityFinding(
                    filePath,
                    lineNumber,
                    SecurityLevel.Critical,
                    "Potential SQL injection via string interpolation",
                    interpolated.ToString(),
                    "SQL Injection"
                ));
            }
        }
    }

    private async Task CheckAuthenticationIssues(string filePath, SyntaxNode root)
    {
        // Check for controllers without authorization attributes
        var controllers = root.DescendantNodes().OfType<ClassDeclarationSyntax>()
            .Where(c => c.Identifier.ValueText.EndsWith("Controller"));

        foreach (var controller in controllers)
        {
            var hasAuthorizeAttribute = controller.AttributeLists
                .SelectMany(list => list.Attributes)
                .Any(attr => attr.Name.ToString().Contains("Authorize"));

            if (!hasAuthorizeAttribute)
            {
                var lineNumber = controller.GetLocation().GetLineSpan().StartLinePosition.Line + 1;
                _findings.Add(new SecurityFinding(
                    filePath,
                    lineNumber,
                    SecurityLevel.Medium,
                    $"Controller '{controller.Identifier.ValueText}' lacks authorization attributes",
                    controller.Identifier.ValueText,
                    "Authorization"
                ));
            }
        }

        // Check for action methods without authorization
        var actionMethods = root.DescendantNodes().OfType<MethodDeclarationSyntax>()
            .Where(m => m.AttributeLists.SelectMany(al => al.Attributes)
                .Any(attr => attr.Name.ToString().Contains("Http")));

        foreach (var method in actionMethods)
        {
            var hasAuthorizeAttribute = method.AttributeLists
                .SelectMany(list => list.Attributes)
                .Any(attr => attr.Name.ToString().Contains("Authorize") || attr.Name.ToString().Contains("AllowAnonymous"));

            if (!hasAuthorizeAttribute)
            {
                var lineNumber = method.GetLocation().GetLineSpan().StartLinePosition.Line + 1;
                _findings.Add(new SecurityFinding(
                    filePath,
                    lineNumber,
                    SecurityLevel.Medium,
                    $"Action method '{method.Identifier.ValueText}' lacks authorization attributes",
                    method.Identifier.ValueText,
                    "Authorization"
                ));
            }
        }
    }

    private async Task CheckInputValidation(string filePath, SyntaxNode root)
    {
        // Check for parameters without validation attributes
        var actionMethods = root.DescendantNodes().OfType<MethodDeclarationSyntax>()
            .Where(m => m.AttributeLists.SelectMany(al => al.Attributes)
                .Any(attr => attr.Name.ToString().Contains("Http")));

        foreach (var method in actionMethods)
        {
            foreach (var parameter in method.ParameterList.Parameters)
            {
                var hasValidationAttribute = parameter.AttributeLists
                    .SelectMany(list => list.Attributes)
                    .Any(attr => attr.Name.ToString().Contains("Required") || 
                                attr.Name.ToString().Contains("Range") ||
                                attr.Name.ToString().Contains("StringLength") ||
                                attr.Name.ToString().Contains("RegularExpression"));

                var parameterType = parameter.Type?.ToString() ?? "";
                if (!hasValidationAttribute && 
                    (parameterType.Contains("string") || parameterType.Contains("int") || parameterType.Contains("decimal")))
                {
                    var lineNumber = parameter.GetLocation().GetLineSpan().StartLinePosition.Line + 1;
                    _findings.Add(new SecurityFinding(
                        filePath,
                        lineNumber,
                        SecurityLevel.Medium,
                        $"Parameter '{parameter.Identifier.ValueText}' lacks input validation attributes",
                        parameter.ToString(),
                        "Input Validation"
                    ));
                }
            }
        }
    }

    private async Task CheckErrorHandling(string filePath, SyntaxNode root)
    {
        // Check for catch blocks that might expose sensitive information
        var catchClauses = root.DescendantNodes().OfType<CatchClauseSyntax>();

        foreach (var catchClause in catchClauses)
        {
            var returnStatements = catchClause.Block?.DescendantNodes().OfType<ReturnStatementSyntax>();
            if (returnStatements != null)
            {
                foreach (var returnStatement in returnStatements)
                {
                    var returnText = returnStatement.ToString().ToLower();
                    if (returnText.Contains("exception") || returnText.Contains("stacktrace") || returnText.Contains("innerexception"))
                    {
                        var lineNumber = returnStatement.GetLocation().GetLineSpan().StartLinePosition.Line + 1;
                        _findings.Add(new SecurityFinding(
                            filePath,
                            lineNumber,
                            SecurityLevel.Medium,
                            "Potential information disclosure in error handling",
                            returnStatement.ToString(),
                            "Error Handling"
                        ));
                    }
                }
            }
        }
    }

    private async Task CheckLoggingIssues(string filePath, SyntaxNode root)
    {
        // Check for logging statements that might log sensitive data
        var invocations = root.DescendantNodes().OfType<InvocationExpressionSyntax>()
            .Where(inv => inv.Expression.ToString().Contains("Log"));

        foreach (var invocation in invocations)
        {
            var arguments = invocation.ArgumentList.Arguments;
            foreach (var arg in arguments)
            {
                var argText = arg.ToString().ToLower();
                if (argText.Contains("password") || argText.Contains("token") || argText.Contains("secret") || argText.Contains("key"))
                {
                    var lineNumber = invocation.GetLocation().GetLineSpan().StartLinePosition.Line + 1;
                    _findings.Add(new SecurityFinding(
                        filePath,
                        lineNumber,
                        SecurityLevel.High,
                        "Potential sensitive data in logging statement",
                        invocation.ToString(),
                        "Logging Security"
                    ));
                }
            }
        }
    }

    private async Task CheckSecurityAntiPatterns(string filePath, SyntaxNode root)
    {
        // Check for HttpClient instantiation without using IHttpClientFactory
        var objectCreations = root.DescendantNodes().OfType<ObjectCreationExpressionSyntax>()
            .Where(obj => obj.Type.ToString().Contains("HttpClient"));

        foreach (var creation in objectCreations)
        {
            var lineNumber = creation.GetLocation().GetLineSpan().StartLinePosition.Line + 1;
            _findings.Add(new SecurityFinding(
                filePath,
                lineNumber,
                SecurityLevel.Low,
                "HttpClient should be created using IHttpClientFactory to avoid socket exhaustion",
                creation.ToString(),
                "Resource Management"
            ));
        }

        // Check for DateTime.Now usage (should use UTC or injected time service)
        var memberAccess = root.DescendantNodes().OfType<MemberAccessExpressionSyntax>()
            .Where(ma => ma.ToString() == "DateTime.Now");

        foreach (var access in memberAccess)
        {
            var lineNumber = access.GetLocation().GetLineSpan().StartLinePosition.Line + 1;
            _findings.Add(new SecurityFinding(
                filePath,
                lineNumber,
                SecurityLevel.Low,
                "Use DateTime.UtcNow instead of DateTime.Now for consistency",
                access.ToString(),
                "Best Practices"
            ));
        }
    }

    private void PrintSummary()
    {
        Console.WriteLine();
        Console.WriteLine("🔒 Security Scan Summary");
        Console.WriteLine("========================");

        var criticalFindings = _findings.Count(f => f.Level == SecurityLevel.Critical);
        var highFindings = _findings.Count(f => f.Level == SecurityLevel.High);
        var mediumFindings = _findings.Count(f => f.Level == SecurityLevel.Medium);
        var lowFindings = _findings.Count(f => f.Level == SecurityLevel.Low);

        Console.WriteLine($"Critical: {criticalFindings}");
        Console.WriteLine($"High:     {highFindings}");
        Console.WriteLine($"Medium:   {mediumFindings}");
        Console.WriteLine($"Low:      {lowFindings}");
        Console.WriteLine($"Total:    {_findings.Count}");
        Console.WriteLine();

        if (_findings.Count == 0)
        {
            Console.WriteLine("✅ No security issues found!");
            return;
        }

        // Group findings by category
        var findingsByCategory = _findings.GroupBy(f => f.Category)
            .OrderByDescending(g => g.Count());

        Console.WriteLine("📋 Issues by category:");
        foreach (var group in findingsByCategory)
        {
            Console.WriteLine($"  {group.Key}: {group.Count()}");
        }

        Console.WriteLine();

        // Show detailed findings if verbose or if critical/high findings exist
        if (_verbose || criticalFindings > 0 || highFindings > 0)
        {
            Console.WriteLine("🚨 Security Findings:");
            foreach (var finding in _findings.OrderByDescending(f => f.Level))
            {
                var levelIcon = finding.Level switch
                {
                    SecurityLevel.Critical => "🔴",
                    SecurityLevel.High => "🟠",
                    SecurityLevel.Medium => "🟡",
                    SecurityLevel.Low => "🔵",
                    _ => "⚪"
                };

                var relativePath = Path.GetRelativePath(Directory.GetCurrentDirectory(), finding.FilePath);
                Console.WriteLine($"{levelIcon} {finding.Level} - {relativePath}:{finding.LineNumber}");
                Console.WriteLine($"   {finding.Description}");
                if (_verbose)
                {
                    Console.WriteLine($"   Code: {finding.CodeSnippet}");
                }
                Console.WriteLine();
            }
        }

        // Exit with error code if critical or high findings exist
        if (criticalFindings > 0 || highFindings > 0)
        {
            Environment.ExitCode = 1;
        }
    }
}

/// <summary>
/// Represents a security finding
/// </summary>
public record SecurityFinding(
    string FilePath,
    int LineNumber,
    SecurityLevel Level,
    string Description,
    string CodeSnippet,
    string Category
);

/// <summary>
/// Security finding severity levels
/// </summary>
public enum SecurityLevel
{
    Low,
    Medium,
    High,
    Critical
}