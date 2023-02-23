using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DynamicData;
using Microsoft.CodeAnalysis.Operations;
using MinimalSample2.Models;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Reactive.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MinimalSample2.ViewModels
{
    public partial class MainViewModel : ObservableObject
    {
        [ObservableProperty]
        private int calculationProgress = 2;

        private int batchSize = 10000;
        private Progress<int> progress = new Progress<int>();

        private List<int> primeList = new();

        private readonly SourceCache<NumberItem, int> numberItemsSourceCache = new(NumberItem => NumberItem.Number);
        private readonly ReadOnlyObservableCollection<NumberItem> numberItems;
        public ReadOnlyObservableCollection<NumberItem> NumberItems => numberItems;

        public MainViewModel()
        {
            _ = numberItemsSourceCache
                    .Connect()
                    //.AutoRefresh(new TimeSpan((long)1000))
                    .ObserveOn(RxApp.MainThreadScheduler)
                    .Bind(out numberItems)
                    .DeferUntilLoaded()
                    .Subscribe();

            primeList.Add(2);

            progress.ProgressChanged += (sender, currentNumbers) => OnProgressChanged(sender, currentNumbers);

            Task.Run(() => Calculate(progress));
            //Dispatcher.UIThread.InvokeAsync( () => Calculate(progress), DispatcherPriority.Background);
        }

        void OnProgressChanged(object? sender, int currentNumbers)
        {
            CalculationProgress = currentNumbers;
            numberItemsSourceCache.Refresh();
        }

        Task Calculate(Progress<int> progress)
        {
            int currentNumbers = 2;

            numberItemsSourceCache.AddOrUpdate(new NumberItem
            {
                Name = "N1",
                Number = 1,
                IsPrime = false
            });

            while (currentNumbers <= 50000)
            {
                numberItemsSourceCache.Edit(innerCache =>
                {
                    for (int i = currentNumbers; i <= currentNumbers + batchSize; i++)
                    {
                        innerCache.AddOrUpdate(new NumberItem
                        {
                            Name = "N" + i,
                            Number = i,
                            IsPrime = IsPrimeCheck(i)
                        });

                        if (i is 50000) break;
                    }
                });
                currentNumbers += batchSize;
                ((IProgress<int>)progress).Report(currentNumbers);
                
            }
            return Task.CompletedTask;
        }

        bool IsPrimeCheck(int n)
        {
            if (n is 2) return true;

            for (int p = 0; p < primeList.Count; p++)
            {
                if (n % primeList[p] == 0)
                {
                    return false;
                }
                else if (primeList[p] > n / 2)
                {
                    break;
                }
            }
            primeList.Add(n);
            Debug.WriteLine(n);
            return true;
        }
    }
}