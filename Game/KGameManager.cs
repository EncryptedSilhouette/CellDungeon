using SFML.System;

//Arrays of Structures:
//string|int|KSprite |string|int|KSprite |string|int|KSprite |...

//Structures of arrays:
//string         |string        |string        |...
//int            |int           |int           |...
//KEntitySprite  |KEntitySprite |KEntitySprite |...
public interface IKEntityHandler 
{
    public void Update(ulong frame, KGameManager game);
    public void FrameUpdate(ulong frame, KRenderManager renderer, KGameManager game);
}

public struct KGamePosition
{
    public Vector2f Position;
    public Vector2f Direction;
}

public enum KEntityState
{
    NONE, 
    PATROL,
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

    public int MaxEntities;
    public int EntityCount;
    public int[] Handles;
    public bool[] IsAlive;
    public KGamePosition[] Position;
    public KSprite[] Sprites;
    public KEntityState[] States;
    public List<IKEntityHandler> entityHandlers;

    public KGameManager(KInputManager input, IEnumerable<IKEntityHandler> children)
    {
        InputManager = input;

        MaxEntities = 1024;
        Handles = new int[MaxEntities];
        IsAlive = new bool[MaxEntities];
        Position = new KGamePosition[MaxEntities];
        Sprites = new KSprite[MaxEntities];
        States = new KEntityState[MaxEntities];
        entityHandlers = new(children);
    }

    public void Update(ulong frame)
    {
        for (int i = 0; i < Position.Length; i++)
        {
            entityHandlers[i].Update(frame, this);
        }
    }

    public void FrameUpdate(ulong frame, KRenderManager renderer)
    {
        for (int i = 0; i < MaxEntities; i++)
        {
            entityHandlers[i].FrameUpdate(frame, renderer, this);
        }
    }
}