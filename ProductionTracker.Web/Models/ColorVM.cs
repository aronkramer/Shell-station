﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ProductionTracker.Data;

namespace ProductionTracker.Web.Models
{
    public class ColorVM
    {
        public IEnumerable<Color> Colors { get; set; }
    }
}