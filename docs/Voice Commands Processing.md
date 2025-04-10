## How Voice Commands Work

The voice command system processes player speech differently depending on the game phase (planning vs. battle), but follows a core workflow:

1.  **Player Speaks:** Issues a voice command, ranging from conversational requests during planning (e.g., "Peter, take the sniper position.") to direct tactical orders during battle (e.g., "Squad Alpha, suppress that bunker!").
2.  **Client:**
    *   Captures the player's audio.
    *   Performs speech-to-text transcription.
    *   Displays the transcribed text.
    *   Sends the transcribed text command to the backend.
3.  **Backend:**
    *   Receives the text command from the client.
    *   Parses the command to identify the intent, target unit(s), and the requested action.
    *   Considers the current game phase and context:
        *   **During Planning:** Focuses on interpreting broader conversational intent or preparatory commands. It takes into account unit personality, history, and the current situation to generate appropriate NPC dialogue or setup actions (e.g., moving to a position, adopting a formation).
        *   **During Battle:** Emphasizes rapid parsing of direct tactical commands. It factors in unit traits (like Courage vs. Caution) and the immediate combat situation to determine how the command is executed under pressure.
    *   Generates a structured response containing:
        *   NPC dialogue snippets (more common in planning).
        *   Specific, actionable data (movement vectors, targeting priorities, abilities to use).
    *   Returns this structured data package to the game client.
4.  **Client:**
    *   Receives the structured data from the backend.
    *   Displays any included NPC dialogue.
    *   Translates the action data into game events: animates units performing actions (moving, shooting, taking cover), updates their status, and modifies tactical objectives.
    *   Provides clear visual and auditory feedback to the player confirming the command's reception and execution (e.g., unit status changes, confirmation sounds/visual cues).
5.  Units carry out the interpreted command based on their traits and the backend's instructions until they receive a new order or a significant change in the tactical situation necessitates a different action.

This system allows for rich, personality-driven interactions during preparation and maintains responsive, clear tactical control during the intensity of combat.
