using Match3.Data;

namespace Match3.Panels
{
    /// <summary>
    /// Wafer floor panel. Sits beneath normal items and is destroyed
    /// when a match occurs directly on top of this cell.
    /// Counts toward wafer floor missions.
    /// </summary>
    public class WaferFloorPanel : Panel
    {
        private void Awake()
        {
            m_PanelType = PanelType.Wafer_floor;
            Defence = 1;
        }

        public override void ItemMatch()
        {
            // Destroyed when the item on this cell is matched
            Brust();
        }

        protected override void OnDestroyed()
        {
            // TODO Phase 9: notify MissionManager
        }
    }
}
