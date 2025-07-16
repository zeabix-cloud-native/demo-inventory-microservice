using System.CommandLine;
using System.Diagnostics;
using System.Text.Json;

namespace DemoInventory.Tools.AICodeValidator;

/// <summary>
/// Main AI Code Validator that orchestrates all validation tools
/// </summary>
public class Program
{
    public static async Task<int> Main(string[] args)
    {
        var pathOption = new Option<string>(
            name: "--path",
            description: "Path to validate (file or directory)",
            getDefaultValue: () => Directory.GetCurrentDirectory());

        var verboseOption = new Option<bool>(
            name: "--verbose",
            description: "Enable verbose output");

        var reportOption = new Option<string?>(
            name: "--report",
            description: "Generate JSON report file");

        var skipOption = new Option<string[]>(
            name: "--skip",
            description: "Skip specific validation types (static|security|architecture)")
        { AllowMultipleArgumentsPerToken = true };

        var ctrfOption = new Option<string?>(
            name: "--ctrf",
            description: "Generate CTRF (Common Test Report Format) JSON report file for security scan");

        var rootCommand = new RootCommand("AI Code Validator - Comprehensive code quality validation")
        {
            pathOption,
            verboseOption,
            reportOption,
            skipOption,
            ctrfOption
        };

        rootCommand.SetHandler(async (path, verbose, report, skip, ctrf) =>
        {
            var validator = new AICodeValidator(verbose);
            await validator.ValidateAsync(path, report, skip, ctrf);
        }, pathOption, verboseOption, reportOption, skipOption, ctrfOption);

        return await rootCommand.InvokeAsync(args);
    }
}

/// <summary>
/// Orchestrates all validation tools and provides comprehensive analysis
/// </summary>
public class AICodeValidator
{
    private readonly bool _verbose;
    private readonly List<ValidationResult> _results = new();

    public AICodeValidator(bool verbose = false)
    {
        _verbose = verbose;
    }

    public async Task ValidateAsync(string path, string? reportPath, string[] skipValidations, string? ctrfPath = null)
    {
        Console.WriteLine("🤖 AI Code Validator");
        Console.WriteLine("====================");
        Console.WriteLine($"Validating: {path}");
        Console.WriteLine($"Timestamp: {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC");
        Console.WriteLine();

        var skipSet = new HashSet<string>(skipValidations, StringComparer.OrdinalIgnoreCase);
        var toolsPath = GetToolsPath();

        // Run Static Analysis
        if (!skipSet.Contains("static"))
        {
            Console.WriteLine("🔍 Running Static Analysis...");
            var staticResult = await RunToolAsync(
                Path.Combine(toolsPath, "DemoInventory.Tools.StaticAnalysis"),
                $"--path \"{path}\" {(_verbose ? "--verbose" : "")}");
            _results.Add(new ValidationResult("Static Analysis", staticResult));
        }

        // Run Security Scan
        if (!skipSet.Contains("security"))
        {
            Console.WriteLine("🔒 Running Security Scan...");
            var ctrfArgs = !string.IsNullOrEmpty(ctrfPath) ? $"--ctrf \"{ctrfPath}\"" : "";
            var securityResult = await RunToolAsync(
                Path.Combine(toolsPath, "DemoInventory.Tools.SecurityScan"),
                $"--path \"{path}\" {(_verbose ? "--verbose" : "")} {ctrfArgs}".Trim());
            _results.Add(new ValidationResult("Security Scan", securityResult));
        }

        // Run Architecture Validation
        if (!skipSet.Contains("architecture"))
        {
            Console.WriteLine("🏛️  Running Architecture Validation...");
            var backendPath = Path.Combine(path, "backend", "src");
            if (!Directory.Exists(backendPath))
            {
                backendPath = path; // Use the provided path if backend/src doesn't exist
            }
            
            var archResult = await RunToolAsync(
                Path.Combine(toolsPath, "DemoInventory.Tools.ArchitectureValidation"),
                $"--path \"{backendPath}\" {(_verbose ? "--verbose" : "")}");
            _results.Add(new ValidationResult("Architecture Validation", archResult));
        }

        // Generate comprehensive summary
        GenerateSummary();

        // Generate JSON report if requested
        if (!string.IsNullOrEmpty(reportPath))
        {
            await GenerateJsonReport(reportPath);
        }

        // Provide recommendations
        GenerateRecommendations();
    }

    private async Task<ToolResult> RunToolAsync(string toolPath, string arguments)
    {
        try
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = "dotnet",
                Arguments = $"run --project \"{toolPath}\" -- {arguments}",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };

            var stopwatch = Stopwatch.StartNew();
            
            using var process = Process.Start(startInfo);
            if (process == null)
            {
                return new ToolResult(false, "Failed to start process", "", TimeSpan.Zero);
            }

            var output = await process.StandardOutput.ReadToEndAsync();
            var error = await process.StandardError.ReadToEndAsync();
            await process.WaitForExitAsync();
            
            stopwatch.Stop();

            var success = process.ExitCode == 0;
            return new ToolResult(success, output, error, stopwatch.Elapsed);
        }
        catch (Exception ex)
        {
            return new ToolResult(false, "", ex.Message, TimeSpan.Zero);
        }
    }

    private void GenerateSummary()
    {
        Console.WriteLine();
        Console.WriteLine("📊 Comprehensive Validation Summary");
        Console.WriteLine("===================================");

        var totalErrors = 0;
        var totalWarnings = 0;
        var allPassed = true;

        foreach (var result in _results)
        {
            var icon = result.ToolResult.Success ? "✅" : "❌";
            var duration = result.ToolResult.Duration.TotalMilliseconds;
            
            Console.WriteLine($"{icon} {result.ValidationName} - {duration:F0}ms");
            
            if (!result.ToolResult.Success)
            {
                allPassed = false;
                totalErrors++;
                
                if (_verbose && !string.IsNullOrEmpty(result.ToolResult.ErrorOutput))
                {
                    Console.WriteLine($"   Error: {result.ToolResult.ErrorOutput}");
                }
            }

            // Extract metrics from output
            var metrics = ExtractMetrics(result.ToolResult.Output);
            if (metrics.Any())
            {
                foreach (var metric in metrics)
                {
                    Console.WriteLine($"   {metric}");
                    if (metric.Contains("issues") || metric.Contains("violations") || metric.Contains("findings"))
                    {
                        totalWarnings++;
                    }
                }
            }
        }

        Console.WriteLine();
        Console.WriteLine($"Overall Status: {(allPassed ? "✅ PASSED" : "❌ FAILED")}");
        Console.WriteLine($"Tools Run: {_results.Count}");
        Console.WriteLine($"Total Duration: {_results.Sum(r => r.ToolResult.Duration.TotalMilliseconds):F0}ms");
        Console.WriteLine();

        if (!allPassed)
        {
            Environment.ExitCode = 1;
        }
    }

    private async Task GenerateJsonReport(string reportPath)
    {
        var report = new ValidationReport
        {
            Timestamp = DateTime.UtcNow,
            Summary = new ValidationSummary
            {
                TotalTools = _results.Count,
                PassedTools = _results.Count(r => r.ToolResult.Success),
                FailedTools = _results.Count(r => !r.ToolResult.Success),
                TotalDuration = _results.Sum(r => r.ToolResult.Duration.TotalMilliseconds)
            },
            Results = _results.Select(r => new ValidationReportItem
            {
                ValidationName = r.ValidationName,
                Success = r.ToolResult.Success,
                Duration = r.ToolResult.Duration.TotalMilliseconds,
                Output = r.ToolResult.Output,
                Error = r.ToolResult.ErrorOutput,
                Metrics = ExtractMetrics(r.ToolResult.Output)
            }).ToList()
        };

        var json = JsonSerializer.Serialize(report, new JsonSerializerOptions 
        { 
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        await File.WriteAllTextAsync(reportPath, json);
        Console.WriteLine($"📄 JSON report generated: {reportPath}");
    }

    private void GenerateRecommendations()
    {
        Console.WriteLine("💡 Comprehensive Recommendations");
        Console.WriteLine("=================================");

        var failedValidations = _results.Where(r => !r.ToolResult.Success).ToList();
        
        if (!failedValidations.Any())
        {
            Console.WriteLine("🎉 Excellent! All validations passed. Your code quality is exceptional.");
            Console.WriteLine();
            Console.WriteLine("📈 Continue following these best practices:");
            Console.WriteLine("  • Maintain Clean Architecture boundaries and dependency flow");
            Console.WriteLine("  • Keep using parameterized queries and proper input validation");
            Console.WriteLine("  • Follow security best practices and OWASP guidelines");
            Console.WriteLine("  • Continue comprehensive testing and code reviews");
            Console.WriteLine("  • Keep methods focused, well-named, and properly documented");
            Console.WriteLine("  • Regular security scanning and dependency updates");
            Console.WriteLine();
            Console.WriteLine("🏆 Quality Achievements:");
            Console.WriteLine("  • No critical security vulnerabilities");
            Console.WriteLine("  • No architectural violations");
            Console.WriteLine("  • Good code complexity and maintainability");
            Console.WriteLine("  • Following coding standards and conventions");
            return;
        }

        Console.WriteLine("🔧 Priority-Based Action Plan:");
        Console.WriteLine();

        // Analyze severity across all tools
        var overallSeverity = DetermineOverallSeverity(failedValidations);
        ShowOverallSeverityGuidance(overallSeverity);

        foreach (var failed in failedValidations)
        {
            var recommendations = GetRecommendations(failed.ValidationName, failed.ToolResult.Output);
            if (recommendations.Any())
            {
                Console.WriteLine($"📌 {failed.ValidationName}:");
                foreach (var recommendation in recommendations)
                {
                    Console.WriteLine($"  {recommendation}");
                }
                Console.WriteLine();
            }
        }

        Console.WriteLine("📅 Implementation Timeline:");
        ShowImplementationTimeline(failedValidations);

        Console.WriteLine();
        Console.WriteLine("📚 Additional Resources:");
        Console.WriteLine("  • Code Verification Guide: docs/copilot/workshop/code-verification.md");
        Console.WriteLine("  • Clean Architecture Documentation: docs/ARCHITECTURE.md");
        Console.WriteLine("  • Security Best Practices: https://owasp.org/www-project-cheat-sheets/");
        Console.WriteLine("  • .NET Coding Standards: https://docs.microsoft.com/en-us/dotnet/csharp/fundamentals/coding-style/");
        Console.WriteLine("  • Entity Framework Security: https://docs.microsoft.com/en-us/ef/core/miscellaneous/security");
    }

    private string DetermineOverallSeverity(List<ValidationResult> failedValidations)
    {
        foreach (var validation in failedValidations)
        {
            var output = validation.ToolResult.Output;
            if (output.Contains("Critical:") && !output.Contains("Critical: 0"))
                return "CRITICAL";
        }
        
        foreach (var validation in failedValidations)
        {
            var output = validation.ToolResult.Output;
            if (output.Contains("High:") && !output.Contains("High:     0") && !output.Contains("High: 0"))
                return "HIGH";
        }
        
        foreach (var validation in failedValidations)
        {
            var output = validation.ToolResult.Output;
            if (output.Contains("Medium:") && !output.Contains("Medium:   0") && !output.Contains("Medium: 0"))
                return "MEDIUM";
        }
        
        return "LOW";
    }

    private void ShowOverallSeverityGuidance(string severity)
    {
        switch (severity)
        {
            case "CRITICAL":
                Console.WriteLine("🚨 CRITICAL ISSUES DETECTED - IMMEDIATE ACTION REQUIRED");
                Console.WriteLine("  ⏰ Timeline: Fix within hours, before any deployment");
                Console.WriteLine("  👥 Resources: All hands on deck, escalate to tech lead/architect");
                Console.WriteLine("  🛑 Deployment: BLOCKED until all critical issues resolved");
                Console.WriteLine("  🔍 Testing: Comprehensive security and architecture review required");
                break;
            case "HIGH":
                Console.WriteLine("🔴 HIGH PRIORITY ISSUES - Address Within 24-48 Hours");
                Console.WriteLine("  ⏰ Timeline: Resolve this sprint, prioritize over new features");
                Console.WriteLine("  👥 Resources: Senior developer review, pair programming recommended");
                Console.WriteLine("  🚀 Deployment: Can deploy with monitoring, fix before next release");
                Console.WriteLine("  🔍 Testing: Additional testing and security review recommended");
                break;
            case "MEDIUM":
                Console.WriteLine("🟡 MEDIUM PRIORITY ISSUES - Plan for Current Sprint");
                Console.WriteLine("  ⏰ Timeline: Address within current or next sprint");
                Console.WriteLine("  👥 Resources: Regular development workflow");
                Console.WriteLine("  🚀 Deployment: Safe to deploy with planned fixes");
                Console.WriteLine("  🔍 Testing: Standard testing procedures sufficient");
                break;
            case "LOW":
                Console.WriteLine("🔵 LOW PRIORITY ISSUES - Technical Debt Cleanup");
                Console.WriteLine("  ⏰ Timeline: Address during maintenance windows");
                Console.WriteLine("  👥 Resources: Junior developers, code review process");
                Console.WriteLine("  🚀 Deployment: No deployment blockers");
                Console.WriteLine("  🔍 Testing: Standard validation sufficient");
                break;
        }
        Console.WriteLine();
    }

    private void ShowImplementationTimeline(List<ValidationResult> failedValidations)
    {
        Console.WriteLine("Week 1 (Immediate):");
        foreach (var validation in failedValidations)
        {
            if (validation.ToolResult.Output.Contains("Critical:") && !validation.ToolResult.Output.Contains("Critical: 0"))
            {
                Console.WriteLine($"  🔴 {validation.ValidationName}: Address all critical issues");
            }
        }

        Console.WriteLine("Week 1-2 (This Sprint):");
        foreach (var validation in failedValidations)
        {
            if (validation.ToolResult.Output.Contains("High:") && !validation.ToolResult.Output.Contains("High:     0"))
            {
                Console.WriteLine($"  🟠 {validation.ValidationName}: Address high-priority issues");
            }
        }

        Console.WriteLine("Week 2-4 (Current Sprint):");
        foreach (var validation in failedValidations)
        {
            if (validation.ToolResult.Output.Contains("Medium:") && !validation.ToolResult.Output.Contains("Medium:   0"))
            {
                Console.WriteLine($"  🟡 {validation.ValidationName}: Address medium-priority issues");
            }
        }

        Console.WriteLine("Ongoing (Technical Debt):");
        foreach (var validation in failedValidations)
        {
            if (validation.ToolResult.Output.Contains("Low:") && !validation.ToolResult.Output.Contains("Low:      0"))
            {
                Console.WriteLine($"  🔵 {validation.ValidationName}: Address low-priority issues during maintenance");
            }
        }
    }

    private List<string> ExtractMetrics(string output)
    {
        var metrics = new List<string>();
        var lines = output.Split('\n', StringSplitOptions.RemoveEmptyEntries);

        foreach (var line in lines)
        {
            var trimmed = line.Trim();
            
            // Look for summary lines with numbers
            if (trimmed.Contains(":") && 
                (trimmed.Contains("issues") || trimmed.Contains("violations") || 
                 trimmed.Contains("findings") || trimmed.Contains("files") ||
                 trimmed.Contains("Total") || trimmed.Contains("Critical") ||
                 trimmed.Contains("High") || trimmed.Contains("Medium") || trimmed.Contains("Low")))
            {
                metrics.Add(trimmed);
            }
        }

        return metrics;
    }

    private List<string> GetRecommendations(string validationName, string output)
    {
        var recommendations = new List<string>();

        // Extract severity information from output
        var hasCritical = output.Contains("Critical:") && !output.Contains("Critical: 0");
        var hasHigh = output.Contains("High:") && !output.Contains("High:     0") && !output.Contains("High: 0");
        var hasMedium = output.Contains("Medium:") && !output.Contains("Medium:   0") && !output.Contains("Medium: 0");
        var hasLow = output.Contains("Low:") && !output.Contains("Low:      0") && !output.Contains("Low: 0");

        switch (validationName.ToLower())
        {
            case "static analysis":
                if (hasCritical)
                {
                    recommendations.Add("🔴 CRITICAL: Immediate refactoring required for maintainability");
                    recommendations.Add("Schedule emergency architecture review session");
                    recommendations.Add("Break down complex methods before continuing development");
                }
                if (hasHigh)
                {
                    recommendations.Add("🟠 HIGH PRIORITY: Address complexity and design issues this sprint");
                    recommendations.Add("Apply SOLID principles and extract methods/classes");
                    recommendations.Add("Consider pair programming for complex refactoring");
                }
                if (hasMedium)
                {
                    recommendations.Add("🟡 MEDIUM: Improve documentation and code structure");
                    recommendations.Add("Add XML documentation to public APIs for better maintainability");
                    recommendations.Add("Plan refactoring tasks for upcoming sprints");
                }
                if (hasLow)
                {
                    recommendations.Add("🔵 LOW: Polish naming conventions and minor improvements");
                    recommendations.Add("Address during regular code reviews and maintenance");
                }
                
                // Add specific recommendations based on content
                if (output.Contains("complexity"))
                    recommendations.Add("Use Extract Method pattern to reduce cyclomatic complexity");
                if (output.Contains("naming"))
                    recommendations.Add("Follow C# naming conventions: PascalCase for public, camelCase for private");
                if (output.Contains("documentation"))
                    recommendations.Add("Add comprehensive XML documentation for public APIs");
                if (output.Contains("parameters"))
                    recommendations.Add("Create parameter objects or use dependency injection to reduce parameter count");
                if (output.Contains("length"))
                    recommendations.Add("Keep methods under 50 lines and classes focused on single responsibility");
                break;

            case "security scan":
                if (hasCritical)
                {
                    recommendations.Add("🔴 CRITICAL SECURITY RISK: Fix SQL injection and secret exposure immediately");
                    recommendations.Add("All critical findings must be resolved before deployment");
                    recommendations.Add("Conduct immediate security audit and penetration testing");
                    recommendations.Add("Rotate any exposed secrets or credentials immediately");
                }
                if (hasHigh)
                {
                    recommendations.Add("🟠 HIGH SECURITY RISK: Address within 24-48 hours");
                    recommendations.Add("Implement proper input validation and sanitization");
                    recommendations.Add("Review and strengthen authentication/authorization");
                    recommendations.Add("Sanitize logging to prevent sensitive data exposure");
                }
                if (hasMedium)
                {
                    recommendations.Add("🟡 MEDIUM SECURITY RISK: Plan security improvements this sprint");
                    recommendations.Add("Add authorization attributes to unprotected endpoints");
                    recommendations.Add("Implement comprehensive input validation");
                    recommendations.Add("Improve error handling to prevent information disclosure");
                }
                if (hasLow)
                {
                    recommendations.Add("🔵 LOW SECURITY RISK: Security hardening improvements");
                    recommendations.Add("Follow OWASP best practices for secure coding");
                    recommendations.Add("Use IHttpClientFactory and proper resource management");
                }
                
                // Add specific security recommendations
                if (output.Contains("hardcoded"))
                    recommendations.Add("Move all secrets to environment variables or Azure Key Vault");
                if (output.Contains("injection"))
                    recommendations.Add("Use parameterized queries and Entity Framework exclusively");
                if (output.Contains("authorization"))
                    recommendations.Add("Add [Authorize] attributes and implement role-based access control");
                if (output.Contains("logging"))
                    recommendations.Add("Sanitize all logged data to remove PII and sensitive information");
                break;

            case "architecture validation":
                if (hasCritical)
                {
                    recommendations.Add("🔴 CRITICAL ARCHITECTURE VIOLATION: Clean Architecture boundaries broken");
                    recommendations.Add("Immediate architectural review and refactoring required");
                    recommendations.Add("Stop feature development until architecture is fixed");
                }
                if (hasHigh)
                {
                    recommendations.Add("🟠 HIGH PRIORITY: Fix layer dependency violations this sprint");
                    recommendations.Add("Move business logic from infrastructure to domain layer");
                    recommendations.Add("Ensure proper dependency injection and interface usage");
                }
                if (hasMedium)
                {
                    recommendations.Add("🟡 MEDIUM: Improve architectural consistency");
                    recommendations.Add("Use async/await pattern consistently in application services");
                    recommendations.Add("Review and strengthen domain model design");
                }
                if (hasLow)
                {
                    recommendations.Add("🔵 LOW: Minor architectural improvements");
                    recommendations.Add("Continue following Clean Architecture principles");
                }
                
                if (output.Contains("dependency"))
                    recommendations.Add("Review and fix layer dependency violations to maintain Clean Architecture");
                if (output.Contains("domain"))
                    recommendations.Add("Keep domain layer pure with no external dependencies");
                if (output.Contains("application"))
                    recommendations.Add("Use async/await pattern consistently in application services");
                if (output.Contains("infrastructure"))
                    recommendations.Add("Move business logic from infrastructure to domain layer");
                break;
        }

        if (!recommendations.Any())
        {
            recommendations.Add($"✅ {validationName} passed - continue following current practices");
            recommendations.Add("Regular code reviews help maintain quality standards");
        }

        return recommendations;
    }

    private string GetToolsPath()
    {
        var currentDir = Directory.GetCurrentDirectory();
        
        // Try to find tools directory
        var toolsPath = Path.Combine(currentDir, "tools");
        if (Directory.Exists(toolsPath))
            return toolsPath;
            
        // Try parent directory
        var parent = Directory.GetParent(currentDir);
        if (parent != null)
        {
            toolsPath = Path.Combine(parent.FullName, "tools");
            if (Directory.Exists(toolsPath))
                return toolsPath;
        }

        // Use current directory as fallback
        return currentDir;
    }
}

/// <summary>
/// Represents the result of running a validation tool
/// </summary>
public record ToolResult(bool Success, string Output, string ErrorOutput, TimeSpan Duration);

/// <summary>
/// Represents a validation result
/// </summary>
public record ValidationResult(string ValidationName, ToolResult ToolResult);

/// <summary>
/// JSON report structure
/// </summary>
public class ValidationReport
{
    public DateTime Timestamp { get; set; }
    public ValidationSummary Summary { get; set; } = new();
    public List<ValidationReportItem> Results { get; set; } = new();
}

public class ValidationSummary
{
    public int TotalTools { get; set; }
    public int PassedTools { get; set; }
    public int FailedTools { get; set; }
    public double TotalDuration { get; set; }
}

public class ValidationReportItem
{
    public string ValidationName { get; set; } = "";
    public bool Success { get; set; }
    public double Duration { get; set; }
    public string Output { get; set; } = "";
    public string Error { get; set; } = "";
    public List<string> Metrics { get; set; } = new();
}