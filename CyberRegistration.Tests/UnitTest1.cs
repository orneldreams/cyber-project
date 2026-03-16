using CyberRegistration;

namespace CyberRegistration.Tests;

public class SecurityAndRegistrationTests
{
    private readonly SecurityService securityService = new();
    private readonly CryptoService cryptoService = new();
    private readonly DatabaseService databaseService = new();

    [Fact]
    public void EntreeAvecInjectionSql_DoitEtreDetectee()
    {
        var username = "admin; DROP TABLE Users;--";

        var estAttaque = securityService.EstAttaque(username);

        Assert.True(estAttaque);
    }

    [Fact]
    public void MotDePasseFaible_DoitEtreRefuse()
    {
        var motDePasseFaible = "abc123";

        var estFort = securityService.MotDePasseFort(motDePasseFaible);

        Assert.False(estFort);
    }

    [Fact]
    public void MotDePasseValide_DoitEtreHacheEtInsere()
    {
        const string password = "Password123";
        var username = $"test_{Guid.NewGuid():N}";

        databaseService.CreerTable();

        var hash = cryptoService.Hasher(password);
        Assert.False(string.IsNullOrWhiteSpace(hash));
        Assert.NotEqual(password, hash);

        databaseService.InsererUser(new User
        {
            Username = username,
            PasswordHash = hash
        });

        var authentifie = databaseService.AuthentifierUser(username, hash);
        Assert.True(authentifie);
    }
}