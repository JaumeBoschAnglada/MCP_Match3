using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Match3.Data
{
    /// <summary>
    /// Serializable level data loaded from JSON. Defines everything needed to build a game session:
    /// board shape, gravity, initial pieces, panels, missions, spawn lines, and special item parameters.
    /// </summary>
    [Serializable]
    public class Stage
    {
        // === BOARD (81 cells = 9x9) ===

        /// <summary>Gravity direction(s) per cell. 81 elements, each an array of DROP_DIR.</summary>
        [JsonProperty("DropDirs")]
        public List<DROP_DIR[]> DropDirs;

        /// <summary>Whether this level uses custom gravity. If false, CreateGravity() fills all "U".</summary>
        public bool isUseGravity;

        /// <summary>Panel data per cell. Defines board shape + obstacles. 81 elements.</summary>
        public Pannels[] panels;

        /// <summary>Initial item type per cell. 81 elements.</summary>
        [JsonProperty("items", ItemConverterType = typeof(StringEnumConverter))]
        public ItemType[] items;

        /// <summary>Initial color per cell. 81 elements.</summary>
        [JsonProperty("colors", ItemConverterType = typeof(StringEnumConverter))]
        public ColorType[] colors;

        /// <summary>Cell indices for initial focus visual effect.</summary>
        public List<int> focus;

        // === SPAWN LINES (9 bools per axis) ===

        public bool[] defaultSpawnLine;
        public bool[] defaultSpawnLineY;
        public bool[] foodSpawnLine;
        public bool[] foodSpawnLineY;
        public bool[] SpiralSpawnLine;
        public bool[] SpiralSpawnLineY;
        public bool[] DonutSpawnLine;
        public bool[] DonutSpawnLineY;
        public bool[] TimeBombSpawnLine;
        public bool[] TimeBombSpawnLineY;
        public bool[] MysterySpawnLine;
        public bool[] MysterySpawnLineY;
        public bool[] ChameleonSpawnLine;
        public bool[] ChameleonSpawnLineY;
        public bool[] KeySpawnLine;
        public bool[] KeySpawnLineY;

        // === COLORS AND SCORE ===

        /// <summary>6 bools: RED, YELLOW, GREEN, BLUE, PURPLE, ORANGE.</summary>
        public bool[] appearColor;

        public long scoreStar1;
        public long scoreStar2;
        public long scoreStar3;
        public int limit_Move;

        // === MISSIONS ===

        [JsonConverter(typeof(StringEnumConverter))]
        public MissionType missionType;

        public bool isOrderNMission;
        public bool isOrderSMission;
        public bool isWaferMission;
        public bool isFoodMission;
        public bool isBearMission;
        public bool isIceCreamMission;
        public bool isJamMission;
        public bool isSteleMission;
        public bool isScoreMission;
        public List<MissionData> missionInfo;

        // === SPECIAL ITEM SPAWN PARAMETERS ===

        public int Spiral_MinExist;
        public int Spiral_MaxExist;
        public int Spiral_Interval;
        public int Spiral_SpawnCnt;

        public int Donut_MinExist;
        public int Donut_MaxExist;
        public int Donut_Interval;
        public int Donut_SpawnCnt;

        public int TimeBomb_MinExist;
        public int TimeBomb_MaxExist;
        public int TimeBomb_Interval;
        public int TimeBomb_SpawnCnt;
        public int TimeBomb_FirstCount = 15;

        public int Mystery_MinExist;
        public int Mystery_MaxExist;
        public int Mystery_Interval;
        public int Mystery_SpawnCnt;

        [JsonConverter(typeof(StringEnumConverter))]
        public MysterySettingType Mystery_SettingType;

        public int Chameleon_MinExist;
        public int Chameleon_MaxExist;
        public int Chameleon_Interval;
        public int Chameleon_SpawnCnt;

        public int Key_MinExist;
        public int Key_MaxExist;
        public int Key_Interval;
        public int Key_SpawnCnt;

        public int food_MaxExist;
        public int food_Interval;
        public int food_SpawnCnt;

        public int Bear_MaxExist;
        public int Bear_Interval;

        public int IceCream_Interval;
        public int IceCreamCreator_Interval;

        [JsonConverter(typeof(StringEnumConverter))]
        public S_TreeSettingType S_Tree_SettingType;

        [JsonProperty("JewelTreeItem", ItemConverterType = typeof(StringEnumConverter))]
        public ItemType[] JewelTreeItem;

        [JsonProperty("JewelTreeItemColor", ItemConverterType = typeof(StringEnumConverter))]
        public ColorType[] JewelTreeItemColor;

        /// <summary>
        /// If isUseGravity is false, generates a default DropDirs array with all cells set to U.
        /// </summary>
        public void CreateGravity()
        {
            if (DropDirs == null)
                DropDirs = new List<DROP_DIR[]>();

            DropDirs.Clear();
            for (int i = 0; i < 81; i++)
            {
                DropDirs.Add(new[] { DROP_DIR.U });
            }
        }
    }

    /// <summary>
    /// Panel stack for a single cell. Contains one or more PanelData entries.
    /// </summary>
    [Serializable]
    public class Pannels
    {
        public List<PanelData> listinfo;
    }

    /// <summary>
    /// Data for a single panel on a cell.
    /// </summary>
    [Serializable]
    public class PanelData
    {
        [JsonConverter(typeof(StringEnumConverter))]
        public PanelType paneltype;

        /// <summary>Hits to destroy. -1 = indestructible/not applicable.</summary>
        public int defence = -1;

        /// <summary>Additional configurable value per panel type.</summary>
        public int value;

        /// <summary>Extra JSON data (conveyer belt config, warp target index, etc.).</summary>
        public string addData;
    }

    /// <summary>
    /// A single mission objective.
    /// </summary>
    [Serializable]
    public class MissionData
    {
        [JsonConverter(typeof(StringEnumConverter))]
        public MissionType type;

        [JsonConverter(typeof(StringEnumConverter))]
        public MissionKind kind;

        /// <summary>Target count. 0 = destroy all currently on the board.</summary>
        public int count;
    }
}
