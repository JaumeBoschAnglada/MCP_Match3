using Match3.Data;

namespace Match3.Panels
{
    /// <summary>
    /// Bread obstacle. Destructible by an adjacent match (not direct — no item on this cell).
    /// Destroyed in one hit. Counts toward bread missions.
    /// </summary>
    public class BreadPanel : Panel
    {
        public override int VisualSortingOrder => OverlaySortingOrder;

        private void Awake()
        {
            m_PanelType = PanelType.Bread_Block;
            Defence = 1;
        }

        protected override void OnDestroyed()
        {
            // TODO Phase 9: notify MissionManager (bread mission)
        }
    }
}
