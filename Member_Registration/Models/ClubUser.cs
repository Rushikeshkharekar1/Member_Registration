using System;
using System.Collections.Generic;

namespace Member_Registration.Models
{
    public partial class ClubUser
    {
        public Guid ClubUserId { get; set; }
        public string UserName { get; set; } = null!;
        public string Password { get; set; } = null!;
    }
}
