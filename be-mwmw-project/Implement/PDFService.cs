using Microsoft.Playwright;

public class PdfService : IPdfService
{
    public async Task<byte[]> CreateAsync(string html)
    {
        using var playwright = await Playwright.CreateAsync();
        await using var browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions { Headless = true });
        //Browser Launch
        //เริ่มต้น Playwright และเปิดเบราว์เซอร์ Chromium แบบ headless (ไม่แสดง UI) 🌐

        var page = await browser.NewPageAsync();
        await page.EmulateMediaAsync(new PageEmulateMediaOptions { Media = Media.Screen });
        //Page Setup
        //สร้างหน้าเว็บใหม่และตั้งค่าให้จำลองเป็นหน้าจอ

        await page.SetContentAsync(html, new PageSetContentOptions() { WaitUntil = WaitUntilState.Load });
        //Content Loading
        //โหลด HTML content ลงในหน้าเว็บและรอให้โหลดเสร็จสมบูรณ์

        return await page.PdfAsync(new PagePdfOptions { Format = "A4" });
        //แปลงหน้าเว็บเป็น PDF ขนาด A4 และคืนค่าเป็น byte array

        //HTML string → เปิดเบราว์เซอร์ → โหลด HTML → แปลงเป็น PDF → คืนค่าไฟล์ PDF เป็น byte array
    }
}