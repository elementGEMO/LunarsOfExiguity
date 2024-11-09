## Version 0.1.3
- Networked ``Honor of Greed`` to properly show on all clients
- Networked ``Cleansing Pool`` to disappear on all clients
- Added a new ``Cleansing Pool`` config to enable scaling with ``Player Count``, spawning one per ``Player`` when guaranteed
- Changed the message event for a guaranteed ``Cleansing Pool`` to have ``..`` instead of ``...``, being more consistent with the rest
- Added a condition behind ``Cleansing Pool`` just in case
- Fixed ``Relic / Honor of Focus`` from spamming NREs on stages without proper Teleporters
- Fixed ``Relic / Honor of Foucs`` to properly work on all Holdout Zones that ``Focused Convergence`` was allowed on
- Fixed ``Honor of Glass`` giving negative damage boosts when having barrier
## Version 0.1.2
- Added new Config options for ``Cleansing Pool``
<br> - Spawn Every # Stage in a Loop
<br> - Spawn After # Stages
<br> - Director Cost Modifier
<br> - Uses Before Consume
- Added a condition to ``Relic of Glass`` to prevent extremely small health that allows taking damage without dying
- ``Relic of Purity`` now sets proc-coefficient to an extremely small number, letting Survivors apply their status effects normally
<br> Default stat increases are now decreased due to allowing some items to function as well
- Fixed ``Relic of Focus`` not properly dealing damage sometimes
- Fixed ``Relic of Focus`` disabling an important variable that others use, notably ``Abstracted Locus`` from ``BubbetsItems``
- Temporarily blacklisted ``Egocentrism`` from being Fractured for the time being
## Version 0.1.1
- Forgot to add a dependency to ``FreeCostFix``
<br> - The mod is still functional even without it, but is preferred for ``Honor of Greed``
## Version 0.1.0
- Initial **BETA** release