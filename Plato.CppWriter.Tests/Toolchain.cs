using System.Diagnostics;

namespace PlatoCppWriterTests;

/// <summary>
/// Locates a host C++ compiler (MSVC) and, if it is installed, the CUDA compiler, and runs
/// them over generated source. The CUDA gate has two modes:
///
/// - <b>nvcc</b>, when the CUDA Toolkit is installed: the real thing.
/// - <b>MSVC + shim</b>, otherwise: the .cu is compiled as C++ against a stand-in
///   cuda_runtime.h that defines the __host__/__device__ qualifiers away and supplies the
///   vector types. That still gates everything the writer decides (types, overloads, name
///   collisions, unresolved calls); it cannot gate CUDA-specific semantics.
///
/// The mode is reported in the test output so a green run is never mistaken for more than
/// it is, and the tests upgrade themselves the day the toolkit is installed.
/// </summary>
public static class Toolchain
{
    public static string? FindVcVars()
    {
        var programFilesX86 = Environment.GetEnvironmentVariable("ProgramFiles(x86)")
                              ?? @"C:\Program Files (x86)";
        var vswhere = Path.Combine(programFilesX86, "Microsoft Visual Studio", "Installer", "vswhere.exe");
        if (File.Exists(vswhere))
        {
            var (exit, output) = Run(vswhere,
                "-latest -products * -requires Microsoft.VisualStudio.Component.VC.Tools.x86.x64 -property installationPath",
                Environment.CurrentDirectory);
            var path = output.Trim().Split('\n').FirstOrDefault()?.Trim();
            if (exit == 0 && !string.IsNullOrEmpty(path))
            {
                var vcvars = Path.Combine(path!, "VC", "Auxiliary", "Build", "vcvars64.bat");
                if (File.Exists(vcvars))
                    return vcvars;
            }
        }

        // vswhere is not installed everywhere; fall back to the standard layout.
        foreach (var root in new[] { @"C:\Program Files\Microsoft Visual Studio", programFilesX86 + @"\Microsoft Visual Studio" })
        {
            if (!Directory.Exists(root))
                continue;
            var found = Directory.EnumerateFiles(root, "vcvars64.bat", SearchOption.AllDirectories)
                .OrderByDescending(f => f)
                .FirstOrDefault();
            if (found != null)
                return found;
        }
        return null;
    }

    public static string? FindNvcc()
    {
        var cudaPath = Environment.GetEnvironmentVariable("CUDA_PATH");
        if (!string.IsNullOrEmpty(cudaPath))
        {
            var candidate = Path.Combine(cudaPath!, "bin", "nvcc.exe");
            if (File.Exists(candidate))
                return candidate;
        }

        var toolkit = @"C:\Program Files\NVIDIA GPU Computing Toolkit\CUDA";
        if (Directory.Exists(toolkit))
        {
            var candidate = Directory.EnumerateDirectories(toolkit)
                .OrderByDescending(d => d)
                .Select(d => Path.Combine(d, "bin", "nvcc.exe"))
                .FirstOrDefault(File.Exists);
            if (candidate != null)
                return candidate;
        }

        var onPath = (Environment.GetEnvironmentVariable("PATH") ?? "")
            .Split(Path.PathSeparator)
            .Where(p => !string.IsNullOrWhiteSpace(p))
            .Select(p =>
            {
                try { return Path.Combine(p, "nvcc.exe"); }
                catch (ArgumentException) { return null; }
            })
            .FirstOrDefault(p => p != null && File.Exists(p));
        return onPath;
    }

    public static (int ExitCode, string Output) Run(string exe, string args, string workingDir)
    {
        var psi = new ProcessStartInfo(exe, args)
        {
            WorkingDirectory = workingDir,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true,
        };
        using var p = Process.Start(psi)!;
        var stdout = p.StandardOutput.ReadToEnd();
        var stderr = p.StandardError.ReadToEnd();
        p.WaitForExit();
        return (p.ExitCode, stdout + stderr);
    }

    /// <summary>Runs cl.exe on <paramref name="sourceFile"/> with the MSVC environment loaded.</summary>
    public static (int ExitCode, string Output) CompileWithMsvc(string vcvars, string workingDir,
        string sourceFile, string extraArgs = "")
    {
        // The compiler only exists inside the environment vcvars64.bat sets up, so both run
        // in one cmd invocation. The outer quotes are cmd's own /c "..." wrapper.
        var command = $"\"\"{vcvars}\" >nul && cl /nologo /std:c++17 /W3 /EHsc /c {extraArgs} \"{sourceFile}\"\"";
        return Run("cmd.exe", "/c " + command, workingDir);
    }

    public static (int ExitCode, string Output) CompileWithNvcc(string nvcc, string workingDir, string sourceFile,
        string? vcvars = null)
    {
        var outObj = Path.ChangeExtension(sourceFile, ".obj");
        var nvccArgs = $"-std=c++17 -c \"{sourceFile}\" -o \"{outObj}\"";
        // nvcc shells out to cl.exe; without the MSVC environment it dies with
        // "Cannot find compiler 'cl.exe' in PATH" and no ": error" line for AssertCompiles.
        if (vcvars != null)
        {
            var command = $"\"\"{vcvars}\" >nul && \"{nvcc}\" {nvccArgs}\"";
            return Run("cmd.exe", "/c " + command, workingDir);
        }
        return Run(nvcc, nvccArgs, workingDir);
    }

    /// <summary>
    /// A stand-in for the CUDA headers, so a .cu can be compiled by a plain host C++ compiler
    /// when no toolkit is installed. It declares exactly what the generated CUDA preamble
    /// consumes: the qualifier macros and the float2/3/4 vector types with their make_ helpers.
    /// </summary>
    public const string CudaShimHeader = @"#pragma once
// Test-only stand-in for <cuda_runtime.h>. NOT part of the generated output.
#define __host__
#define __device__
#define __global__
#define __forceinline__ inline

struct float2 { float x, y; };
struct float3 { float x, y, z; };
struct float4 { float x, y, z, w; };

inline float2 make_float2(float x, float y) { return float2{ x, y }; }
inline float3 make_float3(float x, float y, float z) { return float3{ x, y, z }; }
inline float4 make_float4(float x, float y, float z, float w) { return float4{ x, y, z, w }; }
";
}
