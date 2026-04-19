
using System;
using System.Diagnostics;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Interactivity;
using static CopyFolders2.GlobalVariables;

namespace CopyFolders2;

public partial class Info : Window
{
    private readonly bool stoPartendo;
    // Una sola volta per tutta la classe
    private MainViewModel? Vm => DataContext as MainViewModel;
    
    public Info()
    {
        InitializeComponent();
        MyErr = 600;
        stoPartendo = true;
        switch(Lang.ToLower())
        {
            case "en":
                CmbLang.SelectedIndex=0;
                break;
            case "de":
                CmbLang.SelectedIndex=1;
                break;
            case "fr":
                CmbLang.SelectedIndex=2;
                break;
            case "it":
                CmbLang.SelectedIndex=3;
                break;
            case "es":
                CmbLang.SelectedIndex=4;
                break;
            case "pl":
                CmbLang.SelectedIndex=5;
                break;
            case "pt":
                CmbLang.SelectedIndex=6;
                break;
            case "ru":
                CmbLang.SelectedIndex=7;
                break;
            case "zh":
                CmbLang.SelectedIndex=8;
                break;
            default:
                CmbLang.SelectedIndex=0;
                break;
        } 
        stoPartendo = false;
    }
    
    private void TopLevel_OnOpened(object? sender, EventArgs e)
    {
        ChkInf.IsChecked= CheckUpdates;
    }

    private void CmbLang_OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        MyErr = 610;
        if (stoPartendo) return;
        
        if (e.Source is ComboBox)
        {
            if (e.AddedItems.Count > 0)
            {
                var selectedItem = e.AddedItems[0];
    
                if (selectedItem is ComboBoxItem item)
                {
                    Lang = item.Name ?? "en";
        
                    MyErr = 615;
                    //(DataContext as MainViewModel)?.SetLanguage(Lang);
                    Vm!.SetLanguage(Lang);
                    
                    MyErr = 617;
                    if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
                    {
                        // Recupera l'istanza reale della MainWindow già aperta
                        var mw = desktop.MainWindow as MainWindow;
    
                        // Ora mw.TxtFiles si riferisce al controllo visibile
                        mw?.TxtFiles.Clear();
                        mw?.ActEscape(200); 
                    }
                }
            }
        }
    }

    private void ChkInf_OnIsCheckedChanged(object? sender, RoutedEventArgs e)
    {
        CheckUpdates= (bool)ChkInf.IsChecked !;
    }

    private void BtnClose_OnClick(object? sender, RoutedEventArgs e)
    {
        this.Close();
    }

    private async void BtnCheck_OnClick(object? sender, RoutedEventArgs e)
    {
        try
        {
            MyErr = 630;
            //per il binding multilang
            //var vm = this.DataContext as MainViewModel; riportato una sola volta in cima

            BtnCheck.Content = $"{Lbl.Wait}..." ;
            int versione = await CheckVersion.LeggiVersioneAsync(200);
            Console.WriteLine(versione);
            MyErr = 632;
            if (versione == 1) //************ correggere a 1
            {
                // img: 0= info -1=errore -2=warning
                var inf = new MessBox(true, 0, Vm!.Update)
                {
                    // Assegna il DataContext della finestra corrente alla nuova finestra
                    DataContext = this.DataContext
                };
                var res= await inf.ShowDialog<string>(this);
                Console.WriteLine(res);
                if (res == "Yes")
                {
                    string link;
                    if (Lang.Equals("it", StringComparison.CurrentCultureIgnoreCase))
                         link = "https://psmate.com/it/download/copyfolders-2.html";
                    else link = "https://psmate.com/en/downloads/copyfolders-2.html";
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = link,
                        UseShellExecute = true // Fondamentale per aprire file con l'app predefinita
                    });
                    this.Close();
                }
            }
            else if (versione == 2) //************ correggere a 2
            {
                 BtnCheck.Content = Vm!.UpToDate;
            }
            else
            {
                var inf = new MessBox(true, 0, Vm!.UpdateError)
                {
                    // Assegna il DataContext della finestra corrente alla nuova finestra
                    DataContext = this.DataContext
                };
                await inf.ShowDialog<string>(this);
                BtnCheck.Content = Vm!.CheckNow;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            var inf = new MessBox(false, 0, $"{MyErr} -{ex.Message}")
            {
                // Assegna il DataContext della finestra corrente alla nuova finestra
                DataContext = this.DataContext
            };
            await inf.ShowDialog<string>(this);
        }
        
    }
}   