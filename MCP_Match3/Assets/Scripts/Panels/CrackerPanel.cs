using Match3.Data;

namespace Match3.Panels
{
    /// <summary>
    /// Cracker obstacle. Requires 2 hits from adjacent matches.
    /// Each hit removes one visual layer. Counts toward cracker missions.
    /// </summary>
    public class CrackerPanel : Panel
    {
        private void Awake()
        {
            m_PanelType = PanelType.Cracker;
            Defence = 2;
        }

        protected override void OnDestroyed()
        {
            // TODO Phase 9: notify MissionManager
        }
    }
}
