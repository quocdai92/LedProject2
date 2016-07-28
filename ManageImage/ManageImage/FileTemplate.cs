using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LedProject
{
    public class FileTemplate
    {
        public string FileName { get; set; }
        public List<Image> ListImages { get; set; } 
        public int TimePlay { get; set; }

        public FileTemplate()
        {
            ListImages = new List<Image>();
        }
    }
}
