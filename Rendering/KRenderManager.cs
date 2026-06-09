using System.Buffers;
using SFML.Graphics;
using SFML.System;
using SFML.Window;

public static class KPrimitiveTypeExtensions
{
    public static int GetVertexCount(this PrimitiveType primitive) => primitive switch
    {
        PrimitiveType.Points => 1,
        PrimitiveType.Triangles => 3,
        PrimitiveType.Lines => 2,
        _ => 3
    };
}

public struct KSprite
{
    public Color Color;
    public FloatRect Bounds;
    public FloatRect TRect;
}

public interface IKRenderer
{
    public ref KCanvas Canvas { get; }
    public IRenderTarget RenderTarget { get; }

    public void FrameUpdate(ulong currentFrame, KRenderManager renderer);
    public void UpdateBuffer(Span<Vertex> vertices);
}

//Maybe should be a class as it's really only ever passed by refrence.
//Additonally there is a strong case for extension.
public class KWorldRenderer : IKRenderer
{
    private int _dBOffset;
    private Vertex[] _drawBuffer;
    public KCanvas _canvas;
    private Sprite _displayTexture;

    public Color BKColor;
    public RenderTexture RTexture;
    public RenderStates States;

    public ref KCanvas Canvas => ref _canvas;
    public IRenderTarget RenderTarget => RTexture;

    public KWorldRenderer(KCanvas canvas)
    {
        _dBOffset = 0;
        _drawBuffer = [];
        _canvas = canvas;
        BKColor = new(0, 0, 0, 0);
        RTexture = new((Vector2u)canvas.Resolution);
        States = RenderStates.Default;

        _displayTexture = new(RTexture.Texture);
    }

    public void FrameUpdate(ulong currentFrame, KRenderManager renderer)
    {
        RTexture.Clear();



        RTexture.Display();

        renderer.Window.Draw(_displayTexture);
    }

    public void UpdateBuffer(Span<Vertex> vertices)
    {
        var region = _drawBuffer.AsSpan(_dBOffset);

        if (region.Length < vertices.Length) vertices = vertices.Slice(region.Length);

        vertices.CopyTo(region);
    }
}

public class KRenderManager
{
    private RenderWindow _window;
    private View _windowView;
    private List<View> _views;

    public int RenderScale;
    public List<IKRenderer> Renderers;

    public RenderWindow Window => _window;
    public event Action<KResourceManager, RenderWindow>? WindowChanged;

    public KRenderManager(RenderWindow window, VertexBuffer buffer)
    {
        _window = window;
        _windowView = window.GetView();
        _views = [];

        RenderScale = 1;
        Renderers = [];
    }

    public void FrameUpdate(ulong frame)
    {
        _window.Clear();

        //Renders each layer
        for (int i = 0; i < Renderers.Count; i++) Renderers[i].FrameUpdate(frame, this);

        _window.Display();
    }

    public void DrawBuffer(Span<Vertex> vertices, int layer) => Renderers[layer].UpdateBuffer(vertices);

    //SFML 3.0 removed Quads from their PrimitiveType enum, so you must draw quads with triangles.
    //ABD represents the first half of the quad (top left, top right, bottom left), 
    //BCD represents the other half (top right, bottom right, bottom, left). 
    public void DrawRect(FloatRect rect, FloatRect textureRect, Color color, int layer)
    {
        Span<Vertex> buffer = stackalloc Vertex[6];

        //ABD
        buffer[0] = new(rect.Position, color, textureRect.Position);
        buffer[1] = new((rect.Left + rect.Width, rect.Top), color, (textureRect.Left + textureRect.Width, textureRect.Top));
        buffer[2] = new((rect.Left, rect.Top + rect.Height), color, (textureRect.Left, textureRect.Top + textureRect.Height));
        //BCD
        buffer[3] = buffer[1];
        buffer[4] = new(rect.Position + rect.Size, color, textureRect.Position + textureRect.Size);
        buffer[5] = buffer[2];

        DrawBuffer(buffer, layer);
    }

    public void DrawRect(FloatRect rect, Color color, int layer) => DrawRect(rect, color, layer);

    public void DrawArc(KCircle circle, float startAngle, float arcLength, int segments, Color color, int layer)
    {
        Span<Vertex> buffer = stackalloc Vertex[segments * 3];

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

        DrawBuffer(buffer, layer);
    }
    public void DrawCircle(KCircle circle, int segments, Color color, int layer) => DrawArc(circle, 0.0f, MathF.PI * 2, segments, color, layer);

    public void DrawSprite(KSprite sprite, int layer) => DrawRect(sprite.Bounds, sprite.TRect, sprite.Color, layer);

    public void DrawGridOverlay(Vector2f cellSize, Color color, int layer)
    {
        var scale = Renderers[layer].Canvas.Resolution.X / (float)Window.Size.X;
        cellSize *= scale;

        //Calcualtes amount of rows and colums to fill screen.
        int cols = (int)(Window.Size.X / cellSize.X) + 1;
        int rows = (int)(Window.Size.Y / cellSize.Y) + 1;
        int vCount = (cols + rows) * 2;

        Span<Vertex> buff = stackalloc Vertex[vCount];

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

        DrawBuffer(buff, layer);
    }

    public Vector2i MapCoordsToPixel(Vector2f point, int camera) =>
        Window.MapCoordsToPixel(point, _views[camera]);

    public Vector2f MapPixelToCoords(Vector2i point, int camera) =>
        Window.MapPixelToCoords(point, _views[camera]);

    //Untested, unused, should work?
    private void ResizeView(object? _, SizeEventArgs e)
    {
        _windowView.Size = (Vector2f)e.Size;
        _windowView.Center = (0, 0);
        Window.SetView(_windowView);
    }
}