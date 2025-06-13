namespace Onboarding.Models
{
    public class TestTask
    {
        public int TestId { get; set; }
        public Test Test { get; set; }

        public int TaskId { get; set; }
        public Task Task { get; set; }
    }

}
