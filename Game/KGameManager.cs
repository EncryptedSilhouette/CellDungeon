using SFML.Graphics;

public class KGameManager
{
    public void Update()
    {

    }

    public void FrameUpdate(KRenderManager renderManager)
    {
        var sprite = KProgram.TextureAtlas.Sprites["key"];
        renderManager.DrawRect(new((0, 0), (4, 4)), sprite, Color.Yellow, 0);
    }
}