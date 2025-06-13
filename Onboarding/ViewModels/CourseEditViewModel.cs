using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.ComponentModel.DataAnnotations;

namespace Onboarding.ViewModels
{
	public class CourseEditViewModel
	{
		public int Id { get; set; }

		[Required(ErrorMessage = "Nazwa kursu jest wymagana.")]
		public string Name { get; set; }

		public IFormFile ImageFile { get; set; }

		[BindNever]
		public byte[]? ExistingImage { get; set; }

		[BindNever]
		public string? ExistingImageMimeType { get; set; }

	}
}
