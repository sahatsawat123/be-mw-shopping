using Microsoft.AspNetCore.Mvc;

namespace Shopping.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SamplePdfTemplateController : ControllerBase
    {
        private readonly IPdfService _pdfService;

        private readonly IPdfTemplate _template;

         public SamplePdfTemplateController(IPdfService pdfService, IPdfTemplate template)
        {
            _pdfService = pdfService;
            _template = template;
        }

        [HttpPost]
        public async Task<IActionResult> Create()
        {
            string purchaseOrderHtml = _template.GenerateHtml();
            byte[] bytes = await _pdfService.CreateAsync(purchaseOrderHtml);
            await System.IO.File.WriteAllBytesAsync($"purchase_order.pdf", bytes);

            return File(bytes, "application/pdf", "purchase_order.pdf");
        }
    }
}