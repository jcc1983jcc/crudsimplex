using CrudSimplex.Interfaces;
using Microsoft.AspNet.OData.Query;
using Microsoft.AspNetCore.Mvc;
using System.Linq;

namespace CrudSimplex
{
	public abstract class BaseCrudController<TModel> : ControllerBase
	{
		private readonly ICrudBaseService<TModel> _service;

		public BaseCrudController(ICrudBaseService<TModel> service)
		{
			_service = service;
		}

		[HttpGet]
		[Route("{id}")]
		public IActionResult Get(string id)
		{
			if (!_service.DoesObjectExists(id))
			{
				return NotFound();
			}
			return Ok(_service.Select(id));
		}


		[HttpGet]
		[Route("")]
		public virtual IActionResult Get(ODataQueryOptions<TModel> query)
		{
			var data = _service.Select().AsQueryable();
			return Ok(new
			{
				Count = query.Count?.GetEntityCount(query.Filter?.ApplyTo(data, new ODataQuerySettings()) ?? data),
				Items = query.ApplyTo(data)
			});
		}



		[HttpPost]
		[Route("")]
		public virtual IActionResult Post([FromBody] TModel model)
		{
			var validation = _service.ValidateInsert(model);
			if (!validation.IsValid)
			{
				return BadRequest(validation.Errors);
			}
			var creation = _service.Insert(model);
			return Created(HttpContext.Request.Path + "/" + creation.Identifier, creation.Object);
		}

		[HttpPut]
		[Route("{id}")]
		public virtual IActionResult Put(string id, [FromBody] TModel model)
		{
			if (!_service.DoesObjectExists(id))
			{
				return NotFound("Object to Update Does Not Exists");
			}
			var validation = _service.ValidateUpdate(id, model);
			if (!validation.IsValid)
			{
				return BadRequest(validation.Errors);
			}
			_service.Update(id, model);
			return NoContent();
		}

		[HttpDelete]
		[Route("{id}")]
		public virtual IActionResult Delete(string id)
		{
			if (!_service.DoesObjectExists(id))
			{
				return NotFound("Object to Delete Does Not Exists");
			}
			var validation = _service.ValidateDelete(id);
			if (!validation.IsValid)
			{
				return BadRequest(validation.Errors);
			}
			_service.Delete(id);
			return NoContent();
		}
	}
}

