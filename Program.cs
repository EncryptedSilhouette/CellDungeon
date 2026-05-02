//There are a few Tenets I try to maintain in my code:
//#1 LEAVE NOTHING null. null is the devil, there is always a better way. 
//#2 Avoid OOP. Once again there usually is a better way, with a couple exceptions.
//#2.1 Additionally this means avoid classes and object allocations when possible.
//#3 Avoid singletons, that's what the KProgram class is for.
//#4 Avoid exceptions; use proper sanitation and error logging.

using SFML.Graphics;
using SFML.System;
using SFML.Window;

public struct KTextureAtlas
{
    public Texture Texture;
    public Dictionary<string, FloatRect> Sprites;
}

//PURE MADNESS.
public static class KWindowExtensions
{
    public static float GetAspect(this Window self) => (float)self.Size.Y / self.Size.X;
}

//This class acts as the foundation for the rest of the program.
//It contains the Main method, and initializes many systems for the application.
//This class stands at the top of the program's heirarchy, 
//and acts as a mediator to access any part of the program.
//Accessing this class anywhere is meant to be temporary, 
//so that functionality can be tested without having to worry about program structure, 
//while polish can be applied "later".
public static class KProgram
{
    //TODO 
    //Error handling

    private static string s_title = "dungeons";
    public static bool Running = false;
    public static KResolution Resolution;
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

    static KProgram() //Initialization.
    {
        VideoMode videoMode = VideoMode.DesktopMode;
        KResolution resolution = new((Vector2i)videoMode.Size);
        
        Window = new(videoMode, Title);
        Window.Closed += (_, _) => Running = false;
        Window.SetFramerateLimit(60);

        //Single vertexBuffer for entire program.
        //This will need constant tweaking until a better system is created.
        VertexBuffer vBuffer = new(18_000, PrimitiveType.Triangles, VertexBuffer.UsageSpecifier.Dynamic);
        //This buffer is split into regions for render layers and differing primitives.
        KBufferRegion[] bufferRegions =
        [
            new(0, 6_000),      //Each region represents a range of verticies within the VertexBuffer.
            new(6_000, 6_000),
            new(12_000, 6_000),
        ];

        //Load default atlas.
        LoadAtlas("assets/atlas.csv", out KTextureAtlas atlas);
        TextureAtlas = atlas;

        #region Render Layers

        KRenderLayer WorldLayer = new(
            new RenderTexture((170, 90)),
            PrimitiveType.Triangles,
            bufferRegions[0])
        {
            Canvas = new KCanvas
            {
                Position = (0,0),
                Scale = new Vector2f(0.66666f, 0.66666f),
                CanvasAnchor = KCanvasAnchor.CENTER
            },
            States = new RenderStates(atlas.Texture),
        };

        KRenderLayer lineLayer = new(
            new(VideoMode.DesktopMode.Size),
            PrimitiveType.Lines,
            bufferRegions[1])
        {
            Canvas = new KCanvas
            {
                Position = (0,0),
                Scale = new Vector2f(1, 1),
                CanvasAnchor = KCanvasAnchor.TOP_LEFT
            },
        };
        //lineLayer.Init(Window);

        KRenderLayer[] renderLayers =
        [
            WorldLayer,
            lineLayer
        ];

        #endregion

        //handles text drawing.
        KTextHandler textHandler = new(new Font("assets/Roboto-Black.ttf"), vBuffer, bufferRegions[2]);

        //Initializes systems.
        RenderManager = new(Window, resolution, vBuffer, renderLayers, textHandler);
        InputManager = new(Window);
        GameManager = new(InputManager);

        //If all succeed then allow the program to run.
        Running = true;
    }

    public static void Main()
    {
        ulong currentFrame = 0;

        while (Running)
        {
            GameManager.Update(currentFrame);

            Window.Clear();

            GameManager.FrameUpdate(RenderManager, currentFrame);
            RenderManager.FrameUpdate();

            Window.Display();

            InputManager.Update();
            Window.DispatchEvents();

            currentFrame++;
        }
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

    //Resoution shit.
    public static int GreatestCommonFactor(int x, int y)
    {
        var smaller = Math.Min(x, y);
        var factor = 1;

        for (int i = 2; i <= smaller; i++)
        {
            if ((x % i == 0) && (y % i == 0)) factor = i;
        }

        return factor;
    }
    public static int GreatestCommonFactor(Vector2i values) =>
        GreatestCommonFactor(values.X, values.Y);
}