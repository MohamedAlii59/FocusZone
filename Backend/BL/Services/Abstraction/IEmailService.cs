using System;
using System.Collections.Generic;
using System.Text;

namespace BL.Services.Abstraction
{
    public interface IEmailService
    {
        Task SendEmailAsync(string to, string subject, string htmlBody);
    }
}
