using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace QSProject.Models
{
    public class MedicineRequestCreateViewModel
    {
        // selectlist of patients 
        public SelectList Patients { get; set; }

        // Collecting PatientID and Medicine Request in Form
        [Required]
        [Display(Name = "Select Patient")]
        public int PatientId { get; set; }

        [Required]
        [StringLength(100, MinimumLength = 1)]
        public string MedicineName { get; set; }
    }
}