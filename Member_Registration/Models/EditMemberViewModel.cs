namespace Member_Registration.Models
{
    public class EditMemberViewModel
    {
        public Guid Id { get; set; }
        public string MemberName { get; set; }
        public int Gender { get; set; }
        public int MembershipCategory { get; set; }
        public Guid SocietyId { get; set; }
        public List<Guid> SelectedHobbies { get; set; } // Ensure this is List<Guid>
        public bool IsActive { get; set; }
        public string Remark { get; set; }
        public List<Society>? Societies { get; set; }
        public List<Hobby>? Hobbies { get; set; }
    }
}
