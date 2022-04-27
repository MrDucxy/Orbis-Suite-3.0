﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SetupBA.MVVM.ViewModel
{
    public class InstallViewModel
    {
        public InstallViewModel(MainViewModel mainViewModel)
        {
            MainVM = mainViewModel;
        }

        public MainViewModel MainVM { get; set; }
    }
}