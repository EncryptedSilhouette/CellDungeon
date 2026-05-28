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

    KCircle circ0 = new((0, 0), 50);
    KCircle circ1 = new((500, 500), 50);
    FloatRect rect0 = new((700, 500), (100, 50));

    Color defaultColor = new(255, 255, 255, 100);
    Color collisionColor = new(255, 0, 0, 100);
    Color circleColor = Color.White;
    Color rectColor = Color.White;


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

        circ0.Position = Player.Position;
        circleColor = KCollision.CircleCircle(circ0, circ1) ?
            collisionColor : defaultColor;

        rectColor = KCollision.CircleRect(circ0, rect0) ?
            collisionColor : defaultColor;
    }

    public void FrameUpdate(KRenderManager renderer, ulong currentFrame)
    {
        renderer.DrawCircle(circ0, 32, new(255, 255, 255, 100), 0);
        renderer.DrawCircle(circ1, 32, circleColor, 0);

        renderer.DrawRect(rect0, rectColor, 0);

        Player.FrameUpdate(renderer, currentFrame);
    }
}