using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;

namespace Game;

public class MainWindow : GameWindow
{
    private readonly float[] _vertices =
    [
        0f, 0.5f, 0f,
        -0.5f, -0.5f, 0f,
        0.5f, -0.5f, 0f
    ];

    // Render pipeline
    private int _vao;
    private int _shaderProgram;

    private int _screenWidth;
    private int _screenHeight;

    public MainWindow(int width, int height)
        : base(GameWindowSettings.Default, NativeWindowSettings.Default)
    {
        CenterWindow(new Vector2i(width, height));
        _screenWidth = width;
        _screenHeight = height;
    }

    protected override void OnResize(ResizeEventArgs e)
    {
        base.OnResize(e);
        GL.Viewport(0, 0, e.Width, e.Height);
        _screenWidth = e.Width;
        _screenHeight = e.Height;
    }

    protected override void OnLoad()
    {
        base.OnLoad();

        _vao = GL.GenVertexArray();

        // Create VBO
        var vbo = GL.GenBuffer();
        GL.BindBuffer(BufferTarget.ArrayBuffer, vbo);
        GL.BufferData(BufferTarget.ArrayBuffer, _vertices.Length * sizeof(float), _vertices, BufferUsageHint.StaticDraw);

        // Bind the VAO
        GL.BindVertexArray(_vao);
        GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 0, 0);
        GL.EnableVertexArrayAttrib(_vao, 0);

        // Unbind (cleanup)
        GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
        GL.BindVertexArray(0);

        // Create shader program
        _shaderProgram = GL.CreateProgram();

        var vertexShader = GL.CreateShader(ShaderType.VertexShader);
        GL.ShaderSource(vertexShader, LoadShaderSource("Default.vert"));
        GL.CompileShader(vertexShader);

        var fragmentShader = GL.CreateShader(ShaderType.FragmentShader);
        GL.ShaderSource(fragmentShader, LoadShaderSource("Default.frag"));
        GL.CompileShader(fragmentShader);

        GL.AttachShader(_shaderProgram, vertexShader);
        GL.AttachShader(_shaderProgram, fragmentShader);

        GL.LinkProgram(_shaderProgram);

        // Delete shaders as they're linked into our program now and no longer necessary
        GL.DeleteShader(vertexShader);
        GL.DeleteShader(fragmentShader);
    }

    protected override void OnUnload()
    {
        base.OnUnload();

        GL.DeleteVertexArray(_vao);
        GL.DeleteProgram(_shaderProgram);
    }

    protected override void OnRenderFrame(FrameEventArgs args)
    {
        base.OnRenderFrame(args);

        GL.ClearColor(0.6f, 0.3f, 1f, 1f);
        GL.Clear(ClearBufferMask.ColorBufferBit);

        // Draw triangle
        GL.UseProgram(_shaderProgram);
        GL.BindVertexArray(_vao);
        GL.DrawArrays(PrimitiveType.Triangles, 0, 3);

        Context.SwapBuffers();
    }

    protected override void OnUpdateFrame(FrameEventArgs args)
    {
        base.OnUpdateFrame(args);
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