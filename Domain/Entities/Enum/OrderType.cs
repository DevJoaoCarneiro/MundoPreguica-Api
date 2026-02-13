using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Domain.Entities.Enum
{
    public enum OrderType
    {
        [Display(Name = "Venda")]
        Sale = 1,
        [Display(Name = "Consignado")]
        Consignment = 2
    }
}
