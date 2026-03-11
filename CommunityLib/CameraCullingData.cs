using Assets.Code;
using System.Collections.Generic;

namespace CommunityLib
{
    public class CameraCullingData
    {
        public float PreciseX;
        public float PreciseY;
        public float Scale;

        public int X = -1;
        public int Y = -1;
        public int VisibleRadius = -1;

        public bool BoundsHaveChanged = false;
        public bool CameraHasMoved = false;

        public readonly HashSet<GraphicalHex> Loaded = new HashSet<GraphicalHex>();

        public void Clear()
        {
            PreciseX = -1f;
            PreciseY = -1f;
            Scale = -1f;

            X = -1;
            Y = -1;
            VisibleRadius = -1;
            BoundsHaveChanged = false;
            CameraHasMoved = false;

            Loaded.Clear();
        }
    }
}
