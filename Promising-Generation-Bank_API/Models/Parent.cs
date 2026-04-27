namespace Promising_Generation_Bank_API.Models
{
    public class Parent
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;

        // Navigation Property
        public ICollection<Child> Children { get; set; } = new List<Child>();
    }
}
