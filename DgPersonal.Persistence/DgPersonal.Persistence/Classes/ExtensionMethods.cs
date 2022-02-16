using System;
using System.Linq.Expressions;
using DgPersonal.Persistence.Interfaces;

namespace DgPersonal.Persistence.Classes
{
    public static class ExtensionMethods
    {
        public static Expression<Func<TEntity, bool>> FindById<TEntity>(this IIntIdentifier from) 
            where TEntity : IIntIdentifier
            => x => x.Id == from.Id;
    }
}