using SFML.Graphics;
using SFML.System;
using SFML.Window;

public struct KTextureAtlas
{
    public Texture Texture;
    public Dictionary<string, FloatRect> Sprites;
}

public static class KProgram
{
    //TODO 
    //Error handling

    private static string s_title;

    public static bool Running = false;
    public static RenderWindow Window;
    public static KRenderManager RenderManager;
    public static KInputManager InputManager;
    public static KGameManager GameManager;
    public static KTextureAtlas TextureAtlas;

    public static string Title
    {
        get => s_title;
        set
        {
            s_title = value;
            Window.SetTitle(value);
        }
    }

    static KProgram()
    {
        s_title = "dungeons";

        Window = new(VideoMode.DesktopMode, Title);
        Window.SetFramerateLimit(60);
        Window.Closed += (_, _) => Running = false;

        LoadAtlas("assets/atlas.csv", out KTextureAtlas atlas);

        TextureAtlas = atlas;

        VertexBuffer vBuffer = new(12_000, PrimitiveType.Triangles, VertexBuffer.UsageSpecifier.Dynamic);

        KRenderLayer[] renderLayers =
        [
            new()
            {
                RenderTexture = new(Window.Size / 8),
                Bounds = new FloatRect((0,0), (Vector2f)Window.Size),
                Primitive = PrimitiveType.Triangles,
                States = new RenderStates(atlas.Texture),
                ClearColor = Color.Transparent,
                Region = new KBufferRegion(0, 6_000),
            },
        ];

        KTextHandler textHandler = new(new Font("assets/Roboto-Black.ttf"), vBuffer, new(6_000, 6_000));

        RenderManager = new(Window, vBuffer, renderLayers, textHandler);
        InputManager = new(Window);
        GameManager = new();

    }

    public static void Main()
    {
        Running = true;

        while (Running)
        {
            GameManager.Update();

            Window.Clear();

            GameManager.FrameUpdate(RenderManager);
            RenderManager.FrameUpdate();

            Window.Display();

            InputManager.Update();
            Window.DispatchEvents();
        }
    }

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