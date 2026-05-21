namespace P4_Backend_Car_App.Interfaces
{
    public interface IMailService
    {
        public Task SendEmailAsync(string emailAddress, string subject, string body, bool isHtml = true);
    }
}
