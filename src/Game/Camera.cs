using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace Game;

public class Camera(float width, float height, Vector3 position)
{
    private float _speed = 20f;
    private const float Sensitivity = 0.1f;
    private const float Fov = 45f;

    public float ScreenWidth { get; set; } = width;
    public float ScreenHeight { get; set; } = height;

    // Position variables
    public Vector3 Position = position;
    private Vector3 _up = Vector3.UnitY;
    private Vector3 _front = -Vector3.UnitZ;
    private Vector3 _right = Vector3.UnitX;

    // View Rotations
    private float _pitch;
    private float _yaw = -90f;

    public Matrix4 GetViewMatrix()
    {
        return Matrix4.LookAt(Position, Position + _front, _up);
    }

    public Matrix4 GetProjectionMatrix()
    {
        if (ScreenWidth <= 0 || ScreenHeight <= 0)
            return Matrix4.Identity;

        return Matrix4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(Fov), ScreenWidth / ScreenHeight, 0.1f, 200f);
    }

    private void UpdateVectors()
    {
        if (_pitch > 89f)
        {
            _pitch = 89f;
        }
        if (_pitch < -89f)
        {
            _pitch = -89f;
        }

        _front.X = MathF.Cos(MathHelper.DegreesToRadians(_pitch)) * MathF.Cos(MathHelper.DegreesToRadians(_yaw));
        _front.Y = MathF.Sin(MathHelper.DegreesToRadians(_pitch));
        _front.Z = MathF.Cos(MathHelper.DegreesToRadians(_pitch)) * MathF.Sin(MathHelper.DegreesToRadians(_yaw));
        _front = Vector3.Normalize(_front);

        _right = Vector3.Normalize(Vector3.Cross(_front, Vector3.UnitY));
        _up = Vector3.Normalize(Vector3.Cross(_right, _front));
    }

    public void InputController(KeyboardState input, MouseState mouse, FrameEventArgs e)
    {
        if (input.IsKeyDown(Keys.W))
        {
            Position += _front * _speed * (float)e.Time;
        }
        if (input.IsKeyDown(Keys.A))
        {
            Position -= _right * _speed * (float)e.Time;
        }
        if (input.IsKeyDown(Keys.S))
        {
            Position -= _front * _speed * (float)e.Time;
        }
        if (input.IsKeyDown(Keys.D))
        {
            Position += _right * _speed * (float)e.Time;
        }

        if (input.IsKeyDown(Keys.Space))
        {
            Position.Y += _speed * (float)e.Time;
        }
        if (input.IsKeyDown(Keys.LeftControl))
        {
            Position.Y -= _speed * (float)e.Time;
        }

        if (mouse.ScrollDelta != Vector2.Zero)
        {
            _speed += mouse.ScrollDelta.Y;
        }

        var deltaX = mouse.Delta.X;
        var deltaY = mouse.Delta.Y;

        _yaw += deltaX * Sensitivity;
        _pitch -= deltaY * Sensitivity;

        UpdateVectors();
    }

    public void Update(KeyboardState input, MouseState mouse, FrameEventArgs e)
    {
        InputController(input, mouse, e);
    }
}