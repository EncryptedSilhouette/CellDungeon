// using SFML.Graphics;
// using SFML.System;

// public struct KInputField
// {
//     public KButton InputBox;
//     public float MinWidth;
//     public bool HasFocus;
//     public float MinHeight;

//     public Vector2f MinSize => (MinWidth, MinHeight);
//     public Vector2f MaxSize => (MinWidth, MinHeight);
//     public string TextSTR
//     {
//         get => InputBox.Text.TextStr;
//         set
//         {
//             InputBox.Text.TextStr = value;
//         }
//     }

//     public KInputField(KButton inputBox)
//     {
//         HasFocus = false;
//         InputBox = inputBox;
//         MinWidth = inputBox.Sprite.Bounds.Size.X;
//         MinHeight = inputBox.Sprite.Bounds.Size.Y;
//     }

//     public void Update(KInputManager input)
//     {
//         var mpos = new Vector2f(input.MousePosX, input.MousePosY);

//         InputBox.Update(input, mpos);

//         if (InputBox.CheckState(KButtonState.PRESSED))
//         {
//             input.StartTextRead();
//             HasFocus = true;
//             return;
//         }

//         if (input.MouseStates.HasFlag(KMouseStates.M1_DOWN))
//         {
//             if (!KProgram.CheckPointRectCollision(input.GetMousePosition(), InputBox.Sprite.Bounds))
//             {
//                 HasFocus = false;
//                 InputBox.Text.TextStr = input.StopTextRead();
//             }
//         }
//         else
//         {
//             InputBox.Text.TextStr = input.ReadText();
//         }
//     }

//     public void FrameUpdate(KRenderManager renderer, int textLayer)
//     {
//         renderer.TextRenderer.DrawText(InputBox.Text, InputBox.Sprite.Bounds.Position, textLayer, out FloatRect b);

//         InputBox.Sprite.Bounds.Size.X = b.Width > MinWidth ? b.Width : MinWidth;
//         InputBox.Sprite.Bounds.Size.Y = b.Height > MinHeight ? b.Height : MinHeight;

//         var color = InputBox.States.HasFlag(KButtonState.HOVER) || HasFocus ?
//             InputBox.HeldColor :
//             InputBox.Sprite.Color;

//         renderer.DrawSprite(InputBox.Sprite, color);
//     }
// }