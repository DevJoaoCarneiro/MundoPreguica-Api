using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Xml.Linq;

namespace Domain.Entities.Enum
{
    public enum ProductSize
    {
        [Display(Name = "Tamanho P")]
        Bebe1 = 1,
        [Display(Name = "Tamanho M")]
        Bebe2 = 2,
        [Display(Name = "Tamanho G")]
        Bebe3 = 3,
        [Display(Name = "Tamanho GG")]
        Bebe4 = 4,

        [Display(Name = "Tamanho 1")]
        Infantil1 = 6,
        [Display(Name = "Tamanho 2")]
        Infantil2 = 7,
        [Display(Name = "Tamanho 3")]
        Infantil3 = 8,
        [Display(Name = "Tamanho 4")]
        Infantil4 = 9,
        [Display(Name = "Tamanho 6")]
        Infantil6 = 10,
        [Display(Name = "Tamanho 8")]
        Infantil8 = 11,
        [Display(Name = "Tamanho 10")]
        Infantil10 = 12,
        [Display(Name = "Tamanho 12")]
        Infantil12 = 13,
        [Display(Name = "Tamanho 14")]
        Infantil14 = 14,
        [Display(Name = "Tamanho 16")]
        Infantil16 = 15,

        [Display(Name = "Adulto PP")]
        AdultoPP = 30,
        [Display(Name = "Adulto P")]
        AdultoP = 31,
        [Display(Name = "Adulto M")]
        AdultoM = 32,
        [Display(Name = "Adulto G")]
        AdultoG = 33,
        [Display(Name = "Adulto GG")]
        AdultoGG = 34,
    }
}
