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

    private static string s_title;

    public static bool Running = false;
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
        }
    }

    static KProgram() //Initialization.
    {
        
        //Initializes systems.
        RenderManager = new();
        InputManager = new();
        GameManager = new(InputManager, []);

        //If all succeed then allow the program to run.
        Running = true;
    }

    public static void Main()
    {
        ulong currentFrame = 0;

        while (Running)
        {
            Update(currentFrame);
            FrameUpdate(currentFrame);

            currentFrame++;
        }
    }

    public static void Update(ulong currentFrame)
    {
        InputManager.Update();
        GameManager.Update(currentFrame);
    }

    public static void FrameUpdate(ulong currentFrame)
    {
        GameManager.FrameUpdate(currentFrame, RenderManager);
        RenderManager.FrameUpdate(currentFrame);
    }

    //Resoution shit.
    public static int GreatestCommonFactor(int a, int b)
    {
        var smaller = Math.Min(a, b);
        var factor = 1;

        for (int i = 2; i <= smaller; i++)
        {
            if ((a % i == 0) && (b % i == 0)) factor = i;
        }

        return factor;
    }
    public static int GreatestCommonFactor(Vector2i values) =>
        GreatestCommonFactor(values.X, values.Y);
        
}