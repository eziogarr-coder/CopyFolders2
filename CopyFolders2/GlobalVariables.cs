
using System;
using System.IO;
using Avalonia;

namespace CopyFolders2;

public static class GlobalVariables
{
    public static int MyErr;
    public static readonly string[] DestPaths = new string[10];
    public static readonly string[] Masks = new string[10];
    public static readonly string[] StartPaths = new string[10];
    public static readonly bool[] TopOnlys = new bool[10];
    public static readonly string[] NumBackups = new string[10];  
    public static PixelPoint Startup { get; set; }
    public static string Lang { get; set; } = string.Empty;
    public static int LessDays  { get; set; }
    public static int VisibleRows { get; set; }
    public static DateTime DateFrom { get; set; }
    public static DateTime DateTo { get; set; }
    public static bool CheckDates { get; set; } 
    public static bool CheckUpdates { get; set; }
    public static bool AlsoHidden { get; set; }  
    public static bool AlsoSystem { get; set; }
    //particolare... messa direttamente nell' .axaml
    public static double GridHeight {get;set;}

    //public static bool AppendToLine; // serve per i puntini di seguito sulla stessa riga
    
    public static readonly string LogPath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "ezio2000", "CopyFolders 2", "CopyFolders2.log");
    
}