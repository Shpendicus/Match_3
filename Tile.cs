﻿using System.Numerics;
using Raylib_cs;

namespace Match_3;

public enum Shape : sbyte
{
    Ball,
    Cube,
    Zylinder,
    Triangle,
    EMPTY = -1
}

public enum OriginalColor : byte
{
    Blue,
    Yellow,
    Red,
    Green
}

public class Tile : IEquatable<Tile>
{
    private bool wasDrawn;

    public Color Colour { get; set; }
    public OriginalColor OriginColor { get; set; }
    public Rectangle DrawDestination { get; set; }
    public Int2 Coords { get; set; }
    public static Texture2D SpriteSheet { get; set; }
    public static Texture2D DestroyedTile { get; set; }
    public static string FontPath { get; set; }
    public bool Swapped { get; set; } = true;
    public Shape Shape { get; set; } = Shape.EMPTY;

    private static Font font;

    private bool selected;

    public bool Selected
    {
        get => selected;

        set
        {
            if (!value)
            {
                targetAlpha = 1;
            }
            else
            {
                targetAlpha = currentAlpha = 0f;
            }

            selected = value;
        }
    }

    private float targetAlpha = 1;
    private float currentAlpha = 1;
    private const float alphaSpeed = 2f;

    private Tile()
    {
    }

    private static Tile CreateNewTile(Rectangle drawDestinationRectangle, OriginalColor originCol, Int2 coord,
        bool swapped, Shape kind, Color color)
    {
        var mapTile = new Tile
        {
            DrawDestination =
                drawDestinationRectangle, // this one was calculated from the current map position and the descriptors map offset values.
            Swapped = swapped,
            Shape = kind,
            Colour = color,
            OriginColor = originCol,
            Coords = coord
        };

        return mapTile;
    }

    private static readonly Rectangle[] frames = {
        new Rectangle(0f, 0f, 64f, 64f), //blue
        new Rectangle(64f, 0f, 64f, 64f), //yellow
        new Rectangle(0f, 64f, 64f, 64f), //red
        new Rectangle(64f, 64f, 64f, 64f), //green
    };

    private static Shape IdentifyTileKind(Rectangle current)
    {
        Shape tmp = Shape.EMPTY;

        switch (current)
        {
            case Rectangle ballRec when ballRec.x == 0f && ballRec.y == 0f:
                tmp = Shape.Ball;
                break;

            case Rectangle cubeRec when cubeRec.x == 64f && cubeRec.y == 0f:
                tmp = Shape.Cube;
                break;

            case Rectangle zylinderRec when zylinderRec.x == 0f && zylinderRec.y == 64f:
                tmp = Shape.Zylinder;
                break;

            case Rectangle triangleRec when triangleRec.x == 64f && triangleRec.y == 64f:
                tmp = Shape.Triangle;
                break;
        }

        return tmp;
    }

    private static Tile BLUE_BALL = CreateNewTile(frames[0],
        OriginalColor.Blue,
        -Int2.One,
        false,
        IdentifyTileKind(frames[0]),
        Color.WHITE);


    private static Tile YELLOW_CUBE = CreateNewTile(frames[1],
        OriginalColor.Yellow,
        -Int2.One,
        false,
        IdentifyTileKind(frames[1]),
        Color.WHITE);


    private static Tile RED_ZYLINDER = CreateNewTile(frames[2],
        OriginalColor.Red,
        -Int2.One,
        false,
        IdentifyTileKind(frames[2]),
        Color.WHITE);


    private static Tile GREEN_TRIANGLE = CreateNewTile(frames[3],
        OriginalColor.Green,
        -Int2.One,
        false,
        IdentifyTileKind(frames[3]),
        Color.WHITE);

    public static Tile GetRandomTile()
    {
        var id = (Shape) Random.Shared.Next((int) Shape.Ball, (int) (Shape.Triangle) + 1);

        switch (id)
        {
            case Shape.Ball:
                return CreateNewTile(frames[0],
                    OriginalColor.Blue,
                    -Int2.One,
                    false,
                    IdentifyTileKind(frames[0]),
                    Color.WHITE);

            case Shape.Cube:
                return CreateNewTile(frames[1],
                    OriginalColor.Yellow,
                    -Int2.One,
                    false,
                    IdentifyTileKind(frames[1]),
                    Color.WHITE);
                ;

            case Shape.Zylinder:
                return CreateNewTile(frames[2],
                    OriginalColor.Red,
                    -Int2.One,
                    false,
                    IdentifyTileKind(frames[2]),
                    Color.WHITE);
                ;

            case Shape.Triangle:
                return CreateNewTile(frames[3],
                    OriginalColor.Green,
                    -Int2.One,
                    false,
                    IdentifyTileKind(frames[3]),
                    Color.WHITE);
                ;
        }

        throw new ArgumentNullException("This code shall NEVER be reached!");
    }

    public override string ToString() => $"Position: {Coords}; Tilekind: {Shape} OriginColor: {OriginColor.ToString()}";

    float Lerp(float firstFloat, float secondFloat, float by)
    {
        return firstFloat * (1 - by) + secondFloat * by;
    }

    public virtual void Draw(float deltaTime)
    {
        if (!wasDrawn)
        {
            font = Raylib.LoadFont(FontPath);
            wasDrawn = true;
        }

        Vector2 worldPosition = new Vector2(Coords.X, Coords.Y) * Program.TileSize;

        Color drawColor = Selected ? Color.GRAY : Colour;
        drawColor = Raylib.ColorAlpha(drawColor, currentAlpha);
        currentAlpha = Lerp(currentAlpha, targetAlpha, alphaSpeed * deltaTime);

        Raylib.DrawTextureRec(SpriteSheet, DrawDestination, worldPosition, drawColor);

        float xCenter = worldPosition.X + Program.TileSize.X / 4.3f;
        float yCenter = worldPosition.Y < 128f ? worldPosition.Y + Program.TileSize.Y / 2.5f :
            worldPosition.Y >= 128f ? (worldPosition.Y + Program.TileSize.Y / 2f) - 5f : 0f;

        Vector2 drawPos = new(xCenter - 10f, yCenter);
        Raylib.DrawTextEx(font, Coords.ToString(), drawPos, 20f, 1f, Color.BLACK);
    }

    public bool Equals(Tile? other)
    {
        return Shape == other?.Shape &&
               OriginColor == other.OriginColor;
    }
}