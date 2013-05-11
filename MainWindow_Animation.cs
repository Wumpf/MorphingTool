using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace MorphingTool
{
    /// <summary>
    /// Partial class of the MainWindow with all animation related code.
    /// </summary>
    public partial class MainWindow
    {
        private DispatcherTimer _animationPlayer = new DispatcherTimer();
        private System.Diagnostics.Stopwatch _animationStopWatch = new System.Diagnostics.Stopwatch();

        private void AnimationPlayerTimeElapsed(object sender, EventArgs e)
        {
            double progress = _animationStopWatch.Elapsed.TotalSeconds / (double)Duration.Value;

            // this should raise a changed value event and refresh the image therefore
            ProgressBar.Value = Math.Min(ProgressBar.Maximum, ProgressBar.Minimum + (ProgressBar.Maximum - ProgressBar.Minimum) * progress);

            if (progress >= 1.0)
            {
                if ((bool)Loop.IsChecked)
                {
                    // correct interval and restart/keep goin
                    _animationPlayer.Interval = new TimeSpan(0, 0, 0, 0, (int)((double)Duration.Value / (double)NumFrames.Value * 1000.0));
                    _animationStopWatch.Restart();
                }
                else
                    StopAutoAnimation();
            }
        }

        private void StopAutoAnimation()
        {
            _animationPlayer.Stop();
            ProgressBar.IsEnabled = true;
            AutoAnimationStartButton.Content = "Start";
        }

        private void PlayButton_Click(object sender, RoutedEventArgs e)
        {
            if (!_animationPlayer.IsEnabled)
            {
                ProgressBar.IsEnabled = false;
                AutoAnimationStartButton.Content = "Stop";
                _animationPlayer.Interval = new TimeSpan(0, 0, 0, 0, (int)((double)Duration.Value / (double)NumFrames.Value * 1000.0));
                _animationStopWatch.Restart();
                _animationPlayer.Start();
            }
            else
            {
                StopAutoAnimation();
            }
        }

        private void NumberOfFrames_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            UpdateOutputImageContent();
        }
    }
}
