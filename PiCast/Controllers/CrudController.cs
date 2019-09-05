using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using PiCast.Model;
using PiCast.Service;

namespace PiCast.Controllers
{

    [ApiController]
    public abstract class ReadOnlyController<T> : ControllerBase where T : Entity
    {
        protected readonly IReadOnlyService<T> _service;

        public ReadOnlyController(IReadOnlyService<T> service)
        {
            _service = service;
        }

        /// <summary>
        /// Generic solution for Get with a filter
        /// </summary>
        /// <param name="filter">value to filter (depends on context)</param>
        /// <returns>Enumeration of DbItems</returns>
        [HttpGet]
        public abstract Task<IEnumerable<T>> Get(string filter);

        /// <summary>
        /// Generic solution for GetById with a filter
        /// </summary>
        /// <param name="id">Entity ID to return from database</param>
        /// <returns>DBEntity with referred ID</returns>
        [HttpGet("{id}")]
        public virtual async Task<T> Get(int id)
        {
            return await _service.Get(id);
        }
    }

    [ApiController]
    public abstract class CrudController<T> : ReadOnlyController<T> where T : Entity
    {
        protected readonly IService<T> _service;

        public CrudController(IService<T> service) : base(service)
        {
            _service = service;
        }

        /// <summary>
        /// Generic solution for a Inset into the database
        /// </summary>
        /// <param name="value">Entity to be inserted</param>
        /// <returns>The inserted entity</returns>
        [HttpPost]
        public virtual async Task<T> Post([FromBody] T value)
        {
            return await _service.Add(value);
        }

        /// <summary>
        /// Generic solution for a Update into the database
        /// </summary>
        /// <param name="id">Entity's ID to be updated</param>
        /// <param name="value">Entity to be updated</param>
        /// <returns>The updated entity</returns>
        [HttpPut("{id}")]
        public virtual async Task<T> Put(int id, [FromBody] T value)
        {
            return await _service.Update(id, value);
        }

        /// <summary>
        /// Generic solution for a Delete into the database
        /// </summary>
        /// <param name="id">Entity's ID to be deleted</param>
        /// <returns>The number of items deleted</returns>
        [HttpDelete("{id}")]
        public virtual async Task<int> Delete(int id)
        {
            return await _service.Delete(id);
        }
    }
}
