using System.IO.Compression;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Shopping.Interface;
using Shopping.Models;

namespace Shopping.Controllers;

[ApiController]
[Route("[Controller]")]
public class SellController : ControllerBase
{
    IDapperController _dapper;
    private readonly IPdfService _pdfService;
    private readonly IPdfTemplate _template;

    public SellController(IConfiguration config, IPdfService pdfService, IPdfTemplate template)
    {
        _dapper = new DapperController(config);
        _pdfService = pdfService;
        _template = template;
    }

    [HttpGet("SearchSellOrder")]
    public IActionResult SearchSellOrder(
    
    [FromQuery] string? CustomerName = null,
    [FromQuery] string? SellNumber = null,
    [FromQuery] DateTime? orderDateFrom = null,
    [FromQuery] DateTime? orderDateTo = null,
    [FromQuery] int page = 1,
    [FromQuery] int pageSize = 10)
    {
    try
    {
        // Base Query ที่คุณใช้ (เพิ่มฟิลด์ Phone_No และ Tax_Id)
        var queryBuilder = new StringBuilder(@"
            SELECT 
                so.sell_id,
                so.sell_number,
                so.sell_date,
                so.total_price,
                so.vat_rate,
                so.remark,
                c.customer_id,
                c.customer_name,
                c.address,
                c.phone_no,
                c.tax_id
            FROM public.trn_sell so
            LEFT JOIN public.mst_customer c ON so.customer_id = c.customer_id");

        var conditions = new List<string>();
        
        // Search by Supplier Name
        if (!string.IsNullOrEmpty(CustomerName))
        {
            conditions.Add($" LOWER(c.customer_name) LIKE LOWER('%{CustomerName}%')");
        }

        // Search by Purchase Order Number
        if (!string.IsNullOrEmpty(SellNumber))
        {
            conditions.Add($" LOWER(so.sell_number) LIKE LOWER('%{SellNumber}%')");
        }

        // Search by Date Range
        if (orderDateFrom.HasValue)
        {
            conditions.Add($" so.order_date >= '{orderDateFrom.Value:yyyy-MM-dd}'");
        }

        if (orderDateTo.HasValue)
        {
            conditions.Add($" so.order_date <= '{orderDateTo.Value:yyyy-MM-dd}'");
        }

        if (conditions.Any())
        {
            queryBuilder.Append(" WHERE ");
            queryBuilder.Append(string.Join(" AND ", conditions));
        }

        queryBuilder.Append(" ORDER BY so.sell_date DESC, so.sell_number DESC");

        // รวม Conditions เข้า Query
        string finalQuery = queryBuilder.ToString();

        // Execute Query - ใช้ DTO ที่มีอยู่
        var SellOrders = _dapper.LoadData<Trn_Sell_OrderSearch>(finalQuery);

        // ดึง Order Details สำหรับแต่ละ Purchase Order
        var searchResults = new List<SellOrderRequestSearch>();

        foreach (var so in SellOrders)
        {
            // 🛍️ ดึง Order Details - เพิ่ม space ใน SQL
            string orderDetailsQuery = @"
                SELECT 
                    sod.sell_detail_id,
                    sod.sell_id,
                    sod.product_id,
                    sod.amount,
                    sod.price_per_unit,
                    p.product_name,
                    p.product_detail
                FROM public.trn_sell_detail sod
                LEFT JOIN public.mst_product p ON sod.product_id = p.product_id
                WHERE sod.sell_id = '" + so.Sell_Id.ToString() + "' ORDER BY sod.sell_detail_id";

            var orderDetails = _dapper.LoadData<SellOrderDetailsSearch>(orderDetailsQuery);

            // เพิ่มข้อมูลรวม
            searchResults.Add(new SellOrderRequestSearch
            {
                SearchSellOrder = so,
                SellOrderDetailsSearch = orderDetails.ToList()
            });
        }

        // จัดการ Pagination และ Format วันที่
        int totalRecords = searchResults.Count();
        int totalPages = (int)Math.Ceiling((double)totalRecords / pageSize);
        
        var pagedResults = searchResults
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new
            {
                x.SearchSellOrder.Sell_Id,
                x.SearchSellOrder.Sell_Number,
                Order_Date = x.SearchSellOrder.Sell_Date.ToString("yyyy-MM-dd"), // ✅ Format วันที่
                x.SearchSellOrder.Total_Price,
                x.SearchSellOrder.Vat_Rate,
                x.SearchSellOrder.Remark,
                x.SearchSellOrder.Customer_Id,
                x.SearchSellOrder.Customer_Name,
                x.SearchSellOrder.Address,
                x.SearchSellOrder.Phone_No,
                x.SearchSellOrder.Tax_Id,
                OrderDetails = x.SellOrderDetailsSearch.Select(detail => new // ✅ เพิ่ม OrderDetails
                {
                    detail.Sell_Detail_Id,
                    detail.Sell_Id,
                    detail.Product_Id,
                    detail.Product_Name,
                    detail.Product_Detail,
                    detail.Amount,
                    detail.Price_Per_Unit,
                    Line_Total = detail.Amount * detail.Price_Per_Unit
                }).ToList()
            }).ToList();

        // Return ผลลัพธ์
        return Ok(new
        {
            Success = true,
            Message = $"Found {totalRecords} Sell orders! ",
            Data = pagedResults,
            Pagination = new
            {
                CurrentPage = page,
                PageSize = pageSize,
                TotalRecords = totalRecords,
                TotalPages = totalPages

            },
            SearchCriteria = new
            {
                CustomerName = CustomerName,
                SellOrderNumber = SellNumber,
                OrderDateFrom = orderDateFrom?.ToString("yyyy-MM-dd"),
                OrderDateTo = orderDateTo?.ToString("yyyy-MM-dd")
            }
        });
    }
    catch (Exception ex)
    {
        return BadRequest(new
        {
            Success = false,
            Message = "Search failed! 😞",
            Error = ex.Message
        });
    }
    }

    [HttpPost("CreateSellOrder")]
    public async Task<IActionResult> SellPurchaseOrder([FromBody] SellOrderRequestDTO request)
    {
        decimal calculatedTotalPrice = 0;
        List<(Guid ProductId, int Amount, decimal Price)> orderItemsWithPrice = new List<(Guid, int, decimal)>();

        // Step 1: ตรวจสอบ Stock ก่อนทำทุกอย่าง 📦
        foreach (var detail in request.OrderDetails)
        {
            string checkInventorySql = @"SELECT product_id, amount FROM public.stk_inventory WHERE product_id = '" + detail.Product_Id.ToString() + "'";
            var existingProducts = _dapper.LoadData<Stk_inventory>(checkInventorySql);

            if (!existingProducts.Any())
            {
                return BadRequest(new
                {
                    Success = false,
                    Message = $"❌ Product not found in inventory: {detail.Product_Id}!",
                    ProductId = detail.Product_Id,
                    RequestedAmount = detail.Amount,
                    AvailableAmount = 0
                });
            }

            int currentStock = existingProducts.First().Amount;
            if (currentStock < detail.Amount)
            {
                return BadRequest(new
                {
                    Success = false,
                    Message = $"⚠️ Insufficient stock for product: {detail.Product_Id}! Available: {currentStock}, Requested: {detail.Amount}",
                    ProductId = detail.Product_Id,
                    RequestedAmount = detail.Amount,
                    AvailableAmount = currentStock,
                    ShortfallAmount = detail.Amount - currentStock
                });
            }
        }

        // Step 2: ดึงข้อมูล Customer 👤
        string getCustomerSql = @"SELECT customer_id, customer_name, address, phone_no 
                                 FROM public.mst_customer 
                                 WHERE customer_id = '" + request.SellOrder.Customer_Id.ToString() + "'";
        var customerResult = _dapper.LoadData<Mst_Customer>(getCustomerSql);

        if (!customerResult.Any())
        {
            return BadRequest(new
            {
                Success = false,
                Message = $"❌ Customer not found: {request.SellOrder.Customer_Id}!"
            });
        }

        var customerInfo = customerResult.First();

        // ตรวจสอบ Sell Number ซ้ำ 🔍
        string checkSellNumberSql = @"SELECT COUNT(*) FROM public.trn_sell WHERE sell_number = '" + request.SellOrder.Sell_Number + "'";
        var sellNumberCount = _dapper.LoadData<int>(checkSellNumberSql);

        if (sellNumberCount.Any() && sellNumberCount.First() > 0)
        {
            return BadRequest(new
            {
                Success = false,
                Message = $"Sell Number '{request.SellOrder.Sell_Number}' already exists!",
                SellNumber = request.SellOrder.Sell_Number,
                ErrorCode = "DUPLICATE_SELL_NUMBER"
            });
        }

        List<dynamic> productDetails = new List<dynamic>();

        // Step 3: ดึงราคาและคำนวณยอดรวม 💰
        foreach (var detail in request.OrderDetails)
        {
            string getProductPriceSql = @"SELECT price_per_unit, product_name, product_detail FROM public.mst_product WHERE product_id = '" + detail.Product_Id.ToString() + "'";
            var productResult = _dapper.LoadData<Mst_Product>(getProductPriceSql);

            if (productResult.Any())
            {
                var product = productResult.First();
                decimal productPrice = product.Price_Per_Unit;
                decimal TotalPrice = productPrice * detail.Amount;
                calculatedTotalPrice += TotalPrice;

                orderItemsWithPrice.Add((detail.Product_Id, detail.Amount, productPrice));
                productDetails.Add(new
                {
                    ProductCode = product.Product_Id,
                    ProductName = product.Product_Name,
                    Quantity = detail.Amount,
                    UnitPrice = productPrice,
                    LineTotal = TotalPrice
                });
            }
            else
            {
                return BadRequest(new
                {
                    Success = false,
                    Message = $"❌ Product not found: {detail.Product_Id}!"
                });
            }
        }

        // คำนวณ VAT 📊
        decimal totalWithVat = calculatedTotalPrice;
        decimal Vat = 0;
        if (request.SellOrder.Vat_Rate > 0)
        {
            Vat = calculatedTotalPrice * (request.SellOrder.Vat_Rate / 100);
            totalWithVat = calculatedTotalPrice + Vat;
        }

        var SellOrderId = Guid.NewGuid();

        // Step 4: สร้าง Sell Order 📝
        string sql = "INSERT INTO public.trn_sell " +
                     "(sell_id, sell_number, sell_date, customer_id, remark, total_price, vat_rate) " +
                     "VALUES ('" + SellOrderId + "', " +
                     "'" + request.SellOrder.Sell_Number.ToString() + "', " +
                     "'" + request.SellOrder.Sell_Date.ToString() + "', " +
                     "'" + request.SellOrder.Customer_Id.ToString() + "', " +
                     "'" + request.SellOrder.Remark.ToString() + "', " +
                     "'" + totalWithVat + "', " +
                     "'" + request.SellOrder.Vat_Rate + "')";

        if (!_dapper.ExecuteSQL(sql))
        {
            throw new Exception("❌ Failed to Create SellOrder");
        }

        int successfulDetails = 0;

        // Step 5: สร้าง Order Details และอัพเดต Stock 📋
        foreach (var detail in request.OrderDetails)
        {
            string getProductPriceSql = "SELECT price_per_unit FROM public.mst_product WHERE product_id = '" + detail.Product_Id.ToString() + "'";
            var priceResult = _dapper.LoadData<decimal>(getProductPriceSql);
            decimal productPrice = 0;
            if (priceResult.Any())
            {
                productPrice = priceResult.First();
            }

            string insertDetailSql = @"
                INSERT INTO public.trn_sell_detail 
                (sell_detail_id, sell_id, product_id, amount, price_per_unit) 
                VALUES ('" + detail.Sell_Detail_Id.ToString() + "', " +
                "'" + SellOrderId + "', " +
                "'" + detail.Product_Id.ToString() + "', " +
                "'" + detail.Amount + "', " +
                "'" + productPrice + "')";

            int detailResult = _dapper.ExecuteintSQL(insertDetailSql);
            if (detailResult == 0)
            {
                return BadRequest(new
                {
                    Success = false,
                    Message = $" Failed to create Sell Order Detail for Product: {detail.Product_Id}!",
                    FailedAtDetail = successfulDetails + 1
                });
            }

            // อัพเดต Stock
            string checkInventorySql = @"SELECT product_id, amount FROM public.stk_inventory WHERE product_id = '" + detail.Product_Id.ToString() + "'";
            var existingProducts = _dapper.LoadData<Stk_inventory>(checkInventorySql);

            if (existingProducts.Any())
            {
                int currentStock = existingProducts.First().Amount;
                if (currentStock >= detail.Amount)
                {
                    string updateInventorySql = @"
                        UPDATE public.stk_inventory 
                        SET amount = amount - " + detail.Amount + @" 
                        WHERE product_id = '" + detail.Product_Id.ToString() + "'";

                    int updateResult = _dapper.ExecuteintSQL(updateInventorySql);
                    if (updateResult == 0)
                    {
                        return BadRequest(new
                        {
                            Success = false,
                            Message = $"❌ Failed to update inventory for Product: {detail.Product_Id}!",
                            FailedAtDetail = successfulDetails + 1
                        });
                    }
                }
                else
                {
                    return BadRequest(new
                    {
                        Success = false,
                        Message = $"⚠️ Stock changed during transaction! Product: {detail.Product_Id}, Available: {currentStock}, Requested: {detail.Amount}",
                        FailedAtDetail = successfulDetails + 1
                    });
                }
            }
            else
            {
                return BadRequest(new
                {
                    Success = false,
                    Message = $"❌ Product not found in inventory: {detail.Product_Id}!",
                    FailedAtDetail = successfulDetails + 1
                });
            }

            successfulDetails++;
        }

        // Step 6: สร้าง Invoice Record 🧾
        string InsertInvoice = "INSERT INTO public.trn_invoice " +
                              "(invoice_number, sell_id) " +
                              "VALUES ('" + request.SellOrder.Sell_Number.ToString() + "', " +
                              "'" + SellOrderId + "')";

        int InvoiceResult = _dapper.ExecuteintSQL(InsertInvoice);
        if (InvoiceResult == 0)
        {
            return BadRequest(new
            {
                Success = false,
                Message = $"❌ Failed to create Invoice!",
                FailedAtDetail = successfulDetails + 1
            });
        }

        // Step 7: ตรวจสอบ PDF Options
        bool generateInvoicePdf = request.GenerateInvoicePdf ?? false;
        bool generateSellOrderPdf = true;
        // SellOrder PDF ต้องดาวน์โหลดทุกครั้งเสมอ!

        try
        {
            // PDF Priority Logic: ถ้าเลือก Invoice PDF ให้ดาวน์โหลด Invoice แทน
            if (generateInvoicePdf && generateSellOrderPdf)
            {
                return await GeneratePdfZip(request, customerInfo, productDetails, calculatedTotalPrice, Vat, totalWithVat, SellOrderId);
            }
            else
            {
                // สร้าง Sell Order PDF และส่งให้ download (Default) 📋
                return await GenerateSellOrderPdf(request, customerInfo, productDetails, calculatedTotalPrice, Vat, totalWithVat);
            }
        }
        catch (Exception ex)
        {
            // ถ้าสร้าง PDF ไม่สำเร็จ ส่ง Response ปกติ
            return Ok(new
            {
                Success = true,
                Message = "✅ Sell order created successfully! (PDF generation failed)",
                SellOrderId = SellOrderId,
                TotalAmount = totalWithVat,
                ProcessedItems = successfulDetails,
                PdfError = ex.Message
            });
        }
    }

    private async Task<IActionResult> GeneratePdfZip(SellOrderRequestDTO request, Mst_Customer customerInfo, 
    List<dynamic> productDetails, decimal subtotal, decimal vat, decimal total, Guid sellOrderId)
    {
    // สร้าง SellOrder HTML
    string sellOrderHtml = GetSellOrderTemplate(
        request.SellOrder.Sell_Number,
        request.SellOrder.Sell_Date,
        customerInfo,
        productDetails,
        subtotal,
        vat,
        total,
        request.SellOrder.Remark
    );

    // สร้าง Invoice HTML
    string invoiceHtml = GetInvoiceTemplate(
        request.SellOrder.Sell_Number,
        request.SellOrder.Sell_Date,
        customerInfo,
        productDetails,
        subtotal,
        vat,
        total,
        request.SellOrder.Remark,
        sellOrderId
    );

    // สร้าง PDF Files
    byte[] sellOrderPdf = await _pdfService.CreateAsync(sellOrderHtml);
    byte[] invoicePdf = await _pdfService.CreateAsync(invoiceHtml);

    // สร้าง ZIP File
    using (var zipStream = new MemoryStream())
    {
        using (var archive = new ZipArchive(zipStream, ZipArchiveMode.Create, true))
        {
            // เพิ่ม SellOrder PDF เข้า ZIP
            var sellOrderEntry = archive.CreateEntry($"SellOrder_{request.SellOrder.Sell_Number}_{DateTime.Now:yyyyMMdd}.pdf");
            using (var entryStream = sellOrderEntry.Open())
            {
                await entryStream.WriteAsync(sellOrderPdf, 0, sellOrderPdf.Length);
            }

            // เพิ่ม Invoice PDF เข้า ZIP
            var invoiceEntry = archive.CreateEntry($"Invoice_{request.SellOrder.Sell_Number}_{DateTime.Now:yyyyMMdd}.pdf");
            using (var entryStream = invoiceEntry.Open())
            {
                await entryStream.WriteAsync(invoicePdf, 0, invoicePdf.Length);
            }
        }

        string zipFileName = $"{request.SellOrder.Sell_Number}_{DateTime.Now:yyyyMMdd}.zip";
        return File(zipStream.ToArray(), "application/zip", zipFileName);
        }
    }
    // Method สำหรับสร้าง Invoice PDF
    private async Task<IActionResult> GenerateInvoicePdf(SellOrderRequestDTO request, Mst_Customer customerInfo,
        List<dynamic> productDetails, decimal subtotal, decimal vat, decimal total, Guid sellOrderId)
    {
        string invoiceHtml = GetInvoiceTemplate(
            request.SellOrder.Sell_Number,
            request.SellOrder.Sell_Date,
            customerInfo,
            productDetails,
            subtotal,
            vat,
            total,
            request.SellOrder.Remark,
            sellOrderId
        );

        byte[] pdfBytes = await _pdfService.CreateAsync(invoiceHtml);
        string fileName = $"Invoice_{request.SellOrder.Sell_Number}_{DateTime.Now:yyyyMMdd}.pdf";

        return File(pdfBytes, "application/pdf", fileName);
    }

    // Method สำหรับสร้าง Sell Order PDF
    private async Task<IActionResult> GenerateSellOrderPdf(SellOrderRequestDTO request, Mst_Customer customerInfo,
        List<dynamic> productDetails, decimal subtotal, decimal vat, decimal total)
    {
        string sellOrderHtml = GetSellOrderTemplate(
            request.SellOrder.Sell_Number,
            request.SellOrder.Sell_Date,
            customerInfo,
            productDetails,
            subtotal,
            vat,
            total,
            request.SellOrder.Remark
        );

        byte[] pdfBytes = await _pdfService.CreateAsync(sellOrderHtml);
        string fileName = $"SellOrder_{request.SellOrder.Sell_Number}_{DateTime.Now:yyyyMMdd}.pdf";

        return File(pdfBytes, "application/pdf", fileName);
    }

    // Method สำหรับโหลด Logo
    private string GetLogoBase64()
    {
        try
        {
            var logoPath = Path.Combine(Directory.GetCurrentDirectory(), "images", "KSlogo.jpg");
            if (System.IO.File.Exists(logoPath))
            {
                byte[] logoBytes = System.IO.File.ReadAllBytes(logoPath);
                return Convert.ToBase64String(logoBytes);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading logo: {ex.Message}");
        }
        return string.Empty;
    }

    // Template สำหรับ Invoice PDF
    private string GetInvoiceTemplate(string invoiceNumber, DateTime invoiceDate, Mst_Customer customer,
        List<dynamic> productDetails, decimal subtotal, decimal vat, decimal total, string remark, Guid sellOrderId)
    {
        string currentDate = invoiceDate.ToString("dd/MM/yyyy");
        string customerName = customer?.Customer_Name ?? "[Customer Name]";
        string customerAddress = customer?.Address ?? "[Customer Address]";
        string customerPhone = customer?.Phone_No ?? "(xxx) xxx-xxxx";

        string productRows = "";
        int itemNumber = 1;
        foreach (var product in productDetails)
        {
            productRows += $@"
            <tr>
                <td>{itemNumber}</td>
                <td>{product.ProductName}</td>
                <td class='number-right'>{product.Quantity}</td>
                <td class='number-right'>{product.UnitPrice:F2}</td>
                <td class='number-right'>{product.LineTotal:F2}</td>
            </tr>";
            itemNumber += 1;
        }

        for (int i = productDetails.Count; i < 10; i++)
        {
            productRows += "<tr><td>&nbsp;</td><td>&nbsp;</td><td>&nbsp;</td><td>&nbsp;</td><td>&nbsp;</td></tr>";
        }

        string logoBase64 = GetLogoBase64();
        string logoSection = "";
        if (!string.IsNullOrEmpty(logoBase64))
        {
            logoSection = $"<img src=\"data:image/png;base64,{logoBase64}\" alt=\"Company Logo\" style=\"max-width: 200px; max-height: 80px; object-fit: contain;\" />";
        }

        return @"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <title>Invoice</title>
    <style>
        body { font-family: Arial, sans-serif; font-size: 12px; margin: 20px; padding: 40px 20px 20px 20px; color: #333; }
        .header { display: flex; justify-content: space-between; align-items: flex-start; margin-bottom: 20px; }
        .company-info { flex: 2; display: flex; align-items: flex-start; gap: 20px; }
        .company-text { flex: 1; }
        .company-logo { flex-shrink: 0; }
        .invoice-details { text-align: right; flex: 1; }
        .customer-content { padding: 10px 0; min-height: 100px; }
        .customer-label { margin-top: 10px; }
        .customer-label .label-text { font-weight: bold; }
        .items-table { width: 100%; border-collapse: collapse; margin-bottom: 20px; }
        .items-table th { background-color: #DC143C; color: white; padding: 10px; text-align: left; border: 1px solid #ccc; }
        .items-table td { padding: 8px; border: 1px solid #ccc; text-align: left; }
        .number-right { text-align: right; }
        .total-section { display: flex; justify-content: flex-end; margin-top: 20px; }
        .totals { width: 200px; }
        .totals table { width: 100%; border-collapse: collapse; }
        .totals td { padding: 5px; border: 1px solid #ccc; text-align: right; }
        .totals .total-row { background-color: #FFB6C1; font-weight: bold; }
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
        <div class='invoice-details'>
            <strong style='color: #DC143C; font-size: 14px;'>INVOICE</strong><br>
            Invoice Number: " + invoiceNumber + @"<br>
            DATE: " + currentDate + @"
        </div>
    </div>
    <div class='customer-content'>
        <div class='customer-label'><span class='label-text'>ชื่อลูกค้า / Customer:</span> " + customerName + @"</div>
        <div class='customer-label'><span class='label-text'>ที่อยู่ / Address:</span> " + customerAddress + @"</div>
        <div class='customer-label'><span class='label-text'>Phone:</span> " + customerPhone + @"</div>
    </div>
    <table class='items-table'>
        <thead>
            <tr><th>ITEM</th><th>PRODUCT NAME</th><th>QTY</th><th>UNIT PRICE</th><th>TOTAL</th></tr>
        </thead>
        <tbody>" + productRows + @"</tbody>
    </table>
    <div class='total-section'>
        <div class='totals'>
            <table>
                <tr><td>SUBTOTAL</td><td class='number-right'>" + subtotal.ToString("F2") + @"</td></tr>
                <tr><td>TAX</td><td class='number-right'>" + vat.ToString("F2") + @"</td></tr>
                <tr class='total-row'><td>TOTAL</td><td class='number-right'>฿ " + total.ToString("F2") + @"</td></tr>
            </table>
        </div>
    </div>
    <div style='margin-top: 30px; text-align: center; font-size: 11px; color: #666;'>
        Thank you for your business<br>
    </div>
</body>
</html>";
    }

    // Template สำหรับ Sell Order PDF
    private string GetSellOrderTemplate(string sellNumber, DateTime sellDate, Mst_Customer customer,
        List<dynamic> productDetails, decimal subtotal, decimal vat, decimal total, string remark)
    {
        string currentDate = sellDate.ToString("dd/MM/yyyy");
        string customerName = customer?.Customer_Name ?? "[Customer Name]";
        string customerAddress = customer?.Address ?? "[Customer Address]";
        string customerPhone = customer?.Phone_No ?? "(xxx) xxx-xxxx";

        string productRows = "";
        int itemNumber = 1;
        foreach (var product in productDetails)
        {
            productRows += $@"
            <tr>
                <td>{itemNumber}</td>
                <td>{product.ProductName}</td>
                <td class='number-right'>{product.Quantity}</td>
                <td class='number-right'>{product.UnitPrice:F2}</td>
                <td class='number-right'>{product.LineTotal:F2}</td>
            </tr>";
            itemNumber += 1;
        }

        for (int i = productDetails.Count; i < 10; i++)
        {
            productRows += "<tr><td>&nbsp;</td><td>&nbsp;</td><td>&nbsp;</td><td>&nbsp;</td><td>&nbsp;</td></tr>";
        }

        string logoBase64 = GetLogoBase64();
        string logoSection = "";
        if (!string.IsNullOrEmpty(logoBase64))
        {
            logoSection = $"<img src=\"data:image/png;base64,{logoBase64}\" alt=\"Company Logo\" style=\"max-width: 200px; max-height: 80px; object-fit: contain;\" />";
        }

        return @"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <title>Sales Order</title>
    <style>
        body { font-family: Arial, sans-serif; font-size: 12px; margin: 20px; padding: 40px 20px 20px 20px; color: #333; }
        .header { display: flex; justify-content: space-between; align-items: flex-start; margin-bottom: 20px; }
        .company-info { flex: 2; display: flex; align-items: flex-start; gap: 20px; }
        .company-text { flex: 1; }
        .company-logo { flex-shrink: 0; }
        .so-details { text-align: right; flex: 1; }
        .customer-content { padding: 10px 0; min-height: 100px; }
        .customer-label { margin-top: 10px; }
        .customer-label .label-text { font-weight: bold; }
        .items-table { width: 100%; border-collapse: collapse; margin-bottom: 20px; }
        .items-table th { background-color: #4A6FA5; color: white; padding: 10px; text-align: left; border: 1px solid #ccc; }
        .items-table td { padding: 8px; border: 1px solid #ccc; text-align: left; }
        .number-right { text-align: right; }
        .total-section { display: flex; gap: 20px; margin-top: 20px; }
        .comments { flex: 1; border: 1px solid #ccc; padding: 10px; min-height: 100px; background-color: #f0f0f0; }
        .comments-header { background-color: #888; color: white; padding: 5px; margin: -10px -10px 10px -10px; font-weight: bold; }
        .totals { width: 200px; }
        .totals table { width: 100%; border-collapse: collapse; }
        .totals td { padding: 5px; border: 1px solid #ccc; text-align: right; }
        .totals .total-row { background-color: #E6E6FA; font-weight: bold; }
        .footer { text-align: center; margin-top: 30px; font-size: 11px; color: #666; }
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
        <div class='so-details'>
            <strong style='color: #4A6FA5; font-size: 14px;'>SALES ORDER</strong><br>
            SO Number: " + sellNumber + @"<br>
            DATE: " + currentDate + @"
        </div>
    </div>
    <div class='customer-content'>
        <div class='customer-label'><span class='label-text'>ชื่อลูกค้า / Customer:</span> " + customerName + @"</div>
        <div class='customer-label'><span class='label-text'>ที่อยู่ / Address:</span> " + customerAddress + @"</div>
        <div class='customer-label'><span class='label-text'>Phone:</span> " + customerPhone + @"</div>
    </div>
    <table class='items-table'>
        <thead>
            <tr><th>ITEM</th><th>PRODUCT NAME</th><th>QTY</th><th>UNIT PRICE</th><th>TOTAL</th></tr>
        </thead>
        <tbody>" + productRows + @"</tbody>
    </table>
    <div class='total-section'>
        <div class='comments'>
            <div class='comments-header'>Comments or Special Instructions</div>
            <div style='min-height: 80px;'>" + (string.IsNullOrEmpty(remark) ? "" : remark) + @"</div>
        </div>
        <div class='totals'>
            <table>
                <tr><td>SUBTOTAL</td><td class='number-right'>" + subtotal.ToString("F2") + @"</td></tr>
                <tr><td>TAX</td><td class='number-right'>" + vat.ToString("F2") + @"</td></tr>
                <tr><td>SHIPPING</td><td class='number-right'>-</td></tr>
                <tr><td>OTHER</td><td class='number-right'>-</td></tr>
                <tr class='total-row'><td>TOTAL</td><td class='number-right'>฿ " + total.ToString("F2") + @"</td></tr>
            </table>
        </div>
    </div>
    <div class='footer'>
        If you have any questions about this sales order, please contact<br>
        KS Company Limited, 099-498-4422, kscompany@gmail.com
    </div>
</body>
</html>";
    }

}