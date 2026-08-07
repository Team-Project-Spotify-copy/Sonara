namespace Application.Interfaces;

public interface IRecaptchaServices
{
    Task<bool> VerifyTokenAsync(string token, string action);
}
