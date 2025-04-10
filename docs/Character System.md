# Character System

## TLDR (German)

Das Charaktersystem bietet einzigartige NPCs mit ausgeprägten Persönlichkeiten, welche ihr Verhalten im Kampf sowie ihre Reaktionen auf Sprachbefehle prägen. Jeder NPC verfügt über drei zentrale Charaktermerkmale (Mut vs. Vorsicht, Disziplin vs. Eigenständigkeit, Loyalität vs. Selbsterhaltung), die auf einer Skala von -10 bis +10 bewertet werden. Diese Eigenschaften beeinflussen nicht nur die Kampfwerte, sondern auch die taktischen Entscheidungen der Einheit - von der Kampfbereitschaft über die Befolgung von Befehlen bis hin zur Risikobereitschaft. Zusätzlich haben NPCs individuelle Kommunikationspräferenzen (beispielsweise direkt, respektvoll oder motivierend), die sich auf die Effektivität von Befehlen auswirken. Die Persönlichkeitsmerkmale der Gegner können taktisch ausgenutzt werden - so lassen sich etwa besonders waghalsige Feinde gezielt in Fallen locken. Durch Kampferfahrung entwickeln sich die Charaktereigenschaften der Einheiten dynamisch weiter.

## Introduction

The character system in this game is designed to have diverse, personality-driven NPCs that respond uniquely to player voice commands. This system focuses on personality traits that influence both combat behavior and command response, creating strategic depth through NPC management.

## Personality Traits

Each NPC has three personality traits, each represented as a value from -10 to +10, with negative values representing one end of the spectrum and positive values representing the other.

### Courage vs. Caution (-10 to +10)

- **Courage (+1 to +10)**: NPCs with positive values are more willing to engage in combat and take risks
  - At +5 to +7: Willing to hold position under moderate fire
  - At +8 to +10: Will charge into dangerous situations, prioritizing offense over defense
- **Caution (-1 to -10)**: NPCs with negative values prioritize survival and avoid unnecessary risks
  - At -3 to -5: Prefers to maintain distance and safety margin in combat
  - At -6 to -10: May retreat from combat when health drops below 50%

- **Combat Impact:**
  - Each point of Courage adds +2% to attack damage
  - Each point of Caution adds +1% to defense when in cover

### Discipline vs. Independence (-10 to +10)

- **Discipline (+1 to +10)**: NPCs with positive values follow orders more precisely
  - At +5 to +7: Follows orders with minimal deviation
  - At +8 to +10: Executes commands exactly as given, even in suboptimal circumstances
- **Independence (-1 to -10)**: NPCs with negative values interpret commands more freely
  - At -3 to -5: Adapts commands to current situation as they see fit
  - At -6 to -10: May ignore parts of orders that seem unreasonable to them

- **Combat Impact:**
    - Each point of Discipline adds +5% accuracy to command execution
    - Each point of Independence adds a 2% chance to take alternative action if better opportunity arises

### Loyalty vs. Self-Preservation (-10 to +10)

- **Loyalty (+1 to +10)**: NPCs with positive values prioritize team objectives and allies
  - At +5 to +7: Will assist nearby allies in trouble
  - At +8 to +10: May sacrifice their safety to protect teammates or complete objectives
- **Self-Preservation (-1 to -10)**: NPCs with negative values prioritize their own survival
  - At -3 to -5: Reluctant to take actions that endanger themselves
  - At -6 to -10: May abandon objectives or teammates if personally threatened

- **Combat Impact:**
  - Each point of Loyalty gives +3% bonus to defense when near allies
  - Each point of Self-Preservation reduces the health threshold for retreat by 3%

## Communication Preferences

Each NPC may have a preferred communication style. If a command is phrased in this preferred style, the NPC is more likely to comply favorably. If an NPC has no specific preference, the command's phrasing style does not significantly impact their response.

### Communication Styles

| Style             | Description                        | Example Command                                                                  |
| ----------------- | ---------------------------------- | -------------------------------------------------------------------------------- |
| **Direct**        | Concise, clear instructions        | "Schmidt, attack left flank"                                                     |
| **Polite**        | Respectful, considerate phrasing   | "Weber, would you please secure that position?"                                  |
| **Inspiring**     | Motivational, emotional appeals    | "Show them what you're made of, Fischer! For victory!"                           |
| **Logical**       | Reasoned explanations and analysis | "Müller, the enemy is exposed at coordinate B5, targeting them would be optimal" |
| **Authoritative** | Commands based on hierarchy        | "As your commander, I order you to advance!"                                     |

## Using Enemy Weaknesses

Enemies have personalities just like allies. You can use their weak spots against them if you pay attention.

| Trait (Range)                   | Their Weak Spot    | How to Use It (Examples)                 | What Happens                                |
| :------------------------------ | :----------------- | :--------------------------------------- | :------------------------------------------ |
| **Courageous (+5 to +10)**      | Too Confident      | Pretend to run away; look easy to beat | They leave cover and chase carelessly       |
| **Cautious (-5 to -10)**        | Gets Scared Easily | Shoot at them a lot; charge at them      | They run away too soon                      |
| **Disciplined (+5 to +10)**     | Sticks to the Plan | Attack from the side; use surprises      | They get confused and fight worse           |
| **Independent (-5 to -10)**     | Won't Listen       | Take out their boss; confuse them        | They ignore their boss's orders             |
| **Loyal (+5 to +10)**           | Will Risk Self     | Attack their nearby friends              | They leave their spot to help friends       |
| **Self-Preserving (-5 to -10)** | Runs Away Easily   | Bring more soldiers; use big guns        | They run away from the whole fight          |

## Character Development

Characters gain experience by completing missions. Upon leveling up, besides upgrades to core stats like health or damage, traits may also shift based on the combat experiences gained during the previous level:

- Successful aggressive actions may increase Courage
- Taking heavy damage may increase Caution
- Successful following of orders may increase Discipline
- Finding better solutions independently may increase Independence
- Saving teammates may increase Loyalty
- Near-death experiences may increase Self-Preservation

## Example Characters

### Allied Units

#### Ivan

- **Class:** Ranged
- **Traits:** Courage +7, Discipline +5, Loyalty +8
- **Prefers:** Direct commands

#### Lena

- **Class:** Ranged
- **Traits:** Courage -6, Discipline -8, Loyalty +4
- **Prefers:** No preference

#### Boris

- **Class:** Ranged
- **Traits:** Courage +2, Discipline +9, Loyalty +6
- **Prefers:** Logical commands

### Enemy Units (Defenders)

#### Markov

- **Class:** Ranged
- **Traits:** Courage +8, Discipline +6, Loyalty +5

#### Ghost

- **Class:** Ranged
- **Traits:** Courage -8, Discipline -6, Loyalty -7

#### Dimitri

- **Class:** Ranged
- **Traits:** Courage +9, Discipline -3, Loyalty +9
