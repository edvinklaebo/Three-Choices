using System.Collections.Generic;
using System.Text;
using Core;

namespace UI
{
    public static class ObtainedItemsFormatter
    {
        public static string Format(IReadOnlyList<ObtainedItem> items, bool hasCompleteHistory = true)
        {
            if (items == null)
                return "Player information is not yet available.";

            var text = new StringBuilder();
            if (!hasCompleteHistory)
                text.AppendLine("Acquisitions from this older save are unavailable. Only newly recorded items are shown.");
            AppendCategory(text, items, ObtainedItemCategory.Upgrades);
            AppendCategory(text, items, ObtainedItemCategory.Abilities);
            AppendCategory(text, items, ObtainedItemCategory.Passives);
            AppendCategory(text, items, ObtainedItemCategory.Artifacts);
            return text.ToString();
        }

        private static void AppendCategory(StringBuilder text, IReadOnlyList<ObtainedItem> items,
            ObtainedItemCategory category)
        {
            text.AppendLine().AppendLine(category.ToString());
            var found = false;
            for (var i = 0; i < items.Count; i++)
            {
                var item = items[i];
                if (item == null || item.Category != category)
                    continue;

                found = true;
                text.Append(item.DisplayName).Append(" x").Append(item.Count).AppendLine();
                text.AppendLine(item.Description);
            }

            if (!found)
                text.AppendLine("No recorded items.");
        }
    }
}
