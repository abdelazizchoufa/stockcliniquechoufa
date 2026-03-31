# Gestion de stock medicale en C#

Cette base de projet propose une application ASP.NET Core MVC pour suivre le stock d'un centre de diagnostique medical avec quatre services :

- Imagerie
- Laboratoire d'analyses
- Consultations
- Hopital du jour

## Fonctions incluses

- tableau de bord avec indicateurs simples
- liste des articles du stock
- alerte sur les niveaux bas
- date d'expiration des consommables
- ajout manuel d'un article
- separation des articles par service
- persistance des donnees avec SQL Server et Entity Framework Core
- enregistrement des entrees, sorties et ajustements de stock
- historique des mouvements par article
- gestion de fournisseurs
- commandes d'approvisionnement avec reception et mise a jour automatique du stock
- authentification par formulaire avec roles
- rapport de consommation par service et par article
- recherche et filtrage avances du stock
- alertes stock bas et expiration sur le tableau de bord
- commandes fournisseurs multi-lignes
- export CSV des rapports
- journal d'audit des actions utilisateur
- edition des articles de stock
- creation et modification des fournisseurs
- tableau de bord enrichi avec synthese par service et activite recente
- statistiques financieres des commandes fournisseurs
- vues imprimables pour les fiches article, commandes et rapports
- suppression securisee avec ecrans de confirmation
- interface visuelle modernisee

## Structure

- `MedicalStockManager/Controllers` : logique web MVC
- `MedicalStockManager/Data` : contexte EF Core et jeu de donnees initial
- `MedicalStockManager/Models` : entites metier
- `MedicalStockManager/Services` : service de gestion du stock
- `MedicalStockManager/Views` : ecrans Razor
- `MedicalStockManager/wwwroot` : styles CSS

## Prerequis

Installe :

- le SDK .NET 8
- aucune base externe n'est obligatoire en mode local, SQLite est utilise par defaut

Lien .NET :

[https://dotnet.microsoft.com/download/dotnet/8.0](https://dotnet.microsoft.com/download/dotnet/8.0)

La chaine de connexion par defaut se trouve dans [appsettings.json](C:\Users\Chouf\Documents\New%20project\MedicalStockManager\appsettings.json) et pointe vers le fichier SQLite `medical-stock.db`.

## Demarrage

Depuis le dossier `MedicalStockManager` :

```powershell
dotnet restore
dotnet run
```

Puis ouvre l'adresse locale affichee par ASP.NET Core.

Au premier lancement, Entity Framework Core cree automatiquement la base si elle n'existe pas.

Important : si tu avais deja lance une ancienne version du projet avec une base existante, supprime le fichier `medical-stock.db` ou passe aux migrations EF Core pour regenerer le schema avec les tables `Suppliers`, `PurchaseOrders`, `PurchaseOrderLines`, `AppUsers` et `AuditLogs`.

Si tu veux ensuite passer a une gestion par migrations :

```powershell
dotnet tool install --global dotnet-ef
dotnet ef migrations add InitialCreate
dotnet ef database update
```

## Comptes de demonstration

- `admin` / `Admin123!` : acces complet
- `stock` / `Stock123!` : gestion du stock et approvisionnement
- `lecture` / `Lecture123!` : consultation uniquement
