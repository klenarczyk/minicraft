using OpenTK.Mathematics;
using OpenTK.Windowing.Desktop;

namespace Game;

public class MainWindow : GameWindow
{
    private static int _screenWidth;
    private static int _screenHeight;

    public MainWindow(int width, int height)
        : base(GameWindowSettings.Default, NativeWindowSettings.Default)
    {
        CenterWindow(new Vector2i(width, height));
        _screenWidth = width;
        _screenHeight = height;
    }
}