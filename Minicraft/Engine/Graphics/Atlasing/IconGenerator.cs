using Minicraft.Engine.Graphics.Resources;
using Minicraft.Game.World.Blocks;
using Minicraft.Game.World.Meshing;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace Minicraft.Engine.Graphics.Atlasing;

public sealed class IconGenerator : IDisposable
{
    private readonly Shader _iconShader;

    private readonly int _vao;
    private readonly int _vbo;
    private readonly int _fbo;

    private readonly int _fboTexture;
    private readonly int _rboDepth;

    private const int IconSize = 64;

    public IconGenerator(Shader iconShader)
    {
        _iconShader = iconShader;
        _vao = GL.GenVertexArray();
        _vbo = GL.GenBuffer();
        _fbo = GL.GenFramebuffer();
        GL.BindFramebuffer(FramebufferTarget.Framebuffer, _fbo);

        _fboTexture = GL.GenTexture();
        GL.BindTexture(TextureTarget.Texture2D, _fboTexture);
        GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, IconSize, IconSize, 0, PixelFormat.Rgba, PixelType.UnsignedByte, IntPtr.Zero);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
        GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0, TextureTarget.Texture2D, _fboTexture, 0);

        _rboDepth = GL.GenRenderbuffer();
        GL.BindRenderbuffer(RenderbufferTarget.Renderbuffer, _rboDepth);
        GL.RenderbufferStorage(RenderbufferTarget.Renderbuffer, RenderbufferStorage.DepthComponent24, IconSize, IconSize);
        GL.FramebufferRenderbuffer(FramebufferTarget.Framebuffer, FramebufferAttachment.DepthAttachment, RenderbufferTarget.Renderbuffer, _rboDepth);

        GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
    }

    /// <summary>
    /// Generates a 3D isometric icon for a block.
    /// </summary>
    /// <param name="debugName">If provided, saves the icon to Disk/Debug/Icons/{name}.png</param>
    public Image<Rgba32> GenerateBlockIcon(Texture2D blockAtlas, Block block, string? debugName = null)
    {
        GL.BindFramebuffer(FramebufferTarget.Framebuffer, _fbo);
        GL.Viewport(0, 0, IconSize, IconSize);

        GL.ClearColor(0, 0, 0, 0);
        GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

        GL.Enable(EnableCap.DepthTest);
        GL.Disable(EnableCap.CullFace);

        // Center the Block
        var model = Matrix4.CreateTranslation(-0.5f, -0.5f, -0.5f);

        // Position the Camera
        var cameraPos = new Vector3(2.0f, 1.7f, 2.0f);
        var view = Matrix4.LookAt(cameraPos, Vector3.Zero, Vector3.UnitY);

        // Projection
        var projection = Matrix4.CreateOrthographic(1.9f, 1.9f, 0.1f, 10.0f);

        // Update Shader
        _iconShader.Use();
        _iconShader.SetMatrix4("u_Mvp", model * view * projection);

        GL.ActiveTexture(TextureUnit.Texture0);
        blockAtlas.Bind();
        _iconShader.SetInt("u_Texture", 0);

        // Render
        RenderCube(block);

        // Read Pixels
        var pixelData = new byte[IconSize * IconSize * 4];
        GL.ReadPixels(0, 0, IconSize, IconSize, PixelFormat.Rgba, PixelType.UnsignedByte, pixelData);

        GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);

        // Process Image
        var image = Image.LoadPixelData<Rgba32>(pixelData, IconSize, IconSize);
        image.Mutate(x => x.Flip(FlipMode.Vertical));

        if (!string.IsNullOrEmpty(debugName))
        {
            try
            {
                Directory.CreateDirectory("Debug/Icons");
                image.SaveAsPng($"Debug/Icons/{debugName}.png");
            }
            catch { }
        }

        return image;
    }

    private void RenderCube(Block block)
    {
        var facesToDraw = new[] { BlockFace.Top, BlockFace.Front, BlockFace.Right };

        GL.BindVertexArray(_vao);
        GL.BindBuffer(BufferTarget.ArrayBuffer, _vbo);

        foreach (var face in facesToDraw)
        {
            if (!block.Uvs.TryGetValue(face, out var uv))
            {
                if (block.Uvs.Count > 0)
                    RenderFace(face, block.Uvs.First().Value);

                continue;
            }

            RenderFace(face, uv);
        }
    }

    private static void RenderFace(BlockFace face, Vector4 uvs)
    {
        var vertices = BlockGeometry.RawVertexData[face];
        var normal = BlockGeometry.FaceNormals[face];
        var data = BuildVertexData(vertices, uvs, normal);

        GL.BufferData(BufferTarget.ArrayBuffer, data.Length * sizeof(float), data, BufferUsageHint.StreamDraw);

        GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 8 * sizeof(float), 0);
        GL.EnableVertexAttribArray(0);
        GL.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, 8 * sizeof(float), 3 * sizeof(float));
        GL.EnableVertexAttribArray(1);
        GL.VertexAttribPointer(2, 3, VertexAttribPointerType.Float, false, 8 * sizeof(float), 5 * sizeof(float));
        GL.EnableVertexAttribArray(2);

        GL.DrawArrays(PrimitiveType.TriangleFan, 0, 4);
    }

    // Helper to build vertex data remains the same...
    private static float[] BuildVertexData(Vector3[] pos, Vector4 uvs, Vector3 norm)
    {
       var uMin = uvs.X;
       var vMin = uvs.Y;
       var uMax = uvs.X + uvs.Z;
       var vMax = uvs.Y + uvs.W;

       return
       [
           pos[0].X, pos[0].Y, pos[0].Z,  uMin, vMin,  norm.X, norm.Y, norm.Z,
           pos[1].X, pos[1].Y, pos[1].Z,  uMax, vMin,  norm.X, norm.Y, norm.Z,
           pos[2].X, pos[2].Y, pos[2].Z,  uMax, vMax,  norm.X, norm.Y, norm.Z,
           pos[3].X, pos[3].Y, pos[3].Z,  uMin, vMax,  norm.X, norm.Y, norm.Z
       ];
    }

    public void Dispose()
    {
        GL.DeleteVertexArray(_vao);
        GL.DeleteBuffer(_vbo);
        GL.DeleteFramebuffer(_fbo);
        GL.DeleteTexture(_fboTexture);
        GL.DeleteRenderbuffer(_rboDepth);
    }
}