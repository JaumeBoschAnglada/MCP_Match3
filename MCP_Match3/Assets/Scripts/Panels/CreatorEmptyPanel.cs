using Match3.Data;

namespace Match3.Panels
{
    /// <summary>
    /// Invisible creator cell used as a logical spawn source.
    /// It is not destructible and does not block the board by itself.
    /// </summary>
    public class CreatorEmptyPanel : Panel
    {
        private void Awake()
        {
            m_PanelType = PanelType.Creator_Empty;
            Defence = 0;
        }

        public override bool Brust() => false;
    }
}