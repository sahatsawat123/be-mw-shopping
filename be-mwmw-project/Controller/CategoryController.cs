using System.Runtime.ConstrainedExecution;
using Microsoft.AspNetCore.Mvc;
using Shopping.Interface;
using Shopping.Models;

namespace Shopping.Controllers;

//first things we can do add a tag

[ApiController]
[Route("[Controller]")]
public class CategoryController : ControllerBase
{
    CategoryDapperController _dapper;
    public CategoryController(IConfiguration config)
    {
        _dapper = new CategoryDapperController(config);
    }

    [HttpGet("GetCategory")]
    public IEnumerable<Mst_Category> GetCategory()
    {
        string sql = @"SELECT category_id, 
                        category_name
                        FROM public.mst_category";
        IEnumerable<Mst_Category> Categorys = _dapper.LoadData<Mst_Category>(sql);
        return Categorys;
    }

    [HttpGet("GetCategory/{category_id}")]
    public Mst_Category GetSingleCategory(String category_id)
    {
        string sql = @"SELECT category_id, 
                        category_name 
                        FROM public.mst_category
                        WHERE category_id = '" + category_id + "'";
        Mst_Category Category = _dapper.LoadDataSingle<Mst_Category>(sql);
        return Category;
    }

    [HttpPost("CreateCategory")]

    public IActionResult AddCategory(Mst_CategoryDTO Category)
    {
        string sql = "INSERT INTO public.mst_category " +
                 "(category_id, category_name) " +
                 "VALUES ('" + Category.Category_Id.ToString() + "', " +
                "'" + Category.Category_Name?.ToString() + "')";

        Console.WriteLine(sql);
        if (_dapper.ExecuteSQL(sql))
        {
            return Ok();
        }
        throw new Exception("Failed to Create Category");
    }

    [HttpPut("EditCategory")]
    public IActionResult EditCategory(Mst_Category Category)
    {
        string sql = "UPDATE public.mst_category " +
                        "SET category_name = '" + Category.Category_Name?.ToString() + "'" +
                    "WHERE category_id = '" + Category.Category_Id.ToString() + "'";
        if (_dapper.ExecuteSQL(sql))
        {
            return Ok();
        }
        throw new Exception("Failed to Update Category");
    }

    [HttpDelete("Delete/{category_id}")]

    public IActionResult DeleteCategory(String category_id)
    {
        string sql = "DELETE FROM public.mst_category " +
                        "WHERE category_id = '" + category_id.ToString() + "'";
        Console.WriteLine(sql);
        if (_dapper.ExecuteSQL(sql))
        {
            return Ok();
        }
        throw new Exception("Failed to Update Category");
    }
}