# Alchemy Card Game

This is a two-player, turn-based card game developed in Unity. The game features a top-down 3D interface where players take turns either drawing a jewel as a card or playing one from their hand. First player to collect 4 different types of jewels in their hand wins.

## 🎮 Gameplay

- Each player starts with one random jewel.
- On a player's turn, they can:
  - **Draw a jewel** (if they have fewer than 4 jewels), or
  - **Play a jewel** to activate its effect.
- Jewel effects include:
  - Destroying an opponent's jewel.
  - Drawing a jewel for yourself.
  - Drawing a jewel for your opponent.
  - Drawing a selected jewel type.
- A player wins if they collect **all 4 different types** of jewels in their hand.

## 🃏 Jewel Types

1. **Ruby(DestroyOpponentCard)** – Destroys one random jewel from the opponent's hand.
2. **Emerald(DrawCardForSelf)** – Allows the player to draw an additional jewel.
3. **Sapphire(DrawCardForOpponent)** – Forces the opponent to draw a jewel.
4. **Amethyst(DrawSpecificCard)** – Lets the player draw a jewel type of their choice.

## 🛠️ Technologies

- **Unity Engine (C#)**
- Unity UI System
- Localization system (support for English & Turkish)
- Ads integration (rewarded and interstitial ads)

## 🌍 Localization

The game supports two languages:
- English 🇬🇧
- Turkish 🇹🇷

Players can choose their preferred language from the main menu.

## 📦 Installation

To run the project:

1. Clone the repository:
   ```bash
   git clone https://github.com/SirCardboardbox/AlchemyCardGame.git
   ```

2. Open the project in Unity (Unity 2022.x or later recommended).

3. Run the game from the `MainMenu` scene.

## 🔁 Game Flow

1. The game starts with a tutorial shown once per device (can be reset).
2. After closing the tutorial, players are directed to the main menu.
3. Starting a game triggers a rewarded ad.
4. Each player takes turns with a visible turn timer.
5. If a player doesn't act before the timer ends, the turn passes automatically.
6. The game ends when a player has all four unique jewel types.

## 📋 Developer Notes

- You can pause the game and return to the main menu.
- The deck automatically generates cards randomly.
- All gameplay logic is managed by `GameManager.cs`.

## ✨ Feature Plans

- Adding online multiplayer.
- Custom made meshes and UI.
- Game mode against an AI.


## 👨‍💻 Developer

**Sadri Alp Güldür**  
[www.linkedin.com/in/sadri-alp-guldur]

## 📄 License

This project is licensed under MIT license. Check LICENSE file for more. 

Feel free to fork, contribute, or open issues!
