using System;
using System.Collections.Generic;

namespace Member_Registration.Models
{
    public partial class User
    {
        public Guid Id { get; set; }
        public string UserName { get; set; } = null!;
        public string Password { get; set; } = null!;
    }
}
