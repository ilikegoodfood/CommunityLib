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

        public bool isClean = true;

        private Dictionary<string, ModIntegrationData> modIntegrationData;

        private Dictionary<Culture, ModCultureData> modCultureData;

        public ModData()
        {
            initialiseModIntegrationData();
            initialiseModCultureData();
        }

        private void initialiseModIntegrationData()
        {
            if (modIntegrationData == null)
            {
                modIntegrationData = new Dictionary<string, ModIntegrationData>();
            }
        }

        private void initialiseModCultureData()
        {
            if (modCultureData == null)
            {
                modCultureData = new Dictionary<Culture, ModCultureData>();
            }
        }

        public void clean()
        {
            if (isClean)
            {
                return;
            }

            map = null;
            modIntegrationData.Clear();
            modCultureData.Clear();

            isClean = true;
        }

        public void onLoad(Map map)
        {
            this.map = map;

            initialiseModIntegrationData();
            initialiseModCultureData();
        }

        internal void addModIntegrationData(string key, ModIntegrationData intData)
        {
            if (key == "" || intData.assembly == null)
            {
                return;
            }

            initialiseModIntegrationData();

            if (modIntegrationData.TryGetValue(key, out ModIntegrationData intData2) && intData2.assembly == null)
            {
                modIntegrationData[key] = intData;
            }
            else
            {
                modIntegrationData.Add(key, intData);
            }
        }

        internal bool tryGetModIntegrationData(string key, out ModIntegrationData intData) => modIntegrationData.TryGetValue(key, out intData);

        internal void addCultureData(Culture key, ModCultureData data)
        {
            if (key == null || data == null)
            {
                return;
            }

            initialiseModCultureData();

            if (modCultureData.TryGetValue(key, out ModCultureData data2) && data2 == null)
            {
                modCultureData[key] = data;
            }
            else
            {
                modCultureData.Add(key, data);
            }
        }

        internal bool tryGetModCultureData(Culture key, out ModCultureData cultureData) => modCultureData.TryGetValue(key, out cultureData);

        internal Dictionary<Culture, ModCultureData> GetModCultureData() => modCultureData;
    }
}
