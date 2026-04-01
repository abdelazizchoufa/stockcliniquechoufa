# Phase 0, 1 & 2 — Corrections & UX
- [x] Cloner le repo, Fix .NET 8, NuGet
- [x] Sécurisation BCrypt, Serilog, Mots de passe Seed
- [x] Pagination, Async EF Core, Tests Unitaires

---

# Phase 3 — Architecture Avancée & Fonctionnalités Métier

*L'objectif est d'implémenter 5 fonctionnalités majeures sans régressions.*

## Étape 1 : Refonte du Modèle de Données (Lots & Emplacements)
- [x] Créer les entités `Location` et `StockBatch` (Lot)
- [x] Modifier `StockMovement` pour lier aux Lots
- [x] Modifier `ApplicationDbContext` et `SeedData`
- [x] Générer la migration EF Core (Lots)

## Étape 2 : Implémentation de la Logique Métier FEFO
- [x] Adapter `IStockService` et `StockService` pour les Lots
- [x] Gérer la déduction FEFO (First Expired, First Out)
- [x] Créer l'interface UI pour afficher les Lots par article
- [x] Interface de Transfert Intersites

## Étape 3 : Workflows de Validation
- [x] Modèles `MaterialRequest` et `MaterialRequestLine`
- [x] Service et Contrôleur pour les demandes
- [x] Vues Razor de type panier pour "Commander du stock"
- [x] Validation par le Gestionnaire et décaissement automatique

## Étape 4 : Automatisation & Alertes
- [x] Créer `AutoReplenishmentService` (IHostedService)
- [x] Génération de `PurchaseOrder` (brouillons) sur stock bas

## Étape 5 : API Scanner (PWA / Flutter)
- [x] Étendre `StockApiController` (endpoints REST /api/scan/out)
- [x] Générateur de QR Code pour les fiches articles
