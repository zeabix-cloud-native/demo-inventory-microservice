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

        var rootCommand = new RootCommand("AI Code Validator - Comprehensive code quality validation")
        {
            pathOption,
            verboseOption,
            reportOption,
            skipOption
        };

        rootCommand.SetHandler(async (path, verbose, report, skip) =>
        {
            var validator = new AICodeValidator(verbose);
            await validator.ValidateAsync(path, report, skip);
        }, pathOption, verboseOption, reportOption, skipOption);

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

    public async Task ValidateAsync(string path, string? reportPath, string[] skipValidations)
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
            var securityResult = await RunToolAsync(
                Path.Combine(toolsPath, "DemoInventory.Tools.SecurityScan"),
                $"--path \"{path}\" {(_verbose ? "--verbose" : "")}");
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
        await GenerateSummary();

        // Generate JSON report if requested
        if (!string.IsNullOrEmpty(reportPath))
        {
            await GenerateJsonReport(reportPath);
        }

        // Provide recommendations
        await GenerateRecommendations();
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

    private async Task GenerateSummary()
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

    private async Task GenerateRecommendations()
    {
        Console.WriteLine("💡 Recommendations");
        Console.WriteLine("==================");

        var failedValidations = _results.Where(r => !r.ToolResult.Success).ToList();
        
        if (!failedValidations.Any())
        {
            Console.WriteLine("🎉 Excellent! All validations passed. Your code quality is high.");
            Console.WriteLine();
            Console.WriteLine("📈 Continue following these best practices:");
            Console.WriteLine("  • Keep using proper naming conventions");
            Console.WriteLine("  • Maintain Clean Architecture boundaries");
            Console.WriteLine("  • Follow security best practices");
            Console.WriteLine("  • Write comprehensive tests");
            Console.WriteLine("  • Keep methods focused and simple");
            return;
        }

        Console.WriteLine("🔧 Areas for improvement:");
        Console.WriteLine();

        foreach (var failed in failedValidations)
        {
            var recommendations = GetRecommendations(failed.ValidationName, failed.ToolResult.Output);
            if (recommendations.Any())
            {
                Console.WriteLine($"📌 {failed.ValidationName}:");
                foreach (var recommendation in recommendations)
                {
                    Console.WriteLine($"  • {recommendation}");
                }
                Console.WriteLine();
            }
        }

        Console.WriteLine("📚 Additional Resources:");
        Console.WriteLine("  • Code Verification Guide: docs/copilot/workshop/code-verification.md");
        Console.WriteLine("  • Clean Architecture Documentation: docs/ARCHITECTURE.md");
        Console.WriteLine("  • Security Best Practices: https://owasp.org/www-project-cheat-sheets/");
        Console.WriteLine("  • .NET Coding Standards: https://docs.microsoft.com/en-us/dotnet/csharp/fundamentals/coding-style/");
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

        switch (validationName.ToLower())
        {
            case "static analysis":
                if (output.Contains("complexity"))
                    recommendations.Add("Break down complex methods into smaller, focused functions");
                if (output.Contains("naming"))
                    recommendations.Add("Follow consistent naming conventions (PascalCase for public members, camelCase for private)");
                if (output.Contains("documentation"))
                    recommendations.Add("Add XML documentation to public APIs for better maintainability");
                if (output.Contains("parameters"))
                    recommendations.Add("Reduce parameter count by using parameter objects or configuration classes");
                if (output.Contains("length"))
                    recommendations.Add("Keep methods short and focused on a single responsibility");
                break;

            case "security scan":
                if (output.Contains("hardcoded"))
                    recommendations.Add("Move secrets to configuration files or environment variables");
                if (output.Contains("injection"))
                    recommendations.Add("Use parameterized queries and input validation to prevent injection attacks");
                if (output.Contains("authorization"))
                    recommendations.Add("Add proper authorization attributes to controllers and actions");
                if (output.Contains("logging"))
                    recommendations.Add("Sanitize sensitive data before logging");
                break;

            case "architecture validation":
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
            recommendations.Add($"Review the {validationName} output for specific improvement suggestions");
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