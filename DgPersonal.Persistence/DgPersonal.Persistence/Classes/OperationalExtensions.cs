using System;
using System.Collections.Generic;
using System.Linq;
using DgPersonal.Extensions.General.Classes;
using DgPersonal.Persistence.Interfaces;

namespace DgPersonal.Persistence.Classes
{
    public static class OperationalExtensions
    {
        private static void PerformAdaptMerge<TDestination, TSource>(ICollection<TDestination> destinationList, IEnumerable<TSource> sourceList, 
            Func<TSource, TDestination> createDelegate, Action<TDestination, TSource> updateDelegate)
            where TDestination : IEqualityComparer<TDestination, TSource>
        {
            foreach (var src in sourceList.OrEmptyIfNull())
            {
                var dest = destinationList.FirstOrDefault(x => x.Equals(x, src));
                if (dest == null)
                {
                    var newModel = createDelegate(src);
                    destinationList.Add(newModel);
                }
                else
                {
                    updateDelegate(dest, src);
                }
            }
        }

        private static void PerformAdaptDelete<TDestination, TSource>(IList<TDestination> destinationList, List<TSource> sourceList)
            where TDestination : IEqualityComparer<TDestination, TSource>
        {
            sourceList ??= new List<TSource>();
            var comparerList = destinationList.ToList();
            
            if (comparerList.Count == sourceList.Count)
                return;
            
            foreach (var dbModel in comparerList)
            {
                var cmdModel = sourceList.FindIndex(x => dbModel.Equals(dbModel, x));
                
                if (cmdModel >= 0)
                    continue;
                
                var dbModelIndex = destinationList.IndexOf(dbModel);
                destinationList.RemoveAt(dbModelIndex);
            }
        }

        /// <summary>
        /// Performs a merge of data of a list within a tracked entry in Entity Framework with
        /// a set of data that has been pulled from HTTP POST.
        /// </summary>
        /// <param name="destinationList">The list from Entity Framework's tracked entry.</param>
        /// <param name="sourceList">The list from HTTP POST, which may include existing entries from EF.</param>
        /// <param name="createDelegate">The method that will create a new object in the destinationList.</param>
        /// <param name="updateDelegate">The method that will update an existing object in the destinationList.</param>
        public static void AdaptWith<TDestination, TSource>(this List<TDestination> destinationList, List<TSource> sourceList, 
            Func<TSource, TDestination> createDelegate, Action<TDestination, TSource> updateDelegate) 
            where TDestination : IEqualityComparer<TDestination, TSource>
        {
            destinationList ??= new List<TDestination>();
            
            PerformAdaptMerge(destinationList, sourceList, createDelegate, updateDelegate);
            PerformAdaptDelete(destinationList, sourceList);
        }
    }
}