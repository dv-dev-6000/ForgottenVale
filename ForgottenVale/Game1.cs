using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using System;
using System.Collections.Generic;

namespace ForgottenVale
{
    enum GameState 
    {
        StartScreen,
        GameWorld,
        Battlemode, 
        End
    }

    enum Level
    {
        Clearing,
        Vale,
        EastRoad,
        WestRoad,
        OldForest,
        Cave,
        Workshop,
        RitualPeak,
        CPU,
        TreeHouse,
        Casino,
        Saloon,
        TownHall,
        FarmHouse,
        PondHouse,
        CaveBoss
    }

    struct Camera2d                                                                              // Camera SetUp
    {                                                                                            // 
        public Vector2 Position;                                                                 //
        public float Zoom;                                                                       //
                                                                                                 //
        public Matrix getCam()                                                                   //
        {                                                                                        //
            Matrix temp;                                                                         //
            temp = Matrix.CreateTranslation(new Vector3(Position.X, Position.Y, 0));             //
            temp *= Matrix.CreateScale(Zoom);                                                    //
            return temp;                                                                         //
        }                                                                                        //
    }

    struct Door
    {
        public Door(Vector2 newPlayerPos, int destValue, Vector2 doorLoc)
        {
            NewPlayerPos = newPlayerPos;
            DoorDestination = destValue;
            DoorLocation = doorLoc;
        }

        public Vector2 NewPlayerPos { get; }
        public int DoorDestination { get; }
        public Vector2 DoorLocation { get; }
    }

    public class Game1 : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;

        public static readonly Random RNG = new Random();               // Random Number Generator
        public static readonly int TILESIZE = 64;                       // Square Tilesize
        public static SpriteFont debugFont, uiFontOne, uiFontTwo;       // Fonts
        public static int environStage;                                 // The Current state of the world environment
        bool showNavGrid = false;                                       // Toggle Visibility of Nav Grid
        bool contentLoaded = true;                                      // Trigger to call load content method

        bool titleScrolled, gameInProgress;

        int A, B; // variable for tilemaps

        // start screen stuff
        bool titleDone = false, titleTrackTrigd;
        float titleCount = 2.5f, titleReps = 1, titleTrackCount;

        SoundEffect dialogBlip, uiMovCurs, chestOpenFX, dramaHorn, potionDrink, doorOpenFX, vendFX, encounterFX;
        Song battleTheme, eerieTheme, generalTheme, villageTheme;
        float genThemeVol = 0.25f;
        float villThemeVol = 0.4f;
        float battleThemeVol = 0.25f;
        float encBlipVol = 0.2f;

        const int SAFESTEPS = 20;

        // Persistent data
        Vector2 newPlayerPos;
        bool[] chestsOpen;          // track chest status between loads
        bool[] enemiesDefeated;     // track dead enemies between loads
        bool noBattles, firstHeroEnc, heroEncPrepped, canAdv, danceLearned, getRandLoot, deathReset, dstrydLandru;

        Vector2 deathResetPos;

        Camera2d camera;
        GamePadState padOneCurr, padOneOld;

        GameState currState;
        Level currLevel;
        
        Texture2D singlePix, cursorTex, battleUICover, blackScreenTex;
        Texture2D dialogUI, dialogUICover, portraits, pauseMenuTex, notificationBox;
        Texture2D iceCore, iceOut, earthCore, earthOut, flameCore, flameOut;
        Texture2D contTex, howToTex, newGameTex, quitTex, howToPageTex, startBack, rockTex, howToPageTwo;

        StaticGraphic currMap, currOverlay, currUnderlay, battleBack, battleUIBox, battleTop, BattleShield, strtScreen, blackScreen;
        Animated2D playerBattle;
        MotionGraphic ScrollTextEnd;
        PlayerClass theWanderer;
        PlayerInfo PInfo;
        BattleManager battMan;
        DialogManager diMan;
        TextManager tMan;
        PauseMenu pMenu;
        StartMenu sMenu;

        List<baseNPC> currNPCs; // stores the info for currently loaded NPC phisReps
        List<Animated2D> aniItemList; // list of animated objects eg. campfire, torch
        List<Animated2D> WaterTiles;
        List<ChestClass> ChestList;
        List<Door> DoorList;
        List<Notifications> NotificationsList;
        List<FadingGraphic> OpeningSlides;
        List<StaticGraphic> RocksList;

        // NAV GRID Layouts
        int[,] gaiaShrine;
        int[,] theClearing, theVale, eastRoad, westRoad, oldForest, theCaveMain, theCaveBoss, spellCaveInt;
        int[,] testInterior, saloonInt, treeHouseInt, TownhallInt, WorkshopInt, spaceCPU;
        TileClass[,] navGrid;


        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = false;

            _graphics.PreferredBackBufferWidth = 1920;       // set screen dimensions and set full screen
            _graphics.PreferredBackBufferHeight = 1080;      //
            _graphics.IsFullScreen = true;                   //
            _graphics.HardwareModeSwitch = false;            //
        }

        /////////////////////////////////////////////////////////////////////////////// ** MISC FUNCTIONS ** ////////////////////////////////////////////////////////////////////////////////////////////////////////////

        public void RollForLoot()
        {
            int roll = RNG.Next(1, 7);

            if (roll == 6)
            {
                NotificationsList.Add(new Notifications(notificationBox, 1));
                PInfo.HealthPotion++;
            }
            else if (roll == 5)
            {
                NotificationsList.Add(new Notifications(notificationBox, 2));
                PInfo.MagickPotion++;
            }
            else
            {
                NotificationsList.Add(new Notifications(notificationBox, 3));
                PInfo.Coins += roll * 5;
            }

            getRandLoot = false;
        }

        public void TriggerHeroDia()
        {
            if (firstHeroEnc)
            {
                currNPCs.ForEach(npc =>
                {
                    if (npc.CharName == "Legendary Hero") { npc.IsActive = false; }
                });
                firstHeroEnc = false;
            }
            else
            {
                PInfo.LegHeroRank++;
                
                if (PInfo.LegHeroRank == 5)
                {
                    PInfo.HeroGone = true;
                }
            }

            diMan = new DialogManager(PInfo, "Legendary Hero", portraits, -camera.Position, vendFX, potionDrink);
            diMan.IsIdle = false;
            theWanderer.TalkTo = "unknown";
        }

        public void useDoor(Vector2 newPos, int dest)
        {
            newPlayerPos = newPos;
            currLevel = (Level)dest;
            contentLoaded = false;
            doorOpenFX.Play(0.2f, 0, 0);
        }
        
        public void BuildNavGrid(int[,] layout)
        {
            currNPCs.RemoveAll(npc => npc.ID >= 0 && enemiesDefeated[npc.ID] == true);

            for (int i = 0; i < layout.GetLength(0); i++) // row id                                        // This loop populates the nav grid array with tile class objects whilst
            {                                                                                              // checking them against the map layout array to determain whether they 
                for (int j = 0; j < layout.GetLength(1); j++) // col id                                    // are walkable or not.
                {                                                                                          //
                    if (layout[i, j] == 0)                                                                 //
                    {                                                                                      //
                        navGrid[j, i] = new TileClass(new Vector2(j, i), false, singlePix);                //
                    }                                                                                      //
                    else                                                                                   //
                    {                                                                                      //
                        if (layout[i, j] == 1)                                                             //
                        {                                                                                  //
                            navGrid[j, i] = new TileClass(new Vector2(j, i), true, singlePix);             //
                        }                                                                                  //
                        if (layout[i, j] == 2)                                                             //
                        {                                                                                  //
                            navGrid[j, i] = new TileClass(new Vector2(j, i), true, singlePix, true);       //
                        }                                                                                  //
                                                                                                           //
                    }                                                                                      //
                                                                                                           //
                    // check if tile has NPC, if so update iswalkable                                      //
                    currNPCs.ForEach(npc =>                                                                //
                    {                                                                                      //
                        if (navGrid[j, i].TilePos == npc.Position)                                         //
                        {                                                                                  //
                            navGrid[j, i].IsWalkable = false;                                              //
                            navGrid[j, i].NPC_Name = npc.CharName;
                            navGrid[j, i].EnemyID = npc.ID;
                        }                                                                                  //
                    });                                                                                    //

                    // check if tile has Chest, if so update iswalkable                                    //
                    ChestList.ForEach(chest =>                                                             //
                    {                                                                                      //
                        if (navGrid[j, i].TilePos == chest.Position)                                       //
                        {                                                                                  //
                            navGrid[j, i].ChestID = chest.ID;                                              //
                            navGrid[j, i].IsWalkable = false;                                              //
                        }                                                                                  //
                        chest.IsOpen = chestsOpen[chest.ID];                                               // << this line reopens previously opened chests after reloading
                    });                                                                                    //
                                                                                                           //
                    // check if tile has Rock, if so update iswalkable                                     //
                    RocksList.ForEach(r =>                                                                 //
                    {                                                                                      // << if it has a rock, the player cant walk
                        if (navGrid[j, i].TilePos == r.Position)                                           //
                        {                                                                                  //
                            navGrid[j, i].IsWalkable = false;                                              //
                        }                                                                                  //
                    });                                                                                    //
                }                                                                                          // 
            }                                                                                               
        }

        public void AdvanceEnvironStage()
        {
            if (environStage == 1)
            {
                environStage = 2;
                A = 1; B = 1;
                PInfo.CurrObjective = "Return To Elder Kevin " +
                                      "\n'I should return to The Elder. I found the" +
                                      "\nShard in the forest but bringing them " +
                                      "\ntogether caused an ecological disturbance. '";
                PInfo.KevinRank = 2;
            }
            else if (environStage == 2)
            {
                environStage = 3;
                A = 1; B = 0;
                PInfo.CurrObjective = "Return To Elder Kevin " +
                                      "\n'I found the Shard in the Cave but now" +
                                      "\nthe environmental crisis seems to have" +
                                      "\nescalated. I should return to Kevin.'";
                PInfo.KevinRank = 4;
            }

            //change navGrid
            //switch (environStage)
            //{
            //    case 1:
            //        A = 0; B = 0;
            //        break;
            //    case 2:
            //        A = 1; B = 1;
            //        break;
            //    case 3:
            //        A = 1; B = 0;
            //        break;
            //}

            initLevelLayouts();

            PInfo.HeroEncTrig = true;
            heroEncPrepped = true;
        }

        /////////////////////////////////////////////////////////////////////////////// ** INITIALIZE & INIT LEVEL LAYOUTS ** /////////////////////////////////////////////////////////////////////////////////////////////

        protected override void Initialize()
        {
            currState = GameState.StartScreen;   // For Real Playthrough
            currLevel = Level.Clearing;          //

            camera.Position = Vector2.Zero;
            camera.Zoom = 1;

            currNPCs = new List<baseNPC>();
            aniItemList = new List<Animated2D>();
            WaterTiles = new List<Animated2D>();
            ChestList = new List<ChestClass>();
            DoorList = new List<Door>();
            NotificationsList = new List<Notifications>();
            OpeningSlides = new List<FadingGraphic>();
            RocksList = new List<StaticGraphic>();

            // set player start
            newPlayerPos = new Vector2(21,21);     //(21, 21)
            chestsOpen = new bool[60];              // UPDATE if more than 50 chests
            enemiesDefeated = new bool[60];         // UPDATE if more than 60 phys enemies
            environStage = 1;

            firstHeroEnc = true;
            heroEncPrepped = false;
            canAdv = true;
            titleScrolled = false;
            gameInProgress = false;
            danceLearned = false;
            getRandLoot = false;
            deathReset = true;

            titleTrackCount = 3;
            titleTrackTrigd = false;

            deathResetPos = new Vector2(5,7);

            noBattles = false;

            A = 0; // marks the position of underwater stairs (accessable during dry season && ice season)
            B = 0; // marks the position of waters edge (accessable during ice season)


            //currState = GameState.GameWorld;         // For TESTING
            //currLevel = Level.CPU;                  //
            //titleScrolled = true;                    //
            //newPlayerPos = new Vector2(7,9);        //

            #region Perminent Level Layouts

            theClearing = new int[30, 30] // this is the grid layout for The Clearing
            {
            //    0  1  2  3  4  5  6  7  8  9  10 11 12 13 14 15 16 17 18 19 20 21 22 23 24 25 26 27 28 29 
                { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0}, // 0
                { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0}, // 1
                { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0}, // 2
                { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0}, // 3
                { 0, 0, 0, 0, 0, 0, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 0, 0, 0, 0, 0, 0}, // 4
                { 0, 0, 0, 0, 0, 0, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 0, 0, 0, 0, 0, 0}, // 5
                { 0, 0, 0, 0, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 0, 0, 0, 0}, // 6
                { 0, 0, 0, 0, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 0, 0, 0, 0}, // 7
                { 0, 0, 0, 0, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 0, 0, 0, 0}, // 8
                { 0, 0, 0, 0, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 0, 0, 0, 0}, // 9
                { 0, 0, 0, 0, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 0, 0, 0, 0}, // 10
                { 0, 0, 0, 0, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 0, 0, 0, 0}, // 11
                { 0, 0, 0, 0, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 0, 0, 0, 0}, // 12
                { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0}, // 13
                { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0}, // 14
                { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0}, // 15
                { 0, 0, 0, 0, 0, 1, 1, 1, 1, 1, 0, 0, 0, 0, 1, 1, 0, 0, 0, 0, 1, 1, 1, 1, 1, 0, 0, 0, 0, 0}, // 16
                { 0, 0, 0, 0, 1, 1, 0, 1, 0, 1, 1, 0, 0, 1, 1, 1, 0, 0, 0, 1, 1, 1, 1, 1, 1, 1, 0, 0, 0, 0}, // 17
                { 0, 0, 0, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 0, 0, 0, 0, 1, 1, 1, 1, 1, 1, 1, 1, 0, 0, 0}, // 18
                { 0, 0, 0, 1, 0, 1, 1, 1, 1, 1, 0, 1, 1, 1, 1, 0, 0, 0, 0, 1, 1, 1, 1, 1, 1, 1, 1, 0, 0, 0}, // 19
                { 0, 0, 0, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 0, 0, 1, 1, 1, 1, 0, 0, 1, 1, 1, 0, 0, 0}, // 20
                { 0, 0, 0, 1, 1, 0, 1, 1, 1, 0, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 0, 0, 0}, // 21
                { 0, 0, 0, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 0, 0, 0}, // 22
                { 0, 0, 0, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 0, 0, 1, 1, 1, 0, 0, 0}, // 23
                { 0, 0, 0, 1, 1, 1, 1, 2, 2, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 0, 0, 0}, // 24
                { 0, 0, 0, 1, 1, 1, 1, 2, 2, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 0, 0, 0}, // 25
                { 0, 0, 0, 0, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 0, 0, 0, 0}, // 26
                { 0, 0, 0, 0, 0, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 0, 0, 0, 0, 0}, // 27
                { 0, 0, 0, 0, 0, 0, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 0, 0, 0, 0, 0, 0}, // 28
                { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0}, // 29
            };

            eastRoad = new int[30, 30] // this is the grid layout for the East Road
            {
            //    0  1  2  3  4  5  6  7  8  9  10 11 12 13 14 15 16 17 18 19 20 21 22 23 24 25 26 27 28 29 
                { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0}, // 0
                { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0}, // 1
                { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 2, 2, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0}, // 2
                { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 2, 2, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0}, // 3
                { 0, 0, 0, 0, 0, 2, 2, 2, 2, 2, 2, 2, 0, 2, 2, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0}, // 4
                { 0, 0, 0, 0, 2, 2, 2, 2, 2, 2, 2, 2, 0, 2, 2, 0, 0, 0, 0, 0, 0, 0, 2, 0, 0, 0, 0, 0, 0, 0}, // 5
                { 0, 0, 0, 2, 2, 2, 2, 2, 2, 2, 2, 2, 0, 2, 2, 2, 0, 0, 2, 0, 0, 2, 2, 0, 0, 0, 0, 0, 2, 0}, // 6
                { 0, 0, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 0, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 0, 0, 0, 0, 0, 2, 0}, // 7
                { 0, 1, 2, 2, 2, 2, 0, 0, 2, 2, 2, 2, 0, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 0, 0, 1, 0, 0, 2, 0}, // 8
                { 1, 1, 2, 2, 2, 0, 0, 0, 0, 2, 2, 2, 0, 2, 2, 2, 0, 0, 2, 0, 0, 2, 2, 2, 2, 2, 2, 2, 2, 0}, // 9
                { 0, 1, 2, 2, 2, 0, 0, 0, 0, 0, 2, 2, 0, 0, 0, 2, 0, 0, 2, 0, 0, 2, 2, 2, 2, 2, 2, 2, 0, 0}, // 10
                { 0, 0, 2, 2, 2, 0, 0, 0, 0, 0, 2, 2, 0, 2, 2, 2, 0, 0, 2, 0, 0, 2, 2, 2, 2, 2, 2, 0, 0, 0}, // 11
                { 0, 0, 2, 2, 2, 2, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 2, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0}, // 12
                { 0, 0, 0, 2, 2, 2, 2, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 2, 2, 2, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0}, // 13
                { 0, 0, 0, 0, 2, 2, 2, 2, 0, 0, 0, 0, 0, 0, 0, 0, 0, 2, 2, 2, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0}, // 14
                { 0, 0, 0, 2, 2, 2, 2, 2, 2, 0, 0, 0, 0, 0, 0, 0, 0, 2, 2, 2, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0}, // 15
                { 0, 0, 2, 2, 2, 2, 2, 2, 2, 0, 0, 0, 0, 2, 2, 2, 2, 2, 2, 2, 2, 2, 0, 0, 0, 0, 0, 0, 0, 0}, // 16
                { 0, 0, 2, 2, 2, 0, 2, 2, 2, 0, 0, 0, 0, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 0, 0, 0, 0, 0}, // 17
                { 0, 0, 2, 2, 2, 2, 2, 2, 2, 0, 0, 0, 0, 0, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 0, 0, 0, 0, 0}, // 18
                { 0, 0, 2, 2, 2, 2, 2, 2, 2, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 2, 2, 2, 2, 2, 2, 0, 0, 0, 0, 0}, // 19
                { 0, 0, 2, 2, 2, 2, 2, 2, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 2, 2, 2, 2, 0, 0, 0, 0}, // 20
                { 0, 0, 0, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 0, 0, 0, 0, 0, 0, 0, 0, 0, 2, 2, 2, 2, 2, 0, 0, 0}, // 21
                { 0, 0, 0, 0, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 0, 0, 0, 0, 0, 0, 0, 0, 2, 2, 2, 2, 2, 2, 0, 0}, // 22
                { 0, 0, 0, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 0, 0, 0, 0, 2, 2, 2, 2, 2, 2, 2, 0, 0}, // 23
                { 0, 0, 2, 2, 2, 2, 0, 0, 0, 0, 0, 0, 2, 2, 2, 2, 2, 2, 0, 0, 2, 2, 2, 2, 0, 2, 2, 2, 0, 0}, // 24
                { 0, 0, 2, 2, 2, 0, 0, 0, 0, 0, 0, 0, 0, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 0, 0}, // 25
                { 0, 0, 2, 2, 2, 2, 0, 0, 0, 0, 2, 2, 0, 0, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 0, 0}, // 26
                { 0, 0, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 0, 0, 0, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 0, 0}, // 27
                { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0}, // 28
                { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0}, // 29
            };

            oldForest = new int[50, 50] // this is the grid layout for the Old Forest
            {
            //    0  1  2  3  4  5  6  7  8  9  10 11 12 13 14 15 16 17 18 19 20 21 22 23 24 25 26 27 28 29 30 31 32 33 34 35 36 37 38 39 40 41 42 43 44 45 46 47 48 49
                { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0}, // 0
                { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 2, 2, 0, 0, 0, 0, 0, 0, 0, 0}, // 1
                { 0, 0, 0, 2, 2, 0, 0, 2, 2, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 2, 2, 0, 0, 0, 0, 0, 0, 0, 0, 2, 2, 0, 2, 2, 0, 2, 2, 0, 0, 0, 0, 0}, // 2
                { 0, 0, 2, 2, 2, 2, 2, 2, 2, 2, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 2, 2, 2, 2, 2, 2, 2, 0, 0, 0, 0, 0, 0, 0, 2, 2, 0, 0, 0, 0, 0, 0, 2, 2, 0, 0, 0, 0}, // 3
                { 0, 0, 2, 2, 2, 2, 2, 2, 2, 2, 2, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 2, 2, 2, 2, 2, 2, 2, 2, 0, 0, 0, 0, 0, 0, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 0, 0, 0}, // 4
                { 0, 0, 2, 2, 2, 2, 0, 2, 2, 2, 2, 2, 2, 2, 0, 0, 0, 0, 0, 0, 2, 2, 2, 2, 2, 2, 2, 2, 0, 0, 0, 0, 0, 0, 2, 2, 0, 2, 2, 2, 2, 2, 2, 2, 2, 0, 2, 0, 0, 0}, // 5
                { 0, 0, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 0, 0, 0, 0, 2, 2, 2, 2, 2, 2, 2, 2, 0, 0, 0, 0, 0, 0, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 0, 0, 0}, // 6
                { 0, 0, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 0, 0, 0, 0, 2, 2, 2, 2, 2, 0, 2, 2, 2, 2, 2, 2, 0, 2, 2, 0, 0, 0}, // 7
                { 0, 0, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 0, 0, 0}, // 8
                { 0, 0, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 0, 0, 0}, // 9
                { 0, 0, 2, 2, 2, 2, 2, 2, 2, 2, 0, 0, 0, 0, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 0, 0}, // 10
                { 0, 0, 2, 2, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 2, 2, 2, 2, 2, 2, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0}, // 11
                { 0, 0, 2, 2, 0, 2, 2, 2, 2, 2, 0, 0, 0, 0, 0, 0, 2, 2, 2, 2, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 2, 2, 2, 2, 2, 0, 0, 0, 0, 0, 0, 0, 0}, // 12
                { 0, 0, 2, 2, 0, 2, 2, 2, 2, 2, 2, 0, 0, 2, 0, 0, 0, 2, 2, 0, 0, 0, 2, 0, 0, 0, 0, 0, 0, 2, 0, 0, 0, 0, 0, 0, 0, 2, 2, 2, 2, 2, 0, 0, 0, 0, 0, 0, 0, 0}, // 13
                { 0, 0, 2, 2, 0, 2, 0, 0, 2, 2, 2, 2, 2, 2, 0, 0, 0, 0, 0, 0, 0, 0, 2, 2, 0, 0, 0, 0, 2, 2, 0, 2, 0, 0, 0, 0, 2, 2, 2, 2, 2, 2, 2, 0, 0, 0, 0, 2, 0, 0}, // 14
                { 0, 0, 2, 2, 0, 2, 0, 0, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 0, 0, 0, 2, 2, 2, 2, 2, 2, 2, 2, 0, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 0, 0}, // 15
                { 0, 0, 2, 2, 0, 2, 0, 0, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 0, 0, 0, 0, 2, 2, 2, 2, 2, 2, 0, 2, 0, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 0, 0}, // 16
                { 0, 0, 2, 2, 2, 2, 2, 0, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 0, 0, 0, 0, 2, 2, 2, 2, 2, 2, 2, 2, 0, 2, 0, 0, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 0, 0, 2, 0, 0}, // 17
                { 0, 0, 2, 2, 2, 2, 2, 0, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 0, 0, 0, 2, 2, 2, 2, 2, 2, 2, 2, 0, 2, 2, 2, 2, 0, 0, 0, 0, 2, 0, 0, 0, 0, 2, 2, 2, 2, 0, 0}, // 18
                { 0, 0, 2, 2, 2, 2, 2, 0, 2, 2, 2, 2, 2, 2, 0, 0, 0, 0, 0, 0, 0, 0, 2, 2, 2, 2, 2, 2, 2, 2, 0, 0, 0, 0, 0, 0, 0, 0, 0, 2, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0}, // 19
                { 0, 0, 2, 2, 2, 2, 2, 0, 2, 2, 2, 2, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 2, 0, 0, 2, 2, 2, 2, 0, 0, 0, 0, 0, 0, 2, 2, 2, 2, 2, 2, 2, 0, 0, 0, 0, 0, 0, 0}, // 20
                { 0, 0, 2, 2, 2, 2, 2, 0, 2, 2, 2, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 2, 0, 0, 2, 2, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 2, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0}, // 21
                { 0, 0, 2, 2, 2, 2, 2, 0, 2, 2, 2, 2, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 2, 0, 0, 2, 2, 2, 2, 0, 0, 0, 0, 0, 0, 0, 0, 0, 2, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0}, // 22
                { 0, 0, 2, 2, 2, 2, 2, 0, 2, 2, 2, 2, 0, 0, 0, 2, 0, 0, 0, 0, 2, 0, 0, 2, 0, 0, 2, 2, 2, 2, 2, 2, 2, 2, 2, 0, 0, 0, 0, 2, 0, 0, 0, 0, 0, 0, 2, 2, 2, 0}, // 23
                { 0, 0, 2, 2, 2, 2, 2, 0, 2, 2, 2, 2, 0, 0, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 0, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 0, 0, 2, 2, 2, 2, 0}, // 24
                { 0, 0, 2, 2, 0, 2, 2, 0, 2, 2, 2, 2, 0, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 0, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 0, 0}, // 25
                { 0, 0, 2, 2, 2, 2, 2, 0, 2, 2, 2, 2, 0, 2, 2, 2, 2, 0, 0, 2, 2, 2, 2, 2, 2, 0, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 0, 0}, // 26
                { 0, 0, 2, 2, 2, 2, 2, 0, 2, 2, 2, 2, 0, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 0, 2, 0, 0, 0, 0, 0, 0, 0, 0, 0, 2, 0, 0, 0, 0, 2, 2, 2, 2, 2, 2, 2, 2, 2, 0, 0}, // 27
                { 0, 0, 0, 0, 0, 0, 0, 0, 2, 2, 2, 2, 0, 2, 0, 0, 2, 2, 2, 2, 2, 2, 2, 2, 2, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 2, 2, 2, 2, 2, 2, 2, 2, 0, 0}, // 28
                { 0, 0, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 0, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 2, 2, 2, 2, 2, 2, 2, 2, 0, 0}, // 29
                { 0, 0, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 0, 2, 2, 2, 2, 2, 2, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 2, 2, 2, 2, 2, 2, 2, 2, 0, 0}, // 30
                { 0, 0, 2, 2, 2, 2, 2, 2, 2, 0, 0, 2, 0, 2, 2, 2, 2, 2, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 2, 2, 2, 0, 2, 0, 0, 0, 0, 2, 2, 0, 0, 2, 0, 0, 2, 2, 0, 0}, // 31
                { 0, 0, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 0, 2, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 2, 2, 2, 2, 0, 2, 2, 2, 2, 2, 2, 2, 0, 0, 2, 0, 0, 2, 2, 0, 0}, // 32
                { 0, 0, 0, 0, 0, 2, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 2, 2, 2, 2, 2, 0, 0, 0, 2, 2, 2, 2, 2, 2, 2, 0, 0, 2, 0, 0, 2, 2, 2, 0}, // 33
                { 0, 0, 0, 0, 0, 2, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 2, 0, 0, 0, 0, 0, 0}, // 34
                { 0, 0, 0, 0, 0, 2, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 2, 2, 2, 0, 0, 0, 0, 0}, // 35
                { 0, 0, 2, 2, 2, 2, 2, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 2, 2, 2, 0, 0, 0, 0, 0}, // 36
                { 0, 0, 2, 2, 2, 2, 2, 0, 0, 0, 0, 0, 0, 0, 0, 0, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 2, 2, 2, 0, 0, 0, 0, 0}, // 37
                { 0, 0, 2, 2, 2, 2, 2, 0, 0, 0, 0, 0, 0, 0, 0, 2, 2, 2, 2, 2, 0, 0, 0, 0, 2, 2, 2, 2, 2, 2, 0, 0, 0, 0, 0, 0, 0, 0, 0, 2, 2, 2, 2, 2, 2, 2, 2, 2, 0, 0}, // 38
                { 0, 0, 2, 2, 2, 2, 2, 2, 0, 0, 0, 0, 0, 0, 0, 2, 2, 2, 2, 0, 0, 0, 0, 0, 0, 2, 2, 2, 2, 2, 2, 0, 0, 0, 0, 0, 0, 0, 0, 2, 2, 2, 2, 2, 2, 2, 2, 2, 0, 0}, // 39
                { 0, 0, 2, 2, 2, 2, 2, 2, 2, 0, 0, 0, 0, 0, 0, 2, 2, 2, 0, 0, 0, 0, 0, 0, 0, 0, 2, 2, 2, 2, 2, 2, 0, 0, 0, 0, 0, 0, 0, 2, 2, 2, 2, 2, 2, 2, 2, 2, 0, 0}, // 40
                { 0, 0, 2, 2, 2, 2, 2, 2, 2, 2, 0, 0, 0, 0, 2, 2, 2, 2, 2, 0, 0, 0, 0, 0, 0, 0, 2, 2, 2, 2, 2, 0, 0, 0, 0, 0, 0, 0, 0, 2, 2, 2, 2, 2, 2, 2, 2, 2, 0, 0}, // 41
                { 0, 0, 2, 2, 2, 2, 2, 2, 2, 2, 2, 0, 0, 2, 2, 2, 2, 2, 2, 2, 2, 2, 0, 0, 0, 0, 2, 2, 2, 2, 0, 0, 0, 0, 0, 0, 0, 0, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 0, 0}, // 42
                { 0, 0, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 0, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 0, 0, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 0, 0}, // 43
                { 0, 0, 2, 2, 0, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 0, 0}, // 44
                { 0, 0, 2, 2, 2, 2, 2, 2, 2, 2, 2, 0, 0, 2, 2, 2, 2, 2, 2, 2, 2, 2, 0, 0, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 0, 0, 2, 2, 2, 2, 2, 2, 2, 0, 2, 2, 0, 0}, // 45
                { 0, 0, 2, 2, 2, 2, 2, 2, 2, 2, 0, 0, 0, 0, 2, 2, 2, 2, 2, 2, 2, 0, 0, 0, 0, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 0, 0, 0, 0, 2, 2, 2, 2, 2, 2, 2, 2, 2, 0, 0}, // 46
                { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 1, 1, 1, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0}, // 47
                { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 1, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0}, // 48
                { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0}, // 49
            };

            theCaveMain = new int[50, 50] // this is the grid layout for the Cave
            {
            //    0  1  2  3  4  5  6  7  8  9  10 11 12 13 14 15 16 17 18 19 20 21 22 23 24 25 26 27 28 29 30 31 32 33 34 35 36 37 38 39 40 41 42 43 44 45 46 47 48 49
                { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0}, // 0
                { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0}, // 1
                { 0, 2, 2, 0, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 0, 2, 2, 2, 2, 0, 2, 2, 2, 2, 2, 2, 2, 0, 0, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 0}, // 2
                { 0, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 0, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 0, 2, 2, 0, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 0}, // 3
                { 0, 2, 2, 2, 2, 0, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 0, 2, 2, 2, 2, 2, 2, 2, 2, 2, 0, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 0}, // 4
                { 0, 0, 0, 0, 0, 0, 0, 2, 2, 2, 2, 0, 0, 2, 0, 0, 2, 2, 2, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 2, 0, 2, 2, 2, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0}, // 5
                { 0, 0, 0, 0, 0, 0, 0, 2, 2, 2, 2, 0, 0, 2, 0, 0, 2, 2, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 2, 2, 2, 2, 2, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0}, // 6
                { 0, 2, 2, 2, 2, 2, 0, 2, 2, 2, 2, 0, 2, 2, 2, 0, 2, 2, 2, 0, 2, 2, 2, 2, 2, 2, 2, 2, 0, 2, 2, 2, 2, 2, 0, 2, 2, 2, 2, 2, 2, 0, 2, 2, 2, 2, 2, 2, 2, 0}, // 7
                { 0, 2, 0, 2, 0, 2, 0, 2, 2, 2, 2, 0, 0, 2, 0, 0, 2, 2, 2, 0, 2, 0, 2, 2, 2, 2, 0, 2, 0, 2, 2, 2, 0, 2, 0, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 0}, // 8
                { 0, 2, 2, 2, 2, 2, 0, 2, 0, 2, 2, 0, 2, 2, 2, 0, 2, 2, 2, 0, 2, 2, 2, 2, 2, 2, 2, 2, 0, 2, 2, 2, 2, 2, 0, 2, 2, 0, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 0}, // 9
                { 0, 2, 2, 2, 2, 2, 0, 2, 2, 2, 2, 0, 2, 2, 2, 0, 0, 2, 2, 0, 2, 2, 2, 2, 2, 2, 2, 2, 0, 2, 2, 2, 2, 2, 0, 2, 2, 2, 2, 2, 2, 0, 0, 0, 0, 0, 0, 0, 0, 0}, // 10
                { 0, 2, 2, 2, 2, 2, 0, 2, 2, 2, 0, 0, 0, 2, 0, 0, 2, 2, 2, 0, 2, 0, 2, 2, 2, 2, 0, 2, 0, 0, 0, 0, 0, 0, 0, 2, 2, 2, 2, 2, 2, 0, 2, 2, 2, 0, 2, 2, 2, 0}, // 11
                { 0, 2, 2, 2, 2, 2, 0, 2, 2, 2, 2, 0, 2, 2, 2, 0, 2, 2, 2, 0, 2, 2, 2, 2, 2, 2, 2, 2, 0, 0, 0, 2, 0, 0, 0, 2, 2, 2, 2, 2, 2, 0, 2, 2, 2, 2, 2, 2, 2, 0}, // 12
                { 0, 2, 2, 2, 2, 0, 0, 2, 2, 2, 2, 0, 2, 2, 2, 0, 2, 2, 2, 0, 2, 2, 2, 2, 2, 2, 2, 2, 2, 0, 0, 2, 0, 0, 2, 2, 2, 2, 2, 2, 2, 0, 2, 2, 2, 2, 2, 2, 2, 0}, // 13
                { 0, 2, 2, 2, 2, 2, 0, 2, 2, 2, 2, 0, 0, 2, 0, 0, 2, 2, 2, 0, 2, 2, 2, 2, 2, 2, 2, 2, 2, 0, 2, 2, 2, 0, 2, 2, 2, 2, 2, 2, 2, 0, 0, 2, 0, 0, 0, 2, 2, 0}, // 14
                { 0, 2, 2, 2, 2, 2, 0, 0, 0, 0, 0, 0, 2, 2, 2, 0, 2, 2, 0, 0, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 0, 0, 2, 0, 0, 0, 2, 2, 0}, // 15
                { 0, 0, 2, 2, 2, 2, 0, 0, 0, 0, 0, 0, 2, 2, 2, 0, 2, 2, 2, 0, 2, 2, 2, 2, 0, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 0, 2, 2, 0}, // 16
                { 0, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 0, 2, 2, 2, 0, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 0, 2, 2, 0}, // 17
                { 0, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 0, 2, 0, 2, 0, 2, 0, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 0, 2, 2, 2, 2, 2, 2, 2, 2, 0, 2, 2, 0}, // 18
                { 0, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 0, 2, 2, 2, 0, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 0, 2, 0, 0}, // 19
                { 0, 2, 2, 2, 2, 2, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 2, 2, 2, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 2, 2, 2, 0, 2, 2, 0}, // 20
                { 0, 2, 2, 2, 2, 2, 0, 2, 2, 0, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 0, 2, 2, 2, 2, 2, 2, 2, 0, 2, 2, 2, 2, 2, 2, 2, 2, 0, 2, 2, 2, 0, 2, 2, 0}, // 21
                { 0, 2, 2, 2, 2, 2, 0, 2, 2, 2, 2, 2, 2, 0, 2, 2, 2, 2, 2, 2, 2, 0, 2, 2, 2, 2, 2, 2, 2, 0, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 0, 2, 2, 2, 0, 2, 2, 0}, // 22
                { 0, 2, 2, 2, 2, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 2, 2, 2, 2, 2, 2, 0, 0, 0, 0, 0, 2, 2, 0}, // 23
                { 0, 2, 2, 2, 2, 2, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 2, 2, 2, 2, 2, 2, 2, 2, 2, 0, 2, 2, 2, 0}, // 24
                { 0, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 0, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 0, 2, 2, 2, 2, 2, 2, 0, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 0}, // 25
                { 0, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 0, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 0, 2, 2, 2, 0, 2, 2, 2, 2, 2, 2, 2, 2, 2, 0}, // 26
                { 0, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 0, 2, 2, 2, 2, 2, 2, 2, 0, 2, 2, 2, 2, 2, 0}, // 27
                { 0, 2, 0, 2, 2, 2, 2, 2, 2, 2, 2, 2, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 2, 2, 2, 0, 0, 0, 0, 0, 0, 0, 2, 2, 2, 0, 2, 2, 2, 2, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0}, // 28
                { 0, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 2, 2, 0, 2, 2, 2, 2, 2, 0, 2, 2, 2, 0, 2, 2, 2, 2, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0}, // 29
                { 0, 0, 0, 0, 0, 0, 0, 0, 0, 2, 2, 2, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 2, 2, 2, 0, 2, 0, 2, 0, 2, 0, 2, 2, 0, 0, 2, 2, 2, 2, 0, 2, 2, 2, 2, 2, 2, 2, 2, 0}, // 30
                { 0, 2, 2, 2, 2, 2, 2, 2, 0, 0, 2, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 2, 2, 2, 0, 2, 2, 2, 2, 2, 0, 2, 2, 2, 0, 2, 2, 2, 2, 0, 2, 0, 2, 2, 2, 2, 0, 2, 0}, // 31
                { 0, 2, 2, 2, 2, 2, 0, 2, 0, 2, 2, 2, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 2, 2, 0, 0, 0, 0, 2, 0, 0, 0, 2, 2, 2, 0, 2, 2, 2, 2, 0, 2, 2, 2, 2, 2, 2, 2, 2, 0}, // 32
                { 0, 2, 2, 2, 2, 2, 2, 2, 0, 2, 2, 2, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 2, 2, 2, 0, 0, 0, 2, 0, 0, 0, 2, 2, 2, 0, 2, 0, 2, 2, 0, 2, 2, 2, 2, 2, 2, 2, 2, 0}, // 33
                { 0, 2, 2, 2, 2, 2, 2, 2, 0, 2, 2, 2, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 0, 2, 2, 2, 2, 0, 2, 0, 2, 2, 2, 2, 0, 2, 0}, // 34
                { 0, 0, 2, 0, 0, 0, 0, 0, 0, 2, 2, 2, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 2, 2, 2, 2, 2, 2, 2, 2, 0, 2, 2, 2, 2, 0, 2, 2, 2, 2, 0, 2, 2, 2, 2, 2, 2, 2, 2, 0}, // 35
                { 0, 0, 2, 0, 0, 0, 0, 0, 0, 0, 2, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 0, 2, 2, 2, 2, 0, 2, 2, 2, 2, 2, 2, 2, 2, 0}, // 36
                { 0, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 2, 2, 2, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 2, 2, 2, 2, 0, 2, 2, 2, 2, 2, 2, 2, 2, 0}, // 37
                { 0, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 2, 2, 2, 0, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 0, 2, 2, 2, 2, 2, 2, 2, 2, 0}, // 38
                { 0, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 2, 2, 2, 0, 2, 2, 2, 2, 2, 2, 0, 2, 2, 2, 2, 2, 2, 2, 0, 0, 0, 2, 2, 2, 0, 0, 0, 0}, // 39
                { 0, 2, 2, 2, 0, 0, 0, 0, 0, 2, 2, 2, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 2, 2, 0, 2, 0, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 0, 2, 2, 2, 0, 2, 2, 2, 0, 2, 2, 0}, // 40
                { 0, 0, 2, 2, 0, 0, 0, 0, 0, 2, 2, 2, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 2, 2, 2, 0, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 0, 2, 2, 2, 0, 2, 2, 0}, // 41
                { 0, 2, 2, 2, 0, 0, 0, 0, 0, 2, 2, 2, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 2, 2, 2, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 2, 2, 2, 0, 0, 0, 0}, // 42
                { 0, 2, 2, 2, 0, 0, 0, 0, 0, 2, 2, 2, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 2, 2, 2, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 2, 2, 2, 0, 0, 0, 0}, // 43
                { 0, 2, 2, 0, 0, 0, 0, 0, 0, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 0, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 0, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 0}, // 44
                { 0, 2, 2, 2, 0, 0, 0, 0, 0, 2, 2, 2, 2, 0, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 0, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 0, 2, 2, 0}, // 45
                { 0, 2, 2, 2, 0, 0, 0, 0, 0, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 0}, // 46
                { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 1, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0}, // 47
                { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 1, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0}, // 48
                { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0}, // 49
            };

            gaiaShrine = new int[30, 30] // this is the grid layout for the test level
            {
            //    0  1  2  3  4  5  6  7  8  9  10 11 12 13 14 15 16 17 18 19 20 21 22 23 24 25 26 27 28 29 
                { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0}, // 0
                { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0}, // 1
                { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0}, // 2
                { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0}, // 3
                { 0, 0, 0, 0, 0, 0, 1, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0}, // 4
                { 0, 0, 0, 0, 0, 0, 1, 1, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0}, // 5
                { 0, 0, 0, 0, 1, 1, 1, 1, 0, 1, 0, 0, 0, 1, 1, 1, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0}, // 6
                { 0, 0, 0, 0, 1, 1, 1, 1, 0, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 0, 1, 1, 1, 1, 1, 0, 0, 0, 0}, // 7
                { 0, 0, 0, 0, 1, 1, 1, 1, 0, 1, 1, 1, 1, 1, 1, 1, 1, 1, 0, 1, 0, 1, 1, 1, 1, 1, 0, 0, 0, 0}, // 8
                { 0, 0, 0, 0, 1, 1, 1, 1, 0, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 0, 1, 1, 1, 1, 1, 0, 0, 0, 0}, // 9
                { 0, 0, 0, 0, 1, 1, 1, 1, 0, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 0, 1, 1, 1, 1, 1, 0, 0, 0, 0}, // 10
                { 0, 0, 0, 0, 1, 1, 1, 1, 0, 1, 0, 1, 1, 1, 1, 1, 1, 1, 1, 1, 0, 1, 1, 1, 1, 1, 0, 0, 0, 0}, // 11
                { 0, 0, 0, 0, 1, 1, 1, 1, 0, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 0, 1, 1, 1, 1, 1, 0, 0, 0, 0}, // 12
                { 0, 0, 0, 0, 1, 1, 1, 1, 0, 0, 0, 0, 0, 1, 1, 1, 0, 0, 0, 0, 0, 1, 1, 1, 1, 1, 0, 0, 0, 0}, // 13
                { 0, 0, 0, 0, 1, 1, 1, 1, 0, 1, 1, 1, 0, 1, 1, 1, 0, 1, 1, 1, 0, 1, 1, 1, 1, 1, 0, 0, 0, 0}, // 14
                { 0, 0, 0, 0, 1, 1, 1, 1, 0, 1, 1, 1, 0, 1, 1, 1, 0, 1, 1, 1, 0, 1, 1, 1, 1, 1, 0, 0, 0, 0}, // 15
                { 0, 0, 0, 0, 1, 1, 1, 1, 0, 1, 1, 1, 0, 0, 1, 0, 0, 1, 1, 1, 0, 1, 1, 1, 1, 1, 0, 0, 0, 0}, // 16
                { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0}, // 17
                { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0}, // 18
                { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0}, // 19
                { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0}, // 20
                { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0}, // 21
                { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0}, // 22
                { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0}, // 23
                { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0}, // 24
                { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0}, // 25
                { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0}, // 26
                { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0}, // 27
                { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0}, // 28
                { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0}, // 29
            };

            theCaveBoss = new int[11, 11]
            {
            //    0  1  2  3  4  5  6  7  8  9  10
                { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0}, // 0
                { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0}, // 1
                { 0, 1, 1, 0, 0, 0, 0, 0, 1, 1, 0}, // 2
                { 0, 1, 1, 0, 1, 1, 1, 0, 1, 1, 0}, // 3
                { 0, 1, 1, 1, 1, 1, 1, 1, 1, 1, 0}, // 4
                { 0, 1, 1, 1, 1, 1, 1, 1, 1, 1, 0}, // 5
                { 0, 1, 1, 1, 1, 1, 1, 1, 1, 1, 0}, // 6
                { 0, 1, 0, 1, 1, 1, 1, 1, 0, 1, 0}, // 7
                { 0, 1, 1, 1, 1, 1, 1, 1, 1, 1, 0}, // 8
                { 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0}, // 9
                { 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0}, // 10
            };

            testInterior = new int[9, 9]
            {
            //    0  1  2  3  4  5  6  7  8
                { 0, 0, 0, 0, 0, 0, 0, 0, 0}, // 0
                { 0, 1, 1, 1, 1, 1, 1, 1, 0}, // 1
                { 0, 1, 1, 1, 1, 1, 1, 1, 0}, // 2
                { 0, 1, 1, 1, 1, 1, 1, 1, 0}, // 3
                { 0, 1, 1, 1, 1, 1, 1, 1, 0}, // 4
                { 0, 1, 1, 1, 1, 1, 1, 1, 0}, // 5
                { 0, 1, 1, 1, 1, 1, 1, 1, 0}, // 6
                { 0, 0, 0, 0, 1, 0, 0, 0, 0}, // 7
                { 0, 0, 0, 0, 0, 0, 0, 0, 0}, // 8
            };

            spellCaveInt = new int[9, 9]
            {
            //    0  1  2  3  4  5  6  7  8
                { 0, 0, 0, 0, 0, 0, 0, 0, 0}, // 0
                { 0, 1, 1, 1, 1, 1, 1, 1, 0}, // 1
                { 0, 1, 1, 1, 1, 1, 0, 1, 0}, // 2
                { 0, 1, 1, 1, 1, 1, 1, 1, 0}, // 3
                { 0, 1, 1, 1, 1, 1, 1, 1, 0}, // 4
                { 0, 1, 1, 1, 1, 1, 1, 1, 0}, // 5
                { 0, 0, 1, 1, 1, 1, 1, 0, 0}, // 6
                { 0, 0, 0, 0, 1, 0, 0, 0, 0}, // 7
                { 0, 0, 0, 0, 0, 0, 0, 0, 0}, // 8
            };

            treeHouseInt = new int[11, 11]
            {
            //    0  1  2  3  4  5  6  7  8  9  10
                { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0}, // 0
                { 0, 0, 0, 1, 1, 0, 1, 1, 0, 0, 0}, // 1
                { 0, 0, 1, 1, 1, 1, 1, 1, 1, 0, 0}, // 2
                { 0, 0, 1, 1, 1, 1, 1, 1, 1, 0, 0}, // 3
                { 0, 0, 1, 1, 1, 1, 1, 1, 1, 0, 0}, // 4
                { 0, 1, 1, 1, 1, 1, 1, 1, 1, 1, 0}, // 5
                { 0, 1, 1, 1, 1, 1, 1, 1, 1, 1, 0}, // 6
                { 0, 1, 1, 1, 1, 1, 1, 1, 1, 1, 0}, // 7
                { 0, 0, 1, 1, 1, 1, 1, 1, 1, 0, 0}, // 8
                { 0, 0, 0, 1, 1, 1, 1, 1, 0, 0, 0}, // 9
                { 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0}, // 10
            };

            saloonInt = new int[11, 11]
            {
            //    0  1  2  3  4  5  6  7  8  9  10
                { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0}, // 0
                { 0, 1, 1, 1, 1, 0, 1, 0, 1, 1, 0}, // 1
                { 0, 1, 1, 1, 1, 1, 1, 0, 1, 1, 0}, // 2
                { 0, 0, 0, 0, 0, 0, 0, 0, 1, 1, 0}, // 3
                { 0, 1, 0, 1, 0, 1, 0, 1, 1, 1, 0}, // 4
                { 0, 1, 1, 1, 1, 1, 1, 1, 1, 1, 0}, // 5
                { 0, 1, 0, 0, 1, 1, 1, 0, 0, 1, 0}, // 6
                { 0, 1, 0, 0, 1, 1, 1, 0, 0, 1, 0}, // 7
                { 0, 1, 0, 0, 1, 1, 1, 0, 0, 1, 0}, // 8
                { 0, 1, 1, 1, 1, 1, 1, 1, 1, 1, 0}, // 9
                { 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0}, // 10
            };

            TownhallInt = new int[11, 11]
            {
            //    0  1  2  3  4  5  6  7  8  9  10
                { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0}, // 0
                { 0, 1, 1, 1, 1, 0, 1, 1, 1, 1, 0}, // 1
                { 0, 1, 1, 1, 1, 1, 1, 1, 1, 1, 0}, // 2
                { 0, 1, 1, 1, 1, 1, 1, 1, 1, 1, 0}, // 3
                { 0, 1, 1, 1, 1, 1, 1, 1, 1, 1, 0}, // 4
                { 0, 1, 1, 1, 1, 1, 1, 1, 1, 1, 0}, // 5
                { 0, 1, 0, 0, 1, 1, 1, 0, 0, 1, 0}, // 6
                { 0, 1, 0, 0, 1, 1, 1, 0, 0, 1, 0}, // 7
                { 0, 1, 0, 0, 1, 1, 1, 0, 0, 1, 0}, // 8
                { 0, 1, 1, 1, 1, 1, 1, 1, 1, 1, 0}, // 9
                { 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0}, // 10
            };

            WorkshopInt = new int[11, 11]
            {
            //    0  1  2  3  4  5  6  7  8  9  10
                { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0}, // 0
                { 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0}, // 1
                { 0, 0, 0, 1, 1, 1, 1, 1, 0, 0, 0}, // 2
                { 0, 0, 0, 1, 1, 1, 1, 1, 0, 0, 0}, // 3
                { 0, 0, 1, 1, 1, 1, 1, 1, 1, 1, 0}, // 4
                { 0, 0, 1, 1, 1, 1, 1, 1, 1, 1, 0}, // 5
                { 0, 1, 1, 1, 1, 1, 1, 1, 1, 1, 0}, // 6
                { 0, 1, 1, 1, 1, 1, 1, 1, 1, 1, 0}, // 7
                { 0, 1, 1, 1, 1, 1, 1, 1, 1, 1, 0}, // 8
                { 0, 1, 1, 1, 1, 1, 1, 1, 1, 1, 0}, // 9
                { 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0}, // 10
            };

            spaceCPU = new int[16, 16]
            {
            //    0  1  2  3  4  5  6  7  8  9  10
                { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0}, // 0
                { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0}, // 1
                { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0}, // 2
                { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0}, // 3
                { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0}, // 4
                { 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0}, // 5
                { 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0}, // 6
                { 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0}, // 7
                { 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0}, // 8
                { 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0}, // 9
                { 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0}, // 10
                { 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0}, // 6
                { 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0}, // 7
                { 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0}, // 8
                { 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0}, // 9
                { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0}, // 10
            };

            #endregion

            // adaptive level layouts 
            initLevelLayouts();

            base.Initialize();
        }

        public void initLevelLayouts()
        {
            // 0 = not walkable
            // 1 = walkable
            // 2 = wilderness (walkable with chance of battle trigger)


            theVale = new int[50, 50] // this is the grid layout for the test level
            {
            //    0  1  2  3  4  5  6  7  8  9  10 11 12 13 14 15 16 17 18 19 20 21 22 23 24 25 26 27 28 29 
                { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0}, // 0
                { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 1, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0}, // 1
                { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 1, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0}, // 2
                { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 1, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0}, // 3
                { 0, 0, 0, 0, 0, 0, 0, 1, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 1, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0}, // 4
                { 0, 0, 0, 0, 0, 0, 1, 1, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 1, 1, 1, 1, 1, 1, 0, 0, 0, 0, 1, 1, 1, 0, 0, 0, 0, 0, 0, 1, 1, 1, 1, 1, 1, 1, 0, 0, 0, 0, 0}, // 5
                { 0, 0, 0, 0, 0, 0, 1, 1, 1, 1, 1, 0, 0, 0, 0, 0, 0, 1, 1, 1, 1, 1, 1, 1, 1, 0, 0, 0, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 0, 0}, // 6
                { 0, 0, 0, 0, 0, 0, 1, 0, 0, 1, 1, 0, 0, 0, 0, 0, 0, 1, 0, 0, 1, 1, 1, 1, 1, 0, 0, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 0, 0}, // 7
                { 0, 0, 0, 0, 0, 0, 1, 1, 1, 1, 1, 0, 0, 0, 0, 0, 0, 1, 1, 1, 1, 1, 1, 1, 1, 0, B, B, B, 1, 1, 1, B, B, B, B, B, B, B, B, B, B, B, B, B, B, B, A, B, 0}, // 8
                { 0, 0, 1, 1, 1, 1, 1, 1, 1, 1, 1, 0, 0, 1, 0, 0, 0, 1, 1, 1, 1, 1, 1, 1, 1, 0, B, 1, 1, B, B, B, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 0}, // 9
                { 0, 0, 1, 1, 1, 1, 0, 0, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 0, 0, 1, 1, 1, 1, 0, B, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 0}, // 10
                { 0, 0, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 0, B, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 0}, // 11
                { 0, 0, 1, 1, 1, 1, 1, 1, 1, 0, 0, 1, 1, 1, 1, 1, 0, 0, 1, 1, 1, 1, 1, 1, 1, 0, B, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 0}, // 12
                { 0, 0, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 0, B, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 0}, // 13
                { 0, 0, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 0, 0, B, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 0}, // 14
                { 0, 0, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 0, 0, B, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 0}, // 15
                { 0, 0, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 0, 0, B, 1, 1, B, B, B, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 0}, // 16
                { 0, 0, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 0, 0, B, A, B, 1, 1, 1, B, B, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 0}, // 17
                { 0, 0, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 0, 1, 1, 1, 1, 1, 1, 1, 1, 1, B, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 0}, // 18
                { 0, 0, 1, 1, 1, 1, 1, 0, 0, 1, 0, 0, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 0, 1, 1, 1, 1, 1, 1, 1, 1, 1, B, B, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 0}, // 19
                { 0, 0, 1, 1, 1, 1, 1, 0, 0, 1, 0, 0, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 0, 0, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, B, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 0}, // 20
                { 0, 0, 1, 1, 1, 1, 1, 0, 0, 1, 0, 0, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 0, 0, 0, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, B, B, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 0}, // 21
                { 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, B, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 0}, // 22
                { 0, 0, 0, 0, 0, 0, 0, 0, 1, 1, 1, 0, 0, 0, 1, 1, 1, 1, 1, 1, 0, 0, 0, 0, 0, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, B, B, B, B, B, B, B, B, B, B, B, 0}, // 23
                { 0, 0, 0, 0, 0, 0, 0, 0, 1, 1, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 0, 0}, // 24
                { 0, 0, 0, 0, 0, 0, 0, 0, 1, 1, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 1, 1, 1, 1, 1, 1, 1, 1, 0, 0, 0, 0, 0, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 0, 0}, // 25
                { 0, 0, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 0, 0, 0, 0, 1, 0, 0, 0, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 0, 0, 0, 0, 0, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 0, 0}, // 26
                { 0, 0, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 0, 0, 0, 0, 0, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 0, 0}, // 27
                { 0, 0, 1, 0, 0, 0, 0, 0, 1, 1, 1, 1, 0, 0, 0, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 0, 0, 1, 0, 0, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 0, 0}, // 28
                { 0, 0, 1, 0, 0, 0, 0, 0, 1, 1, 1, 1, 0, 0, 0, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 0}, // 29
                { 0, 0, 1, 0, 0, 0, 0, 0, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1}, // 30
                { 0, 0, 1, 0, 0, 0, 0, 0, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 0}, // 31
                { 0, 0, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 0, 0}, // 32
                { 0, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 0, 0, 0, 0, 0, 1, 1, 1, 0, 0}, // 33
                { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 0, 0, 0, 0, 0, 1, 1, 1, 0, 0}, // 34
                { 0, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 0, 0, 0, 0, 0, 1, 1, 1, 0, 0}, // 35
                { 0, 0, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 0, 0, 0, 0, 0, 1, 1, 1, 0, 0}, // 36
                { 0, 0, 1, 1, 1, B, B, B, B, B, B, B, B, B, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 0, 0}, // 37
                { 0, 0, 1, 1, 1, B, 1, 1, 1, 1, 1, 1, 1, B, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 0, 0}, // 38
                { 0, 0, 1, 1, 1, B, 1, 1, 1, 1, 1, 1, 1, B, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 0, 0}, // 39
                { 0, 0, 1, 1, 1, B, 1, 1, 1, 1, 1, 1, 1, B, 1, 1, 1, 1, 1, 1, 0, 0, 0, 0, 0, 0, 0, 0, 1, 1, 1, 1, 1, 1, 1, 1, 1, 0, 0, 0, 0, 0, 0, 0, 0, 1, 1, 1, 0, 0}, // 40
                { 0, 0, 1, 1, 1, B, 1, 1, 1, 1, 1, 1, 1, B, 1, 1, 1, 1, 1, 1, 0, 0, 0, 0, 0, 0, 0, 0, 1, 1, 1, 1, 1, 1, 1, 1, 1, 0, 0, 0, 0, 0, 0, 0, 0, 1, 1, 1, 0, 0}, // 41
                { 0, 0, 1, 1, 1, B, 1, 1, 1, 1, 1, 1, 1, B, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 0, 0}, // 42
                { 0, 0, 1, 1, 1, B, 1, 1, 1, 1, 1, 1, 1, B, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 0, 0}, // 43
                { 0, 0, 1, 1, 1, B, B, B, B, B, B, B, B, B, 1, 1, 1, 1, 1, 1, 0, 0, 0, 0, 0, 0, 0, 0, 1, 1, 1, 1, 1, 1, 1, 1, 1, 0, 0, 0, 0, 0, 0, 0, 0, 1, 1, 1, 0, 0}, // 44
                { 0, 0, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 0, 0, 0, 0, 0, 0, 0, 0, 1, 1, 1, 1, 1, 1, 1, 1, 1, 0, 0, 0, 0, 0, 0, 0, 0, 1, 1, 1, 0, 0}, // 45
                { 0, 0, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 0, 0}, // 46
                { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 1, 1, 1, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0}, // 47
                { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 1, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0}, // 48
                { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0}, // 49
            };

            

            westRoad = new int[30, 30] // this is the grid layout for the West Road
            {
            //    0  1  2  3  4  5  6  7  8  9  10 11 12 13 14 15 16 17 18 19 20 21 22 23 24 25 26 27 28 29 
                { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0}, // 0
                { 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0}, // 1
                { 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0}, // 2
                { 0, 0, 0, 0, 0, 1, 1, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0}, // 3
                { 0, 0, 0, 0, 2, 2, 2, 2, 2, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0}, // 4
                { 0, 0, 0, 0, 2, 2, 2, 2, 2, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 2, 0, 0, 0, 0, 0, 0, 0}, // 5
                { 0, 0, 0, 2, 2, 2, 2, 2, 2, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 2, 2, 2, 2, 2, 2, 0, 0, 0}, // 6
                { 0, 0, 2, 2, 2, 2, 2, 2, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 2, 2, 2, 2, 2, 2, 2, 2, 0, 0}, // 7
                { 0, 0, 2, 2, 2, 2, 2, 0, 0, 0, 0, 0, 0, 2, 0, 0, 0, 0, 2, 2, 2, 2, 0, 2, 2, 2, 2, 2, 0, 0}, // 8
                { 0, 0, 2, 2, 2, 2, 2, 2, 0, 0, 0, 0, 2, 2, 0, 0, 0, 0, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 0, 0}, // 9
                { 0, 0, 2, 2, 2, 2, 2, 0, 0, 2, 2, 2, 2, 2, 0, 0, 0, 0, 2, 2, 2, 2, 2, 2, 0, 0, 2, 2, 1, 0}, // 10
                { 0, 0, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 0, 0, 0, 0, 0, 0, 2, 2, 2, 2, 2, 2, 2, 2, 1, 1}, // 11
                { 0, 0, 2, 2, 0, 0, 2, 2, 2, 2, 2, 2, 2, 2, 0, 0, 0, 0, 0, 0, 0, 2, 2, 2, 2, 2, 2, 2, 1, 0}, // 12
                { 0, 0, 2, 2, 2, 2, 2, 0, 2, 2, 2, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 2, 2, 2, 2, 2, 2, 0, 0}, // 13
                { 0, 0, 2, 2, 2, 2, 2, 2, 2, 2, 0, 0, 0, 0, 0, 2, 0, 0, 0, 0, 0, 0, 0, 2, 2, 2, 2, 2, 0, 0}, // 14
                { 0, 0, 2, 2, 2, 2, 2, 2, 2, 0, 0, 0, 0, 0, 0, 2, 2, 0, 0, 0, 0, 0, 0, 2, 2, 2, 2, 2, 0, 0}, // 15
                { 0, 0, 2, 2, 2, 2, 2, 2, 2, 0, 0, 0, 0, 0, 0, 2, 2, 2, 0, 0, 0, 0, 0, 2, 2, 2, 2, 2, 0, 0}, // 16
                { 0, 0, 2, 2, 2, 2, 2, 2, 0, 0, 0, 0, 0, 0, 0, 0, 2, 2, 0, 0, 0, 0, 0, 0, 2, 2, 2, 2, 0, 0}, // 17
                { 0, 0, 2, 2, 2, 2, 2, 2, 2, 2, 0, 0, 0, 0, 0, 0, 2, 2, 0, 0, 0, 0, 0, 0, 2, 2, 2, 2, 0, 0}, // 18
                { 0, 0, 2, 2, 2, 2, 2, 2, 2, 2, 2, 0, 0, 0, 0, 2, 2, 2, 2, 0, 0, 0, 0, 0, 2, 2, 2, 2, 0, 0}, // 19
                { 0, 0, 2, 2, 2, 0, 0, 2, 2, 2, 2, 0, 0, 0, B, B, B, B, B, B, 0, 0, 0, 0, 2, 2, 2, 2, 0, 0}, // 20
                { 0, 0, 0, 2, 2, 2, 2, 2, 2, 2, 0, 0, 0, 0, B, B, B, B, B, B, 0, 0, 0, 0, 2, 2, 2, 2, 0, 0}, // 21
                { 0, 0, 0, 2, 2, 2, 2, 2, 2, 2, 0, 0, 0, 0, B, B, B, B, B, B, 0, 0, 0, 0, 2, 2, 2, 2, 0, 0}, // 22
                { 0, 0, 2, 2, 2, 2, 2, 2, 2, 2, 2, 0, 0, 2, 2, 2, 2, 2, 2, 2, 2, 0, 0, 2, 2, 2, 2, 2, 0, 0}, // 23
                { 0, 0, 2, 2, 0, 0, 2, 2, 2, 2, 2, 2, 2, 0, 2, 2, 2, 2, 2, 2, 0, 2, 2, 2, 2, 2, 2, 2, 0, 0}, // 24
                { 0, 0, 0, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 0, 0}, // 25
                { 0, 0, 0, 2, 2, 2, 2, 2, 0, 0, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 0, 0}, // 26
                { 0, 0, 0, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 0, 0}, // 27
                { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0}, // 28
                { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0}, // 29
            };

            
        }

        /////////////////////////////////////////////////////////////////////////////// ** LOAD CONTENT ** //////////////////////////////////////////////////////////////////////////////////////////////////////////////

        protected override void LoadContent()                           // Initial content load, happens only at start of the game and loads persistent data
        {                                                               // 
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            debugFont = Content.Load<SpriteFont>("DebugFont");
            uiFontOne = Content.Load<SpriteFont>("UIFont1");
            uiFontTwo = Content.Load<SpriteFont>("UIFont2");
            singlePix = Content.Load<Texture2D>("SinglePixel");
            cursorTex = Content.Load<Texture2D>("Cursor");
            battleUICover = Content.Load<Texture2D>("BattleUICover");
            dialogUI = Content.Load<Texture2D>("DialogUI");
            dialogUICover = Content.Load<Texture2D>("DialogUICover");
            portraits = Content.Load<Texture2D>("Portraits");
            pauseMenuTex = Content.Load<Texture2D>("PauseMenu");
            notificationBox = Content.Load<Texture2D>("NotificationBox");

            flameCore = Content.Load<Texture2D>("EnemySprites//FlameCore");
            flameOut = Content.Load<Texture2D>("EnemySprites//FlameOuter");
            iceCore = Content.Load<Texture2D>("EnemySprites//IceCore");
            iceOut = Content.Load<Texture2D>("EnemySprites//IceOuter");
            earthCore = Content.Load<Texture2D>("EnemySprites//EarthCore");
            earthOut = Content.Load<Texture2D>("EnemySprites//EarthOuter");

            contTex = Content.Load<Texture2D>("StartUI//Resume");
            howToTex = Content.Load<Texture2D>("StartUI//HowTo");
            newGameTex = Content.Load<Texture2D>("StartUI//NewGame");
            quitTex = Content.Load<Texture2D>("StartUI//Quit");
            howToPageTex = Content.Load<Texture2D>("StartUI//Controls");
            startBack = Content.Load<Texture2D>("StartUI//StartBack");
            howToPageTwo = Content.Load<Texture2D>("StartUI//Overview");

            blackScreenTex = Content.Load<Texture2D>("Slides//BlackScreen");
            rockTex  = Content.Load<Texture2D>("Maps//RockSml");

            dialogBlip = Content.Load<SoundEffect>("Sounds//DiBlip");
            uiMovCurs = Content.Load<SoundEffect>("Sounds//ButtonSwitch");
            dramaHorn = Content.Load<SoundEffect>("Sounds//dramatic-horn");
            chestOpenFX = Content.Load<SoundEffect>("Sounds//ChestOpen");
            potionDrink = Content.Load<SoundEffect>("Sounds//Potion");
            doorOpenFX = Content.Load<SoundEffect>("Sounds//Door");
            vendFX = Content.Load<SoundEffect>("Sounds//Vend");
            encounterFX = Content.Load<SoundEffect>("Sounds//Jump03");

            battleTheme = Content.Load<Song>( "Sounds//battle theme for dan" );
            eerieTheme = Content.Load<Song>("Sounds//Eerie");
            generalTheme = Content.Load<Song>("Sounds//GeneralTheme");
            villageTheme = Content.Load<Song>("Sounds//Village");

            PInfo = new PlayerInfo(singlePix, singlePix);

            diMan = new DialogManager(PInfo, " ", portraits, camera.Position, vendFX, potionDrink);

            pMenu = new PauseMenu(pauseMenuTex, cursorTex, PInfo);

            reLoadContent();
        }

        /////////////////////////////////////////////////////////////////////////////// ** RELOAD CONTENT ** ///////////////////////////////////////////////////////////////////////////////////////////////////////////

        public void reLoadContent()                                                                                // reloadContent deals with loading and reloading content that requires updating 
        {                                                                                                          // during gameplay i.e. level maps, gamemode layouts
            // stop current track
            MediaPlayer.Stop();

            // Clear the Lists
            currNPCs.RemoveAll(npc => npc.CharName != " ");
            aniItemList.RemoveAll(item => item.Position != Vector2.Zero);       // remove items ** NEVER PLACE AT VECTOR 0,0 **
            WaterTiles.RemoveAll(tile => tile.Position != Vector2.Zero);        // remove tiles
            WaterTiles.RemoveAll(tile => tile.Position == Vector2.Zero);
            ChestList.RemoveAll(chest => chest.Position != Vector2.Zero);       // remove tiles ** NEVER PLACE AT VECTOR 0,0 **
            DoorList.RemoveAll(door => door.DoorDestination != -1);
            RocksList.RemoveAll(rocks => rocks.Position != Vector2.Zero);       // remove items ** NEVER PLACE AT VECTOR 0,0 **

            currUnderlay = new StaticGraphic(Vector2.Zero, singlePix);

            // Load New Content
            if (currState == GameState.StartScreen)
            {
                // LOAD STARTSCREEN
                strtScreen = new StaticGraphic(Vector2.Zero, Content.Load<Texture2D>("Maps//TheClearing"));
                blackScreen = new StaticGraphic(Vector2.Zero, blackScreenTex);
                sMenu = new StartMenu(startBack, howToPageTex, cursorTex, newGameTex, howToTex, contTex, quitTex, howToPageTwo);

                if (titleDone)
                {
                    MediaPlayer.Play(eerieTheme);
                    MediaPlayer.Volume = 0.2f;
                    MediaPlayer.IsRepeating = true;
                }
            }
            else if (currState == GameState.GameWorld)
            {
                // Load PLayer
                theWanderer = new PlayerClass(Content.Load<Texture2D>("WW_Brown"), newPlayerPos, 4, 4, 6);
                OpeningSlides.Add(new FadingGraphic(-camera.Position, blackScreenTex, 1.2f));

                switch (currLevel)
                {
                    case Level.Clearing:

                        // Load Map
                        switch (environStage)
                        {
                            case 1:
                                currMap = new StaticGraphic(new Vector2(0, -240), Content.Load<Texture2D>("Maps//TheClearing"));
                                break;
                            case 2:
                                currMap = new StaticGraphic(new Vector2(0, -240), Content.Load<Texture2D>("Maps//TheClearingWIN"));
                                break;
                            case 3:
                                currMap = new StaticGraphic(new Vector2(0, -240), Content.Load<Texture2D>("Maps//TheClearingDRY"));
                                break;
                        }
                        currOverlay = new StaticGraphic(new Vector2(0, -240), Content.Load<Texture2D>("Maps//TheClearingOverlay"));
                        currUnderlay = new StaticGraphic(Vector2.Zero, singlePix);

                        //add rocks
                        RocksList.Add(new StaticGraphic(new Vector2(7,19), rockTex));
                        RocksList.Add(new StaticGraphic(new Vector2(14,19), rockTex));

                        // add shard
                        if (firstHeroEnc)
                        {
                            currNPCs.Add(new baseNPC(Content.Load<Texture2D>("Fragment"), new Vector2(7, 19), 1, 3, 6, "Legendary Hero"));
                        }

                        // add extras
                        aniItemList.Add(new Animated2D(Content.Load<Texture2D>("CampFire"), new Vector2(22, 22), 1, 5, 6));
                        ChestList.Add(new ChestClass(new Vector2(21,17), Content.Load<Texture2D>("Chest"), 0, 1));
                        ChestList.Add(new ChestClass(new Vector2(23, 17), Content.Load<Texture2D>("Chest"), 1, 2));

                        // add enemies
                        currNPCs.Add(new EnemySprite(Content.Load<Texture2D>("EnemySprites//ThornStrip"), new Vector2(13, 18), 1, 3, 6, "DrowzyBush", 52));

                        // Add Doors 
                        DoorList.Add(new Door(new Vector2(17, 48), 1, new Vector2(14, 15)));
                        DoorList.Add(new Door(new Vector2(17, 48), 1, new Vector2(15, 15)));

                        if (!titleScrolled)
                        {
                            camera.Position = new Vector2(0, 240);
                        }
                        else { camera.Position = new Vector2(0, -840); }

                        //music
                        MediaPlayer.Play(generalTheme);
                        MediaPlayer.IsRepeating = true;
                        MediaPlayer.Volume = genThemeVol;

                        //Generate Nav Grid
                        navGrid = new TileClass[30, 30]; // sets nav grid to same size as test level
                        BuildNavGrid(theClearing);

                        break;

                    case Level.Vale:

                        // Load Map
                        switch (environStage)
                        {
                            case 1:
                                currMap = new StaticGraphic(Vector2.Zero, Content.Load<Texture2D>("Maps//TestMap"));
                                currOverlay = new StaticGraphic(Vector2.Zero, Content.Load<Texture2D>("Maps//The vAle Small OVERLAY"));
                                break;
                            case 2:
                                currMap = new StaticGraphic(Vector2.Zero, Content.Load<Texture2D>("Maps//ValeWinter"));
                                currOverlay = new StaticGraphic(Vector2.Zero, Content.Load<Texture2D>("Maps//The vAle Small OVERLAY DRY"));
                                break;
                            case 3:
                                currMap = new StaticGraphic(Vector2.Zero, Content.Load<Texture2D>("Maps//ValeDry"));
                                currOverlay = new StaticGraphic(Vector2.Zero, Content.Load<Texture2D>("Maps//The vAle Small OVERLAY DRY"));
                                break;
                        }
                        currUnderlay = new StaticGraphic(Vector2.Zero, Content.Load<Texture2D>("Maps//ValeUnder"));

                        // Load Required NPCs (always load before generating nav grid)
                        
                        if (PInfo.GetCompanion != Companion.TheWarrior) { currNPCs.Add(new baseNPC(Content.Load<Texture2D>("HewRanIdle"), new Vector2(13, 31), 1, 2, 6, "Hewran")); }
                        if (PInfo.GetCompanion != Companion.TheWitchdoctor) { currNPCs.Add(new baseNPC(Content.Load<Texture2D>("WitchDocTwo"), new Vector2(28, 18), 1, 2, 6, "Nerwen")); }

                        if (environStage != 3) { currNPCs.Add(new baseNPC(Content.Load<Texture2D>("proff"), new Vector2(38, 28), 1, 2, 6, "Calli")); }
                        currNPCs.Add(new baseNPC(Content.Load<Texture2D>("Broox"), new Vector2(19, 27), 1, 2, 6, "Broox"));
                        

                        currNPCs.Add(new baseNPC(Content.Load<Texture2D>("HealthVend"), new Vector2(21, 26), 1, 2, 6, "hVend"));
                        currNPCs.Add(new baseNPC(Content.Load<Texture2D>("MagickVend"), new Vector2(22, 26), 1, 2, 6, "mVend"));
                        

                        // load extras
                        RocksList.Add(new StaticGraphic(new Vector2(9,40), rockTex));
                        RocksList.Add(new StaticGraphic(new Vector2(32,41), rockTex));
                        RocksList.Add(new StaticGraphic(new Vector2(33,42), rockTex));
                        RocksList.Add(new StaticGraphic(new Vector2(31,42), rockTex));
                        RocksList.Add(new StaticGraphic(new Vector2(4,9), rockTex));
                        RocksList.Add(new StaticGraphic(new Vector2(16, 16), rockTex));
                        RocksList.Add(new StaticGraphic(new Vector2(20, 18), rockTex));
                        RocksList.Add(new StaticGraphic(new Vector2(24, 28), rockTex));
                        RocksList.Add(new StaticGraphic(new Vector2(25, 25), rockTex));
                        RocksList.Add(new StaticGraphic(new Vector2(43, 26), rockTex));
                        RocksList.Add(new StaticGraphic(new Vector2(7, 42), rockTex));
                        RocksList.Add(new StaticGraphic(new Vector2(31, 0), rockTex));
                        RocksList.Add(new StaticGraphic(new Vector2(29, 0), rockTex));
                        RocksList.Add(new StaticGraphic(new Vector2(29, 1), rockTex));
                        RocksList.Add(new StaticGraphic(new Vector2(31, 1), rockTex));
                        ChestList.Add(new ChestClass(new Vector2(32, 42), Content.Load<Texture2D>("Chest"), 48, 3));                                                         
                        ChestList.Add(new ChestClass(new Vector2(19, 23), Content.Load<Texture2D>("Chest"), 49, 1));
                        ChestList.Add(new ChestClass(new Vector2(35, 20), Content.Load<Texture2D>("Chest"), 50, 2));
                        ChestList.Add(new ChestClass(new Vector2(3, 9), Content.Load<Texture2D>("Chest"), 51, 6));
                        ChestList.Add(new ChestClass(new Vector2(9, 40), Content.Load<Texture2D>("Chest"), 52, 4));                                                                 

                        // add doors
                        DoorList.Add(new Door(new Vector2(14, 16), 0, new Vector2(17, 49)));     // to clearing (0)
                        DoorList.Add(new Door(new Vector2(29, 11), 3, new Vector2(0, 34)));      // to WestPath (3)
                        DoorList.Add(new Door(new Vector2(1, 9), 2, new Vector2(49, 30)));      // to EastPath (2)
                        DoorList.Add(new Door(new Vector2(28, 48), 5, new Vector2(41, 4)));      // to CaveMain (5)
                        DoorList.Add(new Door(new Vector2(14,15), 7, new Vector2(30,1)));       // to Shrine [ritual peak] (7)

                        DoorList.Add(new Door(new Vector2(5, 9), 9, new Vector2(13, 9)));        // to treehouse (9)
                        DoorList.Add(new Door(new Vector2(4, 6), 10, new Vector2(17, 26)));      // to casino (10)
                        DoorList.Add(new Door(new Vector2(5, 9), 11, new Vector2(34, 28)));      // to saloon (11)
                        DoorList.Add(new Door(new Vector2(5, 9), 12, new Vector2(27, 36)));      // to TownHall (12)

                        // load water
                        // if stage 1
                        for (int i = 0; i < 50; i++)
                        {
                            for (int j = 0; j < 50; j++)
                            {
                                WaterTiles.Add(new Animated2D(Content.Load<Texture2D>("Ocean_SpriteSheet"), new Vector2(j, i), 2, 8, 2));
                            }
                        }

                        //music
                        MediaPlayer.Play(villageTheme);
                        MediaPlayer.IsRepeating = true;
                        MediaPlayer.Volume = villThemeVol;

                        //Generate Nav Grid
                        navGrid = new TileClass[50, 50]; // sets nav grid to same size as test level
                        BuildNavGrid(theVale);

                        if (heroEncPrepped)
                        {
                            PInfo.HeroEncTrig = true;
                            heroEncPrepped = false;
                        }

                        break;

                    case Level.EastRoad:

                        // Load Map
                        switch (environStage)
                        {
                            case 1:
                                currMap = new StaticGraphic(Vector2.Zero, Content.Load<Texture2D>("Maps//EastPath"));
                                currOverlay = new StaticGraphic(new Vector2(0, 0), Content.Load<Texture2D>("Maps//EastPathOverlay"));
                                break;
                            case 2:
                                currMap = new StaticGraphic(Vector2.Zero, Content.Load<Texture2D>("Maps//EastPathWIN"));
                                currOverlay = new StaticGraphic(new Vector2(0, 0), Content.Load<Texture2D>("Maps//EastPathOverlayDRY"));
                                break;
                            case 3:
                                currMap = new StaticGraphic(Vector2.Zero, Content.Load<Texture2D>("Maps//EastPathDRY"));
                                currOverlay = new StaticGraphic(new Vector2(0, 0), Content.Load<Texture2D>("Maps//EastPathOverlayDRY"));
                                break;
                        }
                        currUnderlay = new StaticGraphic(Vector2.Zero, singlePix);

                        // add extras
                        ChestList.Add(new ChestClass(new Vector2(11, 9), Content.Load<Texture2D>("Chest"), 21, 1));
                        ChestList.Add(new ChestClass(new Vector2(13, 11), Content.Load<Texture2D>("Chest"), 22, 3));
                        ChestList.Add(new ChestClass(new Vector2(14, 2), Content.Load<Texture2D>("Chest"), 23, 1));
                        ChestList.Add(new ChestClass(new Vector2(28, 6), Content.Load<Texture2D>("Chest"), 24, 2));
                        ChestList.Add(new ChestClass(new Vector2(13, 16), Content.Load<Texture2D>("Chest"), 25, 3));
                        ChestList.Add(new ChestClass(new Vector2(10, 26), Content.Load<Texture2D>("Chest"), 26, 1));
                        ChestList.Add(new ChestClass(new Vector2(11, 26), Content.Load<Texture2D>("Chest"), 27, 4));
                        ChestList.Add(new ChestClass(new Vector2(24, 25), Content.Load<Texture2D>("Chest"), 28, 1));

                        // add enemy NPCs
                        currNPCs.Add(new EnemySprite(Content.Load<Texture2D>("EnemySprites//ThornStrip"), new Vector2(5, 6), 1, 3, 6, "Thorn", 32));
                        currNPCs.Add(new EnemySprite(Content.Load<Texture2D>("EnemySprites//ThornStrip"), new Vector2(5, 14), 1, 3, 6, "Thorn", 33));
                        currNPCs.Add(new EnemySprite(Content.Load<Texture2D>("EnemySprites//ThornStrip"), new Vector2(6, 27), 1, 3, 6, "Thorn", 34));
                        currNPCs.Add(new EnemySprite(Content.Load<Texture2D>("EnemySprites//ThornStrip"), new Vector2(8, 27), 1, 3, 6, "Thorn", 35));
                        currNPCs.Add(new EnemySprite(Content.Load<Texture2D>("EnemySprites//ThornStrip"), new Vector2(14, 24), 1, 3, 6, "Thorn", 36));
                        currNPCs.Add(new EnemySprite(Content.Load<Texture2D>("EnemySprites//ThornStrip"), new Vector2(19, 25), 1, 3, 6, "Thorn", 37));
                        currNPCs.Add(new EnemySprite(Content.Load<Texture2D>("EnemySprites//ThornStrip"), new Vector2(19, 27), 1, 3, 6, "Thorn", 38));
                        currNPCs.Add(new EnemySprite(Content.Load<Texture2D>("EnemySprites//ThornStrip"), new Vector2(14, 16), 1, 3, 6, "Thorn", 39));

                        // Add Doors 
                        DoorList.Add(new Door(new Vector2(49, 30), 1, new Vector2(0, 9)));      // to vale
                        DoorList.Add(new Door(new Vector2(5, 9), 6, new Vector2(25, 8)));        // to workshop

                        //music
                        MediaPlayer.Play(generalTheme);
                        MediaPlayer.IsRepeating = true;
                        MediaPlayer.Volume = genThemeVol;

                        //Generate Nav Grid
                        navGrid = new TileClass[30, 30]; // sets nav grid to same size as test level
                        BuildNavGrid(eastRoad);
                        break;

                    case Level.WestRoad:

                        // Load Map
                        switch (environStage)
                        {
                            case 1:
                                currMap = new StaticGraphic(Vector2.Zero, Content.Load<Texture2D>("Maps//WestPath"));
                                currOverlay = new StaticGraphic(Vector2.Zero, Content.Load<Texture2D>("Maps//WestPathOverlay"));
                                break;
                            case 2:
                                currMap = new StaticGraphic(Vector2.Zero, Content.Load<Texture2D>("Maps//WestPathWin"));
                                currOverlay = new StaticGraphic(Vector2.Zero, Content.Load<Texture2D>("Maps//WestPathOverlayWIN"));
                                break;
                            case 3:
                                currMap = new StaticGraphic(Vector2.Zero, Content.Load<Texture2D>("Maps//WestPathDRY"));
                                currOverlay = new StaticGraphic(Vector2.Zero, Content.Load<Texture2D>("Maps//WestPathOverlayWIN"));
                                break;
                        }
                        
                        currUnderlay = new StaticGraphic(Vector2.Zero, singlePix);

                        // add extras
                        ChestList.Add(new ChestClass(new Vector2(22, 5), Content.Load<Texture2D>("Chest"), 2, 1));
                        ChestList.Add(new ChestClass(new Vector2(13, 8), Content.Load<Texture2D>("Chest"), 3, 3));
                        ChestList.Add(new ChestClass(new Vector2(17, 16), Content.Load<Texture2D>("Chest"), 4, 4));
                        ChestList.Add(new ChestClass(new Vector2(2, 23), Content.Load<Texture2D>("Chest"), 5, 1));
                        ChestList.Add(new ChestClass(new Vector2(10, 19), Content.Load<Texture2D>("Chest"), 6, 3));

                        // load enemy npcs
                        currNPCs.Add(new EnemySprite(Content.Load<Texture2D>("EnemySprites//ThornStrip"), new Vector2(22, 6), 1, 3, 6, "Thorn", 0));
                        currNPCs.Add(new EnemySprite(Content.Load<Texture2D>("EnemySprites//ThornStrip"), new Vector2(18, 9), 1, 3, 6, "Thorn", 1));
                        currNPCs.Add(new EnemySprite(Content.Load<Texture2D>("EnemySprites//ThornStrip"), new Vector2(8, 14), 1, 3, 6, "Thorn", 2));
                        currNPCs.Add(new EnemySprite(Content.Load<Texture2D>("EnemySprites//ThornStrip"), new Vector2(6, 12), 1, 3, 6, "Thorn", 3));
                        currNPCs.Add(new EnemySprite(Content.Load<Texture2D>("EnemySprites//ThornStrip"), new Vector2(26, 26), 1, 3, 6, "Thorn", 4));
                        currNPCs.Add(new EnemySprite(Content.Load<Texture2D>("EnemySprites//ThornStrip"), new Vector2(25, 19), 1, 3, 6, "Thorn", 5));
                        currNPCs.Add(new EnemySprite(Content.Load<Texture2D>("EnemySprites//ThornStrip"), new Vector2(6, 25), 1, 3, 6, "Thorn", 6));
                        currNPCs.Add(new EnemySprite(Content.Load<Texture2D>("EnemySprites//ThornStrip"), new Vector2(3, 16), 1, 3, 6, "Thorn", 7));

                        // Add Doors 
                        DoorList.Add(new Door(new Vector2(1, 34), 1, new Vector2(29, 11)));    // to vale
                        DoorList.Add(new Door(new Vector2(17, 49), 4, new Vector2(6, 1)));     // to old forest
                        DoorList.Add(new Door(new Vector2(4, 7), 13, new Vector2(15, 14)));      //add to spell cave

                        // load water
                        // if stage 1
                        for (int i = 0; i < 6; i++)
                        {
                            for (int j = 0; j < 6; j++)
                            {
                                WaterTiles.Add(new Animated2D(Content.Load<Texture2D>("Ocean_SpriteSheet"), new Vector2(j, i) + new Vector2(14,20), 2, 8, 2));
                            }
                        }

                        //music
                        MediaPlayer.Play(generalTheme);
                        MediaPlayer.IsRepeating = true;
                        MediaPlayer.Volume = genThemeVol;

                        //Generate Nav Grid
                        navGrid = new TileClass[30, 30]; // sets nav grid to same size as test level
                        BuildNavGrid(westRoad);

                        if (heroEncPrepped)
                        {
                            PInfo.HeroEncTrig = true;
                            heroEncPrepped = false;
                        }

                        break;

                    case Level.OldForest:

                        // Load Map
                        currMap = new StaticGraphic(Vector2.Zero, Content.Load<Texture2D>("Maps//OldForestOne"));
                        currOverlay = new StaticGraphic(Vector2.Zero, Content.Load<Texture2D>("Maps//OldForestOverlayCanopy"));
                        currUnderlay = new StaticGraphic(Vector2.Zero, singlePix);

                        // add extras
                        aniItemList.Add(new Animated2D(Content.Load<Texture2D>("CampFire"), new Vector2(14, 27), 1, 5, 6));

                        ChestList.Add(new ChestClass(new Vector2(3, 44), Content.Load<Texture2D>("Chest"), 7, 1));
                        ChestList.Add(new ChestClass(new Vector2(46, 45), Content.Load<Texture2D>("Chest"), 8, 3));
                        ChestList.Add(new ChestClass(new Vector2(34, 32), Content.Load<Texture2D>("Chest"), 9, 1));
                        ChestList.Add(new ChestClass(new Vector2(48, 23), Content.Load<Texture2D>("Chest"), 10, 1));
                        ChestList.Add(new ChestClass(new Vector2(23, 28), Content.Load<Texture2D>("Chest"), 11, 2));
                        ChestList.Add(new ChestClass(new Vector2(17, 29), Content.Load<Texture2D>("Chest"), 12, 5));
                        ChestList.Add(new ChestClass(new Vector2(37, 14), Content.Load<Texture2D>("Chest"), 13, 3));
                        ChestList.Add(new ChestClass(new Vector2(41, 14), Content.Load<Texture2D>("Chest"), 14, 1));
                        ChestList.Add(new ChestClass(new Vector2(4, 26), Content.Load<Texture2D>("Chest"), 15, 3));
                        ChestList.Add(new ChestClass(new Vector2(18, 15), Content.Load<Texture2D>("Chest"), 16, 2));
                        ChestList.Add(new ChestClass(new Vector2(8, 2), Content.Load<Texture2D>("Chest"), 17, 4));
                        ChestList.Add(new ChestClass(new Vector2(28, 2), Content.Load<Texture2D>("Chest"), 18, 1));
                        ChestList.Add(new ChestClass(new Vector2(29, 13), Content.Load<Texture2D>("Chest"), 19, 1));
                        ChestList.Add(new ChestClass(new Vector2(32, 31), Content.Load<Texture2D>("Chest"), 20, 4));

                        if (enemiesDefeated[50])
                        {
                            ChestList.Add(new ChestClass(new Vector2(40, 3), Content.Load<Texture2D>("Chest"), 46, 1));
                            ChestList.Add(new ChestClass(new Vector2(41, 3), Content.Load<Texture2D>("Chest"), 47, 8));
                        }

                        // load enemy npcs
                        currNPCs.Add(new EnemySprite(Content.Load<Texture2D>("EnemySprites//ThornStrip"), new Vector2(5, 36), 1, 3, 6, "Thorn", 8));
                        currNPCs.Add(new EnemySprite(Content.Load<Texture2D>("EnemySprites//ThornStrip"), new Vector2(4, 45), 1, 3, 6, "Thorn", 9));
                        currNPCs.Add(new EnemySprite(Content.Load<Texture2D>("EnemySprites//ThornStrip"), new Vector2(12, 43), 1, 3, 6, "Thorn", 10));
                        currNPCs.Add(new EnemySprite(Content.Load<Texture2D>("EnemySprites//ThornStrip"), new Vector2(12, 44), 1, 3, 6, "Thorn", 11));
                        currNPCs.Add(new EnemySprite(Content.Load<Texture2D>("EnemySprites//ThornStrip"), new Vector2(22, 43), 1, 3, 6, "Thorn", 12));
                        currNPCs.Add(new EnemySprite(Content.Load<Texture2D>("EnemySprites//ThornStrip"), new Vector2(22, 44), 1, 3, 6, "Thorn", 13));
                        currNPCs.Add(new EnemySprite(Content.Load<Texture2D>("EnemySprites//ThornStrip"), new Vector2(37, 43), 1, 3, 6, "Thorn", 14));
                        currNPCs.Add(new EnemySprite(Content.Load<Texture2D>("EnemySprites//ThornStrip"), new Vector2(37, 44), 1, 3, 6, "Thorn", 15));
                        currNPCs.Add(new EnemySprite(Content.Load<Texture2D>("EnemySprites//ThornStrip"), new Vector2(46, 44), 1, 3, 6, "Thorn", 16));
                        currNPCs.Add(new EnemySprite(Content.Load<Texture2D>("EnemySprites//ThornStrip"), new Vector2(35, 32), 1, 3, 6, "Thorn", 17));
                        currNPCs.Add(new EnemySprite(Content.Load<Texture2D>("EnemySprites//ThornStrip"), new Vector2(39, 20), 1, 3, 6, "Thorn", 18));
                        currNPCs.Add(new EnemySprite(Content.Load<Texture2D>("EnemySprites//ThornStrip"), new Vector2(26, 20), 1, 3, 6, "Thorn", 19));
                        currNPCs.Add(new EnemySprite(Content.Load<Texture2D>("EnemySprites//ThornStrip"), new Vector2(27, 21), 1, 3, 6, "Thorn", 20));
                        currNPCs.Add(new EnemySprite(Content.Load<Texture2D>("EnemySprites//ThornStrip"), new Vector2(8, 29), 1, 3, 6, "Thorn", 21));
                        currNPCs.Add(new EnemySprite(Content.Load<Texture2D>("EnemySprites//ThornStrip"), new Vector2(9, 20), 1, 3, 6, "Thorn", 22));
                        currNPCs.Add(new EnemySprite(Content.Load<Texture2D>("EnemySprites//ThornStrip"), new Vector2(7, 2), 1, 3, 6, "Thorn", 23));
                        currNPCs.Add(new EnemySprite(Content.Load<Texture2D>("EnemySprites//ThornStrip"), new Vector2(8, 3), 1, 3, 6, "Thorn", 24));
                        currNPCs.Add(new EnemySprite(Content.Load<Texture2D>("EnemySprites//ThornStrip"), new Vector2(29, 8), 1, 3, 6, "Thorn", 25));
                        currNPCs.Add(new EnemySprite(Content.Load<Texture2D>("EnemySprites//ThornStrip"), new Vector2(30, 9), 1, 3, 6, "Thorn", 26));
                        currNPCs.Add(new EnemySprite(Content.Load<Texture2D>("EnemySprites//ThornStrip"), new Vector2(17, 44), 1, 3, 6, "Thorn", 28));
                        currNPCs.Add(new EnemySprite(Content.Load<Texture2D>("EnemySprites//ThornStrip"), new Vector2(35, 33), 1, 3, 6, "Thorn", 29));
                        currNPCs.Add(new EnemySprite(Content.Load<Texture2D>("EnemySprites//ThornStrip"), new Vector2(2, 12), 1, 3, 6, "Thorn", 30));
                        currNPCs.Add(new EnemySprite(Content.Load<Texture2D>("EnemySprites//ThornStrip"), new Vector2(3, 12), 1, 3, 6, "Thorn", 31));

                        //testing linked enemy entities (FORN BOSS & Tablet Fragment)
                        currNPCs.Add(new BossSprite(Content.Load<Texture2D>("EnemySprites//TreeStrip"), new Vector2(39, 0), 1, 7, 6, "Forn", 50));
                        currNPCs.Add(new EnemySprite(Content.Load<Texture2D>("Fragment"), new Vector2(40, 5), 1, 3, 3, "Forn", 50));

                        // Add Doors 
                        DoorList.Add(new Door(new Vector2(6, 2), 3, new Vector2(17, 49)));

                        //music
                        MediaPlayer.Play(generalTheme);
                        MediaPlayer.IsRepeating = true;
                        MediaPlayer.Volume = genThemeVol;

                        //Generate Nav Grid
                        navGrid = new TileClass[50, 50]; // sets nav grid to same size as test level
                        BuildNavGrid(oldForest);
                        break;

                    case Level.Cave:

                        // Load Map
                        currMap = new StaticGraphic(Vector2.Zero, Content.Load<Texture2D>("Maps//TheCaveMain"));
                        currOverlay = new StaticGraphic(Vector2.Zero, Content.Load<Texture2D>("Maps//TheCaveOverlay"));
                        currUnderlay = new StaticGraphic(Vector2.Zero, singlePix);

                        // add extras
                        ChestList.Add(new ChestClass(new Vector2(28, 44), Content.Load<Texture2D>("Chest"), 29, 1));
                        ChestList.Add(new ChestClass(new Vector2(3, 46), Content.Load<Texture2D>("Chest"), 30, 2));
                        ChestList.Add(new ChestClass(new Vector2(6, 33), Content.Load<Texture2D>("Chest"), 31, 1));
                        ChestList.Add(new ChestClass(new Vector2(47, 45), Content.Load<Texture2D>("Chest"), 32, 2));
                        ChestList.Add(new ChestClass(new Vector2(46, 32), Content.Load<Texture2D>("Chest"), 33, 1));
                        ChestList.Add(new ChestClass(new Vector2(28, 30), Content.Load<Texture2D>("Chest"), 34, 1));
                        ChestList.Add(new ChestClass(new Vector2(1, 29), Content.Load<Texture2D>("Chest"), 35, 4));
                        ChestList.Add(new ChestClass(new Vector2(3, 8), Content.Load<Texture2D>("Chest"), 36, 2));
                        ChestList.Add(new ChestClass(new Vector2(14, 9), Content.Load<Texture2D>("Chest"), 37, 1));
                        ChestList.Add(new ChestClass(new Vector2(8, 10), Content.Load<Texture2D>("Chest"), 38, 1));
                        ChestList.Add(new ChestClass(new Vector2(36, 3), Content.Load<Texture2D>("Chest"), 39, 3));
                        ChestList.Add(new ChestClass(new Vector2(31, 8), Content.Load<Texture2D>("Chest"), 40, 3));
                        ChestList.Add(new ChestClass(new Vector2(28, 40), Content.Load<Texture2D>("Chest"), 41, 1));
                        ChestList.Add(new ChestClass(new Vector2(44, 21), Content.Load<Texture2D>("Chest"), 42, 4));
                        ChestList.Add(new ChestClass(new Vector2(47, 8), Content.Load<Texture2D>("Chest"), 43, 3));
                        ChestList.Add(new ChestClass(new Vector2(22, 9), Content.Load<Texture2D>("Chest"), 44, 1));

                       

                        // add enemy NPCs
                        currNPCs.Add(new EnemySprite(Content.Load<Texture2D>("EnemySprites//Mimic"), new Vector2(22, 41), 1, 3, 6, "Mimic", 40));
                        currNPCs.Add(new EnemySprite(Content.Load<Texture2D>("EnemySprites//Mimic"), new Vector2(1, 42), 1, 3, 6, "Mimic", 41));
                        currNPCs.Add(new EnemySprite(Content.Load<Texture2D>("EnemySprites//Mimic"), new Vector2(10, 33), 1, 3, 6, "Mimic", 42));
                        currNPCs.Add(new EnemySprite(Content.Load<Texture2D>("EnemySprites//Mimic"), new Vector2(43, 32), 1, 3, 6, "Mimic", 43));
                        currNPCs.Add(new EnemySprite(Content.Load<Texture2D>("EnemySprites//Mimic"), new Vector2(15, 25), 1, 3, 6, "Mimic", 44));
                        currNPCs.Add(new EnemySprite(Content.Load<Texture2D>("EnemySprites//Mimic"), new Vector2(12, 9), 1, 3, 6, "Mimic", 45));
                        currNPCs.Add(new EnemySprite(Content.Load<Texture2D>("EnemySprites//Mimic"), new Vector2(3, 3), 1, 3, 6, "Mimic", 46));
                        currNPCs.Add(new EnemySprite(Content.Load<Texture2D>("EnemySprites//Mimic"), new Vector2(40, 40), 1, 3, 6, "Mimic", 47));
                        currNPCs.Add(new EnemySprite(Content.Load<Texture2D>("EnemySprites//Mimic"), new Vector2(39, 8), 1, 3, 6, "Mimic", 48));
                        currNPCs.Add(new EnemySprite(Content.Load<Texture2D>("EnemySprites//Mimic"), new Vector2(25, 9), 1, 3, 6, "Mimic", 49));      // most recent enemy ** (extend enemy array)

                        // Add Doors 
                        DoorList.Add(new Door(new Vector2(41, 5), 1, new Vector2(28, 49)));      // to Vale (1)
                        DoorList.Add(new Door(new Vector2(5, 9), 15, new Vector2(31, 12)));      // to Cave Boss (15)

                        //music
                        MediaPlayer.Play(generalTheme);
                        MediaPlayer.IsRepeating = true;
                        MediaPlayer.Volume = genThemeVol;

                        //Generate Nav Grid
                        navGrid = new TileClass[50, 50]; // sets nav grid to same size as test level
                        BuildNavGrid(theCaveMain);
                        break;

                    case Level.Workshop:

                        // Load Map
                        currMap = new StaticGraphic(new Vector2(0, 0), Content.Load<Texture2D>("Maps//Workshop"));
                        currOverlay = new StaticGraphic(new Vector2(0, -240), singlePix);

                        // add extras
                        if (environStage == 3) { currNPCs.Add(new baseNPC(Content.Load<Texture2D>("proff"), new Vector2(5, 2), 1, 2, 6, "Calli")); }

                        // Add Doors 
                        DoorList.Add(new Door(new Vector2(25, 9), 2, new Vector2(5, 10)));

                        //Generate Nav Grid
                        navGrid = new TileClass[11, 11]; // sets nav grid to same size as test level
                        BuildNavGrid(WorkshopInt);

                        if (environStage == 3) { PInfo.HeroEncTrig = true; }

                        break;

                    case Level.RitualPeak:

                        // Load Map
                        switch (environStage)
                        {
                            case 1:
                                currMap = new StaticGraphic(new Vector2(-32, 0), Content.Load<Texture2D>("Maps//GaiaShrine"));
                                currOverlay = new StaticGraphic(Vector2.Zero, singlePix);
                                break;
                            case 2:
                                currMap = new StaticGraphic(new Vector2(-32, 0), Content.Load<Texture2D>("Maps//GaiaShrineWin"));
                                currOverlay = new StaticGraphic(Vector2.Zero, singlePix);
                                break;
                            case 3:
                                currMap = new StaticGraphic(new Vector2(-32, 0), Content.Load<Texture2D>("Maps//GaiaShrineDRY"));
                                currOverlay = new StaticGraphic(Vector2.Zero, singlePix);
                                break;
                        }

                        currUnderlay = new StaticGraphic(Vector2.Zero, singlePix);

                        // add extras

                        // Add Doors 
                        DoorList.Add(new Door(new Vector2(30, 2), 1, new Vector2(14, 16)));
                        if (PInfo.ShrineActive) { DoorList.Add(new Door(new Vector2(7, 14), 8, new Vector2(14, 7))); }

                        //music
                        MediaPlayer.Play(generalTheme);
                        MediaPlayer.IsRepeating = true;
                        MediaPlayer.Volume = genThemeVol;

                        //Generate Nav Grid
                        navGrid = new TileClass[30, 30]; // sets nav grid to same size as test level
                        BuildNavGrid(gaiaShrine);

                        break;

                    case Level.CPU:

                        // Load Map
                        currMap = new StaticGraphic(new Vector2(0, 0), Content.Load<Texture2D>("Maps//Space"));
                        currOverlay = new StaticGraphic(new Vector2(0, -240), singlePix);

                        // add extras
                        currNPCs.Add(new baseNPC(Content.Load<Texture2D>("Comp"), new Vector2(7, 5), 1, 2, 6, "Landru_6000"));

                        // Add Doors 


                        //Generate Nav Grid
                        navGrid = new TileClass[16, 16]; // sets nav grid to same size as test level
                        BuildNavGrid(spaceCPU);

                        break;

                    case Level.TreeHouse:

                        // Load Map
                        currMap = new StaticGraphic(new Vector2(0, 0), Content.Load<Texture2D>("Maps//TreeHouse"));
                        currOverlay = new StaticGraphic(new Vector2(0, -240), singlePix);

                        // add extras
                        currNPCs.Add(new baseNPC(Content.Load<Texture2D>("TheSeer"), new Vector2(5, 2), 1, 2, 6, "Elsa"));
                        currNPCs.Add(new baseNPC(Content.Load<Texture2D>("caulSheet"), new Vector2(4, 5), 1, 2, 6, "    Cauldron'o'Health"));
                        currNPCs.Add(new baseNPC(Content.Load<Texture2D>("caulSheet2"), new Vector2(6, 5), 1, 2, 6, "    Cauldron'o'Magick"));

                        // Add Doors 
                        DoorList.Add(new Door(new Vector2(13, 9), 1, new Vector2(5, 10)));

                        //Generate Nav Grid
                        navGrid = new TileClass[11, 11]; // sets nav grid to same size as test level
                        BuildNavGrid(treeHouseInt);

                        break;

                    case Level.Casino:

                        // Load Map
                        currMap = new StaticGraphic(new Vector2(0, 0), Content.Load<Texture2D>("Maps//Casino"));
                        currOverlay = new StaticGraphic(new Vector2(0, -240), singlePix);

                        // add extras

                        // Add Doors 
                        DoorList.Add(new Door(new Vector2(17, 26), 1, new Vector2(4, 7)));

                        //Generate Nav Grid
                        navGrid = new TileClass[9, 9]; // sets nav grid to same size as test level
                        BuildNavGrid(testInterior);

                        break;

                    case Level.Saloon:

                        // Load Map
                        currMap = new StaticGraphic(new Vector2(0, 0), Content.Load<Texture2D>("Maps//Saloon"));
                        currOverlay = new StaticGraphic(new Vector2(0, -240), singlePix);

                        // add extras
                        if (PInfo.GetCompanion != Companion.TheGunslinger) { currNPCs.Add(new baseNPC(Content.Load<Texture2D>("JuanIdle"), new Vector2(7, 4), 1, 2, 6, "Juan")); }

                        currNPCs.Add(new baseNPC(Content.Load<Texture2D>("HealthVend"), new Vector2(8, 1), 1, 2, 6, "hVend"));
                        currNPCs.Add(new baseNPC(Content.Load<Texture2D>("MagickVend"), new Vector2(9, 1), 1, 2, 6, "mVend"));

                        // Add Doors 
                        DoorList.Add(new Door(new Vector2(34, 28), 1, new Vector2(5, 10)));

                        //Generate Nav Grid
                        navGrid = new TileClass[11, 11]; // sets nav grid to same size as test level
                        BuildNavGrid(saloonInt);

                        break;

                    case Level.TownHall:

                        // Load Map
                        currMap = new StaticGraphic(new Vector2(0, 0), Content.Load<Texture2D>("Maps//TownHall"));
                        currOverlay = new StaticGraphic(new Vector2(0, -240), singlePix);

                        // add extras
                        currNPCs.Add(new baseNPC(Content.Load<Texture2D>("WitchDocOne"), new Vector2(5, 2), 1, 2, 6, "Elder Kevin"));

                        // Add Doors 
                        DoorList.Add(new Door(new Vector2(27, 37), 1, new Vector2(5, 10)));

                        //Generate Nav Grid
                        navGrid = new TileClass[11, 11]; // sets nav grid to same size as test level
                        BuildNavGrid(TownhallInt);

                        break;

                    case Level.FarmHouse:       // FARMHOUSE changed to Spell Cave

                        // Load Map
                        currMap = new StaticGraphic(new Vector2(0, 0), Content.Load<Texture2D>("Maps//CaveInt"));
                        currOverlay = new StaticGraphic(Vector2.Zero, singlePix);

                        // add extras
                        ChestList.Add(new ChestClass(new Vector2(4, 3), Content.Load<Texture2D>("Chest"), 45, 7));                                             

                        // Add Doors 
                        DoorList.Add(new Door(new Vector2(15, 15), 3, new Vector2(4, 7)));

                        //Generate Nav Grid
                        navGrid = new TileClass[9, 9]; // sets nav grid to same size as test level
                        BuildNavGrid(spellCaveInt);
                        break;

                    case Level.PondHouse:

                        break;

                    case Level.CaveBoss:

                        // Load Map
                        currMap = new StaticGraphic(Vector2.Zero, Content.Load<Texture2D>("Maps//TheCaveBoss"));
                        currOverlay = new StaticGraphic(Vector2.Zero, singlePix);
                        currUnderlay = new StaticGraphic(Vector2.Zero, singlePix);

                        // add extras
                        if (enemiesDefeated[51])
                        {
                            ChestList.Add(new ChestClass(new Vector2(4, 3), Content.Load<Texture2D>("Chest"), 53, 1));
                            ChestList.Add(new ChestClass(new Vector2(6, 3), Content.Load<Texture2D>("Chest"), 54, 9));                         //** Most Recent chest 
                        }

                        //testing linked enemy entities (SIMMONS BOSS & Tablet Fragment)
                        currNPCs.Add(new BossSprite(Content.Load<Texture2D>("EnemySprites//SimStrip"), new Vector2(4, 0), 1, 4, 3, "Simmons", 51));
                        currNPCs.Add(new EnemySprite(Content.Load<Texture2D>("Fragment"), new Vector2(5, 4), 1, 3, 3, "Simmons", 51));

                        // Add Doors 
                        DoorList.Add(new Door(new Vector2(31, 13), 5, new Vector2(5, 10)));      // to Cave (5)

                        //music
                        MediaPlayer.Play(generalTheme);
                        MediaPlayer.IsRepeating = true;
                        MediaPlayer.Volume = genThemeVol;

                        //Generate Nav Grid
                        navGrid = new TileClass[11, 11]; // sets nav grid to same size as test level
                        BuildNavGrid(theCaveBoss);
                        break;
                }

            }
            else if (currState == GameState.Battlemode)
            {
                // play battle theme
                MediaPlayer.Play(battleTheme);
                MediaPlayer.IsRepeating = true;
                MediaPlayer.Volume = battleThemeVol;

                battleBack = new StaticGraphic(Vector2.Zero, Content.Load<Texture2D>("Battlemode"));
                battleUIBox = new StaticGraphic(Vector2.Zero, Content.Load<Texture2D>("BattleUI"));
                battleTop = new StaticGraphic(Vector2.Zero, Content.Load<Texture2D>("BattleUI_top"));
                BattleShield = new StaticGraphic(Vector2.Zero, Content.Load<Texture2D>("BattleUI_Shield"));
                playerBattle = new Animated2D(Content.Load<Texture2D>("PlayerBattle"), new Vector2(3, 4), 1, 2, 3);

                // SELECT ENEMY ENCOUNTER (based on prgression stage)
                if (theWanderer.EncounterChance == 5)
                {
                    switch (environStage)
                    {
                        case 1:
                            battMan = new BattleManager(PInfo, new Elemental(earthCore, earthOut, singlePix, "Earth"));
                            break;
                        case 2:
                            battMan = new BattleManager(PInfo, new Elemental(iceCore, iceOut, singlePix, "Ice"));
                            break;
                        case 3:
                            battMan = new BattleManager(PInfo, new Elemental(flameCore, flameOut, singlePix, "Fire"));
                            break;
                    }
                }
                else
                {
                    switch (theWanderer.EncounterChance)
                    {
                        case -1:
                            battMan = new BattleManager(PInfo, new Thorn(Content.Load<Texture2D>("EnemySprites//ThornBushStrip"), singlePix));       //**Thorn
                            break;
                        case -2:
                            battMan = new BattleManager(PInfo, new Mimic(Content.Load<Texture2D>("EnemySprites//Mimic"), singlePix));                //**MIMIC
                            break;
                        case -3:
                            battMan = new BattleManager(PInfo, new Forn(Content.Load<Texture2D>("EnemySprites//TreeStrip"), singlePix));                //**FORN
                            break;
                        case -4:
                            battMan = new BattleManager(PInfo, new Simmons(Content.Load<Texture2D>("EnemySprites//SimStrip"), singlePix));                //**Simmons
                            break;
                        case -5:
                            battMan = new BattleManager(PInfo, new DrowzyBush(Content.Load<Texture2D>("EnemySprites//ThornBushStrip"), singlePix));     //**DrowzyBush
                            break;
                        case -6:
                            battMan = new BattleManager(PInfo, new Landru6000(Content.Load<Texture2D>("EnemySprites//CompBoss"), singlePix));                        //** Landru6000
                            break;
                    }
                }
            }
            else if (currState == GameState.End)
            {
                // play battle theme
                MediaPlayer.Play(eerieTheme);
                MediaPlayer.IsRepeating = false;
                MediaPlayer.Volume = 0.2f;

                //camera
                camera.Position = Vector2.Zero;

                // player
                newPlayerPos = new Vector2(7, 9);

                //stuff
                blackScreen = new StaticGraphic(Vector2.Zero, blackScreenTex);

                //dstrydLandru = true;
                if (PInfo.EndTrigger == 1)
                {
                    ScrollTextEnd = new MotionGraphic(new Vector2(0, _graphics.PreferredBackBufferHeight), Content.Load<Texture2D>("Slides//LanDefeatedEnd"));
                }
                else if (PInfo.EndTrigger == 2)
                {
                    ScrollTextEnd = new MotionGraphic(new Vector2(0, _graphics.PreferredBackBufferHeight), Content.Load<Texture2D>("Slides//LanWinsEnd"));
                }

                PInfo.EndTrigger = 0;
            }
        }

        /////////////////////////////////////////////////////////////////////////////// ** UPDATE ** ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        
        protected override void Update(GameTime gameTime)
        {
            // set up gamepad
            padOneCurr = GamePad.GetState(PlayerIndex.One);


            if (currState == GameState.StartScreen)
            {
                camera.Position = Vector2.Zero;

                // load Content
                if (!contentLoaded)
                {
                    reLoadContent();
                    contentLoaded = true;
                }

                if (titleCount > 0)
                {
                    if (titleReps < 7)
                    {
                        titleCount -= (float)gameTime.ElapsedGameTime.TotalSeconds;
                    }
                    else 
                    { 
                        titleDone = true; 
                    }
                }
                else
                {
                    dramaHorn.Play(0.3f, -0.7f, 0);
                    OpeningSlides.Add(new FadingGraphic(Vector2.Zero, Content.Load<Texture2D>("Slides//" + titleReps), 4));
                    titleReps++;
                    titleCount = 4.3f;
                }

                OpeningSlides.ForEach(os => os.updateme(gameTime));

                if (titleDone)
                {
                    if (titleTrackCount > 0) 
                    { 
                        titleTrackCount -= (float)gameTime.ElapsedGameTime.TotalSeconds;
                    }
                    else if (!titleTrackTrigd) 
                    { 
                        MediaPlayer.Play(eerieTheme);
                        MediaPlayer.Volume = 0.2f;
                        MediaPlayer.IsRepeating = true;
                        titleTrackTrigd = true;
                    }

                    sMenu.updateMe(padOneCurr, padOneOld, uiMovCurs);
                }

                if (padOneCurr.Buttons.A == ButtonState.Pressed && padOneOld.Buttons.A == ButtonState.Released && titleDone && !sMenu.IsTutorialUp)
                {
                    switch (sMenu.chooseMe())
                    {
                        case 0:                     // How To Play
                            sMenu.IsTutorialUp = true;
                            break;
                        case 1:                     // New Game
                            if (gameInProgress)
                            {
                                Initialize();
                                LoadContent();
                                currState = GameState.GameWorld;
                                contentLoaded = false;
                            }
                            else
                            {
                                currState = GameState.GameWorld;
                                contentLoaded = false;
                            }
                            break;
                        case 2:                     // Resume
                            if (gameInProgress)
                            {
                                currState = GameState.GameWorld;
                                contentLoaded = false;
                            }
                            break;
                        case 3:                     // Quit
                            Exit();
                            break;
                    }
                }
                // remove spent slides
                OpeningSlides.RemoveAll(os => os.Timer < 0);

            }
            else if (currState == GameState.GameWorld)
            {
                #region NON LEVEL SPECIFIC UPDATE

                #region load Content
                if (!contentLoaded)
                {
                    reLoadContent();                              // load content for gameworld
                    theWanderer.m_safeSteps = SAFESTEPS;        // apply player safesteps
                    contentLoaded = true;
                }

                if (diMan.IsIdle && diMan.ForceLoad)
                {
                    diMan.ForceLoad = false;
                    newPlayerPos = theWanderer.Destination;
                    reLoadContent();
                    contentLoaded = true;
                }

                if (!gameInProgress)
                {
                    gameInProgress = true;
                }
                #endregion

                #region Back to Title
                if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                {
                    currState = GameState.StartScreen;
                    newPlayerPos = theWanderer.Destination;
                    contentLoaded = false;
                    //Exit();
                }
                #endregion

                #region update calls
                if (diMan.IsIdle)
                {
                    if (!pMenu.IsPaused)
                    {
                        if (titleScrolled)
                        {
                            theWanderer.updateme(gameTime, navGrid, padOneCurr, padOneOld);
                        }
                    }
                    else
                    {
                        pMenu.updateMe(padOneCurr, padOneOld, -camera.Position, uiMovCurs);
                    }
                }
                else
                {
                    diMan.updateMe(padOneCurr, padOneOld, uiMovCurs);
                }
                #endregion

                #region Toggle NavGrid DEBUG TOOL
                //if (padOneCurr.Buttons.RightShoulder == ButtonState.Pressed)
                //{
                //    showNavGrid = true;
                //}
                //else
                //{
                //    showNavGrid = false;
                //}
                #endregion

                #region Toggle Pause Menu
                if (padOneCurr.Buttons.Start == ButtonState.Pressed && padOneOld.Buttons.Start == ButtonState.Released && diMan.IsIdle)
                {
                    pMenu.IsPaused = true;
                }
                #endregion

                #region Trigger Battlemode
                if ((theWanderer.EncounterChance == 5 && !noBattles))         // added cheat to turn rng battles off      // || theWanderer.EncounterChance < 0 ** Dont think its needed
                {
                    newPlayerPos = theWanderer.Destination;
                    currState = GameState.Battlemode;
                    contentLoaded = false;
                    encounterFX.Play(encBlipVol, 0, 0);
                }
                else if(diMan.IsIdle && PInfo.TriggerLandru)
                {
                    theWanderer.EncounterChance = -6;
                    newPlayerPos = theWanderer.Destination;
                    currState = GameState.Battlemode;
                    contentLoaded = false;
                    PInfo.TriggerLandru = false;
                    encounterFX.Play(encBlipVol, 0, 0);
                }
                #endregion

                #region Trigger Dialog
                if (theWanderer.TalkTo == "Legendary Hero" && diMan.IsIdle)
                {
                    if (firstHeroEnc) { TriggerHeroDia(); }
                }
                else if (theWanderer.TalkTo != "unknown" && diMan.IsIdle)
                {
                    diMan = new DialogManager(PInfo, theWanderer.TalkTo, portraits, -camera.Position, vendFX, potionDrink);
                    diMan.IsIdle = false;
                    theWanderer.TalkTo = "unknown";
                }
                #endregion

                #region GetNonChestBasedNotifications
                if(!danceLearned && PInfo.GetSpellKnown(2))
                {
                    NotificationsList.Add(new Notifications(notificationBox, 10));
                    danceLearned = true;
                }
                #endregion

                #region open chests
                if (theWanderer.TargetChest != -1)
                {
                    ChestList.ForEach(c =>
                    {
                        if (c.ID == theWanderer.TargetChest && !chestsOpen[c.ID])
                        {
                            switch (c.Contents)
                            {
                                case 1:
                                    PInfo.HealthPotion++;
                                    NotificationsList.Add(new Notifications(notificationBox, c.Contents));
                                    break;
                                case 2:
                                    PInfo.MagickPotion++;
                                    NotificationsList.Add(new Notifications(notificationBox, c.Contents));
                                    break;
                                case 3:
                                    PInfo.Coins += 25;
                                    NotificationsList.Add(new Notifications(notificationBox, c.Contents));
                                    break;
                                case 4:
                                    PInfo.SpiritOrbs++;
                                    NotificationsList.Add(new Notifications(notificationBox, c.Contents));
                                    break;
                                case 5:
                                    PInfo.GotStaff = true;
                                    NotificationsList.Add(new Notifications(notificationBox, c.Contents));
                                    break;
                                case 6:
                                    PInfo.GotShoes = true;
                                    NotificationsList.Add(new Notifications(notificationBox, c.Contents));
                                    break;
                                case 7:
                                    PInfo.LearnSpell(3);
                                    NotificationsList.Add(new Notifications(notificationBox, c.Contents));
                                    break;
                                case 8:
                                    PInfo.LearnSpell(4);
                                    NotificationsList.Add(new Notifications(notificationBox, c.Contents));
                                    break;
                                case 9:
                                    PInfo.LearnSpell(5);
                                    NotificationsList.Add(new Notifications(notificationBox, c.Contents));
                                    break;
                                default:
                                    PInfo.Coins += 10;
                                    NotificationsList.Add(new Notifications(notificationBox, 1));
                                    break;
                            }
                            c.IsOpen = true;
                            chestsOpen[c.ID] = true;
                            theWanderer.TargetChest = -1;
                            chestOpenFX.Play(0.9f, 0, 0);
                        }
                    });
                }
                #endregion

                #region activate enemies
                if (theWanderer.TargetEnemy != -1)
                {
                    currNPCs.ForEach(npc =>
                    {
                        if (npc.ID == theWanderer.TargetEnemy)
                        {
                            if (!npc.IsActive)
                            {
                                npc.IsActive = true;
                                encounterFX.Play(encBlipVol, 0, 0);
                            }
                            
                            if ((padOneCurr.Buttons.A == ButtonState.Pressed && padOneOld.Buttons.A == ButtonState.Released || padOneCurr.Buttons.B == ButtonState.Pressed && padOneOld.Buttons.B == ButtonState.Released) && diMan.CanProceed)
                            {
                                switch (npc.CharName)
                                {
                                    case "Thorn":
                                        theWanderer.EncounterChance = -1;
                                        break;
                                    case "Mimic":
                                        theWanderer.EncounterChance = -2;
                                        break;
                                    case "Forn":
                                        theWanderer.EncounterChance = -3;
                                        break;
                                    case "Simmons":
                                        theWanderer.EncounterChance = -4;
                                        break;
                                    case "DrowzyBush":
                                        theWanderer.EncounterChance = -5;
                                        break;
                                }

                                if (firstHeroEnc)
                                {
                                    theWanderer.EncounterChance = 0;
                                }
                                else
                                {
                                    newPlayerPos = theWanderer.Destination;
                                    currState = GameState.Battlemode;
                                    contentLoaded = false;
                                    theWanderer.TargetEnemy = -1;
                                    enemiesDefeated[npc.ID] = true;
                                }
                            }
                        }
                    });
                }
                #endregion

                #region Use Doors
                DoorList.ForEach(d => 
                {
                    if (theWanderer.Destination == d.DoorLocation)
                    {
                        useDoor(d.NewPlayerPos, d.DoorDestination);
                    }
                });
                #endregion

                // remove expired notifications
                NotificationsList.RemoveAll(n => n.Timer <= 0);

                #endregion

                #region Level Specific & Camera Rules

                if (currLevel == Level.Clearing)
                {

                    
                    if (!titleScrolled && camera.Position.Y > -840)
                    {
                        camera.Position.Y -= 0.1f * (float)gameTime.ElapsedGameTime.TotalMilliseconds;
                    }
                    else
                    {
                        camera.Position = new Vector2(0, -840);
                        titleScrolled = true;
                    }
                }
                else if (currLevel == Level.Vale || currLevel == Level.OldForest || currLevel == Level.Cave)
                {
                    #region update camera position
                    Vector2 tempCamPos;
                    
                    if (theWanderer.Position.X > 35)
                    {
                        tempCamPos.X = 35;
                    }
                    else if (theWanderer.Position.X < 15)
                    {
                        tempCamPos.X = 15;
                    }
                    else
                    {
                        tempCamPos.X = theWanderer.Position.X;
                    }

                    if (theWanderer.Position.Y > 41)
                    {
                        tempCamPos.Y = 41;
                    }
                    else if (theWanderer.Position.Y < 8)
                    {
                        tempCamPos.Y = 8;
                    }
                    else
                    {
                        tempCamPos.Y = theWanderer.Position.Y;
                    }

                    camera.Position = new Vector2((-tempCamPos.X * TILESIZE) + _graphics.PreferredBackBufferWidth / 2, (-tempCamPos.Y * TILESIZE) + (_graphics.PreferredBackBufferHeight / 2) - 28);
                    #endregion

                }
                else if (currLevel == Level.EastRoad || currLevel == Level.WestRoad)
                {
                    #region update camera position
                    float tempCamPosY;

                    if (theWanderer.Position.Y > 21)
                    {
                        tempCamPosY = 21;
                    }
                    else if (theWanderer.Position.Y < 8)
                    {
                        tempCamPosY = 8;
                    }
                    else
                    {
                        tempCamPosY = theWanderer.Position.Y;
                    }

                    camera.Position = new Vector2(0, (-tempCamPosY * TILESIZE) + (_graphics.PreferredBackBufferHeight / 2) - 28);
                    #endregion
                }
                else if (currLevel == Level.RitualPeak)
                {
                    camera.Position = new Vector2(32, 0);
                }
                else
                {
                    // update camera position
                    camera.Position = new Vector2((-theWanderer.Position.X * TILESIZE) + _graphics.PreferredBackBufferWidth / 2, (-theWanderer.Position.Y * TILESIZE) + (_graphics.PreferredBackBufferHeight / 2) - 28);
                }

                #endregion

                #region NOT LEVEL SPECIFIC (post camera updates)

                #region Trig End
                if (PInfo.EndTrigger == 2)
                {
                    currState = GameState.End;
                    contentLoaded = false;
                }
                #endregion

                #region Fade Ins
                OpeningSlides.ForEach(os => os.updateWithPos(gameTime, -camera.Position));
                OpeningSlides.RemoveAll(os => os.Timer < 0);
                #endregion

                if (PInfo.HeroEncTrig)
                {
                    TriggerHeroDia();
                    PInfo.HeroEncTrig = false;
                }

                if (PInfo.JustDied && diMan.IsIdle)
                {
                    diMan = new DialogManager(PInfo, "Elsa", portraits, -camera.Position, vendFX, potionDrink);
                    diMan.IsIdle = false;
                }

                if (getRandLoot)
                {
                    RollForLoot();
                }

                #endregion

            }
            else if (currState == GameState.Battlemode)
            {
                // load Content
                if (!contentLoaded)
                {
                    reLoadContent();
                    contentLoaded = true;
                }

                // update POWER (NERWEN PASSIVE)
                if (PInfo.GetCompanion == Companion.TheWitchdoctor)
                {
                    if (PInfo.HitPoints < PInfo.MaxHP / 3)
                    {
                        PInfo.Power = 10;
                    }
                    else if (PInfo.HitPoints < (PInfo.MaxHP / 3) * 2) 
                    {
                        PInfo.Power = 5;
                    }
                    else { PInfo.Power = 0; }
                }
                else { PInfo.Power = 0; }
                
                // update camera position
                camera.Position = Vector2.Zero;
                
                // cycle through battle
                if (!battMan.BattleOver)
                {
                    // check for battle end
                    if (PInfo.HitPoints <= 0)                                                  //
                    {                                                                          //   text refresh ** 
                        battMan.m_battNotification = "YOU DEAD!";                              //
                        battMan.BattleOver = true;                                             //   FIND A BETTER WAY TO DO THIS !! NO STRING COMPARISONS
                        if (battMan.CurrEnemyName == "Forn")
                        {
                            enemiesDefeated[50] = false;
                            canAdv = false;
                        }
                        else if (battMan.CurrEnemyName == "Simmons")
                        {
                            enemiesDefeated[51] = false;
                            canAdv = false;
                        }
                        else if (battMan.CurrEnemyName == "Landru_6000")
                        {
                            canAdv = false;
                        }
                    }                                                        
                    else if (battMan.EnemyHP <= 0)                           
                    {   
                        battMan.m_battNotification = "Enemy Defeated!";
                        battMan.BattleOver = true;
                        getRandLoot = true;
                        //if (battMan.CurrEnemyName == "Forn" || battMan.CurrEnemyName == "Simmons") { AdvanceEnvironStage(); }
                    }    

                    if (battMan.CurrBattleStage == BattleStage.PreRound)
                    {
                        battMan.preRound(padOneCurr, padOneOld);
                    }
                    else if (battMan.CurrBattleStage == BattleStage.PlayerTurn)
                    {
                        battMan.playerTurn(padOneCurr, padOneOld, uiMovCurs);
                    }
                    else if (battMan.CurrBattleStage == BattleStage.EnemyTurn)
                    {
                        battMan.enemyTurn(padOneCurr, padOneOld);
                    }
                    else if (battMan.CurrBattleStage == BattleStage.PostRound)
                    {
                        battMan.postRound(padOneCurr, padOneOld);
                    }
                }
                else 
                {
                    if (padOneCurr.Buttons.A == ButtonState.Pressed && padOneOld.Buttons.A == ButtonState.Released)
                    {
                        if ((battMan.CurrEnemyName == "Forn" || battMan.CurrEnemyName == "Simmons") && canAdv) { AdvanceEnvironStage(); }
                        if (PInfo.HitPoints <= 0 && deathReset)
                        {
                            currLevel = Level.TreeHouse;
                            newPlayerPos = deathResetPos;
                            PInfo.HitPoints = 2;
                            PInfo.JustDied = true;
                        }

                        if (battMan.CurrEnemyName == "Landru_6000" && canAdv)
                        {
                            PInfo.EndTrigger = 1;
                            currState = GameState.End;
                            contentLoaded = false;
                            PInfo.Defence = 1;
                            canAdv = true;
                        }
                        else
                        {
                            currState = GameState.GameWorld;
                            contentLoaded = false;
                            PInfo.Defence = 1;
                            canAdv = true;
                        }
                    }
                }
            }
            else if (currState == GameState.End)
            {
                // load Content
                if (!contentLoaded)
                {
                    reLoadContent();
                    contentLoaded = true;
                }

                if (ScrollTextEnd.Position.Y > 0)
                {
                    ScrollTextEnd.updateme(new Vector2(0, -0.5f));
                }
                else
                {
                    if (padOneCurr.Buttons.A == ButtonState.Pressed && padOneOld.Buttons.A == ButtonState.Released)
                    {
                        currState = GameState.StartScreen;
                        gameInProgress = false;
                        contentLoaded = false;
                    }
                }

            }

            // store old gamepad state for edge detection
            padOneOld = padOneCurr;

            base.Update(gameTime);
        }

        /////////////////////////////////////////////////////////////////////////////// ** DRAW ** ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);

            _spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null, null, camera.getCam());

            if (currState == GameState.StartScreen && contentLoaded)
            {
                strtScreen.drawme(_spriteBatch);
                if (!titleDone)
                { 
                    blackScreen.drawme(_spriteBatch); 
                }
                else
                {
                    sMenu.drawMe(_spriteBatch, gameInProgress);
                }
                OpeningSlides.ForEach(os => os.drawme(_spriteBatch));
                //_spriteBatch.DrawString(uiFontOne, "" + titleCount, Vector2.Zero, Color.White);
            }
            else if (currState == GameState.GameWorld && contentLoaded)
            {
                currUnderlay.drawme(_spriteBatch);

                if (environStage == 1)
                {
                    WaterTiles.ForEach(w => w.drawme(_spriteBatch, gameTime));
                }
                else if (environStage == 2)
                {
                    // draw ice
                }
                
                currMap.drawme(_spriteBatch);
                RocksList.ForEach(r => r.drawRock(_spriteBatch));
                ChestList.ForEach(c => c.drawme(_spriteBatch));

                theWanderer.drawme(_spriteBatch, gameTime);

                aniItemList.ForEach(i => i.drawmeHigher(_spriteBatch, gameTime));
                currOverlay.drawme(_spriteBatch);

                currNPCs.ForEach(npc => npc.drawme(_spriteBatch, gameTime));

                NotificationsList.ForEach(not => not.drawMe(_spriteBatch, gameTime, -camera.Position));

                // Dialog UI
                if (!diMan.IsIdle)
                {
                    diMan.drawMe(_spriteBatch, gameTime, dialogUI, dialogUICover, cursorTex, dialogBlip);
                }
                if (pMenu.IsPaused)
                {
                    pMenu.drawMe(_spriteBatch);
                }
                OpeningSlides.ForEach(os => os.drawme(_spriteBatch));
            }
            else if (currState == GameState.Battlemode && contentLoaded)
            {
                battleBack.drawme(_spriteBatch);
                playerBattle.drawme(_spriteBatch, gameTime);
                battleUIBox.drawme(_spriteBatch);
                battMan.drawMe(_spriteBatch, cursorTex, battleUICover, gameTime, dialogBlip);
                battleTop.drawme(_spriteBatch);
                if(PInfo.Defence > 1)
                {
                    BattleShield.drawme(_spriteBatch);
                }
            }
            else if (currState == GameState.End && contentLoaded)
            {
                blackScreen.drawme(_spriteBatch);
                ScrollTextEnd.drawme(_spriteBatch);
                if (ScrollTextEnd.Position.Y <= 0)
                {
                    _spriteBatch.DrawString(uiFontOne, "** PRESS A **", new Vector2(((_graphics.PreferredBackBufferWidth / 2)) - (uiFontOne.MeasureString("**PRESS A **").X / 2), _graphics.PreferredBackBufferHeight - 50), Color.LightPink);
                }
            }

            #region DEBUG INFO

            //_spriteBatch.DrawString(uiFontOne, " " + gameInProgress, -camera.Position * Vector2.One, Color.White);
            if (showNavGrid)
            {
                for (int i = 0; i < navGrid.GetLength(0); i++)
                {
                    for (int j = 0; j < navGrid.GetLength(1); j++)
                    {
                        navGrid[i, j].drawme(_spriteBatch);
                    }
                }
            }
            //_spriteBatch.DrawString(debugFont, " " + theWanderer.EncounterChance, new Vector2(theWanderer.Position.X * 64, (theWanderer.Position.Y * 64) - 20), Color.White);
            //_spriteBatch.DrawString(debugFont, " " + theWanderer.m_safeSteps , new Vector2((theWanderer.Position.X * 64) + 50, (theWanderer.Position.Y * 64) - 20), Color.White);
            //if (theWanderer.EncounterChance == 5) { _spriteBatch.DrawString(debugFont, "Battle Time", new Vector2(theWanderer.Position.X * 64, (theWanderer.Position.Y * 64) - 50), Color.White); }
            //if (theWanderer.TalkTo != "unknown") { _spriteBatch.DrawString(uiFontOne, "Hello " + theWanderer.TalkTo , new Vector2(theWanderer.Position.X * 64, (theWanderer.Position.Y * 64) - 50), Color.White); }

            #endregion

            _spriteBatch.End();


            base.Draw(gameTime);
        }
    }
}
