using CrudSimplex.Models;
using System.Collections.Generic;

namespace CrudSimplex.Interfaces
{
	public interface ICrudBaseService<T>
	{
		bool DoesObjectExists(string id);
		ValidationResult ValidateInsert(T model);
		ValidationResult ValidateDelete(string id);
		ValidationResult ValidateUpdate(string id, T model);
		CreationModel<T> Insert(T model);
		T Update(string id, T model);
		void Delete(string id);
		IEnumerable<T> Select();
		T Select(string id);
	}
}
