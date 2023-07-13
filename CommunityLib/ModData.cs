using Assets.Code;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace CommunityLib
{
    public class ModData
    {
        public Map map;

        public struct ModIntegrationData
        {
            public Assembly assembly;
            public Dictionary<string, Type> typeDict;
            public Dictionary<string, MethodInfo> methodInfoDict;
            public Dictionary<string, FieldInfo> fieldInfoDict;

            public ModIntegrationData(Assembly asm)
            {
                assembly = asm;
                typeDict = new Dictionary<string, Type>();
                methodInfoDict = new Dictionary<string, MethodInfo>();
                fieldInfoDict = new Dictionary<string, FieldInfo>();
            }
        }

        private Dictionary<string, ModIntegrationData> modAssemblies;

        public ModData(Map map)
        {
            this.map = map;
            
            modAssemblies = new Dictionary<string, ModIntegrationData>();
        }

        public void onLoad(Map map)
        {
            this.map = map;

            if (modAssemblies == null)
            {
                modAssemblies = new Dictionary<string, ModIntegrationData>();
            }
        }

        internal void addModAssembly(string key, ModIntegrationData asm)
        {
            if (key == "" || asm.assembly == null)
            {
                return;
            }

            if (modAssemblies.ContainsKey(key) && modAssemblies[key].assembly == null)
            {
                modAssemblies[key] = asm;
            }
            else
            {
                modAssemblies.Add(key, asm);
            }
        }

        internal bool tryGetModAssembly(string key, out ModIntegrationData asm)
        {
            bool result = modAssemblies.TryGetValue(key, out ModIntegrationData retASM);
            asm = retASM;

            return result;
        }
    }
}
