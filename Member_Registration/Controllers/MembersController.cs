using Microsoft.AspNetCore.Mvc;
using System.Linq;
using Microsoft.EntityFrameworkCore;
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

        // Action to show all active members, optionally filtered by search criteria
        public IActionResult ShowMembers(string memberName, string societyName, int? gender, int? membershipCategory, bool? isActive)
        {
            // Fetch all active members with their associated society and hobby
            var activeMembers = _context.ClubMembers
                .Include(m => m.Society) // Include Society data
                .Include(m => m.Hobby) // Include Hobby data
                .Where(m => m.IsActive == true);

            // Apply filtering based on the provided search criteria
            if (!string.IsNullOrEmpty(memberName))
            {
                activeMembers = activeMembers.Where(m => m.MemberName.Contains(memberName));
            }

            if (!string.IsNullOrEmpty(societyName))
            {
                activeMembers = activeMembers.Where(m => m.Society.SocietyName.Contains(societyName));
            }

            if (gender.HasValue)
            {
                activeMembers = activeMembers.Where(m => m.Gender == gender.Value);
            }

            if (membershipCategory.HasValue)
            {
                activeMembers = activeMembers.Where(m => m.MembershipCategory == membershipCategory.Value);
            }

            if (isActive.HasValue)
            {
                activeMembers = activeMembers.Where(m => m.IsActive == isActive.Value);
            }

            return View(activeMembers.ToList());
        }
    }
}
