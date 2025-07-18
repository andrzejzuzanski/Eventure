using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace Eventure.ViewModels
{
    public class EventCreateViewModel
    {
        [Required]
        [StringLength(100)]
        public string Title { get; set; }

        [DataType(DataType.MultilineText)]
        public string? Description { get; set; }

        [Required]
        public DateTime StartDateTime { get; set; } = DateTime.Now;

        [Required]
        public DateTime EndDateTime { get; set; } = DateTime.Now.AddHours(1);

        [StringLength(200)]
        public string? Location { get; set; }

        [Range(1, 10000)]
        public int? MaxParticipants { get; set; }
        public int CategoryId { get; set; }
        [BindNever]
        public List<SelectListItem> Categories { get; set; }
    }
}
