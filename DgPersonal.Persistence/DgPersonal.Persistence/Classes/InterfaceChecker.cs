using System;
using System.Collections.Generic;
using System.Linq;

namespace DgPersonal.Persistence.Classes
{
    internal static class InterfaceChecker
    {
        internal static bool TypeSupportsInterfaces(this Type type, IEnumerable<Type> interfaceTypes)
        {
            foreach (var interfaceType in interfaceTypes)
            {
                if(interfaceType.IsAssignableFrom(type))
                    return true;
            
                return type != null 
                       && type.GetInterfaces()
                           .Any(i=>i. IsGenericType 
                                   && i.GetGenericTypeDefinition() == interfaceType);
            }

            return false;
        }
    }
}