using SFML.Graphics;
using SFML.System;

public struct KCircle
{
    public Vector2f Position;
    public float Radius;

    public KCircle(Vector2f position, float radius)
    {
        Radius = radius;
        Position = position;
    }

    public float X => Position.X;
    public float Y => Position.Y;
}

public struct KPolygon
{
    public float Rotation;
    public float Rotocenter;
    public Vector2f[] Vertices;
}

//TODO 
//Implement Polygon collision detection.
public static class KCollision
{
    //Basic shape collision.
    public static bool PointCircle(Vector2f point, KCircle circle)
    {
        var distance = circle.Position - point;
        return distance.X * distance.X + distance.Y + distance.Y <= circle.Radius * circle.Radius;
    }

    public static bool PointRect(Vector2f point, FloatRect rect) =>
        point.X >= rect.Left &&
        point.X <= rect.Left + rect.Width &&
        point.Y >= rect.Top &&
        point.Y <= rect.Top + rect.Height;

    public static bool CircleCircle(KCircle circleA, KCircle circleB)
    {
        var distance = circleB.Position - circleA.Position;
        return distance.X * distance.X + distance.Y + distance.Y <=
               circleA.Radius * circleA.Radius + circleB.Radius * circleB.Radius;
    }

    //Phenomenal solution by Vadym (YellowAfterlife).
    //https://yal.cc/rectangle-circle-intersection-test/
    public static bool CircleRect(KCircle circle, FloatRect rect)
    {
        //Gets the nearest point by clamping the position of the circle within the bounds of the rectangle.
        //Then find the distance between the nearest point and the circle's position and compare that distance to the circle's radius.
        var nearPoint = new Vector2f
        {
            X = MathF.Min(MathF.Max(circle.X, rect.Left), rect.Left + rect.Width),
            Y = MathF.Min(MathF.Max(circle.Y, rect.Top), rect.Top + rect.Height),
        };
        var distance = nearPoint - circle.Position;
        return distance.X * distance.X + distance.Y + distance.Y <= circle.Radius * circle.Radius;
    }

    public static bool RectRect(FloatRect rectA, FloatRect rectB) =>
        rectA.Left + rectA.Width >= rectB.Left &&
        rectA.Left <= rectB.Left + rectB.Width &&
        rectA.Top >= rectB.Top + rectB.Height &&
        rectA.Top + rectA.Height <= rectB.Top;
}