﻿using Raylib_cs;
using System.Diagnostics;
using System.Numerics;
using Match_3;

//INITIALIZATION:................................

class Program
{
    public static Texture2D TileSheet { get; private set; }
    private static Stopwatch _stopwatch = new();
  //private static TileMap _tileMap = new(8, 8);
    private static Tilemap _tileMap;
    private static ISet<ITile> _matches = new HashSet<ITile>(3);
    
    private const int _tileSize = 64;
    private const int _tileCountX = 8;
    private const int _tileCountY = 8;

    public static readonly Int2 WindowSize = new Int2(_tileCountX, _tileCountY) * _tileSize;
    public static readonly Int2 TileSize = new Int2(_tileSize);

    private static Tile? secondClickedTile;
    private static bool isUndoPressed;
    private static HashSet<Tile> undoBuffer = new (5);
   
    private static void Main(string[] args)
    {
        Initialize();
        GameLoop();
        CleanUp();
    }

    private static void Initialize()
    {
        Raylib.InitWindow(WindowSize.X, WindowSize.Y, "Match3 By Alex und Shpend");
        string net6Path = Environment.CurrentDirectory;
        const string projectName = "Match3";
        int lastProjectNameOccurence = net6Path.LastIndexOf(projectName) + projectName.Length;
        var fontPath = $"{net6Path.AsSpan(0, lastProjectNameOccurence)}/Assets/font3.ttf";
        var tilePath = $"{net6Path.AsSpan(0, lastProjectNameOccurence)}/Assets/shapes.png";
        Console.WriteLine(tilePath);
        TileSheet = Raylib.LoadTexture(tilePath);
        _tileMap = new(TileSheet,8, 8);
        Tile.FontPath = fontPath;
        _stopwatch = Stopwatch.StartNew();
        Raylib.SetTargetFPS(60);
    }

    private static void GameLoop()
    {
        while (!Raylib.WindowShouldClose())
        {
            Raylib.BeginDrawing();
            Raylib.ClearBackground(Color.BEIGE);
            _stopwatch.Stop();
            //_tileMap.Draw((float)_stopwatch.Elapsed.TotalSeconds);
            _tileMap.Draw();
            Raylib.DrawFPS(0,0);
            _stopwatch.Restart();
            ProcessSelectedTiles();
            UndoAllOperations();
            Raylib.EndDrawing();
        }
    }

    private static void ProcessSelectedTiles()
    {
        if (!_tileMap.TryGetClickedTile(out var firstClickedTile))
            return;

        //No tile selected yet
        if (secondClickedTile is null)
        {
            secondClickedTile = firstClickedTile as Tile; 
            secondClickedTile.Selected = true;
            return;
        }

        //Same tile selected => deselect
        if (firstClickedTile.Equals(secondClickedTile))
        {
            secondClickedTile.Selected = false;
            secondClickedTile = null;
            return;
        }
        
        //Different tile selected => swap
        firstClickedTile.Selected = true;
        _tileMap.Swap(firstClickedTile, secondClickedTile);
        undoBuffer.Add(firstClickedTile as Tile);
        undoBuffer.Add(secondClickedTile);
        secondClickedTile.Selected = false;
        
        if (_tileMap.MatchInAnyDirection(secondClickedTile!.Cell, _matches))
        {
            undoBuffer.Clear();
            //Console.WriteLine("FOUND A MATCH-3");
            
            foreach (var match in _matches)
            {
                undoBuffer.Add(_tileMap[match.Cell] as Tile); 
                _tileMap[match.Cell] = null;
                //Console.WriteLine(match);
            }
        }
        
        _matches.Clear();
        secondClickedTile = null;        
        firstClickedTile.Selected = false;
    }

    private static void CleanUp()
    {
        Raylib.UnloadTexture(TileSheet);
        Raylib.CloseWindow();
    }
    
    private static void UndoAllOperations()
    {
        bool keyDown = (Raylib.IsKeyDown(KeyboardKey.KEY_A));

        //UNDO...!
        if (keyDown)
        {
            bool wasSwappedBack = false;

            foreach (Tile storedItem in undoBuffer)
            {
                //check if they have been ONLY swapped without leading to a match3
                if (!wasSwappedBack && _tileMap[storedItem.Cell] is not null)
                {
                    var secondTile = _tileMap[storedItem.Cell];
                    var firstTie = _tileMap[storedItem.CoordsB4Swap];
                    _tileMap.Swap(secondTile, firstTie);
                    wasSwappedBack = true;
                }
                else
                {
                    //their has been a match3 after swap!
                    //for delete we dont have a .IsDeleted, cause we onl NULL
                    //a tile at a certain coordinate, so we test for that
                    //if (_tileMap[storedItem.Cell] is { } backupItem)
                    var tmp = (_tileMap[storedItem.Cell] = storedItem) as Tile;
                    tmp!.Selected = false;
                    tmp.Blur = Color.WHITE;
                }
                if (!wasSwappedBack)
                    _tileMap.Swap(_tileMap[Tilemap.MatchXTrigger.CoordsB4Swap], 
                        _tileMap[Tilemap.MatchXTrigger.Cell]);
            }
            undoBuffer.Clear();
        }
    }
}

    