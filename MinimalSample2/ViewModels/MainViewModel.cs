using AsyncAwaitBestPractices;
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
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MinimalSample2.ViewModels
{
    public partial class MainViewModel : ObservableObject
    {
        [ObservableProperty]
        private int calculationProgress = 2;

        [ObservableProperty]
        bool isUpdating = false;

        [ObservableProperty]
        bool isAnimating = true;

        [ObservableProperty]
        private int upperLimit;

        private int batchSize = 10000;
        private Progress<IList<NumberItem>> progress = new Progress<IList<NumberItem>>();

        private List<int> primeList = new();

        //private readonly SourceCache<NumberItem, int> numberItemsSourceCache = new(NumberItem => NumberItem.Number);
        private List<NumberItem> bufferList = new();
        private readonly SourceCache<NumberItem, int> numberItemsDisplayCache = new(NumberItem => NumberItem.Number);
        private readonly ReadOnlyObservableCollection<NumberItem> numberItems;
        public ReadOnlyObservableCollection<NumberItem> NumberItems => numberItems;

        //private ObservableCollection<NumberItem> numberItems = new();
        //public ObservableCollection<NumberItem> NumberItems = new();

        

        public MainViewModel()
        {
            _ = numberItemsDisplayCache
                    .Connect()
                    //.AutoRefresh(new TimeSpan((long)1000))
                    .ObserveOn(RxApp.MainThreadScheduler)
                    .Bind(out numberItems)
                    .DeferUntilLoaded()
                    .Subscribe();

            primeList.Add(2);

            progress.ProgressChanged += (sender, newItems) => OnProgressChanged(sender, newItems);

            Calculate(progress).SafeFireAndForget(onException: ex => Console.WriteLine(ex));
        }

        [RelayCommand]
        private async void StartDisplay()
        {
            IsUpdating = true;
            await Dispatcher.UIThread.InvokeAsync(() => UpdateNumberItems(UpperLimit, IsAnimating));
            IsUpdating = false;
        }

        private async Task UpdateNumberItems(int upperLimit, bool isAnimating)
        {
            int currentlyDisplayed = numberItemsDisplayCache.Count;
            bool isAdding = upperLimit > currentlyDisplayed;

            if (isAnimating)
            {
                if (isAdding)
                {
                    for (int i = currentlyDisplayed; i < upperLimit; i++)
                    {
                        numberItemsDisplayCache.AddOrUpdate(bufferList[i]);
                        await Task.Delay(10);
                    }
                }
                else
                {
                    for (int i = currentlyDisplayed; i > upperLimit; i--)
                    {
                        numberItemsDisplayCache.RemoveKey(i);
                        await Task.Delay(10);
                    }
                }
            }
            else
            {
                if (isAdding)
                {
                    numberItemsDisplayCache.AddOrUpdate(bufferList.GetRange(currentlyDisplayed, UpperLimit - currentlyDisplayed));
                }
                else
                {
                    numberItemsDisplayCache.RemoveKeys(numberItemsDisplayCache.Keys.Where(x => x > UpperLimit));
                }
                
            }
        }

        void OnProgressChanged(object? sender, IList<NumberItem> newItems)
        {
            CalculationProgress = newItems.Count;
            bufferList = new();
            bufferList.AddRange(newItems);
        }

        async Task Calculate(Progress<IList<NumberItem>> progress)
        {
            await Task.Yield();

            SourceCache<NumberItem, int> numberItemsSourceCache = new(NumberItem => NumberItem.Number);

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
                ((IProgress<IList<NumberItem>>)progress).Report((IList<NumberItem>)numberItemsSourceCache.Items);
                
            }
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