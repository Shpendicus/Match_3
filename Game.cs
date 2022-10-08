﻿using System.Numerics;
using Match_3.GameTypes;
using Raylib_CsLo;
using static Match_3.AssetManager;
using static Match_3.Utils;
using static Raylib_CsLo.Raylib;

namespace Match_3;

internal static class Game
{
    public static QuestState Quest { get; private set; }
    public static Level Level { get; private set; }
    private static Grid _grid;
    private static MatchX? _matchesOf3;
    private static EnemyMatches? _enemyMatches;
    private static Tile? _secondClicked;
    
    private static bool _enterGame;
    private static bool _shallCreateEnemies;

    public static event Action<QuestState> OnMatchFound;
    public static event Action<QuestState> OnTileClicked;
    public static event Action<QuestState> OnTileSwapped;
    
    public static event Action<QuestState> OnEnemyTileClicked;
    
    private static void Main()
    {
        InitGame();
        MainGameLoop();
        CleanUp();
    }

    private static void InitGame()
    {
        Level = new(0,45, 4, 10, 10, 64);
        Quest = new()
        {
            EventData = new Dictionary<Type, Numbers>((int)Type.Length)
        };
        _matchesOf3 = new(Level.MAX_TILES_PER_MATCH);
        SetTargetFPS(60);
        SetConfigFlags(ConfigFlags.FLAG_WINDOW_RESIZABLE);
        InitWindow(Level.WindowWidth, Level.WindowHeight, "Match3 By Shpendicus");
        LoadAssets();
        QuestHandler.InitAllQuestHandlers(Level.ID);
        _grid = new(Level);
    }
    
    private static void CenterMouseToEnemyMatch()
    {
        //we only fix the mouse point,
        //WHEN the cursor exceeds at a certain bounding box
        if (_enemyMatches is { } && _enemyMatches.WorldPos != INVALID_CELL)
        {
            bool outsideRect = !CheckCollisionPointRec(GetMousePosition(), _enemyMatches.Border);

            if (outsideRect && Quest.EnemiesStillPresent)
            {
                /*the player has to get these enemies out of the way b4 he can pass!*/
                SetMouseToWorldPos(_enemyMatches.WorldPos, 1);
            }
            else if (!Quest.EnemiesStillPresent && _enemyMatches.IsMatch)
            {
                //we set this to null, because we cant make any swaps after this, cause 
                //_secondClicked has a value! so we then can repeat the entire cycle!
                _secondClicked = null;
                //enemies were created from the matchesOf3, so we have to
                //delete all of them, because else we will reference always the base-matches internally which is bad!
                _enemyMatches.Clear(); 
                _matchesOf3?.Clear();
                //move freely
            }
        }
    }

    private static bool TileClicked(out Tile? tile)
    {
        tile = default!;

        if (!IsMouseButtonPressed(MouseButton.MOUSE_BUTTON_LEFT))
            return false;

        var mouseVec2 = GetMousePosition();
        Vector2 gridPos = new Vector2((int)mouseVec2.X, (int)mouseVec2.Y);
        gridPos /= Tile.Size;
        tile = _grid[gridPos];
        return tile is { };
    }
    
    private static void ProcessSelectedTiles()
    {
        if (!TileClicked(out var firstClickedTile))
            return;

        //Enemy tile was clicked on , ofc after a matchX happened!
        if (firstClickedTile is EnemyTile e)
        {
            Quest.Enemy = e;
            Quest.Grid = _grid;
            OnEnemyTileClicked(Quest);
        }
        
        firstClickedTile!.Select();
        
        /*No tile selected yet*/
        if (_secondClicked is null)
        {
            //prepare for next round, so we store first in second!
            _secondClicked = firstClickedTile;
            //_secondClicked.Select();
            return;
        }

        /*Same tile selected => deselect*/
        if (StateAndBodyComparer.Singleton.Equals(firstClickedTile, _secondClicked))
        {
            Console.Clear();
            //Console.WriteLine($"{tmpFirst.GridCell} was clicked AGAIN!");
            _secondClicked.DeSelect();
            _secondClicked = null;
        }
        /*Different tile selected ==> swap*/
        else
        {
            firstClickedTile.TileState |= TileState.Selected;

            if (_grid.Swap(firstClickedTile, _secondClicked))
            {
                Quest.WasSwapped = true;
                OnTileSwapped(Quest);
            }
        }
    }
    
    static void ComputeMatches()
    {
        if (!Quest.WasSwapped)
            return;

        bool ShallTransformMatchesToEnemyMatches() => Randomizer.NextSingle() <= 0.5f;

        void CreateEnemiesIfNeeded()
        {
            if (_shallCreateEnemies && 
                (_enemyMatches is null || _enemyMatches.Count == 0) &&
                _matchesOf3?.Count > 0)
            {
                _enemyMatches = _matchesOf3.AsEnemies(_grid);
            }
        }
        
        if (_grid.WasAMatchInAnyDirection(_secondClicked!, _matchesOf3!) /*&& !_shallCreateEnemies*/)
        {
            if (_secondClicked?.Body is TileShape t)
            {
                Quest.EventData.TryGetValue(t.TileType, out var matchData);
                matchData.Match.Count++;
                matchData.Match.Time = Level.GameTimer.ElapsedSeconds;
                Quest.EventData.TryAdd(t.TileType, matchData);
                OnMatchFound(Quest);
            }

            Quest.EnemiesStillPresent = _shallCreateEnemies = true;
            CreateEnemiesIfNeeded();
        }
        else
            _matchesOf3!.Clear();
        
        Quest.WasSwapped = false;
        _secondClicked = null;
    }
    
    private static void HardReset()
    {
        if (IsKeyDown(KeyboardKey.KEY_A))
        {
            _grid = new Grid(Level);
            _shallCreateEnemies = true;
            _matchesOf3?.Clear();
            _enemyMatches?.Clear();
            Quest.EnemiesStillPresent = false;
            _secondClicked = null;
            Quest.WasSwapped = false;
            Console.Clear();
        }
    }

    private static void MainGameLoop()
    {
        while (!WindowShouldClose())
        {
            BeginDrawing();
            ClearBackground(WHITE);
            Renderer.DrawBackground();

            if (!_enterGame)
            {
                Renderer.ShowWelcomeScreen();
                //Renderer.LogQuest(false, Level);
            }
            if (IsKeyDown(KeyboardKey.KEY_ENTER) || _enterGame)
            {
                Level.GameTimer.Run();
                
                bool isGameOver = Level.GameTimer.Done();

               // Console.WriteLine("TIME  : "+Level.GameTimer.ElapsedSeconds);
                
                if (isGameOver)
                {
                    if (Renderer.OnGameOver(Level.GameTimer.Done(),false))
                    {
                        //leave Gameloop and hence end the game
                        return;
                    }
                }
                else if (Quest.WasGameWonB4Timeout)
                {
                    if (Renderer.OnGameOver(Level.GameTimer.Done(), true))
                    {
                        InitGame();
                        Quest.WasGameWonB4Timeout = false;
                        continue;
                    }
                }
                else
                {
                    float elapsedTime = Level.GameTimer.ElapsedSeconds;
                    Renderer.DrawTimer(elapsedTime);
                    CenterMouseToEnemyMatch();
                    ProcessSelectedTiles();
                    ComputeMatches();
                    Renderer.DrawOuterBox(_enemyMatches,elapsedTime);  //works!
                    Renderer.DrawInnerBox(_matchesOf3, elapsedTime) ;  //works!
                    Renderer.DrawGrid(_grid, elapsedTime);
                    HardReset();
                }
                _enterGame = true;
            }
            EndDrawing();
        }
    }

    private static void CleanUp()
    {
        UnloadTexture(DefaultTileAtlas);
        CloseWindow();
    }
}