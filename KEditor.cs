// #if false
// using SFML.Graphics;
// using SFML.System;
// using SFML.Window;

// public enum KEditorTool
// {
//     CURSOR = 0,
//     TILE_BRUSH = 1
// }

// public class KEditor
// {
//     enum KTileLayers : int
//     {
//         TILE_LAYER_0,
//         TILE_LAYER_1,
//         TILE_LAYER_2,
//         TILE_LAYER_3,
//         TILE_LAYER_4,
//         LAYER_COUNT
//     }

//     public const int TILE_SIZE = 4;
        
//     public KInputField InputField;
//     public KTexturePalette Palette;
//     public KRenderManager Renderer;
//     public KInputManager InputManager;

//     public Vector2i TileUnit => new(TILE_SIZE, TILE_SIZE);

//     public KEditor(KRenderManager renderer, KInputManager inputManager)
//     {
//         Renderer = renderer;
//         InputManager = inputManager;

//         InputField = new(new KButton(
//             new KSprite
//             {
//                 Rotation = 0.0f,
//                 Color = new(200, 200, 200),
//                 Rotocenter = (0, 0),
//                 Bounds = new((16, 16), (80, 40)),
//                 TextureBounds = new(),
//                 Frames = []
//             }, new("Test", Color.White, 0, size: 14)));

//         KTileMap[] tileMaps =
//         [
//             new(new(200, 200, 0, 0, TILE_SIZE, TILE_SIZE))
//             {
//                 Enabled = true    
//             },
//             new(new(200, 200, 0, 0, TILE_SIZE, TILE_SIZE))
//             {
//                 Enabled = true    
//             }
//         ];

//         Palette = new(tileMaps);
//     }

//     public void Init(KRenderManager renderer, KTextureAtlas atlas)
//     {
//         renderer.Window.Resized += (_, e) =>
//         {
//             ref var ll = ref KProgram.Renderer.DrawLayers[(int)KProgram.KLayers.LINE];
//             ll.Size = (Vector2f)e.Size;
//         };
//         Palette.Init(atlas.Texture);
//     }

//     public void Update(uint currentFrame)
//     {
//         //value to downscale mouse/screen coords to layer's coords.
//         var downScale = 1 / KProgram.DrawLayers[(int)KProgram.KLayers.DEFAULT].GetScaleXRelativeTo(KProgram.Window.Size.X);

//         if (InputManager.IsKeyPressed(Keyboard.Key.Q)) Palette.Enabled = !Palette.Enabled; 
        
//         //InputField.Update(InputManager);
//     }

//     public void FrameUpdate(uint currentFrame, KRenderManager renderer)
//     {
//         if (Palette.Enabled) Palette.FrameUpdate(renderer, InputManager, 
//             (int)KProgram.KLayers.DEFAULT, 
//             (int)KProgram.KLayers.LINE);

//         if (!Palette.Enabled) 
//         {
//             renderer.TextHandler.DrawText(
// @"Help:
// Q toggle palette,
// E: toggle texture
// L: toggle layer
// F: next"
//                 ,(16, 64), 0, Color.White, out FloatRect b);
//         }

//         //InputField.FrameUpdate(Renderer, (byte)KProgram.KLayers.TEXT_DEFAULT);
//     }
// }
// #endif