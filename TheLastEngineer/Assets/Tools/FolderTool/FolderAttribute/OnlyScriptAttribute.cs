using System;

namespace Tools.FolderTool.FolderAttribute
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public class OnlyScriptAttribute : Attribute { }
}
