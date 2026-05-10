using Assets.Code;
using FullSerializer;
using System;

namespace CommunityLib.Serialization
{
    public class MarkovModelConverter : fsConverter
    {
        public override bool CanProcess(Type type)
        {
            return typeof(MarkovModel).IsAssignableFrom(type);
        }

        public override fsResult TryDeserialize(fsData data, ref object instance, Type storageType)
        {
            return fsResult.Success;
        }

        public override fsResult TrySerialize(object instance, out fsData serialized, Type storageType)
        {
            Console.WriteLine($"CommunityLib: Prevented serialization of MarkovModel instance.");

            serialized = fsData.Null;
            return fsResult.Success;
        }
    }
}
