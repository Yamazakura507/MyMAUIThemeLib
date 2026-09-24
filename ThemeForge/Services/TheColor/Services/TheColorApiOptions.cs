namespace ThemeForge.Services.TheColor.Services
{
    /// <summary>
    /// Настройки подключения к TheColor API.
    /// </summary>
    public sealed class TheColorApiOptions
    {
        /// <summary>
        /// Включать ли сетевые запросы к API.
        /// Если <c>false</c>, сервисы работают только локально.
        /// </summary>
        public bool EnableApi { get; set; } = true;

        /// <summary>
        /// Базовый адрес TheColor API.
        /// По умолчанию используется публичный endpoint.
        /// </summary>
        public string BaseUrl { get; set; } = "https://www.thecolorapi.com/id";

        /// <summary>
        /// Таймаут одного HTTP-запроса в секундах.
        /// </summary>
        public int TimeoutSeconds { get; set; } = 5;

        /// <summary>
        /// Количество повторных попыток при сетевых ошибках/таймаутах.
        /// </summary>
        public int RetryCount { get; set; } = 1;

        /// <summary>
        /// Максимальное количество параллельных запросов при пакетном получении имен.
        /// </summary>
        public int MaxParallelRequests { get; set; } = 4;

        /// <summary>
        /// Время хранения ответа API в памяти.
        /// </summary>
        public TimeSpan CacheDuration { get; set; } = TimeSpan.FromDays(7);

        /// <summary>
        /// Дополнительные HTTP-заголовки, например Authorization, если потребуется.
        /// </summary>
        public Dictionary<string, string> DefaultHeaders { get; set; } = new(StringComparer.OrdinalIgnoreCase);
    }
}
