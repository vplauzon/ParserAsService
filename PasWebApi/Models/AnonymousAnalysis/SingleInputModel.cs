﻿using System;
using System.Collections.Generic;

namespace PasWebApi.Models.AnonymousAnalysis
{
    public class SingleInputModel
    {
        public string Grammar { get; set; }

        public string Rule { get; set; }

        public string Text { get; set; }
    }
}