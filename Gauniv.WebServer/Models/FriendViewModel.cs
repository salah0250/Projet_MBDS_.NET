namespace Gauniv.WebServer.Models
{
    public class FriendViewModel
    {
        public string Id { get; set; }
        public string UserName { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public bool IsOnline { get; set; }
        public int ConnectionCount { get; set; }
    }
}
