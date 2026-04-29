using SFML.Graphics;
using SFML.System;

public class KGameManager
{
    public static long GetHandle(Vector2i postion) => postion.X + postion.Y * uint.MaxValue;

    public static Vector2i GetPosition(long handle) => new Vector2i
    {
        X = (int)(handle % uint.MaxValue),
        Y = (int)(handle / uint.MaxValue),
    };

    public KPlayer Player;
    public KInputManager InputManager;

    public KGameManager(KInputManager inputManager)
    {
        InputManager = inputManager;
        Player = new();
    }

    public void Update(ulong currentFrame)
    {
        Player.Update(InputManager, currentFrame);
    }

    public void FrameUpdate(KRenderManager renderer, ulong currentFrame)
    {
        Player.FrameUpdate(renderer, currentFrame);
        renderer.DrawGridOverlay((32, 32), Color.White, 1);
    }
}