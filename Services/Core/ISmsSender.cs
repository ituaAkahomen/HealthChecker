using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AnnualHealthCheckJs.Services.Core
{
    using Tools;
    using ViewModels;

    public interface ISmsSender
    {
        Task SendSmsAsync(string number, string senderid, string message);

        Task SendForgetPinAsync(string number, string senderid, tForgotPinVM tvm);

        void SendSmsInBackground(SMSModel model);

        void SendForgetPINBackground(string number, string senderid, tForgotPinVM tvm);

    }
}
