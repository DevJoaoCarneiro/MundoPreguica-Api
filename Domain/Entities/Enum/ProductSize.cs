using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Xml.Linq;

namespace Domain.Entities.Enum
{
    public enum ProductSize
    {
        [Display(Name = "Tamanho 2")]
        Infantil2 = 1,
        [Display(Name = "Tamanho 3")]
        Infantil3 = 2,
        [Display(Name = "Tamanho 4")]
        Infantil4 = 3,
        [Display(Name = "Tamanho 6")]
        Infantil6 = 4,
        [Display(Name = "Tamanho 8")]
        Infantil8 = 5,
        [Display(Name = "Tamanho 10")]
        Infantil10 = 6,
        [Display(Name = "Tamanho 12")]
        Infantil12 = 7,
        [Display(Name = "Tamanho 14")]
        Infantil14 = 8,
        [Display(Name = "Tamanho 16")]
        Infantil16 = 9,

        [Display(Name = "Adulto PP")]
        AdultoPP = 10,
        [Display(Name = "Adulto P")]
        AdultoP = 11,
        [Display(Name = "Adulto M")]
        AdultoM = 12,
        [Display(Name = "Adulto G")]
        AdultoG = 13,
        [Display(Name = "Adulto GG")]
        AdultoGG = 14,
    }
}
