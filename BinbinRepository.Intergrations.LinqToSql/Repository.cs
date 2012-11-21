using System;
using System.Collections.Generic;
using System.Data.Linq;
using System.Data.Linq.Mapping;
using System.Linq;
using System.Linq.Expressions;

namespace BinbinRepository.Intergrations.LinqToSql
{
    public abstract class Repository<TDataContext, TEntity, TKey> : IRepository<TEntity, TKey>
        where TDataContext : DataContext
        where TEntity : class
    {
        public Repository(TDataContext dataContext)
        {
            DataContext = dataContext;
        }

        private TDataContext DataContext { get; set; }

        public virtual List<TEntity> Find(Expression<Func<TEntity, bool>> expression)
        {
            return GetTable().Where(expression).ToList();
        }

        public virtual void Add(TEntity entity)
        {
            DataContext.GetTable<TEntity>().InsertOnSubmit(entity);
            DataContext.SubmitChanges();
        }

        public virtual void Remove(TEntity entity)
        {
            DataContext.GetTable<TEntity>().DeleteOnSubmit(entity);
            DataContext.SubmitChanges();
        }

        public virtual void Save(TEntity entity)
        {
            DataContext.SubmitChanges();
        }

        public virtual TEntity Find(TKey key)
        {
            var itemParameter = Expression.Parameter(typeof (TEntity), "item");
            var whereExpression = Expression.Lambda<Func<TEntity, bool>>
                (
                    Expression.Equal(
                        Expression.Property(
                            itemParameter,
                            GetPrimaryKeyName()
                            ),
                        Expression.Constant(key)
                        ),
                    new[] {itemParameter}
                );
            return GetTable().Where(whereExpression).Single();
        }

        public string GetPrimaryKeyName()
        {
            var metaType = GetMetaType();
            var primaryKey = metaType.DataMembers.Single(m => m.IsPrimaryKey);
            return primaryKey.Name;
        }

        private IQueryable<TEntity> GetTable()
        {
            var metaType = DataContext.Mapping.GetMetaType(typeof (TEntity));
            IQueryable<TEntity> table;
            if (metaType.HasInheritance)
            {
                var baseType = metaType.InheritanceRoot.Type;
                table = DataContext.GetTable(baseType).OfType<TEntity>();
            }
            else
            {
                table = DataContext.GetTable<TEntity>();
            }
            return table;
        }

       


        private MetaType GetMetaType()
        {
            var metaType = DataContext.Mapping.GetMetaType(typeof (TEntity));
            if (metaType.HasInheritance)
            {
                return metaType.InheritanceRoot;
            }
            return metaType;
        }
    }
}