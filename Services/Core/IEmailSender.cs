using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AnnualHealthCheckJs.Services.Core
{
    using Models;
    using Tools;
    using ViewModels;


    public interface IEmailSender
    {
        Task SendEmailAsync(string email, string subject, string message,
            bool hasPlainText = true, bool hasHtml = false,
            List<KeyValuePair<string, byte[]>> attachments = null);

        Task SendNewAccountWithRandomPassword(string email, string subject, NewAccountVM nvm);
        Task SendEmailConfirmationAsync(string email, string callbackUrl);
        Task SendForgetPinAsync(string email, string subject, tForgotPinVM tvm);

        void SendEmailInBackground(EmailModel model);
        void SendNewAccoundWithRandomPasswordInBackground(string email, string subject, NewAccountVM nvm);
        void SendEmailConfirmationInBackground(string email, string callbackUrl);

        void SendReferenceLetterViaEmailInBackground(SignUp signUp, string schemeHost);
        void SendReferenceLetterViaEmailCCInBackground(SignUp signUp, string schemeHost);
        void SendForgetPINBackground(string email, string subject, tForgotPinVM tvm);

        void SendPreCheckupEmailReminder(SignUp signUp);
        void SendPostCheckupEmailReminder(SignUp signUp, string schemeHost);
    }
}
