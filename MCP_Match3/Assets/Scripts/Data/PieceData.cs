using UnityEngine;

namespace Match3.Data
{
    /// <summary>
    /// Color/Type of a piece
    /// </summary>
    public enum ColorType
    {
        Empty = 0,
        Red = 1,
        Blue = 2,
        Green = 3,
        Yellow = 4
    }

    /// <summary>
    /// Special effects that can be applied to pieces
    /// Created when 4+ pieces match
    /// </summary>
    public enum SpecialEffect
    {
        None = 0,
        HorizontalRow = 1,  // 4+ horizontal match
        VerticalRow = 2     // 4+ vertical match
    }

    /// <summary>
    /// Legacy PieceType for backward compatibility lookup
    /// </summary>
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
        public ColorType colorType;
        public SpecialEffect specialEffect;
        public int x;
        public int y;
        public bool isMarkedForRemoval;

        /// <summary>
        /// Constructor with ColorType
        /// </summary>
        public PieceData(int x, int y, ColorType color = ColorType.Empty, SpecialEffect effect = SpecialEffect.None)
        {
            this.x = x;
            this.y = y;
            this.colorType = color;
            this.specialEffect = effect;
            this.isMarkedForRemoval = false;
        }

        /// <summary>
        /// Legacy constructor for backward compatibility
        /// </summary>
        public PieceData(int x, int y, PieceType type = PieceType.Empty)
        {
            this.x = x;
            this.y = y;
            this.colorType = (ColorType)type;
            this.specialEffect = SpecialEffect.None;
            this.isMarkedForRemoval = false;
        }

        /// <summary>
        /// Property for backward compatibility
        /// </summary>
        public PieceType type
        {
            get => (PieceType)colorType;
            set => colorType = (ColorType)value;
        }

        /// <summary>
        /// Legacy property - now same as colorType
        /// </summary>
        public PieceType originalColor
        {
            get => (PieceType)colorType;
            set => colorType = (ColorType)value;
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
            return new PieceData(x, y, colorType, specialEffect) { isMarkedForRemoval = isMarkedForRemoval };
        }
    }
}
