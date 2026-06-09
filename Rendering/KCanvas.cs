
using SFML.Graphics;
using SFML.System;

public enum KCanvasAnchor : int
{
    TOP_LEFT, TOP, TOP_RIGHT,
    LEFT, CENTER, RIGHT,
    BOTTOM_LEFT, BOTTOM, BOTTOM_RIGHT,
    UNDEFINED
}

public struct KCanvas
{
    public required Vector2i Resolution;
    public required Vector2i Position;
    public required KCanvasAnchor Anchor;

    public KCanvas(Vector2i position, Vector2i resolution, KCanvasAnchor anchor)
    {
        Position = position;
        Resolution = resolution;
        Anchor = anchor;
    }

    public KCanvas(Vector2i resolution, KCanvasAnchor anchor) :
        this((0, 0), resolution, anchor)
    { }

    public KCanvas(Vector2i resolution) :
        this((0, 0), resolution, KCanvasAnchor.CENTER)
    { }

    public Vector2i ScaledResolution(int scale) => Resolution * scale;
    public Vector2i ScaledPosition(int scale) => Position * scale;

    public FloatRect ScreenBounds(int scale)
    {
        Vector2f res = (Vector2f)ScaledResolution(scale);
        Vector2f pos = (Vector2f)ScaledPosition(scale);

        pos = Anchor switch
        {
            //Top
            KCanvasAnchor.TOP_LEFT => new Vector2f
            {
                X = pos.X - res.X / 2.0f,
                Y = pos.Y - res.Y / 2.0f,
            },
            KCanvasAnchor.TOP => new Vector2f
            {
                X = pos.X - res.X / 2.0f,
                Y = pos.Y - res.Y / 2.0f,
            },
            KCanvasAnchor.TOP_RIGHT => new Vector2f
            {
                X = res.X / 2.0f - pos.X - res.X,
                Y = pos.Y - res.Y / 2.0f,
            },
            //Center
            KCanvasAnchor.LEFT => new Vector2f
            {
                X = pos.X - res.X / 2.0f,
                Y = pos.Y - res.Y / 2.0f,
            },
            KCanvasAnchor.CENTER => new Vector2f
            {
                X = pos.X - res.X / 2.0f,
                Y = pos.Y - res.Y / 2.0f,
            },
            KCanvasAnchor.RIGHT => new Vector2f
            {
                X = res.X / 2.0f - pos.X - res.X,
                Y = pos.Y - res.Y / 2.0f,
            },
            //bottom
            KCanvasAnchor.BOTTOM_LEFT => new Vector2f
            {
                X = pos.X - res.X / 2.0f,
                Y = res.Y / 2.0f - pos.Y - res.Y,
            },
            KCanvasAnchor.BOTTOM => new Vector2f
            {
                X = pos.X - res.X / 2.0f,
                Y = res.Y / 2.0f - pos.Y - res.Y,
            },
            KCanvasAnchor.BOTTOM_RIGHT => new Vector2f
            {
                X = res.X / 2.0f - pos.X - res.X,
                Y = res.Y / 2.0f - pos.Y - res.Y,
            },

            _ => new(0, 0)
        };
        return new(pos, res);
    }
}
