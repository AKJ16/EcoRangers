using NUnit.Framework;
using System.IO;
using UnityEngine;

public class SaveSystemTest
{
    // Score save test
    [Test]
    public void SaveSystem_EncryptionAndDecryption_ReturnsOriginalValue()
    {
        int originalScore = 1250;
        SaveSystem.SaveHighscore(originalScore);
        int loadedScore = SaveSystem.LoadHighscore();

        Assert.AreEqual(originalScore, loadedScore, "The decrypted score does not match the original!");
    }

    // Missing file test
    [Test]
    public void SaveSystem_FileDoesNotExist_ReturnsZero()
    {
        string path = Application.persistentDataPath + "/savedata.txt";
        if (File.Exists(path))
        {
            File.Delete(path);
        }

        int loadedScore = SaveSystem.LoadHighscore();

        Assert.AreEqual(0, loadedScore, "If no save file exists, the system must return 0 instead of throwing an error.");
    }

    // Encryption test
    [Test]
    public void SaveSystem_SaveFileOnDisk_IsEncryptedAndNotPlaintext()
    {
        int scoreToSave = 9999;
        SaveSystem.SaveHighscore(scoreToSave);

        string path = Application.persistentDataPath + "/savedata.txt";
        string rawTextOnDisk = File.ReadAllText(path);

        Assert.AreNotEqual(scoreToSave.ToString(), rawTextOnDisk, "The raw file on disk is stored in plaintext! Encryption failed.");
    }
}