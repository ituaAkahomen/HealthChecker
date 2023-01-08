using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AnnualHealthCheckJs.Services.Core
{
    public interface IPdfService
    {
        Task<byte[]> CreateAsync();
        byte[] Create(string template);
        byte[] CreateFromUrl(string Url);
    }
}
