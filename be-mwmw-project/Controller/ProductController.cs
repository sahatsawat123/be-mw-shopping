using Microsoft.AspNetCore.Mvc;
using Shopping.Interface;
using Shopping.Models;

namespace Shopping.Controllers;

//first things we can do add a tag

[ApiController]
[Route("[Controller]")]
public class ProductController : ControllerBase
{
    IDapperController _dapper;
    public ProductController(IConfiguration config)
    {
        _dapper = new DapperController(config);
    }

    [HttpGet("GetProduct")]
    public IEnumerable<Mst_Product> GetProduct()
    {
        string sql = @"SELECT product_id, 
                        product_name,
                        product_detail,
                        price_per_unit,
                        category_id
                        FROM public.mst_product";
        IEnumerable<Mst_Product> Products = _dapper.LoadData<Mst_Product>(sql);
        return Products;
    }

    [HttpGet("GetProduct/{product_id}")]
    public Mst_Product GetSingleProduct(String product_id)
    {
        string sql = @"SELECT product_id, 
                        product_name,
                        product_detail,
                        price_per_unit,
                        category_id 
                        FROM public.mst_product
                        WHERE product_id = '" + product_id + "'";
        Mst_Product Product = _dapper.LoadDataSingle<Mst_Product>(sql);
        return Product;
    }

    [HttpPost("CreateProduct")]

    public IActionResult AddProduct(Mst_ProductDTO Product)
    {
        string sql = "INSERT INTO public.mst_product " +
                 "(product_id, product_name, product_detail, price_per_unit, category_id) " +
                 "VALUES ('" + Product.Product_Id.ToString() + "', " +
                 "'" + Product.Product_Name?.ToString() + "', " +
                 "'" + Product.Product_Detail?.ToString() + "', " +
                 "'" + Product.Price_Per_Unit.ToString() + "', " +
                 "'" + Product.Category_Id.ToString() + "')";
        Console.WriteLine(sql);
        if (_dapper.ExecuteSQL(sql))
        {
            return Ok();
        }
        throw new Exception("Failed to Create Product");
    }

    [HttpPut("EditProduct")]
    public IActionResult EditProduct(Mst_Product Product)
    {
        string sql = "UPDATE public.mst_product " +
                        "SET product_name = '" + Product.Product_Name?.ToString() + "', " +
                        "product_detail = '" + Product.Product_Detail?.ToString() + "', " +
                        "price_per_unit = '" + Product.Price_Per_Unit.ToString() + "', " +
                        "category_id = '" + Product.Category_Id.ToString() + "'" +
                    "WHERE product_id = '" + Product.Product_Id.ToString() + "'";
        if (_dapper.ExecuteSQL(sql))
        {
            return Ok();
        }
        throw new Exception("Failed to Update Product");
    }

    [HttpDelete("Delete/{product_id}")]

    public IActionResult DeleteProduct(String product_id)
    {
        string sql = "DELETE FROM public.mst_product " +
                        "WHERE product_id = '" + product_id.ToString() + "'";
        Console.WriteLine(sql);
        if (_dapper.ExecuteSQL(sql))
        {
            return Ok();
        }
        throw new Exception("Failed to Delete Product");
    }
}