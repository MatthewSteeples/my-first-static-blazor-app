using BlazorApp.Shared;
using Blazored.LocalStorage;

namespace BlazorApp.Client.Services
{
    public static class TrackedItemStorageHelper
    {
        public static async Task SaveTrackedItemWithArchivingAsync(ILocalStorageService localStorage, TrackedItem trackedItem)
        {
            // Check if archiving is needed
            var archive = trackedItem.CheckForArchiving();
            
            if (archive != null)
            {
                // Find the next archive number for this TrackedItem
                var existingArchiveKeys = (await localStorage.KeysAsync())
                    .Where(key => key.StartsWith($"TrackedItemArchive{trackedItem.Id}-"))
                    .ToList();

                var nextArchiveNumber = 1;
                if (existingArchiveKeys.Any())
                {
                    var maxNumber = existingArchiveKeys
                        .Select(key => 
                        {
                            var parts = key.Split('-');
                            return int.TryParse(parts.LastOrDefault(), out int num) ? num : 0;
                        })
                        .DefaultIfEmpty(0)
                        .Max();
                    nextArchiveNumber = maxNumber + 1;
                }

                archive.ArchiveNumber = nextArchiveNumber;

                // Save the archive
                var archiveKey = $"TrackedItemArchive{trackedItem.Id}-{nextArchiveNumber}";
                await localStorage.SetItemAsync(archiveKey, archive);
            }

            // Save the TrackedItem
            await localStorage.SetItemAsync($"TrackedItem{trackedItem.Id}", trackedItem);
        }
    }
}