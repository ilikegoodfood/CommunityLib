using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace CommunityLib
{
    public static class UIUtils
    {
        public static void LogTree(Component c, bool printProperties, bool printParent = true, bool printSiblings = true, bool printChildren = true, bool printDescendants = false)
        {
            Console.WriteLine($"CommunityLib: Logging Tree for {c.name} ({c.GetType().Name})");

            Component parent = c.transform.parent;
            if (printParent)
            {
                LogComponent(c, printProperties);
            }
            LogComponent(c, printProperties);

            if (printSiblings)
            {
                LogSiblings(c);
            }

            if (printDescendants)
            {
                LogDecendants(c);
            }
            else if (printChildren)
            {
                LogChildren(c);
            }
        }

        public static void LogComponent(Component c, bool printProperties)
        {
            Console.WriteLine($"CommunityLib: Logging Tree for {c.name} ({c.GetType().Name})");
            Console.WriteLine($"Component is {c.name} ({c.GetType().Name}). {(c.gameObject.activeSelf == true ? "Active" : "Inactive")}.");

            if (printProperties)
            {
                // stub
            }
        }

        public static void LogParent(Component c, bool printProperties)
        {
            Component parent = c.transform.parent;

            Console.WriteLine($"Parent is {parent.name} ({parent.GetType().Name}). {(parent.gameObject.activeSelf == true ? "Active" : "Inactive")}.");

            if (printProperties)
            {
                // stub
            }
        }

        public static void LogSiblings(Component c)
        {
            Component parent = c.transform.parent;
            if (parent == null)
            {
                Console.WriteLine($"CommunityLib: {c.name} ({c.GetType().Name}) is unparented, and therefore cannot have siblings.");
                return;
            }

            Component[] siblings = parent.GetComponentsInChildren<Component>();
            Console.WriteLine($"{siblings.Length} {(siblings.Length == 1 ? "Sibling" : "Siblings")} Found");
            for (int i = 0; i < siblings.Length; i++)
            {
                Console.WriteLine($"Sibling {i} is {siblings[i].name} ({siblings[i].GetType().Name ?? "null"})");
            }
        }

        public static void LogChildren(Component c)
        {
            Component[] children = c.GetComponents<Component>();
            Console.WriteLine($"{children.Length} {(children.Length == 1 ? "Child" : "Children")} Found");
            for (int i = 0; i < children.Length; i++)
            {
                Console.WriteLine($"Child {i} is {children[i].name} ({children[i].GetType()})");
            }
        }

        public static void LogDecendants(Component c)
        {
            Component[] decendants = c.GetComponentsInChildren<Component>();
            Console.WriteLine($"{decendants.Length} {(decendants.Length == 1 ? "Descendant" : "Decendants")} Found");
            for (int i = 0; i < decendants.Length; i++)
            {
                if (decendants[i].transform.parent == c.transform)
                {
                    Console.WriteLine($"Child {i} is {decendants[i].name} ({decendants[i].GetType()})");
                }
                else
                {
                    Console.WriteLine($"Descent {i} is {decendants[i].name} ({decendants[i].GetType()}). Child of {(decendants[i].transform.parent != null ? decendants[i].transform.parent.gameObject.name : "null")} ({(decendants[i].transform.parent != null ? decendants[i].transform.parent.gameObject.GetType().ToString() : "null")}).");
                }
                
            }
        }

        public static Transform GetChildStrict(Component parent, string name)
        {
            return GetChildStrict(parent.transform, name);
        }

        public static Transform GetChildStrict(Transform parent, string name)
        {
            Transform child = parent.Find(name);

            if (child == null)
            {
                Console.WriteLine($"CommunityLib: ERROR: Child {name} could not be found on {parent.name}");
            }

            return child;
        }
    }
}
