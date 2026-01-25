using Minicraft.Engine.Diagnostics;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace Minicraft.Engine.Graphics.Data;

/// <summary>
/// A wrapper for an OpenGL Shader Program.
/// Handles compiling GLSL source code, linking the program, and sending Uniform data to the GPU.
/// </summary>
public class Shader : IDisposable
{
    public readonly int Id;
    private bool _disposed;
    private readonly string _name;

    public Shader(string vertexPath, string fragmentPath)
    {
        _name = $"{vertexPath}/{fragmentPath}";
        Logger.Debug($"[Shader] Compiling Shader: {_name}");

        // --- Compilation Pipeline ---
        // Create the empty program container
        Id = GL.CreateProgram();

        // Compile individual shader stages
        var vertex = CompileShader(ShaderType.VertexShader, vertexPath);
        var fragment = CompileShader(ShaderType.FragmentShader, fragmentPath);

        // Attach and Link
        GL.AttachShader(Id, vertex);
        GL.AttachShader(Id, fragment);
        GL.LinkProgram(Id);

        // Verification
        GL.GetProgram(Id, GetProgramParameterName.LinkStatus, out int success);
        if (success == 0)
        {
            var infoLog = GL.GetProgramInfoLog(Id);
            Logger.Error($"[Shader] Linking Failed ({_name}):\n{infoLog}");
        }

        //  Cleanup Intermediate Objects
        GL.DetachShader(Id, vertex);
        GL.DetachShader(Id, fragment);
        GL.DeleteShader(vertex);
        GL.DeleteShader(fragment);
    }

    public void Use() => GL.UseProgram(Id);

    // --- Uniform Setters ---
    // These methods push data from CPU (C#) to GPU (GLSL)

    public void SetInt(string name, int value)
    {
        var location = GL.GetUniformLocation(Id, name);
        if (location == -1)
        {
            Logger.Warn($"[Shader] Uniform '{name}' not found (or optimized out) in shader '{_name}'.");
        }
        GL.Uniform1(location, value);
    }

    public void SetFloat(string name, float value)
    {
        var location = GL.GetUniformLocation(Id, name);
        GL.Uniform1(location, value);
    }

    public void SetMatrix4(string name, Matrix4 data)
    {
        var location = GL.GetUniformLocation(Id, name);
        GL.UniformMatrix4(location, false, ref data);
    }

    public void SetVector3(string name, Vector3 data)
    {
        var location = GL.GetUniformLocation(Id, name);
        GL.Uniform3(location, data);
    }

    public void SetVector4(string name, Vector4 data)
    {
        var location = GL.GetUniformLocation(Id, name);
        GL.Uniform4(location, data);
    }

    // --- Internal Helpers ---

    private int CompileShader(ShaderType type, string path)
    {
        var shader = GL.CreateShader(type);
        GL.ShaderSource(shader, LoadSource(path));
        GL.CompileShader(shader);

        GL.GetShader(shader, ShaderParameter.CompileStatus, out var success);
        if (success == 0)
        {
            var infoLog = GL.GetShaderInfoLog(shader);
            Logger.Error($"[Shader] Compilation Failed ({type} - {path}):\n{infoLog}");
        }
        return shader;
    }

    private static string LoadSource(string filePath)
    {
        var fullPath = Path.Combine(AppContext.BaseDirectory, "Assets", "Shaders", filePath);
        if (!File.Exists(fullPath))
        {
            Logger.Error($"[Shader] File missing: {fullPath}");
            throw new FileNotFoundException($"Shader not found: {fullPath}");
        }
        return File.ReadAllText(fullPath);
    }

    public void Dispose()
    {
        if (_disposed) return;
        GL.DeleteProgram(Id);
        _disposed = true;
        Logger.Debug($"[Shader] Disposed: {_name}");
        GC.SuppressFinalize(this);
    }
}