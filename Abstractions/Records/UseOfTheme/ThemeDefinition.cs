using Abstractions.Enums;
using Abstractions.Records.UseOfEffects;
using Abstractions.Records.UseOfGeometry;
using Abstractions.Records.UseOfTypograhy;
using Abstractions.Helpers;

namespace Abstractions.Records.UseOfTheme
{
    /// <summary>
    /// Полное определение темы.
    /// </summary>
    /// <param name="Name">Отображаемое имя темы.</param>
    /// <param name="Kind">Вид темы.</param>
    public sealed record ThemeDefinition(string Name, ThemeKind Kind)
    {
        /// <summary>
        /// Уникальный идентификатор темы.
        /// </summary>
        public Guid Id { get; init; } = Guid.NewGuid();

        /// <summary>
        /// Версия схемы для миграции.
        /// </summary>
        public int SchemaVersion { get; init; } = 1;

        /// <summary>
        /// Простая цветовая тема, если Kind == <see cref="ThemeKind.Solid"/>.
        /// </summary>
        public SolidTheme? Solid { get; init; }

        /// <summary>
        /// Градиентная тема, если Kind == <see cref="ThemeKind.LinearGradient"/> или <see cref="ThemeKind.RadialGradient"/>.
        /// </summary>
        public GradientTheme? Gradient { get; init; }

        /// <summary>
        /// Глобальные настройки типографики.
        /// </summary>
        public TypographySettings? GlobalTypography { get; init; }

        /// <summary>
        /// Глобальные геометрические настройки.
        /// </summary>
        public GeometrySettings? GlobalGeometry { get; init; }

        /// <summary>
        /// Глобальный фоновый эффект.
        /// </summary>
        public EffectSettings? GlobalEffect { get; init; }

        /// <summary>
        /// Компонентные темы. Ключ формируется через <see cref="ThemeComponentKey"/>.
        /// </summary>
        public IReadOnlyDictionary<string, ComponentTheme> Components { get; init; } = new Dictionary<string, ComponentTheme>();

        /// <summary>
        /// Произвольные метаданные.
        /// </summary>
        public IReadOnlyDictionary<string, string> Metadata { get; init; } = new Dictionary<string, string>();

        /// <summary>
        /// Теги темы.
        /// </summary>
        public IReadOnlyList<string> Tags { get; init; } = new List<string>();

        /// <summary>
        /// Дата создания в UTC.
        /// </summary>
        public DateTimeOffset CreatedUtc { get; init; } = DateTimeOffset.UtcNow;

        /// <summary>
        /// Дата последнего изменения в UTC.
        /// </summary>
        public DateTimeOffset UpdatedUtc { get; init; } = DateTimeOffset.UtcNow;

        /// <summary>
        /// Встроенная ли тема. Встроенные обычно нельзя удалить, но можно дублировать.
        /// </summary>
        public bool IsBuiltIn { get; init; }

        /// <summary>
        /// Отмечена ли тема как избранная/пользовательский пресет.
        /// </summary>
        public bool IsFavorite { get; init; }
    }
}
