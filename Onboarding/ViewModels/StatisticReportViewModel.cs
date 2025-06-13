using Onboarding.Models;

public class StatisticReportViewModel
{
    public int TotalUsersCount { get; set; }
    public List<string> Roles { get; set; }
    public Dictionary<string, int> UserCountsByRole { get; set; }
    public string SelectedRole { get; set; }
    public int? SelectedUserId { get; set; }
    public object RoleDetails { get; set; }
    public List<User> NewUsers { get; set; }

    public List<CourseReportModel> Courses { get; set; }
    public List<NewUserCourseModel> NewUsersInCourses { get; set; }
}

public class CourseReportModel
{
    public int Id { get; set; }
    public string Name { get; set; }
    public int TaskCount { get; set; }
    public string ManagerName { get; set; }
    public List<TaskReportModel> Tasks { get; set; }
}

public class TaskReportModel
{
    public int Id { get; set; }
    public string Title { get; set; }
    public string MentorName { get; set; }
}

public class NewUserCourseModel
{
    public int UserId { get; set; }
    public string UserName { get; set; }
    public string CourseName { get; set; }
}
