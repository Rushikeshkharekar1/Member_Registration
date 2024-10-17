using System;
using System.Collections.Generic;

namespace Member_Registration.Models
{
    public partial class ClubMemberHobby
    {
        public Guid Id { get; set; }
        public Guid ClubMemberId { get; set; }
        public Guid HobbyId { get; set; }

        public virtual ClubMember ClubMember { get; set; } = null!;
        public virtual Hobby Hobby { get; set; } = null!;
    }
}
