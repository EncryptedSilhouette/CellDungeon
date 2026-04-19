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

    public KGameManager()
    {
        Player = new();
    }

    public void Update()
    {

    }

    public void FrameUpdate(KRenderManager renderManager)
    {
        //Test
        var sprite = KProgram.TextureAtlas.Sprites["key"];
        renderManager.DrawRect(new((0, 0), (4, 4)), sprite, Color.Yellow, 0);
    }
}