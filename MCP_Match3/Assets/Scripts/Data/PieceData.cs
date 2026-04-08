using UnityEngine;

namespace Match3.Data
{
    public enum PieceType
    {
        Empty = 0,
        Red = 1,
        Blue = 2,
        Green = 3,
        Yellow = 4
    }

    public class PieceData
    {
        public PieceType type;
        public int x;
        public int y;
        public bool isMarkedForRemoval;

        public PieceData(int x, int y, PieceType type = PieceType.Empty)
        {
            this.x = x;
            this.y = y;
            this.type = type;
            this.isMarkedForRemoval = false;
        }

        public void MarkForRemoval()
        {
            isMarkedForRemoval = true;
        }

        public void Unmark()
        {
            isMarkedForRemoval = false;
        }

        public PieceData Clone()
        {
            return new PieceData(x, y, type) { isMarkedForRemoval = isMarkedForRemoval };
        }
    }
}
