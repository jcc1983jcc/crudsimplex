using AutoMapper;
using CrudSimplex.Interfaces;
using CrudSimplex.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Linq.Expressions;
using System.Reflection;

namespace CrudSimplex.EntityFramework
{
	public abstract class BaseCrudQueryableContextService<TModel, TEntity> : ICrudBaseService<TModel> where TEntity : class
	{
		private readonly IMapper _mapper;
		private readonly DbContext _dbContext;
		protected abstract string EntityKey { get; }
		protected abstract Func<TModel, string> ModelKeyGetter { get; }
		protected virtual IQueryable<TEntity> FullElements => _dbContext.Set<TEntity>();
		protected virtual Expression<Func<TModel, bool>> ExtraUserDynamicFilter => model => true;
		protected virtual string EntityComparer => $"{EntityKey} == @0";
		protected virtual bool GenerateGuidOnCreate => false;

		protected BaseCrudQueryableContextService(IMapper mapper, DbContext dbContext)
		{
			_mapper = mapper;
			_dbContext = dbContext;
		}

		public virtual void Delete(string id)
		{
			var entity = _dbContext.Set<TEntity>().Where(ExtraUserDynamicFilter.ToString()).Single(EntityComparer, id);
			_dbContext.Remove(entity);
			_dbContext.SaveChanges();
		}

		#region Validation
		public virtual bool DoesObjectExists(string id) => FetchSingleEntity(id).Any();
		public virtual bool DoesObjectExists(TModel model) => FetchSingleEntity(ModelKeyGetter(model)).Any();
		public virtual ValidationResult ValidateInsert(TModel model) => new ValidationResult(ValidateModel(model).ToArray());
		public virtual ValidationResult ValidateDelete(string id) => new ValidationResult();
		public virtual ValidationResult ValidateUpdate(string id, TModel model) => new ValidationResult { Errors = ValidateModel(model) };
		protected virtual IEnumerable<string> ValidateModel(TModel model) => new string[0];
		#endregion

		public virtual CreationModel<TModel> Insert(TModel model)
		{
			var entity = Merge(model);
			GenerateIdentifier(entity);
			_dbContext.Add(entity);
			_dbContext.SaveChanges();
			var createdModel = _mapper.Map<TModel>(entity);
			return new CreationModel<TModel>
			{
				Object = createdModel,
				Identifier = ModelKeyGetter(createdModel)
			};
		}

		public virtual IEnumerable<TModel> Select()
		{
			return FullElements.Where(ExtraUserDynamicFilter.ToString()).Select(_mapper.Map<TModel>);
		}

		public virtual TModel Select(string id)
		{
			return _mapper.Map<TModel>(GetEntity(id));
		}

		public virtual TModel Update(string id, TModel model)
		{
			var entity = GetEntity(id);
			Merge(model, entity);
			_dbContext.SaveChanges();
			return _mapper.Map<TModel>(entity);
		}

		public virtual TEntity GetEntity(string id) => FetchSingleEntity(id).Single();
		protected virtual Func<string, IQueryable<TEntity>> FetchSingleEntity => id =>
			FullElements.Where(ExtraUserDynamicFilter).Where(EntityComparer, id);
		protected virtual TEntity Merge(TModel model = default, TEntity entity = default) => _mapper.Map(model, entity);

		protected virtual void GenerateIdentifier(TEntity entity)
		{
			if (GenerateGuidOnCreate)
			{
				PropertyInfo prop = entity.GetType().GetProperty(EntityKey, BindingFlags.Public | BindingFlags.Instance);
				prop.SetValue(entity, Guid.NewGuid().ToString(), null);
			}
		}
	}
}
