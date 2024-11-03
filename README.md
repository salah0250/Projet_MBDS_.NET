# Application de Gestion de Jeux (WPF, MAUI, WINUI)

## Vue d'ensemble
L'application permet aux utilisateurs de gérer et d'interagir avec leur bibliothèque de jeux. Les utilisateurs peuvent parcourir, filtrer et effectuer des actions liées aux jeux (comme télécharger, lancer, supprimer, etc.), tout en ayant la possibilité de mettre à jour leur profil (informations utilisateur telles que prénom, nom, etc.).

## Fonctionnalités

### 1. Liste des jeux

- **Pagination** : Prend en charge le défilement infini.
- **Options de filtrage** :
  - Filtrer par nom de jeu
  - Filtrer par catégorie
  - Filtrer par gamme de prix (minimum et maximum)

### 2. Gestion des jeux possédés
- **Liste des jeux possédés** : Affiche la liste des jeux possédés par l'utilisateur dans la section "Mes Jeux".
- **Affichage des détails d'un jeu** : Affiche des informations détaillées sur chaque jeu, y compris :
  - **Nom**
  - **Description**
  - **Statut**
  - **Catégories**
  - **Configuration requise**

### 3. Actions sur les jeux
- **Contrôle des jeux** :
  - Télécharger les jeux
  - Supprimer les jeux du stockage local
  - Lancer les jeux directement depuis l'application
- **Visibilité dynamique des boutons** :
  - Les boutons "Jouer" et "Supprimer" sont affichés uniquement si le jeu est téléchargé.
  - Le bouton "Télécharger" est masqué si le jeu est déjà disponible.

### 4. Contrôle de la lecture des jeux
- **Lancer et contrôler les jeux** : Les utilisateurs peuvent lancer des jeux directement depuis l'interface.

### 5. Gestion du profil utilisateur
- **Mise à jour du profil** :
  - Permettre à l'utilisateur de définir le chemin d'installation des jeux dans la section "Mes Jeux".
  - Mettre à jour les informations de compte telles que le prénom, le nom, le numéro de téléphone et l'email. Les modes édition et visualisation sont définis.

### 6. Source de données
- **Intégration avec le serveur** : Les données des jeux sont récupérées depuis un serveur pour garantir des informations à jour.

### Connexion
- **La connexion permet à l'utilisateur d'accéder à l'application via le serveur web (ASP.NET Core Identity)**

## Fonctionnalités optionnelles

### Affichage de description en texte enrichi
- **Affichage de description formatée** : Les descriptions des jeux sont affichées avec des options de formatage personnalisables telles que :
  - Style de police
  - Taille du texte

## Guide d'utilisation

### Parcourir les jeux
1. Lancez l'application et connectez-vous. Si vous n'êtes pas inscrit, vous pouvez créer un compte en cliquant sur "Créer un compte".
2. Naviguez dans la bibliothèque de jeux.
3. Faites défiler la liste des jeux à l'aide du défilement infini.
4. Utilisez différents filtres pour visualiser les jeux disponibles.

### Visualisation et gestion des jeux
1. Cliquez sur un jeu pour afficher ses détails.
2. Utilisez les boutons "Télécharger," "Jouer," ou "Supprimer" en fonction du statut du jeu.
   - **Remarque** : La visibilité des boutons change en fonction du statut de disponibilité du jeu.
3. Mettez à jour les chemins d'installation ou modifiez les informations d'identification si nécessaire.

### Mettre à jour votre profil
1. Accédez à la section profil de l'application.
2. Visualisez ou éditez les informations de votre compte.


# Plateforme de distribution de contenu

## Introduction

Cette plateforme est conçue pour gérer une bibliothèque de jeux vidéo en ligne accessible aux utilisateurs et administrateurs. Elle propose la gestion des utilisateurs, des jeux, des transactions d'achat et des téléchargements sécurisés. Le projet est développé avec ASP.NET Core, utilisant une base de données pour stocker les informations sur les utilisateurs, les jeux, les catégories et les transactions.

## Fonctionnalités

### Gestion des Utilisateurs et des Rôles
- **Utilisateur** : Chaque utilisateur possède un compte unique avec des informations essentielles (ID, prénom, nom, email, role).
- **Rôles** : Les utilisateurs peuvent avoir le rôle d’Administrateur ou de Joueur.
    - **Administrateurs** : Accès complet aux fonctionnalités de gestion des jeux.
    - **Joueurs** : Peuvent accéder aux jeux achetés et en acheter de nouveaux.

> **NB** : Lorsqu'un nouvel utilisateur est créé, il est automatiquement assigné au rôle de joueur normal. Pour lui attribuer le rôle d'administrateur, il faut modifier manuellement son rôle dans la base de données en ajoutant une entrée dans la table `AspNetUserRoles`. Une fois que le rôle administrateur est attribué, la gestion des utilisateurs peut se faire via la plateforme.


### Gestion des Jeux
- **Jeu** : Chaque jeu possède des attributs essentiels pour son identification et stockage :
    - **ID** : Identifiant unique du jeu.
    - **Titre** : Nom du jeu.
    - **Description** : Description du jeu.
    - **Payload** : Données binaires du jeu (fichier téléchargé par les utilisateurs).
    - **Prix** : Prix du jeu.
    - **Catégorie** : Le jeu est associé à une ou plusieurs catégories (genres) pour le filtrage.
    - **Image** : Image de couverture du jeu.
  
- **UserGame** : Une table d’association qui lie les utilisateurs aux jeux qu’ils possèdent, contenant :
    - ID de l’utilisateur
    - ID du jeu
    - Date d'achat

### Fonctions pour les Administrateurs
- **Ajouter un Jeu** : Les administrateurs peuvent ajouter de nouveaux jeux avec toutes les informations nécessaires.
> **NB** : Lors de l'ajout du jeu, il est impératif que le payload et le titre du jeu soient identiques pour pouvoir lancer le jeu directement depuis le client lourd.
- **Modifier un Jeu** : Les administrateurs peuvent modifier les informations d'un jeu existant.
- **Supprimer un Jeu** : Les administrateurs peuvent supprimer un jeu de la bibliothèque.
- **Gérer les Catégories** : Les administrateurs peuvent créer, modifier ou supprimer des catégories de jeux.

### Suivi des Joueurs en Ligne
- **Affichage en Temps Réel** : Les utilisateurs peuvent voir quels joueurs sont en ligne grâce à l'implémentation de **SignalR** (WebSocket).
- **Hub de Connexion** : Un hub `OnlineHub` gère la connexion et la déconnexion des utilisateurs en temps réel.
- **Mise à Jour Automatique** : Lorsqu'un utilisateur se connecte ou se déconnecte, la liste des joueurs en ligne est automatiquement mise à jour et affichée à tous les utilisateurs connectés.

> Cette fonctionnalité permet une interaction plus engageante entre les joueurs, et l’affichage des statuts de connexion est actualisé en direct sans rafraîchissement manuel.

### Fonctions pour les Utilisateurs
- **Acheter un Jeu** : Les utilisateurs peuvent acheter des jeux, qui seront alors ajoutés à leur bibliothèque.
- **Télécharger un Jeu** : Les utilisateurs peuvent télécharger les jeux qu’ils possèdent. Les téléchargements sont sécurisés et gérés via la table `UserGame`.
- **Bibliothèque de Jeux** : Les utilisateurs peuvent voir la liste des jeux qu’ils possèdent.
- **Statut des Joueurs en Temps Réel** : Les utilisateurs peuvent voir une liste des autres joueurs en ligne et leur statut actuel.

### Navigation et Filtrage des Jeux
- **Voir Tous les Jeux** : Tous les utilisateurs (connectés ou non) peuvent voir la liste des jeux disponibles sur la plateforme.
- **Filtres de Recherche** :
    - Par nom
    - Par fourchette de prix
    - Par catégorie
    - Par statut de possession
    - Par taille de fichier

### Fonctionnalités Supplémentaires
- **Page de Statistiques de la Plateforme** : Une page de statistiques affiche les données globales sur la plateforme :
    - Nombre total de jeux disponibles.
    - Nombre de jeux par catégorie.
    - Nombre moyen de jeux possédés par compte.
    - Temps moyen joué par jeu.
    - Nombre maximal de joueurs en simultané sur la plateforme et par jeu.
  
- **Stockage Externe des Jeux** : Étant donné la taille importante des fichiers de jeux, un mécanisme de stockage externe est mis en place pour les fichiers binaires (par exemple, fichiers `.zip`) pour éviter de les stocker directement dans la base de données.

## Structure de la Base de Données

1. **User** : Contient les informations des utilisateurs (ID, prénom, nom, email, rôle).
2. **Game** : Contient les informations sur chaque jeu (ID, titre, description, payload, prix, catégorie).
3. **Category** : Contient les genres ou catégories de jeux.
4. **UserGame** : Associe les utilisateurs aux jeux qu’ils possèdent, incluant la date d'achat.

# Game & GameServer

# Features Implemented

1. **Single-User Compatibility**
-Current Setup: The game currently supports only one player per session.
2. **Login System**
-Authentication: Players authenticate with a username and password.
-Unique Username Selection: Each player can choose a unique username to participate in the game.
3. **Ready Check**
-Functionality: A ready-check system ensures that all players confirm they are ready before the game starts.

#    Game Server
*Branch GameServer*
**Console-Based Server**
1. Server Type: A console application acts as the game server, coordinating player actions and managing game flow.
2. Communication Protocol: TCP is used to handle communication between players and the server.
**Serialization Issue**
-Current Limitation: The server's synchronization functionality is currently limited due to a serialization issue with MessagePack.

#  Game Flow
Game Start
1. Player Readiness: All players must confirm readiness for the game to start.
2. Role Assignment: The server assigns one player as the Game Master (GM) and informs each participant of their roles.

## Game Master (GM) Functionality
Cell Selection: The GM chooses a target cell on the game grid for players to click
