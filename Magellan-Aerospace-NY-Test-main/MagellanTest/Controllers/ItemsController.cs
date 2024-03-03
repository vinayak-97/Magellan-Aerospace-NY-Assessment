using Microsoft.AspNetCore.Mvc;
using Npgsql;


namespace MagellanTest.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ItemsController : ControllerBase
    {
        private readonly string _connectionString;

        public ItemsController(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection"); 
        }

        // POST: /items
        [HttpPost]
        public async Task<IActionResult> CreateItem([FromBody] Item item)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            using (var connection = new NpgsqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                using (var command = new NpgsqlCommand(@"
                    INSERT INTO item (item_name, parent_item, cost, req_date)
                    VALUES (@item_name, @parent_item, @cost, @req_date)
                    RETURNING id;", connection))
                {
                    command.Parameters.AddWithValue("@item_name", item.ItemName);
                    command.Parameters.AddWithValue("@parent_item", item.ParentItem);
                    command.Parameters.AddWithValue("@cost", item.Cost);
                    command.Parameters.AddWithValue("@req_date", item.ReqDate);

                    var id = await command.ExecuteScalarAsync();
                    return Ok(new { id = (int)id });
                }
            }
        }

        // GET: /items/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetItem(int id)
        {
            using (var connection = new NpgsqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                using (var command = new NpgsqlCommand(@"
                    SELECT id, item_name, parent_item, cost, req_date
                    FROM item
                    WHERE id = @id;", connection))
                {
                    command.Parameters.AddWithValue("@id", id);

                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        if (reader.HasRows)
                        {
                            reader.Read();
                            return Ok(new
                            {
                                id = reader.GetInt32(0),
                                item_name = reader.GetString(1),
                                parent_item = reader.IsDBNull(2) ? null : (int?)reader.GetInt32(2),
                                cost = reader.GetInt32(3),
                                req_date = reader.GetDateTime(4)
                            });
                        }
                        else
                        {
                            return NotFound();
                        }
                    }
                }
            }
        }

        // GET: /items/total/{item_name}
        [HttpGet("total/{item_name}")]
        public async Task<IActionResult> GetTotalCost(string item_name)
        {
            using (var connection = new NpgsqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                using (var command = new NpgsqlCommand(@"
                    SELECT Get_Total_Cost(@item_name);", connection))
                {
                    command.Parameters.AddWithValue("@item_name", item_name);

                    var totalCost = await command.ExecuteScalarAsync();
                    if (totalCost is null)
                    {
                        return NotFound();
                    }
                    return Ok(new { total_cost = (int)totalCost });
                }
            }
        }
    }

    public class Item
    {
        public string ItemName { get; set; }
        public int? ParentItem { get; set; }
        public int Cost { get; set; }
        public DateTime ReqDate { get; set; }
    }
}
