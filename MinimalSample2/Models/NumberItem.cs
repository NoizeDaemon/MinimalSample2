using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MinimalSample2.Models
{
    public partial class NumberItem : ObservableObject
    {
        [ObservableProperty]
        private string _name;

        [ObservableProperty]
        private int _number;

        [ObservableProperty]
        private bool _isPrime;

        [ObservableProperty]
        private bool _visible = true;
    }
}
