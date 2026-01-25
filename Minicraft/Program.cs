using Minicraft;
using Minicraft.Engine.Diagnostics;

Logger.Initialize();

try
{
    Logger.Info("Starting Game...");

    using var game = new GameWindow(1280, 720);
    game.Run();
}
catch (Exception ex)
{
    Logger.Error("CRITICAL FAILURE: The game crashed!", ex);
}
finally
{
    Logger.Info("Game Session Ended.");
}