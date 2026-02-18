using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;


namespace Application.Request
{
    public class ProductRequestDto
    {

        [Required(ErrorMessage = "Nome é obrigatório.")]
        public string Name { get; set; } = string.Empty;

        [Range(1, int.MaxValue, ErrorMessage = "Categoria inválida.")]
        public int CategoryId { get; set; }

        [Required(ErrorMessage = "Preço é obrigatório.")]
        public string Price { get; set; } = string.Empty;

        public bool IsPromotion { get; set; }

        public string? OldPrice { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Gênero inválido.")]
        public int Gender { get; set; }
        public IFormFile? Image { get; set; }

        [MinLength(1, ErrorMessage = "Informe ao menos uma variação.")]
        public List<ProductVariantRequest> Variant { get; set; } = new();


    }

    public class ProductVariantRequest
    {
        [Range(1, int.MaxValue, ErrorMessage = "Tamanho inválido.")]
        public int Size { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "Estoque não pode ser negativo.")]
        public int Stock { get; set; }
    }
}
