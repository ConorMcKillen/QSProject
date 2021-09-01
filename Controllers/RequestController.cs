using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

using QSProject.Data.Models;
using QSProject.Data.Services;
using QSProject.Models;
using QSProject.Helpers;


namespace QSProject.Controllers
{
    [ApiController]
    [Route("api")]
    public class RequestController : ControllerBase
    {
        private IMedicineService svc;
        private readonly string secret; // jwt secret

        public RequestController(IMedicineService service, IConfiguration config)
        {
            // retrieve secret from appsettings to use when signing jwt token in login action
            secret = config.GetValue<string>("JwtConfig:Secret");
            svc = service;
        }

        // POST api/user/login
        [AllowAnonymous]
        [HttpPost("login")]
        public ActionResult<User> Login(UserLoginViewModel login)
        {
            var user = svc.Authenticate(login.Email, login.Password);

            if (user == null)
            {
                return BadRequest(new { message = "Email or Password is incorrect" });
            }

            // sign jwt token to use in secure api requests
            var userWithToken = AuthBuilder.SignJwtToken(user, secret);
            return Ok(userWithToken);
        }


        [HttpGet("medicines")]
        public ActionResult<IList<Medicine>> GetMedicineRequests()
        {
            var medicines = svc.GetAllMedicineRequests();
            return Ok(medicines);
        }

        [HttpGet("medicines/id/{id}")]
        public ActionResult<Medicine> GetMedicineRequest(int id)
        {
            var medicine = svc.GetMedicineRequest(id);
            if (medicine == null)
            {
                return NotFound();
            }

            var vm = new MedicineViewModel
            {
                Id = medicine.Id,
                MedicineName = medicine.MedicineName,
                Resolution = medicine.Resolution,
                CreatedOn = medicine.CreatedOn,
                ResolvedOn = medicine.ResolvedOn,
                Active = medicine.Active,
                PatientId = medicine.PatientId,
                PatientName = medicine.Patient.Name
            };

            return Ok(vm);
        }

        [HttpPost("medicines")]
        public ActionResult<DemoViewModel> Create(MedicineRequestCreateViewModel mvm)
        {
            if (ModelState.IsValid)
            {
                var medicine = svc.CreateMedicineRequest(mvm.PatientId, mvm.MedicineName);

                return CreatedAtAction(nameof(GetMedicineRequest), new { Id = medicine.Id }, medicine);
            }

            return BadRequest("Medicine request could not be created");
        }

        [HttpGet("medicine/search/{range}/{request?}")]
        public ActionResult<IList<MedicineViewModel>> RequestTicket(MedicineRange range = MedicineRange.OPEN, string request = "")
        {
            var medicines = svc.SearchMedicineRequests(range, request)
                .Select(m => new MedicineViewModel
                {
                    Id = m.Id,
                    MedicineName = m.MedicineName,
                    Resolution = m.Resolution,
                    CreatedOn = m.CreatedOn,
                    ResolvedOn = m.ResolvedOn,
                    Active = m.Active,
                    PatientId = m.PatientId,
                    PatientName = m.Patient.Name
                })
                .ToList();

            return Ok(medicines);
        }

      

    }
}
