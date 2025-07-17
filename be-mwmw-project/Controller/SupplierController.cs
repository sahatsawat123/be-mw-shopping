using Microsoft.AspNetCore.Mvc;
using Shopping.Interface;
using Shopping.Models;

namespace Shopping.Controllers;

//first things we can do add a tag

[ApiController]
[Route("[Controller]")]
public class SupplierController : ControllerBase
{
    IDapperController _dapper;
    public SupplierController(IConfiguration config)
    {
        _dapper = new DapperController(config);
    }

    [HttpGet("GetSupplier")]
    public IEnumerable<Mst_Supplier> GetSupplier()
    {
        string sql = @"SELECT Supplier_id, 
                        Supplier_name,
                        Address,
                        Phone_No,
                        Tax_Id
                        FROM public.mst_supplier";
        IEnumerable<Mst_Supplier> Suppliers = _dapper.LoadData<Mst_Supplier>(sql);
        return Suppliers;
    }

    [HttpGet("GetSupplier/{supplier_id}")]
    public Mst_Supplier GetSingleSupplier(String supplier_id)
    {
        string sql = @"SELECT Supplier_id, 
                       Supplier_name,
                        Address,
                        Phone_No,
                        Tax_Id
                        FROM public.mst_supplier
                        WHERE Supplier_id = '" + supplier_id + "'";
        Mst_Supplier Supplier = _dapper.LoadDataSingle<Mst_Supplier>(sql);
        return Supplier;
    }

    [HttpPost("CreateSupplier")]

    public IActionResult AddSupplier(Mst_SupplierDTO Supplier)
    {
        string sql = "INSERT INTO public.mst_supplier " +
                 "(supplier_id, supplier_name, address, phone_no, tax_id) " +
                 "VALUES ('" + Supplier.Supplier_Id.ToString() + "', " +
                 "'" + Supplier.Supplier_Name?.ToString() + "', " +
                 "'" + Supplier.Address?.ToString() + "', " +
                 "'" + Supplier.Phone_No.ToString() + "', " +
                 "'" + Supplier.Tax_Id.ToString() + "')";
        Console.WriteLine(sql);
        if (_dapper.ExecuteSQL(sql))
        {
            return Ok();
        }
        throw new Exception("Failed to Create Supplier");
    }

    [HttpPut("EditSupplier")]
    public IActionResult EditSupplier(Mst_Supplier Supplier)
    {
        string sql = "UPDATE public.mst_supplier " +
                        "SET supplier_name = '" + Supplier.Supplier_Name?.ToString() + "', " +
                        "address = '" + Supplier.Address?.ToString() + "', " +
                        "phone_no = '" + Supplier.Phone_No.ToString() + "', " +
                        "tax_id = '" + Supplier.Tax_Id.ToString() + "'" +
                    "WHERE supplier_id = '" + Supplier.Supplier_Id.ToString() + "'";
        if (_dapper.ExecuteSQL(sql))
        {
            return Ok();
        }
        throw new Exception("Failed to Update Supplier");
    }

    [HttpDelete("Delete/{Supplier_id}")]

    public IActionResult DeleteSupplier(String Supplier_id)
    {
        string sql = "DELETE FROM public.mst_supplier " +
                        "WHERE supplier_id = '" + Supplier_id.ToString() + "'";
        Console.WriteLine(sql);
        if (_dapper.ExecuteSQL(sql))
        {
            return Ok();
        }
        throw new Exception("Failed to Delete Supplier");
    }
}