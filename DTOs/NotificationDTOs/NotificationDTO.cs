﻿namespace ShopAI.DTOs.NotificationDTOs
{
    public class NotificationDTO
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string Message { get; set; }
        public bool IsRead { get; set; }
        public DateTime CreatedAt { get; set; }
    }


}
