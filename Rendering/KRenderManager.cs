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

    public static void DrawLine(this VertexBuffer self, Vector2f pointA, Vector2f pointB, Color color, ref KBufferRegion region)
    {
        var buffer = ArrayPool<Vertex>.Shared.Rent(2);
        buffer[0] = new(pointA, color);
        buffer[1] = new(pointB, color);

        self.DrawBuffer(buffer, 2, ref region);
    }

    //SFML 3.0 removed Quads from their PrimitiveType enum, so you must draw quads with triangles.
    //ABD represents the first half of the quad (top left, top right, bottom left), 
    //BCD represents the other half (top right, bottom right, bottom, left). 
    public static void DrawRect(this VertexBuffer self, FloatRect rect, Color color, ref KBufferRegion region)
    {
        var buffer = ArrayPool<Vertex>.Shared.Rent(6);
        //ABD       
        buffer[0] = new((rect.Left, rect.Top), color, (0, 0));
        buffer[1] = new((rect.Left + rect.Width, rect.Top), color, (0, 0));
        buffer[2] = new((rect.Left, rect.Top + rect.Height), color, (0, 0));
        //BCD
        buffer[3] = buffer[1];
        buffer[4] = new((rect.Left + rect.Width, rect.Top + rect.Height), color, (0, 0));
        buffer[5] = buffer[2];

        self.DrawBuffer(buffer, 6, ref region);
    }

    public static void DrawRect(this VertexBuffer self, FloatRect rect, FloatRect textureRect, Color color, ref KBufferRegion region)
    {
        var buffer = ArrayPool<Vertex>.Shared.Rent(6);
        //ABD
        buffer[0] = new(rect.Position, color, textureRect.Position);
        buffer[1] = new((rect.Left + rect.Width, rect.Top), color, (textureRect.Left + textureRect.Width, textureRect.Top));
        buffer[2] = new((rect.Left, rect.Top + rect.Height), color, (textureRect.Left, textureRect.Top + textureRect.Height));
        //BCD
        buffer[3] = buffer[1];
        buffer[4] = new(rect.Position + rect.Size, color, textureRect.Position + textureRect.Size);
        buffer[5] = buffer[2];

        self.DrawBuffer(buffer, 6, ref region);
    }

    public static void Draw(this VertexBuffer self, IRenderTarget target, ref KBufferRegion region, RenderStates states, bool resetRegion = false)
    {
        self.Draw(target, region.Offset, region.Count, states);
        if (resetRegion) region.Count = 0;
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
public struct KRenderLayer
{
    private View _view;

    public bool IsStatic;
    public FloatRect Bounds; //Defines the bounds, that the layer will be drawn to. 
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

    public KRenderLayer(RenderTexture renderTexture, PrimitiveType primitive, KBufferRegion region)
    {
        _view = renderTexture.DefaultView;

        IsStatic = false;
        RenderTexture = renderTexture;
        Bounds = new((0, 0), (Vector2f)renderTexture.Size);
        Primitive = primitive;
        States = RenderStates.Default;
        Region = region;
        ClearColor = Color.Transparent;
    }

    public void Clear() => RenderTexture.Clear(ClearColor);

    public void RenderFrame(VertexBuffer buffer)
    {
        if (buffer.PrimitiveType != Primitive) buffer.PrimitiveType = Primitive;
        buffer.Draw(RenderTexture, ref Region, States);
        if (!IsStatic) Region.Count = 0;
    }

    public void Display() => RenderTexture.Display();

    //Maybe overengineering?? I don't think there will be a case to draw to a layer directly,
    //unless used outside of the RenderManager type.
    // public void DrawBuffer(VertexBuffer vBuffer, Vertex[] vertices, uint vCount) =>
    //     vBuffer.DrawBuffer(vertices, vCount, ref Region);

    // public void DrawLine(VertexBuffer vBuffer, Vector2f a, Vector2f b, Color color) =>
    //     vBuffer.DrawLine(a, b, color, ref Region);

    // public void DrawRect(VertexBuffer vBuffer, FloatRect rect, Color color) =>
    //     vBuffer.DrawRect(rect, color, ref Region);

    // public void DrawRect(VertexBuffer vBuffer, FloatRect rect, FloatRect textureRect, Color color) =>
    //     vBuffer.DrawRect(rect, textureRect, color, ref Region);

    public Vector2f GetScaleRelativeTo(Vector2f otherSize) =>
        new(otherSize.X / Bounds.Size.X, otherSize.Y / Bounds.Size.Y);
    public float GetScaleXRelativeTo(float width) => width / Bounds.Size.X;
    public float GetScaleYRelativeTo(float height) => height / Bounds.Size.Y;
}

public class KRenderManager
{
    private View _view;
    public RenderWindow Window;
    public VertexBuffer VBuffer; //Refrence to the VertexBuffer.
    public KTextHandler TextHandler;
    public KRenderLayer[] RenderLayers;

    public View View
    {
        get => _view;
        set => Window.SetView(_view = value);
    }

    public KRenderManager(RenderWindow window, VertexBuffer vBuffer, KRenderLayer[] renderLayers, KTextHandler textHandler)
    {
        _view = window.DefaultView;

        Window = window;
        VBuffer = vBuffer;
        RenderLayers = renderLayers;
        TextHandler = textHandler;

        window.Resized += ResizeView;
    }

    public void FrameUpdate()
    {
        for (int i = 0; i < RenderLayers.Length; i++)
        {
            //Renders each layer
            RenderLayers[i].Clear();
            RenderLayers[i].RenderFrame(VBuffer);
            RenderLayers[i].Display();

            //Draws each layer to the window.
            FloatRect rect = RenderLayers[i].Bounds;
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
        }

        //Draws text to the window.
        TextHandler.FrameUpdate(Window);
    }

    public void DrawBuffer(Vertex[] vertices, uint vCount, int layer) =>
        VBuffer.DrawBuffer(vertices, vCount, ref RenderLayers[layer].Region);

    public void DrawLine(Vector2f a, Vector2f b, Color color, int layer) =>
        VBuffer.DrawLine(a, b, color, ref RenderLayers[layer].Region);

    public void DrawRect(FloatRect rect, Color color, int layer) =>
        VBuffer.DrawRect(rect, color, ref RenderLayers[layer].Region);

    public void DrawRect(FloatRect rect, FloatRect textureRect, Color color, int layer) =>
        VBuffer.DrawRect(rect, textureRect, color, ref RenderLayers[layer].Region);

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
        _view.Center = _view.Size / 2;
        Window.SetView(_view);
    }
}

