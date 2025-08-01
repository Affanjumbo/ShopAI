﻿using ShopAI.Models;
using System.ComponentModel.DataAnnotations;

namespace ShopAI.DTOs.OrderDTOs
{
    // DTO for updating order status
    public class OrderStatusUpdateDTO
    {
        [Required(ErrorMessage = "OrderId is Required")]
        public int OrderId { get; set; }

        [Required]
        [EnumDataType(typeof(OrderStatus), ErrorMessage = "Invalid Order Status.")]
        public OrderStatus OrderStatus { get; set; }
    }
}