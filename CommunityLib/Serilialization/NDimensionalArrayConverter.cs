using FullSerializer;
using System;
using System.Collections.Generic;
using System.Text;

namespace CommunityLib.Serialization
{
    public class NDimensionalArrayConverter : fsConverter
    {
        public override bool CanProcess(Type type)
        {
            return type.IsArray && type.GetArrayRank() > 1;
        }

        public override fsResult TrySerialize(object instance, out fsData serialized, Type storageType)
        {
            Console.WriteLine($"CommunityLib: FullSerializer cannot natively serialize n-dimensional arrays, where n is greater than 1.");
            (Type ObjectType, Type StorageType) pathData = PathTraceObjectProcessor.PathStack.Pop();
            (Type ObjectType, Type StorageType) pathDataParent = PathTraceObjectProcessor.PathStack.Pop();

            Console.WriteLine($"CommunityLib: {pathData.ObjectType?.Namespace ?? pathData.StorageType.Namespace}.{pathData.ObjectType?.Name ?? pathData.StorageType.Name} encountered at {pathDataParent.ObjectType?.Namespace ?? pathDataParent.StorageType.Namespace}.{pathDataParent.ObjectType?.Name ?? pathDataParent.StorageType.Name}");

            PathTraceObjectProcessor.PathStack.Push(pathDataParent);
            PathTraceObjectProcessor.PathStack.Push(pathData);

            serialized = fsData.Null;
            return fsResult.Success;
        }

        public override fsResult TryDeserialize(fsData data, ref object instance, Type storageType)
        {
            instance = null;
            return fsResult.Success;
        }

        private void ReportError(Type storageType)
        {
            // Retrieve the stack from the context
            Stack<(Type ObjectType, Type StorageType)> pathStack = PathTraceObjectProcessor.PathStack;
            StringBuilder pathBuilder = new StringBuilder();

            while (pathStack.Count > 0)
            {
                (Type ObjectType, Type StorageType) pathData = pathStack.Pop();
                pathBuilder.Append(pathData.ObjectType?.FullName ?? "NULL");
                pathBuilder.Append(" - ");
                pathBuilder.AppendLine(pathData.StorageType.FullName);
            }

            string path = pathBuilder.ToString();
            if (string.IsNullOrWhiteSpace(path))
            {
                path = "Root";
            }

            throw new NotSupportedException($"FullSerializer cannot natively serialize {storageType.GetArrayRank()}-dimensional arrays.\nPath: {path}");
        }
    }
}
