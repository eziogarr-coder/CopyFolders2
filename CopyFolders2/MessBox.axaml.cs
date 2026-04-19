using System;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using static CopyFolders2.GlobalVariables;

namespace CopyFolders2;

public partial class MessBox : Window
{
    // Una sola volta per tutta la classe
    //private MainViewModel? Vm => DataContext as MainViewModel;
    
    private readonly string[] iconPaths =
      [
          "avares://CopyFolders2/Assets/info.png",
          "avares://CopyFolders2/Assets/errore.png",
          "avares://CopyFolders2/Assets/warning.png"
      ];  
     
    // 1. Costruttore VUOTO (per il Designer/Anteprima)
    public MessBox()
    {
        InitializeComponent();
 
    }
    
    public MessBox(bool showYes, int imgIndex, string msg)
    {
        // 1. FONDAMENTALE: Carica lo XAML prima di toccare qualsiasi controllo
        AvaloniaXamlLoader.Load(this);
        
        // 2. Recupera i riferimenti ai controlli (per evitare il NullReference 806)
        var txtMsg = this.FindControl<TextBlock>("TxtMsg");
        var btnYes = this.FindControl<Button>("BtnYes");
        var btnClose = this.FindControl<Button>("BtnClose");
        var msgImg = this.FindControl<Image>("MsgImg");

        // 3. Recupera il ViewModel (se passato tramite DataContext o creane uno al volo)
        var vm = DataContext as MainViewModel ?? new MainViewModel();

        // --- INIZIO TUA LOGICA (Spostata da Opened al Costruttore) ---
        MyErr = 806;
        if (txtMsg != null) txtMsg.Text = msg;

        MyErr = 807;
        if (btnYes != null) btnYes.IsVisible = showYes;

        MyErr = 810;
        // Gestione immagine (usa try-catch per sicurezza all'avvio)
        try
        {
            if (msgImg != null)
            {
                Console.WriteLine(iconPaths[imgIndex]);
                msgImg.Source = new Bitmap(AssetLoader.Open(new(iconPaths[imgIndex])));

                // Debug: se arrivi qui, il bitmap è stato creato
                Console.WriteLine("Bitmap caricata con successo.");
            }
        }
        catch (Exception ex)
        {
            MyErr = 811; 
            Console.WriteLine($"Errore caricamento asset: {ex.Message}");
        }

        if (showYes)
        {
            MyErr = 814;
            if (btnYes != null) btnYes.Content = vm.Yes;
            if (btnClose != null) btnClose.Content = "No";
        }
        else
        {
            MyErr = 816;
            if (btnClose != null) btnClose.Content = vm.Close;
        }

        MyErr = 820;
        if (msg=="") msg = "?? no msg ??";
        int inx = msg.IndexOf('-', StringComparison.Ordinal);    
        
        if (msg.Contains('/'))
        {
            if (inx != -1)
            {
               string msg1 = msg.Substring(0, inx).Trim();
               string msg2 = msg.Substring(inx + 1).Trim();
               if (txtMsg != null)
                  txtMsg.Text = vm.CriticalError + msg1 + Environment.NewLine + msg2 +
                                Environment.NewLine + vm.WillClosed;
            }
            else txtMsg!.Text = msg;
        }
        else
        {
            if (inx != -1)
            {
                string msg1 = msg.Substring(0, inx).Trim();
                string msg2 = msg.Substring(inx + 1).Trim();
                if (txtMsg != null)
                    txtMsg.Text = vm.CriticalError + msg1 + Environment.NewLine + msg2;
            }
            else txtMsg!.Text = msg;  
        }
    }
    
    private void Yes_OnClick(object? sender, RoutedEventArgs e)
    {
        this.Close("Yes");
    }

    private void Close_OnClick(object? sender, RoutedEventArgs e)
    {
        this.Close("Close");
    }
}