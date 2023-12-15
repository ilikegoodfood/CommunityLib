using Assets.Code.Modding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

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

        public ModIntegrationData(Assembly asm, ModKernel modKernel)
        {
            assembly = asm;
            kernel = modKernel;
            typeDict = new Dictionary<string, Type>();
            methodInfoDict = new Dictionary<string, MethodInfo>();
            fieldInfoDict = new Dictionary<string, FieldInfo>();
            constructorInfoDict = new Dictionary<string, ConstructorInfo>();
        }
    }
}
