using System.Data;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Shopping.Interface;
using Shopping.Models;

namespace Shopping.Controllers;

//first things we can do add a tag

[ApiController]
[Route("[Controller]")]
public class PurchaseOrderController : ControllerBase 
{

    IDapperController _dapper;

    private readonly IPdfService _pdfService;

    private readonly IPdfTemplate _template;

    public PurchaseOrderController(IConfiguration config, IPdfService pdfService, IPdfTemplate template)
    {
        _dapper = new DapperController(config);
        _pdfService = pdfService;
        _template = template;

    }

    [HttpGet("SearchPurchaseOrder")]
    public IActionResult SearchPurchaseOrder(
    
    [FromQuery] string? supplierName = null,
    [FromQuery] string? purchaseOrderNumber = null,
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
                po.purchase_order_id,
                po.purchase_order_number,
                po.order_date,
                po.total_price,
                po.vat,
                po.vat_rate,
                po.remark,
                s.supplier_id,
                s.supplier_name,
                s.Address,
                s.Phone_No,
                s.Tax_Id
            FROM public.trn_purchase_order po
            LEFT JOIN public.mst_supplier s ON po.supplier_id = s.supplier_id");

        var conditions = new List<string>();
        
        // Search by Supplier Name
        if (!string.IsNullOrEmpty(supplierName))
        {
            conditions.Add($" LOWER(s.supplier_name) LIKE LOWER('%{supplierName}%')");
        }

        // Search by Purchase Order Number
        if (!string.IsNullOrEmpty(purchaseOrderNumber))
        {
            conditions.Add($" LOWER(po.purchase_order_number) LIKE LOWER('%{purchaseOrderNumber}%')");
        }

        // Search by Date Range
        if (orderDateFrom.HasValue)
        {
            conditions.Add($" po.order_date >= '{orderDateFrom.Value:yyyy-MM-dd}'");
        }

        if (orderDateTo.HasValue)
        {
            conditions.Add($" po.order_date <= '{orderDateTo.Value:yyyy-MM-dd}'");
        }
        if (conditions.Any())
        {
            queryBuilder.Append(" WHERE ");
            queryBuilder.Append(string.Join(" AND ", conditions));
            
        }
        queryBuilder.Append(" ORDER BY po.order_date DESC, po.purchase_order_number DESC");

        // รวม Conditions เข้า Query
            string whereClause = string.Join("", conditions);
        string finalQuery = queryBuilder.ToString();

        // Execute Query - ใช้ DTO ที่มีอยู่
        var purchaseOrders = _dapper.LoadData<Trn_Purchase_OrderSearch>(finalQuery);

        // ดึง Order Details สำหรับแต่ละ Purchase Order
        var searchResults = new List<PurchaseOrderCreateRequestSearch>();

        foreach (var po in purchaseOrders)
        {
            // 🛍️ ดึง Order Details - เพิ่ม space ใน SQL
            string orderDetailsQuery = @"
                SELECT 
                    pod.purchase_order_detail_id,
                    pod.purchase_order_id,
                    pod.product_id,
                    pod.amount,
                    pod.price_per_unit,
                    p.product_name,
                    p.product_detail
                FROM public.trn_purchase_order_detail pod
                LEFT JOIN public.mst_product p ON pod.product_id = p.product_id
                WHERE pod.purchase_order_id = '" + po.Purchase_Order_Id.ToString() + "' ORDER BY pod.purchase_order_detail_id";

            var orderDetails = _dapper.LoadData<PurchaseOrderDetailsSearch>(orderDetailsQuery);

            // เพิ่มข้อมูลรวม
            searchResults.Add(new PurchaseOrderCreateRequestSearch
            {
                PurchaseOrderSearch = po,
                OrderDetailsSearch = orderDetails.ToList()
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
                x.PurchaseOrderSearch.Purchase_Order_Id,
                x.PurchaseOrderSearch.Purchase_Order_Number,
                Order_Date = x.PurchaseOrderSearch.Order_Date.ToString("yyyy-MM-dd"), // ✅ Format วันที่
                x.PurchaseOrderSearch.Total_Price,
                x.PurchaseOrderSearch.Vat,
                x.PurchaseOrderSearch.Vat_Rate,
                x.PurchaseOrderSearch.Remark,
                x.PurchaseOrderSearch.Supplier_Id,
                x.PurchaseOrderSearch.Supplier_Name,
                x.PurchaseOrderSearch.Address,
                x.PurchaseOrderSearch.Phone_No,
                x.PurchaseOrderSearch.Tax_Id,
                OrderDetails = x.OrderDetailsSearch.Select(detail => new // ✅ เพิ่ม OrderDetails
                {
                    detail.Purchase_Order_Detail_Id,
                    detail.Purchase_Order_Id,
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
            Message = $"Found {totalRecords} purchase orders! 🎉",
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
                SupplierName = supplierName,
                PurchaseOrderNumber = purchaseOrderNumber,
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

    [HttpPost("CreatePurchaseOrder")]

    public async Task<IActionResult> AddPurchaseOrderAsync([FromBody] PurchaseOrderCreateRequestDTO request)
    {

        decimal calculatedTotalPrice = 0;
        List<(Guid ProductId, int Amount, decimal Price)> orderItemsWithPrice = new List<(Guid, int, decimal)>();

        // ดึงราคาและคำนวณยอดรวมก่อน
        foreach (var detail in request.OrderDetails)
        {
            string getProductPriceSql = "SELECT price_per_unit FROM public.mst_product WHERE product_id = '" + detail.Product_Id.ToString() + "'";

            var priceResult = _dapper.LoadData<decimal>(getProductPriceSql);
            if (priceResult.Any())
            {
                decimal productPrice = priceResult.First();
                decimal TotalPrice = productPrice * detail.Amount;
                calculatedTotalPrice += TotalPrice;

                orderItemsWithPrice.Add((detail.Product_Id, detail.Amount, productPrice));
                //Console.WriteLine($"Product: {detail.Product_Id}, Price: {productPrice}, Amount: {detail.Amount}, Line Total: {TotalPrice}");
            }
            else
            {
                return BadRequest(new
                {
                    Success = false,
                    Message = $"Product not found: {detail.Product_Id}!"
                });
            }
        }

        //เพิ่ม VAT
        decimal totalWithVat = calculatedTotalPrice;
        if (request.PurchaseOrder.Vat_Rate > 0)
        {
            decimal vatAmount = calculatedTotalPrice * (request.PurchaseOrder.Vat_Rate / 100);
            totalWithVat = calculatedTotalPrice + vatAmount;
            //Console.WriteLine($"Subtotal: {calculatedTotalPrice}, VAT: {vatAmount}, Total: {totalWithVat}");
        }
        decimal Vat = (calculatedTotalPrice * (request.PurchaseOrder.Vat_Rate / 100));

        var purchaseOrder = Guid.NewGuid();

        string sql = "INSERT INTO public.trn_purchase_order " +
                 "(purchase_order_id, order_date, purchase_order_number, total_price, supplier_id, vat, vat_rate, remark) " +
                 "VALUES ('" + purchaseOrder + "', " +
                 "'" + request.PurchaseOrder.Order_Date.ToString() + "', " +
                 "'" + request.PurchaseOrder.Purchase_Order_Number.ToString() + "', " +
                 "'" + totalWithVat + "', " +
                 "'" + request.PurchaseOrder.Supplier_Id.ToString() + "', " +
                 "'" + Vat + "', " +
                 "'" + request.PurchaseOrder.Vat_Rate + "', " +
                "'" + request.PurchaseOrder.Remark.ToString() + "')";

        if (_dapper.ExecuteSQL(sql))
        {
        }
        else
        {
            throw new Exception("Failed to Create PurchaseOrder");
        }

        int successfulDetails = 0;

        foreach (var detail in request.OrderDetails)
        {
            //ดึงราคาสินค้าจากตาราง Product ก่อน
            string getProductPriceSql = "SELECT price_per_unit FROM public.mst_product WHERE product_id = '" + detail.Product_Id.ToString() + "'";
            var priceResult = _dapper.LoadData<decimal>(getProductPriceSql);
            decimal productPrice = 0;
            if (priceResult.Any())
            {
                productPrice = priceResult.First();
                //Console.WriteLine($"Found product price: {productPrice} for Product ID: {detail.Product_Id}");
            }

            string insertDetailSql = @"
                    INSERT INTO public.trn_purchase_order_detail 
                    (purchase_order_detail_id, purchase_order_id, product_id, amount, price_per_unit) 
                    VALUES ('" + detail.Purchase_Order_Detail_Id.ToString() + "', " +
                    "'" + purchaseOrder + "', " +
                    "'" + detail.Product_Id.ToString() + "', " +
                    "'" + detail.Amount + "', " +
                    "'" + productPrice + "')";

            int detailResult = _dapper.ExecuteintSQL(insertDetailSql);
            if (detailResult == 0)
            {
                return BadRequest(new
                {
                    Success = false,
                    Message = $"Failed to create Purchase Order Detail for Product: {detail.Product_Id}!",
                    FailedAtDetail = successfulDetails + 1
                });
            }

            string checkInventorySql = @"SELECT product_id, amount FROM public.stk_inventory WHERE product_id = '" + detail.Product_Id.ToString() + "'";
            var existingProducts = _dapper.LoadData<Stk_inventory>(checkInventorySql);

            if (existingProducts.Any())
            {
                string updateInventorySql = @"
                        UPDATE public.stk_inventory 
                        SET amount = amount + " + detail.Amount + @" 
                        WHERE product_id = '" + detail.Product_Id.ToString() + "'";

                int updateResult = _dapper.ExecuteintSQL(updateInventorySql);
                if (updateResult == 0)
                {
                    return BadRequest(new
                    {
                        Success = false,
                        Message = $"Failed to update inventory for Product: {detail.Product_Id}!",
                        FailedAtDetail = successfulDetails + 1
                    });
                }
            }
            else
            {
                string insertInventorySql = @"
                        INSERT INTO public.stk_inventory (product_id, amount) 
                        VALUES ('" + detail.Product_Id.ToString() + "', "
                        + detail.Amount + ")";
                Console.WriteLine(insertInventorySql);
                int insertResult = _dapper.ExecuteintSQL(insertInventorySql);
                Console.WriteLine(insertResult);
                if (insertResult == 0)
                {
                    return BadRequest(new
                    {
                        Success = false,
                        Message = $"Failed to create inventory record for Product: {detail.Product_Id}!",
                        FailedAtDetail = successfulDetails + 1
                    });
                }
            }

            successfulDetails++;
        }
        // เพิ่มส่วนสร้าง PDF หลังจากบันทึกข้อมูลสำเร็จ
        try
        {
        // ดึงข้อมูล Supplier
        string getSupplierSql = "SELECT supplier_name, address FROM public.mst_supplier WHERE supplier_id = '" + request.PurchaseOrder.Supplier_Id.ToString() + "'";
        var supplierResult = _dapper.LoadData<dynamic>(getSupplierSql);
        
        var supplierInfo = supplierResult.FirstOrDefault();

        // ดึงรายละเอียดสินค้าพร้อมชื่อ
        var productDetails = new List<dynamic>();
        foreach (var detail in request.OrderDetails)
        {
            string getProductInfoSql = @"
                SELECT product_name, price_per_unit 
                FROM public.mst_product 
                WHERE product_id = '" + detail.Product_Id.ToString() + "'";
            
            var productResult = _dapper.LoadData<Mst_Product>(getProductInfoSql);
            if (productResult.Any())
            {
                var product = productResult.First();
                productDetails.Add(new
                {
                    ProductId = detail.Product_Id,
                    ProductName = product.Product_Name,
                    detail.Amount,
                    PricePerUnit = product.Price_Per_Unit,
                    TotalPrice = detail.Amount * product.Price_Per_Unit
                });
            }
        }

        // สร้าง PDF
        string purchaseOrderHtml = GetPurchaseOrderTemplate(
            request.PurchaseOrder.Purchase_Order_Number,
            request.PurchaseOrder.Order_Date,
            supplierInfo,
            productDetails,
            calculatedTotalPrice,
            Vat,
            totalWithVat,
            request.PurchaseOrder.Remark
        );
            byte[] pdfBytes = await _pdfService.CreateAsync(purchaseOrderHtml);
            string fileName = $"{request.PurchaseOrder.Purchase_Order_Number}_{DateTime.Now:yyyyMMdd}.pdf";

            return File(pdfBytes, "application/pdf", fileName);

            // Return ข้อมูลพร้อม PDF
        }
        catch (Exception ex)
        {
            // ถ้าสร้าง PDF ไม่ได้ ก็ส่งข้อมูลปกติไป (เพราะ Database บันทึกสำเร็จแล้ว)
            return Ok(new
            {
                Success = true,
                Message = "Purchase Order created successfully! (PDF generation failed)",
                PurchaseOrderId = purchaseOrder,
                PdfError = ex.Message
            });
        }
    }
    private string GetLogoBase64()
    {
    try
    {
        // วางไฟล์ logo ใน wwwroot/images/KSlogo.jpg
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
    // Method สำหรับสร้าง HTML Template
    private string GetPurchaseOrderTemplate(
        string poNumber,
        DateTime orderDate,
        dynamic supplierInfo,
        List<dynamic> productDetails,
        decimal subtotal,
        decimal vat,
        decimal total,
        string remark)
    {
        // ใช้วันที่จาก request แทน DateTime.Now
        string currentDate = orderDate.ToString("dd/MM/yyyy");

        // ข้อมูล Supplier
        string supplierName = supplierInfo?.supplier_name ?? "[Company Name]";
        string supplierAddress = supplierInfo?.address ?? "[Street Address]";
        string supplierPhone = supplierInfo?.phone ?? "(xxx) xxx-xxxx";

        // สร้างแถวสินค้า
        string productRows = "";
        int itemNumber = 1;
        foreach (var product in productDetails)
        {
            productRows += $@"
            <tr>
                <td>{itemNumber}</td>
                <td>{product.ProductName}</td>
                <td class='number-right'>{product.Amount}</td>
                <td class='number-right'>{product.PricePerUnit:F2}</td>
                <td class='number-right'>{product.TotalPrice:F2}</td>
            </tr>";
            itemNumber += 1;
        }

        // เติมแถวว่างให้ครบ 10 แถว
        for (int i = productDetails.Count; i < 10; i++)
        {
            productRows += "<tr><td>&nbsp;</td><td>&nbsp;</td><td>&nbsp;</td><td>&nbsp;</td><td>&nbsp;</td></tr>";
        }

        string logoBase64 = GetLogoBase64();
            
            // ตรวจสอบว่ามีรูปหรือไม่ และสร้าง HTML แยกกัน
            string logoSection = "";
            if (!string.IsNullOrEmpty(logoBase64))
            {
                logoSection = $"<img src=\"data:image/png;base64,{logoBase64}\" alt=\"Company Logo\" style=\"max-width: 200px; max-height: 80px; object-fit: contain;\" />";
            }
            else
            {
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
            padding: 40px 20px 20px 20px;  /* บน ขวา ล่าง ซ้าย */
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
            margin-bottom: 20px;
        }
        .vendor {
            width: 100%;
        }
        .vendor-content {
            padding: 10px 0;
            min-height: 100px;
            background-color: transparent;
        }
        .vendor-label {
            margin-top: 10px;
        }
        .vendor-label .label-text {
            font-weight: bold;
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
            <strong style='color: #4A6FA5; font-size: 14px;'>PURCHASE ORDER</strong><br>
            PO Number: " + poNumber + @"<br>
            DATE: " + currentDate + @"
        </div>
    </div>

    <div class='vendor-ship-container'>
        <div class='vendor'>
            <div class='vendor-content'>
                <div class='vendor-label'><span class='label-text'>ชื่อผู้ขาย / Supplier:</span> " + supplierName + @"</div>
                <div class='vendor-label'><span class='label-text'>ที่อยู่ / Address:</span> " + supplierAddress + @"</div>
                <div class='vendor-label'><span class='label-text'>Phone:</span> " + supplierPhone + @"
            </div>
        </div>
    </div>
    <table class='items-table'>
        <thead>
            <tr>
                <th>ITEM</th>
                <th>PRODUCT NAME</th>
                <th>QTY</th>
                <th>UNIT PRICE</th>
                <th>TOTAL</th>
            </tr>
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
                <tr>
                    <td>SUBTOTAL</td>
                    <td class='number-right'>" + subtotal.ToString("F2") + @"</td>
                </tr>
                <tr>
                    <td>TAX</td>
                    <td class='number-right'>" + vat.ToString("F2") + @"</td>
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
                    <td class='number-right'>฿ " + total.ToString("F2") + @"</td>
                </tr>
            </table>
        </div>
    </div>

    <div class='footer'>
        If you have any questions about this purchase order, please contact<br>
        KS Company Limited, 099-498-4422, kscompany@gmail.com
    </div>
</body>
</html>";
    }
}