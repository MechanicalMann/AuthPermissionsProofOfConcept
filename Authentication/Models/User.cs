namespace Authentication.Models
{
    public class User
    {
        public string Id { get; set; }

        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Name { get; set; }
        public string Title { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        public bool Employee { get; set; }
    }
}