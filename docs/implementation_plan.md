# Plan d'Architecture — Phase 3 (Fonctionnalités Avancées)

L'objectif de cette phase est de transformer le MVP en un véritable ERP médical complet en implémentant 5 fonctionnalités métier majeures : Lots (FEFO), Multi-Magasins, Scan Code-barres, Workflows d'approbation et Automatisation.

Pour garantir la stabilité du projet, nous procéderons par étapes itératives.

---

## Étape 1 : Refonte du Modèle de Données (Lots & Emplacements)
*La base de tout le système d'inventaire.*

- **Création de `Location` (Emplacement)** : Pharmacie Centrale, Urgences, Bloc Opératoire, Imagerie.
- **Création de `StockBatch` (Lot)** : Numéro de Lot, Date d'expiration, Quantité disponible, Lié à un `StockItem` et une `Location`.
- **Modification de `StockMovement`** : Lier le mouvement non plus au `StockItem` de façon abstraite, mais à un `StockBatch` précis (pour retracer le lot manipulé). Ajouter le type de mouvement `TransfertInterne` (avec `SourceLocationId` et `DestinationLocationId`).
- **Migration EF Core** : Générer une migration pour ces énormes changements de schéma sans casser le seed data (le seed initialisera tout dans le magasin central avec un "Lot par défaut").

## Étape 2 : Implémentation de la Logique Métier FEFO (First Expired, First Out)
*Assurer la sécurité des patients en évitant les produits périmés.*

- **StockService (Sorties)** : Quand un médecin demande 10 boîtes de gants, le service trouvera automatiquement le(s) lot(s) qui expirent le plus tôt, et les déduira en priorité. S'il faut puiser dans 2 lots différents (ex: 3 d'un vieux lot, 7 d'un nouveau), le système créera 2 mouvements automatiquement.
- **StockService (Transferts)** : Créer la fonction pour déplacer un lot complet ou partiel de la Pharmacie Centrale vers une Pharmacie de Service (Emplacement).

## Étape 3 : Workflows de Validation & Rôles
*Sécuriser les flux logistiques.*

- **Création de `MaterialRequest` (Demande de matériel)** : Un acteur `Lecture` d'un Service crée une demande pour son emplacement.
- **Création de `MaterialRequestLine`** : Les articles demandés.
- **Statuts** : `Brouillon` -> `Soumis` -> `EnCoursDePreparation` -> `Livre`.
- **Contrôleur (MaterialRequestController)** : Vues pour permettre aux infirmières de commander, et aux Gestionnaires de valider et distribuer (ce qui déclenchera un transfert de stock sous le capot).

## Étape 4 : Automatisation & Alertes
*Aider le service des achats.*

- **Générateur de commandes (`AutoReplenishmentService`)** : Un Hosted Service (tâche de fond) qui tourne chaque nuit, vérifie quels lots périment à < 30 jours (pour déclencher une alerte) et quels articles (tous lots confondus) passent sous le seuil d'alerte, pour générer des *Brouillons* de `PurchaseOrder`.

## Étape 5 : API Scanner (PWA & Flutter)
*Pour le travail sur le terrain.*

- **Extension du `StockApiController`** : Ajouter des endpoints REST `POST /api/scan/out` et `POST /api/scan/inventory`.
- **QR Codes** : Générer et afficher des QR Codes depuis l'application Web pour coller sur les boîtes.
- Ces endpoints permettront de décrémenter le stock ou de le valider via une application Flutter (ou PWA) conçue séparément, que le magasinier utilisera avec son téléphone.

---

Voulez-vous que je commence par l'**Étape 1 et 2 (Modèle de données Lots/Magasins et Algorithme FEFO)** pour installer les fondations solides ?
