using System;
using System.Globalization;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;

namespace Game.GameObjects
{
    public class TypingText : Sprite
    {
        private const int QualityScale = 4;
        private const int SizeScaleDivision = 40;

        private FormattedText _font;
        private readonly Typeface _typeface;
        private readonly CultureInfo _culture;
        private readonly Brush _background;
        private readonly Brush _originalColor;

        private int _coloredCharacters;
        public int ColoredCharacters { get { return _coloredCharacters; } set {ChangeColoredCharacters(value);} }
        private string _text;
        public string Text { get { return _text; } set {ChangeText(value);} }
        public Brush FontColor { get; set; }

        public TypingText(Vector position, string text, Brush fontColor = null, Brush background = null) : base(position)
        {
            _text = text;
            FontColor = fontColor;
            _background = background;
            _originalColor = fontColor;
            _culture = new CultureInfo("en-us");
            
            var font = new FontFamily(new Uri("pack://application:,,,/"), "./resources/#Joystix Monospace");
            _typeface = new Typeface(font, FontStyles.Normal, FontWeights.Normal, new FontStretch());
            _font = new FormattedText(text, _culture, FlowDirection.LeftToRight, _typeface, 16.0, Brushes.Red);

            Scale = new Vector(_font.Width / SizeScaleDivision, _font.Height / SizeScaleDivision);
            Layer = 1.1;
        }

        public override void Input(System.Windows.Input.KeyEventArgs e)
        {
        }

        public override void Draw()
        {
            if (_text.Length == 0) 
            {
                SpriteImage = null;
            }
            else
            {
                _font.SetForegroundBrush(FontColor ?? Brushes.White, ColoredCharacters, _text.Length - ColoredCharacters);

                var drawingVisual = new DrawingVisual();
                var drawingContext = drawingVisual.RenderOpen();

                if (_background != null)
                {
                    drawingContext.DrawRectangle(_background, null, new Rect(0, 0, _font.Width, _font.Height));
                }

                drawingContext.DrawText(_font, new Point());
                drawingContext.Close();

                var bmp = new RenderTargetBitmap((int)_font.Width*QualityScale, (int)_font.Height*QualityScale, 
                                                 96*QualityScale, 96*QualityScale, PixelFormats.Pbgra32);
                bmp.Render(drawingVisual);

                SpriteImage = new DiffuseMaterial(new ImageBrush { ImageSource = bmp });
            }
            
            base.Draw();
        }

        private void ChangeText(string text)
        {
            _text = text;
            _font = new FormattedText(text, _culture, FlowDirection.LeftToRight, _typeface, 16.0, Brushes.Red);

            Scale = new Vector(_font.Width / SizeScaleDivision, _font.Height / SizeScaleDivision);

            Draw();
        }

        private void ChangeColoredCharacters(int chars)
        {
            _coloredCharacters = chars;
            _font = new FormattedText(_text, _culture, FlowDirection.LeftToRight, _typeface, 16.0, Brushes.Red);

            Draw();
        }

        public int CheckMatch(string str)
        {
            var charMatch = 0;

            while (charMatch < _text.Length && charMatch < str.Length &&  Char.ToLower(str[charMatch]) == Char.ToLower(_text[charMatch]))
            {
                charMatch++;
            }

            return charMatch;
        }

        public void Disable()
        {
            FontColor = Brushes.DimGray;
            Draw();

            Alive = false;
        }

        public void Enable()
        {
            FontColor = _originalColor;
            Draw();

            Alive = true;
        }
    }
}
