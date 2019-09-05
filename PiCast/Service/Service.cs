using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using PiCast.Model;
using PiCast.Repository;

namespace PiCast.Service
{

    public interface IReadOnlyService<T> where T : Entity
    {
        IQueryable<T> Get(Expression<Func<T, bool>> exp = null);
        Task<T> Get(int id);
    }
    public interface IService<T> : IReadOnlyService<T> where T : Entity
    {
        Task<T> Add(T entity);
        Task<T> Update(int id, T entity);
        Task<int> Delete(int id);
    }

    public class ReadOnlyService<T> : IReadOnlyService<T> where T : Entity
    {
        protected readonly IReadOnlyRepository<T> _repository;

        public ReadOnlyService(IReadOnlyRepository<T> repository)
        {
            _repository = repository;
        }

        public IQueryable<T> Get(Expression<Func<T, bool>> exp = null)
        {
            var items = _repository.Get().AsQueryable();
            if (exp == null)
                return items;
            return items.Where(exp);
        }

        public Task<T> Get(int id)
        {
            return _repository.Get(id);
        }
    }

    public class Service<T> : ReadOnlyService<T>, IService<T> where T : Entity
    {
        private readonly IRepository<T> _repository;

        public Service(IRepository<T> repository) : base(repository)
        {
            _repository = repository;
        }
        public Task<T> Add(T entity)
        {
            return _repository.Add(entity);
        }

        public Task<T> Update(int id, T entity)
        {
            return _repository.Update(id, entity);
        }

        public Task<int> Delete(int id)
        {
            return _repository.Delete(id);
        }
    }
}
