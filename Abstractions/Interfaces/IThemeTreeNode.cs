using Abstractions.Enums;

namespace Abstractions.Interfaces
{
    /// <summary>
    /// Универсальный узел многоуровневого дерева.
    /// Используется для готовых тем, кастомных настроек, категорий и действий.
    /// </summary>
    public interface IThemeTreeNode
    {
        /// <summary>
        /// Заголовок узла.
        /// </summary>
        string Title { get; }

        /// <summary>
        /// Дополнительный текст/описание/подзаголовок. Может быть пустым.
        /// </summary>
        string? Detail { get; }

        /// <summary>
        /// Иконка. Может быть не указана.
        /// </summary>
        string? Icon { get; }

        /// <summary>
        /// Тип узла.
        /// </summary>
        TreeNodeKind Kind { get; }

        /// <summary>
        /// Раскрыт ли узел.
        /// </summary>
        bool IsExpanded { get; set; }

        /// <summary>
        /// Глубина узла. Устанавливается flattener-ом для виртуализированного списка.
        /// </summary>
        int Depth { get; set; }

        /// <summary>
        /// Полезная нагрузка: тема, настройка, descriptor, команда и т.д.
        /// </summary>
        object? Payload { get; }

        /// <summary>
        /// Дочерние узлы.
        /// </summary>
        IReadOnlyList<IThemeTreeNode> Children { get; }
    }
}
