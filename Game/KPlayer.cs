using SFML.System;

public class KPlayer
{
    public string spriteID = "player";
    public float speed = 2;
    public Vector2f Position;

    public KPlayer()
    {

    }

    public void Update(KInputManager input)
    {
        Vector2f direction = new(0, 0);

        //X axis.
        if (input.IsKeyDown(SFML.Window.Keyboard.Key.A)) direction.X = -1;
        else if (input.IsKeyDown(SFML.Window.Keyboard.Key.D)) direction.X = 1;
        //Y axis.
        if (input.IsKeyDown(SFML.Window.Keyboard.Key.W)) direction.Y = -1;
        else if (input.IsKeyDown(SFML.Window.Keyboard.Key.S)) direction.Y = 1;

        if (direction == (0, 0)) return;

        direction = direction.Normalized();
        Position += direction * speed;
    }

    public void FrameUpdate(KRenderManager renderer)
    {
        //renderer.DrawRect();
    }
}