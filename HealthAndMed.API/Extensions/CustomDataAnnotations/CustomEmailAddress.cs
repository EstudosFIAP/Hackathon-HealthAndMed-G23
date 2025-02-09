using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace FIAP_TC.Contact.Api.Extensions.CustomDataAnnotations;

/// <summary>
///     Atributo de validação de endereço de e-mail
/// </summary>
public class CustomEmailAddress : ValidationAttribute
{
    public override bool IsValid(object? value)
    {
        if (value == null)
            return true;

        if (!(value is string valueAsString))
            return false;

        // only return true if there is only 1 '@' character
        // and it is neither the first nor the last character
        int index = valueAsString.IndexOf('@');

        return Regex.IsMatch(valueAsString,
            @"^[^@\s]+@[^@\s]+\.[^@\s]+$",
            RegexOptions.IgnoreCase, TimeSpan.FromMilliseconds(250));
    }
}