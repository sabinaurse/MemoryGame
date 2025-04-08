# MemoryGame

**MemoryGame** is a C# WPF application implementing the classic Memory game, featuring user accounts, save/load functionality, real-time statistics, and customizable game settings. The project follows the MVVM design pattern and uses Data Binding extensively to separate the business logic from the UI.

## Features

- User Management:
  - Sign in, create, or delete user accounts, each associated with a custom image.
- Gameplay:
  - Play the Memory game with random board layouts.
  - Select predefined or custom board sizes (4x4 or 2x2 to 6x6 grids).
  - Choose from different image categories.
  - Time-limited gameplay with adjustable time settings.
  - Save and load games mid-progress, preserving the timer and board state.
- Statistics:
  - Track games played and games won per user.
  - View statistics in a dedicated window.
- Persistence:
  - Save user data, game progress, and statistics in external files.
- Architecture:
  - MVVM design pattern with extensive use of Data Binding.
  - Non-resizable main window.
  - Both mouse and keyboard interaction supported.

## Technologies Used

- C# (.NET 8)
- WPF (Windows Presentation Foundation)
- MVVM architecture
- File-based data persistence (JSON)
