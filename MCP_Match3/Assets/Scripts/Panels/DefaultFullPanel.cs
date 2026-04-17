using Match3.Data;

namespace Match3.Panels
{
    /// <summary>
    /// A normal, fully-playable cell. Items can be placed, matched, and dropped through it.
    /// This is the default panel type for every active cell on the board.
    /// </summary>
    public class DefaultFullPanel : Panel
    {
        private void Awake()
        {
            m_PanelType = PanelType.Default_Full;
            Defence     = 0;
        }

        // DefaultFull is indestructible by normal bursts — it is the cell itself.
        public override bool Brust() => false;
    }
}
