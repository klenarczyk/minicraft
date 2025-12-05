using OpenTK.Graphics.OpenGL4;

namespace Game.Graphics;

public class ShaderProgram : IDisposable
{
    public readonly int Id;
    private bool _disposed;

    public ShaderProgram(string vertexShaderPath, string fragmentShaderPath)
    { 
        Id = GL.CreateProgram();

        var vertexShader = GL.CreateShader(ShaderType.VertexShader);
        GL.ShaderSource(vertexShader, LoadShaderSource(vertexShaderPath));
        GL.CompileShader(vertexShader);

        var fragmentShader = GL.CreateShader(ShaderType.FragmentShader);
        GL.ShaderSource(fragmentShader, LoadShaderSource(fragmentShaderPath));
        GL.CompileShader(fragmentShader);

        GL.AttachShader(Id, vertexShader);
        GL.AttachShader(Id, fragmentShader);

        GL.LinkProgram(Id);

        // Dispose shaders as they're linked into our program now and no longer necessary
        GL.DeleteShader(vertexShader);
        GL.DeleteShader(fragmentShader);
    }

    public void Bind()
    {
        GL.UseProgram(Id);
    }

    public void Unbind()
    {
        GL.UseProgram(0);
    }

    public void Dispose()
    {
        if (_disposed) return;
        GL.DeleteShader(Id);
        _disposed = true;
        GC.SuppressFinalize(this);
    }

    private static string LoadShaderSource(string filePath)
    {
        var baseDir = AppContext.BaseDirectory;
        var fullPath = Path.Combine(baseDir, "Shaders", filePath);

        var shaderSource = "";

        try
        {
            using var reader = new StreamReader(fullPath);
            shaderSource = reader.ReadToEnd();
        }
        catch (Exception e)
        {
            Console.WriteLine($"Error reading shader file at {fullPath}: {e.Message}");
        }

        return shaderSource;
    }
}