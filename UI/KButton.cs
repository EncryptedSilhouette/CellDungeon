// using SFML.Graphics;
// using SFML.System;

// [Flags]
// public enum KButtonState : byte
// {
//     NONE = 0,
//     HOVER = 1,
//     PRESSED = 1 << 1,
//     HELD = 1 << 2,
//     RELEASED = 1 << 3
// }

// public struct KButton
// {
//     private bool _isDown = false;

//     public Color Color;
//     public Color HeldColor;
//     public Color DownColor;
//     public KSprite Sprite;
//     public KButtonState States;
//     public KText Text;

//     public KButton(KSprite sprite, KText text)
//     {
//         Color = new(200, 200, 200);
//         HeldColor = new(125, 125, 125);
//         DownColor = new(100, 100, 100);
//         Text = text;
//         Sprite = sprite;
//     }

//     public void Update(KInputManager inputManager, Vector2f position)
//     {
//         if (KProgram.CheckPointRectCollision(position, Sprite.Bounds))
//         {
//             States |= KButtonState.HOVER;
//             //Color = HeldColor;

//             if (inputManager.IsMouseDown(KMouseStates.M1_DOWN))
//             {
//                 if (!_isDown && !inputManager.PrevMouseStates.HasFlag(KMouseStates.M1_DOWN))
//                 {
//                     _isDown = true;
//                     States |= KButtonState.PRESSED;
//                 }
//                 States |= KButtonState.HELD;
//             }
//             else if (_isDown)
//             {
//                 _isDown = false;
//                 States = KButtonState.RELEASED;
//             }
//         }
//         else if (_isDown)
//         {
//             _isDown = false;
//             //Color = Sprite.Color;
//             States = KButtonState.RELEASED;
//         }
//         else
//         {
//             //Color = Sprite.Color;
//             States = KButtonState.NONE;
//         }
//     }

//     public void FrameUpdate(KRenderManager renderManager, int textLayer)
//     {
//         renderManager.TextRenderer.DrawText(Text, Sprite.Bounds.Position, textLayer, out FloatRect b);

//         var color = States.HasFlag(KButtonState.HOVER) ?
//             HeldColor :
//             Sprite.Color;
//         renderManager.DrawSprite(Sprite, color);
//     }

//     public bool CheckState(KButtonState states) =>
//         States.HasFlag(states);
// }