using Microsoft.AspNetCore.Mvc;

namespace Member_Registration.Controllers
{
    public class MembersController : Controller
    {
        public IActionResult ShowMembers()
        {
            return View();
        }
    }
}
