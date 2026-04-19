
using System;
using System.Diagnostics;
using System.Globalization;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using static CopyFolders2.GlobalVariables;

namespace CopyFolders2;

public class App : Application

{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        // 1. IL PARACADUTE GLOBALE
        TaskScheduler.UnobservedTaskException += (_, e) => // Cambiato '_' in 'senderApp' per evitare conflitti
        {
            e.SetObserved();
            Avalonia.Threading.Dispatcher.UIThread.Post(() => 
            {
                // Lanciamo il task senza assegnarlo a nulla, così Rider non si lamenta
                Task.Run(async () => 
                {
                    try 
                    {
                        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop2)
                        {
                            var msg = e.Exception.InnerException?.Message ?? e.Exception.Message;
                            var errorWin = new MessBox(true, 1, $"{MyErr} Async task error - {msg}")
                                {
                                    DataContext = desktop2.MainWindow?.DataContext
                                };

                            if (desktop2.MainWindow != null) 
                                await errorWin.ShowDialog(desktop2.MainWindow);
                            else 
                                errorWin.Show();
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"On fly error: {ex.Message}");
                    }
                });
            });
        };

        MyErr = 500;
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            var md = new MyData();
            string msg = md.ScriviSettingsFile();
            string str = md.LoadSettings();

            string? erroreRilevato = null;
            if (msg.Length > 4) erroreRilevato = msg;
            else if (str.Length > 4) erroreRilevato = str;

            if (erroreRilevato != null)
            {
                var vm = new MainViewModel();
                var errorWin = new MessBox(false, 1, erroreRilevato) { DataContext = vm };
                desktop.MainWindow = errorWin;
                errorWin.Closed += (_, _) => desktop.Shutdown();    // sostituiti s ed e con underscore: non  i servono i risultati
            }
            else
            {
                MyErr = 510;
                if (string.IsNullOrEmpty(Lang) || Lang.Length > 2)
                {
                    Lang = CultureInfo.CurrentCulture.TwoLetterISOLanguageName;
                }
                else
                {
                    var vm = new MainViewModel();
                    vm.SetLanguage(Lang);
                }

                desktop.MainWindow = new MainWindow { DataContext = new MainViewModel() };
            }
        }

        base.OnFrameworkInitializationCompleted();
    }
}