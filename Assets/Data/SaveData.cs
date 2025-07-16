using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO; // Necesario para trabajar con archivos

public static class SaveData 
{
    public static void SaveScoreToCSV(int totalcoins, int scene,int enemyes, string died)
    {
        //string filePath = Application.persistentDataPath + "/puntuaciones.csv";
        string filePath = Application.dataPath + "/Data/puntuaciones.csv";
        // Si el archivo no existe, agrega el encabezado
        if (!File.Exists(filePath))
        {
            using (StreamWriter writer = new StreamWriter(filePath, false))
            {
                writer.WriteLine("Level,Coins,Enemyes Defeated,Died,DateTime");
            }
        }

        // Agrega el puntaje actual y la fecha/hora al archivo
        using (StreamWriter writer = new StreamWriter(filePath, true))
        {
            string line = $"{scene},{totalcoins},{enemyes},{died},{System.DateTime.Now}";
            
            writer.WriteLine(line);
        }
    }

    public static void SaveScoreToCSVLevel3(float scorePlayer, float scoreEnemy, string winner)
    {
        //string filePath = Application.persistentDataPath + "/puntuaciones.csv";
        string filePath = Application.dataPath + "/Data/puntuacionesLevel3.csv";
        // Si el archivo no existe, agrega el encabezado
        if (!File.Exists(filePath))
        {
            using (StreamWriter writer = new StreamWriter(filePath, false))
            {
                writer.WriteLine("Player Score,Enemy Score,Winner");
            }
        }

        // Agrega el puntaje actual y la fecha/hora al archivo
        using (StreamWriter writer = new StreamWriter(filePath, true))
        {
            string line = $"{(int)scoreEnemy},{(int)scorePlayer},{winner}";
            
            writer.WriteLine(line);
        }
    }
}
