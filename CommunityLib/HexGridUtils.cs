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

        public const float Epsilon = 0.1f;
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
        public static List<Vector3Int> CubeCoordinatesWithinRadius(Map map, Vector3Int origin, int radius, out List<Vector3Int>[] cubeCoordinatesByDistance, bool includeOutOfBoundsCoordinates = false)
        {
            List<Vector3Int> results = new List<Vector3Int>(1 + 3 * radius * (radius + 1));
            cubeCoordinatesByDistance = new List<Vector3Int>[radius + 1];

            for (int d = 0; d < cubeCoordinatesByDistance.Length; d++)
            {
                cubeCoordinatesByDistance[d] = new List<Vector3Int>(d == 0 ? 1 : 6 * d);
            }

            int minDY;
            int maxDY;
            if (includeOutOfBoundsCoordinates)
            {
                minDY = -radius;
                maxDY = radius;
            }
            else
            {
                minDY = Math.Max(-origin.y, -radius);
                maxDY = Math.Min(map.sizeY - 1 - origin.y, radius);
            }

            for (int dy = minDY; dy <= maxDY; dy++)
            {
                int y = origin.y + dy;

                int xOffset = (y + (y & 1)) / 2;

                int minDX;
                int maxDX;
                if (includeOutOfBoundsCoordinates)
                {
                    minDX = -radius;
                    maxDX = radius;
                }
                else
                {
                    minDX = Math.Max(-origin.x - xOffset, -radius);
                    maxDX = Math.Min(map.sizeX - 1 - xOffset - origin.x, radius);
                }

                for (int dx = minDX; dx <= maxDX; dx++)
                {
                    int x = origin.x + dx;

                    int dz = -dx - dy;
                    int z = origin.z + dz;
                    int dist = Math.Max(Math.Abs(dx), Math.Max(Math.Abs(dy), Math.Abs(dz)));
                    if (dist <= radius)
                    {
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
            List<Vector3Int> cubeResults = CubeCoordinatesWithinRadius(map, AxialToCube(origin), radius, out List<Vector3Int>[] cubeCoordinatesByDistance, includeOutOfBoundsCoordinates);
            if (cubeResults.Count == 0)
            {
                axialCoordinatesByDistance = new List<Vector2Int>[0];
                return new List<Vector2Int>();
            }

            List<Vector2Int> results = new List<Vector2Int>(cubeResults.Count);
            axialCoordinatesByDistance = new List<Vector2Int>[cubeCoordinatesByDistance.Length];

            for (int d = 0; d < cubeCoordinatesByDistance.Length; d++)
            {
                axialCoordinatesByDistance[d] = new List<Vector2Int>(cubeCoordinatesByDistance[d].Count);
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
            List<Vector3Int> cubeResults = CubeCoordinatesWithinRadius(map, EvenRToCube(origin), radius, out List<Vector3Int>[] cubeCoordinatesByDistance, includeOutOfBoundsCoordinates);
            if (cubeResults.Count == 0)
            {
                evenRCoordinatesByDistance = new List<Vector2Int>[0];
                return new List<Vector2Int>();
            }

            List<Vector2Int> results = new List<Vector2Int>(cubeResults.Count);
            evenRCoordinatesByDistance = new List<Vector2Int>[cubeCoordinatesByDistance.Length];
            for (int d = 0; d < cubeCoordinatesByDistance.Length; d++)
            {
                evenRCoordinatesByDistance[d] = new List<Vector2Int>(cubeCoordinatesByDistance[d].Count);
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
            List<Vector3Int> cubeResults = CubeCoordinatesWithinRadius(map, HexToCube(origin), radius, out List<Vector3Int>[] cubeCoordinatesByDistance);
            if (cubeResults.Count == 0)
            {
                hexesByDistance = new List<Hex>[0];
                return new List<Hex>();
            }

            List<Hex> results = new List<Hex>(cubeResults.Count * map.grid.Length);
            hexesByDistance = new List<Hex>[cubeCoordinatesByDistance.Length];
            for (int d = 0; d < cubeCoordinatesByDistance.Length; d++)
            {
                hexesByDistance[d] = new List<Hex>(cubeCoordinatesByDistance[d].Count * map.grid.Length);
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
            if (mapLayer < 0 || mapLayer >= map.grid.Length)
            {
                hexesByDistance = new List<Hex>[0];
                return new List<Hex>();
            }

            List<Vector3Int> cubeResults = CubeCoordinatesWithinRadius(map, HexToCube(origin), radius, out List<Vector3Int>[] cubeCoordinatesByDistance);
            if (cubeResults.Count == 0)
            {
                hexesByDistance = new List<Hex>[0];
                return new List<Hex>();
            }

            List<Hex> results = new List<Hex>(cubeResults.Count);
            hexesByDistance = new List<Hex>[cubeCoordinatesByDistance.Length];
            for (int d = 0; d < cubeCoordinatesByDistance.Length; d++)
            {
                hexesByDistance[d] = new List<Hex>(cubeCoordinatesByDistance[d].Count);
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
            if (mapLayers == null || mapLayers.Count == 0 || mapLayers.All(mapLayer => mapLayer < 0 || mapLayer >= map.grid.Length))
            {
                hexesByDistance = new List<Hex>[0];
                return new List<Hex>();
            }

            List<Vector3Int> cubeResults = CubeCoordinatesWithinRadius(map, HexToCube(origin), radius, out List<Vector3Int>[] cubeCoordinatesByDistance);
            if (cubeResults.Count == 0)
            {
                hexesByDistance = new List<Hex>[0];
                return new List<Hex>();
            }

            List<Hex> results = new List<Hex>(cubeResults.Count * mapLayers.Count);
            hexesByDistance = new List<Hex>[cubeCoordinatesByDistance.Length];
            for (int d = 0; d < cubeCoordinatesByDistance.Length; d++)
            {
                hexesByDistance[d] = new List<Hex>(cubeCoordinatesByDistance[d].Count * mapLayers.Count);
                foreach (Vector3Int cube in cubeCoordinatesByDistance[d])
                {
                    Vector2Int evenR = CubeToEvenR(cube);
                    foreach (int mapLayer in mapLayers.Distinct())
                    {
                        if (mapLayer < 0 || mapLayer >= map.grid.Length)
                        {
                            continue;
                        }

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

            for (int d = 0; d < cubeCoordinatesByDistanceFromInnerRadius.Length; d++)
            {
                int dist = innerRadius + d;
                cubeCoordinatesByDistanceFromInnerRadius[d] = new List<Vector3Int>(dist == 0 ? 1 : 6 * dist);
            }

            int minDY;
            int maxDY;
            if (includeOutOfBoundsCoordinates)
            {
                minDY = outerRadius;
                maxDY = outerRadius;
            }
            else
            {
                minDY = Math.Max(-origin.y, -outerRadius);
                maxDY = Math.Min(map.sizeY - 1 - origin.y, outerRadius);
            }

            for (int dy = Math.Max(-outerRadius, minDY); dy <= Math.Min(outerRadius, maxDY); dy++)
            {
                int y = origin.y + dy;
                int xOffset = (y + (y & 1)) / 2;
                int minDX;
                int maxDX;
                if (includeOutOfBoundsCoordinates)
                {
                    minDX = -outerRadius;
                    maxDX = outerRadius;
                }
                else
                {
                    minDX = Math.Max(-origin.x, -outerRadius);
                    maxDX = Math.Min(map.sizeX - 1 - origin.x, outerRadius);
                }

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

            List<Vector3Int> cubeResults = CubeCoordinatesWithinRing(map, AxialToCube(origin), outerRadius, innerRadius, out List<Vector3Int>[] cubeCoordinatesByDistanceFromInnerRadius, includeOutOfBoundsCoordinates);
            if (cubeResults.Count == 0)
            {
                axialCoordinatesByDistanceFromInnerRadius = new List<Vector2Int>[0];
                return new List<Vector2Int>();
            }

            List<Vector2Int> results = new List<Vector2Int>(cubeResults.Count);
            axialCoordinatesByDistanceFromInnerRadius = new List<Vector2Int>[cubeCoordinatesByDistanceFromInnerRadius.Length];
            for (int d = 0; d < cubeCoordinatesByDistanceFromInnerRadius.Length; d++)
            {
                axialCoordinatesByDistanceFromInnerRadius[d] = new List<Vector2Int>(cubeCoordinatesByDistanceFromInnerRadius[d].Count);
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

            List<Vector3Int> cubeResults = CubeCoordinatesWithinRing(map, EvenRToCube(origin), outerRadius, innerRadius, out List<Vector3Int>[] cubeCoordinatesByDistanceFromInnerRadius, includeOutOfBoundsCoordinates);
            if (cubeResults.Count == 0)
            {
                evenRCoordinatesByDistanceFromInnerRadius = new List<Vector2Int>[0];
                return new List<Vector2Int>();
            }

            List<Vector2Int> results = new List<Vector2Int>(cubeResults.Count);
            evenRCoordinatesByDistanceFromInnerRadius = new List<Vector2Int>[cubeCoordinatesByDistanceFromInnerRadius.Length];
            for (int d = 0; d < cubeCoordinatesByDistanceFromInnerRadius.Length; d++)
            {
                evenRCoordinatesByDistanceFromInnerRadius[d] = new List<Vector2Int>(cubeCoordinatesByDistanceFromInnerRadius[d].Count);
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

            List<Vector3Int> cubeResults = CubeCoordinatesWithinRing(map, HexToCube(origin), outerRadius, innerRadius, out List<Vector3Int>[] cubeCoordinatesByDistanceFromInnerRadius);
            if (cubeResults.Count == 0)
            {
                hexesByDistanceFromInnerRadius = new List<Hex>[0];
                return new List<Hex>();
            }

            List<Hex> results = new List<Hex>(cubeResults.Count * map.grid.Length);
            hexesByDistanceFromInnerRadius = new List<Hex>[cubeCoordinatesByDistanceFromInnerRadius.Length];
            for (int d = 0; d < cubeCoordinatesByDistanceFromInnerRadius.Length; d++)
            {
                hexesByDistanceFromInnerRadius[d] = new List<Hex>(cubeCoordinatesByDistanceFromInnerRadius[d].Count * map.grid.Length);
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

            List<Vector3Int> cubeResults = CubeCoordinatesWithinRing(map, HexToCube(origin), outerRadius, innerRadius, out List<Vector3Int>[] cubeCoordinatesByDistanceFromInnerRadius);
            if (cubeResults.Count == 0)
            {
                hexesByDistanceFromInnerRadius = new List<Hex>[0];
                return new List<Hex>();
            }

            List<Hex> results = new List<Hex>(cubeResults.Count);
            hexesByDistanceFromInnerRadius = new List<Hex>[cubeCoordinatesByDistanceFromInnerRadius.Length];
            for (int d = 0; d < cubeCoordinatesByDistanceFromInnerRadius.Length; d++)
            {
                hexesByDistanceFromInnerRadius[d] = new List<Hex>(cubeCoordinatesByDistanceFromInnerRadius[d].Count);
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

            if (mapLayers == null || mapLayers.Count == 0 || mapLayers.All(mapLayer => mapLayer < 0 || mapLayer >= map.grid.Length))
            {
                hexesByDistanceFromInnerRadius = new List<Hex>[0];
                return new List<Hex>();
            }

            List<Vector3Int> cubeResults = CubeCoordinatesWithinRing(map, HexToCube(origin), outerRadius, innerRadius, out List<Vector3Int>[] cubeCoordinatesByDistanceFromInnerRadius);
            if (cubeResults.Count == 0)
            {
                hexesByDistanceFromInnerRadius = new List<Hex>[0];
                return new List<Hex>();
            }

            List<Hex> results = new List<Hex>(cubeResults.Count * mapLayers.Count);
            hexesByDistanceFromInnerRadius = new List<Hex>[cubeCoordinatesByDistanceFromInnerRadius.Length];
            for (int d = 0; d < cubeCoordinatesByDistanceFromInnerRadius.Length; d++)
            {
                hexesByDistanceFromInnerRadius[d] = new List<Hex>(cubeCoordinatesByDistanceFromInnerRadius[d].Count * mapLayers.Count);
                foreach (var cube in cubeCoordinatesByDistanceFromInnerRadius[d])
                {
                    Vector2Int evenR = CubeToEvenR(cube);
                    foreach (int mapLayer in mapLayers.Distinct())
                    {
                        if (mapLayer < 0 || mapLayer >= map.grid.Length)
                        {
                            continue;
                        }

                        Hex hex = map.grid[mapLayer][evenR.x][evenR.y];
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
        public static List<Vector3Int> CubeCoordinatesWithinCircle(Map map, Vector3Int origin, double radius, out List<Vector3Int>[] cubeCoordinatesByDistance, bool skipDistanceBucketing = true, bool includeOutOfBoundsCoordinates = false)
        {
            double hexRadius = radius;
            radius *= HexWidth;
            double verticalSpacing = HexHeight;
            if (radius < verticalSpacing)
            {
                cubeCoordinatesByDistance = new List<Vector3Int>[1];
                cubeCoordinatesByDistance[0] = new List<Vector3Int> { origin };
                return new List<Vector3Int> { origin };
            }

            List<Vector2Int> evenRResults = EvenRCoordinatesWithinCircle(map, CubeToEvenR(origin), hexRadius, out List<Vector2Int>[] evenRCoordinatesByDistance, includeOutOfBoundsCoordinates, skipDistanceBucketing);
            if (evenRResults.Count == 0)
            {
                cubeCoordinatesByDistance = new List<Vector3Int>[0];
                return new List<Vector3Int>();
            }

            List<Vector3Int> results = new List<Vector3Int>(evenRResults.Count);
            cubeCoordinatesByDistance = new List<Vector3Int>[evenRCoordinatesByDistance.Length];
            for (int d = 0; d < evenRCoordinatesByDistance.Length; d++)
            {
                cubeCoordinatesByDistance[d] = new List<Vector3Int>(evenRCoordinatesByDistance[d].Count);
                foreach (Vector2Int evenR in evenRCoordinatesByDistance[d])
                {
                    Vector3Int axial = EvenRToCube(evenR);
                    cubeCoordinatesByDistance[d].Add(axial);
                    results.Add(axial);
                }
            }

            return results;
        }

        public static List<Vector2Int> AxialCoordinatesWithinCircle(Map map, Vector2Int origin, double radius, out List<Vector2Int>[] axialCoordinatesByDistance, bool skipDistanceBucketing = true, bool includeOutOfBoundsCoordinates = false)
        {
            double hexRadius = radius;
            radius *= HexWidth;
            double verticalSpacing = HexHeight;
            if (radius < verticalSpacing)
            {
                axialCoordinatesByDistance = new List<Vector2Int>[1];
                axialCoordinatesByDistance[0] = new List<Vector2Int> { origin };
                return new List<Vector2Int> { origin };
            }

            List<Vector2Int> evenRResults = EvenRCoordinatesWithinCircle(map, AxialToEvenR(origin), hexRadius, out List<Vector2Int>[] evenRCoordinatesByDistance, includeOutOfBoundsCoordinates, skipDistanceBucketing);
            if (evenRResults.Count == 0)
            {
                axialCoordinatesByDistance = new List<Vector2Int>[0];
                return new List<Vector2Int>();
            }

            List<Vector2Int> results = new List<Vector2Int>(evenRResults.Count);
            axialCoordinatesByDistance = new List<Vector2Int>[evenRCoordinatesByDistance.Length];
            for (int d = 0; d < evenRCoordinatesByDistance.Length; d++)
            {
                axialCoordinatesByDistance[d] = new List<Vector2Int>(evenRCoordinatesByDistance[d].Count);
                foreach (Vector2Int evenR in evenRCoordinatesByDistance[d])
                {
                    Vector2Int axial = EvenRToAxial(evenR);
                    axialCoordinatesByDistance[d].Add(axial);
                    results.Add(axial);
                }
            }

            return results;
        }

        public static List<Vector2Int> EvenRCoordinatesWithinCircle(Map map, Vector2Int origin, double radius, out List<Vector2Int>[] evenRCoordinatesByDistance, bool skipDistanceBucketing = true, bool includeOutOfBoundsCoordinates = false)
        {
            double verticalSpacing = HexHeight * 0.75;
            if (radius < verticalSpacing)
            {
                evenRCoordinatesByDistance = new List<Vector2Int>[1];
                evenRCoordinatesByDistance[0] = new List<Vector2Int> { origin };
                return new List<Vector2Int> { origin };
            }

            int rowOffset = (int)Math.Ceiling(radius / verticalSpacing);
            int colOffset = (int)Math.Ceiling(radius / HexWidth) + 1;

            int minDY;
            int maxDY;
            int minDX;
            int maxDX;
            if (includeOutOfBoundsCoordinates)
            {
                minDY = -rowOffset;
                maxDY = rowOffset;
                minDX = -colOffset;
                maxDX = colOffset;
            }
            else
            {
                minDY = Math.Max(-origin.y, -rowOffset);
                maxDY = Math.Min(map.sizeY - 1 - origin.y, rowOffset);
                minDX = Math.Max(-origin.x, -colOffset);
                maxDX = Math.Min(map.sizeX - 1 - origin.x, colOffset);
            }
            double squareRadius = radius * radius;

            List<Vector2Int> results = new List<Vector2Int>(1 + 3 * (int)Math.Ceiling(radius) * ((int)Math.Ceiling(radius) + 1));
            if (skipDistanceBucketing)
            {
                evenRCoordinatesByDistance = new List<Vector2Int>[0];
            }
            else
            {
                evenRCoordinatesByDistance = new List<Vector2Int>[(int)Math.Ceiling(radius) + 1];
                for (int d = 0; d < evenRCoordinatesByDistance.Length; d++)
                {
                    evenRCoordinatesByDistance[d] = new List<Vector2Int>(d == 0 ? 1 : (int)Math.Ceiling((2 * Math.PI * d) / HexWidth));
                }
            }

            for (int dy = minDY; dy <= maxDY; dy++)
            {
                for (int dx = minDX; dx <= maxDX; dx++)
                {
                    bool inside = false;
                    Vector2Int evenR = new Vector2Int(origin.x + dx, origin.y + dy);
                    double dist = EuclideanSquareDistanceEvenR(origin, evenR);
                    if (dist <= squareRadius + Epsilon)
                    {
                        inside = true;

                        if (!skipDistanceBucketing)
                        {
                            evenRCoordinatesByDistance[(int)Math.Floor(Math.Sqrt(dist))].Add(evenR);
                        }
                        results.Add(evenR);
                    }
                    else if (inside)
                    {
                        break;
                    }
                }
            }

            return results;
        }

        public static List<Hex> HexesWithinCircle(Map map, Hex origin, double radius, out List<Hex>[] hexesByDistance, bool skipDistanceBucketing = true)
        {
            double hexRadius = radius;
            radius *= HexWidth;
            double verticalSpacing = HexHeight;
            if (radius < verticalSpacing)
            {
                hexesByDistance = new List<Hex>[1];
                hexesByDistance[0] = new List<Hex>(map.grid.Length);
                List<Hex> earlyResults = new List<Hex>(map.grid.Length);

                for (int mapLayer = 0; mapLayer < map.grid.Length; mapLayer++)
                {
                    Hex hex = map.grid[mapLayer][origin.x][origin.y];
                    if (hex != null)
                    {
                        hexesByDistance[0].Add(hex);
                        earlyResults.Add(hex);
                    }
                }

                return earlyResults;
            }

            List<Vector2Int> evenRResults = EvenRCoordinatesWithinCircle(map, HexToEvenR(origin), hexRadius, out List<Vector2Int>[] evenRCoordinatesByDistance, skipDistanceBucketing);
            if (evenRResults.Count == 0)
            {
                hexesByDistance = new List<Hex>[0];
                return new List<Hex>();
            }

            List<Hex> results = new List<Hex>(evenRResults.Count * map.grid.Length);
            if (skipDistanceBucketing)
            {
                hexesByDistance = new List<Hex>[0];
                foreach (Vector2Int evenR in evenRResults)
                {
                    for (int mapLayer = 0; mapLayer < map.grid.Length; mapLayer++)
                    {
                        Hex hex = map.grid[mapLayer][evenR.x][evenR.y];
                        if (hex != null)
                        {
                            results.Add(hex);
                        }
                    }
                }
            }
            else
            {
                hexesByDistance = new List<Hex>[evenRCoordinatesByDistance.Length];
                for (int d = 0; d < evenRCoordinatesByDistance.Length; d++)
                {
                    hexesByDistance[d] = new List<Hex>(evenRCoordinatesByDistance[d].Count * map.grid.Length);
                    foreach (Vector2Int evenR in evenRCoordinatesByDistance[d])
                    {
                        for (int mapLayer = 0; mapLayer < map.grid.Length; mapLayer++)
                        {
                            Hex hex = map.grid[mapLayer][evenR.x][evenR.y];
                            if (hex != null)
                            {
                                hexesByDistance[d].Add(hex);
                                results.Add(hex);
                            }
                        }
                    }
                }
            }
            
            return results;
        }

        public static List<Hex> HexesWithinCircle(Map map, Hex origin, double radius, int mapLayer, out List<Hex>[] hexesByDistance, bool skipDistanceBucketing = true)
        {
            if (mapLayer < 0 || mapLayer > map.grid.Length)
            {
                hexesByDistance = new List<Hex>[0];
                return new List<Hex>();
            }

            double hexRadius = radius;
            radius *= HexWidth;
            double verticalSpacing = HexHeight;
            if (radius < verticalSpacing)
            {
                hexesByDistance = new List<Hex>[1];
                hexesByDistance[0] = new List<Hex>(1);
                List<Hex> earlyResults = new List<Hex>(1);

                Hex hex = map.grid[mapLayer][origin.x][origin.y];
                if (hex != null)
                {
                    hexesByDistance[0].Add(hex);
                    earlyResults.Add(hex);
                }

                return earlyResults;
            }

            List<Vector2Int> evenRResults = EvenRCoordinatesWithinCircle(map, HexToEvenR(origin), hexRadius, out List<Vector2Int>[] evenRCoordinatesByDistance, skipDistanceBucketing);
            if (evenRResults.Count == 0)
            {
                hexesByDistance = new List<Hex>[0];
                return new List<Hex>();
            }

            List<Hex> results = new List<Hex>(evenRResults.Count * map.grid.Length);
            if (skipDistanceBucketing)
            {
                hexesByDistance = new List<Hex>[0];
                foreach (Vector2Int evenR in evenRResults)
                {
                    Hex hex = map.grid[mapLayer][evenR.x][evenR.y];
                    if (hex != null)
                    {
                        results.Add(hex);
                    }
                }
            }
            else
            {
                hexesByDistance = new List<Hex>[evenRCoordinatesByDistance.Length];
                for (int d = 0; d < evenRCoordinatesByDistance.Length; d++)
                {
                    hexesByDistance[d] = new List<Hex>(evenRCoordinatesByDistance[d].Count * map.grid.Length);
                    foreach (Vector2Int evenR in evenRCoordinatesByDistance[d])
                    {
                        Hex hex = map.grid[mapLayer][evenR.x][evenR.y];
                        if (hex != null)
                        {
                            hexesByDistance[d].Add(hex);
                            results.Add(hex);
                        }
                    }
                }
            }

            return results;
        }

        public static List<Hex> HexesWithinCircle(Map map, Hex origin, double radius, List<int> mapLayers, out List<Hex>[] hexesByDistance, bool skipDistanceBucketing = true)
        {
            if (mapLayers == null || mapLayers.Count == 0 || mapLayers.All(mapLayer => mapLayer < 0 || mapLayer >= map.grid.Length))
            {
                hexesByDistance = new List<Hex>[0];
                return new List<Hex>();
            }

            double hexRadius = radius;
            radius *= HexWidth;
            double verticalSpacing = HexHeight;
            if (radius < verticalSpacing)
            {
                hexesByDistance = new List<Hex>[1];
                hexesByDistance[0] = new List<Hex>(2);
                List<Hex> earlyResults = new List<Hex>(2);

                foreach (int mapLayer in mapLayers)
                {
                    Hex hex = map.grid[mapLayer][origin.x][origin.y];
                    if (hex != null)
                    {
                        hexesByDistance[0].Add(hex);
                        earlyResults.Add(hex);
                    }
                }

                return earlyResults;
            }

            List<Vector2Int> evenRResults = EvenRCoordinatesWithinCircle(map, HexToEvenR(origin), hexRadius, out List<Vector2Int>[] evenRCoordinatesByDistance, skipDistanceBucketing);
            if (evenRResults.Count == 0)
            {
                hexesByDistance = new List<Hex>[0];
                return new List<Hex>();
            }

            List<Hex> results = new List<Hex>(evenRResults.Count * map.grid.Length);
            if (skipDistanceBucketing)
            {
                hexesByDistance = new List<Hex>[0];
                foreach (Vector2Int evenR in evenRResults)
                {
                    foreach (int mapLayer in mapLayers)
                    {
                        Hex hex = map.grid[mapLayer][evenR.x][evenR.y];
                        if (hex != null)
                        {
                            results.Add(hex);
                        }
                    }
                }
            }
            else
            {
                hexesByDistance = new List<Hex>[evenRCoordinatesByDistance.Length];
                for (int d = 0; d < evenRCoordinatesByDistance.Length; d++)
                {
                    hexesByDistance[d] = new List<Hex>(evenRCoordinatesByDistance[d].Count * map.grid.Length);
                    foreach (Vector2Int evenR in evenRCoordinatesByDistance[d])
                    {
                        foreach (int mapLayer in mapLayers)
                        {
                            Hex hex = map.grid[mapLayer][evenR.x][evenR.y];
                            if (hex != null)
                            {
                                hexesByDistance[d].Add(hex);
                                results.Add(hex);
                            }
                        }
                    }
                }
            }

            return results;
        }
        #endregion

        #region HexesWithinCircularRing
        public static List<Vector3Int> CubeCoordinatesWithinCircularRing(Map map, Vector3Int origin, double outerRadius, double innerRadius, out List<Vector3Int>[] cubeCoordinatesByDistanceFromInnerRadius, bool skipDistanceBucketing = true, bool includeOutOfBoundsCoordinates = false)
        {
            double verticalSpacing = HexHeight;
            if (innerRadius < verticalSpacing)
            {
                innerRadius = 0.0;
            }

            if (outerRadius < innerRadius)
            {
                cubeCoordinatesByDistanceFromInnerRadius = new List<Vector3Int>[0];
                return new List<Vector3Int>();
            }

            outerRadius *= HexWidth;
            innerRadius *= HexWidth;
            if (outerRadius < verticalSpacing)
            {
                if (innerRadius > 0.0)
                {
                    cubeCoordinatesByDistanceFromInnerRadius = new List<Vector3Int>[0];
                    return new List<Vector3Int>();
                }

                cubeCoordinatesByDistanceFromInnerRadius = new List<Vector3Int>[1];
                cubeCoordinatesByDistanceFromInnerRadius[0] = new List<Vector3Int> { origin };
                return new List<Vector3Int> { origin };
            }
            int roundedInnerRadius = (int)Math.Floor(innerRadius);

            List<Vector2Int> evenRResults = EvenRCoordinatesWithinCircularRing(map, CubeToEvenR(origin), outerRadius, innerRadius, out List<Vector2Int>[] evenRCoordinatesWithinCircularRing, skipDistanceBucketing, includeOutOfBoundsCoordinates);
            if (evenRResults.Count == 0)
            {
                cubeCoordinatesByDistanceFromInnerRadius = new List<Vector3Int>[0];
                return new List<Vector3Int>();
            }

            List<Vector3Int> results = new List<Vector3Int>(evenRResults.Count);
            cubeCoordinatesByDistanceFromInnerRadius = new List<Vector3Int>[evenRCoordinatesWithinCircularRing.Length];
            if (skipDistanceBucketing)
            {
                foreach (Vector2Int evenR in evenRResults)
                {
                    Vector3Int cube = EvenRToCube(evenR);
                    results.Add(cube);
                }
            }
            else
            {
                for (int d = 0; d < evenRCoordinatesWithinCircularRing.Length; d++)
                {
                    int dist = roundedInnerRadius + d;
                    cubeCoordinatesByDistanceFromInnerRadius[d] = new List<Vector3Int>(evenRCoordinatesWithinCircularRing[d].Count);
                    foreach (Vector2Int evenR in evenRCoordinatesWithinCircularRing[d])
                    {
                        Vector3Int cube = EvenRToCube(evenR);
                        cubeCoordinatesByDistanceFromInnerRadius[d].Add(cube);
                        results.Add(cube);
                    }
                }
            }

            return results;
        }

        public static List<Vector2Int> AxialCoordinatesWithinCircularRing(Map map, Vector2Int origin, double outerRadius, double innerRadius, out List<Vector2Int>[] axialCoordinatesByDistanceFromInnerRadius, bool skipDistanceBucketing = true, bool includeOutOfBoundsCoordinates = false)
        {
            double verticalSpacing = HexHeight;
            if (innerRadius < verticalSpacing)
            {
                innerRadius = 0.0;
            }

            if (outerRadius < innerRadius)
            {
                axialCoordinatesByDistanceFromInnerRadius = new List<Vector2Int>[0];
                return new List<Vector2Int>();
            }

            outerRadius *= HexWidth;
            innerRadius *= HexWidth;
            if (outerRadius < verticalSpacing)
            {
                if (innerRadius > 0.0)
                {
                    axialCoordinatesByDistanceFromInnerRadius = new List<Vector2Int>[0];
                    return new List<Vector2Int>();
                }

                axialCoordinatesByDistanceFromInnerRadius = new List<Vector2Int>[1];
                axialCoordinatesByDistanceFromInnerRadius[0] = new List<Vector2Int> { origin };
                return new List<Vector2Int> { origin };
            }
            int roundedInnerRadius = (int)Math.Floor(innerRadius);

            List<Vector2Int> evenRResults = EvenRCoordinatesWithinCircularRing(map, AxialToEvenR(origin), outerRadius, innerRadius, out List<Vector2Int>[] evenRCoordinatesWithinCircularRing, skipDistanceBucketing, includeOutOfBoundsCoordinates);
            if (evenRResults.Count == 0)
            {
                axialCoordinatesByDistanceFromInnerRadius = new List<Vector2Int>[0];
                return new List<Vector2Int>();
            }

            List<Vector2Int> results = new List<Vector2Int>(evenRResults.Count);
            axialCoordinatesByDistanceFromInnerRadius = new List<Vector2Int>[evenRCoordinatesWithinCircularRing.Length];
            if (skipDistanceBucketing)
            {
                foreach (Vector2Int evenR in evenRResults)
                {
                    Vector2Int axial = EvenRToAxial(evenR);
                    results.Add(axial);
                }
            }
            else
            {
                for (int d = 0; d < evenRCoordinatesWithinCircularRing.Length; d++)
                {
                    int dist = roundedInnerRadius + d;
                    axialCoordinatesByDistanceFromInnerRadius[d] = new List<Vector2Int>(evenRCoordinatesWithinCircularRing[d].Count);
                    foreach (Vector2Int evenR in evenRCoordinatesWithinCircularRing[d])
                    {
                        Vector2Int axial = EvenRToAxial(evenR);
                        axialCoordinatesByDistanceFromInnerRadius[d].Add(axial);
                        results.Add(axial);
                    }
                }
            }

            return results;
        }

        public static List<Vector2Int> EvenRCoordinatesWithinCircularRing(Map map, Vector2Int origin, double outerRadius, double innerRadius, out List<Vector2Int>[] evenRCoordinatesByDistanceFromInnerRadius, bool skipDistanceBucketing = true, bool includeOutOfBoundsCoordinates = false)
        {
            double verticalSpacing = HexHeight * 0.75;
            if (innerRadius < verticalSpacing)
            {
                innerRadius = 0.0;
            }

            if (outerRadius < innerRadius)
            {
                evenRCoordinatesByDistanceFromInnerRadius = new List<Vector2Int>[0];
                return new List<Vector2Int>();
            }

            if (outerRadius < verticalSpacing)
            {
                if (innerRadius > 0.0)
                {
                    evenRCoordinatesByDistanceFromInnerRadius = new List<Vector2Int>[0];
                    return new List<Vector2Int>();
                }

                evenRCoordinatesByDistanceFromInnerRadius = new List<Vector2Int>[1];
                evenRCoordinatesByDistanceFromInnerRadius[0] = new List<Vector2Int> { origin };
                return new List<Vector2Int> { origin };
            }

            int roundedOuterRadius = (int)Math.Ceiling(outerRadius);
            int roundedInnerRadius = (int)Math.Floor(innerRadius);
            double squareOuterRadius = outerRadius * outerRadius;
            double squareInnerRadius = innerRadius * innerRadius;

            List<Vector2Int> results = new List<Vector2Int>(1 + 3 * roundedOuterRadius * (roundedOuterRadius + 1));
            if (skipDistanceBucketing)
            {
                evenRCoordinatesByDistanceFromInnerRadius = new List<Vector2Int>[0];
            }
            else
            {
                evenRCoordinatesByDistanceFromInnerRadius = new List<Vector2Int>[(roundedOuterRadius - roundedInnerRadius) + 1];
                for (int d = 0; d < evenRCoordinatesByDistanceFromInnerRadius.Length; d++)
                {
                    evenRCoordinatesByDistanceFromInnerRadius[d] = new List<Vector2Int>(d == 0 ? 1 : (int)Math.Ceiling((2 * Math.PI * d) / HexWidth));
                }
            }

            List<Vector2Int> innerCircle = EvenRCoordinatesWithinCircle(map, origin, innerRadius - (2 * Epsilon), out _);
            List<Vector2Int> outerCircle = EvenRCoordinatesWithinCircle(map, origin, outerRadius, out _);

            foreach (Vector2Int evenR in outerCircle)
            {
                // Check if hex wis within the inner circle. If it is,remove it and skip. This reduces the search space for all following coordinates.
                if (innerCircle.Remove(evenR))
                {
                    continue;
                }

                if (!skipDistanceBucketing)
                {
                    double dist = EuclideanSquareDistanceEvenR(origin, evenR);
                    double bucket = Math.Sqrt(dist) - roundedInnerRadius;
                    if (bucket < 0)
                    {
                        bucket = 0;
                    }
                    evenRCoordinatesByDistanceFromInnerRadius[(int)bucket].Add(evenR);
                }
                results.Add(evenR);

            }

            return results;
        }

        public static List<Hex> HexesWithinCircularRing(Map map, Hex origin, double outerRadius, double innerRadius, out List<Hex>[] hexesByDistanceFromInnerRadius, bool skipDistanceBucketing = true)
        {
            double verticalSpacing = HexHeight;
            if (innerRadius < verticalSpacing)
            {
                innerRadius = 0.0;
            }

            if (outerRadius < innerRadius)
            {
                hexesByDistanceFromInnerRadius = new List<Hex>[0];
                return new List<Hex>();
            }

            outerRadius *= HexWidth;
            innerRadius *= HexWidth;
            if (outerRadius < verticalSpacing)
            {
                if (innerRadius > 0.0)
                {
                    hexesByDistanceFromInnerRadius = new List<Hex>[0];
                    return new List<Hex>();
                }

                hexesByDistanceFromInnerRadius = new List<Hex>[1];
                hexesByDistanceFromInnerRadius[0] = new List<Hex>(2);
                List<Hex> earlyResults = new List<Hex>(2);

                for (int mapLayer = 0; mapLayer < map.grid.Length; mapLayer++)
                {
                    Hex hex = map.grid[mapLayer][origin.x][origin.y];
                    hexesByDistanceFromInnerRadius[0].Add(hex);
                    earlyResults.Add(hex);
                }

                return earlyResults;
            }
            int roundedInnerRadius = (int)Math.Floor(innerRadius);

            List<Vector2Int> evenRResults = EvenRCoordinatesWithinCircularRing(map, HexToEvenR(origin), outerRadius, innerRadius, out List<Vector2Int>[] evenRCoordinatesWithinCircularRing, skipDistanceBucketing);
            if (evenRResults.Count == 0)
            {
                hexesByDistanceFromInnerRadius = new List<Hex>[0];
                return new List<Hex>();
            }

            List<Hex> results = new List<Hex>(evenRResults.Count * map.grid.Length);
            if (skipDistanceBucketing)
            {
                hexesByDistanceFromInnerRadius = new List<Hex>[0];
                foreach (Vector2Int evenR in evenRResults)
                {
                    for (int mapLayer = 0; mapLayer < map.grid.Length; mapLayer++)
                    {
                        Hex hex = map.grid[mapLayer][evenR.x][evenR.y];
                        results.Add(hex);
                    }
                }
            }
            else
            {
                hexesByDistanceFromInnerRadius = new List<Hex>[evenRCoordinatesWithinCircularRing.Length];
                for (int d = 0; d < evenRCoordinatesWithinCircularRing.Length; d++)
                {
                    int dist = roundedInnerRadius + d;
                    hexesByDistanceFromInnerRadius[d] = new List<Hex>(evenRCoordinatesWithinCircularRing[d].Count * map.grid.Length);
                    foreach (Vector2Int evenR in evenRCoordinatesWithinCircularRing[d])
                    {
                        for (int mapLayer = 0; mapLayer < map.grid.Length; mapLayer++)
                        {
                            Hex hex = map.grid[mapLayer][evenR.x][evenR.y];
                            hexesByDistanceFromInnerRadius[d].Add(hex);
                            results.Add(hex);
                        }
                    }
                }
            }

            return results;
        }

        public static List<Hex> HexesWithinCircularRing(Map map, Hex origin, double outerRadius, double innerRadius, int mapLayer, out List<Hex>[] hexesByDistanceFromInnerRadius, bool skipDistanceBucketing = true)
        {
            if (mapLayer < 0 || mapLayer >= map.grid.Length)
            {
                hexesByDistanceFromInnerRadius = new List<Hex>[0];
                return new List<Hex>();
            }

            double verticalSpacing = HexHeight;
            if (innerRadius < verticalSpacing)
            {
                innerRadius = 0.0;
            }

            if (outerRadius < innerRadius)
            {
                hexesByDistanceFromInnerRadius = new List<Hex>[0];
                return new List<Hex>();
            }

            outerRadius *= HexWidth;
            innerRadius *= HexWidth;
            if (outerRadius < verticalSpacing)
            {
                if (innerRadius > 0.0)
                {
                    hexesByDistanceFromInnerRadius = new List<Hex>[0];
                    return new List<Hex>();
                }

                hexesByDistanceFromInnerRadius = new List<Hex>[1];
                hexesByDistanceFromInnerRadius[0] = new List<Hex>(2);
                List<Hex> earlyResults = new List<Hex>(2);

                Hex hex = map.grid[mapLayer][origin.x][origin.y];
                hexesByDistanceFromInnerRadius[0].Add(hex);
                earlyResults.Add(hex);

                return earlyResults;
            }
            int roundedInnerRadius = (int)Math.Floor(innerRadius);

            List<Vector2Int> evenRResults = EvenRCoordinatesWithinCircularRing(map, HexToEvenR(origin), outerRadius, innerRadius, out List<Vector2Int>[] evenRCoordinatesWithinCircularRing, skipDistanceBucketing);
            if (evenRResults.Count == 0)
            {
                hexesByDistanceFromInnerRadius = new List<Hex>[0];
                return new List<Hex>();
            }

            List<Hex> results = new List<Hex>(evenRResults.Count);
            if (skipDistanceBucketing)
            {
                hexesByDistanceFromInnerRadius = new List<Hex>[0];
                foreach (Vector2Int evenR in evenRResults)
                {
                    Hex hex = map.grid[mapLayer][evenR.x][evenR.y];
                    results.Add(hex);
                }
            }
            else
            {
                hexesByDistanceFromInnerRadius = new List<Hex>[evenRCoordinatesWithinCircularRing.Length];
                for (int d = 0; d < evenRCoordinatesWithinCircularRing.Length; d++)
                {
                    int dist = roundedInnerRadius + d;
                    hexesByDistanceFromInnerRadius[d] = new List<Hex>(evenRCoordinatesWithinCircularRing[d].Count);
                    foreach (Vector2Int evenR in evenRCoordinatesWithinCircularRing[d])
                    {
                        Hex hex = map.grid[mapLayer][evenR.x][evenR.y];
                        hexesByDistanceFromInnerRadius[d].Add(hex);
                        results.Add(hex);
                    }
                }
            }

            return results;
        }

        public static List<Hex> HexesWithinCircularRing(Map map, Hex origin, double outerRadius, double innerRadius, List<int> mapLayers, out List<Hex>[] hexesByDistanceFromInnerRadius, bool skipDistanceBucketing = true)
        {
            if (mapLayers == null || mapLayers.Count == 0 || mapLayers.All(mapLayer => mapLayer < 0 || mapLayer >= map.grid.Length))
            {
                hexesByDistanceFromInnerRadius = new List<Hex>[0];
                return new List<Hex>();
            }

            double verticalSpacing = HexHeight;
            if (innerRadius < verticalSpacing)
            {
                innerRadius = 0.0;
            }

            if (outerRadius < innerRadius)
            {
                hexesByDistanceFromInnerRadius = new List<Hex>[0];
                return new List<Hex>();
            }

            outerRadius *= HexWidth;
            innerRadius *= HexWidth;
            if (outerRadius < verticalSpacing)
            {
                if (innerRadius > 0.0)
                {
                    hexesByDistanceFromInnerRadius = new List<Hex>[0];
                    return new List<Hex>();
                }

                hexesByDistanceFromInnerRadius = new List<Hex>[1];
                hexesByDistanceFromInnerRadius[0] = new List<Hex>(2);
                List<Hex> earlyResults = new List<Hex>(2);

                foreach (int mapLayer in mapLayers)
                {
                    if (mapLayer < 0 || mapLayer >= map.grid.Length)
                    {
                        continue;
                    }

                    Hex hex = map.grid[mapLayer][origin.x][origin.y];
                    hexesByDistanceFromInnerRadius[0].Add(hex);
                    earlyResults.Add(hex);
                }

                return earlyResults;
            }
            int roundedInnerRadius = (int)Math.Floor(innerRadius);

            List<Vector2Int> evenRResults = EvenRCoordinatesWithinCircularRing(map, HexToEvenR(origin), outerRadius, innerRadius, out List<Vector2Int>[] evenRCoordinatesWithinCircularRing, skipDistanceBucketing);
            if (evenRResults.Count == 0)
            {
                hexesByDistanceFromInnerRadius = new List<Hex>[0];
                return new List<Hex>();
            }

            List<Hex> results = new List<Hex>(evenRResults.Count * mapLayers.Count);
            if (skipDistanceBucketing)
            {
                hexesByDistanceFromInnerRadius = new List<Hex>[0];
                foreach (Vector2Int evenR in evenRResults)
                {
                    foreach (int mapLayer in mapLayers)
                    {
                        if (mapLayer < 0 || mapLayer >= map.grid.Length)
                        {
                            continue;
                        }

                        Hex hex = map.grid[mapLayer][evenR.x][evenR.y];
                        results.Add(hex);
                    }
                }
            }
            else
            {
                hexesByDistanceFromInnerRadius = new List<Hex>[evenRCoordinatesWithinCircularRing.Length];
                for (int d = 0; d < evenRCoordinatesWithinCircularRing.Length; d++)
                {
                    int dist = roundedInnerRadius + d;
                    hexesByDistanceFromInnerRadius[d] = new List<Hex>(evenRCoordinatesWithinCircularRing[d].Count * mapLayers.Count);
                    foreach (Vector2Int evenR in evenRCoordinatesWithinCircularRing[d])
                    {
                        foreach (int mapLayer in mapLayers)
                        {
                            if (mapLayer < 0 || mapLayer >= map.grid.Length)
                            {
                                continue;
                            }

                            Hex hex = map.grid[mapLayer][evenR.x][evenR.y];
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

        public static List<Vector3Int> CubeCoordinatesWithinLine(Map map, Vector3Int start, Vector3Int end, bool includeOutOfBoundsCoordinates = false)
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
                Vector2Int evenR = CubeToEvenR(cube);
                if (includeOutOfBoundsCoordinates || (evenR.x >= 0 && evenR.x < map.sizeX && evenR.y >= 0 && evenR.y < map.sizeY))
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
