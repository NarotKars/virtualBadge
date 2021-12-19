namespace DataAccess.Models
{
    public class Request
    {
        public int Id { get; set; }
        public string RequesterId { get; set; }
        public string GiverId { get; set; }
        public int Status { get; set; }
        public int Quantity { get; set; }
    }

    public class RequestCredentials
    {
        public Request Request { get; set; }
        public string RequesterUserName { get; set; }
        public string GiverUserName { get; set; }
    }
}