using System;
using System.Collections.Generic;
using System.Text;

namespace CrudSimplex.Models
{
	public class ValidationResult
	{
		public IEnumerable<string> Errors { get; set; }
		public ValidationCodeEnum ValidationCode { get; set; }
		public bool IsValid => ValidationCode == ValidationCodeEnum.Ok;
		public ValidationResult(ValidationCodeEnum validationCode, params string []  errors)
		{
			ValidationCode = validationCode;
			Errors = errors;
		}

		public ValidationResult(params string [] errors)
		{
			Errors = errors;
			ValidationCode = errors.Length == 0 ? ValidationCodeEnum.Ok : ValidationCodeEnum.BadRequest;
		}
	}
}
