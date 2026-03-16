using CyberRegistration;

var securityService = new SecurityService();
var cryptoService = new CryptoService();
var databaseService = new DatabaseService();

try
{
	databaseService.CreerTable();

	var choice = LireChoixMenu();

	if (choice == "0")
	{
		Console.WriteLine("Au revoir.");
		return;
	}

	var username = LireTexteNonVide("Nom d'utilisateur : ");
	var password = LireMotDePasseNonVide();

	var champSuspecte = securityService.EstAttaque(username) ? "Nom d'utilisateur" : securityService.EstAttaque(password) ? "Mot de passe" : null;
	if (champSuspecte != null)
	{
		var inputSuspecte = champSuspecte == "Nom d'utilisateur" ? username : password;
		var typeAttaque = securityService.DetecterTypeAttaque(inputSuspecte);
		var details = $"Champ : {champSuspecte} | Type : {typeAttaque}";
		databaseService.EnregistrerTentativeAttaque(
			choice == "1" ? "Inscription" : "Connexion",
			username,
			details);
		Console.ForegroundColor = ConsoleColor.Red;
		Console.WriteLine($"\nAttaque detectee !");
		Console.WriteLine($"  Type    : {typeAttaque}");
		Console.WriteLine($"  Champ   : {champSuspecte}");
		Console.WriteLine($"  Action  : {(choice == "1" ? "Inscription" : "Connexion")}");
		Console.ResetColor();
		return;
	}

	if (choice == "1" && !securityService.MotDePasseFort(password))
	{
		Console.WriteLine("Mot de passe faible");
		return;
	}

	var passwordHash = cryptoService.Hasher(password);

	if (choice == "1")
	{
		var user = new User
		{
			Username = username,
			PasswordHash = passwordHash
		};

		databaseService.InsererUser(user);
		Console.WriteLine("Utilisateur enregistré avec succès !");
		return;
	}

	var isAuthenticated = databaseService.AuthentifierUser(username, passwordHash);

	if (!isAuthenticated)
	{
		Console.WriteLine("Nom d'utilisateur ou mot de passe incorrect.");
		return;
	}

	Console.WriteLine($"\nConnexion réussie ! Bienvenue, {username}.");

	var role = databaseService.GetUserRole(username);

	if (role.Equals("admin", StringComparison.OrdinalIgnoreCase))
	{
		Console.WriteLine("\n=== TABLEAU DE BORD ADMINISTRATEUR ===");

		Console.WriteLine("\n--- Utilisateurs inscrits ---");
		var users = databaseService.GetAllUsers();
		Console.WriteLine($"{"Id",-5} {"Username",-20} {"Role",-10} {"PasswordHash (debut)"}");
		Console.WriteLine(new string('-', 65));
		foreach (System.Data.DataRow row in users.Rows)
		{
			Console.WriteLine($"{row["Id"],-5} {row["Username"],-20} {row["PasswordHash"]}");
		}

		Console.WriteLine("\n--- Logs d'attaques (20 derniers) ---");
		var logs = databaseService.GetLogs();
		Console.WriteLine($"{"Id",-5} {"ActionType",-15} {"Username",-20} {"CreatedAt",-22} {"Details"}");
		Console.WriteLine(new string('-', 95));
		foreach (System.Data.DataRow row in logs.Rows)
		{
			Console.WriteLine($"{row["Id"],-5} {row["ActionType"],-15} {row["Username"],-20} {row["CreatedAt"],-22} {row["Details"]}");
		}
	}
	else
	{
		Console.WriteLine($"Role : utilisateur standard. Aucun acces administrateur.");
	}
}
catch (InvalidOperationException exception)
{
	Console.WriteLine(exception.Message);
}
catch (Exception exception)
{
	Console.WriteLine($"Erreur inattendue : {exception.Message}");
}

static string LireChoixMenu()
{
	while (true)
	{
		Console.WriteLine("Choisissez une action :");
		Console.WriteLine("0. Quitter");
		Console.WriteLine("1. Inscription");
		Console.WriteLine("2. Connexion");
		Console.Write("Votre choix : ");

		if (Console.IsInputRedirected)
		{
			var line = Console.ReadLine()?.Trim() ?? string.Empty;
			Console.WriteLine(line);
			if (line == "0" || line == "1" || line == "2") return line;
			Console.WriteLine("Choix invalide. Tapez 0, 1 ou 2.");
			continue;
		}

		var key = Console.ReadKey(intercept: true);
		Console.WriteLine(key.KeyChar);

		if (key.KeyChar == '0' || key.KeyChar == '1' || key.KeyChar == '2')
		{
			return key.KeyChar.ToString();
		}

		Console.WriteLine("Choix invalide. Tapez 0, 1 ou 2.");
	}
}

static string LireTexteNonVide(string prompt)
{
	while (true)
	{
		Console.Write(prompt);
		var value = Console.ReadLine()?.Trim() ?? string.Empty;

		if (!string.IsNullOrWhiteSpace(value))
		{
			return value;
		}

		Console.WriteLine("Ce champ est obligatoire.");
	}
}

static string LireMotDePasseNonVide()
{
	while (true)
	{
		Console.Write("Mot de passe : ");
		var password = LireMotDePasseMasque();

		if (!string.IsNullOrWhiteSpace(password))
		{
			return password;
		}

		Console.WriteLine("Ce champ est obligatoire.");
	}
}

static string LireMotDePasseMasque()
{
	if (Console.IsInputRedirected)
	{
		return Console.ReadLine() ?? string.Empty;
	}

	var passwordChars = new List<char>();

	while (true)
	{
		var key = Console.ReadKey(intercept: true);

		if (key.Key == ConsoleKey.Enter)
		{
			Console.WriteLine();
			break;
		}

		if (key.Key == ConsoleKey.Backspace)
		{
			if (passwordChars.Count == 0)
			{
				continue;
			}

			passwordChars.RemoveAt(passwordChars.Count - 1);
			Console.Write("\b \b");
			continue;
		}

		if (char.IsControl(key.KeyChar))
		{
			continue;
		}

		passwordChars.Add(key.KeyChar);
		Console.Write("*");
	}

	return new string(passwordChars.ToArray());
}
