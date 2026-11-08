using RimWorld;
using UnityEngine;
using Verse;
using VFETribals;

namespace StartingCornerstones
{
    public class StartingCornerstonesSettings : ModSettings
    {
        public const int DefaultStartingPoints = 0;
        public const bool DefaultGrantSkippedEras = true;
        public const int MaxStartingPoints = 10;

        public int startingPoints = DefaultStartingPoints;
        public bool grantSkippedEras = DefaultGrantSkippedEras;

        public override void ExposeData()
        {
            Scribe_Values.Look(ref startingPoints, "startingPoints", DefaultStartingPoints);
            Scribe_Values.Look(ref grantSkippedEras, "grantSkippedEras", DefaultGrantSkippedEras);
            startingPoints = Mathf.Clamp(startingPoints, 0, MaxStartingPoints);
        }
    }

    public class StartingCornerstonesMod : Mod
    {
        public static StartingCornerstonesSettings settings;

        public StartingCornerstonesMod(ModContentPack content) : base(content)
        {
            settings = GetSettings<StartingCornerstonesSettings>();
        }

        public override string SettingsCategory()
        {
            return "VFE Tribals - Starting Cornerstones";
        }

        public override void DoSettingsWindowContents(Rect inRect)
        {
            Listing_Standard listing = new Listing_Standard();
            listing.Begin(inRect);

            listing.Label("SCS_StartingPoints".Translate(settings.startingPoints), tooltip: "SCS_StartingPointsTip".Translate());
            settings.startingPoints = Mathf.RoundToInt(listing.Slider(settings.startingPoints, 0f, StartingCornerstonesSettings.MaxStartingPoints));
            listing.Gap();

            listing.CheckboxLabeled("SCS_GrantSkippedEras".Translate(), ref settings.grantSkippedEras, "SCS_GrantSkippedErasTip".Translate());

            listing.End();
        }
    }

    // Game.InitNewGame calls StartedNewGame once, after the scenario has set up the player faction, and never
    // on load, so existing saves are untouched and the grant needs no saved state.
    public class GameComponent_StartingCornerstones : GameComponent
    {
        public GameComponent_StartingCornerstones(Game game)
        {
        }

        public override void StartedNewGame()
        {
            StartingCornerstonesSettings settings = StartingCornerstonesMod.settings;
            int points = settings.startingPoints;
            if (settings.grantSkippedEras)
            {
                points += PointsForErasUpTo(Faction.OfPlayer.def.techLevel);
            }
            if (points > 0)
            {
                GameComponent_Tribals.Instance.OffsetAvailableCornerstonePoints(points);
            }
        }

        // Sums what Tribals' own era advancements would have paid to reach this tech level, so a patched or
        // modded EraAdvancementDef counts. With the stock defs that is 1 per level above Animal, up to Ultra.
        private static int PointsForErasUpTo(TechLevel techLevel)
        {
            int points = 0;
            foreach (EraAdvancementDef era in DefDatabase<EraAdvancementDef>.AllDefsListForReading)
            {
                if (era.newTechLevel <= techLevel)
                {
                    points += era.cornerstonePoint;
                }
            }
            return points;
        }
    }
}
