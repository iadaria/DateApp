namespace Acquaintance.API.Models
{
    public class Like
    {
        public int LikerId { get; set; } // UserIs
        public int LikeeId { get; set; } //user being liked by another user
        public User Liker { get; set; }
        public User Likee { get; set; }
    }
}