using Assets.Code;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace CommunityLib
{
    static public class HexGridUtils
    {
        #region Constants
        public const float HexWidth = 1.31f;

        public const float HexHeight = 1.11f;

        public static readonly float RootThree = Mathf.Sqrt(3);

        public const float Epsilon = 0.0005f;
        #endregion

        #region CoordinateConversions
        public static Vector2Int EvenRToAxial(Vector2Int evenR)
        {
            int q = evenR.x - (evenR.y + (evenR.y & 1)) / 2;
            int r = evenR.y;

            return new Vector2Int(q, r);
        }

        public static Vector3Int EvenRToCube(Vector2Int evenR)
        {
            int q = evenR.x - (evenR.y + (evenR.y & 1)) / 2;
            int r = evenR.y;

            return new Vector3Int(q, r, -q-r);
        }

        public static Vector3 FractionalEvenRToCube(Vector2 evenR)
        {
            int parity = 1 - (Mathf.FloorToInt(evenR.y) & 1);

            float q = evenR.x - (evenR.y + parity) / 2f;
            float r = evenR.y;

            return new Vector3(q, r, -q - r);
        }

        public static Vector2Int AxialToEvenR(Vector2Int axial)
        {
            int x = axial.x + (axial.y + (axial.y & 1)) / 2;

            return new Vector2Int(x, axial.y);
        }

        public static Vector3Int AxialToCube(Vector2Int axial)
        {
            return new Vector3Int(axial.x, axial.y, -axial.x - axial.y);
        }

        public static Vector3 FractionalAxialToCube(Vector2 axial)
        {
            return new Vector3(axial.x, axial.y, -axial.x - axial.y);
        }

        public static Vector2Int CubeToEvenR(Vector3Int cube)
        {
            int x = cube.x + (cube.y + (cube.y & 1)) / 2;

            return new Vector2Int(x, cube.y);
        }

        public static Vector2Int CubeToAxial(Vector3Int cube)
        {
            return new Vector2Int(cube.x, cube.y);
        }

        public static Vector2 EvenRToWorld(Vector2Int evenR)
        {
            int parity = 1 - (evenR.y & 1);

            float x = evenR.x + parity * (HexWidth / 2f);
            float y = evenR.y * HexHeight * 0.75f;
            return new Vector2(x, y);
        }

        public static Vector2 AxialToWorld(Vector2Int axial)
        {
            return EvenRToWorld(AxialToEvenR(axial));
        }

        public static Vector2 CubeToWorld(Vector3Int cube)
        {
            return EvenRToWorld(CubeToEvenR(cube));
        }

        public static Vector2Int HexToEvenR(Hex hex)
        {
            return new Vector2Int(hex.x, hex.y);
        }

        public static Vector2Int HexToAxial(Hex hex)
        {
            return EvenRToAxial(new Vector2Int(hex.x, hex.y));
        }

        public static Vector3Int HexToCube(Hex hex)
        {
            return EvenRToCube(new Vector2Int(hex.x, hex.y));
        }

        public static Vector2 HexToWorld(Hex hex)
        {
            return EvenRToWorld(new Vector2Int(hex.x, hex.y));
        }
        #endregion

        #region CoordinateRounding
        public static Vector3Int RoundCubeCoordinate(Vector3 cube)
        {
            int rq = Mathf.RoundToInt(cube.x);
            int rr = Mathf.RoundToInt(cube.y);
            int rs = Mathf.RoundToInt(cube.z);

            float qDiff = Math.Abs(rq - cube.x);
            float rDiff = Math.Abs(rr - cube.y);
            float sDiff = Math.Abs(rs - cube.z);

            if (qDiff > rDiff && qDiff > sDiff)
            {
                rq = -rr - rs;
            }
            else if (rDiff > sDiff)
            {
                rr = -rq - rs;
            }
            else
            {
                rs = -rq - rr;
            }

            return new Vector3Int(rq, rr, rs);
        }

        public static Vector2Int RoundAxialCoordinate(Vector2 axial)
        {
            Vector3Int cube = RoundCubeCoordinate(FractionalAxialToCube(axial));
            return CubeToAxial(cube);
        }

        public static Vector2Int RoundEvenRCoordinate(Vector2 evenR)
        {
            Vector3Int cube = RoundCubeCoordinate(FractionalEvenRToCube(evenR));
            return CubeToEvenR(cube);
        }
        #endregion

        #region DistanceCalculations
        public static int HexDistanceCube(Vector3Int cubeA, Vector3Int cubeB)
        {
            return Math.Max(Math.Abs(cubeA.x - cubeB.x), Math.Max(Math.Abs(cubeA.y - cubeB.y), Math.Abs(cubeA.z - cubeB.z)));
        }

        public static int HexDistanceAxial(Vector2Int axialA, Vector2Int axialB)
        {
            return HexDistanceCube(AxialToCube(axialA), AxialToCube(axialB));
        }

        public static int HexDistanceEvenR(Vector2Int evenRA, Vector2Int evenRB)
        {
            return HexDistanceCube(EvenRToCube(evenRA), EvenRToCube(evenRB));
        }

        public static int HexDistance(Hex hexA, Hex hexB)
        {
            return HexDistanceEvenR(new Vector2Int(hexA.x, hexA.y), new Vector2Int(hexB.x, hexB.y));
        }

        public static float EuclideanSquareDistanceCube(Vector3Int cubeA, Vector3Int cubeB)
        {
            Vector2 worldA = CubeToWorld(cubeA);
            Vector2 worldB = CubeToWorld(cubeB);

            return ((worldB.x - worldA.x) * (worldB.x - worldA.x)) + ((worldB.y - worldA.y) * (worldB.y - worldA.y));
        }

        public static float EuclideanSquareDistanceAxial(Vector2Int axialA, Vector2Int axialB)
        {
            Vector2 worldA = AxialToWorld(axialA);
            Vector2 worldB = AxialToWorld(axialB);

            return (worldA.x - worldB.x) * (worldA.x - worldB.x) + ((worldA.y - worldB.y) * (worldA.y - worldB.y));
        }

        public static float EuclideanSquareDistanceEvenR(Vector2Int evenRA, Vector2Int evenRB)
        {
            Vector2 worldA = EvenRToWorld(evenRA);
            Vector2 worldB = EvenRToWorld(evenRB);

            return (worldA.x - worldB.x) * (worldA.x - worldB.x) + ((worldA.y - worldB.y) * (worldA.y - worldB.y));
        }

        public static float EuclideanDistanceCube(Vector3Int cubeA, Vector3Int cubeB)
        {
            return Mathf.Sqrt(EuclideanSquareDistanceCube(cubeA, cubeB));
        }

        public static float EuclideanDistanceAxial(Vector2Int axialA, Vector2Int axialB)
        {
            return Mathf.Sqrt(EuclideanSquareDistanceAxial(axialA, axialB));
        }

        public static float EuclideanDistanceEvenR(Vector2Int evenRA, Vector2Int evenRB)
        {
            return Mathf.Sqrt(EuclideanSquareDistanceEvenR(evenRA, evenRB));
        }

        public static float EuclideanDistance(Hex hexA, Hex hexB)
        {
            return EuclideanDistanceEvenR(new Vector2Int(hexA.x, hexA.y), new Vector2Int(hexB.x, hexB.y));
        }
        #endregion

        #region HexesInRadius
        public static List<Vector3Int> CubeCoordinatesWithinRadiusNaive(Vector3Int origin, int radius, out List<Vector3Int>[] cubeCoordinatesByDistance)
        {
            List<Vector3Int> results = new List<Vector3Int>(1 + 3 * radius * (radius + 1));
            cubeCoordinatesByDistance = new List<Vector3Int>[radius + 1];
            for (int d = 0; d <= radius; d++)
            {
                cubeCoordinatesByDistance[d] = new List<Vector3Int>(d == 0 ? 1 : 6 * d);
            }

            for (int dx = -radius; dx <= radius; dx++)
            {
                for (int dy = -radius; dy <= radius; dy++)
                {
                    int dz = -dx - dy;
                    if (Math.Abs(dz) <= radius)
                    {
                        int dist = Math.Max(Math.Abs(dx), Math.Max(Math.Abs(dy), Math.Abs(dz)));
                        Vector3Int cube = new Vector3Int(origin.x + dx, origin.y + dy, origin.z + dz);
                        cubeCoordinatesByDistance[dist].Add(cube);
                        results.Add(cube);
                    }
                }
            }

            return results;
        }

        public static List<Vector3Int> CubeCoordinatesWithinRadius(Map map, Vector3Int origin, int radius, out List<Vector3Int>[] cubeCoordinatesByDistance, bool includeOutOfBoundsCoordinates = false)
        {
            List<Vector3Int> results = new List<Vector3Int>(1 + 3 * radius * (radius + 1));
            cubeCoordinatesByDistance = new List<Vector3Int>[radius + 1];

            if (includeOutOfBoundsCoordinates)
            {
                return CubeCoordinatesWithinRadiusNaive(origin, radius, out cubeCoordinatesByDistance);
            }

            for (int d = 0; d <= radius; d++)
            {
                cubeCoordinatesByDistance[d] = new List<Vector3Int>(d == 0 ? 1 : 6 * d);
            }

            int minDY = -origin.y;
            int maxDY = map.sizeY - 1 - origin.y;
            for (int dy = Math.Max(-radius, minDY); dy <= Math.Min(radius, maxDY); dy++)
            {
                int y = origin.y + dy;

                int xOffset = (y + (y & 1)) / 2;
                int minDX = -origin.x - xOffset;
                int maxDX = (map.sizeX - 1) - xOffset - origin.x;

                for (int dx = Math.Max(-radius, minDX); dx <= Math.Min(radius, maxDX); dx++)
                {
                    int x = origin.x + dx;

                    int dz = -dx - dy;
                    if (Math.Abs(dz) <= radius)
                    {
                        int z = origin.z + dz;
                        int dist = Math.Max(Math.Abs(dx), Math.Max(Math.Abs(dy), Math.Abs(dz)));
                        Vector3Int cube = new Vector3Int(x, y, z);
                        cubeCoordinatesByDistance[dist].Add(cube);
                        results.Add(cube);
                    }
                }
            }

            return results;
        }

        public static List<Vector2Int> AxialCoordinatesWithinRadius(Map map, Vector2Int origin, int radius, out List<Vector2Int>[] axialCoordinatesByDistance, bool includeOutOfBoundsCoordinates = false)
        {
            List<Vector2Int> results = new List<Vector2Int>(1 + 3 * radius * (radius + 1));
            axialCoordinatesByDistance = new List<Vector2Int>[radius + 1];

            if (CubeCoordinatesWithinRadius(map, AxialToCube(origin), radius, out List<Vector3Int>[] cubeCoordinatesByDistance, includeOutOfBoundsCoordinates).Count == 0)
            {
                return results;
            }

            for (int d = 0; d < cubeCoordinatesByDistance.Length; d++)
            {
                axialCoordinatesByDistance[d] = new List<Vector2Int>(d == 0 ? 1 : 6 * d);
                foreach (Vector3Int cube in cubeCoordinatesByDistance[d])
                {
                    Vector2Int axial = CubeToAxial(cube);
                    results.Add(axial);
                    axialCoordinatesByDistance[d].Add(axial);
                }
            }

            return results;
        }

        public static List<Vector2Int> EvenRCoordinatesWithinRadius(Map map, Vector2Int origin, int radius, out List<Vector2Int>[] evenRCoordinatesByDistance, bool includeOutOfBoundsCoordinates = false)
        {
            List<Vector2Int> results = new List<Vector2Int>(1 + 3 * radius * (radius + 1));
            evenRCoordinatesByDistance = new List<Vector2Int>[radius + 1];

            if (CubeCoordinatesWithinRadius(map, EvenRToCube(origin), radius, out List<Vector3Int>[] cubeCoordinatesByDistance, includeOutOfBoundsCoordinates).Count == 0)
            {
                return results;
            }

            for (int d = 0; d < cubeCoordinatesByDistance.Length; d++)
            {
                evenRCoordinatesByDistance[d] = new List<Vector2Int>(d == 0 ? 1 : 6 * d);
                foreach (Vector3Int cube in cubeCoordinatesByDistance[d])
                {
                    Vector2Int evenR = CubeToEvenR(cube);
                    results.Add(evenR);
                    evenRCoordinatesByDistance[d].Add(evenR);
                }
            }

            return results;
        }

        public static List<Hex> HexesWithinRadius(Map map, Hex origin, int radius, out List<Hex>[] hexesByDistance)
        {
            List<Hex> results = new List<Hex>(1 + 3 * radius * (radius + 1));
            hexesByDistance = new List<Hex>[radius + 1];

            if (CubeCoordinatesWithinRadius(map, HexToCube(origin), radius, out List<Vector3Int>[] cubeCoordinatesByDistance).Count == 0)
            {
                return results;
            }

            for (int d = 0; d < cubeCoordinatesByDistance.Length; d++)
            {
                hexesByDistance[d] = new List<Hex>(d == 0 ? 1 : 6 * d);
                foreach (Vector3Int cube in cubeCoordinatesByDistance[d])
                {
                    Vector2Int evenR = CubeToEvenR(cube);
                    for (int i = 0; i < map.grid.Length; i++)
                    {
                        Hex hex = map.grid[i][evenR.x][evenR.y];

                        if (hex != null)
                        {
                            results.Add(hex);
                            hexesByDistance[d].Add(hex);
                        }
                    }
                }
            }

            return results;
        }

        public static List<Hex> HexesWithinRadius(Map map, Hex origin, int radius, int mapLayer, out List<Hex>[] hexesByDistance)
        {
            List<Hex> results = new List<Hex>(1 + 3 * radius * (radius + 1));
            hexesByDistance = new List<Hex>[radius + 1];

            if (mapLayer < 0 || mapLayer >= map.grid.Length)
            {
                return results;
            }

            if (CubeCoordinatesWithinRadius(map, HexToCube(origin), radius, out List<Vector3Int>[] cubeCoordinatesByDistance).Count == 0)
            {
                return results;
            }

            for (int d = 0; d < cubeCoordinatesByDistance.Length; d++)
            {
                hexesByDistance[d] = new List<Hex>(d == 0 ? 1 : 6 * d);
                foreach (Vector3Int cube in cubeCoordinatesByDistance[d])
                {
                    Vector2Int evenR = CubeToEvenR(cube);
                    Hex hex = map.grid[mapLayer][evenR.x][evenR.y];
                    if (hex != null)
                    {
                        results.Add(hex);
                        hexesByDistance[d].Add(hex);
                    }
                }
            }

            return results;
        }

        public static List<Hex> HexesWithinRadius(Map map, Hex origin, int radius, List<int> mapLayers, out List<Hex>[] hexesByDistance)
        {
            List<Hex> results = new List<Hex>(1 + 3 * radius * (radius + 1));
            hexesByDistance = new List<Hex>[radius + 1];

            if (mapLayers == null || mapLayers.Count == 0)
            {
                return results;
            }

            if (CubeCoordinatesWithinRadius(map, HexToCube(origin), radius, out List<Vector3Int>[] cubeCoordinatesByDistance).Count == 0)
            {
                return results;
            }

            for (int d = 0; d < cubeCoordinatesByDistance.Length; d++)
            {
                hexesByDistance[d] = new List<Hex>();
                foreach (Vector3Int cube in cubeCoordinatesByDistance[d])
                {
                    Vector2Int evenR = CubeToEvenR(cube);
                    foreach (int mapLayer in mapLayers.Distinct())
                    {
                        Hex hex = map.grid[mapLayer][evenR.x][evenR.y];

                        if (hex != null)
                        {
                            results.Add(hex);
                            hexesByDistance[d].Add(hex);
                        }
                    }
                }
            }

            return results;
        }
        #endregion

        #region HexesInRing
        public static List<Vector3Int> CubeCoordinatesWithinRingNaive(Vector3Int origin, int outerRadius, int innerRadius, out List<Vector3Int>[] cubeCoordinatesByDistanceFromInnerRadius)
        {
            if(outerRadius < innerRadius)
            {
                cubeCoordinatesByDistanceFromInnerRadius = new List<Vector3Int>[0];
                return new List<Vector3Int>();
            }

            int outerCount = 1 + 3 * outerRadius * (outerRadius + 1);
            int innerCount = 1 + 3 * innerRadius * (innerRadius + 1);
            List<Vector3Int> results = new List<Vector3Int>(outerCount - innerCount);

            int ringWidth = outerRadius - innerRadius;
            cubeCoordinatesByDistanceFromInnerRadius = new List<Vector3Int>[ringWidth + 1];

            for (int d = 0; d < cubeCoordinatesByDistanceFromInnerRadius.Length; d++)
            {
                int dist = innerRadius + d;
                cubeCoordinatesByDistanceFromInnerRadius[d] = new List<Vector3Int>(dist == 0 ? 1 : 6 * dist);
            }

            for (int dx = -outerRadius; dx <= outerRadius; dx++)
            {
                for (int dy = -outerRadius; dy <= outerRadius; dy++)
                {
                    int dz = -dx - dy;
                    if (Math.Abs(dz) <= outerRadius)
                    {
                        int dist = Math.Max(Math.Abs(dx), Math.Max(Math.Abs(dy), Math.Abs(dz)));
                        if (dist >= innerRadius)
                        {
                            Vector3Int cube = new Vector3Int(origin.x + dx, origin.y + dy, origin.z + dz);
                            cubeCoordinatesByDistanceFromInnerRadius[dist - innerRadius].Add(cube);
                            results.Add(cube);
                        }
                    }
                }
            }

            return results;
        }

        public static List<Vector3Int> CubeCoordinatesWithinRing(Map map, Vector3Int origin, int outerRadius, int innerRadius, out List<Vector3Int>[] cubeCoordinatesByDistanceFromInnerRadius, bool includeOutOfBoundsCoordinates = false)
        {
            if (outerRadius < innerRadius)
            {
                cubeCoordinatesByDistanceFromInnerRadius = new List<Vector3Int>[0];
                return new List<Vector3Int>();
            }

            int outerCount = 1 + 3 * outerRadius * (outerRadius + 1);
            int innerCount = 1 + 3 * innerRadius * (innerRadius + 1);
            List<Vector3Int> results = new List<Vector3Int>(outerCount - innerCount);

            int ringWidth = outerRadius - innerRadius;
            cubeCoordinatesByDistanceFromInnerRadius = new List<Vector3Int>[ringWidth + 1];

            if (includeOutOfBoundsCoordinates)
            {
                return CubeCoordinatesWithinRingNaive(origin, outerRadius, innerRadius, out cubeCoordinatesByDistanceFromInnerRadius);
            }

            for (int d = 0; d < cubeCoordinatesByDistanceFromInnerRadius.Length; d++)
            {
                int dist = innerRadius + d;
                cubeCoordinatesByDistanceFromInnerRadius[d] = new List<Vector3Int>(dist == 0 ? 1 : 6 * dist);
            }

            int minDY = -origin.y;
            int maxDY = map.sizeY - 1 - origin.y;
            for (int dy = Math.Max(-outerRadius, minDY); dy <= Math.Min(outerRadius, maxDY); dy++)
            {
                int y = origin.y + dy;
                int xOffset = (y + (y & 1)) / 2;
                int minDX = -origin.x - xOffset;
                int maxDX = (map.sizeX - 1) - xOffset - origin.x;

                for (int dx = Math.Max(-outerRadius, minDX); dx <= Math.Min(outerRadius, maxDX); dx++)
                {
                    int x = origin.x + dx;
                    int dz = -dx - dy;
                    int dist = Math.Max(Math.Abs(dx), Math.Max(Math.Abs(dy), Math.Abs(dz)));
                    if (dist >= innerRadius && dist <= outerRadius)
                    {
                        Vector3Int cube = new Vector3Int(x, y, origin.z + dz);
                        cubeCoordinatesByDistanceFromInnerRadius[dist - innerRadius].Add(cube);
                        results.Add(cube);
                    }
                }
            }

            return results;
        }

        public static List<Vector2Int> AxialCoordinatesWithinRing(Map map, Vector2Int origin, int outerRadius, int innerRadius, out List<Vector2Int>[] axialCoordinatesByDistanceFromInnerRadius, bool includeOutOfBoundsCoordinates = false)
        {
            if (outerRadius < innerRadius)
            {
                axialCoordinatesByDistanceFromInnerRadius = new List<Vector2Int>[0];
                return new List<Vector2Int>();
            }

            int outerCount = 1 + 3 * outerRadius * (outerRadius + 1);
            int innerCount = 1 + 3 * innerRadius * (innerRadius + 1);
            List<Vector2Int> results = new List<Vector2Int>(outerCount - innerCount);

            int ringWidth = outerRadius - innerRadius;
            axialCoordinatesByDistanceFromInnerRadius = new List<Vector2Int>[ringWidth + 1];

            if (CubeCoordinatesWithinRing(map, AxialToCube(origin), outerRadius, innerRadius, out List<Vector3Int>[] cubeCoordinatesByDistanceFromInnerRadius, includeOutOfBoundsCoordinates).Count == 0)
            {
                return results;
            }

            for (int d = 0; d < cubeCoordinatesByDistanceFromInnerRadius.Length; d++)
            {
                axialCoordinatesByDistanceFromInnerRadius[d] = new List<Vector2Int>(d == 0 ? 1 : 6 * d);
                foreach (var cube in cubeCoordinatesByDistanceFromInnerRadius[d])
                {
                    Vector2Int axial = CubeToAxial(cube);
                    axialCoordinatesByDistanceFromInnerRadius[d].Add(axial);
                    results.Add(axial);
                }
            }

            return results;
        }

        public static List<Vector2Int> EvenRCoordinatesWithinRing(Map map, Vector2Int origin, int outerRadius, int innerRadius, out List<Vector2Int>[] evenRCoordinatesByDistanceFromInnerRadius, bool includeOutOfBoundsCoordinates = false)
        {
            if (outerRadius < innerRadius)
            {
                evenRCoordinatesByDistanceFromInnerRadius = new List<Vector2Int>[0];
                return new List<Vector2Int>();
            }

            int outerCount = 1 + 3 * outerRadius * (outerRadius + 1);
            int innerCount = 1 + 3 * innerRadius * (innerRadius + 1);
            List<Vector2Int> results = new List<Vector2Int>(outerCount - innerCount);

            int ringWidth = outerRadius - innerRadius;
            evenRCoordinatesByDistanceFromInnerRadius = new List<Vector2Int>[ringWidth + 1];

            if (CubeCoordinatesWithinRing(map, EvenRToCube(origin), outerRadius, innerRadius, out List<Vector3Int>[] cubeCoordinatesByDistanceFromInnerRadius, includeOutOfBoundsCoordinates).Count == 0)
            {
                return results;
            }

            for (int d = 0; d < cubeCoordinatesByDistanceFromInnerRadius.Length; d++)
            {
                evenRCoordinatesByDistanceFromInnerRadius[d] = new List<Vector2Int>(d == 0 ? 1 : 6 * d);
                foreach (var cube in cubeCoordinatesByDistanceFromInnerRadius[d])
                {
                    Vector2Int evenR = CubeToEvenR(cube);
                    evenRCoordinatesByDistanceFromInnerRadius[d].Add(evenR);
                    results.Add(evenR);
                }
            }

            return results;
        }

        public static List<Hex> HexesWithinRing(Map map, Hex origin, int outerRadius, int innerRadius, out List<Hex>[] hexesByDistanceFromInnerRadius)
        {
            if (outerRadius < innerRadius)
            {
                hexesByDistanceFromInnerRadius = new List<Hex>[0];
                return new List<Hex>();
            }

            int outerCount = 1 + 3 * outerRadius * (outerRadius + 1);
            int innerCount = 1 + 3 * innerRadius * (innerRadius + 1);
            List<Hex> results = new List<Hex>(outerCount - innerCount);

            int ringWidth = outerRadius - innerRadius;
            hexesByDistanceFromInnerRadius = new List<Hex>[ringWidth + 1];

            if (CubeCoordinatesWithinRing(map, HexToCube(origin), outerRadius, innerRadius, out List<Vector3Int>[] cubeCoordinatesByDistanceFromInnerRadius).Count == 0)
            {
                return results;
            }

            for (int d = 0; d < cubeCoordinatesByDistanceFromInnerRadius.Length; d++)
            {
                hexesByDistanceFromInnerRadius[d] = new List<Hex>(d == 0 ? 1 : 6 * d);
                foreach (var cube in cubeCoordinatesByDistanceFromInnerRadius[d])
                {
                    Vector2Int evenR = CubeToEvenR(cube);
                    for (int layer = 0; layer < map.grid.Length; layer++)
                    {
                        Hex hex = map.grid[layer][evenR.x][evenR.y];
                        if (hex != null)
                        {
                            hexesByDistanceFromInnerRadius[d].Add(hex);
                            results.Add(hex);
                        }
                    }
                }
            }

            return results;
        }

        public static List<Hex> HexesWithinRing(Map map, Hex origin, int outerRadius, int innerRadius, int mapLayer, out List<Hex>[] hexesByDistanceFromInnerRadius)
        {
            if (outerRadius < innerRadius)
            {
                hexesByDistanceFromInnerRadius = new List<Hex>[0];
                return new List<Hex>();
            }

            int outerCount = 1 + 3 * outerRadius * (outerRadius + 1);
            int innerCount = 1 + 3 * innerRadius * (innerRadius + 1);
            List<Hex> results = new List<Hex>(outerCount - innerCount);

            int ringWidth = outerRadius - innerRadius;
            hexesByDistanceFromInnerRadius = new List<Hex>[ringWidth + 1];

            if (mapLayer < 0 || mapLayer >= map.grid.Length)
            {
                return results;
            }

            if (CubeCoordinatesWithinRing(map, HexToCube(origin), outerRadius, innerRadius, out List<Vector3Int>[] cubeCoordinatesByDistanceFromInnerRadius).Count == 0)
            {
                return results;
            }

            for (int d = 0; d < cubeCoordinatesByDistanceFromInnerRadius.Length; d++)
            {
                hexesByDistanceFromInnerRadius[d] = new List<Hex>(d == 0 ? 1 : 6 * d);
                foreach (var cube in cubeCoordinatesByDistanceFromInnerRadius[d])
                {
                    Vector2Int evenR = CubeToEvenR(cube);
                    Hex hex = map.grid[mapLayer][evenR.x][evenR.y];
                    if (hex != null)
                    {
                        hexesByDistanceFromInnerRadius[d].Add(hex);
                        results.Add(hex);
                    }
                }
            }

            return results;
        }

        public static List<Hex> HexesWithinRing(Map map, Hex origin, int outerRadius, int innerRadius, List<int> mapLayers, out List<Hex>[] hexesByDistanceFromInnerRadius)
        {
            if (outerRadius < innerRadius)
            {
                hexesByDistanceFromInnerRadius = new List<Hex>[0];
                return new List<Hex>();
            }

            int outerCount = 1 + 3 * outerRadius * (outerRadius + 1);
            int innerCount = 1 + 3 * innerRadius * (innerRadius + 1);
            List<Hex> results = new List<Hex>(outerCount - innerCount);

            int ringWidth = outerRadius - innerRadius;
            hexesByDistanceFromInnerRadius = new List<Hex>[ringWidth + 1];

            if (mapLayers == null || mapLayers.Count == 0)
            {
                return results;
            }

            if (CubeCoordinatesWithinRing(map, HexToCube(origin), outerRadius, innerRadius, out List<Vector3Int>[] cubeCoordinatesByDistanceFromInnerRadius).Count == 0)
            {
                return results;
            }

            for (int d = 0; d < cubeCoordinatesByDistanceFromInnerRadius.Length; d++)
            {
                hexesByDistanceFromInnerRadius[d] = new List<Hex>(d == 0 ? 1 : 6 * d);
                foreach (var cube in cubeCoordinatesByDistanceFromInnerRadius[d])
                {
                    Vector2Int evenR = CubeToEvenR(cube);
                    foreach (int layer in mapLayers.Distinct())
                    {
                        Hex hex = map.grid[layer][evenR.x][evenR.y];
                        if (hex != null)
                        {
                            hexesByDistanceFromInnerRadius[d].Add(hex);
                            results.Add(hex);
                        }
                    }
                }
            }

            return results;
        }
        #endregion

        #region HexesWithinCircle
        public static List<Vector3Int> CubeCoordinatesWithinCircleNaive(Vector3Int origin, double radius, out List<Vector3Int>[] cubeCoordinatesByDistance)
        {
            int outerRadius = (int)Math.Ceiling(radius);

            List<Vector3Int> results = new List<Vector3Int>(1 + 3 * outerRadius * (outerRadius + 1));

            int innerRadius = (int)Math.Floor((RootThree / 2) * outerRadius);
            double squareRadius = radius * radius;
            cubeCoordinatesByDistance = new List<Vector3Int>[outerRadius + 1];

            if (CubeCoordinatesWithinRadiusNaive(origin, outerRadius, out List<Vector3Int>[] outerCubeCoordinatesByDistance).Count == 0)
            {
                return results;
            }

            for (int d = 0; d < outerCubeCoordinatesByDistance.Length; d++)
            {
                if (d <= innerRadius)
                {
                    cubeCoordinatesByDistance[d] = outerCubeCoordinatesByDistance[d];
                    results.AddRange(cubeCoordinatesByDistance[d]);
                    continue;
                }

                cubeCoordinatesByDistance[d] = new List<Vector3Int>(d == 0 ? 1 : 6 * d);
                foreach (Vector3Int cube in outerCubeCoordinatesByDistance[d])
                {
                    float squareDistance = EuclideanSquareDistanceCube(origin, cube);
                    if (squareDistance <= squareRadius + Epsilon)
                    {
                        cubeCoordinatesByDistance[d].Add(cube);
                        results.Add(cube);
                    }
                }
            }

            return results;
        }

        public static List<Vector3Int> CubeCoordinatesWithinCircle(Map map, Vector3Int origin, double radius, out List<Vector3Int>[] cubeCoordinatesByDistance, bool includeOutOfBoundsCoordinates = false)
        {
            int outerRadius = (int)Math.Ceiling(radius);

            List<Vector3Int> results = new List<Vector3Int>(1 + 3 * outerRadius * (outerRadius + 1));

            cubeCoordinatesByDistance = new List<Vector3Int>[outerRadius + 1];

            if (includeOutOfBoundsCoordinates)
            {
                return CubeCoordinatesWithinCircleNaive(origin, radius, out cubeCoordinatesByDistance);
            }

            if (CubeCoordinatesWithinRadius(map, origin, outerRadius, out List<Vector3Int>[] outerCubeCoordinatesByDistance).Count == 0)
            {
                return results;
            }

            int innerRadius = (int)Math.Floor((RootThree / 2) * outerRadius);
            double squareRadius = radius * radius;
            for (int d = 0; d < outerCubeCoordinatesByDistance.Length; d++)
            {
                if (d <= innerRadius)
                {
                    cubeCoordinatesByDistance[d] = outerCubeCoordinatesByDistance[d];
                    results.AddRange(cubeCoordinatesByDistance[d]);
                    continue;
                }

                cubeCoordinatesByDistance[d] = new List<Vector3Int>(d == 0 ? 1 : 6 * d);
                foreach (Vector3Int cube in outerCubeCoordinatesByDistance[d])
                {
                    float squareDistance = EuclideanSquareDistanceCube(origin, cube);
                    if (squareDistance <= squareRadius + Epsilon)
                    {
                        cubeCoordinatesByDistance[d].Add(cube);
                        results.Add(cube);
                    }
                }
            }

            return results;
        }

        public static List<Vector2Int> AxialCoordinatesWithinCircle(Map map, Vector2Int origin, double radius, out List<Vector2Int>[] axialCoordinatesByDistance, bool includeOutOfBoundsCoordinates = false)
        {
            int outerRadius = (int)Math.Ceiling(radius);

            List<Vector2Int> results = new List<Vector2Int>(1 + 3 * outerRadius * (outerRadius + 1));

            axialCoordinatesByDistance = new List<Vector2Int>[outerRadius + 1];

            if (CubeCoordinatesWithinCircle(map, AxialToCube(origin), radius, out List<Vector3Int>[] naiveCoordinatesByDistance, includeOutOfBoundsCoordinates).Count == 0)
            {
                return results;
            }
            if (includeOutOfBoundsCoordinates)
            {
                for (int d = 0; d < naiveCoordinatesByDistance.Length; d++)
                {
                    axialCoordinatesByDistance[d] = new List<Vector2Int>(d == 0 ? 1 : 6 * d);
                    foreach (Vector3Int cube in naiveCoordinatesByDistance[d])
                    {
                        Vector2Int axial = CubeToAxial(cube);
                        axialCoordinatesByDistance[d].Add(axial);
                        results.Add(axial);
                    }
                }

                return results;
            }

            for (int d = 0; d < naiveCoordinatesByDistance.Length; d++)
            {
                axialCoordinatesByDistance[d] = new List<Vector2Int>();
                foreach (Vector3Int cube in naiveCoordinatesByDistance[d])
                {
                    Vector2Int evenR = CubeToEvenR(cube);
                    if (evenR.x < 0 || evenR.x >= map.sizeX || evenR.y < 0 || evenR.y >= map.sizeY)
                    {
                        continue;
                    }
                    Vector2Int axial = CubeToAxial(cube);
                    axialCoordinatesByDistance[d].Add(axial);
                    results.Add(axial);
                }
            }

            return results;
        }

        public static List<Vector2Int> EvenRCoordinatesWithinCircle(Map map, Vector2Int origin, double radius, out List<Vector2Int>[] evenRCoordinatesByDistance, bool includeOutOfBoundsCoordinates = false)
        {
            int outerRadius = (int)Math.Ceiling(radius);

            List<Vector2Int> results = new List<Vector2Int>(1 + 3 * outerRadius * (outerRadius + 1));

            evenRCoordinatesByDistance = new List<Vector2Int>[outerRadius + 1];

            if (CubeCoordinatesWithinCircle(map, EvenRToCube(origin), radius, out List<Vector3Int>[] naiveCoordinatesByDistance, includeOutOfBoundsCoordinates).Count == 0)
            {
                return results;
            }

            for (int d = 0; d < naiveCoordinatesByDistance.Length; d++)
            {
                evenRCoordinatesByDistance[d] = new List<Vector2Int>(d == 0 ? 1 : 6 * d);
                foreach (Vector3Int cube in naiveCoordinatesByDistance[d])
                {
                    Vector2Int evenR = CubeToEvenR(cube);
                    if (evenR.x < 0 || evenR.x >= map.sizeX || evenR.y < 0 || evenR.y >= map.sizeY)
                    {
                        continue;
                    }
                    evenRCoordinatesByDistance[d].Add(evenR);
                    results.Add(evenR);
                }
            }

            return results;
        }

        public static List<Hex> HexesWithinCircle(Map map, Hex origin, double radius, out List<Hex>[] hexesByDistance)
        {
            int outerRadius = (int)Math.Ceiling(radius);

            List<Hex> results = new List<Hex>(1 + 3 * outerRadius * (outerRadius + 1));

            hexesByDistance = new List<Hex>[outerRadius + 1];

            if (CubeCoordinatesWithinCircle(map, HexToCube(origin), radius, out List<Vector3Int>[] cubeCoordinatesByDistance).Count == 0)
            {
                return results;
            }

            for (int d = 0; d < cubeCoordinatesByDistance.Length; d++)
            {
                hexesByDistance[d] = new List<Hex>(d == 0 ? 1 : 6 * d);
                foreach (Vector3Int cube in cubeCoordinatesByDistance[d])
                {
                    Vector2Int evenR = CubeToEvenR(cube);
                    for (int i = 0; i < map.grid.Length; i++)
                    {
                        Hex hex = map.grid[i][evenR.x][evenR.y];

                        if (hex != null)
                        {
                            results.Add(hex);
                            hexesByDistance[d].Add(hex);
                        }
                    }
                }
            }

            return results;
        }

        public static List<Hex> HexesWithinCircle(Map map, Hex origin, double radius, int mapLayer, out List<Hex>[] hexesByDistance)
        {
            int outerRadius = (int)Math.Ceiling(radius);

            List<Hex> results = new List<Hex>(1 + 3 * outerRadius * (outerRadius + 1));

            hexesByDistance = new List<Hex>[outerRadius + 1];

            if (mapLayer < 0 || mapLayer >= map.grid.Length)
            {
                return results;
            }

            if (CubeCoordinatesWithinCircle(map, HexToCube(origin), radius, out List<Vector3Int>[] cudeCoordinatesByDistance).Count == 0)
            {
                return results;
            }

            for (int d = 0; d < cudeCoordinatesByDistance.Length; d++)
            {
                hexesByDistance[d] = new List<Hex>(d == 0 ? 1 : 6 * d);
                foreach (Vector3Int cube in cudeCoordinatesByDistance[d])
                {
                    Vector2Int evenR = CubeToEvenR(cube);
                    Hex hex = map.grid[mapLayer][evenR.x][evenR.y];
                    if (hex != null)
                    {
                        results.Add(hex);
                        hexesByDistance[d].Add(hex);
                    }
                }
            }

            return results;
        }

        public static List<Hex> HexesWithinCircle(Map map, Hex origin, double radius, List<int> mapLayers, out List<Hex>[] hexesByDistance)
        {
            int outerRadius = (int)Math.Ceiling(radius);

            List<Hex> results = new List<Hex>(1 + 3 * outerRadius * (outerRadius + 1));

            hexesByDistance = new List<Hex>[outerRadius + 1];

            if (mapLayers == null || mapLayers.Count == 0)
            {
                return results;
            }

            if (CubeCoordinatesWithinCircle(map, HexToCube(origin), radius, out List<Vector3Int>[] cudeCoordinatesByDistance).Count == 0)
            {
                return results;
            }

            for (int d = 0; d < cudeCoordinatesByDistance.Length; d++)
            {
                hexesByDistance[d] = new List<Hex>(d == 0 ? 1 : 6 * d);
                foreach (Vector3Int cube in cudeCoordinatesByDistance[d])
                {
                    Vector2Int evenR = CubeToEvenR(cube);
                    foreach (int mapLayer in mapLayers.Distinct())
                    {
                        Hex hex = map.grid[mapLayer][evenR.x][evenR.y];

                        if (hex != null)
                        {
                            results.Add(hex);
                            hexesByDistance[d].Add(hex);
                        }
                    }
                }
            }

            return results;
        }
        #endregion

        #region HexesWithinCircularRing
        public static List<Vector3Int> CubeCoordinatesWithinCircularRingNaive(Vector3Int origin, double outerRadius, double innerRadius, out List<Vector3Int>[] cubeCoordinatesByDistanceFromInnerRadius)
        {
            if (outerRadius < innerRadius)
            {
                cubeCoordinatesByDistanceFromInnerRadius = new List<Vector3Int>[0];
                return new List<Vector3Int>();
            }

            int outerHexRadius = (int)Math.Ceiling(outerRadius);
            int innerHexRadius = (int)Math.Floor(innerRadius);
            if (innerRadius < 1f)
            {
                innerHexRadius = 0;
            }

            int outerCount = 1 + 3 * outerHexRadius * (outerHexRadius + 1);
            int innerCount = 1 + 3 * innerHexRadius * (innerHexRadius + 1);
            List<Vector3Int> results = new List<Vector3Int>(outerCount - innerCount);

            int ringWidth = outerHexRadius - innerHexRadius;
            cubeCoordinatesByDistanceFromInnerRadius = new List<Vector3Int>[ringWidth + 1];

            double squareOuterRadius = outerRadius * outerRadius;
            double squareInnerRadius = innerRadius * innerRadius;

            if (CubeCoordinatesWithinRingNaive(origin, outerHexRadius, innerHexRadius, out var hexCubeCoordinatesByDistanceFromInnerRadius).Count == 0)
            {
                return results;
            }

            for (int d = 0; d < hexCubeCoordinatesByDistanceFromInnerRadius.Length; d++)
            {
                int dist = innerHexRadius + d;
                cubeCoordinatesByDistanceFromInnerRadius[d] = new List<Vector3Int>(dist == 0 ? 1 : 6 * dist);
                foreach (Vector3Int cube in hexCubeCoordinatesByDistanceFromInnerRadius[d])
                {
                    float squareDistance = EuclideanSquareDistanceCube(origin, cube);
                    if (squareDistance <= squareOuterRadius + Epsilon && squareDistance >= squareInnerRadius + Epsilon)
                    {
                        cubeCoordinatesByDistanceFromInnerRadius[d].Add(cube);
                        results.Add(cube);
                    }
                }
            }

            return results;
        }

        public static List<Vector3Int> CubeCoordinatesWithinCircularRing(Map map, Vector3Int origin, double outerRadius, double innerRadius, out List<Vector3Int>[] cubeCoordinatesByDistanceFromInnerRadius, bool includeOutOfBoundsCoordinates = false)
        {
            if (outerRadius < innerRadius)
            {
                cubeCoordinatesByDistanceFromInnerRadius = new List<Vector3Int>[0];
                return new List<Vector3Int>();
            }

            if (includeOutOfBoundsCoordinates)
            {
                return CubeCoordinatesWithinCircularRingNaive(origin, outerRadius, innerRadius, out cubeCoordinatesByDistanceFromInnerRadius);
            }

            int outerHexRadius = (int)Math.Ceiling(outerRadius);
            int innerHexRadius = (int)Math.Floor(innerRadius);
            if (innerRadius < 1f)
            {
                innerHexRadius = 0;
            }

            int outerCount = 1 + 3 * outerHexRadius * (outerHexRadius + 1);
            int innerCount = 1 + 3 * innerHexRadius * (innerHexRadius + 1);
            List<Vector3Int> results = new List<Vector3Int>(outerCount - innerCount);

            int ringWidth = outerHexRadius - innerHexRadius;
            cubeCoordinatesByDistanceFromInnerRadius = new List<Vector3Int>[ringWidth + 1];

            double squareOuterRadius = outerRadius * outerRadius;
            double squareInnerRadius = innerRadius * innerRadius;

            if (CubeCoordinatesWithinRing(map, origin, outerHexRadius, innerHexRadius, out var hexCubeCoordinatesByDistanceFromInnerRadius).Count == 0)
            {
                return results;
            }

            for (int d = 0; d < hexCubeCoordinatesByDistanceFromInnerRadius.Length; d++)
            {
                int dist = innerHexRadius + d;
                cubeCoordinatesByDistanceFromInnerRadius[d] = new List<Vector3Int>(dist == 0 ? 1 : 6 * dist);
                foreach (Vector3Int cube in hexCubeCoordinatesByDistanceFromInnerRadius[d])
                {
                    float squareDistance = EuclideanSquareDistanceCube(origin, cube);
                    if (squareDistance <= squareOuterRadius + Epsilon && squareDistance >= squareInnerRadius + Epsilon)
                    {
                        cubeCoordinatesByDistanceFromInnerRadius[d].Add(cube);
                        results.Add(cube);
                    }
                }
            }

            return results;
        }

        public static List<Vector2Int> AxialCoordinatesWithinCircularRing(Map map, Vector2Int origin, double outerRadius, double innerRadius, out List<Vector2Int>[] axialCoordinatesByDistanceFromInnerRadius, bool includeOutOfBoundsCoordinates = false)
        {
            if (outerRadius < innerRadius)
            {
                axialCoordinatesByDistanceFromInnerRadius = new List<Vector2Int>[0];
                return new List<Vector2Int>();
            }

            int outerHexRadius = (int)Math.Ceiling(outerRadius);
            int innerHexRadius = (int)Math.Floor(innerRadius);
            if (innerRadius < 1f)
            {
                innerHexRadius = 0;
            }

            int outerCount = 1 + 3 * outerHexRadius * (outerHexRadius + 1);
            int innerCount = 1 + 3 * innerHexRadius * (innerHexRadius + 1);
            List<Vector2Int> results = new List<Vector2Int>(outerCount - innerCount);

            int ringWidth = outerHexRadius - innerHexRadius;
            axialCoordinatesByDistanceFromInnerRadius = new List<Vector2Int>[ringWidth + 1];

            List<Vector3Int>[] cubeCoordinatesByDistanceFromInnerRadius;

            if (includeOutOfBoundsCoordinates)
            {
                if (CubeCoordinatesWithinCircularRingNaive(EvenRToCube(origin), outerRadius, innerRadius, out cubeCoordinatesByDistanceFromInnerRadius).Count == 0)
                {
                    return results;
                }
            }
            else
            {
                if (CubeCoordinatesWithinCircularRing(map, EvenRToCube(origin), outerRadius, innerRadius, out cubeCoordinatesByDistanceFromInnerRadius).Count == 0)
                {
                    return results;
                }
            }

            for (int d = 0; d < cubeCoordinatesByDistanceFromInnerRadius.Length; d++)
            {
                int dist = innerHexRadius + d;
                axialCoordinatesByDistanceFromInnerRadius[d] = new List<Vector2Int>(dist == 0 ? 1 : 6 * dist);
                foreach (Vector3Int cube in cubeCoordinatesByDistanceFromInnerRadius[d])
                {
                    Vector2Int axial = CubeToAxial(cube);
                    axialCoordinatesByDistanceFromInnerRadius[d].Add(axial);
                    results.Add(axial);
                }
            }

            return results;
        }

        public static List<Vector2Int> EvenRCoordinatesWithinCircularRing(Map map, Vector2Int origin, double outerRadius, double innerRadius, out List<Vector2Int>[] evenRCoordinatesByDistanceFromInnerRadius, bool includeOutOfBoundsCoordinates = false)
        {
            if (outerRadius < innerRadius)
            {
                evenRCoordinatesByDistanceFromInnerRadius = new List<Vector2Int>[0];
                return new List<Vector2Int>();
            }

            int outerHexRadius = (int)Math.Ceiling(outerRadius);
            int innerHexRadius = (int)Math.Floor(innerRadius);
            if (innerRadius < 1f)
            {
                innerHexRadius = 0;
            }

            int outerCount = 1 + 3 * outerHexRadius * (outerHexRadius + 1);
            int innerCount = 1 + 3 * innerHexRadius * (innerHexRadius + 1);
            List<Vector2Int> results = new List<Vector2Int>(outerCount - innerCount);

            int ringWidth = outerHexRadius - innerHexRadius;
            evenRCoordinatesByDistanceFromInnerRadius = new List<Vector2Int>[ringWidth + 1];

            List<Vector3Int>[] cubeCoordinatesByDistanceFromInnerRadius;

            if (includeOutOfBoundsCoordinates)
            {
                if (CubeCoordinatesWithinCircularRingNaive(EvenRToCube(origin), outerRadius, innerRadius, out cubeCoordinatesByDistanceFromInnerRadius).Count == 0)
                {
                    return results;
                }
            }
            else
            {
                if (CubeCoordinatesWithinCircularRing(map, EvenRToCube(origin), outerRadius, innerRadius, out cubeCoordinatesByDistanceFromInnerRadius).Count == 0)
                {
                    return results;
                }
            }

            for (int d = 0; d < cubeCoordinatesByDistanceFromInnerRadius.Length; d++)
            {
                int dist = innerHexRadius + d;
                evenRCoordinatesByDistanceFromInnerRadius[d] = new List<Vector2Int>(dist == 0 ? 1 : 6 * dist);
                foreach (Vector3Int cube in cubeCoordinatesByDistanceFromInnerRadius[d])
                {
                    Vector2Int evenR = CubeToEvenR(cube);
                    evenRCoordinatesByDistanceFromInnerRadius[d].Add(evenR);
                    results.Add(evenR);
                }
            }

            return results;
        }

        public static List<Hex> HexesWithinCircularRing(Map map, Hex origin, double outerRadius, double innerRadius, out List<Hex>[] hexesByDistanceFromInnerRadius)
        {
            if (outerRadius < innerRadius)
            {
                hexesByDistanceFromInnerRadius = new List<Hex>[0];
                return new List<Hex>();
            }

            int outerHexRadius = (int)Math.Ceiling(outerRadius);
            int innerHexRadius = (int)Math.Floor(innerRadius);
            if (innerRadius < 1f)
            {
                innerHexRadius = 0;
            }

            int outerCount = 1 + 3 * outerHexRadius * (outerHexRadius + 1);
            int innerCount = 1 + 3 * innerHexRadius * (innerHexRadius + 1);
            List<Hex> results = new List<Hex>(outerCount - innerCount);

            int ringWidth = outerHexRadius - innerHexRadius;
            hexesByDistanceFromInnerRadius = new List<Hex>[ringWidth + 1];

            List<Vector3Int>[] cubeCoordinatesByDistanceFromInnerRadius;
            if (CubeCoordinatesWithinCircularRing(map, HexToCube(origin), outerRadius, innerRadius, out cubeCoordinatesByDistanceFromInnerRadius).Count == 0)
            {
                return results;
            }

            for (int d = 0; d < cubeCoordinatesByDistanceFromInnerRadius.Length; d++)
            {
                int dist = innerHexRadius + d;
                hexesByDistanceFromInnerRadius[d] = new List<Hex>(dist == 0 ? 1 : 6 * dist);
                foreach (Vector3Int cube in cubeCoordinatesByDistanceFromInnerRadius[d])
                {
                    Vector2Int evenR = CubeToEvenR(cube);
                    for (int layer = 0; layer < map.grid.Length; layer++)
                    {
                        Hex hex = map.grid[layer][evenR.x][evenR.y];
                        if (hex != null)
                        {
                            hexesByDistanceFromInnerRadius[d].Add(hex);
                            results.Add(hex);
                        }
                    }
                }
            }

            return results;
        }

        public static List<Hex> HexesWithinCircularRing(Map map, Hex origin, double outerRadius, double innerRadius, int mapLayer, out List<Hex>[] hexesByDistanceFromInnerRadius)
        {
            if (outerRadius < innerRadius)
            {
                hexesByDistanceFromInnerRadius = new List<Hex>[0];
                return new List<Hex>();
            }

            int outerHexRadius = (int)Math.Ceiling(outerRadius);
            int innerHexRadius = (int)Math.Floor(innerRadius);
            if (innerRadius < 1f)
            {
                innerHexRadius = 0;
            }

            int outerCount = 1 + 3 * outerHexRadius * (outerHexRadius + 1);
            int innerCount = 1 + 3 * innerHexRadius * (innerHexRadius + 1);
            List<Hex> results = new List<Hex>(outerCount - innerCount);

            int ringWidth = outerHexRadius - innerHexRadius;
            hexesByDistanceFromInnerRadius = new List<Hex>[ringWidth + 1];

            List<Vector3Int>[] cubeCoordinatesByDistanceFromInnerRadius;
            if (CubeCoordinatesWithinCircularRing(map, HexToCube(origin), outerRadius, innerRadius, out cubeCoordinatesByDistanceFromInnerRadius).Count == 0)
            {
                return results;
            }

            for (int d = 0; d < cubeCoordinatesByDistanceFromInnerRadius.Length; d++)
            {
                int dist = innerHexRadius + d;
                hexesByDistanceFromInnerRadius[d] = new List<Hex>(dist == 0 ? 1 : 6 * dist);
                foreach (var cube in cubeCoordinatesByDistanceFromInnerRadius[d])
                {
                    Vector2Int evenR = CubeToEvenR(cube);
                    Hex hex = map.grid[mapLayer][evenR.x][evenR.y];
                    if (hex != null)
                    {
                        hexesByDistanceFromInnerRadius[d].Add(hex);
                        results.Add(hex);
                    }
                }
            }

            return results;
        }

        public static List<Hex> HexesWithinCircularRing(Map map, Hex origin, double outerRadius, double innerRadius, List<int> mapLayers, out List<Hex>[] hexesByDistanceFromInnerRadius)
        {
            if (outerRadius < innerRadius)
            {
                hexesByDistanceFromInnerRadius = new List<Hex>[0];
                return new List<Hex>();
            }

            int outerHexRadius = (int)Math.Ceiling(outerRadius);
            int innerHexRadius = (int)Math.Floor(innerRadius);
            if (innerRadius < 1f)
            {
                innerHexRadius = 0;
            }

            int outerCount = 1 + 3 * outerHexRadius * (outerHexRadius + 1);
            int innerCount = 1 + 3 * innerHexRadius * (innerHexRadius + 1);
            List<Hex> results = new List<Hex>(outerCount - innerCount);

            int ringWidth = outerHexRadius - innerHexRadius;
            hexesByDistanceFromInnerRadius = new List<Hex>[ringWidth + 1];

            List<Vector3Int>[] cubeCoordinatesByDistanceFromInnerRadius;
            if (CubeCoordinatesWithinCircularRing(map, HexToCube(origin), outerRadius, innerRadius, out cubeCoordinatesByDistanceFromInnerRadius).Count == 0)
            {
                return results;
            }

            for (int d = 0; d < cubeCoordinatesByDistanceFromInnerRadius.Length; d++)
            {
                int dist = innerHexRadius + d;
                hexesByDistanceFromInnerRadius[d] = new List<Hex>(dist == 0 ? 1 : 6 * dist);
                foreach (var cube in cubeCoordinatesByDistanceFromInnerRadius[d])
                {
                    Vector2Int evenR = CubeToEvenR(cube);
                    foreach (int layer in mapLayers)
                    {
                        Hex hex = map.grid[layer][evenR.x][evenR.y];
                        if (hex != null)
                        {
                            hexesByDistanceFromInnerRadius[d].Add(hex);
                            results.Add(hex);
                        }
                    }
                }
            }

            return results;
        }
        #endregion

        #region HexesWithinLine
        public static Vector3 CubeLerp(Vector3Int start, Vector3Int end, float t)
        {
            return new Vector3(Mathf.Lerp(start.x, end.x, t), Mathf.Lerp(start.y, end.y, t), Mathf.Lerp(start.z, end.z, t));
        }

        public static List<Vector3Int> CubeCoordinatesWithinLineNaive(Vector3Int start, Vector3Int end)
        {
            int distance = HexDistanceCube(start, end);
            List<Vector3Int> results = new List<Vector3Int>(distance + 1);
            if (distance == 0)
            {
                results.Add(start);
                return results;
            }

            float stepSize = 1f / distance;
            for (int i = 0; i <= distance; i++)
            {
                Vector3 cubeStep = CubeLerp(start, end, stepSize * i);
                Vector3Int cube = RoundCubeCoordinate(cubeStep);
                results.Add(cube);
            }

            return results;
        }

        public static List<Vector3Int> CubeCoordinatesWithinLine(Map map, Vector3Int start, Vector3Int end, bool includeOutOfBoundsCoordinates = false)
        {
            if (includeOutOfBoundsCoordinates)
            {
                return CubeCoordinatesWithinLineNaive(start, end);
            }

            int distance = HexDistanceCube(start, end);
            List<Vector3Int> results = new List<Vector3Int>(distance + 1);
            if (distance == 0)
            {
                results.Add(start);
                return results;
            }

            float stepSize = 1f / distance;
            for (int i = 0; i <= distance; i++)
            {
                Vector3 cubeStep = CubeLerp(start, end, stepSize * i);
                Vector3Int cube = RoundCubeCoordinate(cubeStep);
                Vector2Int evenR = CubeToEvenR(cube);
                if (evenR.x >= 0 && evenR.x < map.sizeX && evenR.y >= 0 && evenR.y < map.sizeY)
                {
                    results.Add(cube);
                }
            }

            return results;
        }

        public static List<Vector2Int> AxialCoordinatesWithinLine(Map map, Vector2Int start, Vector2Int end, bool includeOutOfBoundsCoordinates = false)
        {
            int distance = HexDistanceAxial(start, end);
            List<Vector2Int> results = new List<Vector2Int>(distance + 1);
            if (distance == 0)
            {
                results.Add(start);
                return results;
            }

            List<Vector3Int> cubeCoordinatesWithinLine = CubeCoordinatesWithinLine(map, AxialToCube(start), AxialToCube(end), includeOutOfBoundsCoordinates);
            foreach (Vector3Int cube in cubeCoordinatesWithinLine)
            {
                Vector2Int axial = CubeToAxial(cube);
                results.Add(axial);
            }

            return results;
        }

        public static List<Vector2Int> EvenRCoordinatesWithinLine(Map map, Vector2Int start, Vector2Int end, bool includeOutOfBoundsCoordinates = false)
        {
            int distance = HexDistanceEvenR(start, end);
            List<Vector2Int> results = new List<Vector2Int>(distance + 1);
            if (distance == 0)
            {
                results.Add(start);
                return results;
            }

            List<Vector3Int> cubeCoordinatesWithinLine = CubeCoordinatesWithinLine(map, EvenRToCube(start), EvenRToCube(end), includeOutOfBoundsCoordinates);
            foreach (Vector3Int cube in cubeCoordinatesWithinLine)
            {
                Vector2Int evenR = CubeToEvenR(cube);
                results.Add(evenR);
            }

            return results;
        }

        public static List<Hex> HexesWithinLine(Map map, Hex start, Hex end)
        {
            int distance = HexDistance(start, end);
            List<Hex> results = new List<Hex>(distance + 1);
            if (distance == 0)
            {
                results.Add(start);
                return results;
            }

            List<Vector2Int> evenRCoordinatesWithinLine = EvenRCoordinatesWithinLine(map, new Vector2Int(start.x, start.y), new Vector2Int(end.x, end.y));
            foreach(Vector2Int evenR in evenRCoordinatesWithinLine)
            {
                for (int layer = 0; layer < map.grid.Length; layer++)
                {
                    Hex hex = map.grid[layer][evenR.x][evenR.y];

                    if (hex != null)
                    {
                        results.Add(hex);
                    }
                }
            }

            return results;
        }

        public static List<Hex> HexesWithinLine(Map map, Hex start, Hex end, int mapLayer)
        {
            if (mapLayer < 0 || mapLayer >= map.grid.Length)
            {
                return new List<Hex>();
            }

            int distance = HexDistance(start, end);
            List<Hex> results = new List<Hex>(distance + 1);
            if (distance == 0)
            {
                results.Add(start);
                return results;
            }

            if (mapLayer >= 0 && mapLayer < map.grid.Length)
            {
                List<Vector2Int> evenRCoordinatesWithinLine = EvenRCoordinatesWithinLine(map, new Vector2Int(start.x, start.y), new Vector2Int(end.x, end.y));
                foreach (Vector2Int evenR in evenRCoordinatesWithinLine)
                {
                    Hex hex = map.grid[mapLayer][evenR.x][evenR.y];
                    if (hex != null)
                    {
                        results.Add(hex);
                    }
                }
            }

            return results;
        }

        public static List<Hex> HexesWithinLine(Map map, Hex start, Hex end, List<int> mapLayers)
        {
            if (mapLayers == null || mapLayers.Count == 0)
            {
                return new List<Hex>();
            }

            int distance = HexDistance(start, end);
            List<Hex> results = new List<Hex>(distance + 1);
            if (distance == 0)
            {
                results.Add(start);
                return results;
            }

            List<Vector2Int> evenRCoordinatesWithinLine = EvenRCoordinatesWithinLine(map, new Vector2Int(start.x, start.y), new Vector2Int(end.x, end.y));
            foreach (Vector2Int evenR in evenRCoordinatesWithinLine)
            {
                foreach (int mapLayer in mapLayers.Distinct())
                {
                    Hex hex = map.grid[mapLayer][evenR.x][evenR.y];
                    if (hex != null)
                    {
                        results.Add(hex);
                    }
                }
            }

            return results;
        }
        #endregion
    }
}
