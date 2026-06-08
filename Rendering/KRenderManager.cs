using System.Buffers;
using System.Collections;
using SFML.Graphics;
using SFML.System;
using SFML.Window;

public struct KBufferRegion
{
    public uint Offset;
    public uint Count;
    public uint Capacity;
}

public enum KCanvasAnchor : int
{
    TOP_LEFT, TOP, TOP_RIGHT,
    LEFT, CENTER, RIGHT,
    BOTTOM_LEFT, BOTTOM, BOTTOM_RIGHT,
    UNDEFINED
}

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

public class KVertexBuffer
{
    public static int INTERNAL_BUFFER_LIMIT = 4_000_000;

    private int _offSet = 0;
    private Vertex[] _internalBuffer;
    private VertexBuffer _buffer;
    
    public int VCount => _offSet;
    public uint Capacity => _buffer.VertexCount;
    public VertexBuffer VBuffer => _buffer;

    public KVertexBuffer(VertexBuffer buffer)
    {
        _buffer = buffer;
        _internalBuffer = new Vertex[2_048 * _buffer.PrimitiveType.GetVertexCount()];
    } 

    public KVertexBuffer(uint bufferSize)
    {
        _buffer = new(bufferSize, PrimitiveType.Triangles, VertexBuffer.UsageSpecifier.Stream);
        _internalBuffer = new Vertex[2_048 * _buffer.PrimitiveType.GetVertexCount()];
    } 

    public void UpdateBuffer(Span<Vertex> vertices)
    {
        if (vertices.Length > _internalBuffer.Length - _offSet) //Resize (rare).
        {
            int max = _internalBuffer.Length * 2;
            if ((uint)max > 4_000_000) max = 4_000_000 % _buffer.PrimitiveType.GetVertexCount();

            var newBuffer = new Vertex[max];
            _internalBuffer.CopyTo(newBuffer);
            _internalBuffer = newBuffer;
        }
        var region =_internalBuffer.AsSpan().Slice(_offSet);
        vertices.CopyTo(region);
    }

    public void DrawTo(IRenderTarget target, RenderStates renderStates)
    {
        _buffer.Update(_internalBuffer, (uint)_offSet, 0);
        _buffer.Draw(target, 0, (uint)_offSet, renderStates);
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

public interface IKRenderer
{
    public ref KCanvas Canvas { get; }
    public ref KBufferRegion Region { get; }
    public IRenderTarget RenderTarget { get; }
    
    public void FrameUpdate(ulong currentFrame, KRenderManager renderer);
    public void DrawToBuffer(VertexBuffer buffer, Span<Vertex> vertices);
}

//Maybe should be a class as it's really only ever passed by refrence.
//Additonally there is a strong case for extension.
public class KRenderLayer : IKRenderer
{
    private int _dBOffset;
    private Vertex[] _drawBuffer;
    private KBufferRegion _region;
    public KCanvas _canvas;

    public Color BKColor;
    public RenderTexture RTexture;
    public RenderStates States;

    public ref KCanvas Canvas => ref _canvas;
    public ref KBufferRegion Region => ref _region;
    public IRenderTarget RenderTarget => RTexture;

    public KRenderLayer(KCanvas canvas, KBufferRegion region)
    {
        _dBOffset = 0;
        _drawBuffer = [];
        _region = region;
        _canvas = canvas;
        BKColor = new(0,0,0,0);
        RTexture = new((Vector2u)canvas.Resolution);
        States = RenderStates.Default;
    }

    public void FrameUpdate(ulong currentFrame, KRenderManager renderer)
    {
        RTexture.Clear();

        if (_dBOffset > 0)
        {
            
        }

        //Draws each layer to the window.
        FloatRect rect = Canvas.ScreenBounds(renderer.RenderScale);
        FloatRect texRect = new((0, 0), (Vector2f)Canvas.Resolution);
        var buffer = ArrayPool<Vertex>.Shared.Rent(6);

        //ABD
        buffer[0] = new(rect.Position, Color.White, texRect.Position);
        buffer[1] = new((rect.Left + rect.Width, rect.Top), Color.White, (texRect.Left + texRect.Width, texRect.Top));
        buffer[2] = new((rect.Left, rect.Top + rect.Height), Color.White, (texRect.Left, texRect.Top + texRect.Height));
        //BCD
        buffer[3] = buffer[1];
        buffer[4] = new(rect.Position + rect.Size, Color.White, texRect.Position + texRect.Size);
        buffer[5] = buffer[2];
            
        RTexture.Display();

        renderer.Window.Draw(buffer, 0, 6, PrimitiveType.Triangles, new(RTexture.Texture));

        ArrayPool<Vertex>.Shared.Return(buffer);
    }

    public void DrawToBuffer()
    {
        
    }

    public void DrawToBuffer(VertexBuffer buffer, Span<Vertex> vertices)
    {
        throw new NotImplementedException();
    }
}

public class KRenderManager
{
    private RenderWindow _window;
    private View _windowView;
    private VertexBuffer _buffer;
    private List<View> _views;

    public int RenderScale;
    public List<IKRenderer> Renderers;

    public RenderWindow Window => _window;
    public VertexBuffer VBuffer => _buffer;
    public event Action<KResourceManager, RenderWindow>? WindowChanged; 

    public KRenderManager(RenderWindow window, VertexBuffer buffer, KBufferRegion[] regions)
    {
        _window = window;
        _windowView = window.GetView();
        _buffer = buffer;
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

    public void DrawBuffer(Span<Vertex> vertices, int layer) => Renderers[layer].DrawToBuffer(_buffer, vertices);

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

    //Untested, unused.
    public VertexBuffer ResizeBuffer(uint size, PrimitiveType primitive = PrimitiveType.Points)
    {
        VertexBuffer newBuffer = new(size, primitive, VertexBuffer.UsageSpecifier.Stream);
        newBuffer.Update(_buffer);

        _buffer.Dispose();
        return _buffer = newBuffer;
    }

    //Untested, unused, should work?
    private void ResizeView(object? _, SizeEventArgs e)
    {
        _windowView.Size = (Vector2f)e.Size;
        _windowView.Center = (0, 0);
        Window.SetView(_windowView);
    }
}