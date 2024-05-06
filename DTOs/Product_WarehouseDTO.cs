using System.ComponentModel.DataAnnotations;

namespace APBD_zaj7.DTOs;

public record Product_WarehouseDTO(
    [Required] int IdProduct, 
    [Required] int IdWarehouse,
    [Required] [Range(1, int.MaxValue, ErrorMessage = "The Amount value should be greater than 0.")] int Amount,
    [Required] DateTime CreatedAt
    );