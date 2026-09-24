using ThemeForge.Abstractions.Interfaces;
using ThemeForge.Abstractions.Records.UseOfTheme;
using ThemeForge.Core.Interfaces;
using ThemeForge.Core.Services.Theme;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ThemeForge.Core.Services.Storage
{
    /// <summary>
    /// JSON-репозиторий тем поверх <see cref="IThemeFileStorage"/>.
    /// </summary>
    public sealed class JsonThemeRepository : IThemeRepository
    {
        private readonly IThemeFileStorage storage;
        private readonly bool seedBuiltInThemes;
        private readonly SemaphoreSlim mutex = new(1, 1);
        private readonly JsonSerializerOptions serializerOptions;

        private List<ThemeDefinition> themes = [];
        private bool loaded;

        /// <inheritdoc />
        public event EventHandler? Changed;

        /// <summary>
        /// Создает JSON-репозиторий.
        /// </summary>
        public JsonThemeRepository(IThemeFileStorage storage, bool seedBuiltInThemes = true)
        {
            this.storage = storage;
            this.seedBuiltInThemes = seedBuiltInThemes;
            this.serializerOptions = CreateSerializerOptions();
        }

        /// <inheritdoc />
        public async ValueTask<IReadOnlyList<ThemeDefinition>> LoadAsync(CancellationToken cancellationToken = default)
        {
            await mutex.WaitAsync(cancellationToken);

            try
            {
                await EnsureLoadedAsync(cancellationToken);

                return themes.ToArray();
            }
            finally
            {
                mutex.Release();
            }
        }

        /// <inheritdoc />
        public async ValueTask SaveAsync(ThemeDefinition theme, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(theme);

            bool changed = false;

            await mutex.WaitAsync(cancellationToken);

            try
            {
                await EnsureLoadedAsync(cancellationToken);
                Upsert(theme);
                await PersistAsync(cancellationToken);

                changed = true;
            }
            finally
            {
                mutex.Release();
            }

            if (changed) Changed?.Invoke(this, EventArgs.Empty);
        }

        /// <inheritdoc />
        public async ValueTask DeleteAsync(Guid id, CancellationToken cancellationToken = default)
        {
            bool changed = false;

            await mutex.WaitAsync(cancellationToken);

            try
            {
                await EnsureLoadedAsync(cancellationToken);

                int removed = themes.RemoveAll(t => t.Id == id);

                if (removed > 0)
                {
                    await PersistAsync(cancellationToken);
                    changed = true;
                }
            }
            finally
            {
                mutex.Release();
            }

            if (changed) Changed?.Invoke(this, EventArgs.Empty);
        }

        /// <inheritdoc />
        public async ValueTask<ThemeDefinition?> FindAsync(Guid id, CancellationToken cancellationToken = default)
        {
            await mutex.WaitAsync(cancellationToken);

            try
            {
                await EnsureLoadedAsync(cancellationToken);

                return themes.FirstOrDefault(t => t.Id == id);
            }
            finally
            {
                mutex.Release();
            }
        }

        private async Task EnsureLoadedAsync(CancellationToken cancellationToken)
        {
            if (loaded) return;

            string? json = await storage.ReadAllAsync(cancellationToken);

            if (string.IsNullOrWhiteSpace(json))
            {
                if (seedBuiltInThemes)
                {
                    themes = BuiltInThemes.CreateDefaultThemes().ToList();

                    await PersistCoreAsync(cancellationToken);
                }

                loaded = true;

                return;
            }

            ThemeStorageDocument? document = null;

            try
            {
                document = JsonSerializer.Deserialize<ThemeStorageDocument>(json, serializerOptions);
            }
            catch (JsonException)
            {
                document = null;
            }

            themes = document?.Themes?.ToList() ?? [];
        
            if (themes.Count == 0 && seedBuiltInThemes)
            {
                themes = BuiltInThemes.CreateDefaultThemes().ToList();

                await PersistCoreAsync(cancellationToken);
            }

            loaded = true;
        }

        private Task PersistAsync(CancellationToken cancellationToken) => PersistCoreAsync(cancellationToken);

        private async Task PersistCoreAsync(CancellationToken cancellationToken)
        {
            ThemeStorageDocument document = new () { Themes = themes };
            string json = JsonSerializer.Serialize(document, serializerOptions);

            await storage.WriteAllAsync(json, cancellationToken);
        }

        private void Upsert(ThemeDefinition theme)
        {
            if (theme.Id == Guid.Empty)
            {
                theme = theme with { Id = Guid.NewGuid() };
            }

            ThemeDefinition updated = theme with { UpdatedUtc = DateTimeOffset.UtcNow };

            int index = themes.FindIndex(t => t.Id == updated.Id);

            if (index >= 0)
            {
                themes[index] = updated;
            }
            else
            {
                themes.Add(updated);
            }
        }

        private static JsonSerializerOptions CreateSerializerOptions()
        {
            return new JsonSerializerOptions(JsonSerializerDefaults.Web)
            {
                WriteIndented = true,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                Converters =
                {
                    new JsonStringEnumConverter(JsonNamingPolicy.CamelCase)
                }
            };
        }
    }
}
