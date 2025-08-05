using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace Eventure.ViewModels
{
    public class EventCreateViewModel
    {
        [Required]
        [StringLength(100)]
        [Display(Name = "Event name")]
        public string Title { get; set; }

        [DataType(DataType.MultilineText)]
        [Display(Name = "Description")]
        public string? Description { get; set; }

        [Required]
        [Display(Name = "Start date")]
        public DateTime StartDateTime { get; set; } = DateTime.Now;

        [Required]
        [Display(Name = "End date")]
        public DateTime EndDateTime { get; set; } = DateTime.Now.AddHours(1);

        [StringLength(200)]
        [Display(Name = "Location")]
        public string? Location { get; set; }

        [Range(1, 10000)]
        [Display(Name = "Number of participants")]
        public int? MaxParticipants { get; set; }
        [Display(Name = "Category")]
        public int CategoryId { get; set; }

        [Display(Name = "Event photo")]
        public IFormFile EventImage { get; set; }

        [BindNever]
        public List<SelectListItem> Categories { get; set; }

        [Display(Name = "(Latitude)")]
        public double? Latitude { get; set; }

        [Display(Name = "(Longitude)")]
        public double? Longitude { get; set; }
    }
}
