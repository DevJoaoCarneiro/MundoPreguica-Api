using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Application.Request
{
    public class SettleConsignmentRequestDto
    {
        [Required(ErrorMessage = "O ID do pedido é obrigatório.")]
        public Guid OrderId { get; set; }

        public List<ItemSettlementDto> ItemsSettlement { get; set; } = new();
    }

    public class ItemSettlementDto
    {
        [Required(ErrorMessage = "O ID do produto é obrigatório.")]
        public Guid ProductId { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "A quantidade vendida não pode ser negativa.")]
        public int SoldAmount { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "A quantidade devolvida não pode ser negativa.")]
        public int ReturnedAmount { get; set; }
    }
}