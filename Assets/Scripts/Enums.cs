using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

/// <summary>
/// Bonus types
/// </summary>
[Flags]

/// <summary>
/// Our simple game state
/// </summary>
public enum GameState
{
    Playing,
    SelectionStarted,
    Animating,
    Lost
}
