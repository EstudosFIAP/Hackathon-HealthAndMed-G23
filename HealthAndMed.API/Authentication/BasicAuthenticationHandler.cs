using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;
using HealthAndMed.API.Repository.Interfaces;

namespace HealthAndMed.API.Authentication
{
    public class BasicAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
    {
        private readonly IUserRepository _userRepository;
        private readonly IDoctorRepository _doctorRepository;

        public BasicAuthenticationHandler(
            IOptionsMonitor<AuthenticationSchemeOptions> options,
            ILoggerFactory logger,
            UrlEncoder encoder,
            IUserRepository userRepository,
            IDoctorRepository doctorRepository)
            : base(options, logger, encoder)
        {
            _userRepository = userRepository;
            _doctorRepository = doctorRepository;
        }

        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            // Log do caminho da requisição
            Logger.LogInformation("Iniciando autenticação para a requisição. Request.Path: {Path}", Request.Path);

            // Permite acesso sem autenticação para endpoints de métricas e live WebSocket.
            if (Request.Path.StartsWithSegments("/metrics") ||
                Request.Path.StartsWithSegments("/api/live/ws")||
                Request.Path.StartsWithSegments("/readiness")||
                Request.Path.StartsWithSegments("/health"))
            {
                Logger.LogInformation("Ignorando autenticação para a requisição com path: {Path}", Request.Path);
                var anonymousIdentity = new ClaimsIdentity();
                var anonymousPrincipal = new ClaimsPrincipal(anonymousIdentity);
                var anonymousTicket = new AuthenticationTicket(anonymousPrincipal, Scheme.Name);
                return Task.FromResult(AuthenticateResult.Success(anonymousTicket));
            }

            // Verifica se o cabeçalho de autorização está presente
            if (!Request.Headers.ContainsKey("Authorization"))
            {
                Logger.LogWarning("Cabeçalho de autorização ausente");
                return Task.FromResult(AuthenticateResult.Fail("Cabeçalho de autorização ausente"));
            }

            try
            {
                var authHeaderValue = Request.Headers["Authorization"].FirstOrDefault();
                if (string.IsNullOrEmpty(authHeaderValue))
                {
                    Logger.LogWarning("Cabeçalho de autorização vazio");
                    return Task.FromResult(AuthenticateResult.Fail("Cabeçalho de autorização vazio"));
                }

                var authHeader = AuthenticationHeaderValue.Parse(authHeaderValue);
                if (!authHeader.Scheme.Equals("Basic", StringComparison.OrdinalIgnoreCase))
                {
                    Logger.LogWarning("Esquema de autorização inválido: {Scheme}", authHeader.Scheme);
                    return Task.FromResult(AuthenticateResult.Fail("Esquema de autorização inválido"));
                }

                if (string.IsNullOrEmpty(authHeader.Parameter))
                {
                    Logger.LogWarning("Parâmetro de autorização ausente");
                    return Task.FromResult(AuthenticateResult.Fail("Parâmetro de autorização ausente"));
                }

                var credentialBytes = Convert.FromBase64String(authHeader.Parameter);
                var credentials = Encoding.UTF8.GetString(credentialBytes).Split(':', 2);
                if (credentials.Length != 2)
                {
                    Logger.LogWarning("Formato das credenciais inválido");
                    return Task.FromResult(AuthenticateResult.Fail("Credenciais no formato inválido"));
                }

                string identifier = credentials[0];
                string password = credentials[1];

                Logger.LogInformation("Tentando autenticar o identificador: {Identifier}", identifier);

                // Caso o identificador contenha "@", assume que é um e-mail
                if (identifier.Contains("@"))
                {
                    var user = _userRepository.ListUsers().FirstOrDefault(u =>
                        u.Email.Equals(identifier, StringComparison.OrdinalIgnoreCase) &&
                        u.Senha == password);
                    if (user == null)
                    {
                        Logger.LogWarning("Falha na autenticação para e-mail: {Email}", identifier);
                        return Task.FromResult(AuthenticateResult.Fail("E-mail ou senha inválidos"));
                    }
                    var claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.Name, user.Nome),
                        new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                        new Claim(ClaimTypes.Email, user.Email),
                        new Claim(ClaimTypes.Role, user.TipoUsuario)
                    };
                    var identityClaims = new ClaimsIdentity(claims, Scheme.Name);
                    var principal = new ClaimsPrincipal(identityClaims);
                    var ticket = new AuthenticationTicket(principal, Scheme.Name);
                    Logger.LogInformation("Autenticação realizada com sucesso para e-mail: {Email}", identifier);
                    return Task.FromResult(AuthenticateResult.Success(ticket));
                }
                // Se o identificador possuir 11 dígitos numéricos, trata como CPF
                else if (IsCpf(identifier))
                {
                    var user = _userRepository.ListUsers().FirstOrDefault(u =>
                        u.Cpf.Equals(identifier) &&
                        u.Senha == password);
                    if (user == null)
                    {
                        Logger.LogWarning("Falha na autenticação para CPF: {Cpf}", identifier);
                        return Task.FromResult(AuthenticateResult.Fail("CPF ou senha inválidos"));
                    }
                    var claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.Name, user.Nome),
                        new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                        new Claim(ClaimTypes.Email, user.Email),
                        new Claim(ClaimTypes.Role, user.TipoUsuario)
                    };
                    var identityClaims = new ClaimsIdentity(claims, Scheme.Name);
                    var principal = new ClaimsPrincipal(identityClaims);
                    var ticket = new AuthenticationTicket(principal, Scheme.Name);
                    Logger.LogInformation("Autenticação realizada com sucesso para CPF: {Cpf}", identifier);
                    return Task.FromResult(AuthenticateResult.Success(ticket));
                }
                // Caso contrário, trata o identificador como sendo o número CRM de um médico
                else
                {
                    var doctor = _doctorRepository.GetAllDoctors().FirstOrDefault(d =>
                        d.NumeroCrm.Equals(identifier, StringComparison.OrdinalIgnoreCase) &&
                        d.Usuario.Senha == password);
                    if (doctor == null)
                    {
                        Logger.LogWarning("Falha na autenticação para CRM: {CRM}", identifier);
                        return Task.FromResult(AuthenticateResult.Fail("CRM ou senha inválidos"));
                    }
                    var claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.Name, doctor.Usuario.Nome ?? string.Empty),
                        new Claim(ClaimTypes.NameIdentifier, doctor.Usuario.Id.ToString()),
                        new Claim(ClaimTypes.Email, doctor.Usuario.Email ?? string.Empty),
                        new Claim(ClaimTypes.Role, doctor.Usuario.TipoUsuario ?? string.Empty)
                    };
                    var identityClaims = new ClaimsIdentity(claims, Scheme.Name);
                    var principal = new ClaimsPrincipal(identityClaims);
                    var ticket = new AuthenticationTicket(principal, Scheme.Name);
                    Logger.LogInformation("Autenticação realizada com sucesso para CRM: {CRM}", identifier);
                    return Task.FromResult(AuthenticateResult.Success(ticket));
                }
            }
            catch (FormatException fe)
            {
                Logger.LogError(fe, "Exceção de formatação durante a autenticação");
                return Task.FromResult(AuthenticateResult.Fail("Formato do cabeçalho de autorização inválido"));
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Exceção durante a autenticação");
                return Task.FromResult(AuthenticateResult.Fail($"Erro no cabeçalho de autorização: {ex.Message}"));
            }
        }

        private bool IsCpf(string identifier)
        {
            return identifier.Length == 11 && identifier.All(char.IsDigit);
        }
    }
}
