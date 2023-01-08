using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DinkToPdf;
using DinkToPdf.Contracts;
using RazorLight;

namespace AnnualHealthCheckJs.Services
{
    using Core;
    using ViewModels;

    public class PdfService : IPdfService
    {
        private readonly IRazorLightEngine _razorEngine;
        private readonly IConverter _pdfConverter;

        public PdfService(IRazorLightEngine razorEngine, IConverter pdfConverter)
        {
            _razorEngine = razorEngine;
            _pdfConverter = pdfConverter;
        }

        public async Task<byte[]> CreateAsync()
        {
            string template = await _razorEngine.CompileRenderAsync("pdfs/testr.cshtml", new NewAccountVM());

            var globalSettings = new GlobalSettings
            {
                ColorMode = ColorMode.Color,
                Orientation = Orientation.Portrait,
                PaperSize = PaperKind.A4,
                Margins = new MarginSettings() { Top = 0, Bottom = 0, Left = 0, Right = 0 },
                DocumentTitle = "Annual Heath Check Reference Letter",
            };
            var objectSettings = new ObjectSettings
            {
                PagesCount = true,
                HtmlContent = template,
                WebSettings = { DefaultEncoding = "utf-8",  },
            };

            var pdf = new HtmlToPdfDocument()
            {
                GlobalSettings = globalSettings,
                Objects = { objectSettings }
            };

            byte[] file = _pdfConverter.Convert(pdf);
            return file;
        }


        public byte[] Create(string template)
        {
            //var model = Data.CarRepository.GetCars();
            //var templatePath = Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), $"Templates/PDFTemplate.cshtml");
            //string template = await _razorEngine.CompileRenderAsync(templatePath, model);

            var globalSettings = new GlobalSettings
            {
                ColorMode = ColorMode.Color,
                Orientation = Orientation.Portrait,
                PaperSize = PaperKind.A4,
                Margins = new MarginSettings() { Top = 0, Bottom = 0, Left = 0, Right = 0 },
                DocumentTitle = "Annual Heath Check Reference Letter",
            };
            var objectSettings = new ObjectSettings
            {
                PagesCount = true,
                HtmlContent = template,
                WebSettings = { DefaultEncoding = "utf-8" },
                //HeaderSettings = { FontName = "Arial", FontSize = 12, Line = true, Center = "Fun pdf document" },
                //FooterSettings = { FontName = "Arial", FontSize = 12, Line = true, Right = "Page [page] of [toPage]" }
            };

            var pdf = new HtmlToPdfDocument()
            {
                GlobalSettings = globalSettings,
                Objects = { objectSettings }
            };

            byte[] file = _pdfConverter.Convert(pdf);
            return file;
        }


        public byte[] CreateFromUrl(string Url)
        {
            var globalSettings = new GlobalSettings
            {
                ColorMode = ColorMode.Color,
                Orientation = Orientation.Portrait,
                PaperSize = PaperKind.A4,
                Margins = new MarginSettings() { Top = 0, Bottom = 0, Left = 0, Right = 0 },
                DocumentTitle = "Annual Heath Check Reference Letter",
            };
            var objectSettings = new ObjectSettings
            {
                Page = Url,
            };

            var pdf = new HtmlToPdfDocument()
            {
                GlobalSettings = globalSettings,
                Objects = { objectSettings }
            };

            byte[] file = _pdfConverter.Convert(pdf);
            return file;

        }
    }
}
