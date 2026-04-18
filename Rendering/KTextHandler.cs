using System.Buffers;
using SFML.Graphics;
using SFML.System;

public struct KText
{
    public Color Color;
    public string TextStr;
    public byte Size;
    public bool Bold;

    public KText()
    {
        TextStr = string.Empty;
        Size = 12;
        Bold = false;
    }

    public KText(string str, Color color, byte size = 12, bool bold = false)
    {
        TextStr = str;
        Color = color;
        Size = size;
        Bold = bold;
    }
}

public struct KGlyphHandle
{
    public bool Bold;
    public byte Size;
    public char Chr;

    public KGlyphHandle(char chr, byte size, bool bold)
    {
        Chr = chr;
        Size = size;
        Bold = bold;
    }
}

public class KTextHandler
{
    private Dictionary<KGlyphHandle, Glyph> _glyphCache = new();

    public uint FontSize = 12;
    public VertexBuffer VBuffer;
    public KBufferRegion Region;
    public Font Font;
    public RenderStates States = RenderStates.Default;

    public KTextHandler(Font font, VertexBuffer buffer, KBufferRegion region)
    {
        Font = font;
        VBuffer = buffer;
        Region = region;
    }

    public void FrameUpdate(IRenderTarget target) =>
        VBuffer.Draw(target, ref Region, States);

    public Glyph GetGlyph(byte ch, bool bold, bool updateTexture = false)
    {
        var glyph = Font.GetGlyph(ch, FontSize, bold, 0);

        if (updateTexture) States.Texture = Font.GetTexture(FontSize);

        return glyph;
    }

    public Glyph GetGlyph(KGlyphHandle glyphHandle, bool updateTexture = false)
    {
        var glyph = Font.GetGlyph(glyphHandle.Chr, glyphHandle.Size, glyphHandle.Bold, 0);

        if (updateTexture) States.Texture = Font.GetTexture(FontSize);

        return glyph;
    }

    public void DrawText(
        KText text,
        Vector2f pos,
        out FloatRect bounds,
        byte lnSpacing = 0,
        uint wrapThres = 0)
    {
        var buffer = ArrayPool<Vertex>.Shared.Rent(text.TextStr.Length * 6);
        bounds = new FloatRect(pos, (0, 0));

        pos.Y += FontSize;

        for (int i = 0; i < text.TextStr.Length; i++)
        {
            var handle = new KGlyphHandle(text.TextStr[i], (byte)FontSize, text.Bold);

            if (text.TextStr[i] == '\n')
            {
                pos.X = bounds.Position.X;
                pos.Y += FontSize + lnSpacing;

                buffer[i * 6] = default;
                buffer[i * 6 + 1] = default;
                buffer[i * 6 + 2] = default;
                buffer[i * 6 + 3] = default;
                buffer[i * 6 + 4] = default;
                buffer[i * 6 + 5] = default;
                continue;
            }

            if (!_glyphCache.TryGetValue(handle, out Glyph glyph))
            {
                glyph = GetGlyph(handle, true);
                _glyphCache.Add(handle, glyph);
            }

            buffer[i * 6] = new Vertex
            {
                Position = pos + glyph.Bounds.Position,
                Color = text.Color,
                TexCoords = (Vector2f)glyph.TextureRect.Position,
            };
            buffer[i * 6 + 1] = new Vertex
            {
                Position = (pos.X + glyph.Bounds.Left + glyph.Bounds.Width, pos.Y + glyph.Bounds.Top),
                Color = text.Color,
                TexCoords = (glyph.TextureRect.Left + glyph.TextureRect.Width, glyph.TextureRect.Top),
            };
            buffer[i * 6 + 2] = new Vertex
            {
                Position = (pos.X + glyph.Bounds.Left, pos.Y + glyph.Bounds.Top + glyph.Bounds.Height),
                Color = text.Color,
                TexCoords = (glyph.TextureRect.Left, glyph.TextureRect.Top + glyph.TextureRect.Height),
            };

            buffer[i * 6 + 3] = new Vertex
            {
                Position = (pos.X + glyph.Bounds.Left + glyph.Bounds.Width, pos.Y + glyph.Bounds.Top),
                Color = text.Color,
                TexCoords = (glyph.TextureRect.Left + glyph.TextureRect.Width, glyph.TextureRect.Top),
            };
            buffer[i * 6 + 4] = new Vertex
            {
                Position = pos + glyph.Bounds.Position + glyph.Bounds.Size,
                Color = text.Color,
                TexCoords = (Vector2f)(glyph.TextureRect.Position + glyph.TextureRect.Size),
            };
            buffer[i * 6 + 5] = new Vertex
            {
                Position = (pos.X + glyph.Bounds.Left, pos.Y + glyph.Bounds.Top + glyph.Bounds.Height),
                Color = text.Color,
                TexCoords = (glyph.TextureRect.Left, glyph.TextureRect.Top + glyph.TextureRect.Height),
            };

            pos.X += glyph.Advance;

            if (wrapThres > 0 && bounds.Size.X + glyph.Advance > wrapThres)
            {
                pos.X = bounds.Position.X;
                pos.Y += FontSize + lnSpacing;
            }

            if (pos.X - bounds.Position.X > bounds.Width)
            {
                bounds.Size.X = pos.X - bounds.Position.X;
            }
        }

        bounds.Size.Y = pos.Y - bounds.Position.Y;
        pos.Y -= FontSize;

        VBuffer.DrawBuffer(buffer, (uint)text.TextStr.Length * 6, ref Region);

        ArrayPool<Vertex>.Shared.Return(buffer);
    }
}

