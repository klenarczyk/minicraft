using Minicraft.Game.World.Coordinates;
using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace Minicraft.Engine.Graphics.Viewing;

/// <summary>
/// Manages the First-Person view state (Position, Orientation) and calculates the View/Projection matrices.
/// </summary>
public class Camera(float width, float height)
{
    private const float Sensitivity = 0.1f;
    private const float Fov = 70f;

    public float ScreenWidth { get; set; } = width;
    public float ScreenHeight { get; set; } = height;

    public GlobalPos Position;

    // --- Orientation Vectors ---
    public Vector3 Up { get; private set; } = Vector3.UnitY;
    public Vector3 Front { get; private set; } = -Vector3.UnitZ;
    public Vector3 Right { get; private set; } = Vector3.UnitX;

    // Euler Angles
    private float _pitch;
    private float _yaw = -90f;

    /// <summary>
    /// Returns the matrix that transforms World Space coordinates to View (Camera) Space.
    /// </summary>
    public Matrix4 GetViewMatrix()
    {
        return Matrix4.LookAt((Vector3)Position, (Vector3)Position + Front, Up);
    }

    /// <summary>
    /// Returns the perspective projection matrix (View Space -> Clip Space).
    /// </summary>
    public Matrix4 GetProjectionMatrix()
    {
        if (ScreenWidth <= 0 || ScreenHeight <= 0)
            return Matrix4.Identity;

        // Near plane: 0.1f, Far plane: 200f (Render Distance)
        return Matrix4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(Fov), ScreenWidth / ScreenHeight, 0.1f, 200f);
    }

    public void InputController(MouseState mouse)
    {
        var deltaX = mouse.Delta.X;
        var deltaY = mouse.Delta.Y;

        _yaw += deltaX * Sensitivity;
        _pitch -= deltaY * Sensitivity;

        // Clamp pitch to prevent screen flipping at 90 degrees
        _pitch = MathHelper.Clamp(_pitch, -89f, 89f);

        UpdateVectors();
    }

    /// <summary>
    /// Recalculates the Front/Right/Up vectors based on the current Yaw and Pitch.
    /// </summary>
    private void UpdateVectors()
    {
        // Spherical to Cartesian coordinates conversion
        Front = new Vector3(
            MathF.Cos(MathHelper.DegreesToRadians(_pitch)) * MathF.Cos(MathHelper.DegreesToRadians(_yaw)),
            MathF.Sin(MathHelper.DegreesToRadians(_pitch)),
            MathF.Cos(MathHelper.DegreesToRadians(_pitch)) * MathF.Sin(MathHelper.DegreesToRadians(_yaw))
        ).Normalized();

        Right = Vector3.Normalize(Vector3.Cross(Front, Vector3.UnitY));
        Up = Vector3.Normalize(Vector3.Cross(Right, Front));
    }
}