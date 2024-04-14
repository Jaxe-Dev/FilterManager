# Filter Manager
![Mod Version](https://img.shields.io/badge/Mod_Version-1.6-blue.svg)
![RimWorld Version](https://img.shields.io/badge/Built_for_RimWorld-1.5-blue.svg)
![Harmony Version](https://img.shields.io/badge/Powered_by_Harmony-2.3-blue.svg)
![Steam Downloads](https://img.shields.io/steam/downloads/2812197851?colorB=blue&label=Steam+Downloads)

[Link to Steam Workshop page](https://steamcommunity.com/sharedfiles/filedetails/?id=2812197851)

---

Manage your filters anywhere.

Got a list of things you want for your freezer? *No problem.*\
Want to invert that list? *Can do.*\
Have a specific outfit and want to copy the list to a stockpile zone? *Easy with this mod.*

This mod allows saving and loading filter presets in any context (Bills, Outfits, Food restrictions, and any other situations where the filter menu is displayed).

Simply click the *Presets* button next to the newly added *Invert* button on the top of your filters window to access the new presets window.
All checked and unchecked things will be saved included any special filters. Any hitpoint and quality range will also be saved if applicable.

Two integrated presets are included: Degradable (which will select things which must be indoors to avoid deterioration) and Rottable (which selects things which can rot unless kept frozen).

This mod can be added or removed to the game at any time without issue.

---

**Note to Translators**: *Although the offers are appreciated, for maintenance reasons only English will included with the base mod. All translations should be uploaded as language submods with full permission to use the mod's images in any way.*

---

The following base methods are patched with Harmony:
```
Prefix  : Verse.ThingFilterUI.DoThingFilterConfigWindow
Postfix : Verse.ThingFilterUI.DoThingFilterConfigWindow
```
*A prefix marked by a \* denotes that in some circumstances the original method will be bypassed*
