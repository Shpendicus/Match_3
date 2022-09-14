﻿using Raylib_cs;
using Match_3;

//INITIALIZATION:................................

class Program
{
    private static GameTime timer;
    private static Grid<Tile> _tileMap;
    private static readonly ISet<Tile> MatchesOf3 = new HashSet<Tile>(3);
    private static Tile? secondClickedTile;
    private static bool isUndoPressed;
    private static readonly HashSet<Tile> UndoBuffer = new (5);
   
    public static int WindowWidth;
    public static int WindowHeight;
   
    private static void Main(string[] args)
    {
        //Now I wanna give the player the task to collect
        //X-Reds, Y-Blues, Z-Greens
        Initialize();
        GameLoop();
        CleanUp();
    }

    private static void Initialize()
    {
        timer = GameTime.GetTimer(30);
        GameTasks.SetQuest();
        GameTasks.LogQuest();
        _tileMap = new(14, 8, timer);
        WindowWidth = _tileMap.TileWidth * Grid<Tile>.TileSize;
        WindowHeight = _tileMap.TileHeight * Grid<Tile>.TileSize;
        Raylib.SetTargetFPS(60);
        Raylib.InitWindow(WindowWidth, WindowHeight, "Match3 By Alex und Shpend");
        AssetManager.Init();
    }

    private static void GameLoop()
    {
        while (!Raylib.WindowShouldClose())
        {
            Raylib.BeginDrawing();
            Raylib.ClearBackground(Color.BEIGE);
            timer.UpdateTimerOnScreen();
            _tileMap.Draw();
            
            ////WORKS GOOD!:
            /*
             * if (timer.TimerDone())
             
            {
                Console.WriteLine("TIMER IS AT 3 SEC NOW!" + Raylib.GetFrameTime());
                timer.Reset();
            }*/

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
        
        /*Different tile selected => swap*/
        firstClickedTile.Selected = true;
        _tileMap.Swap(firstClickedTile, secondClickedTile);
        UndoBuffer.Add(firstClickedTile as Tile);
        UndoBuffer.Add(secondClickedTile);
        secondClickedTile.Selected = false;
        
        if (_tileMap.MatchInAnyDirection(secondClickedTile!.Cell, MatchesOf3))
        {
            UndoBuffer.Clear();
            //Console.WriteLine("FOUND A MATCH-3");
            int tileCounter = 0;
            
            foreach (var match in MatchesOf3)
            {
                if (GameTasks.TryGetQuest(match.TileShape, out int toCollect) &&
                    !GameTasks.IsQuestDone(match.TileShape, tileCounter))
                {
                    if (++tileCounter == toCollect)
                    {
                        Console.WriteLine($"Good job, you got your {tileCounter} match3! by {match.TileShape.Kind}");
                        GameTasks.RemoveSubQuest(match.TileShape);
                        tileCounter = 0;
                    }
                    //  Console.WriteLine($"You sill have to collect: {toCollect- tileCounter}");
                }
                _tileMap.Delete(match.Cell);
                //Console.WriteLine(match)
            }
        }
        MatchesOf3.Clear();
        secondClickedTile = null;        
        firstClickedTile.Selected = false;
    }

    private static void CleanUp()
    {
        Raylib.UnloadTexture(AssetManager.SpriteSheet);
        Raylib.CloseWindow();
    }
    
    private static void UndoAllOperations()
    {
        bool keyDown = (Raylib.IsKeyDown(KeyboardKey.KEY_A));

        //UNDO...!
        if (keyDown)
        {
            bool wasSwappedBack = false;

            foreach (Tile storedItem in UndoBuffer)
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
                    tmp.ChangeTo(Color.WHITE);
                }
                if (!wasSwappedBack)
                    if (Grid<Tile>.MatchXTrigger is { })
                        _tileMap.Swap(_tileMap[Grid<Tile>.MatchXTrigger.CoordsB4Swap],
                            _tileMap[Grid<Tile>.MatchXTrigger.Cell]);
            }
            UndoBuffer.Clear();
        }
    }
}

    