using Match3.Core;
using Match3.Data;

namespace Match3.Panels
{
    /// <summary>
    /// Warp exit panel (Fase 8).
    /// Pieces that enter the paired WarpInPanel are redirected and appear here.
    ///
    /// Pairing: WarpIn and WarpOut panels with matching `value` fields are linked by MatchManager.PanelSetting()
    /// WarpOut.m_Board.m_WarpBoard = WarpIn.m_Board — set automatically based on matching value.
    /// </summary>
    public class WarpOutPanel : Panel
    {
        private void Awake()
        {
            m_PanelType = PanelType.Warp_Out;
            Defence = 0;
        }

        public override void OnPlaced()
        {
            if (m_Board != null)
                m_Board.m_IsWarpOutBoard = true;
        }

        public override bool Brust() => false;
    }
}
