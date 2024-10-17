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

        // GET: Edit Member
        public IActionResult EditMember(Guid id) // Change from int to Guid
        {
            // Fetch the member details based on the provided ID
            var member = _context.ClubMembers
                .Include(cm => cm.Society) // Include related Society
                .Include(cm => cm.ClubMemberHobbies)
                    .ThenInclude(cmh => cmh.Hobby) // Include related Hobbies
                .FirstOrDefault(cm => cm.Id == id);

            if (member == null)
            {
                return NotFound();
            }

            // Prepare the view model
            var model = new EditMemberViewModel
            {
                Id = member.Id,
                MemberName = member.MemberName,
                Gender = member.Gender,
                MembershipCategory = member.MembershipCategory,
                SocietyId = member.SocietyId,
                SelectedHobbies = member.ClubMemberHobbies.Select(cmh => cmh.HobbyId).ToList(),
                IsActive = (bool)member.IsActive,
                Remark = member.Remark,
                Societies = _context.Societies.Where(s => (bool)s.IsActive).ToList(),
                Hobbies = _context.Hobbies.Where(h => (bool)h.IsActive).ToList()
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditMember(Guid id, EditMemberViewModel model, Guid[] selectedHobbies)
        {
            if (id != model.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Fetch the existing member from the database
                    var memberToUpdate = await _context.ClubMembers
                        .Include(cm => cm.ClubMemberHobbies)
                        .FirstOrDefaultAsync(cm => cm.Id == id);

                    if (memberToUpdate == null)
                    {
                        return NotFound();
                    }

                    // Update the member details from the view model
                    memberToUpdate.MemberName = model.MemberName;
                    memberToUpdate.Gender = model.Gender;
                    memberToUpdate.MembershipCategory = model.MembershipCategory;
                    memberToUpdate.SocietyId = model.SocietyId;
                    memberToUpdate.IsActive = model.IsActive;
                    memberToUpdate.Remark = model.Remark;

                    // Update the member in the database
                    _context.Update(memberToUpdate);
                    await _context.SaveChangesAsync();

                    // Update member hobbies
                    var existingHobbies = await _context.ClubMemberHobbies
                        .Where(ch => ch.ClubMemberId == id)
                        .ToListAsync();

                    // Remove old hobbies
                    _context.ClubMemberHobbies.RemoveRange(existingHobbies);

                    // Add selected hobbies
                    if (selectedHobbies != null)
                    {
                        foreach (var hobbyId in selectedHobbies)
                        {
                            var clubMemberHobby = new ClubMemberHobby
                            {
                                Id = Guid.NewGuid(),
                                ClubMemberId = memberToUpdate.Id,
                                HobbyId = hobbyId
                            };
                            _context.ClubMemberHobbies.Add(clubMemberHobby);
                        }
                    }

                    await _context.SaveChangesAsync();

                    return RedirectToAction(nameof(ShowMembers));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!MemberExists(model.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
            }

            // If the model is not valid, re-fetch societies and hobbies for the view
            model.Societies = await _context.Societies.Where(s => (bool)s.IsActive).ToListAsync();
            model.Hobbies = await _context.Hobbies.Where(h => (bool)h.IsActive).ToListAsync();

            return View(model);
        }

        private bool MemberExists(Guid id)
        {
            return _context.ClubMembers.Any(e => e.Id == id);
        }

    }
}
