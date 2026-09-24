using ThemeForge.Core.Interfaces;

namespace ThemeForge.Core.Services.Storage
{
    /// <summary>
    /// In-memory хранилище для тестов и временной работы без диска.
    /// </summary>
    public sealed class InMemoryThemeFileStorage : IThemeFileStorage
    {
        private string? contents;

        /// <inheritdoc />
        public ValueTask<string?> ReadAllAsync(CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            return ValueTask.FromResult(contents);
        }

        /// <inheritdoc />
        public ValueTask WriteAllAsync(string contents, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            this.contents = contents;

            return ValueTask.CompletedTask;
        }
    }
}
