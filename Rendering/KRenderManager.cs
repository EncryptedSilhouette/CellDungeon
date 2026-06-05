using System.Buffers;
using SFML.Graphics;
using SFML.System;
using SFML.Window;

public struct KBufferRegion
{
    public uint Offset;
    public uint Count;
    public uint Capacity;

    public KBufferRegion(uint offset, uint capacity)
    {
        Offset = offset;
        Count = 0;
        Capacity = capacity;
    }
}

//Extension methods to simplify drawing primitives to a VertexBuffer.
public static class KVertexBufferExtensions
{
    public static void DrawBuffer(this VertexBuffer self, Vertex[] vertices, uint vCount, ref KBufferRegion region)
    {
        if (region.Count + vCount > region.Capacity) vCount = region.Capacity - region.Count;

        self.Update(vertices, vCount, region.Offset + region.Count);
        region.Count += vCount;
    }

    //SFML 3.0 removed Quads from their PrimitiveType enum, so you must draw quads with triangles.
    //ABD represents the first half of the quad (top left, top right, bottom left), 
    //BCD represents the other half (top right, bottom right, bottom, left). 
    public static void DrawRect(this VertexBuffer self, FloatRect rect, FloatRect textureRect, Color color, ref KBufferRegion region)
    {
        var buffer = ArrayPool<Vertex>.Shared.Rent(6);

        //ABD
        buffer[0] = new((rect.Position), color, textureRect.Position);
        buffer[1] = new((rect.Left + rect.Width, rect.Top), color, (textureRect.Left + textureRect.Width, textureRect.Top));
        buffer[2] = new((rect.Left, rect.Top + rect.Height), color, (textureRect.Left, textureRect.Top + textureRect.Height));
        //BCD
        buffer[3] = buffer[1];
        buffer[4] = new(rect.Position + rect.Size, color, textureRect.Position + textureRect.Size);
        buffer[5] = buffer[2];

        self.DrawBuffer(buffer, 6, ref region);
    }

    public static void DrawRect(this VertexBuffer self, FloatRect rect, Color color, ref KBufferRegion region) =>
        self.DrawRect(rect, new((0, 0), (0, 0)), color, ref region);


    public static void DrawArc(this VertexBuffer self, KCircle circle, float startAngle, float arcLength, int segments, Color color, ref KBufferRegion region)
    {
        var buffer = ArrayPool<Vertex>.Shared.Rent(segments * 3);

        float angle = arcLength / segments;
        float angleInc = startAngle;

        for (int i = 0; i < segments; i++)
        {
            buffer[i * 3] = new Vertex
            {
                Position = circle.Position,
                Color = color,
            };
            buffer[i * 3 + 1] = new Vertex
            {
                Position = circle.Position + new Vector2f(MathF.Cos(angleInc), MathF.Sin(angleInc)) * circle.Radius,
                Color = color,
            };

            angleInc += angle;

            buffer[i * 3 + 2] = new Vertex
            {
                Position = circle.Position + new Vector2f(MathF.Cos(angleInc), MathF.Sin(angleInc)) * circle.Radius,
                Color = color,
            };
        }

        self.DrawBuffer(buffer, (uint)segments * 3, ref region);

        ArrayPool<Vertex>.Shared.Return(buffer);
    }
    public static void DrawCircle(this VertexBuffer self, KCircle circle, int segments, Color color, ref KBufferRegion region) =>
        self.DrawArc(circle, 0.0f, MathF.PI * 2, segments, color, ref region);

    public static void DrawTo(this VertexBuffer self, IRenderTarget target, ref KBufferRegion region, RenderStates states, bool resetRegion = false)
    {
        self.Draw(target, region.Offset, region.Count, states);
        if (resetRegion) region.Count = 0;
    }
}


public enum KCanvasAnchor : int
{
    TOP_LEFT, TOP, TOP_RIGHT,
    LEFT, CENTER, RIGHT,
    BOTTOM_LEFT, BOTTOM, BOTTOM_RIGHT,
    UNDEFINED
}

public struct KResolution
{
    public float Scale;
    public Vector2f AspectRatio;
    public Vector2f Dimentions => AspectRatio * Scale;

    public KResolution(Vector2i resolution)
    {
        AspectRatio = (Vector2f)resolution / KProgram.GreatestCommonFactor(resolution);
        Scale = resolution.Y / AspectRatio.Y;
    }
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

public struct KSprite
{
    public Color Color;
    public FloatRect Bounds;
    public FloatRect TRect;
}

//Maybe should be a class as it's really only ever passed by refrence.
//Additonally there is a strong case for extension.
public class KRenderLayer
{
    private View _view;

    public bool IsStatic;
    public KCanvas Canvas;
    public PrimitiveType Primitive;
    public RenderStates States;
    public KBufferRegion Region;
    public RenderTexture RenderTexture;
    public Color ClearColor;

    public Texture Texture => RenderTexture.Texture;

    public View View
    {
        get => _view;
        set => RenderTexture.SetView(_view = value);
    }

    public KRenderLayer(KCanvas canvas, PrimitiveType primitive, KBufferRegion region, RenderStates states)
    {
        IsStatic = false;
        Canvas = canvas;
        Primitive = primitive;
        Region = region;
        States = states;
        RenderTexture = new((Vector2u)canvas.Resolution);
        ClearColor = Color.Transparent;

        _view = RenderTexture.DefaultView;
    }

    public KRenderLayer(KCanvas canvas, KBufferRegion region) :
        this(canvas, PrimitiveType.Triangles, region, RenderStates.Default)
    { }

    public FloatRect ScreenBounds(int scale) => Canvas.ScreenBounds(scale);

    public void Clear() => RenderTexture.Clear(ClearColor);

    public void RenderFrame(VertexBuffer buffer)
    {
        if (buffer.PrimitiveType != Primitive) buffer.PrimitiveType = Primitive;
        buffer.DrawTo(RenderTexture, ref Region, States);
        if (!IsStatic) Region.Count = 0;
    }

    public void Display() => RenderTexture.Display();
}

public class KRenderManager
{
    private View _view;
    private int RenderScale;

    public RenderWindow Window;
    public VertexBuffer VBuffer; //Refrence to the VertexBuffer.
    public KTextHandler TextHandler;
    public KRenderLayer[] RenderLayers;

    public View View
    {
        get => _view;
        set => Window.SetView(_view = value);
    }

    public KRenderManager(int scale, RenderWindow window, VertexBuffer vBuffer, KRenderLayer[] renderLayers, KTextHandler textHandler)
    {
        RenderScale = scale;
        Window = window;
        _view = window.DefaultView;
        _view.Center = (0, 0);
        View = _view;

        VBuffer = vBuffer;
        RenderLayers = renderLayers;
        TextHandler = textHandler;

        window.Resized += ResizeView;
    }

    public void FrameUpdate(ulong currentFrame)
    {

        // if (currentFrame % 30 == 0)
        // {
        //     ref var canvas = ref RenderLayers[1].Canvas;
        //     var anchor = canvas.CanvasAnchor;
        //     anchor = anchor + 1 == KCanvasAnchor.UNDEFINED ? 0 : anchor + 1;
        //     canvas.CanvasAnchor = anchor; 
        // }


        //DrawRectOutline(bounds, Color.White, 1);

        for (int i = 0; i < RenderLayers.Length; i++)
        {
            //Renders each layer
            RenderLayers[i].Clear();
            RenderLayers[i].RenderFrame(VBuffer);
            RenderLayers[i].Display();

            //Draws each layer to the window.
            FloatRect rect = RenderLayers[i].ScreenBounds(RenderScale);
            FloatRect texRect = new((0, 0), (Vector2f)RenderLayers[i].Texture.Size);

            var buffer = ArrayPool<Vertex>.Shared.Rent(6);
            //ABD
            buffer[0] = new(rect.Position, Color.White, texRect.Position);
            buffer[1] = new((rect.Left + rect.Width, rect.Top), Color.White, (texRect.Left + texRect.Width, texRect.Top));
            buffer[2] = new((rect.Left, rect.Top + rect.Height), Color.White, (texRect.Left, texRect.Top + texRect.Height));
            //BCD
            buffer[3] = buffer[1];
            buffer[4] = new(rect.Position + rect.Size, Color.White, texRect.Position + texRect.Size);
            buffer[5] = buffer[2];

            Window.Draw(buffer, 0, 6, PrimitiveType.Triangles, new(RenderLayers[i].Texture));

            ArrayPool<Vertex>.Shared.Return(buffer);
        }

        var center = new RectangleShape((4, 4));
        center.Position = -center.Size / 2;
        center.FillColor = Color.Red;
        Window.Draw(center);

        //Draws text to the window.
        TextHandler.FrameUpdate(Window);
    }

    public void DrawBuffer(Vertex[] vertices, uint vCount, int layer) =>
        VBuffer.DrawBuffer(vertices, vCount, ref RenderLayers[layer].Region);

    //This method is gonna make me kms.
    public void DrawRectOutline(IntRect rect, Color color, int layer)
    {
        var buffer = ArrayPool<Vertex>.Shared.Rent(8);

        //AB
        buffer[0] = new((rect.Left, rect.Top), color);
        buffer[1] = new((rect.Left + rect.Width, rect.Top), color);
        //BC
        buffer[2] = buffer[1];
        buffer[3] = new((rect.Left + rect.Width, rect.Top + rect.Height), color);
        //CD
        buffer[4] = buffer[3];
        buffer[5] = new((rect.Left, rect.Top + rect.Height), color);
        //DA
        buffer[6] = buffer[5];
        buffer[7] = buffer[0];

        DrawBuffer(buffer, 8, layer);

        ArrayPool<Vertex>.Shared.Return(buffer);
    }

    public void DrawRect(FloatRect rect, Color color, int layer) =>
        VBuffer.DrawRect(rect, color, ref RenderLayers[layer].Region);

    public void DrawRect(FloatRect rect, FloatRect textureRect, Color color, int layer) =>
        VBuffer.DrawRect(rect, textureRect, color, ref RenderLayers[layer].Region);

    public void DrawCircle(KCircle circle, int segments, Color color, int layer) =>
        VBuffer.DrawCircle(circle, segments, color, ref RenderLayers[layer].Region);

    public void DrawArc(KCircle circle, float angleA, float arcLength, int segments, Color color, int layer) =>
        VBuffer.DrawArc(circle, angleA, arcLength, segments, color, ref RenderLayers[layer].Region);

    public void DrawSprite(KSprite sprite, int layer) =>
        VBuffer.DrawRect(sprite.Bounds, sprite.TRect, sprite.Color, ref RenderLayers[layer].Region);

    public void DrawGridOverlay(Vector2f cellSize, Color color, int layer)
    {
        var scale = RenderLayers[layer].RenderTexture.Size.X / (float)Window.Size.X;
        cellSize *= scale;

        //Calcualtes amount of rows and colums to fill screen.
        int cols = (int)(Window.Size.X / cellSize.X) + 1;
        int rows = (int)(Window.Size.Y / cellSize.Y) + 1;
        int vCount = (cols + rows) * 2;

        var buff = ArrayPool<Vertex>.Shared.Rent(vCount);

        for (int i = 0; i < cols; i++)
        {
            buff[i * 2] = new((i * cellSize.X, 0), color);
            buff[i * 2 + 1] = new((i * cellSize.X, Window.Size.Y), color);
        }

        var offset = cols * 2;

        for (int i = 0; i < rows; i++)
        {
            buff[offset + i * 2] = new((0, i * cellSize.Y), color);
            buff[offset + i * 2 + 1] = new((Window.Size.X, i * cellSize.Y), color);
        }

        DrawBuffer(buff, (uint)vCount, layer);

        ArrayPool<Vertex>.Shared.Return(buff);
    }

    public Vector2i MapCoordsToPixel(Vector2f point, int layer) =>
        Window.MapCoordsToPixel(point, RenderLayers[layer].View);

    public Vector2f MapPixelToCoords(Vector2i point, int layer) =>
        Window.MapPixelToCoords(point, RenderLayers[layer].View);

    //Untested, unused.
    public VertexBuffer ResizeBuffer(uint size, PrimitiveType primitive = PrimitiveType.Points)
    {
        VertexBuffer newBuffer = new(size, primitive, VertexBuffer.UsageSpecifier.Stream);
        newBuffer.Update(VBuffer);

        VBuffer.Dispose();
        return VBuffer = newBuffer;
    }

    //Untested, unused, should work?
    private void ResizeView(object? _, SizeEventArgs e)
    {
        _view.Size = (Vector2f)e.Size;
        _view.Center = (0, 0);
        Window.SetView(_view);
    }
}

