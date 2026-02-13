using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Domain.Entities.Enum
{
    public enum ProductStatus
    {
        [Display(Name = "Disponível")]
        Available = 1,
        [Display(Name = "Indisponível")]
        Inactive = 2
    }
}
