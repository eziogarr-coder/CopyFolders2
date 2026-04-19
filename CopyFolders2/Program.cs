using System;
using Avalonia;
using System.Runtime.InteropServices;
using System.Threading;
using Avalonia.Controls.ApplicationLifetimes;
//using Avalonia.Dialogs; // Necessario per P/Invoke
using static CopyFolders2.GlobalVariables;

namespace CopyFolders2;

public static class Program
{
    [DllImport("user32.dll", CharSet = CharSet.Auto)]
    private static extern int MessageBox(IntPtr hWnd, String text, String caption, uint type);
    
    // Initialization code. Don't use any Avalonia, third-party APIs or any
    // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
    // yet and stuff might break.
    
    private static Mutex? _mutex; //non usato ma serve per il garbage
    
    [STAThread]
    public static void Main(string[] args)
    {
        try
        {
            //per evitare doppi running
            const string mutexName = @"Global\CopyFolders2_{G1B2C3F5}";
            _mutex = new(true, mutexName, out var createdNew);

            if (!createdNew)
            {
                // Già in esecuzione - 0x40 è l'icona di Info
                MessageBox(IntPtr.Zero, $"{Res.msg_Running}\r\n{Res.WillClosed}", 
                    "CopyFolders 2", 0x40);
                return;
            }
            
            BuildAvaloniaApp()
                .StartWithClassicDesktopLifetime(args);
        }
        catch (Exception ex)
        {
            // 1. Log su console (per il debug in Rider)
            Console.WriteLine("**************** Program.cs " + MyErr + " " + ex.Message);

            // 2. MOSTRA MESSAGGIO NATIVO PRIMA DELLO SHUTDOWN
            // Questo blocca l'esecuzione finché l'utente non preme OK
            MessageBox(IntPtr.Zero, 
                $"{Res.Unhandled}: {MyErr}\r\n{ex.Message}\r\n{Res.WillClosed}", 
                "CopyFolders 2", 
                0x10); 

            // 3. Ora che l'utente ha letto e chiuso il messaggio, forza l'uscita
            if (AppBuilder.Configure<App>().Instance?.ApplicationLifetime 
                is IClassicDesktopStyleApplicationLifetime desktop)
            {
                desktop.Shutdown(-1); 
            }
    
            // Sicurezza extra: se Avalonia non è inizializzato, termina il processo
            Environment.Exit(-1);
        }
    }

    // Configurazione di Avalonia (NON toccare il nome di questo metodo, serve al Designer di Rider)
    private static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .WithInterFont()
            .LogToTrace();
            //.UseManagedSystemDialogs();
}
