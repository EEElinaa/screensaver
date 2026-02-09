namespace screensaver
{
    /// <summary>
    /// Класс представляющий снежинку
    /// </summary>
    public class Snowflake
    {
        /// <summary>
        /// Координата X снежинки
        /// </summary>
        public float X { get; set; }

        /// <summary>
        /// Координата Y снежинки
        /// </summary>
        public float Y { get; set; }

        /// <summary>
        /// Скорость падения снежинки
        /// </summary>
        public float Speed { get; set; }

        /// <summary>
        /// Размер снежинки (относительная величина)
        /// </summary>
        public float Size { get; set; }

        /// <summary>
        /// Горизонтальное смещение (эффект ветра)
        /// </summary>
        public float Wind { get; set; }
        public int HorizontalFluctuation { get; internal set; }
    }
}
