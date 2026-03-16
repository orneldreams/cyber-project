# CyberRegistration — Système de détection d'attaques SQL Injection

Projet de cybersécurité en C# .NET 8 démontrant la détection et la journalisation d'attaques par injection SQL (SQL Injection) sur un système d'inscription/connexion.

---

## Fonctionnalités

- **Inscription / Connexion** sécurisée avec hachage des mots de passe (SHA-256)
- **Détection SQL Injection** en temps réel sur les champs de saisie
- **Classification du type d'attaque** (DROP, DELETE, SELECT, INSERT, commentaire SQL `--`, terminaison `;`)
- **Journalisation** de toutes les tentatives d'attaque en base de données
- **Contrôle d'accès basé sur les rôles** (admin / user) stocké en base
- **Interface console** avec affichage coloré des alertes
- **Interface graphique WinForms** avec tableau de bord admin (liste des utilisateurs + logs)

---

## Architecture

```
CyberRegistration/
├── Program.cs                  # Application console (point d'entrée)
├── User.cs                     # Modèle utilisateur (Id, Username, PasswordHash, Role)
├── SecurityService.cs          # Détection d'attaques + validation mot de passe
├── CryptoService.cs            # Hachage SHA-256
├── DatabaseService.cs          # Accès SQL Server LocalDB
├── CyberRegistration.csproj    # Projet console
│
├── CyberRegistration.WinForms/
│   ├── Form1.cs                # Formulaire login/inscription (WinForms)
│   ├── DashboardForm.cs        # Tableau de bord admin (WinForms)
│   └── CyberRegistration.WinForms.csproj
│
└── CyberRegistration.Tests/    # Tests unitaires
```

---

## Prérequis

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- SQL Server LocalDB (inclus avec Visual Studio)

---

## Lancer le projet

### Interface console
```powershell
dotnet run --project .\CyberRegistration.csproj
```

### Interface graphique (WinForms)
```powershell
dotnet run --project .\CyberRegistration.WinForms\CyberRegistration.WinForms.csproj
```

---

## Base de données

La base `CyberDB` est créée automatiquement au premier lancement sur SQL Server LocalDB (`MSSQLLocalDB`).

### Tables

**dbo.Users**
| Colonne | Type | Description |
|---|---|---|
| Id | INT IDENTITY | Clé primaire |
| Username | NVARCHAR(100) | Nom d'utilisateur |
| PasswordHash | NVARCHAR(256) | Mot de passe haché (SHA-256) |
| Role | NVARCHAR(20) | Rôle : `admin` ou `user` |

**dbo.Logs**
| Colonne | Type | Description |
|---|---|---|
| Id | INT IDENTITY | Clé primaire |
| ActionType | NVARCHAR(50) | `Inscription` ou `Connexion` |
| Username | NVARCHAR(100) | Nom saisi (peut être l'attaque elle-même) |
| Details | NVARCHAR(500) | Champ concerné + type d'attaque détecté |
| CreatedAt | DATETIME | Horodatage |

### Mettre un utilisateur en admin
```powershell
sqllocaldb start MSSQLLocalDB | Out-Null
$pipe = (sqllocaldb info MSSQLLocalDB | Select-String "Nom de canal|Instance pipe name").ToString().Split(':',2)[1].Trim()
& "C:\Program Files\sqlcmd\sqlcmd.exe" -S "$pipe" -d CyberDB -E -Q "UPDATE dbo.Users SET Role='admin' WHERE Username='votre_username';"
```

---

## Types d'attaques détectés

| Pattern | Label |
|---|---|
| `DROP` | SQL Injection - DROP (suppression de table) |
| `DELETE` | SQL Injection - DELETE (suppression de données) |
| `SELECT` | SQL Injection - SELECT (extraction de données) |
| `INSERT` | SQL Injection - INSERT (insertion malveillante) |
| `--` | SQL Injection - Commentaire SQL |
| `;` | SQL Injection - Terminaison de requête |

---

## Sécurité

- Les requêtes SQL utilisent des **paramètres préparés** (aucune concaténation directe)
- Les mots de passe sont **hachés** avant stockage (jamais en clair)
- Le tableau de bord admin est accessible **uniquement** aux utilisateurs avec `Role = 'admin'` en base de données
- Les champs `Username` et `Password` sont analysés **avant** toute opération en base

---

## Exemple d'attaque détectée (console)

```
Attaque detectee !
  Type    : SQL Injection - DROP (suppression de table)
  Champ   : Nom d'utilisateur
  Action  : Inscription
```

---

## Lancer les tests
```powershell
dotnet test .\CyberRegistration.Tests\CyberRegistration.Tests.csproj
```
