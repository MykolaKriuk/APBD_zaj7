using System.Data;
using System.Data.SqlClient;
using APBD_zaj7.DTOs;

namespace APBD_zaj7.Services;

public interface IDbService
{
    public Task<int> ExecuteCommandAndGetProduct_WarehouseId(Product_WarehouseDTO productWarehouseRequest);
}

public class DbService(IConfiguration configuration) : IDbService
{
    private async Task<SqlConnection> GetConnection()
    {
        var connection = new SqlConnection(configuration.GetConnectionString("Default"));
        if (connection.State != ConnectionState.Open)
        {
            await connection.OpenAsync();
        }

        return connection;
    }
    
    public async Task<int> ExecuteCommandAndGetProduct_WarehouseId(Product_WarehouseDTO productWarehouseRequest)
    {
        await using var connection = await GetConnection();
        
        var command1 = new SqlCommand();
        command1.Connection = connection;
        command1.CommandText = """
                               SELECT COUNT(*)
                               FROM Product
                               WHERE IdProduct = @id
                               """;
        command1.Parameters.AddWithValue("@id", productWarehouseRequest.IdProduct);
        var result1 = (int)(await command1.ExecuteScalarAsync())!;
        if (result1 == 0)
        {
            throw new ArgumentException("The Product's ID doesn't exist.");
        }
        
        var command2 = new SqlCommand();
        command2.Connection = connection;
        command2.CommandText = """
                               SELECT COUNT(*)
                               FROM Warehouse
                               WHERE IdWarehouse = @id
                               """;
        command2.Parameters.AddWithValue("@id", productWarehouseRequest.IdWarehouse);
        var result2 = (int)(await command2.ExecuteScalarAsync())!;
        if (result2 == 0)
        {
            throw new ArgumentException("The Warehouse's ID doesn't exist.");
        }
        
        var command3 = new SqlCommand();
        command3.Connection = connection;
        command3.CommandText = """
                               SELECT IdOrder
                               FROM [Order]
                               WHERE IdProduct = @idProduct
                               AND Amount = @amount
                               AND CreatedAt < @createdAt
                               """;
        command3.Parameters.AddWithValue("@idProduct", productWarehouseRequest.IdProduct);
        command3.Parameters.AddWithValue("@amount", productWarehouseRequest.Amount);
        command3.Parameters.AddWithValue("@createdAt", productWarehouseRequest.CreatedAt);
        var reader = await command3.ExecuteReaderAsync();
        if (!reader.HasRows)
        {
            throw new ArgumentException("The Order with such product ID doesn't exist or amount is invalid for such product ID.");
        }
        await reader.ReadAsync();
        var idOrder = reader.GetInt32(0);
        await reader.CloseAsync();
        
        var command4 = new SqlCommand();
        command4.Connection = connection;
        command4.CommandText = """
                               SELECT COUNT(*)
                               FROM Product_Warehouse
                               WHERE IdOrder = @idOrder
                               """;
        command4.Parameters.AddWithValue("@idOrder", idOrder);
        var result3 = (int)(await command4.ExecuteScalarAsync())!;
        if (result3 != 0)
        {
            throw new InvalidOperationException("There is already order with such product in Product_Warehouse.");
        }
        
        var command5 = new SqlCommand();
        command5.Connection = connection;
        command5.CommandText = """
                               UPDATE [Order]
                               Set FulfilledAt = @dateTimeNow
                               WHERE IdOrder = @idOrder
                               """;
        command5.Parameters.AddWithValue("@dateTimeNow", DateTime.Now);
        command5.Parameters.AddWithValue("@idOrder", idOrder);
        await command5.ExecuteNonQueryAsync();
        
        var command6 = new SqlCommand();
        command6.Connection = connection;
        command6.CommandText = """
                               INSERT INTO Product_Warehouse VALUES (
                                                                     @idWarehouse,
                                                                     @idProduct,
                                                                     @idOrder,
                                                                     @amount,
                                                                     (SELECT Price*@amount
                                                                      FROM Product
                                                                      WHERE IdProduct = @idProduct),
                                                                     @dateTimeNow
                               );
                               SELECT CAST(scope_identity() as INT)
                               """;
        command6.Parameters.AddWithValue("@idWarehouse", productWarehouseRequest.IdWarehouse);
        command6.Parameters.AddWithValue("@idProduct",productWarehouseRequest.IdProduct);
        command6.Parameters.AddWithValue("@idOrder", idOrder);
        command6.Parameters.AddWithValue("@amount", productWarehouseRequest.Amount);
        command6.Parameters.AddWithValue("@dateTimeNow", DateTime.Now);
        var resultId = (int)(await command6.ExecuteScalarAsync())!;
        return resultId;
    }
}