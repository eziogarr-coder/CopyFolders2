using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Threading;
using System.Threading.Tasks; 
using static CopyFolders2.GlobalVariables;
using static CopyFolders2.Archivio; 

namespace CopyFolders2
{
    public static class Archivio
    {
        public static readonly List<string>[] AccessibleFiles = new List<string>[10];
        public static readonly List<string>[] FilesToCopy = new List<string>[10];

        // Questo è il costruttore statico. 
        // NON devi chiamarlo tu. Lo fa il PC "dietro le quinte".
        static Archivio()
        {
            for (int i = 0; i < 10; i++)
            {
                AccessibleFiles[i] = new(100000);
                FilesToCopy[i] = new(100000);
            }
        }
    }

    public class CheckFoldersAndFiles
    {
        //public static List<string> AccessibleFiles = new List<string>(200000);
        public static readonly List<string> ReadErrors = new(2000);
        public static readonly List<string> SkippedFies = new(40000);
        public static bool StopCopy;

        private readonly EnumerationOptions options = new()
        {
            AttributesToSkip = FileAttributes.None,
            RecurseSubdirectories = true,
            IgnoreInaccessible = false
        };

        //public static int FileTotali;
        public static int FileCopiati;
        public static long TotalSize;
        public static long TotalBytesCopied;

        public static  DateTime DirDate; // solo per generare eventuale errore
        public static  int RowDirs;
        public static long RowFileSize;

        public async Task ScriviTitolo(int from)
        {
            string str;
            if (from == 1) str = $"{Res.logSearch}: ";
                      else str = $"{Res.logCopy}: ";
            
            if (File.Exists(LogPath)) File.Delete(LogPath);
            await using StreamWriter sw = new(LogPath, append: true);
            await sw.WriteLineAsync(str + DateTime.Now.ToString("dddd, dd MMMM yyyy -HH:mm"));
        }
        public async Task CheckFiles(string path, int row, string mask, IProgress<string> progress)
        {
            await Task.Run(() => {
                
                string termToExclude = "";
                if (mask.StartsWith("!="))
                {
                    termToExclude = mask.Trim().Remove(0, 2).Trim();
                    mask = Res.Mask;
                }

                MyErr = 700;
                var dir = new DirectoryInfo(path);
                options.RecurseSubdirectories = !TopOnlys[row];
                        
                //foreach (var f in dir.EnumerateFileSystemInfos(mask,options))   
                using var enumerator = dir.EnumerateFileSystemInfos(mask, options).GetEnumerator();

                while (true)
                {
                    if (StopCopy) return;
                    
                    try
                    {
                        MyErr = 705;
                        // L'errore di "Accesso Negato" scatta qui dentro durante il MoveNext()
                        if (!enumerator.MoveNext()) break; 
                    }
                    catch (System.Security.SecurityException ex)
                    {
                        Debug.Print("---------- error 21: " + ex.Message);
                        lock (ReadErrors)
                            ReadErrors.Add(ex.Message);
                        continue;
                    }
                    catch (UnauthorizedAccessException ex)
                    {
                        Debug.Print("---------- error 22: " + ex.Message);
                        lock (ReadErrors)
                            ReadErrors.Add(ex.Message);
                        continue;
                    }
                    catch (PathTooLongException ex)
                    {
                        Debug.Print("---------- error 23: " + ex.Message);
                        lock (ReadErrors)
                            ReadErrors.Add(ex.Message);
                        continue;
                    }
                    catch (IOException ex)
                    {
                        Debug.Print("---------- error 24: " + ex.Message);
                        lock (ReadErrors)
                            ReadErrors.Add(ex.Message);
                        continue;
                    }
                    catch (Exception ex)
                    {
                        Debug.Print("---------- error 1: " + ex.Message);
                        lock (ReadErrors)
                            ReadErrors.Add(MyErr + " -" +ex.Message);
                        continue;
                    }

                    if (StopCopy) return;
                    var f = enumerator.Current;
                    try
                    {
                        MyErr = 710;    
                        bool isHidden = (f.Attributes & FileAttributes.Hidden) != 0;
                        bool isSystem = (f.Attributes & FileAttributes.System) != 0;
                        bool contieneStringaProibita = !string.IsNullOrEmpty(termToExclude) && 
                                              f.Name.Contains(termToExclude, StringComparison.OrdinalIgnoreCase);

                        // 3. Logica di esclusione
                        bool daEscludere = (isHidden && !AlsoHidden) || (isSystem && !AlsoSystem) || contieneStringaProibita;
                        if (daEscludere) 
                        {
                            // Scriviamo nel log il nome del file e i suoi attributi reali
                            lock (ReadErrors)
                                SkippedFies.Add($"{f.FullName}  - [Attributes: {f.Attributes.ToString()}]");
                            continue;
                        }

                        MyErr = 715;
                        if (f.Attributes.HasFlag(FileAttributes.Directory))
                        {
                            RowDirs++;
                        }
                        else // è un file
                        {
                            // Controllo Date (DateTimePicker equivalenti in Avalonia)
                            if (CheckDates)
                            {
                                var date = f.LastWriteTime.Date;
                                if (date < DateFrom.Date || date > DateTo.Date)  continue;
                            }
                            
                            RowFileSize += ((FileInfo)f).Length;
                            if (StopCopy) return;
                            // await sw.WriteLineAsync($"Found: {f} ({RowSize:#,##0} KB)");            
                            lock (AccessibleFiles[row])
                            {
                                //RowFiles += 1;
                                AccessibleFiles[row].Add(f.FullName);
                                // se ne stanno leggendo un pò troppi...
                                if (AccessibleFiles[row].Count % 2000 == 0)
                                {
                                    //AppendToLine = true;
                                    progress.Report(".");
                                }
                            }
                        }
                    }
                    catch (System.Security.SecurityException ex)
                    {
                        Debug.Print("---------- error 21: " + ex.Message);
                        lock (ReadErrors)
                            ReadErrors.Add(ex.Message);
                    }
                    catch (UnauthorizedAccessException ex)
                    {
                        Debug.Print("---------- error 22: " + ex.Message);
                        lock (ReadErrors)
                            ReadErrors.Add(ex.Message);
                    }
                    catch (PathTooLongException ex)
                    {
                        Debug.Print("---------- error 23: " + ex.Message);
                        lock (ReadErrors)
                            ReadErrors.Add(ex.Message);
                    }
                    catch (IOException ex)
                    {
                        Debug.Print("---------- error 24: " + ex.Message);
                        lock (ReadErrors)
                            ReadErrors.Add(ex.Message);
                    }
                    catch (Exception ex)
                    {
                        Debug.Print("---------- error 1: " + ex.Message);
                        lock (ReadErrors)
                            ReadErrors.Add(MyErr+ " -" + ex.Message);
                    }
                }
                
                // Opzione: Yield per dare tempo alla UI
                Thread.Sleep(100);
                //fine del  task
            });
        }
        
        public async Task SkipExistentsAsync(int index, IProgress<string> progress)
        {
            //li rendo globali per le altre Sub
            // Aggiornamento UI tramite Progress (equivalente a AppendTextTolblFiles)          
            await using FileStream fs = new(LogPath, FileMode.Append, FileAccess.Write, FileShare.ReadWrite);
            await using var sw = new StreamWriter(fs);
            ScriviIntestazione(sw,index, 1);
            
            progressReportText = progress;
            RowFileSize = 0;

            MyErr = 730;
            string sourceFile;
            string destinationFile;
            string destPath= DestPaths[index];           
            
            try
            {
                await Task.Run(() =>
                {
                    MyErr = 740;
                    var lunghezza =StartPaths[index].TrimEnd(Path.DirectorySeparatorChar).Length;
                    int counter = 0;   
                  
                    foreach (var currentFile in AccessibleFiles[index])
                    {
                        counter++;  
                        try
                        {
                            if (counter % 2000 == 0)
                            {
                                //AppendToLine = true;
                                progress.Report(".");
                            }
                            
                            sourceFile = currentFile;
                            if (StopCopy) break;

                            //Console.WriteLine(sourceFile);
                            //Console.WriteLine(DestPaths[index].TrimEnd(Path.DirectorySeparatorChar));
                            //Console.WriteLine(sourceFile.Substring(Lunghezza).TrimStart(Path.DirectorySeparatorChar));
                            destinationFile = Path.Combine(destPath.TrimEnd(Path.DirectorySeparatorChar),
                                              sourceFile.Substring(lunghezza).TrimStart(Path.DirectorySeparatorChar));
                            
                            var sourceFileInfo = new FileInfo(sourceFile);
                            var destFileInfo = new FileInfo(destinationFile);

                            // Controllo Date (DateTimePicker equivalenti in Avalonia)
                            if (CheckDates)
                            {
                                var date = sourceFileInfo.LastWriteTime.Date;
                                if (date < DateFrom.Date || date > DateTo.Date)
                                {
                                    continue;
                                }
                            }

                            bool daCopiare = false;
                            if (!File.Exists(destinationFile))
                            {
                                daCopiare = true;
                            }
                            else if (sourceFileInfo.LastWriteTime > destFileInfo.LastWriteTime)
                            {
                                daCopiare = true;
                            }

                            if (!daCopiare)
                            {
                                continue;
                            }
                            
                            FilesToCopy[index].Add(sourceFile);
                            RowFileSize += sourceFileInfo.Length;

                            if (!StopCopy) continue;
                            break;
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex);
                            lock (ReadErrors)
                                ReadErrors.Add(MyErr + " -" + ex.Message);
                        }
                    } //fine ciclo for
                });    
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                lock (ReadErrors)
                    ReadErrors.Add($"{Res.logFatalError}: {MyErr}  -{ex.Message}");
            }
        
            if (StopCopy)
            {
                await sw.WriteLineAsync(Res.logStopped);  
            }
            else
            {
                    //le scritte finali nella casella di testo sono in MainWindow
                    // FilesToCopy[index].Sort(StringComparer.OrdinalIgnoreCase); //**************
                    if (FilesToCopy[index].Count > 0)
                    {
                         foreach(var file in FilesToCopy[index])
                            await sw.WriteLineAsync($"{Res.logFound}: {file} ({RowFileSize:#,##0} KB)");
                    } else  await sw.WriteLineAsync($"{Res.logNoNew}. {index}");    
            }
        }

        private IProgress<string>? progressReportText;
        private IProgress<long>? progressBar;
        private Stopwatch? stp;
        
        public async Task CopyFolderContentsAsync(int index, IProgress<string> progressReporter,
                                                            IProgress<long> progress, Stopwatch stopwatch)
        {
            //li rendo globali per le altre Sub
            // Aggiornamento UI tramite Progress (equivalente a AppendTextTolblFiles)          
            progressReportText = progressReporter;  
            progressBar = progress;
            stp = stopwatch;

            MyErr = 760;
            string sourceFile;
            string destinationFile;
            int counterfiles = 0;
            
            await using FileStream fs = new(LogPath, FileMode.Append, FileAccess.Write, FileShare.ReadWrite);
            await using var sw = new StreamWriter(fs);
            //prima si ScriviIntestazione per definireDettPat
            
            ScriviIntestazione(sw,index, 2);
            
            try 
            {
                await Task.Run(async () =>
                {
                    var lunghezza =StartPaths[index].TrimEnd(Path.DirectorySeparatorChar).Length;
     
                    foreach (var currentFile in FilesToCopy[index])
                    {
                        try
                        {
                            sourceFile = currentFile;

                            if (StopCopy) break;

                            //FileTotali++;
                            MyErr = 765;
                            //Console.WriteLine(sourceFile);
                            //Console.WriteLine(DestPaths[index].TrimEnd(Path.DirectorySeparatorChar));
                            //Console.WriteLine(sourceFile.Substring(Lunghezza).TrimStart(Path.DirectorySeparatorChar));
                            destinationFile = Path.Combine(DestPaths[index].TrimEnd(Path.DirectorySeparatorChar),
                                              sourceFile.Substring(lunghezza).TrimStart(Path.DirectorySeparatorChar));
                            
                            var sourceFileInfo = new FileInfo(sourceFile);
                            var destFileInfo = new FileInfo(destinationFile);
                            
                            if (!Directory.Exists(destFileInfo.DirectoryName))
                            {
                                Directory.CreateDirectory(destFileInfo.DirectoryName!);
                            }

                            bool daCopiare = false;
                            bool esistente = false;
                            string testo1 = "";
                            string testo2 = "";

                            if (!File.Exists(destinationFile))
                            {
                                daCopiare = true;
                                esistente = false;
                                testo1 = $"{sourceFileInfo.LastWriteTime:d} - {sourceFileInfo.LastWriteTime:t} ==> ";
                                testo2 = $" ({Res.logNotPresent})";
                            }
                            else if (sourceFileInfo.LastWriteTime > destFileInfo.LastWriteTime)
                            {
                                daCopiare = true;
                                esistente = true;
                                testo1 = $"{sourceFileInfo.LastWriteTime:d} - {sourceFileInfo.LastWriteTime:t} ==> ";
                                testo2 = $" {Res.logExisting}: {destFileInfo.LastWriteTime:d} - {destFileInfo.LastWriteTime:t}";
                            }

                            MyErr = 767;
                            if (StopCopy) break;
                            if (daCopiare)
                            {
                                counterfiles++;
                                FileCopiati++;
                                if (Convert.ToInt32(NumBackups[index]) > 0 && esistente)
                                {
                                    await RinominaAsync(destFileInfo, Convert.ToInt32(NumBackups[index]));
                                }
                               
                                long lunghezzaKb = sourceFileInfo.Length / 1024;
                                if (lunghezzaKb == 0) lunghezzaKb = 1;
                               
                                await sw.WriteLineAsync($"{Res.logCopied}: {sourceFile} ({lunghezzaKb:#,##0} KB) {testo1} {destinationFile}{testo2}");

                                //StoCopiandoUnFile = true;
                                // metodo Steam // Più lento di FileCopyEx
                                //await Task.Run(() => CopyFileWithCancelAsync(sourceFile, destinationFile, progressReporter, progress));
                                //StoCopiandoUnFile = false;                    

                                //Copia asincrona con CopyFileEX (consente il report)
                                await Task.Run(() => StartCopyWithCallback(sourceFile , destinationFile)); //i progress sono in variabili a parte
                            }

                            if (!StopCopy) continue;
                            break;
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex);
                            lock (ReadErrors)
                                ReadErrors.Add(ex.Message);
                        }
                    } //fine ciclo for
                    
                    if (counterfiles == 0)
                    {
                        await sw.WriteLineAsync($"{Res.logNoNew}. {index}");    
                    }
                    await sw.WriteLineAsync(Environment.NewLine+ Lbl.Trattini);
                    lock (ReadErrors)
                    {
                        if (ReadErrors.Count > 0)
                        {
                            sw.WriteLineAsync("Copy errors:");
                            foreach (var err in ReadErrors)
                            {
                                sw.WriteLineAsync(err);
                            }
                        }
                        else sw.WriteLineAsync($"{Res.logRowN}. {index} - {Res.logNoCopyErr} ");
                    }
                });        
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                lock (ReadErrors)
                    ReadErrors.Add($"{Res.logFatalError}: {MyErr}  -{ex.Message} ");
            }
            finally
            {
                if (StopCopy) await sw.WriteLineAsync(Res.logStopped);
            }
         
            //le scritte finali nella casella di testo sono in MainWindow
        }
        
        //-----------------------------------------FileCopyEx
        private void StartCopyWithCallback(string source, string destination)
        {
            NativeMethods.CopyFileEx(
                source,
                destination,
                 ProgressHandler,
                IntPtr.Zero,
                ref StopCopy, NativeMethods.CopyFileFlags.COPY_FILE_RESTARTABLE);
        }
        
        private NativeMethods.CopyProgressResult ProgressHandler(
            long totalFileSize,
            long totalBytesTransferred,
            long streamSize,
            long streamBytesTransferred,
            uint dwStreamNumber,
            NativeMethods.CopyProgressCallbackReason dwCallbackReason,
            IntPtr hSourceFile,
            IntPtr hDestinationFile,
            IntPtr lpData)
        {
           
            //Console.WriteLine("StopWatch stp: " + stp.ElapsedMilliseconds);
            //aggiunge puntini di attesa al testo
            //non posso usare i bytes perché non avanza di 1 in 1
            if ((stp!.ElapsedMilliseconds) % 3000 < 5 )
            {
                //await Task.Delay(1);
                //AppendToLine = true;
                progressReportText!.Report(".");
            }
            //avanza la progressBar            
            progressBar?.Report(TotalBytesCopied + totalBytesTransferred);

            if (totalBytesTransferred == totalFileSize)
            {
                TotalBytesCopied += totalBytesTransferred;
            }
            // se richiesto annulla
            return StopCopy
                ? NativeMethods.CopyProgressResult.PROGRESS_CANCEL
                : NativeMethods.CopyProgressResult.PROGRESS_CONTINUE;
        }
        
        //------------------------------------------------------ STEAM
        // Metodo di copia con flag bool STREAM
        /*private async Task CopyFileWithCancelAsync(string source, string dest, IProgress<string> progressReporter,
                                                                                             IProgress<long> progress)
        {
            const int bufferSize = 64 * 1024;; // dimensione del blocco
            byte[] buffer = new byte[bufferSize];

            try
            {
                using (var sourceStream = File.OpenRead(source))
                using (var destStream = File.Create(dest))
                {
                    int bytesRead;
                    while ((bytesRead = await sourceStream.ReadAsync(buffer, 0, buffer.Length)) > 0)
                    {
                        // se è stata richiesta cancellazione
                        if (StopCopy)
                        {
                            Console.WriteLine("Copia annullata!");
                            break; // esce dal loop, gli stream saranno chiusi dal using
                        }

                        await destStream.WriteAsync(buffer, 0, bytesRead);

                        TotalBytesCopied += bytesRead;
                        
                        if (TotalBytesCopied % 1000000 == 0)
                        {
                            //await Task.Delay(1);
                            AppendToLine = true;
                            progressReporter.Report(".");
                        }
                        
                        progress?.Report(TotalBytesCopied);
                        // aggiorna qui la progress bar dal UI thread
                        // es: Dispatcher.UIThread.Post(() => Progress = totalBytesCopied);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Errore durante la copia: {ex.Message}");
                lock (ReadErrors)
                    ReadErrors.Add("StreamCopyError: " + ex.Message);
            }
            finally
            {
                // se abbiamo richiesto cancellazione e il file esiste, eliminiamolo
                if (StopCopy && File.Exists(dest))
                {
                    File.Delete(dest); // elimina il file parziale :contentReference[oaicite:1]{index=1}
                }
            }
        }*/

        private void ScriviIntestazione(StreamWriter sw, int index, int sender)
        {
            sw.WriteLine(Lbl.Trattini + Environment.NewLine);
            sw.WriteLine($"{Res.logRowN}. {index}:  {Res.logStarting} {StartPaths[index]}  -{Res.logDestination} {DestPaths[index]}");
            sw.WriteLine($"{Res.logCopyHidden}: {AlsoHidden}  -{Res.logCopySystem}: {AlsoSystem}" +
                         $" -{Res.logCopyBetween} {DateFrom.ToShortDateString()} {Lbl.AndThe} {DateTo.ToShortDateString()} {Res.logChecked}: {CheckDates}");
            sw.WriteLine($"{Res.logMaskIs}: ' {Masks[index]} '  -{Res.logBoChecked}: {TopOnlys[index]} {Environment.NewLine}");
            if (sender==1) sw.WriteLine($"{Res.logMatchList}   ({Res.logListIsSorted})");
                      else sw.WriteLine($"{Res.logFileToCopy}: {FilesToCopy[index].Count}   ({Res.logListIsSorted})");
            sw.WriteLine(Lbl.Trattini);
        }
        
        // Nota: Restituisce Task invece di void per permettere l'attesa (await)
        private static async Task RinominaAsync(FileInfo myFile, int bkup)
        {
            // Sposta l'intera esecuzione su un thread separato
            await Task.Run(() =>
            {
                string dir = myFile.DirectoryName!;
                string baseName = Path.GetFileNameWithoutExtension(myFile.FullName);
                string extension = Path.GetExtension(myFile.FullName);
                var culture = CultureInfo.InvariantCulture;

                MyErr = 780;
                // 1. Rimuove il backup più vecchio
                string fileName = Path.Combine(dir, $"{baseName}[BKUP{bkup.ToString(culture)}]{extension}");
                if (File.Exists(fileName))
                {
                    File.Delete(fileName);
                }

                // 2. Scala i backup esistenti (operazioni sincrone all'interno del thread di background)
                for (int x = bkup - 1; x >= 1; x--)
                {
                    string currentPath = Path.Combine(dir, $"{baseName}[BKUP{x.ToString(culture)}]{extension}");
                    if (File.Exists(currentPath))
                    {
                        string nextPath = Path.Combine(dir, $"{baseName}[BKUP{(x + 1).ToString(culture)}]{extension}");
                        File.Copy(currentPath, nextPath, true);
                    }
                }
                // 3. Copia il file attuale come BKUP1
                string firstBkup = Path.Combine(dir, $"{baseName}[BKUP1]{extension}");
                File.Copy(myFile.FullName, firstBkup, true);
            });
        }
    }
}