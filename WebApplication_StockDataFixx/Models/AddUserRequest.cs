namespace WebApplication_StockDataFixx.Models
{
    public class AddUserRequest
    {
        public int LevelId { get; set; }
        public string UserId { get; set; } = null!;
        public string Username { get; set; } = null!;
        public string Password { get; set; } = null!;
        public string JobId { get; set; } = null!;
        public string PlantId { get; set; } = null!;
    }
}
