using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using RazorLight;

using Hangfire;

using Flurl;
using Flurl.Http;

namespace AnnualHealthCheckJs.Services
{
    using Services.Core;
    using Tools;
    using ViewModels;

    public class SmsSender : ISmsSender
    {
        private readonly IHostingEnvironment _environment;
        private readonly IRazorLightEngine _engine;

        public SmsSender(IHostingEnvironment env, IRazorLightEngine engine)
        {
            _environment = env;
            _engine = engine;
        }

        public void SendSms(SMSModel model)
        {
            string formatedphone = model.Recipient.FormatPhoneNumber().UnformatPhoneNumber();
            string smsProviderUrlString = "https://www.estoresms.com/smsapi.php";

            smsProviderUrlString.SetQueryParams(new
            {
                username = "bigross",
                password = "n0thin9",
                sender = model.SenderID,
                recipient = formatedphone,
                message = model.Message,
            }).GetStringAsync().GetAwaiter().GetResult();
        }

        public void SendSmsInBackground(SMSModel model)
        {
            var jobId = BackgroundJob.Enqueue(() => SendSms(model));
        }

        public async Task SendSmsAsync(string number, string SenderID, string message)
        {

            string formatedphone = number.FormatPhoneNumber().UnformatPhoneNumber();
            string smsProviderUrlString = "http://www.estoresms.com/smsapi.php";

            try
            {
                await smsProviderUrlString.SetQueryParams(new
                {
                    username = "bigross",
                    password = "n0thin9",
                    sender = SenderID,
                    recipient = formatedphone,
                    message = message
                }).GetStringAsync();

            }
            catch (Exception ex)
            {
            }
        }

        public async Task SendForgetPinAsync(string number, string senderid, tForgotPinVM tvm)
        {
            string result = await _engine.CompileRenderAsync("ForgotPin_SMS.cshtml", tvm);

            await SendSmsAsync(number, senderid, result);
        }

        public void SendForgetPINBackground(string number, string senderid, tForgotPinVM tvm)
        {
            string result = _engine.CompileRenderAsync("ForgotPin_SMS.cshtml", tvm).GetAwaiter().GetResult();

            SendSmsInBackground(new SMSModel() { Recipient = number, SenderID = senderid, Message = result });
        }
    }
}
