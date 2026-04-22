using System.Buffers;
using SFML.Graphics;
using SFML.System;
using SFML.Window;

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

    public void Update()
    {
        Player.Update(InputManager);
    }

    public void FrameUpdate(KRenderManager renderer)
    {
        Player.FrameUpdate(renderer);

        var cols = renderer.Window.Size.X / 16 / 2 + 1;
        var rows = renderer.Window.Size.Y / 16 / 2 + 1;

        var vCount = (cols + rows) * 2;
        Console.WriteLine(vCount);
        var buff = ArrayPool<Vertex>.Shared.Rent((int)vCount);

        for (int i = 0; i < cols; i++)
        {
            buff[i * 2] = new((i * 16 * 2, 0), Color.White);
            buff[i * 2 + 1] = new((i * 16 * 2, renderer.Window.Size.Y), Color.White);
        }

        var offset = cols * 2;

        for (int i = 0; i < rows; i++)
        {
            buff[offset + i * 2] = new((0, i * 16 * 2), Color.White);
            buff[offset + i * 2 + 1] = new((renderer.Window.Size.X, i * 16 * 2), Color.White);
        }

        renderer.DrawBuffer(buff, vCount, 1);
    }
}