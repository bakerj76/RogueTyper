using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Game.GameObjects;
using Game.Managers;
using Game.Map;

namespace Game
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private const int MapWidth = 40, MapHeight = 40;

        public static Viewport3D Viewport { get; private set; }
        public static Camera2D Camera { get; private set; }

        private readonly GameObjectManager _objManager;
        private readonly Level _level;
        private bool _isSplashScreen;

        public MainWindow()
        {
            InitializeComponent();

            Viewport = MainView;
            Camera = new Camera2D(MainCamera);

            _objManager = new GameObjectManager(MapWidth, MapHeight);
            _level = new Level(MapWidth, MapHeight, _objManager);

            _isSplashScreen = false;

            CompositionTargetEx.FrameUpdating += CompositionTargetExOnFrameUpdating;
        }

        // Runs every frame
        private void CompositionTargetExOnFrameUpdating(object sender, RenderingEventArgs renderingEventArgs)
        {
            if (_level.GameOver) ShowSplashScreen();
            Camera.Update();
            _objManager.Update();
        }

        public void ShowSplashScreen()
        {
            if (_isSplashScreen) return;
            
            _isSplashScreen = true;
            _objManager.ClearObjects();
            _objManager.AddUI(null);

            Camera.Move(20, new Vector(0, 0));
            _objManager.AddObject(new TypingText(new Vector(0, 5), "Rogue Typer", Brushes.Green) {Scale = new Vector(25, 12.5)});
            _objManager.AddObject(new TypingText(new Vector(0, -5), "Press enter to start", Brushes.Green) {Scale = new Vector(8, 2)});
            _objManager.Draw();
        }

        public void Input(EventArgs e)
        {
            if (!IsActive) return;
            if (!(e is KeyEventArgs)) return;

            if (_level.GameOver)
            {
                if (((KeyEventArgs)e).Key == Key.Enter)
                {
                    _isSplashScreen = false;
                    _level.StartGame();
                }
            }

            _objManager.Input((KeyEventArgs)e);
        }

        private void OnKeyUp(object sender, KeyEventArgs e)
        {
            Input(e);
        }

        private void OnKeyDown(object sender, KeyEventArgs e)
        {
            Input(e);
        }

        private void MouseInput(object sender, MouseButtonEventArgs e)
        {
            Input(e);
        }
    }
}
