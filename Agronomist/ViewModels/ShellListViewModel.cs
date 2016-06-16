﻿namespace Agronomist.ViewModels
{
    using System;
    using System.Linq;
    using DatabasePOCOs.User;

    public class ShellListViewModel : ViewModelBase
    {
        private string _boxName;
        private string _cropName;
        private string _iconLetter;

        public ShellListViewModel(CropCycle cropCycle)
        {
            CropRunId = cropCycle.ID;
            CropViewModel = new CropViewModel(cropCycle);
            Update(cropCycle);
        }

        public Guid CropRunId { get; }


        public string CropName
        {
            get { return _cropName; }
            set
            {
                if (value == _cropName) return;
                _cropName = value;
                OnPropertyChanged();
            }
        }

        public string IconLetter
        {
            get { return _iconLetter; }
            set
            {
                if (value == _iconLetter) return;
                _iconLetter = value;
                OnPropertyChanged();
            }
        }


        public string BoxName
        {
            get { return _boxName; }
            set
            {
                if (value == _boxName) return;
                _boxName = value;
                OnPropertyChanged();
            }
        }

        private string _isAlerted;

        public const string Visible = "Visible";
        public const string Collapsed = "Collapsed";

        public string IsAlerted
        {
            get { return _isAlerted; }
            set
            {
                if (value == _isAlerted) return;
                _isAlerted = value;
                OnPropertyChanged();
            }
        }

        private CropViewModel _cropViewModel;

        public CropViewModel CropViewModel
        {
            get { return _cropViewModel; }
            set
            {
                if (value == _cropViewModel) return;
                _cropViewModel = value;
                OnPropertyChanged();
            }
        }

        private void Update(CropCycle cropCycle)
        {
            CropName = cropCycle.CropTypeName;
            BoxName = cropCycle.Location.Name;
            IconLetter = CropName.Substring(0, 1);
            CropViewModel.Update(cropCycle);
            if(cropCycle.Location.Devices.SelectMany(s => s.Sensors).Any(s => s.Alarmed))
            {
                IsAlerted = Visible;
            }
            else
            {
                IsAlerted = Collapsed;
            }
        }
    }
}