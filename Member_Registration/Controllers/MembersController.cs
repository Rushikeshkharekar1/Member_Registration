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

        public IActionResult ShowMembers(string memberName, string societyName, int? gender, int? membershipCategory, bool? isActive)
        {
            // Start with the base query
            var query = _context.ClubMembers
                                .Include(cm => cm.Society)  // Include related Society
                                .Include(cm => cm.ClubMemberHobbies)
                                .ThenInclude(cmh => cmh.Hobby)
                                .AsQueryable(); ;  // Include related Hobbies through ClubMemberHobbies

            // Apply filters based on input
            if (!string.IsNullOrEmpty(memberName))
            {
                query = query.Where(cm => cm.MemberName.Contains(memberName));
            }

            if (!string.IsNullOrEmpty(societyName))
            {
                query = query.Where(cm => cm.Society.SocietyName.Contains(societyName));
            }

            if (gender.HasValue)
            {
                query = query.Where(cm => cm.Gender == gender.Value);
            }

            if (membershipCategory.HasValue)
            {
                query = query.Where(cm => cm.MembershipCategory == membershipCategory.Value);
            }

            if (isActive.HasValue)
            {
                query = query.Where(cm => cm.IsActive == isActive.Value);
            }

            var members = query.ToList(); // Execute the query and get the list of members

            return View(members);
        }
        // GET: Members/Add
        public IActionResult AddMember()
        {
            var viewModel = new AddMemberViewModel
            {
                Societies = _context.Societies.Where(s => (bool)s.IsActive).ToList(),
                Hobbies = _context.Hobbies.Where(h => (bool)h.IsActive).ToList()
            };
            return View(viewModel);
        }

        // POST: Members/Add
        [HttpPost]
        public IActionResult AddMember(AddMemberViewModel model)
        {
            if (ModelState.IsValid)
            {
                var newMember = new ClubMember
                {
                    Id = Guid.NewGuid(),
                    MemberName = model.MemberName,
                    SocietyId = model.SocietyId,
                    Gender = model.Gender,
                    MembershipCategory = model.MembershipCategory,
                    Remark = model.Remark,
                    IsActive = model.IsActive
                };

                _context.ClubMembers.Add(newMember);
                _context.SaveChanges();

                // Add selected hobbies
                if (model.SelectedHobbies != null && model.SelectedHobbies.Count > 0)
                {
                    foreach (var hobbyId in model.SelectedHobbies)
                    {
                        var clubMemberHobby = new ClubMemberHobby
                        {
                            Id = Guid.NewGuid(),
                            ClubMemberId = newMember.Id,
                            HobbyId = Guid.Parse(hobbyId) // Ensure this matches your hobby ID type
                        };
                        _context.ClubMemberHobbies.Add(clubMemberHobby);
                    }
                    _context.SaveChanges();
                }

                return RedirectToAction(nameof(ShowMembers)); // Redirect after successful addition
            }

            // If model is not valid, re-fetch societies and hobbies for the view
            model.Societies = _context.Societies.Where(s => (bool)s.IsActive).ToList();
            model.Hobbies = _context.Hobbies.Where(h => (bool)h.IsActive).ToList();

            return View(model);
        }

    }
}
