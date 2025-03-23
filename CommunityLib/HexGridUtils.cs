using Assets.Code;
using System;
using System.Collections.Generic;
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
            List<Vector3Int> results = new List<Vector3Int>();
            cubeCoordinatesByDistance = new List<Vector3Int>[radius + 1];
            for (int d = 0; d <= radius; d++)
            {
                cubeCoordinatesByDistance[d] = new List<Vector3Int>();
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
            List<Vector3Int> results = new List<Vector3Int>();
            cubeCoordinatesByDistance = new List<Vector3Int>[radius + 1];

            List<Vector3Int> naiveResults = CubeCoordinatesWithinRadiusNaive(origin, radius, out List<Vector3Int>[] naiveCoordinatesByDistance);
            if (includeOutOfBoundsCoordinates)
            {
                cubeCoordinatesByDistance = naiveCoordinatesByDistance;
                return naiveResults;
            }

            for (int d = 0; d < naiveCoordinatesByDistance.Length; d++)
            {
                cubeCoordinatesByDistance[d] = new List<Vector3Int>();
                foreach (Vector3Int cube in naiveCoordinatesByDistance[d])
                {
                    Vector2Int evenR = CubeToEvenR(cube);
                    if (evenR.x < 0 || evenR.x >= map.sizeX || evenR.y < 0 || evenR.y >= map.sizeY)
                    {
                        continue;
                    }

                    results.Add(cube);
                    cubeCoordinatesByDistance[d].Add(cube);
                }
            }

            return results;
        }

        public static List<Vector2Int> AxialCoordinatesWithinRadius(Map map, Vector2Int origin, int radius, out List<Vector2Int>[] axialCoordinatesByDistance, bool includeOutOfBoundsCoordinates = false)
        {
            List<Vector2Int> results = new List<Vector2Int>();
            axialCoordinatesByDistance = new List<Vector2Int>[radius + 1];

            CubeCoordinatesWithinRadius(map, AxialToCube(origin), radius, out List<Vector3Int>[] cubeCoordinatesByDistance, includeOutOfBoundsCoordinates);
            for (int d = 0; d < cubeCoordinatesByDistance.Length; d++)
            {
                axialCoordinatesByDistance[d] = new List<Vector2Int>();
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
            List<Vector2Int> results = new List<Vector2Int>();
            evenRCoordinatesByDistance = new List<Vector2Int>[radius + 1];

            CubeCoordinatesWithinRadius(map, EvenRToCube(origin), radius, out List<Vector3Int>[] cubeCoordinatesByDistance, includeOutOfBoundsCoordinates);
            for (int d = 0; d < cubeCoordinatesByDistance.Length; d++)
            {
                evenRCoordinatesByDistance[d] = new List<Vector2Int>();
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
            List<Hex> results = new List<Hex>();
            hexesByDistance = new List<Hex>[radius + 1];

            CubeCoordinatesWithinRadius(map, EvenRToCube(new Vector2Int(origin.x, origin.y)), radius, out List<Vector3Int>[] cubeCoordinatesByDistance);
            for (int d = 0; d < cubeCoordinatesByDistance.Length; d++)
            {
                hexesByDistance[d] = new List<Hex>();
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
            List<Hex> results = new List<Hex>();
            hexesByDistance = new List<Hex>[radius + 1];

            if (mapLayer < 0 || mapLayer >= map.grid.Length)
            {
                return results;
            }


            CubeCoordinatesWithinRadius(map, EvenRToCube(new Vector2Int(origin.x, origin.y)), radius, out List<Vector3Int>[] cubeCoordinatesByDistance);
            for (int d = 0; d < cubeCoordinatesByDistance.Length; d++)
            {
                hexesByDistance[d] = new List<Hex>();
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
            List<Hex> results = new List<Hex>();
            hexesByDistance = new List<Hex>[radius + 1];

            if (mapLayers == null || mapLayers.Count == 0)
            {
                return results;
            }

            CubeCoordinatesWithinRadius(map, EvenRToCube(new Vector2Int(origin.x, origin.y)), radius, out List<Vector3Int>[] cubeCoordinatesByDistance);
            HashSet<int> visitedMapLayers = new HashSet<int>();
            for (int d = 0; d < cubeCoordinatesByDistance.Length; d++)
            {
                hexesByDistance[d] = new List<Hex>();
                foreach (Vector3Int cube in cubeCoordinatesByDistance[d])
                {
                    visitedMapLayers.Clear();
                    Vector2Int evenR = CubeToEvenR(cube);
                    foreach (int mapLayer in mapLayers)
                    {
                        if(visitedMapLayers.Contains(mapLayer))
                        {
                            continue;
                        }
                        visitedMapLayers.Add(mapLayer);
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
            List<Vector3Int> results = new List<Vector3Int>();
            
            if (outerRadius < innerRadius)
            {
                cubeCoordinatesByDistanceFromInnerRadius = new List<Vector3Int>[0];
                return results;
            }

            int ringWidth = outerRadius - innerRadius;
            cubeCoordinatesByDistanceFromInnerRadius = new List<Vector3Int>[ringWidth + 1];

            for (int d = 0; d < cubeCoordinatesByDistanceFromInnerRadius.Length; d++)
            {
                cubeCoordinatesByDistanceFromInnerRadius[d] = new List<Vector3Int>();
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
            List<Vector3Int> results = new List<Vector3Int>();

            if (outerRadius < innerRadius)
            {
                cubeCoordinatesByDistanceFromInnerRadius = new List<Vector3Int>[0];
                return results;
            }

            int ringWidth = outerRadius - innerRadius;
            cubeCoordinatesByDistanceFromInnerRadius = new List<Vector3Int>[ringWidth + 1];

            List<Vector3Int> naiveResults = CubeCoordinatesWithinRingNaive(origin, outerRadius, innerRadius, out List<Vector3Int>[] naiveCubeCoordinatesByDistanceFromInnerRadius);
            if (includeOutOfBoundsCoordinates)
            {
                cubeCoordinatesByDistanceFromInnerRadius = naiveCubeCoordinatesByDistanceFromInnerRadius;
                return naiveResults;
            }

            for (int i = 0; i < naiveCubeCoordinatesByDistanceFromInnerRadius.Length; i++)
            {
                cubeCoordinatesByDistanceFromInnerRadius[i] = new List<Vector3Int>();
                foreach (var cube in naiveCubeCoordinatesByDistanceFromInnerRadius[i])
                {
                    Vector2Int evenR = CubeToEvenR(cube);
                    if (evenR.x < 0 || evenR.x >= map.sizeX || evenR.y < 0 || evenR.y >= map.sizeY)
                    {
                        continue;
                    }

                    cubeCoordinatesByDistanceFromInnerRadius[i].Add(cube);
                    results.Add(cube);
                }
            }

            return results;
        }

        public static List<Vector2Int> AxialCoordinatesWithinRing(Map map, Vector2Int origin, int outerRadius, int innerRadius, out List<Vector2Int>[] axialCoordinatesByDistanceFromInnerRadius, bool includeOutOfBoundsCoordinates = false)
        {
            List<Vector2Int> results = new List<Vector2Int>();

            if (outerRadius < innerRadius)
            {
                axialCoordinatesByDistanceFromInnerRadius = new List<Vector2Int>[0];
                return results;
            }

            int ringWidth = outerRadius - innerRadius;
            axialCoordinatesByDistanceFromInnerRadius = new List<Vector2Int>[ringWidth + 1];

            CubeCoordinatesWithinRing(map, AxialToCube(origin), outerRadius, innerRadius, out List<Vector3Int>[] cubeCoordinatesByDistanceFromInnerRadius, includeOutOfBoundsCoordinates);
            for (int i = 0; i < cubeCoordinatesByDistanceFromInnerRadius.Length; i++)
            {
                axialCoordinatesByDistanceFromInnerRadius[i] = new List<Vector2Int>();
                foreach (var cube in cubeCoordinatesByDistanceFromInnerRadius[i])
                {
                    Vector2Int axial = CubeToAxial(cube);
                    axialCoordinatesByDistanceFromInnerRadius[i].Add(axial);
                    results.Add(axial);
                }
            }

            return results;
        }

        public static List<Vector2Int> EvenRCoordinatesWithinRing(Map map, Vector2Int origin, int outerRadius, int innerRadius, out List<Vector2Int>[] evenRCoordinatesByDistanceFromInnerRadius, bool includeOutOfBoundsCoordinates = false)
        {
            List<Vector2Int> results = new List<Vector2Int>();

            if (outerRadius < innerRadius)
            {
                evenRCoordinatesByDistanceFromInnerRadius = new List<Vector2Int>[0];
                return results;
            }

            int ringWidth = outerRadius - innerRadius;
            evenRCoordinatesByDistanceFromInnerRadius = new List<Vector2Int>[ringWidth + 1];

            CubeCoordinatesWithinRing(map, EvenRToCube(origin), outerRadius, innerRadius, out List<Vector3Int>[] cubeCoordinatesByDistanceFromInnerRadius, includeOutOfBoundsCoordinates);
            for (int i = 0; i < cubeCoordinatesByDistanceFromInnerRadius.Length; i++)
            {
                evenRCoordinatesByDistanceFromInnerRadius[i] = new List<Vector2Int>();
                foreach (var cube in cubeCoordinatesByDistanceFromInnerRadius[i])
                {
                    Vector2Int evenR = CubeToEvenR(cube);
                    evenRCoordinatesByDistanceFromInnerRadius[i].Add(evenR);
                    results.Add(evenR);
                }
            }

            return results;
        }

        public static List<Hex> HexesWithinRing(Map map, Hex origin, int outerRadius, int innerRadius, out List<Hex>[] hexesByDistanceFromInnerRadius)
        {
            List<Hex> results = new List<Hex>();

            if (outerRadius < innerRadius)
            {
                hexesByDistanceFromInnerRadius = new List<Hex>[0];
                return results;
            }

            int ringWidth = outerRadius - innerRadius;
            hexesByDistanceFromInnerRadius = new List<Hex>[ringWidth + 1];

            CubeCoordinatesWithinRing(map, EvenRToCube(new Vector2Int(origin.x, origin.y)), outerRadius, innerRadius, out List<Vector3Int>[] cubeCoordinatesByDistanceFromInnerRadius);
            for (int d = 0; d < cubeCoordinatesByDistanceFromInnerRadius.Length; d++)
            {
                hexesByDistanceFromInnerRadius[d] = new List<Hex>();
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
            List<Hex> results = new List<Hex>();

            if (outerRadius < innerRadius)
            {
                hexesByDistanceFromInnerRadius = new List<Hex>[0];
                return results;
            }

            int ringWidth = outerRadius - innerRadius;
            hexesByDistanceFromInnerRadius = new List<Hex>[ringWidth + 1];

            if (mapLayer < 0 || mapLayer >= map.grid.Length)
            {
                return results;
            }

            CubeCoordinatesWithinRing(map, EvenRToCube(new Vector2Int(origin.x, origin.y)), outerRadius, innerRadius, out List<Vector3Int>[] cubeCoordinatesByDistanceFromInnerRadius);
            for (int d = 0; d < cubeCoordinatesByDistanceFromInnerRadius.Length; d++)
            {
                hexesByDistanceFromInnerRadius[d] = new List<Hex>();
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
            List<Hex> results = new List<Hex>();

            if (outerRadius < innerRadius)
            {
                hexesByDistanceFromInnerRadius = new List<Hex>[0];
                return results;
            }

            int ringWidth = outerRadius - innerRadius;
            hexesByDistanceFromInnerRadius = new List<Hex>[ringWidth + 1];

            if (mapLayers == null || mapLayers.Count == 0)
            {
                return results;
            }

            CubeCoordinatesWithinRing(map, EvenRToCube(new Vector2Int(origin.x, origin.y)), outerRadius, innerRadius, out List<Vector3Int>[] cubeCoordinatesByDistanceFromInnerRadius);
            for (int d = 0; d < cubeCoordinatesByDistanceFromInnerRadius.Length; d++)
            {
                hexesByDistanceFromInnerRadius[d] = new List<Hex>();
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

        #region HexesWithinCircle
        public static List<Vector3Int> CubeCoordinatesWithinCircleNaive(Vector3Int origin, double radius, out List<Vector3Int>[] cubeCoordinatesByDistance)
        {
            List<Vector3Int> results = new List<Vector3Int>();

            int outerRadius = (int)Math.Ceiling(radius);
            int innerRadius = (int)Math.Floor((RootThree / 2) * outerRadius);
            double squareRadius = radius * radius;
            cubeCoordinatesByDistance = new List<Vector3Int>[outerRadius + 1];

            List<Vector3Int> outerCubeCoordinates = CubeCoordinatesWithinRadiusNaive(origin, outerRadius, out List<Vector3Int>[] outerCubeCoordinatesByDistance);

            for (int d = 0; d < outerCubeCoordinatesByDistance.Length; d++)
            {
                if (d <= innerRadius)
                {
                    cubeCoordinatesByDistance[d] = outerCubeCoordinatesByDistance[d];
                    results.AddRange(cubeCoordinatesByDistance[d]);
                    continue;
                }

                cubeCoordinatesByDistance[d] = new List<Vector3Int>();
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
            List<Vector3Int> results = new List<Vector3Int>();
            int outerRadius = (int)Math.Ceiling(radius);
            cubeCoordinatesByDistance = new List<Vector3Int>[outerRadius + 1];

            List<Vector3Int> naiveResults = CubeCoordinatesWithinCircleNaive(origin, radius, out List<Vector3Int>[] naiveCoordinatesByDistance);
            if (includeOutOfBoundsCoordinates)
            {
                cubeCoordinatesByDistance = naiveCoordinatesByDistance;
                return naiveResults;
            }

            for (int d = 0; d < naiveCoordinatesByDistance.Length; d++)
            {
                cubeCoordinatesByDistance[d] = new List<Vector3Int>();
                foreach (Vector3Int cube in naiveCoordinatesByDistance[d])
                {
                    Vector2Int evenR = CubeToEvenR(cube);
                    if (evenR.x < 0 || evenR.x >= map.sizeX || evenR.y < 0 || evenR.y >= map.sizeY)
                    {
                        continue;
                    }

                    results.Add(cube);
                    cubeCoordinatesByDistance[d].Add(cube);
                }
            }

            return results;
        }

        public static List<Vector2Int> AxialCoordinatesWithinCircle(Map map, Vector2Int origin, double radius, out List<Vector2Int>[] axialCoordinatesByDistance, bool includeOutOfBoundsCoordinates = false)
        {
            List<Vector2Int> results = new List<Vector2Int>();
            int outerRadius = (int)Math.Ceiling(radius);
            axialCoordinatesByDistance = new List<Vector2Int>[outerRadius + 1];

            List<Vector3Int> naiveResults = CubeCoordinatesWithinCircleNaive(AxialToCube(origin), radius, out List<Vector3Int>[] naiveCoordinatesByDistance);
            if (includeOutOfBoundsCoordinates)
            {
                for (int d = 0; d < naiveCoordinatesByDistance.Length; d++)
                {
                    axialCoordinatesByDistance[d] = new List<Vector2Int>();
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
            List<Vector2Int> results = new List<Vector2Int>();
            int outerRadius = (int)Math.Ceiling(radius);
            evenRCoordinatesByDistance = new List<Vector2Int>[outerRadius + 1];

            List<Vector3Int> naiveResults = CubeCoordinatesWithinCircleNaive(EvenRToCube(origin), radius, out List<Vector3Int>[] naiveCoordinatesByDistance);
            if (includeOutOfBoundsCoordinates)
            {
                for (int d = 0; d < naiveCoordinatesByDistance.Length; d++)
                {
                    evenRCoordinatesByDistance[d] = new List<Vector2Int>();
                    foreach (Vector3Int cube in naiveCoordinatesByDistance[d])
                    {
                        Vector2Int evenR = CubeToEvenR(cube);
                        evenRCoordinatesByDistance[d].Add(evenR);
                        results.Add(evenR);
                    }
                }

                return results;
            }

            for (int d = 0; d < naiveCoordinatesByDistance.Length; d++)
            {
                evenRCoordinatesByDistance[d] = new List<Vector2Int>();
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

        public static List<Hex> HexesWithinCircle(Map map, Hex origin, int radius, out List<Hex>[] hexesByDistance)
        {
            List<Hex> results = new List<Hex>();
            hexesByDistance = new List<Hex>[radius + 1];

            CubeCoordinatesWithinCircle(map, EvenRToCube(new Vector2Int(origin.x, origin.y)), radius, out List<Vector3Int>[] cubeCoordinatesByDistance);
            for (int d = 0; d < cubeCoordinatesByDistance.Length; d++)
            {
                hexesByDistance[d] = new List<Hex>();
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

        public static List<Hex> HexesWithinCircle(Map map, Hex origin, int radius, int mapLayer, out List<Hex>[] hexesByDistance)
        {
            List<Hex> results = new List<Hex>();
            hexesByDistance = new List<Hex>[radius + 1];

            if (mapLayer < 0 || mapLayer >= map.grid.Length)
            {
                return results;
            }


            CubeCoordinatesWithinCircle(map, EvenRToCube(new Vector2Int(origin.x, origin.y)), radius, out List<Vector3Int>[] cudeCoordinatesByDistance);
            for (int d = 0; d < cudeCoordinatesByDistance.Length; d++)
            {
                hexesByDistance[d] = new List<Hex>();
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

        public static List<Hex> HexesWithinCircle(Map map, Hex origin, int radius, List<int> mapLayers, out List<Hex>[] hexesByDistance)
        {
            List<Hex> results = new List<Hex>();
            hexesByDistance = new List<Hex>[radius + 1];

            if (mapLayers == null || mapLayers.Count == 0)
            {
                return results;
            }

            CubeCoordinatesWithinCircle(map, EvenRToCube(new Vector2Int(origin.x, origin.y)), radius, out List<Vector3Int>[] cudeCoordinatesByDistance);
            HashSet<int> visitedMapLayers = new HashSet<int>();
            for (int d = 0; d < cudeCoordinatesByDistance.Length; d++)
            {
                hexesByDistance[d] = new List<Hex>();
                foreach (Vector3Int cube in cudeCoordinatesByDistance[d])
                {
                    visitedMapLayers.Clear();
                    Vector2Int evenR = CubeToEvenR(cube);
                    foreach (int mapLayer in mapLayers)
                    {
                        if (visitedMapLayers.Contains(mapLayer))
                        {
                            continue;
                        }
                        visitedMapLayers.Add(mapLayer);
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
            List<Vector3Int> results = new List<Vector3Int>();
            
            if (outerRadius < innerRadius)
            {
                cubeCoordinatesByDistanceFromInnerRadius = new List<Vector3Int>[0];
                return results;
            }

            int outerHexRadius = (int)Math.Ceiling(outerRadius);
            int innerHexRadius = (int)Math.Floor(innerRadius);
            if (innerRadius < 1f)
            {
                innerHexRadius = 0;
            }

            double squareOuterRadius = outerRadius * outerRadius;
            double squareInnerRadius = innerRadius * innerRadius;

            int ringWidth = outerHexRadius - innerHexRadius;
            cubeCoordinatesByDistanceFromInnerRadius = new List<Vector3Int>[ringWidth + 1];

            CubeCoordinatesWithinRingNaive(origin, outerHexRadius, innerHexRadius, out var hexCubeCoordinatesByDistanceFromInnerRadius);
            for (int d = 0; d < hexCubeCoordinatesByDistanceFromInnerRadius.Length; d++)
            {
                cubeCoordinatesByDistanceFromInnerRadius[d] = new List<Vector3Int>();
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
            List<Vector3Int> results = new List<Vector3Int>();

            if (outerRadius < innerRadius)
            {
                cubeCoordinatesByDistanceFromInnerRadius = new List<Vector3Int>[0];
                return results;
            }

            int outerHexRadius = (int)Math.Ceiling(outerRadius);
            int innerHexRadius = (int)Math.Floor(innerRadius);
            if (innerRadius < 1f)
            {
                innerHexRadius = 0;
            }

            int ringWidth = outerHexRadius - innerHexRadius;
            cubeCoordinatesByDistanceFromInnerRadius = new List<Vector3Int>[ringWidth + 1];

            List<Vector3Int> naiveResults = CubeCoordinatesWithinCircularRingNaive(origin, outerRadius, innerRadius, out List<Vector3Int>[] naiveCoordinatesByDistance);
            if (includeOutOfBoundsCoordinates)
            {
                cubeCoordinatesByDistanceFromInnerRadius = naiveCoordinatesByDistance;
                return naiveResults;
            }

            for (int d = 0; d < naiveCoordinatesByDistance.Length; d++)
            {
                cubeCoordinatesByDistanceFromInnerRadius[d] = new List<Vector3Int>();
                foreach (Vector3Int cube in naiveCoordinatesByDistance[d])
                {
                    Vector2Int evenR = CubeToEvenR(cube);
                    if (evenR.x < 0 || evenR.x >= map.sizeX || evenR.y < 0 || evenR.y >= map.sizeY)
                    {
                        continue;
                    }

                    results.Add(cube);
                    cubeCoordinatesByDistanceFromInnerRadius[d].Add(cube);
                }
            }

            return results;
        }

        public static List<Vector2Int> AxialCoordinatesWithinCircularRing(Map map, Vector2Int origin, double outerRadius, double innerRadius, out List<Vector2Int>[] axialCoordinatesByDistanceFromInnerRadius, bool includeOutOfBoundsCoordinates = false)
        {
            List<Vector2Int> results = new List<Vector2Int>();

            if (outerRadius < innerRadius)
            {
                axialCoordinatesByDistanceFromInnerRadius = new List<Vector2Int>[0];
                return results;
            }

            int outerHexRadius = (int)Math.Ceiling(outerRadius);
            int innerHexRadius = (int)Math.Floor(innerRadius);
            if (innerRadius < 1f)
            {
                innerHexRadius = 0;
            }

            int ringWidth = outerHexRadius - innerHexRadius;
            axialCoordinatesByDistanceFromInnerRadius = new List<Vector2Int>[ringWidth + 1];

            List<Vector3Int> naiveResults = CubeCoordinatesWithinCircularRingNaive(AxialToCube(origin), outerRadius, innerRadius, out List<Vector3Int>[] naiveCoordinatesByDistance);
            if (includeOutOfBoundsCoordinates)
            {
                for (int d = 0; d < naiveCoordinatesByDistance.Length; d++)
                {
                    axialCoordinatesByDistanceFromInnerRadius[d] = new List<Vector2Int>();
                    foreach (Vector3Int cube in naiveCoordinatesByDistance[d])
                    {
                        Vector2Int axial = CubeToAxial(cube);
                        axialCoordinatesByDistanceFromInnerRadius[d].Add(axial);
                        results.Add(axial);
                    }
                }

                return results;
            }

            for (int d = 0; d < naiveCoordinatesByDistance.Length; d++)
            {
                axialCoordinatesByDistanceFromInnerRadius[d] = new List<Vector2Int>();
                foreach (Vector3Int cube in naiveCoordinatesByDistance[d])
                {
                    Vector2Int evenR = CubeToEvenR(cube);
                    if (evenR.x < 0 || evenR.x >= map.sizeX || evenR.y < 0 || evenR.y >= map.sizeY)
                    {
                        continue;
                    }
                    Vector2Int axial = CubeToAxial(cube);
                    axialCoordinatesByDistanceFromInnerRadius[d].Add(axial);
                    results.Add(axial);
                }
            }

            return results;
        }

        public static List<Vector2Int> EvenRCoordinatesWithinCircularRing(Map map, Vector2Int origin, double outerRadius, double innerRadius, out List<Vector2Int>[] evenRCoordinatesByDistanceFromInnerRadius, bool includeOutOfBoundsCoordinates = false)
        {
            List<Vector2Int> results = new List<Vector2Int>();

            if (outerRadius < innerRadius)
            {
                evenRCoordinatesByDistanceFromInnerRadius = new List<Vector2Int>[0];
                return results;
            }

            int outerHexRadius = (int)Math.Ceiling(outerRadius);
            int innerHexRadius = (int)Math.Floor(innerRadius);
            if (innerRadius < 1f)
            {
                innerHexRadius = 0;
            }

            int ringWidth = outerHexRadius - innerHexRadius;
            evenRCoordinatesByDistanceFromInnerRadius = new List<Vector2Int>[ringWidth + 1];

            List<Vector3Int> naiveResults = CubeCoordinatesWithinCircularRingNaive(EvenRToCube(origin), outerRadius, innerRadius, out List<Vector3Int>[] naiveCoordinatesByDistance);
            if (includeOutOfBoundsCoordinates)
            {
                for (int d = 0; d < naiveCoordinatesByDistance.Length; d++)
                {
                    evenRCoordinatesByDistanceFromInnerRadius[d] = new List<Vector2Int>();
                    foreach (Vector3Int cube in naiveCoordinatesByDistance[d])
                    {
                        Vector2Int evenR = CubeToEvenR(cube);
                        evenRCoordinatesByDistanceFromInnerRadius[d].Add(evenR);
                        results.Add(evenR);
                    }
                }

                return results;
            }

            for (int d = 0; d < naiveCoordinatesByDistance.Length; d++)
            {
                evenRCoordinatesByDistanceFromInnerRadius[d] = new List<Vector2Int>();
                foreach (Vector3Int cube in naiveCoordinatesByDistance[d])
                {
                    Vector2Int evenR = CubeToEvenR(cube);
                    if (evenR.x < 0 || evenR.x >= map.sizeX || evenR.y < 0 || evenR.y >= map.sizeY)
                    {
                        continue;
                    }
                    evenRCoordinatesByDistanceFromInnerRadius[d].Add(evenR);
                    results.Add(evenR);
                }
            }

            return results;
        }

        public static List<Hex> HexesWithinCircularRing(Map map, Hex origin, double outerRadius, double innerRadius, out List<Hex>[] hexesByDistanceFromInnerRadius)
        {
            List<Hex> results = new List<Hex>();

            if (outerRadius < innerRadius)
            {
                hexesByDistanceFromInnerRadius = new List<Hex>[0];
                return results;
            }

            int outerHexRadius = (int)Math.Ceiling(outerRadius);
            int innerHexRadius = (int)Math.Floor(innerRadius);
            if (innerHexRadius < 1)
            {
                innerHexRadius = 0;
            }

            int ringWidth = outerHexRadius - innerHexRadius;
            hexesByDistanceFromInnerRadius = new List<Hex>[ringWidth + 1];

            List<Vector3Int> cubeCoordinates = CubeCoordinatesWithinCircularRing(map, EvenRToCube(new Vector2Int(origin.x, origin.y)), outerRadius, innerRadius, out List<Vector3Int>[] cubeCoordinatesByDistanceFromInnerRadius);
            if (cubeCoordinates.Count == 0)
            {
                return results;
            }

            for (int d = 0; d < cubeCoordinatesByDistanceFromInnerRadius.Length; d++)
            {
                hexesByDistanceFromInnerRadius[d] = new List<Hex>();
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

        public static List<Hex> HexesWithinCircularRing(Map map, Hex origin, double outerRadius, double innerRadius, int mapLayer, out List<Hex>[] hexesByDistanceFromInnerRadius)
        {
            List<Hex> results = new List<Hex>();

            if (outerRadius < innerRadius)
            {
                hexesByDistanceFromInnerRadius = new List<Hex>[0];
                return results;
            }

            int outerHexRadius = (int)Math.Ceiling(outerRadius);
            int innerHexRadius = (int)Math.Floor(innerRadius);
            if (innerHexRadius < 1)
            {
                innerHexRadius = 0;
            }

            int ringWidth = outerHexRadius - innerHexRadius;
            hexesByDistanceFromInnerRadius = new List<Hex>[ringWidth + 1];

            if (mapLayer < 0 || mapLayer >= map.grid.Length)
            {
                return results;
            }

            List<Vector3Int> cubeCoordinates = CubeCoordinatesWithinCircularRing(map, EvenRToCube(new Vector2Int(origin.x, origin.y)), outerRadius, innerRadius, out List<Vector3Int>[] cubeCoordinatesByDistanceFromInnerRadius);
            if (cubeCoordinates.Count == 0)
            {
                return results;
            }
            for (int d = 0; d < cubeCoordinatesByDistanceFromInnerRadius.Length; d++)
            {
                hexesByDistanceFromInnerRadius[d] = new List<Hex>();
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
            List<Hex> results = new List<Hex>();

            if (outerRadius < innerRadius)
            {
                hexesByDistanceFromInnerRadius = new List<Hex>[0];
                return results;
            }

            int outerHexRadius = (int)Math.Ceiling(outerRadius);
            int innerHexRadius = (int)Math.Floor(innerRadius);
            if (innerHexRadius < 1)
            {
                innerHexRadius = 0;
            }

            int ringWidth = outerHexRadius - innerHexRadius;
            hexesByDistanceFromInnerRadius = new List<Hex>[ringWidth + 1];

            if (mapLayers == null || mapLayers.Count == 0)
            {
                return results;
            }

            List<Vector3Int> cubeCoordinates = CubeCoordinatesWithinCircularRing(map, EvenRToCube(new Vector2Int(origin.x, origin.y)), outerRadius, innerRadius, out List<Vector3Int>[] cubeCoordinatesByDistanceFromInnerRadius);
            if (cubeCoordinates.Count == 0)
            {
                return results;
            }
            for (int d = 0; d < cubeCoordinatesByDistanceFromInnerRadius.Length; d++)
            {
                hexesByDistanceFromInnerRadius[d] = new List<Hex>();
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
            HashSet<Vector3Int> seen = new HashSet<Vector3Int>();
            List<Vector3Int> results = new List<Vector3Int>();
            int distance = HexDistanceCube(start, end);
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
                if (seen.Add(cube))
                {
                    results.Add(cube);
                }
            }

            return results;
        }

        public static List<Vector3Int> CubeCoordinatesWithinLine(Map map, Vector3Int start, Vector3Int end, bool includeOutOfBoundsCoordinates = false)
        {
            List<Vector3Int> results = new List<Vector3Int>();

            List<Vector3Int> naiveResults = CubeCoordinatesWithinLineNaive(start, end);
            if (includeOutOfBoundsCoordinates)
            {
                return naiveResults;
            }

            foreach (Vector3Int cube in naiveResults)
            {
                Vector2Int evenR = CubeToEvenR(cube);
                if (evenR.x < 0 || evenR.x >= map.sizeX || evenR.y < 0 || evenR.y >= map.sizeY)
                {
                    continue;
                }

                results.Add(cube);
            }

            return results;
        }

        public static List<Vector2Int> AxialCoordinatesWithinLine(Map map, Vector2Int start, Vector2Int end, bool includeOutOfBoundsCoordinates = false)
        {
            List<Vector2Int> results = new List<Vector2Int>();

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
            List<Vector2Int> results = new List<Vector2Int>();

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
            List<Hex> results = new List<Hex>();

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
            List<Hex> results = new List<Hex>();

            if (mapLayer < 0 || mapLayer >= map.grid.Length)
            {
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
            List<Hex> results = new List<Hex>();

            if (mapLayers == null || mapLayers.Count == 0)
            {
                return results;
            }

            List<Vector2Int> evenRCoordinatesWithinLine = EvenRCoordinatesWithinLine(map, new Vector2Int(start.x, start.y), new Vector2Int(end.x, end.y));
            foreach (Vector2Int evenR in evenRCoordinatesWithinLine)
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

            return results;
        }
        #endregion
    }
}
