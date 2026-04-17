using Match3.Data;

namespace Match3.Panels
{
    /// <summary>
    /// An indestructible fixed block. Items cannot move through or be placed on it.
    /// Gravity skips this cell entirely.
    /// </summary>
    public class FixedPanel : Panel
    {
        public override int VisualSortingOrder => OverlaySortingOrder;
        public override bool BlocksItemSwitch => true;
        public override bool BlocksGravity => true;

        private void Awake()
        {
            m_PanelType = PanelType.Fixed_Block;
            Defence     = 0;
        }

        public override bool Brust()       => false; // indestructible
        public override void ItemDrop()    { }       // gravity does not pass through
        public override void ItemSwitch()  { }       // cannot be swapped
    }
}
