using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace Minicraft.Engine.Graphics.Resources;

public class Shader : IDisposable
{
    public readonly int Id;
    private bool _disposed;

    public Shader(string vertexPath, string fragmentPath)
    {
        Id = GL.CreateProgram();

        var vertex = CompileShader(ShaderType.VertexShader, vertexPath);
        var fragment = CompileShader(ShaderType.FragmentShader, fragmentPath);

        GL.AttachShader(Id, vertex);
        GL.AttachShader(Id, fragment);
        GL.LinkProgram(Id);

        // Check for linking errors
        GL.GetProgram(Id, GetProgramParameterName.LinkStatus, out int success);
        if (success == 0)
        {
            var infoLog = GL.GetProgramInfoLog(Id);
            Console.WriteLine($"Error linking shader program: {infoLog}");
        }

        GL.DetachShader(Id, vertex);
        GL.DetachShader(Id, fragment);
        GL.DeleteShader(Id);
        GL.DeleteShader(vertex);
        GL.DeleteShader(fragment);
    }

    public void Use() => GL.UseProgram(Id);

    // --- Uniform Setters ---

    public void SetInt(string name, int value)
    {
        var location = GL.GetUniformLocation(Id, name);
        if (location == -1) Console.WriteLine($"Warning: Uniform '{name}' not found in shader.");
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

    public void Dispose()
    {
        if (_disposed) return;
        GL.DeleteProgram(Id);
        _disposed = true;
        GC.SuppressFinalize(this);
    }

    private int CompileShader(ShaderType type, string path)
    {
        var shader = GL.CreateShader(type);
        GL.ShaderSource(shader, LoadSource(path));
        GL.CompileShader(shader);

        GL.GetShader(shader, ShaderParameter.CompileStatus, out int success);
        if (success == 0)
        {
            var infoLog = GL.GetShaderInfoLog(shader);
            Console.WriteLine($"Error compiling {type} at {path}: {infoLog}");
        }
        return shader;
    }

    private static string LoadSource(string filePath)
    {
        var fullPath = Path.Combine(AppContext.BaseDirectory, "Assets", "Shaders", filePath);
        return !File.Exists(fullPath) 
            ? throw new FileNotFoundException($"Shader not found: {fullPath}") 
            : File.ReadAllText(fullPath);
    }
}