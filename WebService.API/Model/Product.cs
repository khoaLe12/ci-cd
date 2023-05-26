using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace WebService.Model;

public class Product
{
    [Key]
    [Column("ProductID")]
    public int ProductId { get; set; }

    [MaxLength(40)]
    public string ProductName { get; set; } = null!;

    [MaxLength(20)]
    public string? QuantityPerUnit { get; set; }

    public decimal? UnitPrice { get; set; }

    public short? UnitsInStock { get; set; }

    public short? UnitsOnOrder { get; set; }

    public short? ReorderLevel { get; set; }

    [JsonIgnore]
    public bool Discontinued { get; set; } = false;

    [JsonIgnore]
    public ICollection<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();
}
