using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Xml.Linq;

namespace Domain.Entities.Enum
{
    public enum ProductSize
    {
        [Display(Name = "Infantil P")]
        Infantil2 = 1,
        [Display(Name = "Infantil M")]
        Infantil4 = 2,
        [Display(Name = "Infantil G")]
        Infantil6 = 3,
        [Display(Name = "Infantil GG")]
        Infantil8 = 4,

        [Display(Name = "Adulto P")]
        AdultoP = 10,
        [Display(Name = "Adulto M")]
        AdultoM = 11,
        [Display(Name = "Adulto G")]
        AdultoG = 12,
        [Display(Name = "Adulto GG")]
        AdultoGG = 13,
    }
}
