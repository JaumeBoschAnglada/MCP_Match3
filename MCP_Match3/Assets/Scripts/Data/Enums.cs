namespace Match3.Data
{
    public enum ColorType
    {
        None = 0,
        RED = 1,
        YELLOW = 2,
        GREEN = 3,
        BLUE = 4,
        PURPLE = 5,
        ORANGE = 6,
        Rnd = 7
    }

    public enum ItemType
    {
        None = -1,
        Normal = 0,
        Butterfly = 1,
        Line_X = 2,
        Line_Y = 3,
        Line_C = 4,
        Bomb = 5,
        Rainbow = 6,
        CondItem_Last = 7,
        Donut = 20,
        Spiral = 21,
        JellyBear = 22,
        TimeBomb = 23,
        Mystery = 30,
        Chameleon = 31,
        JellyMon = 35,
        Ghost = 36,
        Key = 40,
        BonusCross = 90,
        BonusBomb = 91,
        Misson_Food1 = 100,
        Misson_Food2 = 101,
        Misson_Food3 = 102,
        Misson_Food4 = 103,
        Misson_Food5 = 104,
        Misson_Food6 = 105
    }

    public enum PanelType
    {
        Default_Full = 100,
        Default_Empty = 101,
        Creator_Empty = 90,
        Fixed_Block = 19,
        Bread_Block = 20,
        IceCream_Creator = 21,
        IceCream_Block = 22,
        ConveyerBelt = 23,
        Cake_A = 24,
        Cake_B = 25,
        Cake_C = 26,
        Cake_D = 27,
        Cracker = 28,
        S_Tree = 29,
        MagicColor = 30,
        Ring = 31,
        JewelTree_A = 32,
        JewelTree_B = 33,
        JewelTree_C = 34,
        JewelTree_D = 35,
        Jam = 79,
        Wafer_floor = 80,
        Stele_Hide = 81,
        Stele = 82,
        Lolly_Cage = -10,
        Ice_Cage = -11,
        Bottle_Cage = -12,
        Creator_Food = -20,
        Creator_Spiral = -21,
        Creator_TimeBomb = -22,
        Creator_Key = -23,
        Creator_Food_Spiral = -24,
        Creator_Food_TimeBomb = -25,
        Creator_Spiral_TimeBomb = -26,
        Creator_Key_Food = -27,
        Creator_Key_TimeBomb = -28,
        Warp_In = -30,
        Warp_Out = -31,
        FoodArrive = -32,
        JellyBearStart = -33
    }

    public enum StepType
    {
        Wait,
        Matching,
        TimeBomb,
        IceCream,
        ConveyerBelt,
        Chameleon,
        MagicColor,
        BearJump,
        BearSpawn,
        Mission,
        Clear,
        Fail,
        Shuffling
    }

    public enum MatchState
    {
        PrepareGame,
        Playing,
        Shuffling,
        BonusTime,
        GameClear,
        GameFail
    }

    public enum TouchState
    {
        Switching,
        CashItemUse,
        JellyMonDrop
    }

    /// <summary>
    /// Direction from which pieces fall into a cell.
    /// U = from above (standard), D = from below, L = from left, R = from right.
    /// List = marks cell as a drop start point.
    /// </summary>
    public enum DROP_DIR
    {
        U,
        D,
        L,
        R,
        List
    }

    /// <summary>
    /// 8 directions for cell adjacency (used in match detection and navigation).
    /// </summary>
    public enum SQR_DIR
    {
        NONE,
        TOP,
        BOTTOM,
        LEFT,
        RIGHT,
        TOPRIGHT,
        TOPLEFT,
        BOTTOMLEFT,
        BOTTOMRIGHT
    }

    public enum MissionType
    {
        OrderN,
        OrderS,
        Wafer,
        Food,
        Bear,
        IceCream,
        Jam,
        Stele,
        Score
    }

    public enum MissionKind
    {
        None,
        Red,
        Yellow,
        Green,
        Blue,
        Purple,
        Orange,
        Donut,
        Spiral,
        Bread,
        LollyCage,
        IceCage,
        Cracker,
        Bottle,
        S_Tree,
        Line,
        Line_Line,
        Cross,
        Cross_Line,
        Cross_Cross,
        Bomb,
        Bomb_Line,
        Bomb_Cross,
        Bomb_Bomb,
        Rainbow,
        Rainbow_Line,
        Rainbow_Cross,
        Rainbow_Bomb,
        Rainbow_Rainbow,
        TimeBomb,
        Wafer,
        StrawberryCake,
        ChocolatePiece,
        MintCake,
        Parfait,
        WhiteCake,
        Hamburger,
        Bear,
        IceCream,
        Jam,
        Stele,
        Score
    }

    public enum FailType
    {
        None,
        Limit_Move,
        TimeBombOver,
        ShufflingOver
    }

    public enum MysterySettingType
    {
        Mystery_Basic,
        Mystery_Easy,
        Mystery_Normal,
        Mystery_Hard,
        Mystery_Candy,
        Mystery_Special,
        Mystery_Icecream,
        Mystery_IceCreamCreator,
        Mystery_Bear,
        Mystery_Last_Test
    }

    public enum S_TreeSettingType
    {
        Basic,
        Easy,
        Normal,
        Hard,
        Last_Test
    }
}
