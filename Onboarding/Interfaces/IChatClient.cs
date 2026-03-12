using System.Threading.Tasks;

namespace Onboarding.Interfaces
{
    public interface IChatClient
    {
        Task ReceiveMessage(string messageContent, string sentAt, int senderId);
    }
}
