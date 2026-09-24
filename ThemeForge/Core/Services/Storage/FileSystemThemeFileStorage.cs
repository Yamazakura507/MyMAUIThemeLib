using ThemeForge.Core.Interfaces;
using System.Text;

namespace ThemeForge.Core.Services.Storage
{
    /// <summary>
    /// Файловое хранилище тем.
    /// </summary>
    public sealed class FileSystemThemeFileStorage : IThemeFileStorage
    {
        private readonly string path;
        private readonly SemaphoreSlim mutex = new(1, 1);

        /// <summary>
        /// Создает файловое хранилище.
        /// </summary>
        /// <param name="path">Полный путь к JSON-файлу.</param>
        public FileSystemThemeFileStorage(string path)
        {
            this.path = Path.GetFullPath(path);
        }

        /// <inheritdoc />
        public async ValueTask<string?> ReadAllAsync(CancellationToken cancellationToken = default)
        {
            await mutex.WaitAsync(cancellationToken);

            try
            {
                if (!File.Exists(path)) return null;

                return await File.ReadAllTextAsync(path, cancellationToken);
            }
            finally
            {
                mutex.Release();
            }
        }

        /// <inheritdoc />
        public async ValueTask WriteAllAsync(string contents, CancellationToken cancellationToken = default)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(contents);

            await mutex.WaitAsync(cancellationToken);

            try
            {
                string? directory = Path.GetDirectoryName(path);

                if (!string.IsNullOrWhiteSpace(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                string tempPath = $"{path}.{Guid.NewGuid():N}.tmp";

                await File.WriteAllTextAsync(
                    tempPath, contents,
                    new UTF8Encoding(encoderShouldEmitUTF8Identifier: false),
                    cancellationToken);

                File.Move(tempPath, path, overwrite: true);
            }
            finally
            {
                mutex.Release();
            }
        }
    }
}
