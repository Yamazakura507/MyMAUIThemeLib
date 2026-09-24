using ThemeForge.Abstractions.EventArgs;
using ThemeForge.Abstractions.Interfaces;
using ThemeForge.Abstractions.Records.UseOfTheme;

namespace ThemeForge.Core.Services.Theme
{
    /// <summary>
    /// Центральный сервис управления текущей темой, черновиком и пресетами.
    /// </summary>
    public sealed class ThemeService : IThemeService
    {
        private readonly IThemeRepository repository;
        private readonly SemaphoreSlim mutex = new(1, 1);

        private ThemeDefinition current;
        private ThemeDefinition draft;
        private List<ThemeDefinition> presets = [];
        private bool loaded;
        private bool initializedFromRepository;
        private bool hasUnsavedChanges;

        /// <inheritdoc />
        public event EventHandler<ThemeChangedEventArgs>? Applied;

        /// <inheritdoc />
        public event EventHandler<ThemeChangedEventArgs>? DraftChanged;

        /// <summary>
        /// Создает сервис тем.
        /// </summary>
        public ThemeService(IThemeRepository repository)
        {
            this.repository = repository;
            current = BuiltInThemes.CreateFallbackTheme();
            draft = current;
        }

        /// <inheritdoc />
        public ThemeDefinition Current => current;

        /// <inheritdoc />
        public ThemeDefinition Draft => draft;

        /// <inheritdoc />
        public bool HasUnsavedChanges => hasUnsavedChanges;

        /// <inheritdoc />
        public async ValueTask<IReadOnlyList<ThemeDefinition>> GetPresetsAsync(CancellationToken cancellationToken = default)
        {
            await mutex.WaitAsync(cancellationToken);

            try
            {
                await EnsureLoadedAsync(cancellationToken);

                return presets.ToArray();
            }
            finally
            {
                mutex.Release();
            }
        }

        /// <inheritdoc />
        public async ValueTask SelectPresetAsync(Guid id, CancellationToken cancellationToken = default)
        {
            ThemeDefinition? selected = null;
            bool changed = false;

            await mutex.WaitAsync(cancellationToken);

            try
            {
                await EnsureLoadedAsync(cancellationToken);

                selected = presets.FirstOrDefault(p => p.Id == id);

                if (selected is null) throw new InvalidOperationException($"Тема с id '{id}' не найдена.");

                draft = selected with { };
                hasUnsavedChanges = selected.Id != current.Id;
                changed = true;
            }
            finally
            {
                mutex.Release();
            }

            if (changed && selected is not null)
            {
                DraftChanged?.Invoke(this, new ThemeChangedEventArgs(selected, isApplied: false));
            }
        }

        /// <inheritdoc />
        public async ValueTask ApplyAsync(CancellationToken cancellationToken = default)
        {
            ThemeDefinition applied;
            bool changed = false;

            await mutex.WaitAsync(cancellationToken);

            try
            {
                applied = draft with { UpdatedUtc = DateTimeOffset.UtcNow };

                current = applied;
                hasUnsavedChanges = false;
                changed = true;
            }
            finally
            {
                mutex.Release();
            }

            if (changed)
            {
                Applied?.Invoke(this, new ThemeChangedEventArgs(applied, isApplied: true));
            }
        }

        /// <inheritdoc />
        public async ValueTask SaveDraftAsCustomAsync(string? name = null, CancellationToken cancellationToken = default)
        {
            ThemeDefinition saved;
            bool changed = false;

            await mutex.WaitAsync(cancellationToken);

            try
            {
                await EnsureLoadedAsync(cancellationToken);

                string finalName = string.IsNullOrWhiteSpace(name) ? NextCustomName() : name;

                DateTimeOffset now = DateTimeOffset.UtcNow;

                saved = draft with
                {
                    Id = Guid.NewGuid(),
                    Name = finalName,
                    IsBuiltIn = false,
                    IsFavorite = true,
                    CreatedUtc = now,
                    UpdatedUtc = now
                };

                await repository.SaveAsync(saved, cancellationToken);

                presets.RemoveAll(p => p.Id == saved.Id);
                presets.Insert(0, saved);

                draft = saved;
                hasUnsavedChanges = saved.Id != current.Id;
                changed = true;
            }
            finally
            {
                mutex.Release();
            }

            if (changed)
            {
                DraftChanged?.Invoke(this, new ThemeChangedEventArgs(saved, isApplied: false));
            }
        }

        /// <inheritdoc />
        public async ValueTask UpdateDraftAsync(ThemeDefinition draft, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(draft);

            ThemeDefinition updated;
            bool changed = false;

            await mutex.WaitAsync(cancellationToken);

            try
            {
                updated = draft with { UpdatedUtc = DateTimeOffset.UtcNow };

                this.draft = updated;
                hasUnsavedChanges = true;
                changed = true;
            }
            finally
            {
                mutex.Release();
            }

            if (changed)
            {
                DraftChanged?.Invoke(this, new ThemeChangedEventArgs(updated, isApplied: false));
            }
        }

        /// <inheritdoc />
        public async ValueTask ResetDraftAsync(CancellationToken cancellationToken = default)
        {
            ThemeDefinition reset;
            bool changed = false;

            await mutex.WaitAsync(cancellationToken);

            try
            {
                reset = current with { };
                draft = reset;
                hasUnsavedChanges = false;
                changed = true;
            }
            finally
            {
                mutex.Release();
            }

            if (changed)
            {
                DraftChanged?.Invoke(this, new ThemeChangedEventArgs(reset, isApplied: false));
            }
        }

        private async Task EnsureLoadedAsync(CancellationToken cancellationToken)
        {
            if (loaded) return;

            IReadOnlyList<ThemeDefinition> presets = await repository.LoadAsync(cancellationToken);
            this.presets = presets.ToList();

            if (!initializedFromRepository && this.presets.Count > 0)
            {
                ThemeDefinition initial = this.presets.FirstOrDefault(p => p.IsFavorite) ?? this.presets[0];

                current = initial with { };
                draft = current;
                hasUnsavedChanges = false;
                initializedFromRepository = true;
            }

            loaded = true;
        }

        private string NextCustomName()
        {
            string startCustomName = "Custom#";
            int max = 0;

            foreach (ThemeDefinition preset in presets)
            {
                if (!preset.Name.StartsWith(startCustomName, StringComparison.OrdinalIgnoreCase)) continue;

                string suffix = preset.Name[startCustomName.Length..];

                if (int.TryParse(suffix, out int index) && index > max) max = index;
            }

            return $"Custom#{max + 1}";
        }
    }
}
