using System.Collections.Generic;
using QSProject.Data.Models;

namespace QSProject.Models
{
    public class MedicineSearchViewModel 
    {
        // result set
        public IList<Medicine> Medicines { get; set; } = new List<Medicine>();

        // search options
        public string Request { get; set; } = "";
        public MedicineRange Range { get; set; } = MedicineRange.OPEN;
    }
}
