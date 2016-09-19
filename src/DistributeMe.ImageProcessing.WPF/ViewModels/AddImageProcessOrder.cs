﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows.Input;
using DistributeMe.ImageProcessing.WPF.Helpers;
using DistributeMe.ImageProcessing.WPF.Messages;
using Microsoft.Win32;

namespace DistributeMe.ImageProcessing.WPF.ViewModels
{
    public class AddImageProcessOrder : ObservableObject,IDisposable
    {
        private ObservableCollection<ProcessRequest> processRequests;
        private RabbitMqManager rabbitMqManager;

        public AddImageProcessOrder()
        {
            OpenImageFileCommand = new RelayCommand(openImageFileCommand_Executed);
            processRequests = new ObservableCollection<ProcessRequest>();

            rabbitMqManager = new RabbitMqManager();
        
            rabbitMqManager.ListenForFaceProcessImageEvent(processRequests);
            rabbitMqManager.ListenForOcrProcessImageEvent(processRequests);
        }

        public ObservableCollection<ProcessRequest> ProcessRequests
        {
            get { return processRequests; }
            set
            {
                processRequests = value;
                RaisePropertyChanged("ProcessRequests");
            }
        }

        public ICommand OpenImageFileCommand { get; }

        private void openImageFileCommand_Executed(object obj)
        {
            var dlg = new OpenFileDialog
            {
                DefaultExt = ".png",
                Filter = "JPEG Files (*.jpeg)|*.jpeg|PNG Files (*.png)|*.png|JPG Files (*.jpg)|*.jpg"
            };

            var result = dlg.ShowDialog();
            if (result == true)
            {
                var request = new ProcessRequest
                {
                    RequestId = Guid.NewGuid()
                };
                ProcessRequests.Insert(0, request);
                var processImageCommand = new ProcessImageCommand(request.RequestId, File.ReadAllBytes(dlg.FileName));
                using (var bus = new RabbitMqManager())
                {
                    bus.SendProcessImageCommand(processImageCommand);
                }
            }
        }

        public void Dispose()
        {
            rabbitMqManager.Dispose();
        }
    }
}