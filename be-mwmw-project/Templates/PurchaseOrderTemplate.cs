// Templates/PurchaseOrderTemplate.cs
using System.Text;

public class PurchaseOrderTemplate : IPdfTemplate
{
    private string GetLogoBase64()
        {
            try
            {
                // วางไฟล์ logo ใน wwwroot/images/logo.png
                var logoPath = Path.Combine(Directory.GetCurrentDirectory(), "images", "KSlogo.jpg");
                
                if (File.Exists(logoPath))
                {
                    byte[] logoBytes = File.ReadAllBytes(logoPath);
                    return Convert.ToBase64String(logoBytes);
                }
            }
            catch (Exception ex)
            {
                // Log error if needed
                Console.WriteLine($"Error loading logo: {ex.Message}");
            }
            
            // ถ้าไม่มีไฟล์หรือเกิด error ให้ return empty string
            return string.Empty;
        }
    public string GetPurchaseOrderTemplate()
    {
        string currentDate = DateTime.Now.ToString("dd/MM/yyyy");
        string logoBase64 = GetLogoBase64();
            
            // ตรวจสอบว่ามีรูปหรือไม่ และสร้าง HTML แยกกัน
            string logoSection = "";
            if (!string.IsNullOrEmpty(logoBase64))
            {
                logoSection = $"<img src=\"data:image/png;base64,{logoBase64}\" alt=\"Company Logo\" style=\"max-width: 200px; max-height: 80px; object-fit: contain;\" />";
            }
            else
            {
                logoSection = "<h1 style=\"font-size: 24px; color: #6699CC; margin: 0; letter-spacing: 2px;\">PURCHASE ORDER</h1>";
            }


        return @"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <title>Purchase Order</title>
    <style>
        body {
            font-family: Arial, sans-serif;
            font-size: 12px;
            margin: 20px;
            padding: 20px;
            color: #333;
        }
        .header {
            display: flex;
            justify-content: space-between;
            align-items: flex-start;
            margin-bottom: 20px;
        }
        .company-info {
            flex: 2;
            display: flex;
            align-items: flex-start;
            gap: 20px;
        }
        .company-text {
            flex: 1;
        }
        .company-logo {
            flex-shrink: 0;
        }
        .po-details {
            text-align: right;
            flex: 1;
        }
        .section-header {
            background-color: #4A6FA5;
            color: white;
            padding: 8px;
            font-weight: bold;
            margin-top: 10px;
        }
        .vendor-ship-container {
            display: flex;
            gap: 20px;
            margin-bottom: 20px;
        }
        .vendor, .shipto {
            flex: 1;
        }
        .vendor-content, .shipto-content {
            border: 1px solid #ccc;
            padding: 10px;
            min-height: 100px;
            background-color: #f9f9f9;
        }
        .ship-terms {
            display: flex;
            margin-bottom: 10px;
        }
        .ship-terms > div {
            flex: 1;
            border: 1px solid #ccc;
            padding: 8px;
            text-align: center;
            background-color: #4A6FA5;
            color: white;
            font-weight: bold;
        }
        .ship-terms-content {
            display: flex;
            border: 1px solid #ccc;
            margin-bottom: 20px;
        }
        .ship-terms-content > div {
            flex: 1;
            padding: 10px;
            border-right: 1px solid #ccc;
        }
        .ship-terms-content > div:last-child {
            border-right: none;
        }
        .items-table {
            width: 100%;
            border-collapse: collapse;
            margin-bottom: 20px;
            table-layout: fixed;
        }
        .items-table th {
            background-color: #4A6FA5;
            color: white;
            padding: 10px;
            text-align: left;
            border: 1px solid #ccc;
        }
        .items-table th:nth-child(1) { width: 15%; }
        .items-table th:nth-child(2) { width: 40%; }
        .items-table th:nth-child(3) { width: 10%; }
        .items-table th:nth-child(4) { width: 15%; }
        .items-table th:nth-child(5) { width: 20%; }
        .items-table td {
            padding: 8px;
            border: 1px solid #ccc;
            text-align: left;
        }
        .items-table tr:nth-child(even) {
            background-color: #f9f9f9;
        }
        .total-section {
            display: flex;
            gap: 20px;
            margin-top: 20px;
        }
        .comments {
            flex: 1;
            border: 1px solid #ccc;
            padding: 10px;
            min-height: 100px;
            background-color: #f0f0f0;
        }
        .comments-header {
            background-color: #888;
            color: white;
            padding: 5px;
            margin: -10px -10px 10px -10px;
            font-weight: bold;
        }
        .totals {
            width: 200px;
        }
        .totals table {
            width: 100%;
            border-collapse: collapse;
        }
        .totals td {
            padding: 5px;
            border: 1px solid #ccc;
            text-align: right;
        }
        .totals .total-row {
            background-color: #E6E6FA;
            font-weight: bold;
        }
        .footer {
            text-align: center;
            margin-top: 30px;
            font-size: 11px;
            color: #666;
        }
        .number-right {
            text-align: right;
        }
    </style>
</head>
<body>
    <div class='header'>
        <div class='company-info'>
            <div class='company-logo'>" + logoSection + @"</div>
            <div class='company-text'>
                <strong>KS Company Limited</strong><br>
                284/1 Soi Kosum Ruam Chai 24, Don<br>
                Mueang SubDistrict, Don Mueang District<br>
                Bangkok 10210<br>
                Phone: 099-498-4422
            </div>
        </div>
        <div class='po-details'>
            DATE: " + currentDate + @"<br>
            PO #: [123456]
        </div>
    </div>

    <div class='vendor-ship-container'>
        <div class='vendor'>
            <div class='section-header'>VENDOR</div>
            <div class='vendor-content'>
                [Company Name]<br>
                [Contact or Department]<br>
                [Street Address]<br>
                [City, ST ZIP]<br>
                Phone: (000) 000-0000<br>
            </div>
        </div>
        <div class='shipto'>
            <div class='section-header'>SHIP TO</div>
            <div class='shipto-content'>
                [Name]<br>
                [Company Name]<br>
                [Street Address]<br>
                [City, ST ZIP]<br>
                [Phone]
            </div>
        </div>
    </div>
    <table class='items-table'>
        <thead>
            <tr>
                <th>ITEM </th>
                <th>PRODUCT NAME</th>
                <th>QTY</th>
                <th>UNIT PRICE</th>
                <th>TOTAL</th>
            </tr>
        </thead>
        <tbody>
            <tr>
                <td>123456789</td>
                <td>Product XYZ</td>
                <td class='number-right'>15</td>
                <td class='number-right'>150.00</td>
                <td class='number-right'>2,250.00</td>
            </tr>
            <tr>
                <td>456456456</td>
                <td>Product ABC</td>
                <td class='number-right'>1</td>
                <td class='number-right'>75.00</td>
                <td class='number-right'>75.00</td>
            </tr>
            <tr><td>&nbsp;</td><td>&nbsp;</td><td>&nbsp;</td><td>&nbsp;</td><td>&nbsp;</td></tr>
            <tr><td>&nbsp;</td><td>&nbsp;</td><td>&nbsp;</td><td>&nbsp;</td><td>&nbsp;</td></tr>
            <tr><td>&nbsp;</td><td>&nbsp;</td><td>&nbsp;</td><td>&nbsp;</td><td>&nbsp;</td></tr>
            <tr><td>&nbsp;</td><td>&nbsp;</td><td>&nbsp;</td><td>&nbsp;</td><td>&nbsp;</td></tr>
            <tr><td>&nbsp;</td><td>&nbsp;</td><td>&nbsp;</td><td>&nbsp;</td><td>&nbsp;</td></tr>
            <tr><td>&nbsp;</td><td>&nbsp;</td><td>&nbsp;</td><td>&nbsp;</td><td>&nbsp;</td></tr>
            <tr><td>&nbsp;</td><td>&nbsp;</td><td>&nbsp;</td><td>&nbsp;</td><td>&nbsp;</td></tr>
            <tr><td>&nbsp;</td><td>&nbsp;</td><td>&nbsp;</td><td>&nbsp;</td><td>&nbsp;</td></tr>
            <tr><td>&nbsp;</td><td>&nbsp;</td><td>&nbsp;</td><td>&nbsp;</td><td>&nbsp;</td></tr>
            <tr><td>&nbsp;</td><td>&nbsp;</td><td>&nbsp;</td><td>&nbsp;</td><td>&nbsp;</td></tr>
            <tr><td>&nbsp;</td><td>&nbsp;</td><td>&nbsp;</td><td>&nbsp;</td><td>&nbsp;</td></tr>
            <tr><td>&nbsp;</td><td>&nbsp;</td><td>&nbsp;</td><td>&nbsp;</td><td>&nbsp;</td></tr>
            <tr><td>&nbsp;</td><td>&nbsp;</td><td>&nbsp;</td><td>&nbsp;</td><td>&nbsp;</td></tr>
            <tr><td>&nbsp;</td><td>&nbsp;</td><td>&nbsp;</td><td>&nbsp;</td><td>&nbsp;</td></tr>
        </tbody>
    </table>

    <div class='total-section'>
        <div class='comments'>
            <div class='comments-header'>Comments or Special Instructions</div>
            <div style='min-height: 80px;'></div>
        </div>
        <div class='totals'>
            <table>
                <tr>
                    <td>SUBTOTAL</td>
                    <td class='number-right'>2,325.00</td>
                </tr>
                <tr>
                    <td>TAX</td>
                    <td class='number-right'>-</td>
                </tr>
                <tr>
                    <td>SHIPPING</td>
                    <td class='number-right'>-</td>
                </tr>
                <tr>
                    <td>OTHER</td>
                    <td class='number-right'>-</td>
                </tr>
                <tr class='total-row'>
                    <td>TOTAL</td>
                    <td class='number-right'>$ 2,325.00</td>
                </tr>
            </table>
        </div>
    </div>

    <div class='footer'>
        If you have any questions about this purchase order, please contact<br>
        [Name, Phone #, E-mail]
    </div>
</body>
</html>";
    }

    public string GenerateHtml()
    {
        return GetPurchaseOrderTemplate();
    }

    public string GenerateHtml<T>(T data) where T : class
    {
        // ถ้าต้องการใช้ข้อมูลจริง ให้เรียกใช้ GetPurchaseOrderTemplate() 
        // แล้วแทนที่ placeholders ตามต้องการ
        return GetPurchaseOrderTemplate();
    }
}
