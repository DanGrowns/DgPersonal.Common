using System;

namespace DgPersonal.Common.Attributes
{
    [AttributeUsage(AttributeTargets.Class)]
    public class DbProcedure : Attribute
    {
        public string SpName { get; set; }
    }
}