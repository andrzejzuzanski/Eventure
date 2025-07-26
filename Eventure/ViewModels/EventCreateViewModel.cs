using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace Eventure.ViewModels
{
    public class EventCreateViewModel
    {
        [Required]
        [StringLength(100)]
        [Display(Name = "Nazwa wydarzenia")]
        public string Title { get; set; }

        [DataType(DataType.MultilineText)]
        [Display(Name = "Opis")]
        public string? Description { get; set; }

        [Required]
        [Display(Name = "Początek wydarzenia")]
        public DateTime StartDateTime { get; set; } = DateTime.Now;

        [Required]
        [Display(Name = "Koniec wydarzenia")]
        public DateTime EndDateTime { get; set; } = DateTime.Now.AddHours(1);

        [StringLength(200)]
        [Display(Name = "Lokalizacja")]
        public string? Location { get; set; }

        [Range(1, 10000)]
        [Display(Name = "Liczba uczestników")]
        public int? MaxParticipants { get; set; }
        [Display(Name = "Kategoria")]
        public int CategoryId { get; set; }
        [BindNever]
        public List<SelectListItem> Categories { get; set; }

        [Display(Name = "Szerokość geograficzna (Latitude)")]
        public double? Latitude { get; set; }

        [Display(Name = "Długość geograficzna (Longitude)")]
        public double? Longitude { get; set; }
    }
}
