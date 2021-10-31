﻿using RimWorld;
using Verse;

namespace Hospitality.Utilities
{
    public static class FoodUtility
    {
        public static bool GuestCanSatisfyFoodNeed(Pawn guest)
        {
            //Check Inventory
            //var inventoryFood = RimWorld.FoodUtility.BestFoodInInventory(guest, minFoodPref: FoodPreferability.RawTasty);
            //if (inventoryFood != null) return true;

            //Search FoodSource
            if (RimWorld.FoodUtility.TryFindBestFoodSourceFor_NewTemp(guest, guest, false, out var foodSource, out var foodDef, false, true, false, false, false, false, false, false, false, false, FoodPreferability.RawTasty))
            {
                if (foodSource != null && foodDef != null) return true;
            }
            return false;
        }

        public static bool GuestCanUseFoodSourceInternal(Pawn guest, Thing foodSource)
        {
            //Log.Message($"Checking FoodSource for {guest.NameShortColored}: {foodSource?.LabelCap} ({foodSource?.Position})");

            //We need to get current status data of the guest
            var foodDef = RimWorld.FoodUtility.GetFinalIngestibleDef(foodSource, true);
            var desperate = guest.needs.food?.CurCategory == HungerCategory.Starving;

            //Log.Message($"FooDef: {foodDef?.LabelCap}| Desperate: {desperate}");
            //If they are starving, they simply take the next best food source
            if (desperate || guest.GetMapComponent().guestsCanTakeFoodForFree)
            {
                return true;
            }

            //Check whether the current food source is a dispenser set as a vending machine for this guest
            //Log.Message($"Dispenser: {foodSource is Building_NutrientPasteDispenser}| CanBeUsed: {(foodSource.TryGetComp<CompVendingMachine>()?.CanBeUsedBy(guest, foodDef) ?? false)}");
            if (foodSource is Building_NutrientPasteDispenser dispenser && (dispenser.TryGetComp<CompVendingMachine>()?.CanBeUsedBy(guest, foodDef) ?? false))
            {
                return true;
            }
            return false;
        }

        //This method is meant to be extended as we find new foodsources that go past the check in 'GuestCanUseFoodSourceInternal'
        public static bool GuestCanUseFoodSourceExceptions(Pawn guest, Thing foodSource, ThingDef foodDef, bool desperate)
        {
            return true;
        }

        public static bool TryPayForFood(Pawn buyerGuest, Building_NutrientPasteDispenser dispenser)
        {
            var vendingMachine = dispenser.TryGetComp<CompVendingMachine>();
            if (vendingMachine.IsActive() && dispenser.CanDispenseNow)
            {
                if (vendingMachine.IsFree) return true;

                if (!vendingMachine.CanAffordFast(buyerGuest, out Thing silver)) return false;

                vendingMachine.ReceivePayment(buyerGuest.inventory.innerContainer, silver);
                return true;
            }
            return false;
        }

        public static bool WillConsume(Pawn pawn, ThingDef foodDef)
        {
            if (foodDef == null) return false;
            var restrictions = pawn.foodRestriction.CurrentFoodRestriction;
            if (!restrictions.Allows(foodDef)) return false;

            var fineAsFood = foodDef.ingestible?.preferability == FoodPreferability.Undefined || foodDef.ingestible?.preferability == FoodPreferability.NeverForNutrition || pawn.WillEat(foodDef);
            return !foodDef.IsDrug && fineAsFood;
        }
    }
}
