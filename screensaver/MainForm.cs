using System.Drawing;
using System.Windows.Forms;

namespace screensaver
{
    public partial class MainForm : Form
    {
        private const int CountSnowflakes = 150;
        private const int SnowflakeMinSpeed = 2;
        private const int SnowflakeMaxSpeed = 8;
        private const float SnowflakeMinSize = 0.4f;
        private const float SnowflakeMaxSize = 2.5f;
        private const float SnowflakeSizeMultiplier = 20f;      // Коэффициент увеличения при отрисовке
        private const float WindStrength = 2f;                  // Сила ветра (горизонтальное движение)
        private const int RespawnMarginBelowScreen = 50;        // Насколько ниже экрана снежинка исчезает
        private const int SpawnHeightMin = 20;                  // Минимальная высота появления
        private const int SpawnHeightMax = 100;                 // Максимальная высота появления
        private const int HorizontalSpawnMargin = 50;           // Запас по бокам для плавного входа/выхода
        private const int AnimationTimerInterval = 25;          // Интервал таймера в миллисекундах
        private const float WindGenerationCenter = 0.5f;        // Центр для генерации случайного ветра
        private const float SnowflakeHorizontalFluctuation = 0.3f; // Фактор боковых колебаний снежинок
        private const int SnowLayerHeight = 100;                // Высота снежного покрова в пикселях

        private System.Windows.Forms.Timer animationTimer;
        private List<Snowflake> snowflakes = new List<Snowflake>(); // Коллекция снежинок
        private Random random = new Random();
        private Bitmap backgroundImage;                         // Предварительно растянутый фон деревни
        private Image snowflakeImage;                           // Изображение снежинки png

        public MainForm()
        {
            InitializeComponent();
            SetupForm();             // Настройка внешнего вида формы
            LoadResources();         // Загрузка графических ресурсов
            SetupTimer();            // Создание и настройка таймера анимации
        }

        /// <summary>
        /// Настройка внешнего вида формы
        /// </summary>
        private void SetupForm()
        {
            // Полноэкранный режим без рамок
            FormBorderStyle = FormBorderStyle.None;
            WindowState = FormWindowState.Maximized;
            BackColor = Color.Black;                  // Черный фон на случай ошибки загрузки
            KeyPreview = true;

            Load += (s, e) =>                         // Когда форма полностью загрузилась
            {
                CreateSnowflakes();                    // Создаем начальный набор снежинок
                animationTimer.Start();                // Запускаем анимацию сразу
            };

            Paint += MainForm_Paint;                   // Событие перерисовки формы
            KeyDown += (s, e) => Close();              // Закрытие по любой клавише
        }

        /// <summary>
        /// Загрузка графических ресурсов
        /// </summary>
        private void LoadResources()
        {
            var screenBounds = Screen.PrimaryScreen.Bounds;
            backgroundImage = new Bitmap(screenBounds.Width, screenBounds.Height);

            using (var graphics = Graphics.FromImage(backgroundImage))
            {
                if (Properties.Resources.Village != null)
                {
                    // Изображение деревни на весь экран
                    graphics.DrawImage(Properties.Resources.Village,
                                       0, 0, screenBounds.Width, screenBounds.Height);
                }
                else
                {
                    // Резервный фон, если основное изображение не найдено
                    graphics.Clear(Color.DarkBlue);                // Ночное небо
                    graphics.FillRectangle(Brushes.White,          // Снег
                        0, screenBounds.Height - SnowLayerHeight,
                        screenBounds.Width, SnowLayerHeight);
                }
            }

            // Загрузка изображения снежинки 
            snowflakeImage = Properties.Resources.snowflake;
        }

        /// <summary>
        /// Создание начального набора снежинок
        /// </summary>
        private void CreateSnowflakes()
        {
            snowflakes.Clear();

            int screenWidth = Screen.PrimaryScreen.Bounds.Width;
            int screenHeight = Screen.PrimaryScreen.Bounds.Height;

            for (int i = 0; i < CountSnowflakes; i++)
            {
                // Случайный размер снежинки в заданном диапазоне
                float size = SnowflakeMinSize + (float)random.NextDouble() * (SnowflakeMaxSize - SnowflakeMinSize);

                // Крупные снежинки падают быстрее (скорость зависит от размера)
                float speed = SnowflakeMinSpeed + (size / SnowflakeMaxSize) * (SnowflakeMaxSpeed - SnowflakeMinSpeed);

                snowflakes.Add(new Snowflake
                {
                    // Начальная позиция с запасом за границами экрана
                    X = random.Next(-HorizontalSpawnMargin, screenWidth + HorizontalSpawnMargin),
                    Y = random.Next(-screenHeight * 2, 0),

                    // Скорость зависит от размера
                    Speed = speed,
                    Size = size,

                    // Случайный ветер (от -WindStrength/2 до +WindStrength/2)
                    Wind = ((float)random.NextDouble() - WindGenerationCenter) * WindStrength
                });
            }
        }

        /// <summary>
        /// Настройка таймера анимации
        /// </summary>
        private void SetupTimer()
        {
            animationTimer = new System.Windows.Forms.Timer() { Interval = AnimationTimerInterval };
            animationTimer.Tick += (s, e) =>
            {
                UpdateSnowflakes(); // Обновление позиции снежинок
                using var graphics = CreateGraphics();
                MainForm_Paint(this, new PaintEventArgs(graphics, ClientRectangle));
            };
        }

        /// <summary>
        /// Обновление позиции всех снежинок
        /// </summary>
        private void UpdateSnowflakes()
        {
            int screenWidth = Screen.PrimaryScreen.Bounds.Width;
            int screenHeight = Screen.PrimaryScreen.Bounds.Height;

            foreach (var flake in snowflakes)
            {
                // Движение вниз
                flake.Y += flake.Speed;

                // Движение вбок: ветер + небольшие случайные колебания
                flake.X += flake.Wind + ((float)random.NextDouble() - WindGenerationCenter) * SnowflakeHorizontalFluctuation;

                // Если снежинка упала ниже видимой области
                if (flake.Y > screenHeight + RespawnMarginBelowScreen)
                {
                    // Снежинка появляется в новой позиции над экраном
                    flake.X = random.Next(-HorizontalSpawnMargin, screenWidth + HorizontalSpawnMargin);
                    flake.Y = -random.Next(SpawnHeightMin, SpawnHeightMax);

                    // Обновляем скорость в соответствии с размером
                    flake.Speed = SnowflakeMinSpeed + (flake.Size / SnowflakeMaxSize) * (SnowflakeMaxSpeed - SnowflakeMinSpeed);
                }

                // Телепортация по горизонтали для непрерывного движения
                if (flake.X > screenWidth + HorizontalSpawnMargin)
                {
                    flake.X = -HorizontalSpawnMargin; // Вышла справа - появляется слева
                }
                else if (flake.X < -HorizontalSpawnMargin)
                {
                    flake.X = screenWidth + HorizontalSpawnMargin; // Вышла слева - появляется справа
                }
            }
        }

        /// <summary>
        /// Отрисовка формы (двойная буферизация)
        /// </summary>
        private void MainForm_Paint(object sender, PaintEventArgs e)
        {
            // Если фон не загружен, ничего не рисуем

            if (backgroundImage == null)
            {
                return;
            }

            using (var buffer = new Bitmap(ClientRectangle.Width, ClientRectangle.Height))
            using (var graphics = Graphics.FromImage(buffer))
            {
                // Фон деревни
                graphics.DrawImage(backgroundImage, ClientRectangle);

                // Снежинки
                foreach (var flake in snowflakes)
                {
                    // Размер для отрисовки с учетом множителя
                    float drawSize = flake.Size * SnowflakeSizeMultiplier;

                    if (snowflakeImage != null)
                    {
                        graphics.DrawImage(snowflakeImage, flake.X, flake.Y, drawSize, drawSize);
                    }
                    else
                    {
                        // Белый круг если изображение не загружено
                        graphics.FillEllipse(Brushes.White, flake.X, flake.Y, drawSize, drawSize);
                    }
                }
                e.Graphics.DrawImage(buffer, Point.Empty);
            }
        }

        /// <summary>
        /// Очистка ресурсов при закрытии формы
        /// </summary>
        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            animationTimer?.Stop();
            animationTimer?.Dispose();
            backgroundImage?.Dispose();
            snowflakeImage?.Dispose();
            base.OnFormClosing(e);
        }
    }
}
