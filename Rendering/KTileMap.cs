// #if false
// using SFML.Graphics;

// public struct KTileMap
// {
//     public bool Enabled;
//     public KGrid Grid;
//     public Vertex[] Buffer;
//     public FloatRect[] TileSet;

//     public KTileMap(in KGrid grid, FloatRect[] tileSet)
//     {
//         Grid = grid;
//         TileSet = tileSet;   
//         Buffer = new Vertex[grid.CellCount * 6];
//     }

//     public KTileMap(in KGrid grid) : this(grid, []) { }

//     public Vertex[] BakeVertices()
//     {
//         for (int i = 0; i < Grid.CellCount; i++)
//         { 
//             Color color;
//             FloatRect tRect;
//             var pos = Grid.IndexToCoords(i);
            
//             color = Color.White;
//             tRect = TileSet[Grid.Cells[i]];

//             Buffer[i * 6] = new(pos, color, tRect.Position);
//             Buffer[i * 6 + 1] = new((pos.X + Grid.CellWidth, pos.Y), color, (tRect.Left + tRect.Width, tRect.Top));   
//             Buffer[i * 6 + 2] = new((pos.X, pos.Y + Grid.CellHeight), color, (tRect.Left, tRect.Top + tRect.Height));

//             Buffer[i * 6 + 3] = new((pos.X + Grid.CellWidth, pos.Y), color, (tRect.Left + tRect.Width, tRect.Top));   
//             Buffer[i * 6 + 4] = new(pos + (Grid.CellWidth, Grid.CellHeight), color, tRect.Position + tRect.Size);   
//             Buffer[i * 6 + 5] = new((pos.X, pos.Y + Grid.CellHeight), color, (tRect.Left, tRect.Top + tRect.Height));   
//         }
//         return Buffer;
//     }

//     public void FrameUpdate(KRenderManager renderer, int layer)
//     {
//         if (renderer.DrawLayers.Length <= layer) return;
//         renderer.DrawBuffer(Buffer, (uint)Buffer.Length, layer);
//     }
// }
// #endif