﻿using BSModManager.Models;
using BSModManager.Views;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Regions;
using Prism.Services.Dialogs;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using System;
using System.Collections.Generic;

namespace BSModManager.ViewModels
{
    public class MainWindowViewModel : BindableBase
    {
        // プロパティにしないとバインドされない
        public ReactiveCommand AllCheckedButtonCommand { get; } = new ReactiveCommand();

        // https://whitedog0215.hatenablog.jp/entry/2020/03/17/221403
        public ReadOnlyReactivePropertySlim<string> Console { get; }
        public ReadOnlyReactivePropertySlim<string> GameVersion { get; }
        
        private string title = "BSModManager";
        public string Title
        {
            get { return title; }
            set { SetProperty(ref title, value); }
        }

        private string myselfVersion = "Version\n---";
        public string MyselfVersion
        {
            get { return myselfVersion; }
            set { SetProperty(ref myselfVersion, value); }
        }

        private bool installButtonEnable = true;
        public bool InstallButtonEnable
        {
            get { return installButtonEnable; }
            set { SetProperty(ref installButtonEnable, value); }
        }

        private bool modRepositoryButtonEnable = true;
        public bool ModRepositoryButtonEnable
        {
            get { return modRepositoryButtonEnable; }
            set { SetProperty(ref modRepositoryButtonEnable, value); }
        }

        private bool changeUrlButtonEnable = true;
        public bool ChangeUrlButtonEnable
        {
            get { return changeUrlButtonEnable; }
            set { SetProperty(ref changeUrlButtonEnable, value); }
        }

        private bool allCheckedButtonEnable = true;
        public bool AllCheckedButtonEnable
        {
            get { return allCheckedButtonEnable; }
            set { SetProperty(ref allCheckedButtonEnable, value); }
        }

        IDialogService dialogService;
        ConfigFileManager configFileManager;
        VersionManager versionManager;
        MainWindowPropertyModel mainWindowPropertyModel;
        MainTabPropertyModel mainTabPropertyModel;
        InitialSettingViewModel initialSettingViewModel;

        public IRegionManager RegionManager { get; private set; }
        public DelegateCommand<string> ShowMainTabViewCommand { get; private set; }
        public DelegateCommand<string> ShowSettingsTabViewCommand { get; private set; }
        public DelegateCommand LoadedCommand { get; }

        public MainWindowViewModel(IRegionManager regionManager,IDialogService ds,ConfigFileManager cfm, VersionManager vm,MainWindowPropertyModel mwpm,MainTabPropertyModel mtpm,InitialSettingViewModel isvm)
        {
            configFileManager = cfm;
            versionManager = vm;
            mainWindowPropertyModel = mwpm;
            mainTabPropertyModel = mtpm;
            initialSettingViewModel = isvm;

            dialogService = ds;

            // https://whitedog0215.hatenablog.jp/entry/2020/03/17/221403
            this.Console = mainWindowPropertyModel.ObserveProperty(x => x.Console).ToReadOnlyReactivePropertySlim();
            this.GameVersion = mainWindowPropertyModel.ObserveProperty(x => x.GameVersion).ToReadOnlyReactivePropertySlim();

            MyselfVersion = versionManager.GetMyselfVersion();

            RegionManager = regionManager;
            RegionManager.RegisterViewWithRegion("ContentRegion", typeof(MainTab));
            ShowMainTabViewCommand = new DelegateCommand<string>((x) =>
              {
                  mainWindowPropertyModel.Console = "Main";
                  InstallButtonEnable = true;
                  ModRepositoryButtonEnable = true;
                  ChangeUrlButtonEnable = true;
                  AllCheckedButtonEnable = true;
                  RegionManager.RequestNavigate("ContentRegion", x);
              });
            ShowSettingsTabViewCommand = new DelegateCommand<string>((x) =>
              {
                  mainWindowPropertyModel.Console = "Settings";
                  InstallButtonEnable = false;
                  ModRepositoryButtonEnable = false;
                  ChangeUrlButtonEnable = false;
                  AllCheckedButtonEnable = false;
                  RegionManager.RequestNavigate("ContentRegion", x);
              });

            AllCheckedButtonCommand.Subscribe(_ => 
            {
                mainTabPropertyModel.AllCheckedOrUnchecked(); 
            });

            LoadedCommand = new DelegateCommand(() =>
            {
                dialogService.ShowDialog("InitialSetting");
            });
        }
    }
}
