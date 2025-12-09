using Minicraft.Engine.Graphics.Resources;
using Minicraft.Engine.Gui;
using OpenTK.Mathematics;

namespace Minicraft.Game.Ui;

public class CrosshairElement
{
    private const float Size = 32f;

    private readonly Vector4 _uvs = new(0f, 0f, 16f / 256f, 16f / 256f);

    public void Draw(GuiRenderer renderer, Texture texture, int screenWidth, int screenHeight)
    {
        var x = (screenWidth - Size) / 2f;
        var y = (screenHeight - Size) / 2f;

        renderer.DrawSprite(texture, x, y, Size, Size, _uvs);
    }
}