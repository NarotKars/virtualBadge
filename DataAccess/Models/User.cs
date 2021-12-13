namespace DataAccess.Models
{
    public class User
    {
        public string Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public int AlreadyRequested { get; set; }
        public int BadgeQuantity { get; set; }
    }
}
