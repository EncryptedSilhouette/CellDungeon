using SFML.Graphics;
using SFML.System;

public class KPlayer
{
    public float speed = 1;
    public Vector2f Position;
    public KSprite Sprite;

    public KPlayer()
    {
        Position = (1, 1);
        Sprite = new KSprite
        {
            Color = Color.Blue,
            Bounds = new(Position, (2, 2)),
            //Replace with refrence.
            TRect = KProgram.TextureAtlas.Sprites["node0"],
        };
    }

    public void Update(KInputManager input, ulong currentFrame)
    {
        if (currentFrame % 10 != 0) return;

        Vector2f direction = new(0, 0);

        //X axis.
        if (input.IsKeyDown(SFML.Window.Keyboard.Key.A)) direction.X = -1;
        else if (input.IsKeyDown(SFML.Window.Keyboard.Key.D)) direction.X = 1;
        if (input.IsKeyDown(SFML.Window.Keyboard.Key.W)) direction.Y = -1;
        else if (input.IsKeyDown(SFML.Window.Keyboard.Key.S)) direction.Y = 1;

        Position += direction;
    }

    public void FrameUpdate(KRenderManager renderer)
    {
        Sprite.Bounds.Position = Position;

        renderer.DrawSprite(Sprite, 0);
    }
}