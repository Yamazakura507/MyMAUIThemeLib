namespace ThemeForge.Maui.Controls.ComponentModels.Trees
{
    /// <summary>
    /// Превращает дерево узлов в плоский список видимых узлов для CollectionView.
    /// </summary>
    public static class ThemeTreeFlattener
    {
        /// <summary>
        /// Возвращает плоский список видимых узлов.
        /// </summary>
        public static IReadOnlyList<ThemeTreeNodeViewModel> Flatten(IEnumerable<ThemeTreeNodeViewModel> roots)
        {
            ArgumentNullException.ThrowIfNull(roots);

            List<ThemeTreeNodeViewModel> result = new ();
            Stack<ThemeTreeNodeViewModel> stack = new ();

            foreach (var root in roots.Reverse())
            {
                stack.Push(root);
            }

            while (stack.Count > 0)
            {
                ThemeTreeNodeViewModel node = stack.Pop();
                result.Add(node);

                if (!node.IsExpanded || node.Children.Count == 0) continue;

                foreach (ThemeTreeNodeViewModel child in node.Children.Reverse())
                {
                    stack.Push(child);
                }
            }

            return result;
        }
    }
}
