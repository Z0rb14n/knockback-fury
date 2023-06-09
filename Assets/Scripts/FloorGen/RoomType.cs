using System;

using Cell = System.ValueTuple<int, int>;
using Grid = System.Collections.Generic.Dictionary<System.ValueTuple<int, int>, FloorGen.RoomType>;

namespace FloorGen
{
    [Flags]
    public enum RoomType
    {
        LeftOpen = 1,
        TopOpen = 2,
        RightOpen = 4,
        BottomOpen = 8
    }

    public static class RoomTypeUtil
    {
        public static RoomType GetOpposing(this RoomType type)
        {
            return type switch
            {
                RoomType.LeftOpen => RoomType.RightOpen,
                RoomType.BottomOpen => RoomType.TopOpen,
                RoomType.RightOpen => RoomType.LeftOpen,
                RoomType.TopOpen => RoomType.BottomOpen,
                _ => 0
            };
        }

        public static Cell Move(this RoomType type, Cell cell)
        {
            return type switch
            {
                RoomType.LeftOpen => (cell.Item1-1,cell.Item2),
                RoomType.BottomOpen => (cell.Item1,cell.Item2-1),
                RoomType.RightOpen => (cell.Item1+1,cell.Item2),
                RoomType.TopOpen => (cell.Item1,cell.Item2+1),
                _ => cell
            };
        }
    }
}