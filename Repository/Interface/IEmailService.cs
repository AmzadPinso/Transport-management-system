using System.Threading.Tasks;

namespace Transport_Management_System.Repository.Interface
{
    public interface IEmailService
    {
        Task SendEmailAsync(string toEmail, string subject, string htmlMessage);
    }
}
