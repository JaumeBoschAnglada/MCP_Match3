using Match3.Data;

namespace Match3.Panels
{
    /// <summary>
    /// An inactive / non-playable cell. Items cannot be placed or dropped here.
    /// The Board.IsActiveCell flag is set to false when this panel is present.
    /// </summary>
    public class DefaultEmptyPanel : Panel
    {
        private void Awake()
        {
            m_PanelType = PanelType.Default_Empty;
            Defence     = 0;
        }

        // Empty panels do nothing on any event.
        public override bool Brust()       => false;
        public override void ItemExist()   { }
        public override void ItemDrop()    { }
        public override void ItemMatch()   { }
        public override void ItemSwitch()  { }
    }
}
