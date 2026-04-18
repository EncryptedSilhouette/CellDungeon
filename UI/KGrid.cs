using System.Buffers;
using SFML.Graphics;
using SFML.System;

public struct KGrid
{
    public int Columns;
    public int Rows;
    public Color LineColor;
    public Vector2i CellSize;
    public Vector2f Position;
    public uint[] Cells;

    public int CellCount => Columns * Rows;

    public KGrid()
    {
        Rows = Columns = 0;
        CellSize = new();
        LineColor = Color.White;
        Cells = [];
    }

    public KGrid(int columns, int rows, Vector2f position, Vector2i size)
    {
        Columns = columns;
        Rows = rows;
        LineColor = Color.White;
        CellSize = size;
        Position = position;
        Cells = new uint[Rows * Columns];
        Array.Fill<uint>(Cells, 0);
    }

    public void FrameUpdate(KRenderManager renderer, int lineLayer)
    {
        uint vCount = 0;
        var buffer = ArrayPool<Vertex>.Shared.Rent(Columns * 2 + Rows * 2);

        for (int i = 0; i < Rows; i++)
        {
            buffer[vCount] = new((Position.X, Position.Y + i * CellSize.X), LineColor);
            buffer[vCount + 1] = new((Position.X + Columns * CellSize.X, Position.Y + i * CellSize.Y), LineColor);
            vCount += 2;
        }
        for (int i = 0; i < Columns; i++)
        {
            buffer[vCount] = new((Position.X + i * CellSize.X, Position.Y), LineColor);
            buffer[vCount + 1] = new((Position.X + i * CellSize.X, Position.Y + Rows * CellSize.Y), LineColor);
            vCount += 2;
        }

        renderer.DrawBuffer(buffer, vCount, lineLayer);

        ArrayPool<Vertex>.Shared.Return(buffer);
    }

    public bool PositionToIndex(int column, int row, out int index)
    {
        index = column + row * Columns;

        //Bounds checking. False if out of bounds.
        return index >= 0 && index < Cells.Length;
    }

    public bool CoordsToIndex(Vector2f pos, out int index, float scale = 1)
    {
        pos -= Position * scale;
        int column = (int)(pos.X / CellSize.X * scale);
        int row = (int)(pos.Y / CellSize.Y * scale);

        PositionToIndex(column, row, out index);

        //Bounds checking. False if out of bounds.
        return index >= 0 && index < Cells.Length;
    }

    public Vector2f IndexToPosition(int index) => (index % Columns, index / Columns);

    public Vector2f IndexToCoords(int index, float scale = 1) => new()
    {
        X = Position.X + (int)(index % Columns) * CellSize.X * scale,
        Y = Position.Y + (int)(index / Columns) * CellSize.Y * scale,
    };
}