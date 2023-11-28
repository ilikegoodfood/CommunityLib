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
        public Assembly assembly = null;
        public Dictionary<string, Type> typeDict = new Dictionary<string, Type>();
        public Dictionary<string, MethodInfo> methodInfoDict = new Dictionary<string, MethodInfo>();
        public Dictionary<string, FieldInfo> fieldInfoDict = new Dictionary<string, FieldInfo>();

        public ModIntegrationData(Assembly asm)
        {
            assembly = asm;
        }
    }
}
