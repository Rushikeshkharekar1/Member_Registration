using System;
using System.Collections.Generic;

namespace Member_Registration.Models
{
    public class AddMemberViewModel
    {
        public string MemberName { get; set; }
        public Guid SocietyId { get; set; }
        public int Gender { get; set; }
        public int MembershipCategory { get; set; }
        public string? Remark { get; set; }
        public bool? IsActive { get; set; }
        public List<Society>? Societies { get; set; }
        public List<Hobby>? Hobbies { get; set; }
        public List<string> SelectedHobbies { get; set; } = new List<string>(); // for selected hobbies
    }
}
