﻿using System;
using System.Linq.Expressions;

namespace DgPersonal.Common.Interfaces
{
    public interface IFindEntity<T> where T : class
    {
        Expression<Func<T, bool>> GetEntity();
    }
}