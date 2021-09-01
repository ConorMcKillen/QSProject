using System;

namespace QSProject.Models
{
    public class MedicineViewModel
    {
        public int Id { get; set; }
        public string MedicineName { get; set; }
        public string Resolution { get; set; }
        public DateTime CreatedOn { get; set; } = DateTime.Now;
        public DateTime ResolvedOn { get; set; } = DateTime.MinValue;
        public bool Active { get; set; }
        public int PatientId { get; set; }
        public string PatientName { get; set; }
    }
}
