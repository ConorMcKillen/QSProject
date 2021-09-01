using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Security.Claims;

namespace QSProject.Controllers
{
    public enum AlertType { Success, Danger, Warning, Info }

    public class BaseController : Controller
    {
        // set alert message
        public void Alert(string message, AlertType type = AlertType.Info)
        {
            TempData["Alert.Message"] = message;
            TempData["Alert.Type"] = type.ToString();
        }

        // return user identity ID if authenticated otherwise null
        public int GetSignedInUserId()
        {
            try
            {
                if(User.Identity.IsAuthenticated)
                {
                    // id stored as a string in the Sid claim - convert to an int and return
                    string sid = User.Claims.SingleOrDefault(c => c.Type == ClaimTypes.Sid).Value;
                    return int.Parse(sid);
                }
            }
            catch (FormatException) { }
            return 0;
        }

        // check if user is currently authenticated
        public bool IsAuthenticated()
        {
            return User.Identity.IsAuthenticated;
        }
    }
}
