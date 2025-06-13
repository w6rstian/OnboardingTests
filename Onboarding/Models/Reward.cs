using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Onboarding.Models
{
    public class Reward
    {
        public int Id { get; set; }
        public int Giver { get; set; }
        public int Receiver { get; set; }
        public int Rating { get; set; }
        public string Feedback { get; set; }
        public DateTime CreatedAt { get; set; }

        public User GiverUser { get; set; }
        public User ReceiverUser { get; set; }
    }
}
