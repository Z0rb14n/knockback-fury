using System;

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
}