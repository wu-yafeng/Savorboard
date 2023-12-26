using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Silos.Perf
{
    public class DiagnosticHelper
    {
        private readonly Meter _metric = new("Savorboard");
        private readonly ActivitySource _trace = new("Savorboard");
        public Meter Metric => _metric;

        public ActivitySource Trace => _trace;
    }
}
