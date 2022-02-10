using System;
using System.Linq;

namespace DgPersonal.Persistence.Classes
{
    internal static class InterfaceChecker
    {
        internal static bool TypeSupportsInterface(this Type type, Type interfaceType)
        {
            if(interfaceType.IsAssignableFrom(type))
                return true;
            
            return type != null && type.GetInterfaces().Any(i=>i. IsGenericType && i.GetGenericTypeDefinition() == interfaceType);
        }
    }
}