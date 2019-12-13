using System;
using System.Collections.Generic;
using System.Text;

namespace CrudSimplex.Models
{
	public class CreationModel<T>
	{
		public string Identifier { get; set; }
		public T Object { get; set; }
	}
}
