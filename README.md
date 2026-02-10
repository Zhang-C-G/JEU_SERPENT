# SNAKE MULTIJOUEUR

Jeu Snake multijoueur avec serveur TCP et mode observateur.

## Structure
- `SnakeGame.Server/` - Serveur de jeu TCP
- `SnakeGame.Client/` - Client Windows Forms
- `SnakeGame.Shared/` - Classes communes

## Fonctionnalités
- Multijoueur en temps réel (TCP)
- Mode observateur
- Bonus d'invincibilité (cercle doré)
- Scores individuels
- Warp autour des bords (pas de game over sur les murs)
- Interface complète avec grille centrée

## Comment jouer
1. Compiler: `.\compile.bat`
2. Lancer: `.\lance.bat` (pour tout lancer)
3. Ou manuellement:
   - Fenêtre 1: `cd SnakeGame.Server && dotnet run`
   - Fenêtre 2: `cd SnakeGame.Client && dotnet run` (Joueur 1)
   - Fenêtre 3: `cd SnakeGame.Client && dotnet run` (Observateur)
   - Fenêtre 4: `cd SnakeGame.Client && dotnet run` (Joueur 2)

## Contrôles
- Flèches ou WASD: Déplacer le serpent
- Espace: Pause/Reprendre
- F2: Activer les contrôles (prendre le focus)
- Échap: Quitter

## Règles
- Mangez les cercles rouges: +10 points, serpent grandit
- Mangez les cercles dorés: +50 points, invincible 3 secondes
- Collision avec un autre serpent: recommence
- Les bords font warp (pas de mort)
