using Assets.Code;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.Windows;

namespace CommunityLib
{
    public class DebugHelper : MonoBehaviour
    {
        public object saveRoot; // assign via Inspector (e.g. World.Instance)

        public void DebugScan()
        {
            var visited = new HashSet<object>();
            World.Log("CommunityLib: DEBUG: Initialising Debug scan");
            ScanObject(saveRoot, visited, saveRoot.GetType().Name);
        }

        void ScanObject(object obj, HashSet<object> visited, string path)
        {
            if (obj == null || visited.Contains(obj)) return;
            visited.Add(obj);

            var type = obj.GetType();
            foreach (var field in type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
            {
                var value = field.GetValue(obj);
                var fieldPath = $"{path}.{field.Name}";

                if (field.FieldType.IsArray && field.FieldType.GetArrayRank() > 1)
                {
                    World.Log($"CommunityLin: DEBUGSCAN: Multi‑dim array found: {fieldPath} ({field.FieldType})");
                }
                else if (value != null && !field.FieldType.IsPrimitive && !(value is string))
                {
                    ScanObject(value, visited, fieldPath);
                }
            }
        }
    }

}
