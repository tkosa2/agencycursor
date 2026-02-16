using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AgencyCursor.Models;

public class Request
{
    public int Id { get; set; }

    [Required, Range(1, int.MaxValue, ErrorMessage = "Please select a requestor.")]
    public int RequestorId { get; set; }
    [ForeignKey(nameof(RequestorId))]
    public Requestor Requestor { get; set; } = null!;

    [Display(Name = "Request Name"), StringLength(200)]
    public string? RequestName { get; set; }

    [Display(Name = "First Name"), StringLength(100)]
    public string? FirstName { get; set; }

    [Display(Name = "Last Name"), StringLength(100)]
    public string? LastName { get; set; }

    [Display(Name = "Number of Individuals")]
    public int NumberOfIndividuals { get; set; } = 1;

    [Display(Name = "Individual Type"), StringLength(50)]
    public string? IndividualType { get; set; }

    [Display(Name = "Type of Service"), StringLength(100)]
    public string? TypeOfService { get; set; }

    [Display(Name = "Type of Service (Other)"), StringLength(100)]
    public string? TypeOfServiceOther { get; set; }

    [Display(Name = "Mode of Interpretation"), StringLength(50)]
    public string? Mode { get; set; }

    [Display(Name = "Meeting Link"), StringLength(500)]
    public string? MeetingLink { get; set; }

    [Display(Name = "Address"), StringLength(200)]
    public string? Address { get; set; }

    [Display(Name = "Address 2"), StringLength(200)]
    public string? Address2 { get; set; }

    [Display(Name = "City"), StringLength(100)]
    public string? City { get; set; }

    [Display(Name = "State"), StringLength(2)]
    public string? State { get; set; }

    [Display(Name = "ZIP Code"), StringLength(10)]
    public string? ZipCode { get; set; }

    [Display(Name = "Gender Preference"), StringLength(20)]
    public string? GenderPreference { get; set; }

    [Display(Name = "Preferred Interpreter")]
    public int? PreferredInterpreterId { get; set; }
    [ForeignKey(nameof(PreferredInterpreterId))]
    public Interpreter? PreferredInterpreter { get; set; }

    [Display(Name = "Preferred Interpreter Name"), StringLength(200)]
    public string? PreferredInterpreterName { get; set; }

    [Display(Name = "Client Names"), StringLength(1000)]
    public string? ClientNames { get; set; }

    [Display(Name = "Specializations"), StringLength(500)]
    public string? Specializations { get; set; }

    [Display(Name = "Date and Time of Service")]
    public DateTime ServiceDateTime { get; set; }

    [Display(Name = "End Time")]
    public DateTime? EndDateTime { get; set; }

    [StringLength(50)]
    public string? Location { get; set; }

    [Display(Name = "Additional Notes"), StringLength(2000)]
    public string? AdditionalNotes { get; set; }

    [StringLength(50)]
    public string Status { get; set; } = "Pending";

    public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
}
