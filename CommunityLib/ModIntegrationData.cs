using Assets.Code.Modding;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace CommunityLib
{
    internal class ModIntegrationData
    {
        public Assembly assembly;
        public ModKernel kernel;
        public Dictionary<string, Type> typeDict;
        public Dictionary<string, MethodInfo> methodInfoDict;
        public Dictionary<string, FieldInfo> fieldInfoDict;
        public Dictionary<string, ConstructorInfo> constructorInfoDict;

        public ModIntegrationData(ModKernel modKernel)
        {
            kernel = modKernel;
            assembly = modKernel.GetType().Assembly;
            
            typeDict = new Dictionary<string, Type> { { "Kernel", kernel.GetType() } };
            methodInfoDict = new Dictionary<string, MethodInfo>();
            fieldInfoDict = new Dictionary<string, FieldInfo>();
            constructorInfoDict = new Dictionary<string, ConstructorInfo>();
        }
    }
}
