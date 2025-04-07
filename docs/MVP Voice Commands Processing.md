## MVP Voice Command Workflow

This describes the simplified voice command processing for the Minimum Viable Product (MVP), focused on direct unit control.

1.  **Player Speaks:** Issues a direct command targeting a specific unit with a clear action (e.g., "Peter, attack William!").
2.  **Client:**
    *   Captures the player's audio.
    *   Performs speech-to-text transcription.
    *   Displays the transcribed text for confirmation.
    *   Sends the transcribed text command to the backend.
3.  **Backend:**
    *   Receives the text command.
    *   Parses the command to identify the core components:
        *   **Actor:** The unit(s) meant to perform the action (e.g., 'Peter').
        *   **Action:** The verb indicating the desired task (e.g., 'attack', 'help', 'move to').
        *   **Target:** The object or location of the action (e.g., 'William', 'Bravo point').
    *   Validates the parsed command (e.g., Are the units valid? Is the action possible?).
    *   Generates a structured data package representing the validated command (e.g., `{ actor: 'Peter', action: 'attack', target: 'William' }`).
    *   Returns this data package to the game client.
4.  **Client:**
    *   Receives the structured data from the backend.
    *   Translates the action data into specific game events (e.g., instructing the 'Peter' game object to initiate an attack sequence on the 'William' game object).
    *   Provides clear visual and/or auditory feedback to the player confirming the command is being executed (e.g., unit highlights, confirmation sound, unit starts moving/attacking).

This MVP workflow focuses on translating direct player speech into actionable commands for game units.