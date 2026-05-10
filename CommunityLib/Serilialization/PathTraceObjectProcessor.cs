using FullSerializer;
using System;
using System.Collections.Generic;

namespace CommunityLib.Serialization
{
    public class PathTraceObjectProcessor : fsObjectProcessor
    {
        private static Stack<(Type ObjectType, Type StorageType)> _pathStack = new Stack<(Type ObjectType, Type StorageType)>();

        public static Stack<(Type ObjectType, Type StorageType)> PathStack => _pathStack;

        public PathTraceObjectProcessor()
        {
            if (_pathStack.Count > 0)
            {
                Console.WriteLine("CommunityLib: ERROR: Path stack was not fully depleated upon previous serialization attempt.");
                Console.WriteLine("Residual data is: " + string.Join("\n", _pathStack));
                _pathStack.Clear();
            }
        }

        public override bool CanProcess(Type type)
        {
            return true;
        }

        public override void OnBeforeSerialize(Type storageType, object instance)
        {
            _pathStack.Push((instance?.GetType(), storageType));
        }

        public override void OnAfterSerialize(Type storageType, object instance, ref fsData data)
        {
            if (_pathStack.Count == 0)
            {
                Console.WriteLine($"CommunityLib: ERROR: Path Stack Corrupt: Path stack is empty when it shouldn't be.");
            }

            (Type ObjectType, Type StorageType) pathData = _pathStack.Pop();
            if (storageType != pathData.StorageType && (!(instance == null && pathData.ObjectType == null) || instance.GetType() != pathData.ObjectType))
            {
                Console.WriteLine($"CommunityLib: ERROR: Path Stack Corrupt: Expected top of path stack to contain elements {instance?.GetType().Name ?? "NULL"} - {storageType}. Instead encountered {(pathData.ObjectType != null ? pathData.ObjectType.Name : "NULL" )} - {pathData.StorageType}.");
            }
        }
    }
}
