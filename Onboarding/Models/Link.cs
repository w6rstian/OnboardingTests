using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Onboarding.Models
{
    public class Link
    {
        public int Id { get; set; }
        public string LinkUrl { get; set; }
        public string Name { get; set; }
        public int TaskId { get; set; }

        public Task Task { get; set; }
    }
}
