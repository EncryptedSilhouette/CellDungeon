using SFML.Graphics;
using SFML.System;

public interface IKGameSystem
{
    public void UserUpdate(ulong frame, KGameManager game);
    public void Update(ulong frame, KGameManager game);
    public void FrameUpdate(ulong frame, KRenderManager renderer, KGameManager game);
}

public interface IKEntityHandler : IKGameSystem
{

}

public class KGameManager
{
    public static long GetHandle(Vector2i postion) => postion.X + postion.Y * uint.MaxValue;

    public static Vector2i GetPosition(long handle) => new Vector2i
    {
        X = (int)(handle % uint.MaxValue),
        Y = (int)(handle / uint.MaxValue),
    };

    public KInputManager InputManager;
    public KPlayer Player;
    public List<IKEntityHandler> EntityHandlers;

    KCircle circle0 = new((0,0), 50);
    FloatRect rect0 = new((500,100), (100, 50));
    Color rectColor = default;

    Color defaultColor = new(255, 255, 255, 200);
    Color collisionColor = new(255, 0, 0, 200);

    public KGameManager(KInputManager inputManager)
    {
        InputManager = inputManager;
        Player = new();
        EntityHandlers = [];
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

#if DEBUG

public struct KDebugEntity
{
    public KSprite Sprite;
    public Vector2f Position;

    public KDebugEntity()
    {

    }
}

public class KDebugEntityHandler : IKEntityHandler
{
    enum KDebugEntityState
    {
        NONE, 
        PATROL,
    }

    public KGameManager Game;

    public int EntityCount;
    public KDebugEntity[] entities;

    public int MaxEntities => entities.Length;

    public KDebugEntityHandler(KGameManager game)
    {
        Game = game;
        entities = new KDebugEntity[256];
    }

    public void Spawn()
    {

    }

    public void UserUpdate(ulong frame, KGameManager game)
    {
    }

    public void Update(ulong frame, KGameManager game)
    {
        for (int i = 0; i < entities.Length; i++)
        {

        }
    }

    public void FrameUpdate(ulong frame, KRenderManager renderer, KGameManager game)
    {
        for (int i = 0; i < entities.Length; i++)
        {
            renderer.DrawSprite(entities[i].Sprite, 0);
        }
    }
}

#endif