using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CSIRO.Metaheuristics
{
    public abstract class Subject
    {
        public event AdvancedEventHandler Advanced;

        protected void OnAdvanced(EventArgs e)
        {
            if (Advanced != null)
                Advanced(this, e);
        }
    }

    public delegate void AdvancedEventHandler(object sender, EventArgs e);
}
