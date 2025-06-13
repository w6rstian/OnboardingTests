using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Onboarding.Models
{
    public class CourseTask
    {
        public int Id { get; set; }
        public byte[] Upload { get; set; }
        public char Completed { get; set; }
        public int TaskId { get; set; }
        public int UserCourseId { get; set; }
        public int TestResult { get; set; }

        public Task Task { get; set; }
        public UserCourse UserCourse { get; set; }
    }
}
