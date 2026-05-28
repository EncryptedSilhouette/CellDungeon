using SFML.System;

public delegate void AIHandler(ref KAICharacter character, ulong currentFrame);
public delegate void AIRenderHandler(ref KAICharacter character, ulong currentFrame, KRenderManager renderer);

public struct KAICharacter
{
    public Vector2f Position;
    public KSprite sprite;

    public float X => Position.X;
    public float Y => Position.X;

    public event AIHandler OnGameUpdate;
    public event AIRenderHandler OnFrameUpdate;

    public KAICharacter(AIHandler gameScript, AIRenderHandler renderScript)
    {
        OnGameUpdate = gameScript;
        OnFrameUpdate = renderScript;
    }

    public void Update(ulong currentFrame) =>
        OnGameUpdate?.Invoke(ref this, currentFrame);

    public void FrameUpdate(ulong currentFrame, KRenderManager renderer) =>
        OnFrameUpdate?.Invoke(ref this, currentFrame, renderer);
}