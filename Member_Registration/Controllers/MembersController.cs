using Microsoft.AspNetCore.Mvc;
using System.Linq;
using Microsoft.EntityFrameworkCore; // Make sure to include this for Include
using Member_Registration.Models;

namespace Member_Registration.Controllers
{
    public class MembersController : Controller
    {
        private readonly iBlueAnts_MembersContext _context;

        // Constructor to inject the database context
        public MembersController(iBlueAnts_MembersContext context)
        {
            _context = context;
        }

        // Action to show all active members
        public IActionResult ShowMembers()
        {
            // Fetch all active members with their associated society and hobby
            var activeMembers = _context.ClubMembers
                .Include(m => m.Society) // Include Society data
                .Include(m => m.Hobby) // Include Hobby data
                .Where(m => m.IsActive == true)
                .ToList();

            // Pass the active members to the view
            return View(activeMembers);
        }
    }
}
