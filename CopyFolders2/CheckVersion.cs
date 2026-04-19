using System;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;

namespace CopyFolders2;

public static class CheckVersion
{
     public static async Task<int> LeggiVersioneAsync(int delay)
     {
         // As Task o lo Sleep sotto blocca il Thread principale
         var client = new HttpClient();
         try
         {
             // Aspetto un pò
             await Task.Delay(delay);
             var response = await client.GetAsync("https://www.psmate.com/dc/CF2.txt");

             // Se la richiesta ha successo…
             if (response.IsSuccessStatusCode)
             {
                 // Leggo il contenuto come stringa
                 string lastVersion = await response.Content.ReadAsStringAsync();
                 if (!string.IsNullOrEmpty(lastVersion))
                 {

                     lastVersion = lastVersion.Replace(".", "", StringComparison.InvariantCulture);
                     Console.WriteLine("----------LastVersion: " + lastVersion);
                     int lastVersionNum = int.Parse(lastVersion.Trim());               
                  
                     string? version = Assembly.GetEntryAssembly()?.GetName().Version?.ToString().Trim();
                     version = version?.Replace(".", "", StringComparison.InvariantCulture);
                     if (string.IsNullOrEmpty(version)) 
                         return 0;
                     
                     int currentVersionNum = int.Parse(version);

                     if (currentVersionNum < lastVersionNum)
                     {
                         return 1;    //c' è aggiornamento
                     } 
                     return 2; //nessun aggiornamento occorre
                 }
             }
             return 0;     //errore
         }
         catch (Exception)
         {
             return 0;
         }
     }
 } 