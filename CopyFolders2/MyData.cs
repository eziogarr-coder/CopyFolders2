using System;
using System.Globalization;
using System.IO;
using System.Xml.Linq;
using Avalonia;
using Avalonia.Platform;
using static CopyFolders2.GlobalVariables;

namespace CopyFolders2;
public class MyData

{
    private readonly string configPath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                                          "ezio2000", "CopyFolders 2", "CF2.Config");

    public string SaveSettings()
    {
        MyErr = 300;
        int x = 22;
        try
        {
            XDocument doc = XDocument.Load(configPath);

            // Modifica di un elemento esistente
            // doc.Root.Element(Element).Value = Value;
            // Lettura di un valore
            MyErr = 301;
            var valToSave = (VisibleRows is 0) ? "4" : VisibleRows.ToString(CultureInfo.InvariantCulture);
            doc.Root!.Element("VisibleRows")!.Value = valToSave;
            MyErr = 302;
            valToSave= (GridHeight is <120 or >420) ? "250" : GridHeight.ToString(CultureInfo.InvariantCulture);
            doc.Root!.Element("GridHeight")!.Value = valToSave;
            MyErr = 303;
            doc.Root!.Element("Lang")!.Value = Lang;
            MyErr = 304;
            doc.Root!.Element("LessDays")!.Value = LessDays.ToString(CultureInfo.InvariantCulture);
            MyErr = 305;
            doc.Root!.Element("CheckDates")!.Value = CheckDates.ToString();
            MyErr = 306;
            doc.Root!.Element("CheckUpdates")!.Value = CheckUpdates.ToString();
            MyErr = 307;
            doc.Root!.Element("AlsoHidden")!.Value = AlsoHidden.ToString();
            MyErr = 308;
            doc.Root!.Element("AlsoSystem")!.Value = AlsoSystem.ToString();
            MyErr = 309;
            doc.Root!.Element("Startup")!.Value = 
                      string.Format(CultureInfo.InvariantCulture, "{0}, {1}", Startup.X, Startup.Y);

            for (x = 1; x <= 9; x++)
            {
                MyErr = 310;
                valToSave= string.IsNullOrEmpty(StartPaths[x]) ? "" : StartPaths[x];
                doc.Root!.Element("StartPaths" + x)!.Value = valToSave;
                MyErr = 311;
                valToSave= string.IsNullOrEmpty(DestPaths[x]) ? "" : DestPaths[x];
                doc.Root.Element("DestPaths" + x)!.Value = valToSave;
                MyErr = 312;
                valToSave= Masks[x] is "" ? "*" : Masks[x];
                doc.Root.Element("Masks" + x)!.Value = valToSave;
                MyErr = 313;
                if (DestPaths[x] == "") 
                {
                    TopOnlys[x] = false;
                } else 
                {
                    // Rimuovi il ?? "False", non serve sui booleani
                    doc.Root!.Element("TopOnlys" + x)!.Value = TopOnlys[x].ToString();
                }
                MyErr = 315;
                doc.Root!.Element("NumBackups" + x)!.Value = NumBackups[x];
            }

            doc.Save(configPath);
            return "";
        }
        catch (Exception ex)
        {
            return $"{MyErr}/{x.ToString(CultureInfo.InvariantCulture)} - {ex.Message}";
        }
    }

    public string LoadSettings()
    {
        // int MyErr = 320;
        MyErr = 320;
        int x = 22;
       
        Console.WriteLine("******************* " + configPath);
        try
        {
            //throw new Exception("Errore load");  
       
            XDocument doc = XDocument.Load(configPath);

            // Lettura di un  valore
            // per ogni elemento aggiunto occorre modificare il file obsoleto in Roaming
            MyErr = 321;
            VisibleRows = int.Parse(doc.Root!.Element("VisibleRows")!.Value) ;
            if (VisibleRows is 0 or > 9) VisibleRows = 4;
            Console.WriteLine("VisibleRows è rilevato come: " + VisibleRows);
            MyErr = 322;
            GridHeight = double.Parse((doc.Root!.Element("GridHeight")!.Value));
            if (GridHeight is <120 or > 420) GridHeight = 250;
            Console.WriteLine("+++++++++++ GridHeight è rilevato come: " + GridHeight);
            MyErr = 323;
            Lang = doc.Root!.Element("Lang")!.Value;
            MyErr = 324;
            LessDays = int.Parse(doc.Root!.Element("LessDays")!.Value);
            MyErr = 325;
            CheckDates = bool.Parse(doc.Root!.Element("CheckDates")!.Value);
            MyErr = 326;
            CheckUpdates = bool.Parse(doc.Root!.Element("CheckUpdates")!.Value); //se si vuole essere avvisati automaticamente
            MyErr = 327;
            AlsoHidden = bool.Parse(doc.Root!.Element("AlsoHidden")!.Value);
            MyErr = 328;
            AlsoSystem = bool.Parse(doc.Root!.Element("AlsoSystem")!.Value);
            MyErr = 329;
            var startupElement = doc.Root?.Element("Startup");
            // 2. Controllo se l'elemento esiste e se ha un testo dentro
            if (startupElement != null && !string.IsNullOrWhiteSpace(startupElement.Value))
            {
                try 
                {
                    // Se il testo c'è, provo a leggerlo
                    Startup = PixelPoint.Parse(startupElement.Value);
                }
                catch
                {
                    // Se il formato è scritto male (es. "326-146"), resettiamo
                    Startup = new(-1, -1); 
                }
            }
            else
            {
                // Il tag non esiste o è vuoto <Startup></Startup>
                Startup = new(-1, -1); 
            }
            
            for (x = 1; x <= 9; x++)
            {
                MyErr = 340;
                StartPaths[x] = doc.Root!.Element("StartPaths" + x)!.Value;
                MyErr = 341;
                DestPaths[x] = doc.Root!.Element("DestPaths" + x)!.Value;
                MyErr = 341;
                Masks[x] = doc.Root!.Element("Masks" + x)!.Value;
                MyErr = 343;
                TopOnlys[x] = bool.Parse(doc.Root!.Element("TopOnlys" + x)!.Value);
                if (string.IsNullOrEmpty(DestPaths[x])) TopOnlys[x] = false;
                MyErr = 345;
                NumBackups[x] = doc.Root!.Element( "NumBackups" + x)!.Value;
                Console.WriteLine(NumBackups[x]);
                //NumBackups[x] = int.Parse(doc.Root.Element("NumBackups" + x.ToString()).Value); ****** da rivedere?
            }

            Console.WriteLine("----------- Path1: " + StartPaths[1]);
            return "";
        }
        catch (Exception ex)
        {
            File.Delete(configPath);
            return $"{MyErr}/{x.ToString(CultureInfo.InvariantCulture)} - {ex.Message}" +
                                                      $"{Environment.NewLine}{Res.msg_Erased}";
        }
    }

    public string ScriviSettingsFile()
    {
       MyErr = 360;
       try
       {
           //throw new Exception("peppereppeeee");
            if (File.Exists(configPath))
            {
                Console.WriteLine("Ok, il file di configurazione esiste!");
                return "";
            }

            MyErr = 361;
            if (!Directory.Exists(Path.GetDirectoryName(configPath)))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(configPath)!);
            }

            MyErr = 362;
            var uri = new Uri($"avares://CopyFolders2/Assets/CF2.config");
            MyErr = 363;
            // 1. Apri lo stream dall'asset interno
            using var assetStream = AssetLoader.Open(uri);
            MyErr = 364;
            // 2. Crea il file fisico di destinazione sul PC
            using var fileStream = File.Create(configPath);
            MyErr = 365;
            // 3. Copia i dati
            assetStream.CopyTo(fileStream);
            return "";
       }
       catch (Exception ex)
       {
           return(MyErr +"-" + ex.Message);
       }
    }
}

