namespace Gauniv.Client.Data
{
    public class Friendship
    {
        public string RequesterId { get; set; }
        public User Requester { get; set; }
        public string AddresseeId { get; set; }
        public User Addressee { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
