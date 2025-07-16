using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.CommandLine;
using System.Text.RegularExpressions;
using System.Text.Json;

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

        var ctrfOption = new Option<string?>(
            name: "--ctrf",
            description: "Generate CTRF (Common Test Report Format) JSON report file");

        var rootCommand = new RootCommand("Security Scanner for Code Vulnerability Detection")
        {
            pathOption,
            verboseOption,
            ctrfOption
        };

        rootCommand.SetHandler(async (path, verbose, ctrf) =>
        {
            var scanner = new SecurityScanner(verbose);
            await scanner.ScanAsync(path, ctrf);
        }, pathOption, verboseOption, ctrfOption);

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

    // Security patterns to detect (OWASP Top 10 & CWE Top 25)
    private readonly Dictionary<string, string> _securityPatterns = new()
    {
        // OWASP A02:2021 - Cryptographic Failures / CWE-798: Hardcoded Credentials
        { @"password\s*[=:]\s*[""'][^""']+[""']", "Hardcoded password detected (OWASP A02, CWE-798)" },
        { @"api[_-]?key\s*[=:]\s*[""'][^""']+[""']", "Hardcoded API key detected (OWASP A02, CWE-798)" },
        { @"secret\s*[=:]\s*[""'][^""']+[""']", "Hardcoded secret detected (OWASP A02, CWE-798)" },
        { @"token\s*[=:]\s*[""'][^""']+[""']", "Hardcoded token detected (OWASP A02, CWE-798)" },
        { @"connectionstring\s*[=:]\s*[""'][^""']+[""']", "Hardcoded connection string detected (OWASP A02, CWE-798)" },
        
        // OWASP A03:2021 - Injection / CWE-89: SQL Injection
        { @"SELECT\s+\*\s+FROM\s+\w+\s+WHERE\s+.*\+", "Potential SQL injection via string concatenation (OWASP A03, CWE-89)" },
        
        // CWE-94: Code Injection
        { @"exec\s*\(\s*[""'][^""']*\+", "Potential code injection via dynamic execution (CWE-94)" },
        
        // CWE-78: OS Command Injection
        { @"Process\.Start\s*\([^)]*\+", "Potential command injection via Process.Start (OWASP A03, CWE-78)" },
        { @"cmd\.exe|powershell\.exe|bash|sh\s", "Direct OS command execution detected (CWE-78)" },
        
        // OWASP A06:2021 - Vulnerable Components
        { @"HttpClient\s*\(\s*\)\s*\.", "HttpClient without proper disposal pattern (OWASP A06)" },
        
        // CWE-22: Path Traversal  
        { @"\.\.[\\/]|\.\.\\", "Potential path traversal pattern detected (CWE-22)" },
        { @"File\.ReadAllText\s*\([^)]*\+", "Potential path traversal in file operations (CWE-22)" },
        
        // CWE-79: Cross-site Scripting
        { @"innerHTML|outerHTML", "Potential XSS via DOM manipulation (CWE-79)" },
        { @"document\.write\s*\(", "Potential XSS via document.write (CWE-79)" },
        
        // OWASP A08:2021 - Software and Data Integrity Failures
        { @"MD5|SHA1(?!256|384|512)", "Weak cryptographic hash algorithm (OWASP A08, CWE-327)" },
        
        // CWE-190: Integer Overflow
        { @"int\.MaxValue|long\.MaxValue", "Potential integer overflow risk (CWE-190)" },
        
        // OWASP A10:2021 - Server-Side Request Forgery
        { @"HttpClient.*\.GetAsync\s*\([^)]*\+", "Potential SSRF via dynamic URL construction (OWASP A10, CWE-918)" },
        
        // CWE-434: Unrestricted File Upload
        { @"\.Save\s*\([^)]*\.FileName", "Potential unrestricted file upload (CWE-434)" },
        
        // Additional security patterns
        { @"Random\s*\(\s*\)", "Weak random number generation (CWE-338)" },
        { @"Thread\.Sleep\s*\(\s*0\s*\)", "Potential timing attack vulnerability (CWE-208)" }
    };

    public SecurityScanner(bool verbose = false)
    {
        _verbose = verbose;
    }

    public async Task ScanAsync(string path, string? ctrfReportPath = null)
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

        // Generate CTRF report if requested
        if (!string.IsNullOrEmpty(ctrfReportPath))
        {
            await GenerateCtrfReport(ctrfReportPath);
        }
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

        // Check for OWASP Top 10 and CWE Top 25 patterns
        await CheckOwaspAndCwePatterns(filePath, root);
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

    private async Task CheckOwaspAndCwePatterns(string filePath, SyntaxNode root)
    {
        // CWE-352: Cross-Site Request Forgery - Check for missing anti-forgery tokens
        await CheckCsrfProtection(filePath, root);

        // CWE-327: Weak Cryptography
        await CheckWeakCryptography(filePath, root);

        // OWASP A05:2021 - Security Misconfiguration
        await CheckSecurityMisconfiguration(filePath, root);

        // CWE-209: Information Exposure Through Error Messages
        await CheckInformationExposure(filePath, root);

        // OWASP A07:2021 - Identification and Authentication Failures
        await CheckAuthenticationFailures(filePath, root);

        // CWE-200: Information Exposure
        await CheckDataExposure(filePath, root);

        // CWE-434: Unrestricted File Upload
        await CheckFileUploadSecurity(filePath, root);
    }

    private async Task CheckCsrfProtection(string filePath, SyntaxNode root)
    {
        // Check for HTTP POST actions without CSRF protection
        var postMethods = root.DescendantNodes().OfType<MethodDeclarationSyntax>()
            .Where(m => m.AttributeLists.SelectMany(al => al.Attributes)
                .Any(attr => attr.Name.ToString().Contains("HttpPost")));

        foreach (var method in postMethods)
        {
            var hasAntiForgeryCsrf = method.AttributeLists
                .SelectMany(list => list.Attributes)
                .Any(attr => attr.Name.ToString().Contains("ValidateAntiForgeryToken") || 
                           attr.Name.ToString().Contains("AutoValidateAntiforgeryToken"));

            if (!hasAntiForgeryCsrf)
            {
                var lineNumber = method.GetLocation().GetLineSpan().StartLinePosition.Line + 1;
                _findings.Add(new SecurityFinding(
                    filePath,
                    lineNumber,
                    SecurityLevel.High,
                    $"POST method '{method.Identifier.ValueText}' lacks CSRF protection (CWE-352)",
                    method.Identifier.ValueText,
                    "CSRF Protection"
                ));
            }
        }
    }

    private async Task CheckWeakCryptography(string filePath, SyntaxNode root)
    {
        // Check for weak hash algorithms
        var memberAccess = root.DescendantNodes().OfType<MemberAccessExpressionSyntax>();

        foreach (var access in memberAccess)
        {
            var accessString = access.ToString();
            if (accessString.Contains("MD5") || accessString.Contains("SHA1.Create"))
            {
                var lineNumber = access.GetLocation().GetLineSpan().StartLinePosition.Line + 1;
                _findings.Add(new SecurityFinding(
                    filePath,
                    lineNumber,
                    SecurityLevel.High,
                    $"Weak cryptographic algorithm detected: {accessString} (OWASP A02, CWE-327)",
                    accessString,
                    "Weak Cryptography"
                ));
            }
        }

        // Check for hardcoded encryption keys
        var invocations = root.DescendantNodes().OfType<InvocationExpressionSyntax>()
            .Where(inv => inv.Expression.ToString().Contains("Encrypt") || 
                         inv.Expression.ToString().Contains("Decrypt"));

        foreach (var invocation in invocations)
        {
            var arguments = invocation.ArgumentList.Arguments;
            foreach (var arg in arguments)
            {
                if (arg.Expression is LiteralExpressionSyntax literal && 
                    literal.Token.IsKind(SyntaxKind.StringLiteralToken))
                {
                    var lineNumber = invocation.GetLocation().GetLineSpan().StartLinePosition.Line + 1;
                    _findings.Add(new SecurityFinding(
                        filePath,
                        lineNumber,
                        SecurityLevel.Critical,
                        "Hardcoded encryption key detected (OWASP A02, CWE-321)",
                        invocation.ToString(),
                        "Hardcoded Secrets"
                    ));
                }
            }
        }
    }

    private async Task CheckSecurityMisconfiguration(string filePath, SyntaxNode root)
    {
        // Check for debug mode in production-like code
        var invocations = root.DescendantNodes().OfType<InvocationExpressionSyntax>();

        foreach (var invocation in invocations)
        {
            var invText = invocation.ToString().ToLower();
            if (invText.Contains("adddeveloperexceptionpage") || 
                invText.Contains("usedeveloperexceptionpage"))
            {
                var lineNumber = invocation.GetLocation().GetLineSpan().StartLinePosition.Line + 1;
                _findings.Add(new SecurityFinding(
                    filePath,
                    lineNumber,
                    SecurityLevel.Medium,
                    "Developer exception page enabled - potential information disclosure (OWASP A05)",
                    invocation.ToString(),
                    "Security Misconfiguration"
                ));
            }

            if (invText.Contains("allowanyorigin") || invText.Contains("allowanyheader"))
            {
                var lineNumber = invocation.GetLocation().GetLineSpan().StartLinePosition.Line + 1;
                _findings.Add(new SecurityFinding(
                    filePath,
                    lineNumber,
                    SecurityLevel.High,
                    "Overly permissive CORS configuration detected (OWASP A05, CWE-942)",
                    invocation.ToString(),
                    "Security Misconfiguration"
                ));
            }
        }
    }

    private async Task CheckInformationExposure(string filePath, SyntaxNode root)
    {
        // Check for sensitive information in ToString() methods
        var toStringMethods = root.DescendantNodes().OfType<MethodDeclarationSyntax>()
            .Where(m => m.Identifier.ValueText == "ToString");

        foreach (var method in toStringMethods)
        {
            var methodText = method.ToString().ToLower();
            if (methodText.Contains("password") || methodText.Contains("secret") || 
                methodText.Contains("token") || methodText.Contains("key"))
            {
                var lineNumber = method.GetLocation().GetLineSpan().StartLinePosition.Line + 1;
                _findings.Add(new SecurityFinding(
                    filePath,
                    lineNumber,
                    SecurityLevel.High,
                    "Sensitive information exposed in ToString method (CWE-200)",
                    method.Identifier.ValueText,
                    "Information Exposure"
                ));
            }
        }
    }

    private async Task CheckAuthenticationFailures(string filePath, SyntaxNode root)
    {
        // Check for weak password validation
        var methodCalls = root.DescendantNodes().OfType<InvocationExpressionSyntax>();

        foreach (var call in methodCalls)
        {
            var callString = call.ToString().ToLower();
            if (callString.Contains("createuser") || callString.Contains("register"))
            {
                // Check if password strength validation is missing
                var containingMethod = call.Ancestors().OfType<MethodDeclarationSyntax>().FirstOrDefault();
                if (containingMethod != null)
                {
                    var methodText = containingMethod.ToString().ToLower();
                    if (!methodText.Contains("password") || 
                        (!methodText.Contains("length") && !methodText.Contains("complexity")))
                    {
                        var lineNumber = call.GetLocation().GetLineSpan().StartLinePosition.Line + 1;
                        _findings.Add(new SecurityFinding(
                            filePath,
                            lineNumber,
                            SecurityLevel.Medium,
                            "Weak password validation detected (OWASP A07, CWE-521)",
                            call.ToString(),
                            "Authentication Failures"
                        ));
                    }
                }
            }
        }
    }

    private async Task CheckDataExposure(string filePath, SyntaxNode root)
    {
        // Check for potential data exposure in API responses
        var returnStatements = root.DescendantNodes().OfType<ReturnStatementSyntax>();

        foreach (var returnStmt in returnStatements)
        {
            var returnText = returnStmt.ToString().ToLower();
            if (returnText.Contains("user") && (returnText.Contains("password") || 
                returnText.Contains("passwordhash") || returnText.Contains("salt")))
            {
                var lineNumber = returnStmt.GetLocation().GetLineSpan().StartLinePosition.Line + 1;
                _findings.Add(new SecurityFinding(
                    filePath,
                    lineNumber,
                    SecurityLevel.Critical,
                    "Sensitive user data exposed in API response (CWE-200)",
                    returnStmt.ToString(),
                    "Data Exposure"
                ));
            }
        }
    }

    private async Task CheckFileUploadSecurity(string filePath, SyntaxNode root)
    {
        // Check for file upload without validation
        var methodCalls = root.DescendantNodes().OfType<InvocationExpressionSyntax>();

        foreach (var call in methodCalls)
        {
            var callString = call.ToString();
            if (callString.Contains("SaveAs") || callString.Contains("CopyTo"))
            {
                var containingMethod = call.Ancestors().OfType<MethodDeclarationSyntax>().FirstOrDefault();
                if (containingMethod != null)
                {
                    var methodText = containingMethod.ToString().ToLower();
                    if (!methodText.Contains("contenttype") && !methodText.Contains("extension"))
                    {
                        var lineNumber = call.GetLocation().GetLineSpan().StartLinePosition.Line + 1;
                        _findings.Add(new SecurityFinding(
                            filePath,
                            lineNumber,
                            SecurityLevel.High,
                            "File upload without proper validation (CWE-434)",
                            call.ToString(),
                            "File Upload Security"
                        ));
                    }
                }
            }
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
            ShowSecurityBestPractices();
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

        // Show severity-specific security recommendations
        ShowSecurityRecommendations();

        // Exit with error code if critical or high findings exist
        if (criticalFindings > 0 || highFindings > 0)
        {
            Environment.ExitCode = 1;
        }
    }

    private void ShowSecurityRecommendations()
    {
        Console.WriteLine("🛡️  Security Recommendations by Severity");
        Console.WriteLine("=========================================");

        var criticalFindings = _findings.Where(f => f.Level == SecurityLevel.Critical).ToList();
        var highFindings = _findings.Where(f => f.Level == SecurityLevel.High).ToList();
        var mediumFindings = _findings.Where(f => f.Level == SecurityLevel.Medium).ToList();
        var lowFindings = _findings.Where(f => f.Level == SecurityLevel.Low).ToList();

        if (criticalFindings.Any())
        {
            Console.WriteLine("🔴 CRITICAL Security Issues (IMMEDIATE ACTION REQUIRED):");
            ShowRecommendationsForSecurityLevel(criticalFindings, SecurityLevel.Critical);
            Console.WriteLine();
        }

        if (highFindings.Any())
        {
            Console.WriteLine("🟠 HIGH Risk Security Issues (Address Within 24-48 Hours):");
            ShowRecommendationsForSecurityLevel(highFindings, SecurityLevel.High);
            Console.WriteLine();
        }

        if (mediumFindings.Any())
        {
            Console.WriteLine("🟡 MEDIUM Risk Security Issues (Address This Sprint):");
            ShowRecommendationsForSecurityLevel(mediumFindings, SecurityLevel.Medium);
            Console.WriteLine();
        }

        if (lowFindings.Any())
        {
            Console.WriteLine("🔵 LOW Risk Security Issues (Security Hardening):");
            ShowRecommendationsForSecurityLevel(lowFindings, SecurityLevel.Low);
            Console.WriteLine();
        }
    }

    private void ShowRecommendationsForSecurityLevel(List<SecurityFinding> findings, SecurityLevel level)
    {
        var groupedByCategory = findings.GroupBy(f => f.Category).OrderByDescending(g => g.Count());
        
        foreach (var category in groupedByCategory)
        {
            var categoryName = category.Key;
            var count = category.Count();
            var recommendations = GetSecurityRecommendations(categoryName, level, count);
            
            Console.WriteLine($"  📂 {categoryName} ({count} issues):");
            foreach (var recommendation in recommendations.Take(3)) // Top 3 recommendations per category
            {
                Console.WriteLine($"    • {recommendation}");
            }
            Console.WriteLine();
        }
    }

    private List<string> GetSecurityRecommendations(string category, SecurityLevel level, int count)
    {
        var recommendations = new List<string>();

        switch (category.ToLower())
        {
            case "hardcoded secrets":
                recommendations.AddRange(level switch
                {
                    SecurityLevel.Critical => new[]
                    {
                        $"URGENT: Remove {count} hardcoded secrets immediately - these are security vulnerabilities",
                        "Move all secrets to environment variables or Azure Key Vault",
                        "Rotate all exposed secrets/keys immediately",
                        "Implement secret scanning in CI/CD pipeline"
                    },
                    SecurityLevel.High => new[]
                    {
                        $"HIGH PRIORITY: Secure {count} hardcoded sensitive values",
                        "Use IConfiguration or IOptions pattern for configuration",
                        "Implement proper secret management strategy"
                    },
                    _ => new[]
                    {
                        $"Move {count} configuration values to appsettings or environment variables",
                        "Follow 12-factor app principles for configuration"
                    }
                });
                break;

            case "sql injection":
                recommendations.AddRange(level switch
                {
                    SecurityLevel.Critical => new[]
                    {
                        $"CRITICAL: Fix {count} SQL injection vulnerabilities NOW - these allow data breaches",
                        "Use parameterized queries or Entity Framework exclusively",
                        "Never concatenate user input into SQL strings",
                        "Conduct immediate security audit of all database access code"
                    },
                    SecurityLevel.High => new[]
                    {
                        $"HIGH RISK: Secure {count} potential SQL injection points",
                        "Replace string concatenation with parameterized queries",
                        "Use Entity Framework or Dapper with proper parameterization"
                    },
                    _ => new[]
                    {
                        $"Review {count} database access patterns for security",
                        "Ensure all user input is properly sanitized"
                    }
                });
                break;

            case "authorization":
                recommendations.AddRange(level switch
                {
                    SecurityLevel.Critical => new[]
                    {
                        $"CRITICAL: {count} endpoints lack authorization - immediate security risk",
                        "Add [Authorize] attributes to all controllers and sensitive actions",
                        "Implement role-based access control (RBAC)",
                        "Review all API endpoints for proper authentication"
                    },
                    SecurityLevel.High => new[]
                    {
                        $"HIGH PRIORITY: Secure {count} unprotected endpoints",
                        "Add proper authorization attributes",
                        "Consider using policy-based authorization"
                    },
                    SecurityLevel.Medium => new[]
                    {
                        $"Add authorization to {count} controller methods",
                        "Use [AllowAnonymous] explicitly for public endpoints",
                        "Review current authorization strategy"
                    },
                    _ => new[]
                    {
                        $"Review authorization patterns for {count} methods",
                        "Ensure consistent security across all endpoints"
                    }
                });
                break;

            case "input validation":
                recommendations.AddRange(level switch
                {
                    SecurityLevel.Critical => new[]
                    {
                        $"CRITICAL: {count} parameters lack validation - injection risk",
                        "Add validation attributes to all user input parameters",
                        "Implement custom validation for complex business rules",
                        "Use ModelState.IsValid in all actions"
                    },
                    SecurityLevel.High => new[]
                    {
                        $"HIGH PRIORITY: Add validation to {count} input parameters",
                        "Use data annotations for input validation",
                        "Implement FluentValidation for complex scenarios"
                    },
                    _ => new[]
                    {
                        $"Add validation attributes to {count} parameters",
                        "Follow defense-in-depth principle for input validation"
                    }
                });
                break;

            case "logging security":
                recommendations.AddRange(level switch
                {
                    SecurityLevel.High => new[]
                    {
                        $"HIGH RISK: {count} logging statements may expose sensitive data",
                        "Sanitize all logged data to remove PII and secrets",
                        "Use structured logging with careful property selection",
                        "Implement log sanitization middleware"
                    },
                    SecurityLevel.Medium => new[]
                    {
                        $"Review {count} logging statements for sensitive data",
                        "Use logging best practices to avoid data exposure"
                    },
                    _ => new[]
                    {
                        $"Review logging patterns in {count} locations",
                        "Ensure no sensitive information is logged"
                    }
                });
                break;

            case "error handling":
                recommendations.AddRange(level switch
                {
                    SecurityLevel.Medium => new[]
                    {
                        $"Improve error handling in {count} locations to prevent information disclosure",
                        "Return generic error messages to users",
                        "Log detailed errors server-side only",
                        "Implement global exception handling middleware"
                    },
                    _ => new[]
                    {
                        $"Review error handling patterns in {count} locations",
                        "Ensure error messages don't expose internal details"
                    }
                });
                break;

            case "resource management":
                recommendations.AddRange(level switch
                {
                    SecurityLevel.Low => new[]
                    {
                        $"Improve resource management in {count} locations",
                        "Use IHttpClientFactory for HttpClient instances",
                        "Implement proper disposal patterns for resources",
                        "Follow .NET resource management best practices"
                    },
                    _ => new[]
                    {
                        $"Review resource management patterns in {count} locations"
                    }
                });
                break;

            case "best practices":
                recommendations.AddRange(level switch
                {
                    SecurityLevel.Low => new[]
                    {
                        $"Follow security best practices in {count} locations",
                        "Use UTC dates for consistency and security",
                        "Follow OWASP guidelines for secure coding",
                        "Regular security code reviews"
                    },
                    _ => new[]
                    {
                        $"Apply security best practices to {count} code locations"
                    }
                });
                break;

            case "csrf protection":
                recommendations.AddRange(level switch
                {
                    SecurityLevel.High => new[]
                    {
                        $"HIGH PRIORITY: Add CSRF protection to {count} POST endpoints",
                        "Use [ValidateAntiForgeryToken] attribute on POST actions",
                        "Implement anti-forgery tokens in forms",
                        "Review all state-changing operations for CSRF protection"
                    },
                    SecurityLevel.Medium => new[]
                    {
                        $"Add CSRF protection to {count} POST methods",
                        "Consider using [AutoValidateAntiforgeryToken] globally"
                    },
                    _ => new[]
                    {
                        $"Review CSRF protection for {count} endpoints"
                    }
                });
                break;

            case "weak cryptography":
                recommendations.AddRange(level switch
                {
                    SecurityLevel.Critical => new[]
                    {
                        $"CRITICAL: Replace {count} weak cryptographic implementations immediately",
                        "Remove all hardcoded encryption keys - use secure key management",
                        "Rotate any compromised cryptographic keys",
                        "Conduct security audit of all cryptographic implementations"
                    },
                    SecurityLevel.High => new[]
                    {
                        $"HIGH PRIORITY: Replace {count} weak cryptographic algorithms",
                        "Use SHA-256 or SHA-512 instead of MD5/SHA-1",
                        "Implement proper key management practices",
                        "Use cryptographically secure random number generators"
                    },
                    _ => new[]
                    {
                        $"Update {count} cryptographic implementations to use stronger algorithms"
                    }
                });
                break;

            case "security misconfiguration":
                recommendations.AddRange(level switch
                {
                    SecurityLevel.High => new[]
                    {
                        $"HIGH PRIORITY: Fix {count} security misconfigurations",
                        "Remove overly permissive CORS policies",
                        "Disable debug features in production",
                        "Implement proper security headers and configurations"
                    },
                    SecurityLevel.Medium => new[]
                    {
                        $"Review {count} configuration issues for security impact",
                        "Implement secure defaults and configuration management"
                    },
                    _ => new[]
                    {
                        $"Address {count} configuration security issues"
                    }
                });
                break;

            case "information exposure":
                recommendations.AddRange(level switch
                {
                    SecurityLevel.High => new[]
                    {
                        $"HIGH PRIORITY: Prevent {count} information exposure vulnerabilities",
                        "Remove sensitive data from ToString() methods",
                        "Sanitize error messages to prevent information disclosure",
                        "Review all public-facing methods for data exposure"
                    },
                    _ => new[]
                    {
                        $"Review {count} potential information exposure points"
                    }
                });
                break;

            case "authentication failures":
                recommendations.AddRange(level switch
                {
                    SecurityLevel.Medium => new[]
                    {
                        $"Strengthen authentication for {count} identified issues",
                        "Implement strong password requirements",
                        "Add password complexity validation",
                        "Consider multi-factor authentication for sensitive operations"
                    },
                    _ => new[]
                    {
                        $"Review authentication patterns in {count} locations"
                    }
                });
                break;

            case "data exposure":
                recommendations.AddRange(level switch
                {
                    SecurityLevel.Critical => new[]
                    {
                        $"CRITICAL: {count} potential data exposure vulnerabilities found",
                        "Never return sensitive user data in API responses",
                        "Implement proper data filtering and DTOs",
                        "Conduct immediate audit of all API endpoints"
                    },
                    _ => new[]
                    {
                        $"Review data exposure risks in {count} locations"
                    }
                });
                break;

            case "file upload security":
                recommendations.AddRange(level switch
                {
                    SecurityLevel.High => new[]
                    {
                        $"HIGH PRIORITY: Secure {count} file upload operations",
                        "Validate file types and extensions",
                        "Implement file size limits",
                        "Scan uploaded files for malware",
                        "Store uploaded files outside web root"
                    },
                    _ => new[]
                    {
                        $"Review file upload security for {count} operations"
                    }
                });
                break;

            default:
                recommendations.Add($"Address {count} {category.ToLower()} security issues based on severity level");
                break;
        }

        return recommendations;
    }

    private void ShowSecurityBestPractices()
    {
        Console.WriteLine("🛡️  Security Best Practices to Maintain:");
        Console.WriteLine("  • Continue using parameterized queries");
        Console.WriteLine("  • Keep authorization attributes on all endpoints");
        Console.WriteLine("  • Maintain proper input validation");
        Console.WriteLine("  • Regular security scanning and code reviews");
        Console.WriteLine("  • Keep dependencies updated");
        Console.WriteLine("  • Follow OWASP Top 10 guidelines");
        Console.WriteLine();
    }

    private async Task GenerateCtrfReport(string reportPath)
    {
        try
        {
            var criticalFindings = _findings.Count(f => f.Level == SecurityLevel.Critical);
            var highFindings = _findings.Count(f => f.Level == SecurityLevel.High);
            var mediumFindings = _findings.Count(f => f.Level == SecurityLevel.Medium);
            var lowFindings = _findings.Count(f => f.Level == SecurityLevel.Low);

            var ctrfReport = new CtrfReport
            {
                Results = new CtrfResults
                {
                    Tool = new CtrfTool
                    {
                        Name = "DemoInventory.Tools.SecurityScan",
                        Version = "1.0.0"
                    },
                    Summary = new CtrfSummary
                    {
                        Tests = _findings.Count,
                        Passed = 0, // Security findings are failures by definition
                        Failed = _findings.Count,
                        Pending = 0,
                        Skipped = 0,
                        Other = 0,
                        Start = DateTime.UtcNow.AddMinutes(-1).Ticks, // Approximate start time
                        Stop = DateTime.UtcNow.Ticks
                    },
                    Tests = _findings.Select(finding => new CtrfTest
                    {
                        Name = $"{finding.Category}: {finding.Description}",
                        Status = "failed",
                        Duration = 0,
                        Message = $"Security issue found in {Path.GetFileName(finding.FilePath)}:{finding.LineNumber}",
                        Trace = finding.CodeSnippet,
                        RawStatus = finding.Level.ToString(),
                        ExtraData = new Dictionary<string, object>
                        {
                            { "severity", finding.Level.ToString() },
                            { "category", finding.Category },
                            { "filePath", finding.FilePath },
                            { "lineNumber", finding.LineNumber },
                            { "owaspMapping", GetOwaspMapping(finding.Description) },
                            { "cweMapping", GetCweMapping(finding.Description) }
                        }
                    }).ToList()
                }
            };

            var json = JsonSerializer.Serialize(ctrfReport, new JsonSerializerOptions 
            { 
                WriteIndented = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            await File.WriteAllTextAsync(reportPath, json);
            Console.WriteLine($"📄 CTRF report generated: {reportPath}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Failed to generate CTRF report: {ex.Message}");
        }
    }

    private string GetOwaspMapping(string description)
    {
        if (description.Contains("OWASP A01")) return "A01:2021 - Broken Access Control";
        if (description.Contains("OWASP A02")) return "A02:2021 - Cryptographic Failures";
        if (description.Contains("OWASP A03")) return "A03:2021 - Injection";
        if (description.Contains("OWASP A05")) return "A05:2021 - Security Misconfiguration";
        if (description.Contains("OWASP A06")) return "A06:2021 - Vulnerable and Outdated Components";
        if (description.Contains("OWASP A07")) return "A07:2021 - Identification and Authentication Failures";
        if (description.Contains("OWASP A08")) return "A08:2021 - Software and Data Integrity Failures";
        if (description.Contains("OWASP A09")) return "A09:2021 - Security Logging and Monitoring Failures";
        if (description.Contains("OWASP A10")) return "A10:2021 - Server-Side Request Forgery";
        return "Multiple OWASP categories may apply";
    }

    private string GetCweMapping(string description)
    {
        if (description.Contains("CWE-79")) return "CWE-79: Cross-site Scripting";
        if (description.Contains("CWE-89")) return "CWE-89: SQL Injection";
        if (description.Contains("CWE-78")) return "CWE-78: OS Command Injection";
        if (description.Contains("CWE-22")) return "CWE-22: Path Traversal";
        if (description.Contains("CWE-352")) return "CWE-352: Cross-Site Request Forgery";
        if (description.Contains("CWE-434")) return "CWE-434: Unrestricted Upload of File with Dangerous Type";
        if (description.Contains("CWE-94")) return "CWE-94: Code Injection";
        if (description.Contains("CWE-190")) return "CWE-190: Integer Overflow";
        if (description.Contains("CWE-200")) return "CWE-200: Information Exposure";
        if (description.Contains("CWE-209")) return "CWE-209: Information Exposure Through Error Messages";
        if (description.Contains("CWE-321")) return "CWE-321: Use of Hard-coded Cryptographic Key";
        if (description.Contains("CWE-327")) return "CWE-327: Use of a Broken or Risky Cryptographic Algorithm";
        if (description.Contains("CWE-338")) return "CWE-338: Use of Cryptographically Weak Pseudo-Random Number Generator";
        if (description.Contains("CWE-521")) return "CWE-521: Weak Password Requirements";
        if (description.Contains("CWE-798")) return "CWE-798: Use of Hard-coded Credentials";
        if (description.Contains("CWE-918")) return "CWE-918: Server-Side Request Forgery";
        if (description.Contains("CWE-942")) return "CWE-942: Overly Permissive Cross-domain Whitelist";
        return "Multiple CWE categories may apply";
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

/// <summary>
/// CTRF (Common Test Report Format) classes for standardized security reporting
/// </summary>
public class CtrfReport
{
    public CtrfResults Results { get; set; } = new();
}

public class CtrfResults
{
    public CtrfTool Tool { get; set; } = new();
    public CtrfSummary Summary { get; set; } = new();
    public List<CtrfTest> Tests { get; set; } = new();
}

public class CtrfTool
{
    public string Name { get; set; } = string.Empty;
    public string Version { get; set; } = string.Empty;
}

public class CtrfSummary
{
    public int Tests { get; set; }
    public int Passed { get; set; }
    public int Failed { get; set; }
    public int Pending { get; set; }
    public int Skipped { get; set; }
    public int Other { get; set; }
    public long Start { get; set; }
    public long Stop { get; set; }
}

public class CtrfTest
{
    public string Name { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public long Duration { get; set; }
    public string Message { get; set; } = string.Empty;
    public string Trace { get; set; } = string.Empty;
    public string RawStatus { get; set; } = string.Empty;
    public Dictionary<string, object> ExtraData { get; set; } = new();
}