using Onboarding.Data.Enums;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace Onboarding.Models
{
    public class Task
    {
        public int Id { get; set; }
        public int MentorId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public int CourseId { get; set; }
        public Course Course { get; set; }
        //public ICollection<Test> Tests { get; set; }
        public ICollection<Article> Articles { get; set; } = new List<Article>();
        public ICollection<Link> Links { get; set; } = new List<Link>();
        public User Mentor { get; set; }

        public ICollection<CourseTask> CourseTasks { get; set; } = new List<CourseTask>();

	}
}
