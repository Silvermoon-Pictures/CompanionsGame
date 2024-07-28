using System;
using UnityEngine;

namespace Companions.Common
{
    public class NodeInfoAttribute : Attribute
    {
        private string title;
        private string menuItem;
        private bool hasInput;
        private bool hasOutput;

        public string Title => title;
        public string MenuItem => menuItem;
        public bool HasInput => hasInput;
        public bool HasOutput => hasOutput;


        public NodeInfoAttribute(string title, string menuItem = "", bool hasInput = true, bool hasOutput = true)
        {
            this.title = title;
            this.menuItem = menuItem;
            this.hasInput = hasInput;
            this.hasOutput = hasOutput;
        }
    }

    public class TypeFilterAttribute : PropertyAttribute
    {
        public Type BaseType { get; private set; }

        public TypeFilterAttribute(Type baseType)
        {
            BaseType = baseType;
        }
    }

    public class ReadOnlyAttribute : PropertyAttribute
    {

        public ReadOnlyAttribute()
        {
        
        }
    }

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
    public class ActionGraphContextAttribute : PropertyAttribute
    {
        public string contextName;
    
        public ActionGraphContextAttribute(string name)
        {
            contextName = name;
        }
    }

}
