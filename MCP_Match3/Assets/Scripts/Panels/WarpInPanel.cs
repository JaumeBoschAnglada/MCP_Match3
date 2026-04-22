using Match3.Core;
using Match3.Data;

namespace Match3.Panels
{
    /// <summary>
    /// Warp entrance panel (Fase 8).
    /// Items that reach the top of this cell's gravity chain are redirected to exit
    /// from the paired WarpOutPanel instead of spawning locally.
    ///
    /// Pairing: WarpIn and WarpOut panels with matching `value` fields are linked by MatchManager.PanelSetting()
    /// WarpIn.m_Board.m_WarpBoard = WarpOut.m_Board — set automatically based on matching value.
    /// </summary>
    public class WarpInPanel : Panel
    {
        private void Awake()
        {
            m_PanelType = PanelType.Warp_In;
            Defence = 0;
        }

        public override void OnPlaced()
        {
            if (m_Board != null)
                m_Board.m_IsWarpInBoard = true;
        }

        public override bool Brust() => false;
    }
}
