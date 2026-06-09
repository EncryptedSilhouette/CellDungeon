using SFML.Graphics;
using SFML.Window;

public class KResourceManager
{
    private string _configPath;
    public KWorldRenderer[] RenderLayers;


    public KResourceManager(string configPath)
    {
        _configPath = configPath;

    }

    public int InitAndLoad()
    {
        int RenderScale = (int)VideoMode.DesktopMode.Size.X / 640;
        if (_configPath is null)
        {
            try
            {
                var lines = File.ReadAllLines("conf.csv");

                for (int i = 0; i < lines.Length; i++)
                {
                    switch (lines[i])
                    {
                        case "title":
                            Window.SetTitle(lines[i + 1]);
                            break;

                        default:
                            break;
                    }
                }
            }
            catch
            {

            }
        }
        else
        {

        }

        Window.Closed += (_, _) => Running = false;
        Window.SetFramerateLimit(60);

        //Single vertexBuffer for entire program.
        //This will need constant tweaking until a better system is created.
        //This buffer is split into regions for render layers and differing primitives.

        VBuffer = new KDrawBuffer
        {
            Buffer = new VertexBuffer(18_000, PrimitiveType.Triangles, VertexBuffer.UsageSpecifier.Dynamic),
            Regions =
            [
                new KBufferRegion(0, 6_000),      //Each region represents a range of verticies within the VertexBuffer.
                new KBufferRegion(6_000, 6_000),
                new KBufferRegion(12_000, 6_000),
            ],
        };

        //Load default atlas.
        LoadAtlas("assets/atlas.csv", out KTextureAtlas atlas);
        TextureAtlas = atlas;

        #region Render Layers

        KWorldRenderer worldLayer = new(
            new KCanvas
            {
                Resolution = (640, 360),
                Position = (0, 0),
                Anchor = KCanvasAnchor.CENTER
            },
            bufferRegions[1])
        {
            States = new RenderStates(atlas.Texture),
            ClearColor = new(255, 255, 255, 50),
        };

        KWorldRenderer[] renderLayers =
        [
            worldLayer,
        ];

        #endregion

        //handles text drawing.
    }

    public void Dispose()
    {
        Window.Dispose();
    }

    //Needs reworking.
    public static bool LoadAtlas(string filePath, out KTextureAtlas atlas)
    {
        var lines = File.ReadAllLines(filePath);
        atlas = new KTextureAtlas
        {
            Texture = null!,
            Sprites = new(),
        };

        for (int i = 0; i < lines.Length; i++)
        {
            if (lines[i] == string.Empty) continue;

            var data = lines[i].Split(',');
            if (data.Length < 1) continue;

            try
            {
                switch (data[0])
                {
                    case "-at":
                        atlas.Texture = new(data[1]);
                        Console.WriteLine($"Loaded texture: {data[1]}.");
                        break;

                    case "-sp":
                        atlas.Sprites.Add(data[1], new FloatRect
                        {
                            Position = (int.Parse(data[2]), int.Parse(data[3])),
                            Size = (int.Parse(data[4]), int.Parse(data[5]))
                        });
                        break;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"failed to read file: {filePath}, {e.Message}.");
            }
        }
        return atlas.Texture is null ? false : true;
    }
}