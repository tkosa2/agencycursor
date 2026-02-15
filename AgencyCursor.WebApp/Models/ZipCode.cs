using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AgencyCursor.Models;

public class ZipCode
{
    public int Id { get; set; }

    [StringLength(2)]
    public string CountryCode { get; set; } = string.Empty;

    [Required]
    [StringLength(20)]
    public string PostalCode { get; set; } = string.Empty;

    [StringLength(180)]
    public string? PlaceName { get; set; }

    [StringLength(100)]
    public string? AdminName1 { get; set; }

    [StringLength(20)]
    public string? AdminCode1 { get; set; }

    [StringLength(100)]
    public string? AdminName2 { get; set; }

    [StringLength(20)]
    public string? AdminCode2 { get; set; }

    [StringLength(100)]
    public string? AdminName3 { get; set; }

    [StringLength(20)]
    public string? AdminCode3 { get; set; }

    [Column(TypeName = "REAL")]
    public double? Latitude { get; set; }

    [Column(TypeName = "REAL")]
    public double? Longitude { get; set; }

    public int? Accuracy { get; set; }
}
