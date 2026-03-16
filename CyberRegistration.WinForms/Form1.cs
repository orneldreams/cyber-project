using CyberRegistration;

namespace CyberRegistration.WinForms;

public partial class Form1 : Form
{
    private readonly SecurityService securityService = new();
    private readonly CryptoService cryptoService = new();
    private readonly DatabaseService databaseService = new();

    private readonly Label lblTitle = new();
    private readonly Label lblUsername = new();
    private readonly Label lblPassword = new();
    private readonly TextBox txtUsername = new();
    private readonly TextBox txtPassword = new();
    private readonly Button btnRegister = new();
    private readonly Button btnLogin = new();
    private readonly Label lblStatus = new();

    public Form1()
    {
        InitializeComponent();
        InitializeUi();
        EnsureDatabase();
    }

    private void InitializeUi()
    {
        Text = "Cyber Registration - WinForms";
        StartPosition = FormStartPosition.CenterScreen;
        ClientSize = new Size(520, 320);
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;

        lblTitle.Text = "Systeme d'inscription securise";
        lblTitle.Font = new Font("Segoe UI", 16, FontStyle.Bold);
        lblTitle.AutoSize = true;
        lblTitle.Location = new Point(95, 20);

        lblUsername.Text = "Nom d'utilisateur";
        lblUsername.AutoSize = true;
        lblUsername.Location = new Point(55, 90);

        txtUsername.Location = new Point(55, 110);
        txtUsername.Size = new Size(400, 27);

        lblPassword.Text = "Mot de passe";
        lblPassword.AutoSize = true;
        lblPassword.Location = new Point(55, 150);

        txtPassword.Location = new Point(55, 170);
        txtPassword.Size = new Size(400, 27);
        txtPassword.UseSystemPasswordChar = true;

        btnRegister.Text = "Inscription";
        btnRegister.Location = new Point(55, 215);
        btnRegister.Size = new Size(180, 36);
        btnRegister.Click += BtnRegister_Click;

        btnLogin.Text = "Connexion";
        btnLogin.Location = new Point(275, 215);
        btnLogin.Size = new Size(180, 36);
        btnLogin.Click += BtnLogin_Click;

        lblStatus.Text = "Pret";
        lblStatus.AutoSize = false;
        lblStatus.ForeColor = Color.DimGray;
        lblStatus.Location = new Point(55, 270);
        lblStatus.Size = new Size(400, 45);

        Controls.Add(lblTitle);
        Controls.Add(lblUsername);
        Controls.Add(txtUsername);
        Controls.Add(lblPassword);
        Controls.Add(txtPassword);
        Controls.Add(btnRegister);
        Controls.Add(btnLogin);
        Controls.Add(lblStatus);
    }

    private void EnsureDatabase()
    {
        try
        {
            databaseService.CreerTable();
            SetStatus("Base de donnees prete.", false);
        }
        catch (Exception exception)
        {
            SetStatus("Erreur d'initialisation de la base.", true);
            MessageBox.Show(
                $"Erreur base de donnees: {exception.Message}",
                "Erreur",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }
    }

    private void BtnRegister_Click(object? sender, EventArgs e)
    {
        HandleAction(isRegistration: true);
    }

    private void BtnLogin_Click(object? sender, EventArgs e)
    {
        HandleAction(isRegistration: false);
    }

    private void HandleAction(bool isRegistration)
    {
        var username = txtUsername.Text.Trim();
        var password = txtPassword.Text;

        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
        {
            SetStatus("Tous les champs sont obligatoires.", true);
            return;
        }

        var champSuspecte = securityService.EstAttaque(username) ? "Nom d'utilisateur" : securityService.EstAttaque(password) ? "Mot de passe" : null;
        if (champSuspecte != null)
        {
            var inputSuspecte = champSuspecte == "Nom d'utilisateur" ? username : password;
            var typeAttaque = securityService.DetecterTypeAttaque(inputSuspecte);
            var details = $"Champ : {champSuspecte} | Type : {typeAttaque}";
            databaseService.EnregistrerTentativeAttaque(
                isRegistration ? "Inscription" : "Connexion",
                username,
                details);

            SetStatus($"Attaque detectee ! {typeAttaque} (champ : {champSuspecte})", true);
            MessageBox.Show(
                $"Type d'attaque : {typeAttaque}\nChamp concerne : {champSuspecte}\nAction : {(isRegistration ? "Inscription" : "Connexion")}",
                "Alerte de securite",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning);
            return;
        }

        if (isRegistration && !securityService.MotDePasseFort(password))
        {
            SetStatus("Mot de passe faible: 8 caracteres, 1 majuscule, 1 chiffre minimum.", true);
            return;
        }

        var passwordHash = cryptoService.Hasher(password);

        try
        {
            if (isRegistration)
            {
                databaseService.InsererUser(new User
                {
                    Username = username,
                    PasswordHash = passwordHash
                });

                SetStatus("Utilisateur enregistre avec succes.", false);
            }
            else
            {
                var isAuthenticated = databaseService.AuthentifierUser(username, passwordHash);
                if (isAuthenticated)
                {
                    var role = databaseService.GetUserRole(username);
                    var dashboard = new DashboardForm(username, role);
                    dashboard.FormClosed += (s, args) => Show();
                    dashboard.Show();
                    Hide();
                }
                else
                {
                    SetStatus("Nom d'utilisateur ou mot de passe incorrect.", true);
                }
            }
        }
        catch (InvalidOperationException exception)
        {
            SetStatus(exception.Message, true);
        }
        catch (Exception exception)
        {
            SetStatus($"Erreur: {exception.Message}", true);
        }
    }

    private void SetStatus(string message, bool isError)
    {
        lblStatus.Text = message;
        lblStatus.ForeColor = isError ? Color.Firebrick : Color.ForestGreen;
    }
}
