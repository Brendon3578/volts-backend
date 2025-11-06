using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Volts.Application.DTOs.Authentication
{
    public class RegisterUserDto
    {
        [Required(ErrorMessage = "Nome é obrigatório")]
        [StringLength(256, MinimumLength = 2, ErrorMessage = "Nome muito curto")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email é obrigatório")]
        [EmailAddress(ErrorMessage = "Email inválido")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Senha é obrigatória")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Senha deve ter no mínimo 6 caracteres")]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "Confirmação de senha errada")]
        public string ConfirmPassword { get; set; } = string.Empty;

        [Required]
        public DateTime Birthdate { get; set; }

        [Required]
        public bool AcceptedTerms {  get; set; } = false;

        [Required]
        public string Gender { get; set; } = string.Empty;
    }
}
