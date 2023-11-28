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

        private Dictionary<string, ModIntegrationData> modIntegrationData;

        private Dictionary<Culture, ModCultureData> modCultureData;

        public ModData(Map map)
        {
            this.map = map;
            
            modIntegrationData = new Dictionary<string, ModIntegrationData>();
            modCultureData = new Dictionary<Culture, ModCultureData>();
        }

        public void onLoad(Map map)
        {
            this.map = map;

            if (modIntegrationData == null)
            {
                modIntegrationData = new Dictionary<string, ModIntegrationData>();
            }

            if (modCultureData == null)
            {
                modCultureData = new Dictionary<Culture, ModCultureData>();
            }
        }

        internal void addModIntegrationData(string key, ModIntegrationData intData)
        {
            if (key == "" || intData.assembly == null)
            {
                return;
            }

            if (modIntegrationData.TryGetValue(key, out ModIntegrationData intData2) && intData2.assembly == null)
            {
                modIntegrationData[key] = intData;
            }
            else
            {
                modIntegrationData.Add(key, intData);
            }
        }

        internal bool tryGetModIntegrationData(string key, out ModIntegrationData intData)
        {
            return modIntegrationData.TryGetValue(key, out intData);
        }

        internal void addCultureData(Culture key, ModCultureData data)
        {
            if (key == null)
            {
                return;
            }

            if (modCultureData.TryGetValue(key, out ModCultureData data2) && data2 == null)
            {
                modCultureData[key] = data;
            }
            else
            {
                modCultureData.Add(key, data);
            }
        }

        internal bool tryGetModCultureData(Culture key, out ModCultureData cultureData)
        {
            return modCultureData.TryGetValue(key, out cultureData);
        }
    }
}
