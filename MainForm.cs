namespace screensaver
{
    public partial class MainForm : Form
    {
        private const int SNOWFLAKE_COUNT = 150;
        private const int MIN_SPEED = 2, MAX_SPEED = 8;
        private const float MIN_SIZE = 0.4f, MAX_SIZE = 2.5f;
        private const float SIZE_MULTIPLIER = 20f;      // Коэффициент увеличения при отрисовке
        private const float WIND_FACTOR = 2f;           // Сила ветра (горизонтальное движение)
        private const int RESPAWN_MARGIN = 50;          // Насколько ниже экрана снежинка исчезает
        private const int SPAWN_OFFSET_MIN = 20, SPAWN_OFFSET_MAX = 100; // Диапазон высоты появления
        private const int HORIZONTAL_MARGIN = 50;       // Запас по бокам для плавного входа/выхода
        private const int TIMER_INTERVAL = 25;

        private System.Windows.Forms.Timer timer;
        private List<Snowflake> snowflakes = new List<Snowflake>(); // Коллекция снежинок
        private Random random = new Random();
        private Bitmap backgroundImage;                 // Предварительно растянутый фон деревни
        private Image snowflakeImg;                     // Изображение снежинки png

        //класс снединок
        private class Snowflake
        {
            public float X, Y;      
            public float Speed;
            public float Size;
            public float Wind;      // Горизонтальное смещение (эффект ветра)
        }


        public MainForm()
        {
            InitializeComponent();
            SetupForm();             // Настройка внешнего вида формы
            LoadResources();         // Загрузка графических ресурсов
            SetupTimer();            // Создание и настройка таймера анимации
        }

        // Найстройкка внешнего вида фоомы
        private void SetupForm()
        {
            // полноэкранный без рамок
            FormBorderStyle = FormBorderStyle.None;
            WindowState = FormWindowState.Maximized;
            BackColor = Color.Black;                  // Черный фон на случай ошибки загрузки
            KeyPreview = true;


            Load += (s, e) =>                         // Когда форма полностью загрузилась
            {
                CreateSnowflakes();                    // Создаем начальный набор снежинок
                timer.Start();                         // Запускаем анимацию сразу
            };

            Paint += MainForm_Paint;                   // Событие перерисовки формы
            KeyDown += (s, e) => Close();              // Закрытие по любой клавише
        }

        //Графическе ресурсы
        private void LoadResources()
        {

            var screenBounds = Screen.PrimaryScreen.Bounds;
            backgroundImage = new Bitmap(screenBounds.Width, screenBounds.Height);
            using (var graphics = Graphics.FromImage(backgroundImage))
            {
                if (Properties.Resources.Village != null)
                {
                    //изображение деревни на весь экран
                    graphics.DrawImage(Properties.Resources.Village,
                                       0, 0, screenBounds.Width, screenBounds.Height);
                }
                else
                {
                    // Резервный фон, если основное изображение не найдено
                    graphics.Clear(Color.DarkBlue);                // Ночное небо
                    graphics.FillRectangle(Brushes.White,          // Снег
                        0, screenBounds.Height - 100,
                        screenBounds.Width, 100);
                }
            }

            // Загружка изображения снежинки 
            snowflakeImg = Properties.Resources.snowflake;
        }

        // Создание набора снежинок
        private void CreateSnowflakes()
        {
            snowflakes.Clear();

            int screenWidth = Screen.PrimaryScreen.Bounds.Width;
            int screenHeight = Screen.PrimaryScreen.Bounds.Height;

            for (int i = 0; i < SNOWFLAKE_COUNT; i++)
            {
                // случайный размер снежинки
                float size = MIN_SIZE + (float)random.NextDouble() * (MAX_SIZE - MIN_SIZE);

                // Крупные снежинки падают быстрее
                float speed = MIN_SPEED + (size / MAX_SIZE) * (MAX_SPEED - MIN_SPEED);

                snowflakes.Add(new Snowflake
                {
                    // Начальная позиция с запасом за границами экрана
                    X = random.Next(-HORIZONTAL_MARGIN, screenWidth + HORIZONTAL_MARGIN),
                    Y = random.Next(-screenHeight * 2, 0),

                    // Скорость зависит от размера
                    Speed = speed,
                    Size = size,

                    // Случайный ветер
                    Wind = ((float)random.NextDouble() - 0.5f) * WIND_FACTOR
                });
            }
        }

        //настройка таймера анимаци
        private void SetupTimer()
        {
            timer = new System.Windows.Forms.Timer() { Interval = TIMER_INTERVAL };
            timer.Tick += (s, e) =>
            {
                UpdateSnowflakes(); // Обновление позиции снежинок
                using (var graphics = CreateGraphics())
                    MainForm_Paint(this, new PaintEventArgs(graphics, ClientRectangle));
            };
        }

        // Обновление позиции всех снежинок
        private void UpdateSnowflakes()
        {
            int screenWidth = Screen.PrimaryScreen.Bounds.Width;
            int screenHeight = Screen.PrimaryScreen.Bounds.Height;

            foreach (var flake in snowflakes)
            {
                // Движение вниз
                flake.Y += flake.Speed;

                // Движение вбок= ветер + небольшие случайные колебания
                flake.X += flake.Wind + ((float)random.NextDouble() - 0.5f) * 0.3f;

                // Если снежинка упала ниже видимой области
                if (flake.Y > screenHeight + RESPAWN_MARGIN)
                {
                    // снежинка в новой позиции над экраном
                    flake.X = random.Next(-HORIZONTAL_MARGIN, screenWidth + HORIZONTAL_MARGIN);
                    flake.Y = -random.Next(SPAWN_OFFSET_MIN, SPAWN_OFFSET_MAX);


                    flake.Speed = MIN_SPEED + (flake.Size / MAX_SIZE) * (MAX_SPEED - MIN_SPEED);
                }

                // телепортация по горизонтали для непрерывного движения
                if (flake.X > screenWidth + HORIZONTAL_MARGIN)
                {
                    flake.X = -HORIZONTAL_MARGIN; // Вышла справа - появляется слева
                }
                else if (flake.X < -HORIZONTAL_MARGIN)
                {
                    flake.X = screenWidth + HORIZONTAL_MARGIN; // Вышла слева - появляется справа
                }
            }
        }

        // Отрисовка формы
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
                // фон ДЕРЕВНИ
                graphics.DrawImage(backgroundImage, ClientRectangle);

                //СНЕЖИНКИ
                foreach (var flake in snowflakes)
                {
                    // размер для отрисовки
                    float drawSize = flake.Size * SIZE_MULTIPLIER;

                    if (snowflakeImg != null)
                    {

                        graphics.DrawImage(snowflakeImg, flake.X, flake.Y, drawSize, drawSize);
                    }
                    else
                    {
                        //белый круг если изображение не загружено)
                        graphics.FillEllipse(Brushes.White, flake.X, flake.Y, drawSize, drawSize);
                    }
                }
                e.Graphics.DrawImage(buffer, Point.Empty);
            }
        }

        // очистка ресурсов при закрытии 
        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            timer?.Stop();
            timer?.Dispose();
            backgroundImage?.Dispose();
            snowflakeImg?.Dispose();
            base.OnFormClosing(e);
        }
    }
}
