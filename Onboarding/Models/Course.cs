using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Onboarding.Models
{
    public class Course
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int? MentorId { get; set; } 
        public User Mentor { get; set; }
        public byte[] Image { get; set; }
        public string ImageMimeType { get; set; }

		public ICollection<UserCourse> UserCourses { get; set; } = new List<UserCourse>();
        public ICollection<Task> Tasks { get; set; } = new List<Task>();
        public ICollection<Test> Tests { get; set; } = new List<Test>();
	}
}
