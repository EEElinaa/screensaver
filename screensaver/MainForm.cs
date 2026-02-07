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
        private const float SnowflakeSizeMultiplier = 20f;
        private const float WindStrength = 2f;
        private const int RespawnMarginBelowScreen = 50;
        private const int SpawnHeightMin = 20;
        private const int SpawnHeightMax = 100;
        private const int HorizontalSpawnMargin = 50;
        private const int AnimationTimerInterval = 25;
        private const float WindGenerationCenter = 0.5f;
        private const float SnowflakeHorizontalFluctuation = 0.3f;
        private const int SnowLayerHeight = 100;
        private const int DoubleScreenHeightMultiplier = 2;
        private const int TopScreenPosition = 0;

        private System.Windows.Forms.Timer animationTimer;
        private readonly List<Snowflake> snowflakes = [];
        private readonly Random random = new();
        private Bitmap backgroundImage;
        private Image snowflakeImage;

        public MainForm()
        {
            InitializeComponent();
        }

        private void LoadResources()
        {
            var screenBounds = Screen.PrimaryScreen.Bounds;
            backgroundImage = new Bitmap(screenBounds.Width, screenBounds.Height);

            using (var graphics = Graphics.FromImage(backgroundImage))
            {
                if (Properties.Resources.Village != null)
                {
                    graphics.DrawImage(Properties.Resources.Village,
                                       0, 0, screenBounds.Width, screenBounds.Height);
                }
                else
                {
                    graphics.Clear(Color.DarkBlue);
                    graphics.FillRectangle(Brushes.White,
                        0,
                        screenBounds.Height - SnowLayerHeight,
                        screenBounds.Width,
                        SnowLayerHeight);
                }
            }

            snowflakeImage = Properties.Resources.snowflake;
        }

        private void CreateSnowflakes()
        {
            int screenWidth = Screen.PrimaryScreen.Bounds.Width;
            int screenHeight = Screen.PrimaryScreen.Bounds.Height;

            for (var index = 0; index < CountSnowflakes; index++)
            {
                var size = SnowflakeMinSize + (float)random.NextDouble() * (SnowflakeMaxSize - SnowflakeMinSize);
                var speed = SnowflakeMinSpeed + (size / SnowflakeMaxSize) * (SnowflakeMaxSpeed - SnowflakeMinSpeed);

                snowflakes.Add(new Snowflake
                {
                    X = random.Next(-HorizontalSpawnMargin, screenWidth + HorizontalSpawnMargin),
                    Y = random.Next(-screenHeight * DoubleScreenHeightMultiplier, TopScreenPosition),
                    Speed = speed,
                    Size = size,
                    Wind = ((float)random.NextDouble() - WindGenerationCenter) * WindStrength
                });
            }
        }

        private void SetupTimer()
        {
            animationTimer = new System.Windows.Forms.Timer() { Interval = AnimationTimerInterval };
            animationTimer.Tick += (_, _) =>
            {
                UpdateSnowflakes();
                using (var graphics = CreateGraphics())
                {
                    MainForm_Paint(this, new PaintEventArgs(graphics, ClientRectangle));
                }
            };
        }

        private void UpdateSnowflakes()
        {
            int screenWidth = Screen.PrimaryScreen.Bounds.Width;
            int screenHeight = Screen.PrimaryScreen.Bounds.Height;

            foreach (var flake in snowflakes)
            {
                flake.Y += flake.Speed;
                flake.X += flake.Wind + ((float)random.NextDouble() - WindGenerationCenter) * SnowflakeHorizontalFluctuation;

                if (flake.Y > screenHeight + RespawnMarginBelowScreen)
                {
                    flake.X = random.Next(-HorizontalSpawnMargin, screenWidth + HorizontalSpawnMargin);
                    flake.Y = -random.Next(SpawnHeightMin, SpawnHeightMax);
                    flake.Speed = SnowflakeMinSpeed + (flake.Size / SnowflakeMaxSize) * (SnowflakeMaxSpeed - SnowflakeMinSpeed);
                }

                if (flake.X > screenWidth + HorizontalSpawnMargin)
                {
                    flake.X = -HorizontalSpawnMargin;
                }
                else if (flake.X < -HorizontalSpawnMargin)
                {
                    flake.X = screenWidth + HorizontalSpawnMargin;
                }
            }
        }

        private void MainForm_Paint(object sender, PaintEventArgs e)
        {
            if (backgroundImage == null)
            {
                return;
            }

            using (var buffer = new Bitmap(ClientRectangle.Width, ClientRectangle.Height))
            using (var graphics = Graphics.FromImage(buffer))
            {
                graphics.DrawImage(backgroundImage, ClientRectangle);

                foreach (var flake in snowflakes)
                {
                    float drawSize = flake.Size * SnowflakeSizeMultiplier;

                    if (snowflakeImage != null)
                    {
                        graphics.DrawImage(snowflakeImage, flake.X, flake.Y, drawSize, drawSize);
                    }
                    else
                    {
                        graphics.FillEllipse(Brushes.White, flake.X, flake.Y, drawSize, drawSize);
                    }
                }
                e.Graphics.DrawImage(buffer, Point.Empty);
            }
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            LoadResources();
            CreateSnowflakes();
            SetupTimer();
            animationTimer.Start();
        }

        private void MainForm_KeyDown(object sender, KeyEventArgs e)
        {
            Close();
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            animationTimer.Stop();
            animationTimer.Dispose();
            backgroundImage?.Dispose();
            snowflakeImage?.Dispose();
            base.OnFormClosing(e);
        }
    }
}
