using System;
using UnityEngine;
using Grid = System.Collections.Generic.Dictionary<UnityEngine.Vector2Int, FloorGen.RoomType>;

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

        public static Vector2Int Move(this RoomType type, Vector2Int vector2Int)
        {
            return type switch
            {
                RoomType.LeftOpen => vector2Int + Vector2Int.left,
                RoomType.BottomOpen => vector2Int + Vector2Int.down,
                RoomType.RightOpen => vector2Int + Vector2Int.right,
                RoomType.TopOpen => vector2Int + Vector2Int.up,
                _ => vector2Int
            };
        }
    }
}