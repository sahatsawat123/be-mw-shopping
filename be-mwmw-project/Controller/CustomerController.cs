using Microsoft.AspNetCore.Mvc;
using Shopping.Interface;
using Shopping.Models;

namespace Shopping.Controllers;

//first things we can do add a tag

[ApiController]
[Route("[Controller]")]
public class CustomerController : ControllerBase
{
    IDapperController _dapper;
    public CustomerController(IConfiguration config)
    {
        _dapper = new DapperController(config);
    }

    [HttpGet("GetCustomer")]
    public IEnumerable<Mst_Customer> GetCustomer()
    {
        string sql = @"SELECT customer_id, 
                        customer_name,
                        address,
                        tax_id,
                        phone_no
                        FROM public.mst_customer";
        IEnumerable<Mst_Customer> Customers = _dapper.LoadData<Mst_Customer>(sql);
        return Customers;
    }

    [HttpGet("GetCustomer/{customer_id}")]
    public Mst_Customer GetSingleCustomer(String customer_id)
    {
        string sql = @"SELECT customer_id, 
                        customer_name,
                        address,
                        tax_id,
                        phone_no 
                        FROM public.mst_customer
                        WHERE customer_id = '" + customer_id + "'";
        Mst_Customer Customer = _dapper.LoadDataSingle<Mst_Customer>(sql);
        return Customer;
    }

    [HttpPost("CreateCustomer")]

    public IActionResult AddCustomer(Mst_CustomerDTO Customer)
    {
        string sql = "INSERT INTO public.mst_customer " +
                 "(customer_id, customer_name, address, tax_id, phone_no) " +
                 "VALUES ('" + Customer.Customer_Id.ToString() + "', " +
                 "'" + Customer.Customer_Name?.ToString() + "', " +
                 "'" + Customer.Address?.ToString() + "', " +
                 "'" + Customer.Tax_Id?.ToString() + "', " +
                "'" + Customer.Phone_No?.ToString() + "')";
        Console.WriteLine(sql);
        if (_dapper.ExecuteSQL(sql))
        {
            return Ok();
        }
        throw new Exception("Failed to Create Customer");
    }

    [HttpPut("EditCustomer")]
    public IActionResult EditCustomer(Mst_Customer Customer)
    {
        string sql = "UPDATE public.mst_customer " +
                        "SET customer_name = '" + Customer.Customer_Name?.ToString() + "', " +
                        "address = '" + Customer.Address?.ToString() + "', " +
                        "tax_id = '" + Customer.Tax_Id?.ToString() + "', " +
                        "phone_no = '" + Customer.Phone_No?.ToString() + "'" +
                    "WHERE customer_id = '" + Customer.Customer_Id.ToString() + "'";
        if (_dapper.ExecuteSQL(sql))
        {
            return Ok();
        }
        throw new Exception("Failed to Update Customer");
    }

    [HttpDelete("Delete/{customer_id}")]

    public IActionResult DeleteCustomer(String customer_id)
    {
        string sql = "DELETE FROM public.mst_customer " +
                        "WHERE customer_id = '" + customer_id.ToString() + "'";
        Console.WriteLine(sql);
        if (_dapper.ExecuteSQL(sql))
        {
            return Ok();
        }
        throw new Exception("Failed to Delete Customer");
    }
}