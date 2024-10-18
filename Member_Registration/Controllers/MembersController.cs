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
            var query = _context.ClubMembers
                                .Include(cm => cm.Society)
                                .Include(cm => cm.ClubMemberHobbies)
                                .ThenInclude(cmh => cmh.Hobby)
                                .AsQueryable();

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

            var members = query.ToList();

            return View(members);
        }

        public IActionResult AddMember()
        {
            var viewModel = new AddMemberViewModel
            {
                Societies = _context.Societies.Where(s => (bool)s.IsActive).ToList(),
                Hobbies = _context.Hobbies.Where(h => (bool)h.IsActive).ToList()
            };
            return View(viewModel);
        }

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

                if (model.SelectedHobbies != null && model.SelectedHobbies.Count > 0)
                {
                    foreach (var hobbyId in model.SelectedHobbies)
                    {
                        var clubMemberHobby = new ClubMemberHobby
                        {
                            Id = Guid.NewGuid(),
                            ClubMemberId = newMember.Id,
                            HobbyId = Guid.Parse(hobbyId)
                        };
                        _context.ClubMemberHobbies.Add(clubMemberHobby);
                    }
                    _context.SaveChanges();
                }

                return RedirectToAction(nameof(ShowMembers));
            }

            model.Societies = _context.Societies.Where(s => (bool)s.IsActive).ToList();
            model.Hobbies = _context.Hobbies.Where(h => (bool)h.IsActive).ToList();

            return View(model);
        }

        public IActionResult EditMember(Guid id)
        {
            
            var member = _context.ClubMembers
                .Include(cm => cm.Society)
                .Include(cm => cm.ClubMemberHobbies)
                    .ThenInclude(cmh => cmh.Hobby)
                .FirstOrDefault(cm => cm.Id == id);

            if (member == null)
            {
                return NotFound();
            }

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
                    var memberToUpdate = await _context.ClubMembers
                        .Include(cm => cm.ClubMemberHobbies)
                        .FirstOrDefaultAsync(cm => cm.Id == id);

                    if (memberToUpdate == null)
                    {
                        return NotFound();
                    }

                    memberToUpdate.MemberName = model.MemberName;
                    memberToUpdate.Gender = model.Gender;
                    memberToUpdate.MembershipCategory = model.MembershipCategory;
                    memberToUpdate.SocietyId = model.SocietyId;
                    memberToUpdate.IsActive = model.IsActive;
                    memberToUpdate.Remark = model.Remark;

                    _context.Update(memberToUpdate);
                    await _context.SaveChangesAsync();

                    var existingHobbies = await _context.ClubMemberHobbies
                        .Where(ch => ch.ClubMemberId == id)
                        .ToListAsync();

                    _context.ClubMemberHobbies.RemoveRange(existingHobbies);

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
