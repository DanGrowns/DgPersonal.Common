using System;

namespace DgPersonal.Persistence.Attributes
{
    [AttributeUsage(AttributeTargets.Class)]
    public class DbProcedure : Attribute
    {
        public string SpName { get; set; }
    }
}