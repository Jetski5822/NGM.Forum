using System.Collections.Generic;

namespace NGM.Forum.Controllers {
    public static class DynamicZoneExtensions {
        public static void RemoveItemFrom(dynamic zone, string itemToDelete) {
            var itemsToDelete = new List<object>();

            foreach (var item in zone.items) {
                if (item.Metadata.Type == itemToDelete)
                    itemsToDelete.Add(item);
            }

            foreach (var item in itemsToDelete) {
                zone.Items.Remove(item);
            }
        }

    }
}