using DAL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace HDO.Application.DAL
{
    public sealed class RepositoryQuery<TEntity> where TEntity : class, new()
    {
        private readonly List<Expression<Func<TEntity, object>>>
            _includeProperties;

        private readonly SQLiteRepostory<TEntity> _repository;
        private List<Expression<Func<TEntity, bool>>> _filters;
        private Func<IQueryable<TEntity>,
            IOrderedQueryable<TEntity>> _orderByQuerable;
        private int? _page;
        private int? _pageSize;

        public RepositoryQuery(SQLiteRepostory<TEntity> repository)
        {
            _repository = repository;
            _includeProperties = new List<Expression<Func<TEntity, object>>>();
            _filters = new List<Expression<Func<TEntity, bool>>>();
        }

        public RepositoryQuery<TEntity> Filter(
            Expression<Func<TEntity, bool>> filter)
        {
            _filters.Add(filter);
            return this;
        }

        public RepositoryQuery<TEntity> Filter(bool condition,
            Expression<Func<TEntity, bool>> filter)
        {
            if (condition)
                _filters.Add(filter);

            return this;
        }

        public RepositoryQuery<TEntity> OrderBy(
            Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy)
        {
            _orderByQuerable = orderBy;
            return this;
        }

        public RepositoryQuery<TEntity> Include(
            Expression<Func<TEntity, object>> expression)
        {
            _includeProperties.Add(expression);
            return this;
        }

        public IEnumerable<TEntity> GetPage(
            int page, int pageSize, out int totalCount)
        {
            _page = page;
            _pageSize = pageSize;
            totalCount = _repository.Get(_filters).Count();

            return _repository.Get(
                _filters,
                _orderByQuerable, _includeProperties, _page, _pageSize);
        }

        public IEnumerable<TEntity> Get()
        {
            return _repository.Get(
                _filters,
                _orderByQuerable, _includeProperties, _page, _pageSize);
        }
    }
}
