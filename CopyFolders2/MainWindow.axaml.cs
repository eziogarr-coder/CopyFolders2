using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Avalonia.Threading;
using Avalonia.VisualTree;
using static CopyFolders2.GlobalVariables;
using static CopyFolders2.Archivio;
using static CopyFolders2.CheckFoldersAndFiles;

namespace CopyFolders2;
//dotnet publish ./CopyFolders2/CopyFolders2.csproj -c Release -r win-x64 -p:PublishSingleFile=true -p:SelfContained=true -p:EnableCompressionInSingleFile=true -p:PublishTrimmed=true 
//se la cartella è già quella del csproj:
//dotnet publish CopyFolders2.csproj -c Release -r win-x64 -p:PublishSingleFile=true -p:SelfContained=true -p:EnableCompressionInSingleFile=true -p:PublishTrimmed=true -p:IncludeNativeLibrariesForSelfExtract=true
public partial class MainWindow : Window
{
    //private Label[] lblNumbers;
    private readonly CheckBox[] chkChks; //= new CheckBox[10];
    private readonly TextBox[] txtMasks; //= new TextBox[10];
    private readonly Label[] lblStartPaths; //= new Label[10];
    private readonly Label[] lblDestPaths; //= new Label[10];
    private readonly CheckBox[] chkTopOnly; //=new CheckBox[10];
    private readonly ComboBox[] cmbNumBackups; // = new ComboBox[10];  
    private readonly Image[] imgAdds;

    //private Point Startup;
    // private int LessDays, VisibleRows;
    // private bool CheckDates, CheckUpdates, ShowWelcome;   

    //private bool DaCompattare;
    private bool stoPartendo;
    private bool isClosingAllowed;
    private bool altPressed;

    private DateTime tick = DateTime.Now;
    private string flg = ""; //per errore
    //private static int Cnt;

    private readonly string crLf = Environment.NewLine;

    // Una sola volta per tutta la classe
    //private MainViewModel? Vm => DataContext as MainViewModel;
    //public ObservableCollection<string> LogLines { get; } = new ObservableCollection<string>();

    public MainWindow()

    {
        InitializeComponent();

        // Colleghiamo la collezione alla ListBox
        /*LogList.ItemsSource = LogLines;

        // AUTO-SCROLL: Ogni volta che aggiungiamo una riga, scorre alla fine
        ((INotifyCollectionChanged)LogLines).CollectionChanged += (s, e) =>
        {
            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                LogList.ScrollIntoView(LogLines[^1]); // Vai all'ultimo elemento
            }
        }; */

        stoPartendo = true;
        //per cambio lingua
        DataContext = new MainViewModel();

        // Aggiunge un gestore che intercetta i tasti prima dei controlli figli
        this.AddHandler(KeyDownEvent, Window_PreviewKeyDown, RoutingStrategies.Tunnel, true);
        this.AddHandler(KeyUpEvent, Window_PreviewKeyUp, RoutingStrategies.Tunnel);

        CultureInfo.DefaultThreadCurrentCulture = CultureInfo.CurrentCulture;
        CultureInfo.DefaultThreadCurrentUICulture = CultureInfo.CurrentCulture;

        //lblNumbers = { lblNum1, lblNum1, lblNum2, lblNum3, lblNum4, lblNum5, lblNum6, lblNum7, lblNum8, lblNum9 };
        chkChks = [ChkC1, ChkC1, ChkC2, ChkC3, ChkC4, ChkC5, ChkC6, ChkC7, ChkC8, ChkC9];
        txtMasks = [Mask1, Mask1, Mask2, Mask3, Mask4, Mask5, Mask6, Mask7, Mask8, Mask9];
        lblStartPaths = [LblStart1, LblStart1, LblStart2, LblStart3, LblStart4, LblStart5, LblStart6, LblStart7, LblStart8, LblStart9];
        lblDestPaths = [LblDest1, LblDest1, LblDest2, LblDest3, LblDest4, LblDest5, LblDest6, LblDest7, LblDest8, LblDest9];
        cmbNumBackups = [CmbBk1, CmbBk1, CmbBk2, CmbBk3, CmbBk4, CmbBk5, CmbBk6, CmbBk7, CmbBk8, CmbBk9];
        chkTopOnly = [ChkTo1, ChkTo1, ChkTo2, ChkTo3, ChkTo4, ChkTo5, ChkTo6, ChkTo7, ChkTo8, ChkTo9];
        imgAdds = [ImgAdd1, ImgAdd1, ImgAdd2, ImgAdd3, ImgAdd4, ImgAdd5, ImgAdd6, ImgAdd7, ImgAdd8, ImgAdd9];

        //riempio i combobox
        var nums = Enumerable.Range(0, 7).ToList();
        var numeri = Enumerable.Range(1, 9).ToList();
        foreach (var cm in cmbNumBackups)
        {
            cm.ItemsSource = nums;
            //cm.SelectedIndex = nums[0];
        }

        CmbRows.ItemsSource = numeri;
        //CmbRows.SelectedItem = numeri[8];
        TxtFiles.Text = Lbl.PressStart;

        BtnStart.Focusable = true;
        BtnCopy.Focusable = false;
    }
    // fine delle implementazioni

    private async void Window_OnClosing(object? sender, WindowClosingEventArgs e)
    {
        try
        {
            //ritorno dopo errore rilevato
            if (isClosingAllowed) return;
            MyErr = 50;  
            LessDays = (DateTimeTo.SelectedDate - DateTimeFrom.SelectedDate)?.Days ?? 0;
            Startup = this.Position;
            GridHeight = MainGrid.RowDefinitions[10].ActualHeight;

            Console.WriteLine("Salvo GridHeight al valore: " + GridHeight.ToString(CultureInfo.InvariantCulture));
            var md = new MyData();
            var msg = md.SaveSettings();
            if (msg.Length > 4 && !isClosingAllowed) //no due volte messaggio di errore
            {
                Console.WriteLine(" ----------- Errore nel salvataggio dati!!!!!");
                e.Cancel = true;
                await Messaggio(false, 1, msg);
                isClosingAllowed = true;
                this.Close();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("Window_OnClosing: " + ex.Message);
            await Messaggio(false, 1, $"{MyErr} - {ex.Message}");
        }
    }

    private async void TopLevel_OnOpened(object? sender, EventArgs e)
    {
        // Mettere in App.axaml = Minimize per evitare eventuale flash bianco 
        //this.WindowState =WindowState.Normal;
        /*if (Startup.X < 0 && Startup.Y < 0)  //messo in app.axaml.cs
        {
            this.WindowStartupLocation = WindowStartupLocation.CenterScreen;
        }
        else
        {
            this.WindowStartupLocation = WindowStartupLocation.Manual;
            this.Position = new PixelPoint(Startup.X, Startup.Y);
        }*/

        /*string a = @"C:\foo\";
        string b = @"\\fooe\fgg";
        string c = Path.Combine(a, b.TrimStart(Path.DirectorySeparatorChar));
        Console.Write(c);
        string basename = Path.GetFileName(@"E:\".TrimEnd(Path.DirectorySeparatorChar));*/
        try
        {
            MyErr = 60;
            flg = await AggiornaControlli();

            //è l' equivalente di FormLoad in vb  
            //c'è errore in Aggiorna
            if (flg.Length > 4 && !isClosingAllowed)
            {
                Console.WriteLine(" ----------- Errore in aggiornamento dati!!!!!");
                await Messaggio(false, 1, flg);
                isClosingAllowed = true;
                this.Close();
            }

            MyErr = 62;
            //Console.WriteLine(Res.Error);
            //carico le impostazioni in App.axaml.cs, prima che si formi la finestra

            Console.WriteLine("VisibleRows in Load è rilevato come: " + VisibleRows.ToString());
            Console.WriteLine("-***************----------- Path1: " + StartPaths[1]);
            MyErr = 65;

            // --- SECONDA PASSATA (Il momento del ripristino) ---
            // 1. Rimettiamo i bordi
            //this.SystemDecorations = SystemDecorations.Full;
            //this.Width = 1094; 

            if (Startup is { X: < 0 } or { Y: < 0 })
            {
                // Almeno una coordinata è fuori limite (ma Startup esiste)
                this.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            }
            else
            {
                this.WindowStartupLocation = WindowStartupLocation.Manual;
                this.Position = new(Startup.X, Startup.Y);
            }

            await CheckFolders();

            // 1. Aspetta un istante che il buffer video sia pronto
            await Task.Delay(100);
            // e ora l'opacità per visualizzarla completamente senza sfarfallii (è sata settata a 0 nell' axaml)
            this.Width = 1094;
            this.Opacity = 1;
        }
        catch (Exception ex)
        {
            Console.WriteLine("In TopLevel_OnOpened: " + ex.Message);
            await Messaggio(false, 1, $"{MyErr} - {ex.Message}");
        }
    }

    private void Control_OnLoaded(object? sender, RoutedEventArgs e)
    {
        MyErr = 70;
        // equivale al form.Shown di vb
        stoPartendo = false;
        Console.WriteLine("Partito !!! ");
        if (CheckUpdates)
        {
            MyErr = 71;
            // Chiamiamo il metodo asincrono senza bloccare la UI
            _ = CheckIfUpdatesAsync(); 
        }
    }
 
    private async void BtnStart_OnClick(object? sender, RoutedEventArgs e)
    {
        try
        {
            int totaleFile=0;
            try
            {
                //per evitare un doppio click
                if ((DateTime.Now - tick).TotalMilliseconds < 800)
                {
                    Console.WriteLine("Doppio click!!");
                    tick = DateTime.MinValue;
                    return;
                }

                tick = DateTime.Now;
                Console.WriteLine("Nessun doppio click!!");

                if (BtnCopy.Focusable) //bocce ferme, basta poco delay
                {
                    await ActEscape(100);
                    return;
                }

                if (!BtnStart.Focusable)
                {
                    StopCopy = true;
                    //await ActEscape(300);  
                    return;
                }

                StopCopy = false;
                pressedEscape = false;
                TxtFiles.Clear();
                MyErr = 112;

                //controllo che vi siano righe selezionate
                bool flag = false;
                foreach (CheckBox chk in chkChks)
                {
                    if ((bool)chk.IsChecked !)
                    {
                        flag = true;
                        break;
                    }
                }

                if (!flag)
                {
                    TxtFiles.Text = Lbl.PressStart; //devo rimetterlo ;)      
                    await Messaggio(false, 0, Res.AlmostOne);
                    return;
                }

                // Definisci l'azione da compiere ogni volta che la classe esterna chiama Report()
                var progressReporter = new Progress<string>(messaggio =>
                {
                    if (messaggio == ".") // && LogLines.Count > 0)
                    {
                        Append(messaggio, false);
                    }
                    else Append(messaggio, true);
                });

                ReadErrors.Clear();
                SkippedFies.Clear();
                TotalSize = 0;
                BtnStart.Focusable = false;

                MyErr = 114;
                for (int i = 0; i < 10; i++)
                {
                    AccessibleFiles[i].Clear();
                    FilesToCopy[i].Clear();
                }

                //Append("mom please..."+ crLf + Lbl.Trattini);
                var check = new CheckFoldersAndFiles();
                await check.ScriviTitolo(1);

                for (int x = 1; x <= VisibleRows; x++)
                {
                    if (StopCopy) return;

                    //Console.WriteLine("+++++++++++++378 " + lblStartPaths[x].Content + "-" + txtMasks[x].Text);

                    if (chkChks[x].IsChecked ?? false)
                    {
                        if (StopCopy) return;

                        if (StartPaths[x].Length > 0 && !Directory.Exists(StartPaths[x]))
                        {
                            lblStartPaths[x].Background = Brushes.LightSalmon;
                            await Messaggio(false, 2, Res.msg_DontExist);
                            chkChks[x].IsChecked = false;
                            return;
                        }

                        RowDirs = 1;
                        RowFileSize = 0;
                        var partialTime = DateTime.Now;
                        //va a capo solo ce c' è già testo
                        bool aCapo = TxtFiles.Text != "";
                        Append($"{Res.txtRowN} {x} ({Res.txtStartingPath}: {StartPaths[x]})...", aCapo);

                        if (Masks[x] == "")
                        {
                            Masks[x] = Res.Mask;
                            txtMasks[x].Text = Res.Mask;
                        }

                        await check.CheckFiles(StartPaths[x], x, Masks[x], progressReporter);
                        if (StopCopy) return;

                        Append($" {Res.txtDoneIn} {(DateTime.Now - partialTime).TotalSeconds.ToString("N2", CultureInfo.CurrentCulture)} {Res.txtSeconds}",
                            false);
                        Append($"{Res.txtFound} {AccessibleFiles[x].Count.ToString("N0", CultureInfo.CurrentCulture)}" +
                               $" {Res.txtAccessibleFiles} {RowDirs.ToString("N0", CultureInfo.CurrentCulture)} {Res.txtAccessibleDir} " +
                               $"{(RowFileSize / (1024.0 * 1024.0)).ToString("N2", CultureInfo.CurrentCulture)} MB.", true);
                        //My.Resources.BWSummary.BW
                        Append($"{Res.txtSkipping}.", true);

                        await check.SkipExistentsAsync(x, progressReporter);
                        Append(
                            $" {Res.txtDoneIn} {(DateTime.Now - partialTime).TotalSeconds.ToString("N2", CultureInfo.CurrentCulture)} {Res.txtTotSeconds}",
                            false);
                        Append($"{Res.txtFound} {FilesToCopy[x].Count.ToString("N0", CultureInfo.CurrentCulture)}" +
                               $" {Res.txtNewSize} {(RowFileSize / (1024.0 * 1024.0)).ToString("N2", CultureInfo.CurrentCulture)} MB.",
                            true);

                        TotalSize += RowFileSize;
                        /*if (ReadErrors.Count > 0) //spostato nel LOG
                    {
                        Append($"{Lbl.Trattini}{crLf}{Res.txtErrList} {x}", true);
                        foreach (var errs in ReadErrors)
                        {
                            if (StopCopy) break;
                            Append(errs,true);
                            await Task.Delay(50);
                        }
                    }
                    else
                    {
                        Append($"{Lbl.Trattini}{crLf}{Res.txtNoErrList} {x}", true);
                    }*/

                        Append(Lbl.Trattini, true);
                        //ReadErrors.Clear();
                    }
                }

                //per ognuna delle 9 matrici di AccessibleFiles[] conto i singoli file trovati
                MyErr = 116;
                if (!StopCopy)
                {
                    foreach (var t in FilesToCopy)
                    {
                        totaleFile += t.Count;
                    }

                    if (totaleFile > 0)
                    {
                        Append(
                            $"{Res.txtTotalFound} {totaleFile.ToString("N0", CultureInfo.CurrentCulture)} {Res.txtNewTotSize} " +
                            $"{(TotalSize / (1024.0 * 1024.0)).ToString("N2", CultureInfo.CurrentCulture)} MB.{crLf}{Res.txtPressOpen}. {Res.txtPressOpen2}. ",
                            true);
                    }
                    else
                    {
                        Append(
                            $"{Res.txtTotalFound} {totaleFile.ToString("N0", CultureInfo.CurrentCulture)} {Res.txtNewNoSize}." +
                            $" {Res.txtNoFileToCopy}.{crLf}{Res.txtPressOpen}. {Res.txtPressOpen2}.", true);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                ReadErrors.Add(ex.Message);
                Append($"{Res.Error} {MyErr} -{ex.Message}", true);
            }
            finally
            {
                if (!StopCopy)
                {
                    if (totaleFile > 0)
                    {
                        BtnCopy.Focusable = true;
                        BtnStart.Focusable = false;
                        Bar.Maximum = TotalSize; // AccessibleFiles.Sum(lista => lista.Count);
                        Append(Lbl.Trattini + crLf + Lbl.PressCopy, true);
                    }
                    else
                    {
                        BtnCopy.Focusable = false;
                        BtnStart.Focusable = true;
                        Bar.IsVisible = false;
                        Bar.Maximum = 0; // AccessibleFiles.Sum(lista => lista.Count);
                        Append(Lbl.Trattini + crLf + Lbl.PressStart, true);
                    }
                }
                else
                {
                    if (!pressedEscape) await ActEscape(400);
                }
            }

            try
            {
                MyErr = 118;
                await Task.Run(async () =>
                {
                    // Il parametro 'true' dice a StreamWriter di AGGIUNGERE alla fine del file, non sovrascrivere
                    await using StreamWriter sw = new StreamWriter(LogPath, append: true);

                    await sw.WriteLineAsync(Lbl.Trattini);
                    await sw.WriteLineAsync(" "); // Una riga vuota di separazione per chiarezza
                    await sw.WriteLineAsync($"----- {Res.logGotErrors} -----");

                    if (ReadErrors.Count > 0)
                    {
                        foreach (var errore in ReadErrors)
                        {
                            await sw.WriteLineAsync(errore);
                        }
                    }
                    else await sw.WriteLineAsync(Res.logNoReadErr);

                    await sw.WriteLineAsync(Lbl.Trattini);
                    await sw.WriteLineAsync(" "); // Una riga vuota di separazione per chiarezza
                    await sw.WriteLineAsync($"----- {Res.logSkippedList} -----");
                    if (SkippedFies.Count > 0)
                    {
                        foreach (var skip in SkippedFies)
                        {
                            await sw.WriteLineAsync($"{Res.logSkipped}: {skip}");
                        }
                    }
                    else await sw.WriteLineAsync(Res.logNoSkipped);
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                ReadErrors.Add(ex.Message);
                Append($"{Res.Error} {MyErr} -{ex.Message}", true);
            }
        }
        catch (Exception exs)
        {
            Console.WriteLine(exs);
            ReadErrors.Add(exs.Message);
            Append($"{Res.Error} {MyErr} -{exs.Message}", true);
        }
    }

    private async void BtnCopy_OnClick(object? sender, RoutedEventArgs e)
    {
        try
        {
            MyErr = 120;
            if (BtnStart.Focusable)
                return;
            if (!BtnCopy.Focusable)
                return;

            pressedEscape = false;
            ReadErrors.Clear();
            Bar.IsVisible = true;
            BlockPercent.IsVisible = true;
            BtnCopy.Focusable = false;
            TotalBytesCopied = 0;
            StopCopy = false;

            Append($"{Lbl.Trattini}{crLf}{Res.txtStartCopy}...", true);

            // Ricevitore TESTO
            var textReporter = new Progress<string>(msg =>
            {
                if (msg == ".") //)&& LogLines.Count > 0)
                {
                    Append(msg, false); // LogLines[LogLines.Count - 1] += msg; // Modifica l'ultimo elemento
                }
                else Append(msg, true);
            });

            //Ricevitore avanzamento barra
            var progressIndicator = new Progress<long>(value =>
            {
                Bar.Value = value; // Questo viene eseguito sul thread UI
                BlockPercent.Text = (value / Bar.Maximum).ToString("P2", CultureInfo.CurrentCulture);
            });

            var copy = new CheckFoldersAndFiles();
            await copy.ScriviTitolo(2);

            MyErr = 122;
            var stopwatch = Stopwatch.StartNew();
            int fileTotali = 0;

            //devo dichiararla perché vado su un altro thread e non posso dichiararci chkChks
            bool[] checks = new bool[10];
            for (int x = 1; x <= 9; x++)
            {
                checks[x] = (bool)chkChks[x].IsChecked!;
                fileTotali += FilesToCopy[x].Count;
            }

            await Task.Run(async () =>
            {
                //Append($" no files to copy in the row {x}", false);
                FileCopiati = 0;
                MyErr = 123;

                for (int x = 1; x <= VisibleRows; x++)
                {
                    if (StopCopy) break; //************da rivedere

                    if (checks[x])
                    {
                        // Qui siamo già in un thread separato (grazie al Task.Run sopra)
                        await Task.Run(() =>
                            copy.CopyFolderContentsAsync(x, textReporter, progressIndicator, stopwatch));
                    }
                    //else
                    //{
                    //if (checks[x]) //************ da togliere????? Si, perché hi già scremato e qui non ci arrivo
                    //Append($" no files to copy in the row {x}", false);
                    //}
                }
            });

            MyErr = 125;
            if (!StopCopy)
            {
                Append(
                    $" {Res.txtDoneOut} {stopwatch.Elapsed.TotalSeconds.ToString("N2", CultureInfo.CurrentCulture)} {Res.txtSeconds}",
                    false);

                if (ReadErrors.Count > 0)
                {
                    Append($"{Lbl.Trattini}{crLf}**** {Res.txtCopyinErr}:", true);
                    foreach (var errs in ReadErrors)
                    {
                        Append(errs, true);
                    }
                }
                else
                {
                    Append($"{Lbl.Trattini}{crLf}**** {Res.txtNoCopyErr}:", true);
                }

                Append(Lbl.Trattini, true);
                Append($"{Res.txtCopied} {FileCopiati.ToString("N0", CultureInfo.CurrentCulture)} {Res.txtNewNoSize} " +
                       $"{Res.txtOver} {fileTotali.ToString("N0", CultureInfo.CurrentCulture)}.{crLf}{Res.txtPressOpen}.",
                    true);
                //se StopCopy il messaggio è in Esc
            }
            else
            {
                Append($"{Res.txtCopied} {FileCopiati.ToString("N0", CultureInfo.CurrentCulture)} {Res.txtNewNoSize} " +
                       $"{Res.txtOver} {fileTotali.ToString("N0", CultureInfo.CurrentCulture)}.{crLf}{Res.txtPressOpen}.",
                    true);
                if (!pressedEscape) await ActEscape(400);
                stopwatch.Stop();
                return; //tutta la manfrina sotto è già in ActEscape
            }

            stopwatch.Stop();
            ReadErrors.Clear();
            SkippedFies.Clear();
            BtnStart.Focusable = true;
            BtnCopy.Focusable = false;
            await Task.Delay(1000);
            Bar.IsVisible = false;
            BlockPercent.IsVisible = false;
            Append(Lbl.Trattini + crLf + Lbl.PressStart, true);
            //fine BtnCopy    
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            ReadErrors.Add(ex.Message);
            Append($"{Res.Error} {MyErr} -{ex.Message}", true);
        }
    }

    private void Append(string messaggio, bool aCapo)
    {
        string finale = aCapo ? Environment.NewLine + messaggio : messaggio;
        // Sposta l'esecuzione sul thread della UI
        Dispatcher.UIThread.Post(() =>
        {
            MyErr = 128;
            TxtFiles.Text += finale;
            TxtFiles.CaretIndex = TxtFiles.Text.Length;
        });
    }

    private async void BtnClose_OnClick(object? sender, RoutedEventArgs e)
    {
        try
        {
            MyErr = 129;
            StopCopy = true;
            await Task.Delay(500);
            Close();
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            ReadErrors.Add(ex.Message);
            Append($"{Res.Error} {MyErr} -{ex.Message}", true);
        }
    }

    private bool pressedEscape;

    private async void Window_PreviewKeyDown(object? sender, KeyEventArgs e)
    {
        try
        {
            //uso del tasto Esc
            MyErr = 130;
            if (e.Key == Key.LeftAlt)
            {
                altPressed = true;
                for (int x = 1; x <= VisibleRows; x++)
                    imgAdds[x].Source = new Bitmap(AssetLoader.Open(new Uri("avares://CopyFolders2/Assets/less.png")));
            }
            else if (e.Key == Key.Escape)
            {
                // Logica globale per il tasto F5
                Console.WriteLine("---------Tasto Esc intercettato dalla finestra");
                // Se vuoi impedire che il controllo figlio riceva l'evento:
                e.Handled = true;
                pressedEscape = true;
                await ActEscape(400);
                return;
            }
            Console.WriteLine($"Tasto premuto: {e.Key}");
            //fine KeyPreview
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            ReadErrors.Add(ex.Message);
            Append($"{Res.Error} {MyErr} -{ex.Message}", true);
        }
    }

    private void Window_PreviewKeyUp(object? sender, KeyEventArgs e)
    {
        Console.WriteLine($"Tasto rilasciato: {e.Key}");
        if (e.Key == Key.LeftAlt)
        {
            altPressed = false;
            for (int x = 1; x <= VisibleRows; x++)
                imgAdds[x].Source = new Bitmap(AssetLoader.Open(new Uri("avares://CopyFolders2/Assets/add.png")));
        }
    }

    public async Task ActEscape(int delay)
    {
        MyErr = 131;
        StopCopy = true;
        BtnStart.Focusable = true;
        BtnCopy.Focusable = false;
        await Task.Delay(delay);
        if (delay != 200) //200: premuta checkbox
            Append(
                $"{Lbl.Trattini}{Lbl.Trattini}{crLf}{Lbl.aborted}{crLf}{Lbl.Trattini}{Lbl.Trattini}{crLf}{Lbl.PressStart}",
                true);
        else Append(Lbl.PressStart, false);
        await Task.Delay(1000);
        Bar.IsVisible = false;
        BlockPercent.IsVisible = false;
        pressedEscape = false;
    }

    private Task<string> AggiornaControlli()
    {
        try
        {
            // CheckBoxDate.Checked -> IsChecked (Nullable bool in Avalonia)
            Console.WriteLine("Sono in aggiornaControlli con VisibleRows: " + VisibleRows);

            int x = 22;
            try
            {
                LblDates.Content = Lbl.Dates;
                ChkCheckDates.IsChecked = CheckDates;
                MyErr = 132;
                // DateTimePicker1.Value -> SelectedDate (DateTimeOffset? in Avalonia CalendarDatePicker)
                // DateTime.Now.AddDays(-LessDays) deve essere convertito in DateTimeOffset
                DateTimeTo.SelectedDate = DateTime.Now;
                DateTimeFrom.SelectedDate = DateTime.Now.AddDays(-LessDays);
                DateFrom = DateTime.Now.AddDays(-LessDays);
                DateTo = DateTime.Now;

                MyErr = 133;
                ChkHidden.IsChecked = AlsoHidden;
                ChkSystem.IsChecked = AlsoSystem;

                MyErr = 134;
                // cmbRows.Text (Le ComboBox in Avalonia preferiscono SelectedItem o SelectedIndex)
                Console.WriteLine("VisibleRows in Aggiorna: " + VisibleRows + " -GrihHeight: " + GridHeight);
                CmbRows.SelectedItem = VisibleRows;

                MyErr = 136;
                //spostato in grid text changed
                //var row = MainGrid.RowDefinitions[10];
                //row.Height = new(GridHeight, GridUnitType.Star);           

                MyErr = 138;
                for (x = 1; x <= 9; x++)
                {
                    // Assumendo che i controlli siano in array/liste (tbxMasks, lblStartPaths, etc.)
                    //Console.WriteLine("In aggiorna: Masks(" +x + "):" + Masks[x]);
                    txtMasks[x].Text = Masks[x];
                    MyErr = 140;
                    lblStartPaths[x].Content = StartPaths[x]; // I Label in Avalonia usano Content
                    // In Avalonia i ToolTip si impostano tramite la classe statica ToolTip
                    if (StartPaths[x].Length > 0)
                        ToolTip.SetTip(lblStartPaths[x],
                            StartPaths[x]); //path completa, che la label potrebbe non contenere tutta
                    MyErr = 142;
                    lblDestPaths[x].Content = DestPaths[x];
                    if (DestPaths[x].Length > 0)
                        ToolTip.SetTip(lblDestPaths[x], DestPaths[x]);
                    MyErr = 144;
                    //Console.WriteLine("Aggiorno: cmbBackups(" +x + "): " + NumBackups[x]);
                    cmbNumBackups[x].SelectedIndex = Convert.ToInt32(NumBackups[x]);
                    //Console.WriteLine(cmbNumBackups[x].Text);
                    MyErr = 146;
                    //throw new Exception("pppppppppppppppp");
                    // Conversione booleana per IsChecked
                    //Console.WriteLine("Aggiorno: chkTo(" +x + "):" + Convert.ToBoolean(TopOnlys[x]));
                    chkTopOnly[x].IsChecked = Convert.ToBoolean(TopOnlys[x]);
                }

                return Task.FromResult(string.Empty);
            
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return Task.FromResult($"{MyErr}/{x} - {e.Message}");
            }
            //fine AggiornaControlli
        }
        catch (Exception exception)
        {
            return Task.FromException<string>(exception);
        }
    }

    private async Task Messaggio(bool show, int indx, string messaggio)
    {
        //genera i messaggi di avviso o errore
        MyErr = 148;
        var inf = new MessBox(show, indx, messaggio)
        {
            // Assegna il DataContext della finestra corrente alla nuova finestra
            DataContext = this.DataContext
        };
        await inf.ShowDialog<string>(this);
    }

    private async void TextBox_OnTextChanged(object? sender, TextChangedEventArgs e)
    {
        // 1. ESCO SE L'ARRAY NON È ANCORA STATO CREATO       
        try
        {
           if (stoPartendo) return;            
            //Console.Write("In TextBoxChanged: " + sender?.ToString());
            MyErr = 150;
            if (e.Source is TextBox { Name: not null } tb)
            {
                //elimino click nella txtFiles
                if (!tb.Name.StartsWith("Mask", StringComparison.OrdinalIgnoreCase))
                {
                    return;
                }

                // Recupero l'indice tramite la proprietà Tag o il Content
                if (tb.Tag == null)
                {
                    Console.WriteLine("tb.tag è nullo!");
                    return;
                }

                MyErr = 152;
                tb.Text ??= "*";

                if (tb.Text.Length > 1 && tb.Text.Trim().StartsWith("!="))
                {
                    if (tb.Text.EndsWith('*') || tb.Text.EndsWith('?'))
                    {
                        await Messaggio(false, 2, Res.msg_NoWildCards);
                        tb.Text = tb.Text.Trim().TrimEnd('*', '?');
                        return;
                    }
                }

                // Recupero l'indice tramite la proprietà Tag o il Content
                int index = int.Parse(tb.Tag.ToString()!);
                //Console.WriteLine("In Txt variato testo con indice: " + index + " -Nuovo testo: " + tb.Text);          
                txtMasks[index].Text = tb.Text.Trim();
                Masks[index] = tb.Text.Trim();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("In TextBox_OnTextChanged: " + ex.Message);
            await Messaggio(false, 1, $"{MyErr} - {ex.Message}");
        }
    }

    private async void SelectingItemsControl_OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        // 1. ESCO SE L'ARRAY NON È ANCORA STATO CREATO
        try
        {
            if (stoPartendo) return;
            //specifico per le combo del numero di backup
            MyErr = 154;          
            
            if (e.Source is ComboBox { Name: not null, Tag: not null } cb)
            {
                if (!cb.Name.StartsWith("CmbBk", StringComparison.OrdinalIgnoreCase))
                {
                    return;
                }

                // Recupero l'indice tramite la proprietà Tag o il Content
                if (cb.Tag == null)
                {
                    Console.WriteLine("cb.tag è nullo!");
                    return;
                }

                int index = int.Parse(cb.Tag!.ToString()!);
                cb.Text ??= "0";
                // Console.WriteLine("In Combo variato testo con indice: " + index + " -Nuovo testo: " + cb.Text);
                MyErr = 155;
                cmbNumBackups[index].SelectedIndex = Convert.ToInt32(cb.Text);
                MyErr = 156;
                NumBackups[index] = cb.Text; //********** da rivedere??
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("In Control_OnSelectionChanged: " + ex.Message);
            await Messaggio(false, 1, $"{MyErr} - {ex.Message}");
        }
    }

    private async void MainGrid_OnClick(object? sender, RoutedEventArgs e)
    {
        try
        {
            // 1. ESCO SE L'ARRAY NON È ANCORA STATO CREATO
            if (stoPartendo || ChkCheckDates == null) return;
            
            MyErr = 160;    
            if (e.Source is CheckBox { Name: not null, Tag: not null } ch)
            {
                //throw new Exception("ppppppppppp");
                //! vuol dire Not
                if (ch.Name.StartsWith("ChkChe", StringComparison.OrdinalIgnoreCase))
                {
                    MyErr = 161;
                    ChkCheckDates.IsChecked = ch.IsChecked;
                    MyErr = 162;
                    bool flag = ch.IsChecked ?? false;
                    CheckDates = flag;
                }
                else if (ch.Name.StartsWith("Chkc", StringComparison.OrdinalIgnoreCase))
                {
                    MyErr = 164;
                    int index = int.Parse(ch.Tag.ToString()!);
                    MyErr = 165;
                    if (Equals(lblStartPaths[index].Background, Brushes.LightSalmon))
                    {
                        chkChks[index].IsChecked = false;
                        await Messaggio(false, 2, Res.msg_CantCheck);
                        return;
                    }
                    if (string.IsNullOrWhiteSpace(StartPaths[index]) || string.IsNullOrWhiteSpace(DestPaths[index]))
                    {
                        chkChks[index].IsChecked = false;
                        // Mostra il tuo avviso
                        await Messaggio(false, 2, Res.msg_BothEmpty);
                    }
                    else
                    {
                        chkChks[index].IsChecked = ch.IsChecked ?? false;
                    }
                    if (!BtnStart.Focusable) await ActEscape(100);
                }
                else if (ch.Name.StartsWith("ChkT", StringComparison.OrdinalIgnoreCase))
                {
                    MyErr = 166;
                    int index = int.Parse(ch.Tag.ToString()!);
                    MyErr = 167;
                    if (string.IsNullOrWhiteSpace(StartPaths[index]) || string.IsNullOrWhiteSpace(DestPaths[index]))
                    {
                        chkTopOnly[index].IsChecked = false;
                        TopOnlys[index] = false;                        
                        // Mostra il tuo avviso
                        await Messaggio(false, 2, Res.msg_BothEmpty);
                    }
                    else
                    {
                        MyErr = 168;
                        chkTopOnly[index].IsChecked = ch.IsChecked ?? false;
                        TopOnlys[index] = ch.IsChecked ?? false;
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("In MainGrid_OnClick: " + ex.Message);
            await Messaggio(false, 1, $"{MyErr} - {ex.Message}");
        }
    }

    private async Task<bool> CheckBoth(int index)
    {
        if ((string)lblDestPaths[index].Content! == "" || (string)lblStartPaths[index].Content! == "")
        {
            await Messaggio(false, 2, Res.msg_BothEmpty);
            return false;
        }

        return true;
    }

    private async void MainGrid_OnTapped(object? sender, TappedEventArgs e)
    {
        //per rilevare le immagini cliccate
        //Console.WriteLine("++++++++++++++++ Source: " + e.Source);
        try
        {
            if (stoPartendo) return;            
            MyErr = 180;
            if (e.Source is Image { Name: not null, Tag: not null } img)
            {
                Console.WriteLine("Ho premuto su una immagine");

                if (img.Tag == null)
                {
                    Console.WriteLine("img.tag è nullo!");
                    return;
                }

                //throw new Exception("ppppppppppp");
                MyErr = 182;
                int index = int.Parse(img.Tag.ToString()!);

                if (img.Name.StartsWith("PbSta", StringComparison.OrdinalIgnoreCase))
                {
                    await OpenFolder(1, index);
                }
                else if (img.Name.StartsWith("PbDes", StringComparison.OrdinalIgnoreCase))
                {
                    await OpenFolder(2, index);
                }
                else if (img.Name.StartsWith("PbDelS", StringComparison.OrdinalIgnoreCase))
                {
                    lblStartPaths[index].Content = "";
                    StartPaths[index] = "";
                    ToolTip.SetTip(lblStartPaths[index], null);
                    lblStartPaths[index].Background = Brushes.WhiteSmoke;
                    AzzeraChecks(index);
                    if (!BtnStart.IsEnabled)
                        await ActEscape(100);
                }
                else if (img.Name.StartsWith("PbDelD", StringComparison.OrdinalIgnoreCase))
                {
                    lblDestPaths[index].Content = "";
                    DestPaths[index] = "";
                    ToolTip.SetTip(lblDestPaths[index], null);
                    lblDestPaths[index].Background = Brushes.WhiteSmoke;
                    AzzeraChecks(index);
                    if (!BtnStart.IsEnabled)
                        await ActEscape(100);
                }
                else if (img.Name.StartsWith("ImgA", StringComparison.OrdinalIgnoreCase))
                {
                    if (!await CheckBoth(index)) return;
                    string basename = Path.GetFileName(StartPaths[index].TrimEnd(Path.DirectorySeparatorChar));
                    if (basename == "")
                    {
                        await Messaggio(false, 0, Res.msg_NoBasename);
                        return;
                    }

                    if (!altPressed)
                    {
                       DestPaths[index] = Path.Combine(DestPaths[index].TrimEnd(Path.DirectorySeparatorChar),
                           basename + Path.DirectorySeparatorChar);
                       lblDestPaths[index].Content = DestPaths[index];
                       if (!Directory.Exists(DestPaths[index])) Directory.CreateDirectory(DestPaths[index]);
                       ToolTip.SetTip(lblDestPaths[index], lblDestPaths[index].Content?.ToString() ?? "");                     
                    }
                    else
                    {
                        var i = DestPaths[index].LastIndexOf(basename, StringComparison.Ordinal);
                        if (i <= 1) return;
                        DestPaths[index] = DestPaths[index].Substring(0,i);
                        if (!DestPaths[index].EndsWith(Path.DirectorySeparatorChar))
                            DestPaths[index] += Path.DirectorySeparatorChar;
                        lblDestPaths[index].Content = DestPaths[index];
                    }
                }
                else
                {
                    MyErr = 186;
                    Console.WriteLine("Il mome dell immagine non torna!! !" + img.Name);
                    throw new("??? " + img.Name);
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("In MainGrid_OnTapped: " + ex.Message);
            await Messaggio(false, 1, $"{MyErr} - {ex.Message}");
        }
    }

    private void CmbRows_OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (CmbRows.Text == null) return; //no 'StoPartendo' o la finestra si apre senza aver elaborato le altezze
        MyErr = 190;
        //specifico per variare la visibiltà a seconda del numero di Row impostate

        Console.WriteLine("Sono in CmbRows con valore:  " + int.Parse(CmbRows.Text));
        Console.WriteLine("Entro in cmbRow con GridHeight: " + GridHeight);

        RowDefinition row;
        int x;

        VisibleRows = int.Parse(CmbRows.Text);

        if (VisibleRows < 9)
        {
            //rendo invisibili le righe non selezionate
            for (x = VisibleRows + 1; x <= 9; x++)
            {
                var currentRow = x; // doppione per evitare Captured variable is modified in the outer scope
                row = MainGrid.RowDefinitions[currentRow];
                row.Height = new(0);
                row.MinHeight = 0;

                var myGrid = this.FindControl<Grid>("MainGrid");
                if (myGrid != null)
                {
                    var controlsInRow = myGrid.Children
                        .Where(c => Grid.GetRow(c) == currentRow);
                    foreach (Control control in controlsInRow)
                    {
                        control.IsVisible = false;
                    }
                }
            }
        }

        MyErr = 192;
        //adesso rendo visibili quelle selezionate
        for (x = 1; x <= VisibleRows; x++)
        {
            var currentRow = x; // variabile locale nuova per evitare Captured variable is modified in the outer scope
            row = MainGrid.RowDefinitions[currentRow];
            row.Height = new(38);

            var myGrid = this.FindControl<Grid>("MainGrid");
            if (myGrid != null)
            {
                var controlsInRow = myGrid.Children
                    .Where(c => Grid.GetRow(c) == currentRow);
                foreach (Control control in controlsInRow)
                {
                    control.IsVisible = true;
                }
            }
        }

        //adesso gestiamo le altezze, cominciando dalla riga fissa
        var flag = MainGrid.RowDefinitions[10]; // è la riga della TextBox
        Console.WriteLine("Entro in cmbRow con GridHeight: " + GridHeight);
        // 1. Definisci i limiti della riga
        flag.MinHeight = 120;
        flag.MaxHeight = 420;

        // 2. Imposta la riga come elastica (Star). 
        // NB: '1' qui NON significa 1 pixel, ma "prendi tutto lo spazio rimasto".
        flag.Height = new(1, GridUnitType.Star);

        // 3. Calcola l'altezza totale che la FINESTRA deve avere 
        // per fare in modo che la riga 10 sia esattamente 'GridHeight'
        double partiFisse = 92 + 74 + (38 * int.Parse(CmbRows.Text));

        // 4. Applica le dimensioni alla FINESTRA
        this.MinHeight = partiFisse + flag.MinHeight;
        this.MaxHeight = partiFisse + flag.MaxHeight;

        // QUI IL FIX: Forzi la finestra ad essere grande abbastanza da ospitare 
        // le parti fisse + i pixel che avevi salvato per la riga 10.
        this.Height = partiFisse + GridHeight;
        // 5. Opzionale: Assicurati che il Layout venga aggiornato
        this.UpdateLayout();
        Console.WriteLine("Altezza reale Row10: " + MainGrid.RowDefinitions[10].ActualHeight);
        Console.WriteLine("Row10 height:" + MainGrid.RowDefinitions[10].Height.Value);
        Console.WriteLine("this.height: " + this.Height + " -this.minheight: " + this.MinHeight + " -this.maxheight: " +
                          this.MaxHeight);
    }

    private async Task OpenFolder(int whosend, int index)
    {
        // Apre la finestra di selezione cartelle
        //se voglio la finestra Avalonia (ma occorre doppio click)
        //occorre aggiungere.UseManagedSystemDialogs(); in Program.cs
        MyErr = 194;
        string oldStart = StartPaths[index];
        string oldDest = DestPaths[index];
        try
        {
            //ritorna null se si preme annulla
            var folderPath = await SelezionaUnita();
            Console.WriteLine("----------- " + folderPath);

            if (folderPath == null) return;

            if (folderPath == "")
            {
                await Messaggio(false, 2, "Path not found!");
                return;
            }

            // whosend=1 è Start, =2 è Dest
            if (whosend == 1)
            {
                MyErr = 196;
                if (folderPath == DestPaths[index])
                {
                    lblStartPaths[index].Content = folderPath;
                    await Messaggio(false, 2, Res.msg_SamePaths);
                    lblStartPaths[index].Content = oldStart;
                }
                else
                {
                    if (StartPaths[index] != folderPath && (!BtnStart.IsEnabled))
                        await ActEscape(100);
                    lblStartPaths[index].Content = folderPath;
                    StartPaths[index] = folderPath;
                    lblStartPaths[index].Background = Brushes.WhiteSmoke;
                    ToolTip.SetTip(lblStartPaths[index], StartPaths[index]);
                    AzzeraChecks(index);
                }
            }
            else
            {
                MyErr = 198;
                if (folderPath == StartPaths[index])
                {
                    lblDestPaths[index].Content = folderPath;
                    await Messaggio(false, 2, Res.msg_SamePaths);
                    lblDestPaths[index].Content = oldDest;
                }
                else
                {
                    if (DestPaths[index] != folderPath && (!BtnStart.IsEnabled))
                        await ActEscape(50);
                    lblDestPaths[index].Content = folderPath;
                    DestPaths[index] = folderPath;
                    lblDestPaths[index].Background = Brushes.WhiteSmoke;
                    ToolTip.SetTip(lblDestPaths[index], DestPaths[index]);
                    AzzeraChecks(index);
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("In MainGrid_OnTapped: " + ex.Message);
            await Messaggio(false, 1, $"{MyErr} - {ex.Message}");
        }
    }

    private void AzzeraChecks(int index)
    {
        chkChks[index].IsChecked = false;
        chkTopOnly[index].IsChecked = false;
        TopOnlys[index] = false;
    }

    private async Task CheckFolders()
    {
        MyErr = 200;
        int x;
        for (x = 1; x <= VisibleRows; x++)
        {
            if (StartPaths[x].Length > 0 && !Directory.Exists(StartPaths[x]))
            {
                lblStartPaths[x].Background = Brushes.LightSalmon;
                await Messaggio(false, 2, Res.msg_DontExist);
            }
            // se la Destpath non esiste verrà comunque ricreata automaticamente
        }
    }

    private async void MenuHelp_OnClick(object? sender, RoutedEventArgs e)
    {
        try
        {
            MyErr = 202;            
            Process.Start(new ProcessStartInfo
            {
                FileName = "https://psmate.com/Help/CF2/intro.html",
                UseShellExecute = true // Fondamentale per aprire file con l'app predefinita
            });
        }
        catch (Exception ex)
        {
            Console.WriteLine("In MenuHelp_OnClick: " + ex.Message);
            await Messaggio(false, 1, $"{MyErr} - {ex.Message}");
        }
    }

    private async void MenuInfo_OnClick(object? sender, RoutedEventArgs e)
    {
        try
        {
            MyErr = 205;
            var inf = new Info
            {
                // Assegna il DataContext della finestra corrente alla nuova finestra
                DataContext = this.DataContext
            };
            await inf.ShowDialog(this);
        }
        catch (Exception exception)
        {
            Console.WriteLine(exception);
        }
    }

    private void InputElement_OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        MyErr = 210;
        if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
        {
            if (sender is Border { ContextMenu: not null } border)
            {
                // In Avalonia, il ContextMenu viene gestito dall'elemento
                border.ContextMenu.Open(border);
            }
        }
    }

    private async Task<string?> SelezionaUnita() //Control control)
    {
        MyErr = 215;
        var topLevel = TopLevel.GetTopLevel(this);
        if (topLevel == null) return null;

        // Con UseManagedSystemDialogs abilitato in Program.cs, 
        // questo picker Avalonia NON blocca le radici dei dischi.
        var folders = await topLevel.StorageProvider.OpenFolderPickerAsync(new()
        {
            Title = Res.SelectFolder,
            AllowMultiple = false
        });

        if (folders.Count > 0)
        {
            // Restituiamo il percorso fisico (es. "E:\")
            var uri = folders[0].Path;
            // OriginalString non crasha mai. Poi puliamo i prefissi file:// o slash iniziali
            string rawPath = uri.IsAbsoluteUri ? uri.LocalPath : Uri.UnescapeDataString(uri.OriginalString);

            // Pulizia finale per Windows (toglie / davanti a C:/ e sistema gli slash)
            return rawPath.Replace('/', '\\').TrimStart('\\'); //************** ???
        }

        return null; // L'utente ha annullato la selezione
    }

    private async void ButtonComp_OnClick(object? sender, RoutedEventArgs e)
    {
        //E' il pulsante 'Compatta'
        try
        {
            MyErr = 220;
            Compatta();
            BtnStart.Focus();
        }
        catch (Exception ex)
        {
            Console.WriteLine("MainGrid_OnClick: " + ex.Message);
            await Messaggio(false, 1, $"{MyErr} - {ex.Message}");
        }
    }

    private void Compatta()
    {
        // Nota: in C# gli array sono 0-based. 
        // Se i tuoi array sono dimensionati su VisibleRows + 1, usa x = 1.
        // Qui assumo 0-based che è lo standard C#.
        MyErr = 225;
        int fullCount = 0;
        int flagCount;
        bool daCompattare = false;

        // 1. Reset e conteggio righe piene
        for (int x = 1; x <= VisibleRows; x++)
        {
            chkChks[x].IsChecked = false;

            if (!string.IsNullOrEmpty(lblStartPaths[x].Content!.ToString()) ||
                !string.IsNullOrEmpty(lblDestPaths[x].Content!.ToString()))
            {
                fullCount++;
            }
        }

        int counter = 0;

        // 2. Ciclo di spostamento (Bubble-up)
        do
        {
            for (int x = 1; x <= VisibleRows - 1; x++)
            {
                // Se la riga X è vuota (sia start che dest)
                if (string.IsNullOrEmpty(lblStartPaths[x].Content!.ToString()) &&
                    string.IsNullOrEmpty(lblDestPaths[x].Content!.ToString()))
                {
                    for (int y = x + 1; y <= VisibleRows; y++)
                    {
                        // Se la riga Y (successiva) non è vuota
                        if (!string.IsNullOrEmpty(lblStartPaths[y].Content!.ToString()) ||
                            !string.IsNullOrEmpty(lblDestPaths[y].Content!.ToString()))
                        {
                            daCompattare = true;

                            // SPOSTAMENTO VALORI NEI CONTROLLI
                            txtMasks[x].Text = txtMasks[y].Text;
                            lblStartPaths[x].Content = lblStartPaths[y].Content;
                            lblDestPaths[x].Content = lblDestPaths[y].Content;
                            chkTopOnly[x].IsChecked = chkTopOnly[y].IsChecked;
                            cmbNumBackups[x].SelectedIndex = cmbNumBackups[y].SelectedIndex;

                            // RESET RIGA DI ORIGINE (Y)
                            txtMasks[y].Text = Res.Mask; // Sostituisci con risorsa: Properties.Resources.Mask
                            lblStartPaths[y].Content = "";
                            lblDestPaths[y].Content = "";
                            chkTopOnly[y].IsChecked = false;
                            cmbNumBackups[y].SelectedIndex = 0; // Sostituisci con risorsa: Properties.Resources.Zero

                            break; // Esci dal ciclo Y, passa al prossimo X
                        }
                    }
                }
            }

            // 3. Verifica stato compattazione
            flagCount = 0;
            for (int w = 0; w <= fullCount; w++)
            {
                if (!string.IsNullOrEmpty(lblStartPaths[w].Content!.ToString()) &&
                    !string.IsNullOrEmpty(lblDestPaths[w].Content!.ToString()))
                {
                    flagCount++;
                }
            }

            counter++;

        } while (flagCount != fullCount && counter <= 50);

        // 4. Se è stato spostato qualcosa, aggiorna le matrici dei valori (Array)
        if (daCompattare)
        {
            for (int x = 1; x <= VisibleRows; x++)
            {
                // Aggiornamento matrici (assumendo siano array di string/bool)
                Masks[x] = txtMasks[x].Text !;
                StartPaths[x] = lblStartPaths[x].Content!.ToString() !;
                DestPaths[x] = lblDestPaths[x].Content!.ToString() !;
                NumBackups[x] = cmbNumBackups[x].Text !;
                TopOnlys[x] = chkTopOnly[x].IsChecked ?? false; // IsChecked è bool? (nullable)
            }
        }
    }

    private void DateTimeTo_OnSelectedDateChanged(object? sender, SelectionChangedEventArgs e)
    {
        DateTo = (DateTimeTo.SelectedDate ?? DateTimeOffset.Now).DateTime;
    }

    private void DateTimeFrom_OnSelectedDateChanged(object? sender, SelectionChangedEventArgs e)
    {
        DateFrom = (DateTimeFrom.SelectedDate ?? DateTimeOffset.Now).DateTime;
    }

    private void ChkCheckDates_OnIsCheckedChanged(object? sender, RoutedEventArgs e)
    {
        CheckDates = ChkCheckDates.IsChecked ?? false;
    }

    private async void BtnOpen_OnClick(object? sender, RoutedEventArgs e)
    {
        try
        {
            MyErr = 230;
            if (!File.Exists(LogPath))
                await Messaggio(false, 0, Res.msg_NoLog + crLf);
            else
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = LogPath,
                    UseShellExecute = true // Fondamentale per aprire file con l'app predefinita
                });
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            ReadErrors.Add(ex.Message);
            Append($"{Res.Error} {MyErr} -{ex.Message}", true);
        }
    }

    private void MainGrid_OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        // Risaliamo l'albero visivo partendo da ciò che è stato cliccato (e.Source)
        MyErr = 235;
        var lb = (e.Source as Visual)?.FindAncestorOfType<Label>();

        if (lb != null)
        {
            if (lb.Name?.StartsWith("LblSt") == true)
            {
                MyErr = 240;
                string ind = lb.Name;
                if (ind.Length > 2)
                {
                    ind = ind.TrimEnd('\\');
                    ind = ind[^1].ToString();
                    int index = int.Parse(ind);
                    if (StartPaths[index].Length > 2)
                    {
                        Process.Start(new ProcessStartInfo
                        {
                            FileName = StartPaths[index],
                            UseShellExecute = true // Fondamentale per aprire file con l'app predefinita
                        });
                    }
                }
            }
            else if (lb.Name?.StartsWith("LblDe") == true)
            {
                MyErr = 242;
                string ind = lb.Name;
                if (ind.Length > 2)
                {
                    ind = ind.TrimEnd('\\');
                    ind = ind[^1].ToString();
                    int index = int.Parse(ind);
                    if (DestPaths[index].Length > 2)
                    {
                        Process.Start(new ProcessStartInfo
                        {
                            FileName = DestPaths[index],
                            UseShellExecute = true // Fondamentale per aprire file con l'app predefinita
                        });
                    }
                }
            }
        }
    }

    private async void ChkHidden_OnIsCheckedChanged(object? sender, RoutedEventArgs e)
    {
        try
        {
            if (e.Source is CheckBox { Name: not null } ch)
            {
                MyErr = 250;
                if (ch.Name.StartsWith("ChkH"))
                {
                    AlsoHidden = ch.IsChecked ?? false;
                    if (!AlsoHidden)
                    {
                        ChkSystem.IsChecked = false;
                        ChkSystem.IsEnabled = false;
                        AlsoSystem = false;
                    }
                    else
                    {
                        ChkSystem.IsEnabled = true;
                    }
                }
                else if (ch.Name.StartsWith("ChkS")) AlsoSystem = ch.IsChecked ?? false;
            }

            if (!BtnStart.IsEnabled)
                await ActEscape(400);
            Console.WriteLine("AlsoHidden e': " + AlsoHidden.ToString());
            Console.WriteLine("AlsoSystem e': " + AlsoSystem.ToString());
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            ReadErrors.Add(ex.Message);
            Append($"{Res.Error} {MyErr} -{ex.Message}", true);
        }
    }

    private void TxtBorder_OnSizeChanged(object? sender, SizeChangedEventArgs e)
    {
        if (!stoPartendo)
        {
            GridHeight = e.NewSize.Height;
            Console.Write($"Nuova altezza: {e.NewSize.Height}");
        }
    }
    
    private async Task CheckIfUpdatesAsync()
    {
        try
        {
            // L'attesa (await) avviene "dietro le quinte", la UI resta reattiva
            int versione = await CheckVersion.LeggiVersioneAsync(3000);
        
            Console.WriteLine("Versione update: " + versione);

            if (versione == 1) 
            {
                MyErr = 255; 
                var inf = new MessBox(true, 0, Res.Update)
                {
                    DataContext = this.DataContext
                };

                // Ora ShowDialog non darà errore perché siamo sul thread UI
                var res = await inf.ShowDialog<string>(this);

                if (res == "Yes")
                {
                    string link;
                    if (Lang.Equals("it", StringComparison.CurrentCultureIgnoreCase))
                         link = "https://psmate.com/it/download/copyfolders-2.html";
                    else link = "https://psmate.com/en/downloads/copyfolders-2.html";

                    Process.Start(new ProcessStartInfo
                    {
                        FileName = link,
                        UseShellExecute = true 
                    });
                }
            }
        }
        catch (Exception e)
        {
            // Se usi un thread UI, puoi anche mostrare l'errore a video se serve
            Console.WriteLine($"{MyErr} - {e.Message}");
        }
    }
}  