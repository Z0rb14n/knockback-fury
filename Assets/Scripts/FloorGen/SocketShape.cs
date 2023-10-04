using System;
using UnityEngine;

namespace FloorGen
{
    /// <summary>
    /// Individual size/position pairing for socket location
    /// </summary>
    [Serializable]
    public struct SocketShape
    {
        public Vector2 size;
        public Vector2 position;
    }
}