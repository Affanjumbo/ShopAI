namespace ShopAI.DTOs.NotificationDTOs
{
    public class NotificationCreateDTO
    {
        public int UserId { get; set; }         // The user to notify
        public string Message { get; set; }     // The message to show
    }

}
