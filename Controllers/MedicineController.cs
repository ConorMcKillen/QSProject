using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Authorization;

using QSProject.Data.Models;
using QSProject.Data.Services;
using QSProject.Models;


namespace QSProject.Controllers
{
    [Authorize]
    public class MedicineController : BaseController
    {
        private readonly IMedicineService svc;

        // configured via DI
        public MedicineController(IMedicineService ms)
        {
            svc = ms;
        }

        public IActionResult Search()
        {
            return View("LitSearch");
        }

        // GET /medicine/index
        public IActionResult Index()
        {
            // retrieve all OPEN medicine requests
            var search = new MedicineSearchViewModel
            {
                Medicines = svc.SearchMedicineRequests(MedicineRange.OPEN, "")
            };

            return View(search);
        }

        // POST /medicine/index
        [HttpPost]
        public IActionResult Index(MedicineSearchViewModel search)
        {
            // perform search request and assign results to viewmodel Medicine Requests property
            search.Medicines = svc.SearchMedicineRequests(search.Range, search.Request);

            // build custom alert message if post
            var alert = $"{search.Medicines.Count} result(s) found searching '{search.Range}' medicine requests.";
            if (search.Request != null && search.Request != "")
            {
                alert += $" for '{search.Request}'";
            }

            // display custom info alert
            Alert(alert, AlertType.Info);

            return View("Index", search);
        }

        // GET /medicine/{id}
        public IActionResult Details(int id)
        {
            var medicine = svc.GetMedicineRequest(id);

            if (medicine == null)
            {
                Alert("Medicine request not found", AlertType.Warning);
                return RedirectToAction(nameof(Index));
            }

            return View(medicine);
        }

        // POST /medicine/close/{id}
        [HttpPost]
        [Authorize(Roles="staff")]
        public IActionResult Close([Bind("Id, Resolution")] Medicine m)
        {
            // close medicine request via service
            var medicine = svc.CloseMedicineRequest(m.Id, m.Resolution);

            if (medicine == null)
            {
                Alert("Medicine request not found", AlertType.Warning);
            }
            else
            {
                Alert($"Medicine request {m.Id} closed", AlertType.Info);
            }

            // redirect to index view

            return RedirectToAction(nameof(Index));
        }

        // GET /medicine/create
        [Authorize(Roles="patient")]
        public IActionResult Create()
        {
            var patients = svc.GetPatients();

            // populate view model select list property
            var mvm = new MedicineRequestCreateViewModel
            {
                Patients = new SelectList(patients, "Id", "Name")
            };

            // render blank form
            return View(mvm);
        }

        // POST /medicine/create
        [HttpPost]
        [Authorize(Roles ="patient")]
        public IActionResult Create(MedicineRequestCreateViewModel mvm)
        {
            if (ModelState.IsValid)
            {
                svc.CreateMedicineRequest(mvm.PatientId, mvm.MedicineName);

                Alert($"Medicine request created", AlertType.Info);
                return RedirectToAction(nameof(Index));
            }

            // redisplay the form for editing

            return View(mvm);
        }
        


    }
}
