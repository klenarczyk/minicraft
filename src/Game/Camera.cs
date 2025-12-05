using Game.Core;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace Game;

public class Camera(float width, float height)
{
    private const float Sensitivity = 0.1f;
    private const float Fov = 70f;

    public float ScreenWidth { get; set; } = width;
    public float ScreenHeight { get; set; } = height;

    public GlobalPos Position;

    public Vector3 Up { get; private set; } = Vector3.UnitY;
    public Vector3 Front { get; private set; } = -Vector3.UnitZ;
    public Vector3 Right { get; private set; } = Vector3.UnitX;

    private float _pitch;
    private float _yaw = -90f;

    public Matrix4 GetViewMatrix()
    {
        return Matrix4.LookAt((Vector3)Position, (Vector3)Position + Front, Up);
    }

    public Matrix4 GetProjectionMatrix()
    {
        if (ScreenWidth <= 0 || ScreenHeight <= 0)
            return Matrix4.Identity;

        return Matrix4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(Fov), ScreenWidth / ScreenHeight, 0.1f, 200f);
    }

    public void InputController(MouseState mouse, FrameEventArgs e)
    {
        var deltaX = mouse.Delta.X;
        var deltaY = mouse.Delta.Y;

        _yaw += deltaX * Sensitivity;
        _pitch -= deltaY * Sensitivity;

        _pitch = MathHelper.Clamp(_pitch, -89f, 89f);

        UpdateVectors();
    }

    private void UpdateVectors()
    {
        Front = new Vector3(
            MathF.Cos(MathHelper.DegreesToRadians(_pitch)) * MathF.Cos(MathHelper.DegreesToRadians(_yaw)),
            MathF.Sin(MathHelper.DegreesToRadians(_pitch)),
            MathF.Cos(MathHelper.DegreesToRadians(_pitch)) * MathF.Sin(MathHelper.DegreesToRadians(_yaw))
        ).Normalized();

        Right = Vector3.Normalize(Vector3.Cross(Front, Vector3.UnitY));
        Up = Vector3.Normalize(Vector3.Cross(Right, Front));
    }
}