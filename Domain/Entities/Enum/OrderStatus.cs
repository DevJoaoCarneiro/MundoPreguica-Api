using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Domain.Entities.Enum
{
    public enum OrderStatus
    {
        [Display(Name = "Pendente")]
        Pending = 1,
        [Display(Name = "Entregue")]
        Delivered = 2,
        [Display(Name = "Finalizado")]
        Finish = 3,
        [Display(Name = "Cancelado")]
        Canceled = 4
    }
}
