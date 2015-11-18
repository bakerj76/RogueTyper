using System;
using System.Windows.Media;


namespace Game
{
    /// <summary>
    /// Code taken from http://evanl.wordpress.com/2009/12/06/efficient-optimal-per-frame-eventing-in-wpf/
    /// Gives us a way to get the time between frames.
    /// </summary>
    public static class CompositionTargetEx
    {
        public static double DeltaFrame;

        private static TimeSpan _last = TimeSpan.Zero;
        private static event EventHandler<RenderingEventArgs> _FrameUpdating;
        public static event EventHandler<RenderingEventArgs> FrameUpdating
        {
            add
            {
                if (_FrameUpdating == null)
                    CompositionTarget.Rendering += CompositionTargetRendering;
                
                _FrameUpdating += value;
            }
            remove
            {
                _FrameUpdating -= value;
                
                if (_FrameUpdating == null)
                    CompositionTarget.Rendering -= CompositionTargetRendering;
            }
        }

        static void CompositionTargetRendering(object sender, EventArgs e)
        {
            var args = (RenderingEventArgs)e;

            if (args.RenderingTime == _last) return;

            DeltaFrame = args.RenderingTime.TotalSeconds - _last.TotalSeconds;
            _last = args.RenderingTime;
            _FrameUpdating(sender, args);
        }
    }
}
