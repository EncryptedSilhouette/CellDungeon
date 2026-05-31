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
    public KAICharacter AI;

    KCircle circle0 = new((0,0), 50);
    FloatRect rect0 = new((500,100), (100, 50));
    Color rectColor = default;

    Color defaultColor = new(255, 255, 255, 200);
    Color collisionColor = new(255, 0, 0, 200);

    public KGameManager(KInputManager inputManager)
    {
        InputManager = inputManager;
        Player = new();

        AI = new KAICharacter
        {
            sprite = new(),
        };
        AI.OnGameUpdate += (ref AI, currentFrame) =>
        {
            
        };
        AI.OnFrameUpdate += (ref AI, currentFrame, renderer) =>
        {

        };
    }

    public void Update(ulong currentFrame)
    {
        Player.Update(InputManager, currentFrame);
    }

    public void FrameUpdate(KRenderManager renderer, ulong currentFrame)
    {
        circle0.Position = Player.Position;
        rectColor = KCollision.CircleRect(circle0, rect0) ? 
            collisionColor : defaultColor;

        renderer.DrawRect(rect0, rectColor, 0);
        renderer.DrawCircle(circle0, 32, defaultColor, 0);
        Player.FrameUpdate(renderer, currentFrame);
    }
}