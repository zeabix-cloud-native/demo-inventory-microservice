using FluentAssertions;
using System.Diagnostics;
using Xunit;

namespace DemoInventory.Tools.Tests;

/// <summary>
/// Integration tests for validation tools
/// </summary>
public class ValidationToolsIntegrationTests
{
    private readonly string _projectRoot;

    public ValidationToolsIntegrationTests()
    {
        _projectRoot = GetProjectRoot();
    }

    [Fact]
    public async Task StaticAnalysis_ShouldExecuteSuccessfully()
    {
        // Arrange
        var toolPath = Path.Combine(_projectRoot, "tools", "DemoInventory.Tools.StaticAnalysis");
        var testPath = Path.Combine(_projectRoot, "backend", "src", "DemoInventory.Domain");

        // Act
        var result = await RunToolAsync(toolPath, $"--path \"{testPath}\"");

        // Assert
        result.ExitCode.Should().Be(0, "Static analysis should complete successfully");
        result.Output.Should().Contain("Starting static analysis", "Tool should start correctly");
        result.Output.Should().Contain("Analysis Summary", "Tool should provide summary");
    }

    [Fact]
    public async Task SecurityScan_ShouldExecuteSuccessfully()
    {
        // Arrange
        var toolPath = Path.Combine(_projectRoot, "tools", "DemoInventory.Tools.SecurityScan");
        var testPath = Path.Combine(_projectRoot, "backend", "src", "DemoInventory.API");

        // Act
        var result = await RunToolAsync(toolPath, $"--path \"{testPath}\"");

        // Assert
        result.ExitCode.Should().Be(0, "Security scan should complete successfully");
        result.Output.Should().Contain("Starting security scan", "Tool should start correctly");
        result.Output.Should().Contain("Security Scan Summary", "Tool should provide summary");
    }

    [Fact]
    public async Task ArchitectureValidation_ShouldExecuteSuccessfully()
    {
        // Arrange
        var toolPath = Path.Combine(_projectRoot, "tools", "DemoInventory.Tools.ArchitectureValidation");
        var testPath = Path.Combine(_projectRoot, "backend", "src");

        // Act
        var result = await RunToolAsync(toolPath, $"--path \"{testPath}\"");

        // Assert
        result.ExitCode.Should().Be(0, "Architecture validation should complete successfully");
        result.Output.Should().Contain("Starting architecture validation", "Tool should start correctly");
        result.Output.Should().Contain("Architecture Validation Summary", "Tool should provide summary");
    }

    [Fact]
    public async Task AICodeValidator_ShouldOrchestrateAllTools()
    {
        // Arrange
        var toolPath = Path.Combine(_projectRoot, "tools", "DemoInventory.Tools.AICodeValidator");
        var testPath = Path.Combine(_projectRoot, "backend", "src", "DemoInventory.Domain");

        // Act
        var result = await RunToolAsync(toolPath, $"--path \"{testPath}\"");

        // Assert
        result.ExitCode.Should().Be(0, "AI Code Validator should complete successfully");
        result.Output.Should().Contain("AI Code Validator", "Tool should start correctly");
        result.Output.Should().Contain("Running Static Analysis", "Should run static analysis");
        result.Output.Should().Contain("Running Security Scan", "Should run security scan");
        result.Output.Should().Contain("Running Architecture Validation", "Should run architecture validation");
        result.Output.Should().Contain("Comprehensive Validation Summary", "Should provide comprehensive summary");
    }

    [Fact]
    public async Task Tools_ShouldHandleInvalidPath()
    {
        // Arrange
        var toolPath = Path.Combine(_projectRoot, "tools", "DemoInventory.Tools.StaticAnalysis");
        var invalidPath = "/nonexistent/path";

        // Act
        var result = await RunToolAsync(toolPath, $"--path \"{invalidPath}\"");

        // Assert
        result.ExitCode.Should().Be(0, "Tool should handle invalid path gracefully");
        result.Output.Should().Contain("Invalid path", "Tool should report invalid path");
    }

    [Fact]
    public async Task Tools_ShouldSupportVerboseMode()
    {
        // Arrange
        var toolPath = Path.Combine(_projectRoot, "tools", "DemoInventory.Tools.StaticAnalysis");
        var testPath = Path.Combine(_projectRoot, "backend", "src", "DemoInventory.Domain");

        // Act
        var result = await RunToolAsync(toolPath, $"--path \"{testPath}\" --verbose");

        // Assert
        result.ExitCode.Should().Be(0, "Verbose mode should work");
        result.Output.Should().Contain("Analyzing:", "Verbose mode should show detailed output");
    }

    private async Task<(int ExitCode, string Output, string Error)> RunToolAsync(string toolPath, string arguments)
    {
        var startInfo = new ProcessStartInfo
        {
            FileName = "dotnet",
            Arguments = $"run --project \"{toolPath}\" -- {arguments}",
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true,
            WorkingDirectory = _projectRoot
        };

        using var process = Process.Start(startInfo);
        if (process == null)
        {
            throw new InvalidOperationException("Failed to start process");
        }

        var output = await process.StandardOutput.ReadToEndAsync();
        var error = await process.StandardError.ReadToEndAsync();
        await process.WaitForExitAsync();

        return (process.ExitCode, output, error);
    }

    private string GetProjectRoot()
    {
        var currentDir = Directory.GetCurrentDirectory();
        
        // Look for the solution file to find project root
        while (currentDir != null && !File.Exists(Path.Combine(currentDir, "DemoInventory.sln")))
        {
            currentDir = Directory.GetParent(currentDir)?.FullName;
        }

        return currentDir ?? throw new InvalidOperationException("Could not find project root");
    }
}