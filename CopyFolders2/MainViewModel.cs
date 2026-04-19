
using System;
using System.ComponentModel;
using System.Globalization;
using System.Threading;

namespace CopyFolders2;

public class MainViewModel : INotifyPropertyChanged
{ 
    public event PropertyChangedEventHandler? PropertyChanged;

   // Proprietà che ritorna stringhe localizzate
   public String AlsoCopy => Lbl.AlsoCopy;    
   public string AlsoHidden => Lbl.AlsoHidden;  
   public string AlsoSystem => Lbl.AlsoSystem;  
   public string Dates => Lbl.Dates;
   public string Help => Lbl.Help;  
   public string Options => Lbl.Options;
   public string Open => Lbl.OpenButton;
   public string Close => Lbl.CloseButton;
   public string AndThe => Lbl.AndThe;
   public string List => Lbl.List;  
   public string Copy => Lbl.CopyButton;
   public string CopyFrom => Lbl.CopyFrom;
   public string CopyTo => Lbl.CopyTo;
   public string TipMask => Lbl.TipMask;
   public string TipBasename => Lbl.TipBasename;
   public string TipAutoBase => Lbl.TipAutoBase;
   public string TipBkps => Lbl.TipBkps;    
   public string TipComp => Lbl.TipComp;
   public string InfoLangs => Lbl.InfoLangs;
   public static string En => Lbl.en;
   public static string It => Lbl.it;
   public static string Fr => Lbl.fr;
   public static string Es => Lbl.es;
   public static string Pt => Lbl.pt;
   public static string De => Lbl.de;
   public static string Pl => Lbl.pl;
   public static string Ru => Lbl.ru;
   public static string Zh => Lbl.zh;
   public string ChkUpd => Lbl.CheckUpdates;
   public string Rows => Lbl.Rows;
   public string CheckNow => Lbl.CheckNow;  
   public string UpToDate => Lbl.UpToDate;  
   public string Update => Res.Update;
   public string UpdateError => Lbl.UpdateError;
   public string Yes => Lbl.Yes;
   public string Error => Res.Error;
   public string CriticalError => Res.CriticalError;    
   public string Unhandled => Res.Unhandled; 
   public string WillClosed => Res.WillClosed;  
   
    public string Version => Lbl.Version;
   
   public DateTime ? DateFrom
   {
       get => GlobalVariables.DateFrom;
       set
       {
           GlobalVariables.DateFrom = value ?? DateTime.Today;
           OnPropertyChanged(nameof(DateFrom));
       }
   } 
   
   public DateTime ? DateTo
   {
       get => GlobalVariables.DateTo;
       set
       {
           GlobalVariables.DateTo = value ?? DateTime.Today;
           OnPropertyChanged(nameof(DateTo));
       }
   } 
   
    // Metodo per cambiare lingua on fly
    public void SetLanguage(string cultureCode)
    {
        // Cambia cultura corrente
        CultureInfo culture = new(cultureCode);
        
        CultureInfo.DefaultThreadCurrentCulture = culture;
        CultureInfo.DefaultThreadCurrentUICulture = culture;
        Thread.CurrentThread.CurrentCulture = culture;
        Thread.CurrentThread.CurrentUICulture = culture;

        // forza refresh del CalendarDatePicker
        var tmp = DateFrom;
        DateFrom = null;
        DateFrom = tmp;
        
        tmp = DateTo;
        DateTo = null;
        DateTo = tmp;

        // In .NET / Avalonia puoi anche settare:
        Res.Culture = culture;
        Lbl.Culture= culture;

        // Notifica che tutte le proprietà sono cambiate
        OnPropertyChanged(nameof(AlsoCopy));
        OnPropertyChanged(nameof(AlsoHidden));
        OnPropertyChanged(nameof(AlsoSystem));
        OnPropertyChanged(nameof(Dates));
        OnPropertyChanged(nameof(Help));
        OnPropertyChanged(nameof(Options));
        OnPropertyChanged(nameof(Open));
        OnPropertyChanged(nameof(Close));
        OnPropertyChanged(nameof(AndThe));
        OnPropertyChanged(nameof(List));
        OnPropertyChanged(nameof(Copy));
        OnPropertyChanged(nameof(CopyFrom));
        OnPropertyChanged(nameof(CopyTo));
        OnPropertyChanged(nameof(TipMask));
        OnPropertyChanged(nameof(TipBasename));
        OnPropertyChanged(nameof(TipAutoBase));
        OnPropertyChanged(nameof(TipBkps));
        OnPropertyChanged(nameof(TipComp));
        OnPropertyChanged(nameof(InfoLangs));
        OnPropertyChanged(nameof(En));
        OnPropertyChanged(nameof(Fr));
        OnPropertyChanged(nameof(It));
        OnPropertyChanged(nameof(Es));
        OnPropertyChanged(nameof(De));
        OnPropertyChanged(nameof(Pl));
        OnPropertyChanged(nameof(Ru));
        OnPropertyChanged(nameof(Zh));
        OnPropertyChanged(nameof(ChkUpd));
        OnPropertyChanged(nameof(Rows));
        OnPropertyChanged(nameof(CheckNow));
        OnPropertyChanged(nameof(UpToDate));
        OnPropertyChanged(nameof(Update));
        OnPropertyChanged(nameof(UpdateError));
        OnPropertyChanged(nameof(Yes));
        OnPropertyChanged(nameof(Error));
        OnPropertyChanged(nameof(CriticalError));
        OnPropertyChanged(nameof(Unhandled));
        OnPropertyChanged(nameof(WillClosed));
    }
    // era protected //**********
    private void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new(propertyName));
    }
}
