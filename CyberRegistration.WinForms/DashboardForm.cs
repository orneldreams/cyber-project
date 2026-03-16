using CyberRegistration;

namespace CyberRegistration.WinForms;

public class DashboardForm : Form
{
    private readonly string _username;
    private readonly DatabaseService _databaseService = new();
    private readonly bool _isAdmin;

    private readonly Label lblWelcome = new();
    private readonly Button btnLogout = new();
    private readonly Button btnRefresh = new();
    private readonly TabControl tabControl = new();
    private readonly TabPage tabUsers = new();
    private readonly TabPage tabLogs = new();
    private readonly DataGridView gridUsers = new();
    private readonly DataGridView gridLogs = new();

    public DashboardForm(string username, string role)
    {
        _username = username;
        _isAdmin = role.Equals("admin", StringComparison.OrdinalIgnoreCase);
        InitializeUi();
        LoadData();
    }

    private void InitializeUi()
    {
        Text = "Tableau de bord - Cyber Registration";
        StartPosition = FormStartPosition.CenterScreen;
        ClientSize = new Size(760, 520);
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;

        lblWelcome.Text = _isAdmin
            ? $"Bienvenue, Administrateur {_username} !"
            : $"Bienvenue, {_username} ! Vous etes connecte.";
        lblWelcome.Font = new Font("Segoe UI", 14, FontStyle.Bold);
        lblWelcome.AutoSize = true;
        lblWelcome.ForeColor = _isAdmin ? Color.DarkRed : Color.DarkGreen;
        lblWelcome.Location = new Point(20, 20);

        btnRefresh.Text = "Actualiser";
        btnRefresh.Location = new Point(20, 460);
        btnRefresh.Size = new Size(120, 36);
        btnRefresh.Click += (s, e) => LoadData();

        btnLogout.Text = "Deconnexion";
        btnLogout.Location = new Point(620, 460);
        btnLogout.Size = new Size(120, 36);
        btnLogout.BackColor = Color.IndianRed;
        btnLogout.ForeColor = Color.White;
        btnLogout.FlatStyle = FlatStyle.Flat;
        btnLogout.Click += BtnLogout_Click;

        if (_isAdmin)
        {
            ConfigureAdminUi();
        }
        else
        {
            ConfigureUserUi();
        }

        Controls.Add(lblWelcome);
        Controls.Add(btnRefresh);
        Controls.Add(btnLogout);
    }

    private void ConfigureAdminUi()
    {
        var lblInfo = new Label
        {
            Text = "Acces administrateur : gestion des utilisateurs et des logs de securite.",
            AutoSize = true,
            ForeColor = Color.DimGray,
            Location = new Point(20, 60)
        };
        Controls.Add(lblInfo);

        tabControl.Location = new Point(20, 90);
        tabControl.Size = new Size(720, 360);

        // Onglet Utilisateurs
        tabUsers.Text = "Utilisateurs inscrits";
        gridUsers.Dock = DockStyle.Fill;
        gridUsers.ReadOnly = true;
        gridUsers.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        gridUsers.RowHeadersVisible = false;
        gridUsers.AllowUserToAddRows = false;
        gridUsers.BackgroundColor = Color.White;
        tabUsers.Controls.Add(gridUsers);

        // Onglet Logs
        tabLogs.Text = "Logs d'attaques";
        gridLogs.Dock = DockStyle.Fill;
        gridLogs.ReadOnly = true;
        gridLogs.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None;
        gridLogs.RowHeadersVisible = false;
        gridLogs.AllowUserToAddRows = false;
        gridLogs.AllowUserToResizeRows = false;
        gridLogs.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        gridLogs.MultiSelect = false;
        gridLogs.BackgroundColor = Color.White;
        gridLogs.CellDoubleClick += GridLogs_CellDoubleClick;
        tabLogs.Controls.Add(gridLogs);

        tabControl.TabPages.Add(tabUsers);
        tabControl.TabPages.Add(tabLogs);
        Controls.Add(tabControl);
    }

    private void ConfigureUserUi()
    {
        var panel = new Panel
        {
            Location = new Point(20, 80),
            Size = new Size(720, 360),
            BorderStyle = BorderStyle.FixedSingle,
            BackColor = Color.FromArgb(245, 250, 255)
        };

        var lblIcon = new Label
        {
            Text = "Compte utilisateur",
            Font = new Font("Segoe UI", 12, FontStyle.Regular),
            AutoSize = true,
            ForeColor = Color.SteelBlue,
            Location = new Point(20, 20)
        };

        var lblName = new Label
        {
            Text = $"Nom d'utilisateur : {_username}",
            Font = new Font("Segoe UI", 11),
            AutoSize = true,
            Location = new Point(20, 60)
        };

        var lblRole = new Label
        {
            Text = "Role : Utilisateur standard",
            Font = new Font("Segoe UI", 11),
            AutoSize = true,
            ForeColor = Color.DimGray,
            Location = new Point(20, 100)
        };

        panel.Controls.Add(lblIcon);
        panel.Controls.Add(lblName);
        panel.Controls.Add(lblRole);
        Controls.Add(panel);
        btnRefresh.Visible = false;
    }

    private void LoadData()
    {
        if (!_isAdmin) return;

        try
        {
            gridUsers.DataSource = _databaseService.GetAllUsers();
            gridLogs.DataSource = _databaseService.GetLogs();
            ConfigureLogsGridColumns();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Erreur de chargement : {ex.Message}", "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
    }

    private void ConfigureLogsGridColumns()
    {
        if (gridLogs.Columns.Count == 0)
        {
            return;
        }

        gridLogs.Columns["Id"].Width = 45;
        gridLogs.Columns["ActionType"].Width = 95;
        gridLogs.Columns["Username"].Width = 150;
        gridLogs.Columns["Details"].Width = 300;
        gridLogs.Columns["CreatedAt"].Width = 120;
    }

    private void GridLogs_CellDoubleClick(object? sender, DataGridViewCellEventArgs e)
    {
        if (e.RowIndex < 0)
        {
            return;
        }

        var details = gridLogs.Rows[e.RowIndex].Cells["Details"].Value?.ToString() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(details))
        {
            return;
        }

        MessageBox.Show(details, "Details de l'attaque", MessageBoxButtons.OK, MessageBoxIcon.Information);
    }

    private void BtnLogout_Click(object? sender, EventArgs e)
    {
        Close();
    }
}
