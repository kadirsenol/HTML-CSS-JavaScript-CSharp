﻿using JQuery_and_Bootstrap_with_Asp.Net_Core_MVC.Layers.DataAccess.Abstract;
using JQuery_and_Bootstrap_with_Asp.Net_Core_MVC.Layers.Entities.Abstract;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace JQuery_and_Bootstrap_with_Asp.Net_Core_MVC.Layers.DataAccess.Concrete
{
    public class Repository<T, TId, TDbContext> : IRepository<T, TId> where T : BaseEntity<TId> where TDbContext : DbContext, new()
    {
        private readonly TDbContext SqlDbContext;

        public Repository()
        {
            SqlDbContext = new TDbContext();
        }

        public virtual async Task<int> Insert(T entity)
        {
            SqlDbContext.Set<T>().AddAsync(entity);
            return await SqlDbContext.SaveChangesAsync();
        }
        public virtual async Task<IEnumerable<T>> Get(Expression<Func<T, bool>> expression = null)
        {
            if (expression != null)
            {
                return await SqlDbContext.Set<T>().Where(expression).ToListAsync();
            }
            else
            {
                return await SqlDbContext.Set<T>().ToListAsync();
            }
        }

        public virtual async Task<bool> Any(Expression<Func<T, bool>> expression)
        {
            return await SqlDbContext.Set<T>().Where(expression).AnyAsync();
        }
    }
}
