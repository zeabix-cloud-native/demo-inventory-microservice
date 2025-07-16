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

    // Security patterns to detect (OWASP Top 10 & CWE Top 25 2024)
    private readonly Dictionary<string, string> _securityPatterns = new()
    {
        // CWE-798: Use of Hard-coded Credentials (OWASP A02:2021 - Cryptographic Failures)
        { @"password\s*[=:]\s*[""'][^""']+[""']", "Hardcoded password detected (OWASP A02, CWE-798)" },
        { @"api[_-]?key\s*[=:]\s*[""'][^""']+[""']", "Hardcoded API key detected (OWASP A02, CWE-798)" },
        { @"secret\s*[=:]\s*[""'][^""']+[""']", "Hardcoded secret detected (OWASP A02, CWE-798)" },
        { @"token\s*[=:]\s*[""'][^""']+[""']", "Hardcoded token detected (OWASP A02, CWE-798)" },
        { @"connectionstring\s*[=:]\s*[""'][^""']+[""']", "Hardcoded connection string detected (OWASP A02, CWE-798)" },
        
        // CWE-89: SQL Injection (OWASP A03:2021 - Injection)
        { @"SELECT\s+\*\s+FROM\s+\w+\s+WHERE\s+.*\+", "Potential SQL injection via string concatenation (OWASP A03, CWE-89)" },
        
        // CWE-20: Improper Input Validation
        { @"Request\.QueryString\s*\[[^]]+\]", "Direct access to query string without validation (CWE-20)" },
        { @"Request\.Form\s*\[[^]]+\]", "Direct access to form data without validation (CWE-20)" },
        
        // CWE-78: OS Command Injection
        { @"Process\.Start\s*\([^)]*\+", "Potential command injection via Process.Start (OWASP A03, CWE-78)" },
        { @"cmd\.exe|powershell\.exe", "Direct OS command execution detected (CWE-78)" },
        
        // CWE-79: Cross-site Scripting
        { @"Response\.Write\s*\([^)]*\+", "Potential XSS via Response.Write with concatenation (CWE-79)" },
        
        // CWE-190: Integer Overflow or Wraparound
        { @"int\.MaxValue|long\.MaxValue", "Potential integer overflow risk (CWE-190) - Review usage context" },
        
        // CWE-22: Improper Limitation of a Pathname to a Restricted Directory (Path Traversal)
        { @"\.\.[\\/]", "Potential path traversal pattern detected (CWE-22)" },
        { @"File\.ReadAllText\s*\([^)]*\+", "Potential path traversal in file operations (CWE-22)" },
        { @"Path\.Combine\s*\([^)]*Request\.", "Path combination with user input (CWE-22)" },
        
        // CWE-125: Out-of-bounds Read
        { @"\.Substring\s*\(\s*\w+\s*,\s*\w+\s*\)", "String substring without length validation (CWE-125)" },
        
        // CWE-434: Unrestricted Upload of File with Dangerous Type
        { @"\.Save\s*\([^)]*\.FileName", "Potential unrestricted file upload (CWE-434)" },
        { @"IFormFile.*\.SaveAs\s*\(", "File upload without type validation (CWE-434)" },
        
        // CWE-787: Out-of-bounds Write
        { @"Buffer\.SetByte\s*\(", "Direct buffer manipulation detected (CWE-787)" },
        { @"Marshal\.Copy\s*\(", "Unsafe memory operation detected (CWE-787)" },
        
        // CWE-94: Improper Control of Generation of Code (Code Injection)
        { @"eval\s*\(|Eval\s*\(", "Dynamic code evaluation detected (CWE-94)" },
        { @"CompileAssemblyFromSource", "Dynamic code compilation detected (CWE-94)" },
        
        // CWE-276: Incorrect Default Permissions
        { @"FilePermissions\.|UnixFileMode\.", "File permission setting detected - review needed (CWE-276)" },
        
        // CWE-200: Exposure of Sensitive Information to an Unauthorized Actor
        { @"Exception\s*\.\s*(?:Message|StackTrace)", "Exception details exposed (CWE-200)" },
        
        // CWE-522: Insufficiently Protected Credentials
        { @"password.*=.*[""'][^""']+[""']", "Password stored in plain text (CWE-522)" },
        { @"Encoding\.UTF8\.GetBytes\s*\(\s*password", "Password converted to bytes without hashing (CWE-522)" },
        
        // CWE-732: Incorrect Permission Assignment for Critical Resource
        { @"Directory\.CreateDirectory\s*\([^)]*\)\s*(?!.*permission)", "Directory creation without permission check (CWE-732)" },
        
        // CWE-611: Improper Restriction of XML External Entity Reference
        { @"XmlDocument|XmlTextReader|XslCompiledTransform", "XML processing without XXE protection (CWE-611)" },
        { @"XmlReaderSettings.*DtdProcessing\s*=\s*DtdProcessing\.Parse", "DTD processing enabled - XXE risk (CWE-611)" },
        
        // CWE-77: Improper Neutralization of Special Elements used in a Command (Command Injection)
        { @"ProcessStartInfo.*Arguments.*\+", "Command arguments concatenation detected (CWE-77)" },
        
        // CWE-502: Deserialization of Untrusted Data
        { @"JsonConvert\.DeserializeObject\s*<.*>\s*\(", "JSON deserialization without validation (CWE-502)" },
        { @"BinaryFormatter|SoapFormatter", "Unsafe deserialization formatter (CWE-502)" },
        { @"XmlSerializer.*Deserialize\s*\(", "XML deserialization without validation (CWE-502)" },
        
        // CWE-269: Improper Privilege Management
        { @"WindowsIdentity\.Impersonate|RunAs", "Privilege escalation operation detected (CWE-269)" },
        
        // CWE-287: Improper Authentication
        { @"FormsAuthentication\.SetAuthCookie\s*\([^)]*false", "Authentication cookie without SSL (CWE-287)" },
        { @"cookieAuthentication.*RequireHttps\s*=\s*false", "Authentication cookie without HTTPS (CWE-287)" },
        
        // CWE-327: Use of a Broken or Risky Cryptographic Algorithm
        { @"\bMD5\b|\bSHA1\b(?!256|384|512)", "Weak cryptographic hash algorithm (OWASP A08, CWE-327)" },
        { @"\bDES\b(?!3)|\bRC4\b|\bRC2\b", "Weak encryption algorithm detected (CWE-327)" },
        
        // CWE-338: Use of Cryptographically Weak Pseudo-Random Number Generator
        { @"Random\s*\(\s*\)", "Weak random number generation (CWE-338)" },
        { @"Environment\.TickCount", "Predictable random seed detected (CWE-338)" },
        
        // CWE-918: Server-Side Request Forgery (SSRF)
        { @"HttpClient.*\.GetAsync\s*\([^)]*\+", "Potential SSRF via dynamic URL construction (OWASP A10, CWE-918)" },
        { @"WebRequest\.Create\s*\([^)]*\+", "Web request with dynamic URL (CWE-918)" },
        
        // Additional CWE patterns
        { @"Thread\.Sleep\s*\(\s*0\s*\)", "Potential timing attack vulnerability (CWE-208)" },
        { @"HttpClient\s*\(\s*\)\s*\.", "HttpClient without proper disposal pattern (OWASP A06)" }
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
            .Where(file => !file.Contains("bin") && 
                          !file.Contains("obj") &&
                          !file.Contains("Tests") && // Exclude test files to reduce false positives
                          !file.Contains("test"))    // Exclude test files (case insensitive)
            .ToList();

        Console.WriteLine($"Found {csFiles.Count} C# files to scan (excluding tests)");
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

        // CWE-20: Improper Input Validation (enhanced)
        await CheckImproperInputValidation(filePath, root);

        // CWE-125: Out-of-bounds Read
        await CheckOutOfBoundsRead(filePath, root);

        // CWE-476: NULL Pointer Dereference
        await CheckNullPointerDereference(filePath, root);

        // CWE-287: Improper Authentication
        await CheckImproperAuthentication(filePath, root);

        // CWE-276: Incorrect Default Permissions
        await CheckIncorrectDefaultPermissions(filePath, root);

        // CWE-522: Insufficiently Protected Credentials
        await CheckInsufficientlyProtectedCredentials(filePath, root);

        // CWE-732: Incorrect Permission Assignment for Critical Resource
        await CheckIncorrectPermissionAssignment(filePath, root);

        // CWE-611: XXE (XML External Entity)
        await CheckXmlExternalEntity(filePath, root);

        // CWE-77: Command Injection (enhanced)
        await CheckCommandInjection(filePath, root);

        // CWE-306: Missing Authentication for Critical Function
        await CheckMissingAuthenticationCritical(filePath, root);

        // CWE-502: Deserialization of Untrusted Data
        await CheckDeserializationUntrustedData(filePath, root);

        // CWE-269: Improper Privilege Management
        await CheckImproperPrivilegeManagement(filePath, root);
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

    private async Task CheckImproperInputValidation(string filePath, SyntaxNode root)
    {
        // Check for direct access to Request properties without validation
        var memberAccess = root.DescendantNodes().OfType<MemberAccessExpressionSyntax>();

        foreach (var access in memberAccess)
        {
            var accessString = access.ToString();
            if (accessString.Contains("Request.QueryString") || 
                accessString.Contains("Request.Form") ||
                accessString.Contains("Request.Headers"))
            {
                var lineNumber = access.GetLocation().GetLineSpan().StartLinePosition.Line + 1;
                _findings.Add(new SecurityFinding(
                    filePath,
                    lineNumber,
                    SecurityLevel.Medium,
                    $"Direct access to request data without validation (CWE-20)",
                    accessString,
                    "Input Validation"
                ));
            }
        }

        // Check for user input used in sensitive operations without validation
        var invocations = root.DescendantNodes().OfType<InvocationExpressionSyntax>();
        foreach (var invocation in invocations)
        {
            var invText = invocation.ToString();
            if ((invText.Contains("File.") || invText.Contains("Directory.")) && 
                invText.Contains("Request."))
            {
                var lineNumber = invocation.GetLocation().GetLineSpan().StartLinePosition.Line + 1;
                _findings.Add(new SecurityFinding(
                    filePath,
                    lineNumber,
                    SecurityLevel.High,
                    "User input used in file/directory operations without validation (CWE-20)",
                    invocation.ToString(),
                    "Input Validation"
                ));
            }
        }
    }

    private async Task CheckOutOfBoundsRead(string filePath, SyntaxNode root)
    {
        // Check for array access without bounds checking
        var elementAccess = root.DescendantNodes().OfType<ElementAccessExpressionSyntax>();

        foreach (var access in elementAccess)
        {
            // Look for array access where the index is not obviously safe
            var containingMethod = access.Ancestors().OfType<MethodDeclarationSyntax>().FirstOrDefault();
            if (containingMethod != null)
            {
                var methodText = containingMethod.ToString();
                var accessText = access.ToString();
                
                // Check if there's no bounds checking in the method
                if (!methodText.Contains(".Length") && !methodText.Contains(".Count") && 
                    !methodText.Contains("bounds") && !accessText.Contains("Length"))
                {
                    var lineNumber = access.GetLocation().GetLineSpan().StartLinePosition.Line + 1;
                    _findings.Add(new SecurityFinding(
                        filePath,
                        lineNumber,
                        SecurityLevel.Medium,
                        "Array/collection access without bounds checking (CWE-125)",
                        access.ToString(),
                        "Bounds Checking"
                    ));
                }
            }
        }

        // Check for Substring operations without length validation
        var invocations = root.DescendantNodes().OfType<InvocationExpressionSyntax>()
            .Where(inv => inv.Expression.ToString().Contains("Substring"));

        foreach (var invocation in invocations)
        {
            var containingMethod = invocation.Ancestors().OfType<MethodDeclarationSyntax>().FirstOrDefault();
            if (containingMethod != null)
            {
                var methodText = containingMethod.ToString();
                if (!methodText.Contains(".Length") && !methodText.Contains("bounds"))
                {
                    var lineNumber = invocation.GetLocation().GetLineSpan().StartLinePosition.Line + 1;
                    _findings.Add(new SecurityFinding(
                        filePath,
                        lineNumber,
                        SecurityLevel.Medium,
                        "String substring operation without length validation (CWE-125)",
                        invocation.ToString(),
                        "Bounds Checking"
                    ));
                }
            }
        }
    }

    private async Task CheckNullPointerDereference(string filePath, SyntaxNode root)
    {
        // Check for potential null reference access
        var memberAccess = root.DescendantNodes().OfType<MemberAccessExpressionSyntax>();

        foreach (var access in memberAccess)
        {
            var accessString = access.ToString();
            
            // Look for method calls that could throw NullReferenceException
            if ((accessString.Contains(".ToString()") || 
                 accessString.Contains(".GetHashCode()") || 
                 accessString.Contains(".Equals(")) &&
                !accessString.Contains("?") && // Null-conditional operator
                !accessString.Contains("??")) // Null-coalescing operator
            {
                // Check if there's no null check in the surrounding code
                var containingStatement = access.Ancestors().OfType<StatementSyntax>().FirstOrDefault();
                if (containingStatement != null)
                {
                    var statementText = containingStatement.ToString();
                    if (!statementText.Contains("!= null") && !statementText.Contains("is not null"))
                    {
                        var lineNumber = access.GetLocation().GetLineSpan().StartLinePosition.Line + 1;
                        _findings.Add(new SecurityFinding(
                            filePath,
                            lineNumber,
                            SecurityLevel.Medium,
                            "Potential null reference access without null check (CWE-476)",
                            access.ToString(),
                            "Null Reference"
                        ));
                    }
                }
            }
        }
    }

    private async Task CheckImproperAuthentication(string filePath, SyntaxNode root)
    {
        // Check for authentication cookies without secure settings
        var invocations = root.DescendantNodes().OfType<InvocationExpressionSyntax>();

        foreach (var invocation in invocations)
        {
            var invText = invocation.ToString();
            if (invText.Contains("SetAuthCookie") && invText.Contains("false"))
            {
                var lineNumber = invocation.GetLocation().GetLineSpan().StartLinePosition.Line + 1;
                _findings.Add(new SecurityFinding(
                    filePath,
                    lineNumber,
                    SecurityLevel.High,
                    "Authentication cookie created without secure settings (CWE-287)",
                    invocation.ToString(),
                    "Authentication"
                ));
            }

            // Check for authentication configuration without HTTPS requirement
            if (invText.Contains("cookieAuthentication") || invText.Contains("CookieAuthentication"))
            {
                var containingMethod = invocation.Ancestors().OfType<MethodDeclarationSyntax>().FirstOrDefault();
                if (containingMethod != null)
                {
                    var methodText = containingMethod.ToString().ToLower();
                    if (!methodText.Contains("requirehttps") || methodText.Contains("requirehttps = false"))
                    {
                        var lineNumber = invocation.GetLocation().GetLineSpan().StartLinePosition.Line + 1;
                        _findings.Add(new SecurityFinding(
                            filePath,
                            lineNumber,
                            SecurityLevel.Medium,
                            "Authentication configuration without HTTPS requirement (CWE-287)",
                            invocation.ToString(),
                            "Authentication"
                        ));
                    }
                }
            }
        }
    }

    private async Task CheckIncorrectDefaultPermissions(string filePath, SyntaxNode root)
    {
        // Check for file/directory creation without explicit permission setting
        var invocations = root.DescendantNodes().OfType<InvocationExpressionSyntax>();

        foreach (var invocation in invocations)
        {
            var invText = invocation.ToString();
            if (invText.Contains("File.Create") || invText.Contains("Directory.CreateDirectory"))
            {
                var containingMethod = invocation.Ancestors().OfType<MethodDeclarationSyntax>().FirstOrDefault();
                if (containingMethod != null)
                {
                    var methodText = containingMethod.ToString();
                    if (!methodText.Contains("FileMode") && !methodText.Contains("FilePermissions") && 
                        !methodText.Contains("UnixFileMode"))
                    {
                        var lineNumber = invocation.GetLocation().GetLineSpan().StartLinePosition.Line + 1;
                        _findings.Add(new SecurityFinding(
                            filePath,
                            lineNumber,
                            SecurityLevel.Medium,
                            "File/directory creation without explicit permission settings (CWE-276)",
                            invocation.ToString(),
                            "File Permissions"
                        ));
                    }
                }
            }
        }
    }

    private async Task CheckInsufficientlyProtectedCredentials(string filePath, SyntaxNode root)
    {
        // Check for passwords stored or transmitted in plain text
        var assignments = root.DescendantNodes().OfType<AssignmentExpressionSyntax>();

        foreach (var assignment in assignments)
        {
            var assignText = assignment.ToString().ToLower();
            if ((assignText.Contains("password") || assignText.Contains("credential")) &&
                !assignText.Contains("hash") && !assignText.Contains("encrypt") && 
                !assignText.Contains("bcrypt") && !assignText.Contains("pbkdf2"))
            {
                var lineNumber = assignment.GetLocation().GetLineSpan().StartLinePosition.Line + 1;
                _findings.Add(new SecurityFinding(
                    filePath,
                    lineNumber,
                    SecurityLevel.High,
                    "Credentials stored without proper protection (CWE-522)",
                    assignment.ToString(),
                    "Credential Protection"
                ));
            }
        }

        // Check for password transmission without encryption
        var invocations = root.DescendantNodes().OfType<InvocationExpressionSyntax>();
        foreach (var invocation in invocations)
        {
            var invText = invocation.ToString().ToLower();
            if ((invText.Contains("send") || invText.Contains("post") || invText.Contains("put")) &&
                invText.Contains("password") && !invText.Contains("https"))
            {
                var lineNumber = invocation.GetLocation().GetLineSpan().StartLinePosition.Line + 1;
                _findings.Add(new SecurityFinding(
                    filePath,
                    lineNumber,
                    SecurityLevel.High,
                    "Password transmission without encryption protection (CWE-522)",
                    invocation.ToString(),
                    "Credential Protection"
                ));
            }
        }
    }

    private async Task CheckIncorrectPermissionAssignment(string filePath, SyntaxNode root)
    {
        // Check for overly permissive file/directory permissions
        var memberAccess = root.DescendantNodes().OfType<MemberAccessExpressionSyntax>();

        foreach (var access in memberAccess)
        {
            var accessString = access.ToString();
            if (accessString.Contains("FilePermissions.All") || 
                accessString.Contains("UnixFileMode.ReadWriteExecute") ||
                accessString.Contains("FileAccess.ReadWrite") && accessString.Contains("FileShare.ReadWrite"))
            {
                var lineNumber = access.GetLocation().GetLineSpan().StartLinePosition.Line + 1;
                _findings.Add(new SecurityFinding(
                    filePath,
                    lineNumber,
                    SecurityLevel.Medium,
                    "Overly permissive file permissions assigned (CWE-732)",
                    access.ToString(),
                    "Permission Assignment"
                ));
            }
        }
    }

    private async Task CheckXmlExternalEntity(string filePath, SyntaxNode root)
    {
        // Check for XML processing without XXE protection
        var objectCreations = root.DescendantNodes().OfType<ObjectCreationExpressionSyntax>();

        foreach (var creation in objectCreations)
        {
            var creationType = creation.Type.ToString();
            if (creationType.Contains("XmlDocument") || creationType.Contains("XmlTextReader") || 
                creationType.Contains("XslCompiledTransform"))
            {
                var containingMethod = creation.Ancestors().OfType<MethodDeclarationSyntax>().FirstOrDefault();
                if (containingMethod != null)
                {
                    var methodText = containingMethod.ToString();
                    if (!methodText.Contains("DtdProcessing.Prohibit") && 
                        !methodText.Contains("XmlResolver = null") &&
                        !methodText.Contains("ProhibitDtd = true"))
                    {
                        var lineNumber = creation.GetLocation().GetLineSpan().StartLinePosition.Line + 1;
                        _findings.Add(new SecurityFinding(
                            filePath,
                            lineNumber,
                            SecurityLevel.High,
                            "XML processing without XXE protection (CWE-611)",
                            creation.ToString(),
                            "XML Security"
                        ));
                    }
                }
            }
        }

        // Check for DTD processing enabled
        var assignments = root.DescendantNodes().OfType<AssignmentExpressionSyntax>();
        foreach (var assignment in assignments)
        {
            var assignText = assignment.ToString();
            if (assignText.Contains("DtdProcessing") && assignText.Contains("DtdProcessing.Parse"))
            {
                var lineNumber = assignment.GetLocation().GetLineSpan().StartLinePosition.Line + 1;
                _findings.Add(new SecurityFinding(
                    filePath,
                    lineNumber,
                    SecurityLevel.High,
                    "DTD processing enabled - XXE vulnerability risk (CWE-611)",
                    assignment.ToString(),
                    "XML Security"
                ));
            }
        }
    }

    private async Task CheckCommandInjection(string filePath, SyntaxNode root)
    {
        // Check for command injection via ProcessStartInfo
        var objectCreations = root.DescendantNodes().OfType<ObjectCreationExpressionSyntax>()
            .Where(obj => obj.Type.ToString().Contains("ProcessStartInfo"));

        foreach (var creation in objectCreations)
        {
            var containingMethod = creation.Ancestors().OfType<MethodDeclarationSyntax>().FirstOrDefault();
            if (containingMethod != null)
            {
                var methodText = containingMethod.ToString();
                if (methodText.Contains("Arguments") && methodText.Contains("+"))
                {
                    var lineNumber = creation.GetLocation().GetLineSpan().StartLinePosition.Line + 1;
                    _findings.Add(new SecurityFinding(
                        filePath,
                        lineNumber,
                        SecurityLevel.High,
                        "Command arguments constructed via concatenation (CWE-77)",
                        creation.ToString(),
                        "Command Injection"
                    ));
                }
            }
        }

        // Check for Process.Start with dynamic parameters
        var invocations = root.DescendantNodes().OfType<InvocationExpressionSyntax>()
            .Where(inv => inv.Expression.ToString().Contains("Process.Start"));

        foreach (var invocation in invocations)
        {
            var invText = invocation.ToString();
            if (invText.Contains("+") || invText.Contains("$\"") || invText.Contains("string.Format"))
            {
                var lineNumber = invocation.GetLocation().GetLineSpan().StartLinePosition.Line + 1;
                _findings.Add(new SecurityFinding(
                    filePath,
                    lineNumber,
                    SecurityLevel.High,
                    "Process execution with dynamic parameters (CWE-77)",
                    invocation.ToString(),
                    "Command Injection"
                ));
            }
        }
    }

    private async Task CheckMissingAuthenticationCritical(string filePath, SyntaxNode root)
    {
        // Check for critical operations without authentication - exclude test methods and infrastructure methods
        var methods = root.DescendantNodes().OfType<MethodDeclarationSyntax>();

        foreach (var method in methods)
        {
            var methodName = method.Identifier.ValueText.ToLower();
            
            // Skip test methods and infrastructure methods
            if (methodName.Contains("test") || methodName.Contains("should") || 
                filePath.ToLower().Contains("test") || filePath.Contains("Repository") || 
                filePath.Contains("Service") || filePath.Contains("Infrastructure"))
                continue;
                
            var hasAuthorization = method.AttributeLists
                .SelectMany(list => list.Attributes)
                .Any(attr => attr.Name.ToString().Contains("Authorize"));

            var hasControllerAuthorization = false;
            var controllerClass = method.Ancestors().OfType<ClassDeclarationSyntax>().FirstOrDefault();
            if (controllerClass != null)
            {
                hasControllerAuthorization = controllerClass.AttributeLists
                    .SelectMany(list => list.Attributes)
                    .Any(attr => attr.Name.ToString().Contains("Authorize"));
            }

            // Check for critical operations only in controllers
            if ((methodName.Contains("delete") || methodName.Contains("remove") || 
                 methodName.Contains("admin") || methodName.Contains("configure") ||
                 methodName.Contains("reset") || methodName.Contains("change")) && 
                !hasAuthorization && !hasControllerAuthorization &&
                controllerClass != null && controllerClass.Identifier.ValueText.EndsWith("Controller"))
            {
                var lineNumber = method.GetLocation().GetLineSpan().StartLinePosition.Line + 1;
                _findings.Add(new SecurityFinding(
                    filePath,
                    lineNumber,
                    SecurityLevel.High,
                    $"Critical function '{method.Identifier.ValueText}' lacks authentication (CWE-306)",
                    method.Identifier.ValueText,
                    "Missing Authentication"
                ));
            }
        }
    }

    private async Task CheckDeserializationUntrustedData(string filePath, SyntaxNode root)
    {
        // Check for unsafe deserialization
        var invocations = root.DescendantNodes().OfType<InvocationExpressionSyntax>();

        foreach (var invocation in invocations)
        {
            var invText = invocation.ToString();
            
            // Check for potentially unsafe deserialization methods
            if (invText.Contains("JsonConvert.DeserializeObject") || 
                invText.Contains("BinaryFormatter") ||
                invText.Contains("SoapFormatter") ||
                invText.Contains("XmlSerializer") && invText.Contains("Deserialize"))
            {
                var containingMethod = invocation.Ancestors().OfType<MethodDeclarationSyntax>().FirstOrDefault();
                if (containingMethod != null)
                {
                    var methodText = containingMethod.ToString();
                    // Check if data comes from user input or external source
                    if (methodText.Contains("Request.") || methodText.Contains("HttpContext") ||
                        methodText.Contains("Stream") || methodText.Contains("byte[]"))
                    {
                        var lineNumber = invocation.GetLocation().GetLineSpan().StartLinePosition.Line + 1;
                        _findings.Add(new SecurityFinding(
                            filePath,
                            lineNumber,
                            SecurityLevel.High,
                            "Deserialization of potentially untrusted data (CWE-502)",
                            invocation.ToString(),
                            "Deserialization"
                        ));
                    }
                }
            }
        }

        // Check for BinaryFormatter usage (always unsafe)
        var memberAccess = root.DescendantNodes().OfType<MemberAccessExpressionSyntax>();
        foreach (var access in memberAccess)
        {
            if (access.ToString().Contains("BinaryFormatter") || access.ToString().Contains("SoapFormatter"))
            {
                var lineNumber = access.GetLocation().GetLineSpan().StartLinePosition.Line + 1;
                _findings.Add(new SecurityFinding(
                    filePath,
                    lineNumber,
                    SecurityLevel.Critical,
                    "Use of unsafe deserialization formatter (CWE-502)",
                    access.ToString(),
                    "Deserialization"
                ));
            }
        }
    }

    private async Task CheckImproperPrivilegeManagement(string filePath, SyntaxNode root)
    {
        // Check for privilege escalation operations
        var invocations = root.DescendantNodes().OfType<InvocationExpressionSyntax>();

        foreach (var invocation in invocations)
        {
            var invText = invocation.ToString();
            if (invText.Contains("WindowsIdentity.Impersonate") || 
                invText.Contains("RunAs") ||
                invText.Contains("SetThreadToken") ||
                invText.Contains("ImpersonateLoggedOnUser"))
            {
                var lineNumber = invocation.GetLocation().GetLineSpan().StartLinePosition.Line + 1;
                _findings.Add(new SecurityFinding(
                    filePath,
                    lineNumber,
                    SecurityLevel.High,
                    "Privilege escalation operation detected (CWE-269)",
                    invocation.ToString(),
                    "Privilege Management"
                ));
            }
        }

        // Check for unsafe privilege assignments
        var memberAccess = root.DescendantNodes().OfType<MemberAccessExpressionSyntax>();
        foreach (var access in memberAccess)
        {
            var accessString = access.ToString();
            if (accessString.Contains("WindowsPrincipal") || accessString.Contains("ClaimsPrincipal"))
            {
                var containingMethod = access.Ancestors().OfType<MethodDeclarationSyntax>().FirstOrDefault();
                if (containingMethod != null)
                {
                    var methodText = containingMethod.ToString();
                    if (!methodText.Contains("IsInRole") && !methodText.Contains("HasClaim"))
                    {
                        var lineNumber = access.GetLocation().GetLineSpan().StartLinePosition.Line + 1;
                        _findings.Add(new SecurityFinding(
                            filePath,
                            lineNumber,
                            SecurityLevel.Medium,
                            "Principal usage without proper role/claim validation (CWE-269)",
                            access.ToString(),
                            "Privilege Management"
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
                            { "owaspMapping", GetOwaspMappingWithUrl(finding.Description) },
                            { "cweMapping", GetCweMappingWithUrl(finding.Description) },
                            { "referenceUrls", GetReferenceUrls(finding.Description) }
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

    private object GetOwaspMappingWithUrl(string description)
    {
        var mapping = new Dictionary<string, object>();
        
        if (description.Contains("OWASP A01"))
        {
            mapping["category"] = "A01:2021 - Broken Access Control";
            mapping["url"] = "https://owasp.org/Top10/A01_2021-Broken_Access_Control/";
        }
        else if (description.Contains("OWASP A02"))
        {
            mapping["category"] = "A02:2021 - Cryptographic Failures";
            mapping["url"] = "https://owasp.org/Top10/A02_2021-Cryptographic_Failures/";
        }
        else if (description.Contains("OWASP A03"))
        {
            mapping["category"] = "A03:2021 - Injection";
            mapping["url"] = "https://owasp.org/Top10/A03_2021-Injection/";
        }
        else if (description.Contains("OWASP A05"))
        {
            mapping["category"] = "A05:2021 - Security Misconfiguration";
            mapping["url"] = "https://owasp.org/Top10/A05_2021-Security_Misconfiguration/";
        }
        else if (description.Contains("OWASP A06"))
        {
            mapping["category"] = "A06:2021 - Vulnerable and Outdated Components";
            mapping["url"] = "https://owasp.org/Top10/A06_2021-Vulnerable_and_Outdated_Components/";
        }
        else if (description.Contains("OWASP A07"))
        {
            mapping["category"] = "A07:2021 - Identification and Authentication Failures";
            mapping["url"] = "https://owasp.org/Top10/A07_2021-Identification_and_Authentication_Failures/";
        }
        else if (description.Contains("OWASP A08"))
        {
            mapping["category"] = "A08:2021 - Software and Data Integrity Failures";
            mapping["url"] = "https://owasp.org/Top10/A08_2021-Software_and_Data_Integrity_Failures/";
        }
        else if (description.Contains("OWASP A09"))
        {
            mapping["category"] = "A09:2021 - Security Logging and Monitoring Failures";
            mapping["url"] = "https://owasp.org/Top10/A09_2021-Security_Logging_and_Monitoring_Failures/";
        }
        else if (description.Contains("OWASP A10"))
        {
            mapping["category"] = "A10:2021 - Server-Side Request Forgery";
            mapping["url"] = "https://owasp.org/Top10/A10_2021-Server-Side_Request_Forgery_%28SSRF%29/";
        }
        else
        {
            mapping["category"] = "Multiple OWASP categories may apply";
            mapping["url"] = "https://owasp.org/Top10/";
        }
        
        return mapping;
    }

    private object GetCweMappingWithUrl(string description)
    {
        var mapping = new Dictionary<string, object>();
        
        // Enhanced CWE mapping with all CWE Top 25 2024 patterns
        if (description.Contains("CWE-79"))
        {
            mapping["id"] = "CWE-79";
            mapping["name"] = "Cross-site Scripting";
            mapping["url"] = "https://cwe.mitre.org/data/definitions/79.html";
        }
        else if (description.Contains("CWE-89"))
        {
            mapping["id"] = "CWE-89";
            mapping["name"] = "SQL Injection";
            mapping["url"] = "https://cwe.mitre.org/data/definitions/89.html";
        }
        else if (description.Contains("CWE-20"))
        {
            mapping["id"] = "CWE-20";
            mapping["name"] = "Improper Input Validation";
            mapping["url"] = "https://cwe.mitre.org/data/definitions/20.html";
        }
        else if (description.Contains("CWE-78"))
        {
            mapping["id"] = "CWE-78";
            mapping["name"] = "OS Command Injection";
            mapping["url"] = "https://cwe.mitre.org/data/definitions/78.html";
        }
        else if (description.Contains("CWE-190"))
        {
            mapping["id"] = "CWE-190";
            mapping["name"] = "Integer Overflow or Wraparound";
            mapping["url"] = "https://cwe.mitre.org/data/definitions/190.html";
        }
        else if (description.Contains("CWE-352"))
        {
            mapping["id"] = "CWE-352";
            mapping["name"] = "Cross-Site Request Forgery (CSRF)";
            mapping["url"] = "https://cwe.mitre.org/data/definitions/352.html";
        }
        else if (description.Contains("CWE-22"))
        {
            mapping["id"] = "CWE-22";
            mapping["name"] = "Path Traversal";
            mapping["url"] = "https://cwe.mitre.org/data/definitions/22.html";
        }
        else if (description.Contains("CWE-125"))
        {
            mapping["id"] = "CWE-125";
            mapping["name"] = "Out-of-bounds Read";
            mapping["url"] = "https://cwe.mitre.org/data/definitions/125.html";
        }
        else if (description.Contains("CWE-434"))
        {
            mapping["id"] = "CWE-434";
            mapping["name"] = "Unrestricted Upload of File with Dangerous Type";
            mapping["url"] = "https://cwe.mitre.org/data/definitions/434.html";
        }
        else if (description.Contains("CWE-862"))
        {
            mapping["id"] = "CWE-862";
            mapping["name"] = "Missing Authorization";
            mapping["url"] = "https://cwe.mitre.org/data/definitions/862.html";
        }
        else if (description.Contains("CWE-476"))
        {
            mapping["id"] = "CWE-476";
            mapping["name"] = "NULL Pointer Dereference";
            mapping["url"] = "https://cwe.mitre.org/data/definitions/476.html";
        }
        else if (description.Contains("CWE-787"))
        {
            mapping["id"] = "CWE-787";
            mapping["name"] = "Out-of-bounds Write";
            mapping["url"] = "https://cwe.mitre.org/data/definitions/787.html";
        }
        else if (description.Contains("CWE-94"))
        {
            mapping["id"] = "CWE-94";
            mapping["name"] = "Code Injection";
            mapping["url"] = "https://cwe.mitre.org/data/definitions/94.html";
        }
        else if (description.Contains("CWE-276"))
        {
            mapping["id"] = "CWE-276";
            mapping["name"] = "Incorrect Default Permissions";
            mapping["url"] = "https://cwe.mitre.org/data/definitions/276.html";
        }
        else if (description.Contains("CWE-200"))
        {
            mapping["id"] = "CWE-200";
            mapping["name"] = "Information Exposure";
            mapping["url"] = "https://cwe.mitre.org/data/definitions/200.html";
        }
        else if (description.Contains("CWE-522"))
        {
            mapping["id"] = "CWE-522";
            mapping["name"] = "Insufficiently Protected Credentials";
            mapping["url"] = "https://cwe.mitre.org/data/definitions/522.html";
        }
        else if (description.Contains("CWE-732"))
        {
            mapping["id"] = "CWE-732";
            mapping["name"] = "Incorrect Permission Assignment for Critical Resource";
            mapping["url"] = "https://cwe.mitre.org/data/definitions/732.html";
        }
        else if (description.Contains("CWE-611"))
        {
            mapping["id"] = "CWE-611";
            mapping["name"] = "Improper Restriction of XML External Entity Reference";
            mapping["url"] = "https://cwe.mitre.org/data/definitions/611.html";
        }
        else if (description.Contains("CWE-77"))
        {
            mapping["id"] = "CWE-77";
            mapping["name"] = "Command Injection";
            mapping["url"] = "https://cwe.mitre.org/data/definitions/77.html";
        }
        else if (description.Contains("CWE-306"))
        {
            mapping["id"] = "CWE-306";
            mapping["name"] = "Missing Authentication for Critical Function";
            mapping["url"] = "https://cwe.mitre.org/data/definitions/306.html";
        }
        else if (description.Contains("CWE-502"))
        {
            mapping["id"] = "CWE-502";
            mapping["name"] = "Deserialization of Untrusted Data";
            mapping["url"] = "https://cwe.mitre.org/data/definitions/502.html";
        }
        else if (description.Contains("CWE-269"))
        {
            mapping["id"] = "CWE-269";
            mapping["name"] = "Improper Privilege Management";
            mapping["url"] = "https://cwe.mitre.org/data/definitions/269.html";
        }
        else if (description.Contains("CWE-287"))
        {
            mapping["id"] = "CWE-287";
            mapping["name"] = "Improper Authentication";
            mapping["url"] = "https://cwe.mitre.org/data/definitions/287.html";
        }
        else if (description.Contains("CWE-209"))
        {
            mapping["id"] = "CWE-209";
            mapping["name"] = "Information Exposure Through Error Messages";
            mapping["url"] = "https://cwe.mitre.org/data/definitions/209.html";
        }
        else if (description.Contains("CWE-321"))
        {
            mapping["id"] = "CWE-321";
            mapping["name"] = "Use of Hard-coded Cryptographic Key";
            mapping["url"] = "https://cwe.mitre.org/data/definitions/321.html";
        }
        else if (description.Contains("CWE-327"))
        {
            mapping["id"] = "CWE-327";
            mapping["name"] = "Use of a Broken or Risky Cryptographic Algorithm";
            mapping["url"] = "https://cwe.mitre.org/data/definitions/327.html";
        }
        else if (description.Contains("CWE-338"))
        {
            mapping["id"] = "CWE-338";
            mapping["name"] = "Use of Cryptographically Weak Pseudo-Random Number Generator";
            mapping["url"] = "https://cwe.mitre.org/data/definitions/338.html";
        }
        else if (description.Contains("CWE-521"))
        {
            mapping["id"] = "CWE-521";
            mapping["name"] = "Weak Password Requirements";
            mapping["url"] = "https://cwe.mitre.org/data/definitions/521.html";
        }
        else if (description.Contains("CWE-798"))
        {
            mapping["id"] = "CWE-798";
            mapping["name"] = "Use of Hard-coded Credentials";
            mapping["url"] = "https://cwe.mitre.org/data/definitions/798.html";
        }
        else if (description.Contains("CWE-918"))
        {
            mapping["id"] = "CWE-918";
            mapping["name"] = "Server-Side Request Forgery (SSRF)";
            mapping["url"] = "https://cwe.mitre.org/data/definitions/918.html";
        }
        else if (description.Contains("CWE-942"))
        {
            mapping["id"] = "CWE-942";
            mapping["name"] = "Overly Permissive Cross-domain Whitelist";
            mapping["url"] = "https://cwe.mitre.org/data/definitions/942.html";
        }
        else if (description.Contains("CWE-208"))
        {
            mapping["id"] = "CWE-208";
            mapping["name"] = "Information Exposure Through Timing Discrepancy";
            mapping["url"] = "https://cwe.mitre.org/data/definitions/208.html";
        }
        else
        {
            mapping["id"] = "Multiple";
            mapping["name"] = "Multiple CWE categories may apply";
            mapping["url"] = "https://cwe.mitre.org/top25/archive/2024/2024_cwe_top25.html";
        }
        
        return mapping;
    }

    private List<object> GetReferenceUrls(string description)
    {
        var urls = new List<object>();
        
        // Add CWE Top 25 2024 reference
        urls.Add(new Dictionary<string, string>
        {
            { "name", "CWE Top 25 Most Dangerous Software Weaknesses 2024" },
            { "url", "https://cwe.mitre.org/top25/archive/2024/2024_cwe_top25.html" }
        });
        
        // Add OWASP Top 10 reference if applicable
        if (description.Contains("OWASP"))
        {
            urls.Add(new Dictionary<string, string>
            {
                { "name", "OWASP Top 10 2021" },
                { "url", "https://owasp.org/Top10/" }
            });
        }
        
        // Add specific CWE reference based on the vulnerability type
        if (description.Contains("CWE-"))
        {
            var cweMatch = System.Text.RegularExpressions.Regex.Match(description, @"CWE-(\d+)");
            if (cweMatch.Success)
            {
                var cweId = cweMatch.Groups[1].Value;
                urls.Add(new Dictionary<string, string>
                {
                    { "name", $"CWE-{cweId} Details" },
                    { "url", $"https://cwe.mitre.org/data/definitions/{cweId}.html" }
                });
            }
        }
        
        // Add general security resources
        urls.Add(new Dictionary<string, string>
        {
            { "name", "NIST Cybersecurity Framework" },
            { "url", "https://www.nist.gov/cyberframework" }
        });
        
        return urls;
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