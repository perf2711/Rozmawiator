using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace Rozmawiator.Controls
{
    public class ToggleImageButton : ImageButton
    {
        public ImageSource OnIcon { get; set; }

        private ImageSource _offIcon;

        public ImageSource OffIcon
        {
            get { return _offIcon; }
            set
            {
                _offIcon = value;
                Icon = value;
            }
        }

        private bool _state = false;

        public bool State
        {
            get { return _state; }
            set
            {
                _state = value;
                Image.Source = State ? OnIcon : OffIcon;
            }
        }

        protected override void OnClick(object sender, RoutedEventArgs e)
        {
            State = !State;
            base.OnClick(sender, e);
        }
    }
}
