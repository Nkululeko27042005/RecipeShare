using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using System.Web;

namespace RecipeShare.Models.Repositories
{
    public class Repository<T> : IRepository<T> where T : class
    {
        protected readonly ApplicationDbContext Context;
        protected readonly DbSet<T> DbSet;

        public Repository(ApplicationDbContext context)
        {
            Context = context;
            DbSet = context.Set<T>();
        }

        public T GetById(int id) => DbSet.Find(id);
        public IQueryable<T> Query() => DbSet.AsQueryable();
        public IEnumerable<T> Find(Expression<Func<T, bool>> predicate) => DbSet.Where(predicate).ToList();
        public void Add(T entity) => DbSet.Add(entity);
        public void Remove(T entity) => DbSet.Remove(entity);
        public void Update(T entity) => Context.Entry(entity).State = EntityState.Modified;
    }
}