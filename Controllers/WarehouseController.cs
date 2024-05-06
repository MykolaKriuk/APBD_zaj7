using System.Data.SqlClient;
using APBD_zaj7.DTOs;
using APBD_zaj7.Services;
using Microsoft.AspNetCore.Mvc;

namespace APBD_zaj7.Controllers;

[ApiController]
[Route("api/[controller]")]
public class WarehouseController(IDbService dbService) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> GetProduct_WarehouseId(Product_WarehouseDTO productWarehouseRequest)
    {
        int result;
        try
        {
            result = await dbService.ExecuteCommandAndGetProduct_WarehouseIdAsync(productWarehouseRequest);
        }
        catch (ArgumentException e)
        {
            return NotFound(e.Message);
        }
        catch (InvalidOperationException e)
        {
            return Conflict(e.Message);
        }

        return Ok($"New Product_Warehouse record is added with ID - {result}.");
    }
}