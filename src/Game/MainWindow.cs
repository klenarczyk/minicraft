using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using StbImageSharp;

namespace Game;

public class MainWindow : GameWindow
{
    private readonly List<Vector3> _vertices =
    [
        // Front
        new(-0.5f, 0.5f, 0.5f),
        new(0.5f, 0.5f, 0.5f),
        new(0.5f, -0.5f, 0.5f),
        new(-0.5f, -0.5f, 0.5f),
        // Right
        new(0.5f, 0.5f, 0.5f),
        new(0.5f, 0.5f, -0.5f),
        new(0.5f, -0.5f, -0.5f),
        new(0.5f, -0.5f, 0.5f),
        // Back
        new(0.5f, 0.5f, -0.5f),
        new(-0.5f, 0.5f, -0.5f),
        new(-0.5f, -0.5f, -0.5f),
        new(0.5f, -0.5f, -0.5f),
        // Left
        new(-0.5f, -0.5f, -0.5f),
        new(-0.5f, -0.5f, 0.5f),
        new(-0.5f, 0.5f, 0.5f),
        new(-0.5f, 0.5f, -0.5f),
        // Top
        new(-0.5f, 0.5f, -0.5f),
        new(0.5f, 0.5f, -0.5f),
        new(0.5f, 0.5f, 0.5f),
        new(-0.5f, 0.5f, 0.5f),
        // Bottom
        new(0.5f, -0.5f, 0.5f),
        new(-0.5f, -0.5f, 0.5f),
        new(-0.5f, -0.5f, -0.5f),
        new(0.5f, -0.5f, -0.5f)
    ];

    private readonly List<Vector2> _texCoords =
    [
        new(0f, 1f),
        new(1f, 1f),
        new(1f, 0f),
        new(0f, 0f),

        new(0f, 1f),
        new(1f, 1f),
        new(1f, 0f),
        new(0f, 0f),

        new(0f, 1f),
        new(1f, 1f),
        new(1f, 0f),
        new(0f, 0f),

        new(0f, 1f),
        new(1f, 1f),
        new(1f, 0f),
        new(0f, 0f),

        new(0f, 1f),
        new(1f, 1f),
        new(1f, 0f),
        new(0f, 0f),

        new(0f, 1f),
        new(1f, 1f),
        new(1f, 0f),
        new(0f, 0f)
    ];

    private readonly uint[] _indices =
    [
        0, 1, 2,
        2, 3, 0,

        4, 5, 6,
        6, 7, 4,

        8, 9, 10,
        10, 11, 8,

        12, 13, 14,
        14, 15, 12,

        16, 17, 18,
        18, 19, 16,

        20, 21, 22,
        22, 23, 20
    ];

    // Render pipeline variables
    private int _vao;
    private int _vbo;
    private int _ebo;
    private int _shaderProgram;
    private int _textureId;
    private int _textureVbo;

    // Transformation variables
    private float _yRot = 0f;

    // Window variables
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

        // Bind VAO
        _vao = GL.GenVertexArray();
        GL.BindVertexArray(_vao);

        // --- Vertices VBO (Slot 0) ---
        _vbo = GL.GenBuffer();
        GL.BindBuffer(BufferTarget.ArrayBuffer, _vbo);
        GL.BufferData(BufferTarget.ArrayBuffer, _vertices.Count * Vector3.SizeInBytes, _vertices.ToArray(), BufferUsageHint.StaticDraw);

        // Configure attribute 0 (Position)
        GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 3 * sizeof(float), 0);
        GL.EnableVertexArrayAttrib(_vao, 0);

        // --- Texture VBO (Slot 1) ---
        _textureVbo = GL.GenBuffer();
        GL.BindBuffer(BufferTarget.ArrayBuffer, _textureVbo);
        GL.BufferData(BufferTarget.ArrayBuffer, _texCoords.Count * Vector2.SizeInBytes, _texCoords.ToArray(), BufferUsageHint.StaticDraw);

        // Configure attribute 1 (Texture Coords)
        GL.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, 2 * sizeof(float), 0);
        GL.EnableVertexArrayAttrib(_vao, 1);

        // --- EBO ---
        _ebo = GL.GenBuffer();
        GL.BindBuffer(BufferTarget.ElementArrayBuffer, _ebo);
        GL.BufferData(BufferTarget.ElementArrayBuffer, _indices.Length * sizeof(uint), _indices, BufferUsageHint.StaticDraw);

        // Cleanup
        GL.BindVertexArray(0);
        GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
        GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);

        // --- Shader Program ---
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

        // --- Textures ---
        _textureId = GL.GenTexture();
        GL.ActiveTexture(TextureUnit.Texture0);
        GL.BindTexture(TextureTarget.Texture2D, _textureId);

        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);

        // Load image
        StbImage.stbi_set_flip_vertically_on_load(1);
        var dirtTexture = ImageResult.FromStream(File.OpenRead("./Textures/dirt.png"), ColorComponents.RedGreenBlueAlpha);

        GL.TexImage2D(TextureTarget.Texture2D, 
            0, 
            PixelInternalFormat.Rgba, 
            dirtTexture.Width, 
            dirtTexture.Height, 
            0, 
            PixelFormat.Rgba, 
            PixelType.UnsignedByte, 
            dirtTexture.Data);
        GL.BindTexture(TextureTarget.Texture2D, 0);

        GL.Enable(EnableCap.DepthTest);
    }

    protected override void OnUnload()
    {
        base.OnUnload();

        // Delete VAO
        GL.DeleteVertexArray(_vao);

        // Delete Buffer Objects
        GL.DeleteBuffer(_vbo);
        GL.DeleteBuffer(_ebo);
        GL.DeleteBuffer(_textureVbo);

        // Delete Program and Texture
        GL.DeleteProgram(_shaderProgram);
        GL.DeleteTexture(_textureId);
    }

    protected override void OnRenderFrame(FrameEventArgs args)
    {
        base.OnRenderFrame(args);

        GL.ClearColor(0.48f, 0.64f, 1f, 1f);
        GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

        GL.UseProgram(_shaderProgram);
        GL.BindVertexArray(_vao);
        GL.BindBuffer(BufferTarget.ElementArrayBuffer, _ebo);

        GL.BindTexture(TextureTarget.Texture2D, _textureId);

        // Transformation matrices
        var model = Matrix4.Identity;
        var view = Matrix4.Identity;
        var projection = Matrix4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(90.0f), (float)_screenWidth / _screenHeight, 0.1f, 100.0f);

        model = Matrix4.CreateRotationY(_yRot);
        _yRot += 0.001f;
        var translation = Matrix4.CreateTranslation(0f, 0f, -3f);
        model *= translation;

        var modelLocation = GL.GetUniformLocation(_shaderProgram, "model");
        var viewLocation = GL.GetUniformLocation(_shaderProgram, "view");
        var projectionLocation = GL.GetUniformLocation(_shaderProgram, "projection");

        GL.UniformMatrix4(modelLocation, true, ref model);
        GL.UniformMatrix4(viewLocation, true, ref view);
        GL.UniformMatrix4(projectionLocation, true, ref projection);

        GL.DrawElements(PrimitiveType.Triangles, _indices.Length, DrawElementsType.UnsignedInt, 0);

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