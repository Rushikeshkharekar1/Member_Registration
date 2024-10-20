using Microsoft.AspNetCore.Mvc;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Member_Registration.Models;
using OfficeOpenXml;
using iText.Kernel.Pdf;
using iText.Layout.Element;
using iText.Layout;
using iText.Layout.Properties;
using Microsoft.AspNetCore.Authorization;

namespace Member_Registration.Controllers
{
    [Authorize]
    public class MembersController : Controller
    {
        
        private readonly iBlueAnts_MembersContext _context;
        
        public MembersController(iBlueAnts_MembersContext context)
        {
            _context = context;
        }
        public IActionResult DownloadExcel()
        {
            // Set the EPPlus license context
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            var members = _context.ClubMembers
                .Select(member => new
                {
                    member.MemberName,
                    SocietyName = member.Society.SocietyName,
                    Hobbies = member.ClubMemberHobbies.Select(h => h.Hobby.HobbyName).ToList(),
                    Gender = member.Gender == 0 ? "Male" : member.Gender == 1 ? "Female" : "Other",
                    member.Remark,
                    IsActive = member.IsActive.HasValue && member.IsActive.Value ? "Yes" : "No"
                })
                .ToList();

            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("Club Members");

                worksheet.Cells[1, 1].Value = "Member Name";
                worksheet.Cells[1, 2].Value = "Society";
                worksheet.Cells[1, 3].Value = "Hobbies";
                worksheet.Cells[1, 4].Value = "Gender";
                worksheet.Cells[1, 5].Value = "Remarks";
                worksheet.Cells[1, 6].Value = "Is Active";

                for (int i = 0; i < members.Count; i++)
                {
                    worksheet.Cells[i + 2, 1].Value = members[i].MemberName;
                    worksheet.Cells[i + 2, 2].Value = members[i].SocietyName;
                    worksheet.Cells[i + 2, 3].Value = string.Join(", ", members[i].Hobbies);
                    worksheet.Cells[i + 2, 4].Value = members[i].Gender;
                    worksheet.Cells[i + 2, 5].Value = members[i].Remark;
                    worksheet.Cells[i + 2, 6].Value = members[i].IsActive;
                }

                var stream = new MemoryStream();
                package.SaveAs(stream);
                stream.Position = 0;

                string excelName = $"ClubMembers-{DateTime.Now.ToString("yyyyMMddHHmmssfff")}.xlsx";
                return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", excelName);
            }
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

        public IActionResult ViewMember(Guid id)
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

            return View(member);
        }

        public IActionResult DownloadPdf(Guid id)
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

            using (var memoryStream = new MemoryStream())
            {
                var pdfWriter = new PdfWriter(memoryStream);
                var pdfDocument = new PdfDocument(pdfWriter);
                var document = new Document(pdfDocument);

                // Add a title
                var title = new Paragraph("Member Details")
                    .SetTextAlignment(TextAlignment.CENTER)
                    .SetFontSize(24)
                    .SetBold();

                document.Add(title);

                // Add a line break
                document.Add(new Paragraph("\n"));

                // Member Name - Bold the label programmatically
                document.Add(new Paragraph("Member Name: ").SetBold().Add(member.MemberName).SetFontSize(14));

                // Society
                document.Add(new Paragraph("Society: ").SetBold().Add(member.Society?.SocietyName ?? "N/A").SetFontSize(14));

                // Hobbies
                document.Add(new Paragraph("Hobbies: ").SetBold());
                if (member.ClubMemberHobbies != null && member.ClubMemberHobbies.Any())
                {
                    foreach (var hobby in member.ClubMemberHobbies)
                    {
                        document.Add(new Paragraph($"- {hobby.Hobby.HobbyName}").SetFontSize(12));
                    }
                }
                else
                {
                    document.Add(new Paragraph("No Hobbies").SetFontSize(12));
                }

                // Gender
                document.Add(new Paragraph("Gender: ").SetBold()
                    .Add(member.Gender == 0 ? "Male" : member.Gender == 1 ? "Female" : "Other").SetFontSize(14));

                // Remarks
                document.Add(new Paragraph("Remarks: ").SetBold().Add(member.Remark ?? "N/A").SetFontSize(14));

                // Is Active
                document.Add(new Paragraph("Is Active: ").SetBold()
                    .Add(member.IsActive.HasValue ? (member.IsActive.Value ? "Yes" : "No") : "N/A").SetFontSize(14));

                // Add a footer or additional info if needed
                document.Add(new Paragraph("\nThank you for using our member registration system!")
                    .SetTextAlignment(TextAlignment.CENTER)
                    .SetFontSize(12));

                document.Close();

                var pdfName = $"Member-{member.MemberName}-{DateTime.Now:yyyyMMddHHmmss}.pdf";
                return File(memoryStream.ToArray(), "application/pdf", pdfName);
            }
        }


        public IActionResult GenderDistribution()
        {
            var genderCounts = new
            {
                Male = _context.ClubMembers.Count(m => m.Gender == 0),
                Female = _context.ClubMembers.Count(m => m.Gender == 1),
                Other = _context.ClubMembers.Count(m => m.Gender == 2)
            };

            ViewBag.GenderCounts = genderCounts;

            return View();
        }

        [HttpPost]
        public IActionResult UpdateIsActive(Guid id, bool isActive)
        {
            var member = _context.ClubMembers.Find(id);
            if (member != null)
            {
                member.IsActive = isActive;
                _context.SaveChanges();
                return Json(new { success = true });
            }
            return Json(new { success = false });
        }

    }
}
