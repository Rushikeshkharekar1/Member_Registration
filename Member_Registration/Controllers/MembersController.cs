using Microsoft.AspNetCore.Mvc;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Member_Registration.Models;

namespace Member_Registration.Controllers
{
    public class MembersController : Controller
    {
        private readonly iBlueAnts_MembersContext _context;

        public MembersController(iBlueAnts_MembersContext context)
        {
            _context = context;
        }

        public IActionResult ShowMembers(string memberName, string societyName, int? gender, int? membershipCategory, bool? isActive,string? Remark)
        {
            // Start by fetching only active members by default
            var members = _context.ClubMembers
                .Include(m => m.Society) // Include Society data
                .Include(m => m.Hobby)// Default to active members
                .AsQueryable();          // Ensure the result is IQueryable for further filtering

            // Apply filtering based on the provided search criteria
            if (!string.IsNullOrEmpty(memberName))
            {
                members = members.Where(m => m.MemberName.Contains(memberName));
            }

            if (!string.IsNullOrEmpty(societyName))
            {
                members = members.Where(m => m.Society.SocietyName.Contains(societyName));
            }

            if (gender.HasValue)
            {
                members = members.Where(m => m.Gender == gender.Value);
            }

            if (membershipCategory.HasValue)
            {
                members = members.Where(m => m.MembershipCategory == membershipCategory.Value);
            }

            // Override the default IsActive filter if the value is specified
            if (isActive.HasValue)
            {
                members = members.Where(m => m.IsActive == isActive.Value);
            }

            return View(members.ToList());
        }

        public IActionResult AddMember()
        {
            // Get all active societies for the dropdown
            var societies = _context.Societies
                .Where(s => s.IsActive == true)
                .ToList();

            // Get all active hobbies for the multi-select
            var hobbies = _context.Hobbies
                .Where(h => h.IsActive == true)
                .ToList();

            // Prepare the view model (you may create a separate ViewModel for better structure)
            ViewBag.Societies = societies;
            ViewBag.Hobbies = hobbies;

            return View();
        }

        [HttpPost]
        public IActionResult AddMember(string memberName, Guid societyId, int gender, Guid hobbyIds, int membershipCategory, bool isActive,string Remark)
        {
            // Create new ClubMember object
            var newMember = new ClubMember
            {
                Id = Guid.NewGuid(), // Generate a new ID
                MemberName = memberName,
                SocietyId = societyId,
                Gender = gender,
                HobbyId= hobbyIds,
                MembershipCategory = membershipCategory,
                IsActive = isActive,
                Remark=Remark
            };

            // Add new member to the context
            _context.ClubMembers.Add(newMember);
            _context.SaveChanges();

            return RedirectToAction("ShowMembers"); // Redirect to the member list after adding
        }

        // Edit Member Action (GET)
        public IActionResult EditMember(Guid id)
        {
            // Fetch the member details
            var member = _context.ClubMembers
                .Include(m => m.Society)
                .Include(m => m.Hobby)
                .FirstOrDefault(m => m.Id == id);

            if (member == null)
            {
                return NotFound();
            }

            // Prepare the dropdowns and pass member data to the view
            ViewBag.Societies = _context.Societies.Where(s => s.IsActive == true).ToList();
            ViewBag.Hobbies = _context.Hobbies.Where(h => h.IsActive == true).ToList();

            return View(member);
        }

        // Edit Member Action (POST)
        [HttpPost]
        public IActionResult EditMember(Guid id, string memberName, Guid societyId, int gender, Guid hobbyId, int membershipCategory, bool isActive)
        {
            var member = _context.ClubMembers.FirstOrDefault(m => m.Id == id);

            if (member == null)
            {
                return NotFound();
            }

            // Update member details
            member.MemberName = memberName;
            member.SocietyId = societyId;
            member.Gender = gender;
            member.HobbyId = hobbyId;
            member.MembershipCategory = membershipCategory;
            member.IsActive = isActive;

            _context.SaveChanges();

            return RedirectToAction("ShowMembers");
        }

    }
}
