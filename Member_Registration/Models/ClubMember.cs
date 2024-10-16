using System;
using System.Collections.Generic;

namespace Member_Registration.Models
{
    public partial class ClubMember
    {
        public Guid Id { get; set; }
        public string MemberName { get; set; } = null!;
        public Guid SocietyId { get; set; }
        public int Gender { get; set; }
        public int MembershipCategory { get; set; }
        public Guid? HobbyId { get; set; }
        public string? Remark { get; set; }
        public bool IsActive { get; set; }

        public virtual Hobby Hobby { get; set; } = null!;
        public virtual Society Society { get; set; } = null!;
    }
}
