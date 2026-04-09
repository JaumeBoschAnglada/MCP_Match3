using UnityEngine;
using Match3.Data;

namespace Match3.Gameplay
{
    /// <summary>
    /// Special piece that eliminates an entire column when activated.
    /// Each color has its own prefab with the appropriate material already assigned.
    /// </summary>
    public class VerticalRowPiece : Piece
    {
        // Material comes from the prefab itself - no runtime override needed
    }
}
