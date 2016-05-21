using FileOperationDemo.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Template10.Mvvm;
using Template10.Services.NavigationService;
using Template10.Services.SerializationService;
using Windows.Media.Capture;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.UI.Core;
using Windows.UI.Xaml.Navigation;

namespace FileOperationDemo.ViewModels
{
    public class MainPageViewModel : ViewModelBase
    {
        public MainPageViewModel()
        {
        }

        private ObservableCollection<FileInfo> _FileList = new ObservableCollection<FileInfo>();

        public ObservableCollection<FileInfo> FileList
        {
            get
            {
                return _FileList;
            }
            set
            {
                Set(ref _FileList, value);
            }
        }

        /// <summary>
        /// ����ļ�
        /// </summary>
        public DelegateCommand AddFile => new DelegateCommand(
                    async () =>
                    {
                        // ѡ�����ļ�
                        FileOpenPicker openPicker = new FileOpenPicker();
                        openPicker.ViewMode = PickerViewMode.Thumbnail;
                        openPicker.SuggestedStartLocation = PickerLocationId.PicturesLibrary;
                        openPicker.FileTypeFilter.Clear();
                        openPicker.FileTypeFilter.Add("*");
                        var files = await openPicker.PickMultipleFilesAsync();
                        files.ToList().ForEach(
                            file => FileList?.Add(new FileInfo(file))
                            );
                    }
                    );

        /// <summary>
        /// ����
        /// </summary>
        public DelegateCommand TakePhoto => new DelegateCommand(
                    async () =>
                    {
                        // ����
                        CameraCaptureUI captureUI = new CameraCaptureUI();
                        captureUI.PhotoSettings.Format = CameraCaptureUIPhotoFormat.Png;
                        captureUI.PhotoSettings.AllowCropping = false;
                        var photo = await captureUI.CaptureFileAsync(CameraCaptureUIMode.Photo);
                        if (photo != null)
                        {
                            FileList?.Add(new FileInfo(photo, true));
                        }
                    }
                    );

        /// <summary>
        /// �鿴�ļ�
        /// </summary>
        public DelegateCommand<FileInfo> ShowFile => new DelegateCommand<FileInfo>(
            async fileinfo =>
            {
                if (fileinfo != null)
                {
                    // ��ȡ�ɷ����ļ���Ϣ
                    var file = await fileinfo.GetShowFileInfo();
                    if (file != null)
                    {
                        // Ĭ��Ӧ�ô��ļ�
                        await Windows.System.Launcher.LaunchFileAsync(file);
                    }
                }
            }
            );
        /// <summary>
        /// ɾ���ļ�
        /// </summary>
        public DelegateCommand<FileInfo> DeleteFile => new DelegateCommand<FileInfo>(
            fileinfo =>
            {
                if (fileinfo != null && FileList.Contains(fileinfo))
                {
                    // ɾ������Ȩ���б��е�����
                    fileinfo.ClearAccessToken();
                    FileList.Remove(fileinfo);
                }
            }
            );

        public override async Task OnNavigatedToAsync(object parameter, NavigationMode mode, IDictionary<string, object> suspensionState)
        {
            try
            {
                if (suspensionState.Any())
                {
                    FileList = SerializationService.Json.Deserialize<ObservableCollection<FileInfo>>(suspensionState[nameof(FileList)]?.ToString());
                }
                else
                {
                    // ������ʱ����
                    await ApplicationData.Current.ClearAsync(ApplicationDataLocality.Temporary);
                }

                // ע��ϵͳ���ذ�ť�¼�
                SystemNavigationManager.GetForCurrentView().BackRequested += OnBackRequested;
                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                Debug.WriteLine(ex.Source);
                Debug.WriteLine(ex.StackTrace);
            }
        }

        public override async Task OnNavigatedFromAsync(IDictionary<string, object> suspensionState, bool suspending)
        {
            try
            {
                if (suspending)
                {
                    suspensionState[nameof(FileList)] = SerializationService.Json.Serialize(FileList);
                }
                // ������ʱ����
                await ApplicationData.Current.ClearAsync(ApplicationDataLocality.Temporary);
                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                Debug.WriteLine(ex.Source);
                Debug.WriteLine(ex.StackTrace);
            }
        }

        public override async Task OnNavigatingFromAsync(NavigatingEventArgs args)
        {
            try
            {
                args.Cancel = false;
                // ע��ϵͳ���ذ�ť�¼�
                SystemNavigationManager.GetForCurrentView().BackRequested -= OnBackRequested;
                // ������ʱ����
                await ApplicationData.Current.ClearAsync(ApplicationDataLocality.Temporary);
            }
            catch
            {
                // ��ʱ�ļ�ɾ���쳣ʱ�����κδ���
            }
            await Task.CompletedTask;
        }

        /// <summary>
        /// ���ذ�ť�¼�
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnBackRequested(object sender, BackRequestedEventArgs e)
        {
            e.Handled = true;
            NavigationService.GoBack();
        }

        public void GotoSettings() =>
            NavigationService.Navigate(typeof(Views.SettingsPage), 0);

        public void GotoAbout() =>
            NavigationService.Navigate(typeof(Views.SettingsPage), 1);

    }
}

