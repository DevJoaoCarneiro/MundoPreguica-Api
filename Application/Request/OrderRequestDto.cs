using Domain.Entities.Enum;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Application.Request
{
    public class OrderRequestDto
    {
        [Required(ErrorMessage = "Tipo do pedido é obrigatório.")]
        public OrderType OrderType { get; set; }

        [Required(ErrorMessage = "Informações do cliente são obrigatórias.")]
        public ClientRequest ClientInformation { get; set; } = new ClientRequest();

        [MinLength(1, ErrorMessage = "O pedido deve conter ao menos um item.")]
        public List<ProductInformation> ProductInformation { get; set; } = new List<ProductInformation>();
    }

    public class ClientRequest
    {
        [Required(ErrorMessage = "Nome é obrigatório.")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Telefone é obrigatório.")]
        [RegularExpression(@"^\d{10,11}$", ErrorMessage = "Telefone inválido.")]
        public string Phone { get; set; } = string.Empty;
    }

    public class ProductInformation
    {
        [Required(ErrorMessage = "Produto é obrigatório.")]
        public Guid ProductId { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Quantidade deve ser maior que zero.")]
        public int Amount { get; set; }
    }

}
