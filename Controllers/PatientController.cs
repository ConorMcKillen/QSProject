using Microsoft.AspNetCore.Mvc;
using System;
using Microsoft.AspNetCore.Authorization;

using QSProject.Data.Models;
using QSProject.Data.Services;
using QSProject.Models;

namespace QSProject.Controllers
{
    [Authorize]
    public class PatientController : BaseController
    {
        private IMedicineService svc;

        // Configured via DI
        public PatientController(IMedicineService ms)
        {
            svc = ms;
        }

        // GET /patient/index
        public IActionResult Index()
        {
            var patients = svc.GetPatients();

            return View(patients);
        }

        // GET /patient/details/{id}
        public IActionResult Details(int id)
        {
            // retrieve the patient with the specified id from the service
            var patient = svc.GetPatient(id);

            if (patient == null)
            {
                Alert("Patient not found", AlertType.Warning);
                return RedirectToAction(nameof(Index));
            }

            return View(patient);
        }

        // GET /patient/create
        [Authorize(Roles = "patient, staff")]
        public IActionResult Create()
        {
            // display blank form to create a patient
            var patient = new Patient();
            return View(patient);
        }

        // POST /patient/create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "patient, staff")]
        public IActionResult Create([Bind("Name, Age, Email, PhotoUrl")] Patient p)
        {
            // check email is unique for this patient
            if (svc.IsDuplicateEmail(p.Email, p.Id))
            {
                // add manual validatione error
                ModelState.AddModelError(nameof(p.Email), "The email address is already in use.");
            }

            // validate patient
            if (ModelState.IsValid)
            {
                // pass data to service to store
                var added = svc.AddPatient(p.Name, p.Age, p.Email, p.PhotoUrl);
                Alert("Patient created successfully", AlertType.Info);

                return RedirectToAction(nameof(Index));
            }

            // redisplay the form for editing as there are validation errors
            return View(p);
        }

        // GET /patient/edit/{id}
        [Authorize(Roles = "patient")]
        public IActionResult Edit(int id)
        {
            // load the patient using the service
            var p = svc.GetPatient(id);

            if (p == null)
            {
                Alert($"No such patient {id}", AlertType.Warning);
                return RedirectToAction(nameof(Index));
            }

            // pass patient to view for editing
            return View(p);
        }

        // POST /patient/edit/{id}
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "patient")]
        public IActionResult Edit(int id, [Bind("Id, Name, Age, Email, PhotoUrl")] Patient p)
        {
            // check email is unique for this patient
            if (svc.IsDuplicateEmail(p.Email, p.Id))
            {
                // add manual validation error
                ModelState.AddModelError(nameof(p.Email), "The email address is already in use.");
            }

            // validate patient
            if (ModelState.IsValid)
            {
                // pass data to service to update
                svc.UpdatePatient(p);
                Alert("Patient details saved", AlertType.Info);

                return RedirectToAction(nameof(Index));
            }

            // redisplay the form for editing as validation errors
            return View(p);
        }

        // GET /patient/delete/{id}
        [Authorize(Roles = "patient")]
        public IActionResult Delete(int id)
        {
            // load the patient using the service
            var p = svc.GetPatient(id);

            if (p == null)
            {
                Alert("Patient not found", AlertType.Warning);
                return RedirectToAction(nameof(Index));
            }

            // pass patient to view for deletion confirmation
            return View(p);
        }

        // POST /patient/delete/{id}
        [HttpPost]
        [Authorize(Roles = "patient")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirm(int id)
        {
            // delete patient via service
            svc.DeletePatient(id);

            Alert($"Patient {id} deleted successfully", AlertType.Success);

            // redirect to the index view
            return RedirectToAction(nameof(Index));
        }

        // GET /patient/createmedicinerequest
        [Authorize(Roles = "patient")]
        public IActionResult CreateMedicineRequest(int id)
        {
            var p = svc.GetPatient(id);

            if (p == null)
            {
                Alert($"Not such patient {id}", AlertType.Warning);
                return RedirectToAction(nameof(Index));
            }

            // create the medicine request view model and populate the PatientId property
            var m = new MedicineRequestCreateViewModel
            {
                PatientId = id
            };

            return View("CreateMedicineRequest", m);
        }

        // POST /patient/createmedicinerequest
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles ="patient")]
        public IActionResult CreateMedicineRequest([Bind("PatientId, MedicineName")] MedicineRequestCreateViewModel m)
        {
            var p = svc.GetPatient(m.PatientId);

            if (p == null)
            {
                Alert($"No such patient {m.PatientId}", AlertType.Warning);
                return RedirectToAction(nameof(Index));
            }

            // create the medicine request view model and populate the PatientId property
            svc.CreateMedicineRequest(m.PatientId, m.MedicineName);
            Alert($"Medicine request submitted successfully", AlertType.Success);

            return RedirectToAction("Details", new { Id = m.PatientId });
        }

        // GET /patient/search/{request}
        public IActionResult Search(string request)
        {
            var results = svc.GetPatientsMedicineRequest(m => m.Name != null && m.Name.ToLower().Contains(request.ToLower()));

            return View("Index", results);
        }


    }
   
        
    }

