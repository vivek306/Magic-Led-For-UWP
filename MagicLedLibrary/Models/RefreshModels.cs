using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI;

namespace MagicLedLibrary.Models
{
    public class RefreshModels
    {
        public bool PowerState { get; set; }
        public Color CurrentColor { get; set; }
        public string Mode { get; set; }
    }
}
