using System;
using System.Collections.Generic;

namespace Member_Registration.Models
{
    public partial class Hobby
    {
        public Hobby()
        {
            ClubMembers = new HashSet<ClubMember>();
        }

        public Guid Id { get; set; }
        public string HobbyName { get; set; } = null!;
        public bool? IsActive { get; set; }

        public virtual ICollection<ClubMember> ClubMembers { get; set; }
    }
}
