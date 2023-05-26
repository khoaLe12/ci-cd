using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace WebService.Model;

public class OrderDetail
{
    public Order? Order { get; set; }

    [NotNull]
    [Column("OrderID")]
    public int? OrderId { get; set; }

    public Product? Product { get; set; }

    [NotNull]
    [Column("ProductID")]
    public int? ProductId { get; set; }

    public decimal UnitPrice { get; set; }

    public short Quantity { get; set; }

    public float Discount { get; set; }

}

public class NotNullAttribute : ValidationAttribute
{
    public override bool IsValid(object? value)
    {
        return value != null;
    }
}
